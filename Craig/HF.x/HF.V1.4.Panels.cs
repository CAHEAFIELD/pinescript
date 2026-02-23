// The MIT License (MIT)
// Copyright © 2024-2026 CAHEAFIELD
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
//  EA.HF Scalper v1.4  —  Optional sub-panel indicators
//
//  These three lightweight indicators are optional companions to HF.V1.4.cs.
//  Each is a standalone cTrader Indicator that renders RSI, Stochastic, or
//  MACD in its own chart panel.  They are intentionally kept separate from the
//  main HFScalperV14 class because cTrader requires exactly one [Indicator]
//  type per compiled .algo assembly.
// ════════════════════════════════════════════════════════════════════════════

using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace CAHEAFIELD.HF
{
    // ─────────────────────────────────────────────────────────────────────
    // OPTIONAL PANELS (separate indicators)
    // NOTE: cTrader indicators cannot render to multiple panels from one class.
    // Add these as separate indicators to get RSI/Stoch/MACD in their own panels.
    // ─────────────────────────────────────────────────────────────────────

    [Indicator("EA.HF RSI v1.4", IsOverlay = false, AutoRescale = true, AccessRights = AccessRights.None)]
    public class EAHF_Rsi_v14 : Indicator
    {
        [Parameter("RSI Period", DefaultValue = 14, MinValue = 1)]
        public int Period { get; set; }

        [Output("RSI", LineColor = "DodgerBlue", PlotType = PlotType.Line, Thickness = 2)]
        public IndicatorDataSeries Result { get; set; }

        private RelativeStrengthIndex _rsi;

        protected override void Initialize()
        {
            _rsi = Indicators.RelativeStrengthIndex(Bars.ClosePrices, Period);
        }

        public override void Calculate(int index)
        {
            Result[index] = _rsi.Result[index];
        }
    }

    [Indicator("EA.HF Stochastic v1.4", IsOverlay = false, AutoRescale = true, AccessRights = AccessRights.None)]
    public class EAHF_Stoch_v14 : Indicator
    {
        [Parameter("%K Period", DefaultValue = 14, MinValue = 1)]
        public int KPeriod { get; set; }

        [Parameter("%D Period", DefaultValue = 3, MinValue = 1)]
        public int DPeriod { get; set; }

        [Parameter("Slowing", DefaultValue = 3, MinValue = 1)]
        public int Slowing { get; set; }

        [Output("%K", LineColor = "LimeGreen", PlotType = PlotType.Line, Thickness = 2)]
        public IndicatorDataSeries K { get; set; }

        [Output("%D", LineColor = "Orange", PlotType = PlotType.Line, Thickness = 2)]
        public IndicatorDataSeries D { get; set; }

        private StochasticOscillator _stoch;

        protected override void Initialize()
        {
            _stoch = Indicators.StochasticOscillator(KPeriod, DPeriod, Slowing, MovingAverageType.Simple);
        }

        public override void Calculate(int index)
        {
            K[index] = _stoch.PercentK[index];
            D[index] = _stoch.PercentD[index];
        }
    }

    [Indicator("EA.HF MACD v1.4", IsOverlay = false, AutoRescale = true, AccessRights = AccessRights.None)]
    public class EAHF_Macd_v14 : Indicator
    {
        [Parameter("Fast", DefaultValue = 12, MinValue = 1)]
        public int Fast { get; set; }

        [Parameter("Slow", DefaultValue = 26, MinValue = 1)]
        public int Slow { get; set; }

        [Parameter("Signal", DefaultValue = 9, MinValue = 1)]
        public int Signal { get; set; }

        [Output("MACD", LineColor = "DodgerBlue", PlotType = PlotType.Line, Thickness = 2)]
        public IndicatorDataSeries Macd { get; set; }

        [Output("Signal", LineColor = "Orange", PlotType = PlotType.Line, Thickness = 2)]
        public IndicatorDataSeries Sig { get; set; }

        [Output("Histogram", LineColor = "Gray", PlotType = PlotType.Histogram, Thickness = 2)]
        public IndicatorDataSeries Hist { get; set; }

        private MacdCrossOver _macd;

        protected override void Initialize()
        {
            _macd = Indicators.MacdCrossOver(Fast, Slow, Signal);
        }

        public override void Calculate(int index)
        {
            Macd[index] = _macd.MACD[index];
            Sig[index]  = _macd.Signal[index];
            Hist[index] = _macd.Histogram[index];
        }
    }

}
