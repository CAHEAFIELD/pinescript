// The MIT License (MIT)
// Copyright © 2024 CAHEAFIELD
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// ════════════════════════════════════════════════════════════════════════════
//  EA.HF Scalper v1.4  —  cTrader Automate Indicator  (net6.0 / cAlgo 1.014)
// ════════════════════════════════════════════════════════════════════════════
//
//  PINE SCRIPT → cTRADER MAPPING
//  ─────────────────────────────
//  Component     Pine built-in                     cTrader equivalent
//  ──────────────────────────────────────────────────────────────────────────
//  RSI           ta.rsi(close, len)                Indicators.RelativeStrengthIndex(...)
//  StochRSI      ta.stoch(rsi_src,...) + ta.sma    Manual: Stochastic formula on RSI values
//                                                  (cTrader StochasticOscillator targets price)
//  MACD          ta.macd(close, f, s, g)           Indicators.MacdCrossOver(fast, slow, sig)
//  EMA           ta.ema(close, len)                Indicators.ExponentialMovingAverage(...)
//  MFI           ta.mfi(hlc3, volume, len)         Manual: Typical-price money-flow over
//                                                  Bars.TickVolumes (see TODO below)
//  ATR           ta.atr(len)                       Indicators.AverageTrueRange(len, Wilder)
//  volume        volume (broker feed)              Bars.TickVolumes (tick-count proxy)
//  syminfo.mintick                                 Symbol.TickSize
//  syminfo.type == "forex"                         Symbol.TickSize vs Symbol.PipSize heuristic
//
//  Multi-timeframe (M1/M2/M5/M15 COMBINED row)
//  ─────────────────────────────────────────────
//  TODO: Pine's request.security() MTF calls have no direct equivalent in a
//        cTrader Indicator.  To replicate the four-TF average, either:
//          a) Instantiate this indicator on four separate chart tabs and
//             aggregate externally, OR
//          b) Move the logic to a cBot that opens multiple MarketData subscriptions
//             and calls GetIndicator() per timeframe.
//        The sc_comb output below is a SINGLE-TIMEFRAME composite score, not
//        the four-TF average.  It is labelled "Combined Score" and is advisory only.
//
//  KNOWN GAPS
//  ──────────
//  • Volume:    Bars.TickVolumes is a tick-count proxy; real traded volume is
//               unavailable in most retail cTrader feeds.  MFI may differ from
//               Pine on tick-based brokers.
//  • Overlay:   ATR-based TP1/TP2/TP3/SL levels are computed and printed to the
//               telemetry log but NOT drawn on the price chart (this indicator
//               runs as a non-overlay oscillator).  To draw price lines, split
//               into a second IsOverlay=true indicator or a cBot.
//  • Alerts:    Pine's alertcondition() has no direct equivalent; wire
//               ExecuteMarketOrder or Chart.DrawStaticText in a cBot instead.
//  • Repainting: Calculate() is called for each closed bar; the last bar (IsLastBar)
//               may repaint during live trading until the bar closes.
//  • Fib window / candle gradient / dashboard table: not ported (UI-only features
//               not available in the cTrader Indicator API).
// ════════════════════════════════════════════════════════════════════════════

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace CAHEAFIELD.HF
{
    [Indicator(
        Name         = "EA.HF Scalper v1.4",
        Description  = "Advisory oscillator — cTrader port of EA.HF Scalper v1.4 Pine Script composite score.",
        IsOverlay    = false,
        AutoRescale  = false,
        AccessRights = AccessRights.None)]
    public class HFScalperV14 : Indicator
    {
        // ── RSI (1) ──────────────────────────────────────────────────────────
        [Parameter("Length", DefaultValue = 14, MinValue = 2, MaxValue = 500, Group = "RSI (1)")]
        public int RsiLength { get; set; }

        // ── Stochastic RSI (2) ───────────────────────────────────────────────
        [Parameter("RSI Period",   DefaultValue = 14, MinValue = 2, MaxValue = 500, Group = "Stochastic RSI (2)")]
        public int StochRsiRsiLen { get; set; }

        [Parameter("Stoch Period", DefaultValue = 14, MinValue = 2, MaxValue = 500, Group = "Stochastic RSI (2)")]
        public int StochRsiStochLen { get; set; }

        [Parameter("%K Smooth",    DefaultValue = 3,  MinValue = 1, MaxValue = 100, Group = "Stochastic RSI (2)")]
        public int StochRsiKPeriod { get; set; }

        [Parameter("%D Smooth",    DefaultValue = 3,  MinValue = 1, MaxValue = 100, Group = "Stochastic RSI (2)")]
        public int StochRsiDPeriod { get; set; }

        // ── MACD (3) ─────────────────────────────────────────────────────────
        [Parameter("Fast",   DefaultValue = 12, MinValue = 2, MaxValue = 500, Group = "MACD (3)")]
        public int MacdFast { get; set; }

        [Parameter("Slow",   DefaultValue = 26, MinValue = 2, MaxValue = 500, Group = "MACD (3)")]
        public int MacdSlow { get; set; }

        [Parameter("Signal", DefaultValue = 9,  MinValue = 2, MaxValue = 500, Group = "MACD (3)")]
        public int MacdSignal { get; set; }

        // ── EMA Cross (4) ────────────────────────────────────────────────────
        [Parameter("Fast EMA", DefaultValue = 9,  MinValue = 2, MaxValue = 500, Group = "EMA Cross (4)")]
        public int EmaFastLen { get; set; }

        [Parameter("Slow EMA", DefaultValue = 21, MinValue = 2, MaxValue = 500, Group = "EMA Cross (4)")]
        public int EmaSlowLen { get; set; }

        // ── MFI (5) ──────────────────────────────────────────────────────────
        [Parameter("Length", DefaultValue = 14, MinValue = 2, MaxValue = 500, Group = "MFI (5)")]
        public int MfiLength { get; set; }

        // ── TP / SL ──────────────────────────────────────────────────────────
        [Parameter("ATR Length",   DefaultValue = 14,  MinValue = 1, MaxValue = 500, Group = "TP / SL")]
        public int AtrLength { get; set; }

        [Parameter("TP1 × ATR",   DefaultValue = 1.5, MinValue = 0.1, Group = "TP / SL")]
        public double Tp1Mult { get; set; }

        [Parameter("TP2 × ATR",   DefaultValue = 2.5, MinValue = 0.1, Group = "TP / SL")]
        public double Tp2Mult { get; set; }

        [Parameter("TP3 × ATR",   DefaultValue = 4.0, MinValue = 0.1, Group = "TP / SL")]
        public double Tp3Mult { get; set; }

        [Parameter("SL × ATR",    DefaultValue = 1.0, MinValue = 0.1, Group = "TP / SL")]
        public double SlMult { get; set; }

        // ── Display ──────────────────────────────────────────────────────────
        /// <summary>Minimum confidence % required before a trade advisory is emitted.</summary>
        [Parameter("Signal Threshold (%)", DefaultValue = 55.0, MinValue = 0.0, MaxValue = 100.0, Group = "Display")]
        public double SignalThresholdPct { get; set; }

        // ── Pip Size Mode ────────────────────────────────────────────────────
        // Pine: "Auto" → forex = mintick×10, others = mintick; "Override" = user value.
        // cTrader: Symbol.PipSize already holds the standard pip for forex pairs
        //          (0.0001 for 5-digit pairs, 0.01 for JPY pairs, etc.).
        //          For non-forex instruments, Symbol.PipSize may equal Symbol.TickSize.
        //
        // The PipSizeOverride is only used when UseCustomPipSize = true.
        [Parameter("Use Custom Pip Size", DefaultValue = false, Group = "Pip Size")]
        public bool UseCustomPipSize { get; set; }

        [Parameter("Pip Size Override", DefaultValue = 0.0001, MinValue = 1e-7, Group = "Pip Size")]
        public double PipSizeOverride { get; set; }

        // ── Telemetry ────────────────────────────────────────────────────────
        [Parameter("Enable Telemetry (Log)", DefaultValue = false, Group = "Telemetry")]
        public bool EnableTelemetry { get; set; }

        // ── Outputs ──────────────────────────────────────────────────────────

        /// <summary>Composite score normalised to [-1, +1].  Positive = bullish, negative = bearish.</summary>
        [Output("Combined Score", LineColor = "DodgerBlue", PlotType = PlotType.Line, Thickness = 2)]
        public IndicatorDataSeries CombinedScore { get; set; }

        /// <summary>Absolute confidence in [0, 1].  Multiply by 100 for percentage.</summary>
        [Output("Confidence (0–1)", LineColor = "Orange", PlotType = PlotType.Line, Thickness = 1)]
        public IndicatorDataSeries Confidence { get; set; }

        /// <summary>Positive signal-threshold level (constant horizontal line).</summary>
        [Output("Threshold +", LineColor = "Green", PlotType = PlotType.Line, LineStyle = LineStyle.Lines, Thickness = 1)]
        public IndicatorDataSeries ThresholdPos { get; set; }

        /// <summary>Negative signal-threshold level (constant horizontal line).</summary>
        [Output("Threshold –", LineColor = "Red", PlotType = PlotType.Line, LineStyle = LineStyle.Lines, Thickness = 1)]
        public IndicatorDataSeries ThresholdNeg { get; set; }

        // Per-component scores — dotted lines for diagnostic use.
        /// <summary>RSI component score in [-1, +1].</summary>
        [Output("Score · RSI",      LineColor = "MediumPurple", PlotType = PlotType.Line, LineStyle = LineStyle.DotsRare, Thickness = 1)]
        public IndicatorDataSeries ScoreRsi { get; set; }

        /// <summary>StochRSI component score in [-1, +1].</summary>
        [Output("Score · StochRSI", LineColor = "HotPink",      PlotType = PlotType.Line, LineStyle = LineStyle.DotsRare, Thickness = 1)]
        public IndicatorDataSeries ScoreStochRsi { get; set; }

        /// <summary>MACD component score in {-1, -0.3, 0, +0.3, +1}.</summary>
        [Output("Score · MACD",     LineColor = "Yellow",        PlotType = PlotType.Line, LineStyle = LineStyle.DotsRare, Thickness = 1)]
        public IndicatorDataSeries ScoreMacd { get; set; }

        /// <summary>EMA-cross component score in [-1, +1].</summary>
        [Output("Score · EMA",      LineColor = "Cyan",          PlotType = PlotType.Line, LineStyle = LineStyle.DotsRare, Thickness = 1)]
        public IndicatorDataSeries ScoreEma { get; set; }

        /// <summary>MFI component score in [-1, +1].</summary>
        [Output("Score · MFI",      LineColor = "Coral",         PlotType = PlotType.Line, LineStyle = LineStyle.DotsRare, Thickness = 1)]
        public IndicatorDataSeries ScoreMfi { get; set; }

        // ── Private indicator references ─────────────────────────────────────
        private RelativeStrengthIndex    _rsi;
        private RelativeStrengthIndex    _stochRsiBase;  // RSI fed into the Stochastic formula
        private MacdCrossOver            _macd;
        private ExponentialMovingAverage _emaFast;
        private ExponentialMovingAverage _emaSlow;
        private AverageTrueRange         _atr;

        // Minimum bar index before all sub-indicators have a valid value.
        private int _warmup;

        // ════════════════════════════════════════════════════════════════════
        protected override void Initialize()
        {
            _rsi          = Indicators.RelativeStrengthIndex(Bars.ClosePrices, RsiLength);
            _stochRsiBase = Indicators.RelativeStrengthIndex(Bars.ClosePrices, StochRsiRsiLen);
            _macd         = Indicators.MacdCrossOver(MacdFast, MacdSlow, MacdSignal);
            _emaFast      = Indicators.ExponentialMovingAverage(Bars.ClosePrices, EmaFastLen);
            _emaSlow      = Indicators.ExponentialMovingAverage(Bars.ClosePrices, EmaSlowLen);
            _atr          = Indicators.AverageTrueRange(AtrLength, MovingAverageType.WilderSmoothing);

            // StochRSI needs: RSI warmup + stoch window + K smooth + D smooth
            int stochRsiWarmup = StochRsiRsiLen + StochRsiStochLen + StochRsiKPeriod + StochRsiDPeriod;

            _warmup = Math.Max(
                Math.Max(RsiLength, stochRsiWarmup),
                Math.Max(MacdSlow + MacdSignal, Math.Max(EmaSlowLen, MfiLength)));
        }

        // ════════════════════════════════════════════════════════════════════
        public override void Calculate(int index)
        {
            double threshNorm = SignalThresholdPct / 100.0;
            ThresholdPos[index] =  threshNorm;
            ThresholdNeg[index] = -threshNorm;

            if (index < _warmup)
            {
                CombinedScore[index] = 0;
                Confidence[index]    = 0;
                ScoreRsi[index]      = 0;
                ScoreStochRsi[index] = 0;
                ScoreMacd[index]     = 0;
                ScoreEma[index]      = 0;
                ScoreMfi[index]      = 0;
                return;
            }

            // ── 1. RSI  (weight 1.0) ──────────────────────────────────────────
            // Pine: s_rsi = clamp((rsi14 - 50) / 50, -1, 1)
            double rsiVal   = _rsi.Result[index];
            double scoreRsi = Clamp((rsiVal - 50.0) / 50.0, -1.0, 1.0);

            // ── 2. Stochastic RSI  (weight 1.2) ───────────────────────────────
            // Pine: k_raw = ta.stoch(rsi_src, rsi_src, rsi_src, stochLen)
            //       k_sm  = ta.sma(k_raw, kSmooth)
            //       d_sm  = ta.sma(k_sm,  dSmooth)
            //       score = clamp((k_sm-50)/50*0.6 + (k_sm-d_sm)/20*0.4, -1, 1)
            double scoreStochRsi = ComputeStochRsiScore(index);

            // ── 3. MACD histogram direction  (weight 1.5) ─────────────────────
            // Pine: +1 if hist>0 and rising; +0.3 if hist>0 but falling;
            //       -1 if hist<0 and falling; -0.3 if hist<0 but rising.
            double hist     = _macd.Histogram[index];
            double histPrev = index > 0 ? _macd.Histogram[index - 1] : 0.0;
            double scoreMacd;
            if      (hist > 0.0 && hist >= histPrev) scoreMacd =  1.0;
            else if (hist < 0.0 && hist <= histPrev) scoreMacd = -1.0;
            else if (hist > 0.0)                     scoreMacd =  0.3;
            else if (hist < 0.0)                     scoreMacd = -0.3;
            else                                     scoreMacd =  0.0;

            // ── 4. EMA cross spread  (weight 1.5) ─────────────────────────────
            // Pine: spread = se > 0 ? (fe - se) / se * 300 : 0; clamp(-1, 1)
            double emaF     = _emaFast.Result[index];
            double emaS     = _emaSlow.Result[index];
            double scoreEma = emaS > 0.0
                ? Clamp((emaF - emaS) / emaS * 300.0, -1.0, 1.0)
                : 0.0;

            // ── 5. MFI  (weight 0.8) ──────────────────────────────────────────
            // Pine: ta.mfi(hlc3, volume, len); score = clamp((mfi-50)/50, -1, 1)
            // TODO: replace Bars.TickVolumes with real traded volume if your broker
            //       feed provides it (e.g. via a custom data source).
            double scoreMfi = ComputeMfiScore(index);

            // ── Weighted composite (weights sum to 6.0) ───────────────────────
            //   RSI=1.0  StochRSI=1.2  MACD=1.5  EMA=1.5  MFI=0.8
            double raw = scoreRsi      * 1.0
                       + scoreStochRsi * 1.2
                       + scoreMacd     * 1.5
                       + scoreEma      * 1.5
                       + scoreMfi      * 0.8;

            double combined   = Clamp(raw / 6.0, -1.0, 1.0);
            double confidence = Math.Abs(combined); // 0–1; ×100 = confidence %

            CombinedScore[index] = combined;
            Confidence[index]    = confidence;
            ScoreRsi[index]      = scoreRsi;
            ScoreStochRsi[index] = scoreStochRsi;
            ScoreMacd[index]     = scoreMacd;
            ScoreEma[index]      = scoreEma;
            ScoreMfi[index]      = scoreMfi;

            // ── Telemetry log (last bar only, when enabled) ───────────────────
            if (EnableTelemetry && IsLastBar)
            {
                double pipSz   = GetPipSize();
                double atrVal  = _atr.Result[index];
                double close   = Bars.ClosePrices[index];
                double confPct = confidence * 100.0;

                bool   isBull = combined >= 0.0;
                // Express TP/SL as signed pip distances from current close.
                double slPips  = (isBull ? -SlMult  : SlMult)  * PriceToPips(atrVal);
                double tp1Pips = (isBull ?  Tp1Mult : -Tp1Mult) * PriceToPips(atrVal);
                double tp2Pips = (isBull ?  Tp2Mult : -Tp2Mult) * PriceToPips(atrVal);
                double tp3Pips = (isBull ?  Tp3Mult : -Tp3Mult) * PriceToPips(atrVal);

                string dir = isBull ? "▲ BUY" : "▼ SELL";

                Print(
                    $"[HF v1.4 | {Bars.OpenTimes[index]:yyyy-MM-dd HH:mm}] {dir}  " +
                    $"Combined={combined:+0.000;-0.000;0.000}  Conf={confPct:0.0}%  " +
                    $"| RSI={scoreRsi:+0.000;-0.000}  StochRSI={scoreStochRsi:+0.000;-0.000}  " +
                    $"MACD={scoreMacd:+0.000;-0.000}  EMA={scoreEma:+0.000;-0.000}  MFI={scoreMfi:+0.000;-0.000}  " +
                    $"| ATR={PriceToPips(atrVal):0.1}p  SL={slPips:+0.1;-0.1}p  TP1={tp1Pips:+0.1}p  TP2={tp2Pips:+0.1}p  TP3={tp3Pips:+0.1}p");

                if (confPct >= SignalThresholdPct)
                    Print($"[HF v1.4] *** ADVISORY SIGNAL: {dir}  confidence {confPct:0.0}% ***");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  StochRSI  (manual — no cTrader built-in applies Stochastic to RSI)
        //
        //  Pine equivalent:
        //    rsi_src = ta.rsi(close, rsiLen)
        //    k_raw   = ta.stoch(rsi_src, rsi_src, rsi_src, stochLen)
        //    k_sm    = ta.sma(k_raw, kSmooth)      // %K
        //    d_sm    = ta.sma(k_sm,  dSmooth)      // %D
        //    score   = clamp((k_sm-50)/50*0.6 + (k_sm-d_sm)/20*0.4, -1, 1)
        // ════════════════════════════════════════════════════════════════════
        private double ComputeStochRsiScore(int index)
        {
            // Compute StochRsiDPeriod smoothed-%K values going back in time to derive %D.
            var smoothedK = new double[StochRsiDPeriod];
            for (int d = 0; d < StochRsiDPeriod; d++)
            {
                // Smooth kPeriod raw %K values centred at (index - d).
                double kSum = 0.0;
                for (int k = 0; k < StochRsiKPeriod; k++)
                    kSum += RawStochK(index - d - k);
                smoothedK[d] = kSum / StochRsiKPeriod;
            }

            double kCurrent = smoothedK[0];

            double dSum = 0.0;
            for (int d = 0; d < StochRsiDPeriod; d++)
                dSum += smoothedK[d];
            double dCurrent = dSum / StochRsiDPeriod;

            double kNorm    = (kCurrent - 50.0) / 50.0;
            double kdSpread = (kCurrent - dCurrent) / 20.0;
            return Clamp(kNorm * 0.6 + kdSpread * 0.4, -1.0, 1.0);
        }

        // Raw Stochastic %K applied to the RSI series at a given bar index.
        // %K = (RSI[idx] - lowestRSI[window]) / (highestRSI[window] - lowestRSI[window]) × 100
        private double RawStochK(int index)
        {
            if (index < 0) return 50.0;

            double high = double.MinValue;
            double low  = double.MaxValue;

            for (int i = 0; i < StochRsiStochLen; i++)
            {
                int idx = index - i;
                if (idx < 0) break;
                double v = _stochRsiBase.Result[idx];
                if (double.IsNaN(v)) continue;
                if (v > high) high = v;
                if (v < low)  low  = v;
            }

            if (double.IsInfinity(high) || double.IsInfinity(low) || high <= low)
                return 50.0; // neutral when range is undefined

            double cur = _stochRsiBase.Result[index];
            if (double.IsNaN(cur)) return 50.0;

            return (cur - low) / (high - low) * 100.0;
        }

        // ════════════════════════════════════════════════════════════════════
        //  MFI  (manual — cTrader has no built-in Money Flow Index)
        //
        //  Pine equivalent:
        //    ta.mfi(hlc3, volume, len)
        //    hlc3 = (high + low + close) / 3
        //    score = clamp((mfi - 50) / 50, -1, 1)
        //
        //  TODO: Bars.TickVolumes is a tick-count proxy; replace with a real
        //        traded-volume data series if your broker or data provider
        //        makes it available.
        // ════════════════════════════════════════════════════════════════════
        private double ComputeMfiScore(int index)
        {
            if (index < MfiLength) return 0.0;

            double posFlow = 0.0;
            double negFlow = 0.0;

            for (int i = 0; i < MfiLength; i++)
            {
                int idx = index - i;
                if (idx <= 0) break;

                double tp     = (Bars.HighPrices[idx]     + Bars.LowPrices[idx]     + Bars.ClosePrices[idx])     / 3.0;
                double tpPrev = (Bars.HighPrices[idx - 1] + Bars.LowPrices[idx - 1] + Bars.ClosePrices[idx - 1]) / 3.0;
                double flow   = tp * Bars.TickVolumes[idx];

                if (tp > tpPrev) posFlow += flow;
                else             negFlow += flow;
            }

            double total = posFlow + negFlow;
            if (total == 0.0) return 0.0;

            double mfi = 100.0 * posFlow / total;
            return Clamp((mfi - 50.0) / 50.0, -1.0, 1.0);
        }

        // ════════════════════════════════════════════════════════════════════
        //  Pip-size helper
        //
        //  Pine equivalent:
        //    Auto:     forex  → syminfo.mintick × 10   (5-digit/3-digit FX)
        //              others → syminfo.mintick
        //    Override: user-supplied value
        //
        //  cTrader equivalent:
        //    Symbol.PipSize  = standard pip (0.0001 EURUSD, 0.01 USDJPY, etc.)
        //                      On non-forex instruments it equals Symbol.TickSize.
        //    Symbol.TickSize = minimum price increment (syminfo.mintick analogue).
        //
        //  We prefer Symbol.PipSize for ATR-in-pips conversions; fall back to
        //  Symbol.TickSize only when PipSize is unavailable or zero.
        //  When UseCustomPipSize = true the user-supplied override is used directly,
        //  matching Pine's "Override" pip mode.
        // ════════════════════════════════════════════════════════════════════
        private double GetPipSize()
        {
            if (UseCustomPipSize && PipSizeOverride > 0.0)
                return PipSizeOverride;

            double ps = Symbol.PipSize;
            return ps > 0.0 ? ps : Symbol.TickSize;
        }

        private double PriceToPips(double priceDistance) => priceDistance / GetPipSize();
        private double PipsToPrice(double pips)          => pips * GetPipSize();

        // Standard .NET 6 Math.Clamp wraps — named locally for call-site clarity.
        private static double Clamp(double value, double min, double max)
            => Math.Clamp(value, min, max);
    }
}
