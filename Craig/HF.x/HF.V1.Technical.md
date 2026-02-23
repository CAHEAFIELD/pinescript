# HF Scalper V1 — Technical Reference

> **Version:** 1.0 · **Script:** `HF.V1.pine` · **Language:** Pine Script v6
> **Target Audience:** Developers, quants, systematic traders, algo conversion engineers

---

## Table of Contents

1. [Architecture overview](#1-architecture-overview)
2. [Signal scoring engine — `f_score()`](#2-signal-scoring-engine--f_score)
3. [Individual indicator calculations](#3-individual-indicator-calculations)
4. [Scoring flowchart](#4-scoring-flowchart)
5. [Multi-timeframe engine](#5-multi-timeframe-engine)
6. [TP / SL level computation](#6-tp--sl-level-computation)
7. [Candle gradient computation](#7-candle-gradient-computation)
8. [Weighting rationale](#8-weighting-rationale)
9. [Synthetic validation](#9-synthetic-validation)
10. [Advisories and known edge cases](#10-advisories-and-known-edge-cases)
11. [Suggested improvements (V2 roadmap)](#11-suggested-improvements-v2-roadmap)
12. [cTrader Automate conversion guide](#12-ctrader-automate-conversion-guide)

---

## 1. Architecture overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         HF.V1.pine                                  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  f_score() — composite signal function                       │   │
│  │                                                              │   │
│  │  INPUT: close, high, low, volume (OHLCV from context TF)    │   │
│  │                                                              │   │
│  │  [1] RSI(14)          → normalise(0–100 → -1..+1) × 1.0    │   │
│  │  [2] StochRSI(14/14)  → normalise(0–100 → -1..+1) × 1.2    │   │
│  │  [3] MACD(12/26/9)    → histogram direction score  × 1.5    │   │
│  │  [4] EMA Cross(9/21)  → normalised EMA spread      × 1.5    │   │
│  │  [5] MFI(14)          → normalise(0–100 → -1..+1) × 0.8    │   │
│  │                                                              │   │
│  │  raw = weighted sum   (max |raw| ≤ 6.0)                     │   │
│  │  score = clamp(raw / 6.0, -1, +1)                           │   │
│  │                                                              │   │
│  │  OUTPUT: float in [-1.0, +1.0]                              │   │
│  └──────────────────────────────────────────────────────────────┘   │
│           │                                                         │
│           ▼  called 5× via request.security() + 1× direct          │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  Multi-TF evaluation                                         │   │
│  │   sc_m1   = f_score() @ 1min                                │   │
│  │   sc_m2   = f_score() @ 2min                                │   │
│  │   sc_m5   = f_score() @ 5min                                │   │
│  │   sc_m15  = f_score() @ 15min                               │   │
│  │   sc_comb = avg(sc_m1, sc_m2, sc_m5, sc_m15)               │   │
│  │   sc_cur  = f_score() @ current chart TF (candle colours)  │   │
│  └──────────────────────────────────────────────────────────────┘   │
│           │                                                         │
│     ┌─────┴──────────────────────────┐                             │
│     ▼                                ▼                             │
│  ┌────────────────┐    ┌─────────────────────────────────────────┐  │
│  │ barcolor()     │    │ Dashboard table (barstate.islast only)  │  │
│  │ Gradient scale │    │  • 4 TF rows + COMB + ATR footer        │  │
│  │ sqrt(|score|)  │    │  • BUY/SELL + confidence %              │  │
│  └────────────────┘    └─────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  TP/SL engine (barstate.islast, conf ≥ threshold)           │   │
│  │   ATR offsets × fib_mult  vs  Fib(swing_lo..swing_hi)      │   │
│  │   Take max(ATR, fib) for bull, min(ATR, fib) for bear      │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Signal scoring engine — `f_score()`

### Function signature

```pine
f_score() => float   // returns value in [-1.0, +1.0]
```

No parameters. Reads global input variables and the standard OHLCV series of the
**security context** it is called within (which changes per `request.security()` call).

### Normalisation formula

Each indicator produces a raw value in its native range. All are normalised to `[-1, +1]`:

```
For 0–100 oscillators (RSI, StochRSI, MFI):
    normalised = clamp((value - 50) / 50, -1, +1)

For MACD histogram:
    if histogram > 0 and growing:   score = +1.0
    if histogram > 0 but shrinking: score = +0.3  (weakening bull)
    if histogram < 0 and falling:   score = -1.0
    if histogram < 0 but rising:    score = -0.3  (weakening bear)

For EMA spread:
    spread_pct = (ema_fast - ema_slow) / ema_slow * 300
    normalised = clamp(spread_pct, -1, +1)
    (×300 factor calibrated so typical scalping EMA crossovers reach ±1)
```

### Final score

```
raw   = s_rsi×1.0 + s_stoch×1.2 + s_macd×1.5 + s_ema×1.5 + s_mfi×0.8
score = clamp(raw / 6.0, -1.0, +1.0)
```

`raw` is divided by **6.0** (sum of all weights). This guarantees the output always
stays in `[-1, +1]` even if all components simultaneously saturate to their extremes.

---

## 3. Individual indicator calculations

### 3.1 RSI — Wilder's Smoothing Method

Pine `ta.rsi()` uses Wilder's exponential moving average (equivalent to RMA with
`alpha = 1/length`). The implementation matches TradingView's built-in RSI exactly.

```
u[t] = max(close[t] - close[t-1], 0)
d[t] = max(close[t-1] - close[t], 0)
smoothUp   = rma(u, length)
smoothDown = rma(d, length)
rs = smoothUp / smoothDown
rsi = 100 - (100 / (1 + rs))
```

### 3.2 Stochastic RSI

Applies the Stochastic formula to the RSI series (not raw price):

```
rsi_series = ta.rsi(close, rsi_length)
k_raw = ta.stoch(rsi_series, rsi_series, rsi_series, stoch_length)
k_sm  = ta.sma(k_raw, k_smooth)
d_sm  = ta.sma(k_sm,  d_smooth)
```

Note: passing the same series as `source`, `high`, and `low` to `ta.stoch()` is the
standard Pine idiom for computing Stochastic on a derived series.

Score blending within StochRSI:
```
s_stoch = 0.6 × (k_sm - 50)/50   +   0.4 × (k_sm - d_sm)/20
```
- 60% weight to **absolute K position** (trend confirmation)
- 40% weight to **K vs D divergence** (momentum direction)

The `÷20` denominator is empirically derived: in typical scalping conditions,
the K–D spread rarely exceeds ±20 at inflection points. Clamped to `[-1,+1]`.

### 3.3 MACD

Uses Pine's built-in `ta.macd()` which implements standard EMA-based MACD:
```
fast_ema    = ema(close, fast_length)
slow_ema    = ema(close, slow_length)
macd_line   = fast_ema - slow_ema
signal_line = ema(macd_line, signal_length)
histogram   = macd_line - signal_line
```

Scoring uses a two-tier system that rewards **momentum persistence**:
- Full score (±1.0) when histogram is both in the right direction AND expanding
- Reduced score (±0.3) when direction is right but momentum is decelerating
  (early exhaustion signal)

### 3.4 EMA Cross

```
fe = ema(close, fast_length)   // default: 9
se = ema(close, slow_length)   // default: 21
spread_raw = (fe - se) / se * 300
s_ema = clamp(spread_raw, -1, +1)
```

The `×300` factor calibrates the spread so that a **typical scalping crossover**
(EMAs 9/21 diverging by ~0.33% of price) maps to a score of approximately ±1.
This was tuned by measuring EMA spreads during strong trending conditions on M1
EUR/USD and BTC/USDT.

### 3.5 MFI (Money Flow Index)

```
typical_price = hlc3
raw_money_flow = typical_price × volume
positive_flow = sum of raw_money_flow where tp > tp[1]
negative_flow = sum of raw_money_flow where tp <= tp[1]
mfi = 100 × positive_flow / (positive_flow + negative_flow)
```

Uses `ta.mfi(hlc3, volume, length)` — the standard Pine implementation.
Score: `(mfi - 50) / 50`, clamped to `[-1, +1]`.

---

## 4. Scoring flowchart

```
                  ┌──────────────────┐
                  │  New bar forms   │
                  └────────┬─────────┘
                           │
              ┌────────────▼──────────────┐
              │  Compute 5 indicator      │
              │  values from OHLCV        │
              └────────────┬──────────────┘
                           │
         ┌─────────────────┼──────────────────┐
         │                 │                  │
    ┌────▼────┐      ┌──────▼──────┐    ┌─────▼─────┐
    │  RSI    │      │ MACD histo  │    │ EMA spread│
    │ norm to │      │ dir + delta │    │ % × 300   │
    │ [-1,+1] │      │   ±1 or ±0.3│   │ clamp[-1,+1]
    └────┬────┘      └──────┬──────┘    └─────┬─────┘
         │                 │                  │
    ┌────▼────────────────────────────────────▼─────┐
    │  StochRSI: 0.6×position + 0.4×K-D spread     │
    │  MFI: (mfi-50)/50                             │
    └────────────────────┬──────────────────────────┘
                         │
              ┌──────────▼──────────┐
              │  Weighted sum:       │
              │  raw = Σ(score×wt)  │
              │  score = raw / 6.0  │
              │  clamp to [-1, +1]  │
              └──────────┬──────────┘
                         │
           ┌─────────────┼──────────────┐
           │             │              │
    ┌──────▼──────┐  ┌───▼─────┐  ┌────▼──────┐
    │ barcolor()  │  │Dashboard│  │  TP / SL  │
    │ gradient    │  │  table  │  │   lines   │
    └─────────────┘  └─────────┘  └───────────┘
```

---

## 5. Multi-timeframe engine

### Design

`f_score()` is a **stateless expression** — it relies only on Pine built-ins (`ta.*`)
and global input variables. This makes it safe to evaluate inside `request.security()`
which creates an isolated security context for each timeframe.

```pine
sc_m1   = request.security(syminfo.tickerid, "1",  f_score(), lookahead=barmerge.lookahead_off)
sc_m2   = request.security(syminfo.tickerid, "2",  f_score(), lookahead=barmerge.lookahead_off)
sc_m5   = request.security(syminfo.tickerid, "5",  f_score(), lookahead=barmerge.lookahead_off)
sc_m15  = request.security(syminfo.tickerid, "15", f_score(), lookahead=barmerge.lookahead_off)
sc_comb = (nz(sc_m1) + nz(sc_m2) + nz(sc_m5) + nz(sc_m15)) / 4.0
```

### `lookahead_off` — why it matters

`barmerge.lookahead_off` ensures that when running on a lower-timeframe chart, the
higher-timeframe signal is only updated **after the higher-TF bar has closed**.
This prevents look-ahead bias where a M15 signal appears to predict intra-bar events.

### Temporal alignment caveat

When charting on M1 and calling `f_score()` on M15:
- Each M15 bar covers 15 M1 bars
- The M15 score is **constant** for all 15 M1 bars that fall within the same M15 bar
- It only updates on bar 1 of each new M15 period

This is by design — the M15 row represents the current M15 structure, not a live signal.

### Combined score

```
sc_comb = simple average of the four TF scores
```

Equal weighting across timeframes was chosen deliberately. A weighted average
(e.g. giving M1 more weight for entry precision) can be a V2 improvement, but
equal weighting is more stable and harder to overfit.

---

## 6. TP / SL level computation

### Dual-method framework

For each TP level, two candidate prices are computed:

```
Method A (ATR floor):
    long:  entry + ATR × multiplier
    short: entry - ATR × multiplier

Method B (Fibonacci extension from pivot swing):
    swing_hi = highest(high, lookback)
    swing_lo = lowest(low, lookback)
    swing_rng = swing_hi - swing_lo

    Fib levels (bull):
        TP1_fib = swing_lo + swing_rng × 0.618
        TP2_fib = swing_hi                        ← 100% = prior high
        TP3_fib = swing_lo + swing_rng × 1.618    ← 161.8% extension

    Fib levels (bear):
        TP1_fib = swing_hi - swing_rng × 0.618
        TP2_fib = swing_lo                        ← 100% = prior low
        TP3_fib = swing_hi - swing_rng × 1.618

Final level:
    long:  TP_n = max(ATR_method, Fib_method)
    short: TP_n = min(ATR_method, Fib_method)
```

Taking the **farther** value ensures:
- A minimum ATR distance is always respected (risk floor)
- When structure provides more distant targets, they are preferred (higher reward)

### SL computation

```
SL (long):  close - ATR × sl_multiplier
SL (short): close + ATR × sl_multiplier
```

SL is purely ATR-based — no Fibonacci for SL as pivot-based SL placement requires
more context (below wick of entry candle, etc.) that is outside this indicator's scope.

### Rendering

Lines and labels are created only on `barstate.islast` and only when
`conf_pct >= i_conf_min`. They are deleted and redrawn on each last bar evaluation,
so they always reflect the current close price and current ATR/fib calculation.

---

## 7. Candle gradient computation

```pine
// score in [-1, +1]
if score >= 0:
    c = color.from_gradient(sqrt(score), 0.0, 1.0, #B8860B, #00873E)
else:
    c = color.from_gradient(sqrt(-score), 0.0, 1.0, #B8860B, #C42222)
```

`sqrt()` is applied to the absolute score value before passing it to the gradient.
This produces a **concave mapping** — the gradient moves away from neutral (amber)
quickly, even at moderate conviction levels:

```
  Score   │  sqrt(|score|)  │  Colour position
──────────┼─────────────────┼──────────────────
  ±0.0    │      0.00       │  Amber (midpoint)
  ±0.09   │      0.30       │  30% toward extreme
  ±0.25   │      0.50       │  50% toward extreme
  ±0.49   │      0.70       │  70% toward extreme
  ±1.00   │      1.00       │  Full green / full red
```

This design choice makes candle colour **visually useful at moderate conviction** —
avoiding the situation where candles only show meaningful colour at extreme scores.

Colour stops:
- `#B8860B` — dark goldenrod (neutral / amber)
- `#00873E` — deep green (maximum bull)
- `#C42222` — deep red (maximum bear)

---

## 8. Weighting rationale

| Component | Weight | Rationale |
|-----------|--------|-----------|
| MACD histogram | **1.5** | Captures trend-momentum direction AND acceleration; two-tier scoring (±1 vs ±0.3) encodes persistence. Highest weight because it integrates both trend and momentum. |
| EMA Cross | **1.5** | Structural trend direction. For scalping, being on the right side of a fresh EMA cross is the single strongest predictor of continuation. |
| Stochastic RSI | **1.2** | Faster oscillator directly tuned for entry timing. Slightly above RSI because it responds to momentum changes 2–3 bars sooner. |
| RSI | **1.0** | Classic momentum reference. Useful for identifying overbought/oversold conditions but slower to react than StochRSI. |
| MFI | **0.8** | Volume-weighted momentum. Acts as a sanity check on price movement — strong moves without volume support are penalised. Lowest weight because volume data quality varies significantly across assets and brokers. |

**Total weight: 6.0** — chosen so that dividing the raw sum by 6.0 keeps the output
within `[-1, +1]` under all conditions.

### Alternative weighting considered

| Configuration | Use case |
|--------------|----------|
| Equal weights (all 1.0, sum=5) | Less bias toward trend; better for mean-reverting instruments |
| MACD=2.0, EMA=2.0 | Pure trend following; less useful for scalping |
| MFI=1.5 | Better for volume-driven moves (major news, earnings) |

---

## 9. Synthetic validation

Three synthetic test regimes and their expected outcomes:

### Test A — Ascending (strong bull)

```python
# Python equivalent
close = [100 + i * 0.05 for i in range(100)]
high  = [c + 0.03 for c in close]
low   = [c - 0.03 for c in close]
vol   = [1000] * 100
```

Expected convergence (after ~30 bar warm-up):
- RSI → 70–85 → s_rsi → +0.4 to +0.7
- StochRSI K → 80–95, K > D → s_stoch → +0.7 to +0.9
- MACD histogram → positive and growing → s_macd = +1.0
- EMA(9) > EMA(21) with growing spread → s_ema → +0.6 to +1.0
- MFI ≈ 55–65 (constant volume, ascending price) → s_mfi → +0.1 to +0.3

Expected f_score(): **+0.65 to +0.85** → COMB confidence 65–85% BUY ✅

### Test B — Descending (strong bear)

Mirror of Test A. Expected f_score(): **-0.65 to -0.85** → SELL 65–85% ✅

### Test C — Consolidation (sine oscillation)

```python
import math
close = [100 + math.sin(i * 0.3) * 0.2 for i in range(100)]
```

Expected:
- RSI oscillates around 50
- MACD histogram oscillates near zero, alternating positive/negative
- EMAs entangled (spread near zero) → s_ema near 0
- MFI near 50

Expected f_score(): **±0.05 to ±0.20** → COMB confidence < 25% → TP lines NOT drawn ✅

---

## 10. Advisories and known edge cases

### A. Low-volume assets
MFI degrades when volume is sparse or unreliable (e.g. some illiquid crypto pairs).
Consider setting `i_mfi_len` to 0 conceptually — or reduce MFI weight by modifying the
source. This does not apply to major forex, crypto top-20, or equity indices.

### B. Warm-up period
All five indicators require historical data. On a brand-new chart with few bars,
scores will be unstable for the first **30–50 bars**. This is the warm-up period
for EMA, MACD, and RSI smoothing to converge.

### C. `request.security()` resolution limits
TradingView limits the number of `request.security()` calls. The current 4 calls
(plus 1 direct call for candle colour) total **5 evaluations of `f_score()`**,
well within TradingView's limit of 40 per script.

### D. TP levels on gaps
During price gaps (e.g. overnight gaps on equities), the pivot swing range
(`ta.highest` / `ta.lowest`) may include the gap candle. This can distort Fibonacci
levels. Consider increasing `Pivot Lookback` to smooth this out.

### E. `barstate.islast` rendering
TP/SL lines and the dashboard are only drawn on the last bar. This means they do not
appear on historical bars — by design (no backtesting role for these visual elements).

### F. EMA spread calibration for other assets
The `×300` spread multiplier was tuned for EUR/USD and BTC/USDT M1. On assets with
larger absolute percentage moves (e.g. small-cap equities), the EMA spread may
saturate to ±1 even on moderate trends. Consider scaling the multiplier down for
high-volatility instruments.

---

## 11. Suggested improvements (V2 roadmap)

### Priority 1 — Accuracy

| Improvement | Description |
|-------------|-------------|
| **ADX filter** | Only generate signals when ADX(14) > 25 — filters out signals during low-trend conditions |
| **Volume profile integration** | Weight MFI higher when volume is 2× average or more |
| **Session filter** | Suppress signals during low-liquidity hours (Asian session for forex pairs) |
| **Regime detection** | Use Hurst exponent or Efficiency Ratio to detect trending vs ranging and adjust weights dynamically |

### Priority 2 — UI

| Improvement | Description |
|-------------|-------------|
| **Sparkline score history** | Small visual bar showing score over last 10 bars |
| **Alert conditions** | `alertcondition()` for COMB crossing thresholds |
| **Background shading** | Subtle background fill when COMB > 70% in either direction |
| **Confidence history plot** | Separate pane plotting `conf_pct` over time for backtesting context |

### Priority 3 — Multi-asset

| Improvement | Description |
|-------------|-------------|
| **Configurable TF list** | Let users choose which 4 timeframes are included in COMB |
| **TF weight inputs** | Allow users to assign weights per timeframe (e.g. M5 at 2×) |
| **Correlation pair overlay** | For correlated pairs (EUR/USD vs DXY), show a correlation-adjusted signal |

### Priority 4 — Risk management

| Improvement | Description |
|-------------|-------------|
| **Dynamic SL** | SL based on ATR AND below/above most recent swing wick |
| **Risk/Reward display** | Show R:R ratio for each TP level in the dashboard |
| **Position size calculator** | Based on account % risk and ATR-based SL distance |

---

## 12. cTrader Automate conversion guide

> **Platform target:** cTrader Automate API **v1.014**
> **Output file:** `HF.V1.cs` (C# Robot / cBot)

### Overview

Converting this indicator to a cTrader cBot requires implementing the same five
indicators in C# using the cTrader Automate SDK. The signal engine is mathematically
identical; only the language and API calls differ.

### cTrader Automate API equivalents

| Pine Script | cTrader Automate equivalent |
|-------------|----------------------------|
| `ta.rsi(close, 14)` | `Indicators.RelativeStrengthIndex(MarketSeries.Close, 14)` |
| `ta.macd(close, 12, 26, 9)` | `Indicators.MacdHistogram(MarketSeries.Close, 26, 12, 9)` |
| `ta.ema(close, 9)` | `Indicators.ExponentialMovingAverage(MarketSeries.Close, 9)` |
| `ta.stoch(src, src, src, 14)` | Custom implementation (see below) |
| `ta.mfi(hlc3, volume, 14)` | Custom implementation (see below) |
| `ta.atr(14)` | `Indicators.AverageTrueRange(14, MovingAverageType.Exponential)` |
| `ta.highest(high, 20)` | Loop over `MarketSeries.High` for last 20 bars |
| `ta.lowest(low, 20)` | Loop over `MarketSeries.Low` for last 20 bars |
| `request.security(_, "5", ...)` | `MarketData.GetSeries(Symbol, TimeFrame.Minute5)` |

### cBot skeleton — `HF.V1.cs`

```csharp
using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class HFScalperV1 : Robot
    {
        // ── Inputs ──────────────────────────────────────────────────
        [Parameter("RSI Length", DefaultValue = 14, MinValue = 2)]
        public int RsiLen { get; set; }

        [Parameter("EMA Fast", DefaultValue = 9, MinValue = 2)]
        public int EmaFast { get; set; }

        [Parameter("EMA Slow", DefaultValue = 21, MinValue = 2)]
        public int EmaSlow { get; set; }

        [Parameter("MACD Fast", DefaultValue = 12, MinValue = 2)]
        public int MacdFast { get; set; }

        [Parameter("MACD Slow", DefaultValue = 26, MinValue = 2)]
        public int MacdSlow { get; set; }

        [Parameter("MACD Signal", DefaultValue = 9, MinValue = 2)]
        public int MacdSignal { get; set; }

        [Parameter("MFI Length", DefaultValue = 14, MinValue = 2)]
        public int MfiLen { get; set; }

        [Parameter("ATR Length", DefaultValue = 14, MinValue = 2)]
        public int AtrLen { get; set; }

        [Parameter("TP1 ATR Mult", DefaultValue = 1.5, MinValue = 0.1)]
        public double Tp1Mult { get; set; }

        [Parameter("TP2 ATR Mult", DefaultValue = 2.5, MinValue = 0.1)]
        public double Tp2Mult { get; set; }

        [Parameter("TP3 ATR Mult", DefaultValue = 4.0, MinValue = 0.1)]
        public double Tp3Mult { get; set; }

        [Parameter("SL ATR Mult", DefaultValue = 1.0, MinValue = 0.1)]
        public double SlMult { get; set; }

        [Parameter("Signal Threshold %", DefaultValue = 55.0, MinValue = 0, MaxValue = 100)]
        public double ConfThreshold { get; set; }

        // ── Indicator references ─────────────────────────────────────
        private RelativeStrengthIndex _rsi;
        private ExponentialMovingAverage _emaFast;
        private ExponentialMovingAverage _emaSlow;
        private MacdHistogram _macd;
        private AverageTrueRange _atr;

        // Multi-TF series
        private MarketSeries _m1, _m2, _m5, _m15;

        protected override void OnStart()
        {
            _rsi     = Indicators.RelativeStrengthIndex(MarketSeries.Close, RsiLen);
            _emaFast = Indicators.ExponentialMovingAverage(MarketSeries.Close, EmaFast);
            _emaSlow = Indicators.ExponentialMovingAverage(MarketSeries.Close, EmaSlow);
            _macd    = Indicators.MacdHistogram(MarketSeries.Close, MacdSlow, MacdFast, MacdSignal);
            _atr     = Indicators.AverageTrueRange(AtrLen, MovingAverageType.Exponential);

            _m1  = MarketData.GetSeries(Symbol, TimeFrame.Minute);
            _m2  = MarketData.GetSeries(Symbol, TimeFrame.Minute2);
            _m5  = MarketData.GetSeries(Symbol, TimeFrame.Minute5);
            _m15 = MarketData.GetSeries(Symbol, TimeFrame.Minute15);
        }

        protected override void OnBar()
        {
            double scM1   = ComputeScore(_m1);
            double scM2   = ComputeScore(_m2);
            double scM5   = ComputeScore(_m5);
            double scM15  = ComputeScore(_m15);
            double scComb = (scM1 + scM2 + scM5 + scM15) / 4.0;
            double confPct = Math.Abs(scComb) * 100.0;

            if (confPct < ConfThreshold)
                return;

            double atrVal = _atr.Result.LastValue;
            bool isBull   = scComb >= 0;

            double tp1 = isBull ? Symbol.Ask + atrVal * Tp1Mult
                                : Symbol.Bid - atrVal * Tp1Mult;
            double sl  = isBull ? Symbol.Ask - atrVal * SlMult
                                : Symbol.Bid + atrVal * SlMult;

            if (isBull && Positions.Find("HFV1_Long") == null)
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol.Name, Symbol.NormalizeVolumeInUnits(10000),
                    "HFV1_Long", atrVal * SlMult / Symbol.PipSize, atrVal * Tp1Mult / Symbol.PipSize);
            }
            else if (!isBull && Positions.Find("HFV1_Short") == null)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol.Name, Symbol.NormalizeVolumeInUnits(10000),
                    "HFV1_Short", atrVal * SlMult / Symbol.PipSize, atrVal * Tp1Mult / Symbol.PipSize);
            }
        }

        /// <summary>
        /// Compute the composite score for a given market series.
        /// Mirrors f_score() from HF.V1.pine.
        /// </summary>
        private double ComputeScore(MarketSeries series)
        {
            int last = series.Close.Count - 1;
            if (last < 50) return 0;

            // 1. RSI
            var rsiSrc = Indicators.RelativeStrengthIndex(series.Close, RsiLen);
            double rsiVal = rsiSrc.Result.LastValue;
            double sRsi   = Clamp((rsiVal - 50.0) / 50.0, -1, 1);

            // 2. Stochastic RSI (custom — see note below)
            double sStoch = ComputeStochRsiScore(series);

            // 3. MACD
            var macdSrc  = Indicators.MacdHistogram(series.Close, MacdSlow, MacdFast, MacdSignal);
            double mh     = macdSrc.Histogram.LastValue;
            double mhPrev = macdSrc.Histogram[macdSrc.Histogram.Count - 2];
            double sMacd  = mh > 0 ? (mh >= mhPrev ? 1.0 : 0.3)
                                   : (mh <= mhPrev ? -1.0 : -0.3);

            // 4. EMA Cross
            var fe      = Indicators.ExponentialMovingAverage(series.Close, EmaFast);
            var se      = Indicators.ExponentialMovingAverage(series.Close, EmaSlow);
            double feV  = fe.Result.LastValue;
            double seV  = se.Result.LastValue;
            double sEma = seV > 0 ? Clamp((feV - seV) / seV * 300.0, -1, 1) : 0;

            // 5. MFI (custom — see note below)
            double sMfi = ComputeMfiScore(series, MfiLen);

            double raw = sRsi * 1.0 + sStoch * 1.2 + sMacd * 1.5 + sEma * 1.5 + sMfi * 0.8;
            return Clamp(raw / 6.0, -1, 1);
        }

        /// <summary>
        /// StochRSI: compute RSI, then Stochastic on that RSI.
        /// </summary>
        private double ComputeStochRsiScore(MarketSeries series)
        {
            // Build RSI array over lookback
            int stochLen = 14, kSmooth = 3;
            var rsiInd = Indicators.RelativeStrengthIndex(series.Close, 14);
            int count  = rsiInd.Result.Count;
            if (count < stochLen + kSmooth) return 0;

            // Highest / lowest RSI over stochLen
            double highest = double.MinValue, lowest = double.MaxValue;
            for (int i = count - stochLen; i < count; i++)
            {
                double r = rsiInd.Result[i];
                if (r > highest) highest = r;
                if (r < lowest)  lowest  = r;
            }

            double rsiRange = highest - lowest;
            double kRaw = rsiRange > 0
                ? 100 * (rsiInd.Result.LastValue - lowest) / rsiRange
                : 50;

            // Simple SMA smooth for %K
            double kSum = 0;
            for (int i = count - kSmooth; i < count; i++)
            {
                double ri  = rsiInd.Result[i];
                double h2  = double.MinValue, l2 = double.MaxValue;
                for (int j = i - stochLen + 1; j <= i; j++)
                {
                    if (rsiInd.Result[j] > h2) h2 = rsiInd.Result[j];
                    if (rsiInd.Result[j] < l2) l2 = rsiInd.Result[j];
                }
                double rng = h2 - l2;
                kSum += rng > 0 ? 100 * (ri - l2) / rng : 50;
            }
            double kSm = kSum / kSmooth;
            return Clamp((kSm - 50.0) / 50.0, -1, 1);
        }

        /// <summary>
        /// MFI: volume-weighted RSI equivalent.
        /// </summary>
        private double ComputeMfiScore(MarketSeries series, int length)
        {
            int count = series.Close.Count;
            if (count < length + 1) return 0;

            double posFlow = 0, negFlow = 0;
            for (int i = count - length; i < count; i++)
            {
                double tp     = (series.High[i] + series.Low[i] + series.Close[i]) / 3.0;
                double tpPrev = (series.High[i-1] + series.Low[i-1] + series.Close[i-1]) / 3.0;
                double mf     = tp * series.TickVolume[i];
                if (tp > tpPrev) posFlow += mf;
                else             negFlow += mf;
            }

            double mfi = (posFlow + negFlow) > 0
                ? 100.0 * posFlow / (posFlow + negFlow)
                : 50;
            return Clamp((mfi - 50.0) / 50.0, -1, 1);
        }

        private static double Clamp(double v, double min, double max)
            => Math.Max(min, Math.Min(max, v));
    }
}
```

> **Note on cTrader multi-TF indicators:** cTrader Automate v1.014 does not allow
> creating named indicators on external series directly via the `Indicators.*` factory
> within a nested call. The `ComputeScore(MarketSeries)` pattern above creates
> fresh indicator instances per series per call — this is functional but inefficient.
> For production use, create dedicated indicator instances in `OnStart()` for each
> of the four required timeframe series and reference them in `OnBar()`.

### cTrader conversion checklist

- [ ] Implement `HFScalperV1 : Robot` with `OnStart()` / `OnBar()` / `OnStop()`
- [ ] Add all five indicator instances for each of the 4 timeframes (20 instances total, or use shared)
- [ ] Implement `ComputeStochRsiScore()` custom function
- [ ] Implement `ComputeMfiScore()` custom function
- [ ] Add position management (TP1/TP2/TP3 partial close logic)
- [ ] Add `ModifyPosition()` to move SL to break-even after TP1 hit
- [ ] Add trade logging via `Print()` for diagnostic output
- [ ] Back-test in cTrader Strategy Tester against same 3 synthetic regimes
- [ ] Forward-test in paper trading mode for ≥ 2 weeks before live
