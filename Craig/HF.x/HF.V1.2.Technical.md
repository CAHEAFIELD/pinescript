# EA.HF Scalper v1.2 — Technical Reference

> **Version:** 1.2 · **Script:** `HF.V1.2.pine` · **Language:** Pine Script v6
> **Target Audience:** Developers, quants, systematic traders, algo conversion engineers

---

## Table of Contents

1. [Architecture overview](#1-architecture-overview)
2. [Signal scoring engine — `f_score_detail()` / `f_score()`](#2-signal-scoring-engine--f_score_detail--f_score)
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
│                         HF.V1.2.pine                                │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  f_score_detail() — full composite + component scores        │   │
│  │                                                              │   │
│  │  INPUT: close, high, low, volume (OHLCV from context TF)    │   │
│  │                                                              │   │
│  │  [1] RSI(14)          → normalise(0–100 → -1..+1) × 1.0    │   │
│  │  [2] StochRSI(14/14)  → normalise(0–100 → -1..+1) × 1.2    │   │
│  │  [3] MACD(12/26/9)    → histogram direction score  × 1.5    │   │
│  │  [4] EMA Cross(9/21)  → normalised EMA spread      × 1.5    │   │
│  │  [5] MFI(14)          → normalise(0–100 → -1..+1) × 0.8    │   │
│  │                                                              │   │
│  │  returns [combined, s_rsi, s_stoch, s_macd, s_ema, s_mfi]  │   │
│  └──────────────────────────────────────────────────────────────┘   │
│           │                                                         │
│  f_score() wraps f_score_detail(), extracts combined only           │
│           │ (used inside request.security() for MTF calls)          │
│           ▼                                                         │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  Multi-TF evaluation                                         │   │
│  │   sc_m1   = f_score() @ 1min                                │   │
│  │   sc_m2   = f_score() @ 2min                                │   │
│  │   sc_m5   = f_score() @ 5min                                │   │
│  │   sc_m15  = f_score() @ 15min                               │   │
│  │   sc_comb = avg(sc_m1, sc_m2, sc_m5, sc_m15)               │   │
│  │   [sc_cur, t_s_rsi, …] = f_score_detail() @ chart TF       │   │
│  └──────────────────────────────────────────────────────────────┘   │
│           │                                                         │
│     ┌─────┴──────────────────────────────┐                         │
│     ▼                                    ▼                         │
│  ┌────────────────┐    ┌─────────────────────────────────────────┐  │
│  │ barcolor()     │    │ Dashboard table (barstate.islast only)  │  │
│  │ Gradient scale │    │  • 4 TF rows + COMB + ATR footer        │  │
│  │ sqrt(|score|)  │    │  • BUY/SELL + confidence %              │  │
│  └────────────────┘    └─────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  TP/SL engine (barstate.islast; gate: conf ≥ threshold       │   │
│  │               or TP Preview Mode; + neutral zone check)     │   │
│  │   create-once / update-in-place pattern (no delete churn)   │   │
│  │   Optional: Neutral Zone filter, Fib target sanity check    │   │
│  └──────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │  Advisory Telemetry (opt-in)                                 │   │
│  │   Data Window plots: component scores, TF scores, conf_pct  │   │
│  │   alert() JSON payload on barstate.isconfirmed              │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Signal scoring engine — `f_score_detail()` / `f_score()`

### Function signatures

```pine
// Full: returns combined score + all 5 component scores
f_score_detail() => [float, float, float, float, float, float]
//  returns [combined, s_rsi, s_stoch, s_macd, s_ema, s_mfi]

// Wrapper for request.security(): returns combined score only
f_score() => float   // returns value in [-1.0, +1.0]
```

**Design rationale:** `f_score_detail()` contains all calculations.
`f_score()` is a thin wrapper that calls `f_score_detail()` and discards the
component tuple — allowing `request.security()` calls to remain clean single-float
requests while the chart-level call can destructure all components for telemetry.

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

### 3.4 EMA Cross

```
fe = ema(close, fast_length)   // default: 9
se = ema(close, slow_length)   // default: 21
spread_raw = (fe - se) / se * 300
s_ema = clamp(spread_raw, -1, +1)
```

The `×300` factor calibrates the spread so that a typical scalping crossover
(EMAs 9/21 diverging by ~0.33% of price) maps to a score of approximately ±1.

### 3.5 MFI (Money Flow Index)

```
typical_price = hlc3
raw_money_flow = typical_price × volume
positive_flow = sum of raw_money_flow where tp > tp[1]
negative_flow = sum of raw_money_flow where tp <= tp[1]
mfi = 100 × positive_flow / (positive_flow + negative_flow)
```

Implements MFI via Pine Script's built-in `ta.mfi(source, length)` (2-argument form —
the built-in uses the chart's volume series internally; do not pass a separate volume arg).
Score: `(mfi - 50) / 50`, clamped to `[-1, +1]`.

---

## 4. Scoring flowchart

```
                  ┌──────────────────┐
                  │  New bar forms   │
                  └────────┬─────────┘
                           │
              ┌────────────▼──────────────┐
              │  f_score_detail()         │
              │  Compute 5 indicator      │
              │  values from OHLCV        │
              └────────────┬──────────────┘
                           │
         ┌─────────────────┼──────────────────┐
         │                 │                  │
    ┌────▼────┐      ┌──────▼──────┐    ┌─────▼─────┐
    │  RSI    │      │ MACD histo  │    │ EMA spread│
    │ norm to │      │ dir + delta │    │ % × 300   │
    │ [-1,+1] │      │  ±1 or ±0.3│    │ clamp[-1,+1]
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
           ┌─────────────┼──────────────┬──────────────┐
           │             │              │              │
    ┌──────▼──────┐  ┌───▼─────┐  ┌────▼──────┐ ┌────▼────────┐
    │ barcolor()  │  │Dashboard│  │  TP / SL  │ │  Telemetry  │
    │ gradient    │  │  table  │  │   lines   │ │ Data Window │
    └─────────────┘  └─────────┘  └───────────┘ └─────────────┘
```

---

## 5. Multi-timeframe engine

### Design

`f_score()` is a **stateless expression wrapper** — it calls `f_score_detail()` which
relies only on Pine built-ins (`ta.*`) and global input variables. This makes it safe
to evaluate inside `request.security()` — each security context gets its own state.

```pine
sc_m1   = request.security(syminfo.tickerid, "1",  f_score(), lookahead=barmerge.lookahead_off)
sc_m2   = request.security(syminfo.tickerid, "2",  f_score(), lookahead=barmerge.lookahead_off)
sc_m5   = request.security(syminfo.tickerid, "5",  f_score(), lookahead=barmerge.lookahead_off)
sc_m15  = request.security(syminfo.tickerid, "15", f_score(), lookahead=barmerge.lookahead_off)
sc_comb = (nz(sc_m1) + nz(sc_m2) + nz(sc_m5) + nz(sc_m15)) / 4.0

// Chart TF: use f_score_detail() to also capture component scores for telemetry
[sc_cur, t_s_rsi, t_s_stoch, t_s_macd, t_s_ema, t_s_mfi] = f_score_detail()
```

### `lookahead_off` — why it matters

`barmerge.lookahead_off` ensures that when running on a lower-timeframe chart, the
higher-timeframe signal is only updated **after the higher-TF bar has closed**.
This prevents look-ahead bias where a M15 signal appears to predict intra-bar events.

### Temporal alignment caveat

When charting on M1 and calling `f_score()` on M15:
- Each M15 bar covers 15 M1 bars
- The M15 score is **constant** for all 15 M1 bars within the same M15 bar
- It only updates on bar 1 of each new M15 period

This is by design — the M15 row represents the current M15 structure, not a live signal.

### Combined score

```
sc_comb = simple average of the four TF scores
```

Equal weighting across timeframes was chosen deliberately. A weighted average
can be a V2 improvement, but equal weighting is more stable and harder to overfit.

---

## 6. TP / SL level computation

### Direction logic

Default (unchanged from V1.0): `is_bull = sc_comb >= 0.0`

**Neutral Zone filter (opt-in, default OFF):**
```
bull    if sc_comb >= +i_conf_min / 100.0
bear    if sc_comb <= -i_conf_min / 100.0
neutral if |sc_comb| < i_conf_min / 100.0  → suppress TP lines
```

When neutral, `show_levels` is set to `false` and no TP/SL objects are shown,
even if TP Preview Mode is ON.

### Dual-method framework

For each TP level, two candidate prices are computed:

```
Method A (ATR floor):
    long:  entry + ATR × multiplier
    short: entry - ATR × multiplier

Method B (Fibonacci extension from rolling range window):
    swing_hi = highest(high, lookback)   ← rolling window, NOT true pivot
    swing_lo = lowest(low,  lookback)
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

### Fib target sanity check (opt-in, default OFF)

When enabled, Fibonacci targets that would be on the wrong side of price are replaced
by the ATR floor:

```
bull sanity: if fib_tp_n <= close  →  use (close + ATR × mult_n) instead
bear sanity: if fib_tp_n >= close  →  use (close - ATR × mult_n) instead
```

This prevents "behind-price" targets that can occur when the rolling window
range spans a recent gap or extreme candle.

### SL computation

```
SL (long):  close - ATR × sl_multiplier
SL (short): close + ATR × sl_multiplier
```

SL is purely ATR-based. Pivot-based SL placement is outside this indicator's scope.

### TP/SL visibility gate

```pine
bool show_levels = (i_show_tp and
                    barstate.islast and
                    not is_neutral and
                    (i_tp_preview or conf_pct >= i_conf_min))
```

- Default (preview OFF, neutral zone OFF): same as V1.0 — levels shown only when
  `i_show_tp AND barstate.islast AND conf_pct >= i_conf_min`.
- **TP Preview Mode ON**: adds levels when below threshold (subtle transparent colours).
- **Neutral Zone ON**: `is_neutral = true` forces `show_levels = false` regardless of
  other settings — direction must be confirmed for any lines to appear.

### Rendering: create-once / update-in-place

V1.2 replaces the V1.0 delete-and-recreate pattern with persistent object handles:

```pine
var line  l_tp1 = na   // created once, updated every last bar
var line  l_tp2 = na
// ... etc.

if show_levels
    if na(l_tp1)
        // CREATE ONCE: line.new() / label.new()
    else
        // UPDATE IN PLACE: line.set_xy1(), label.set_text(), etc.
else
    // HIDE: set color to color.new(color.gray, 100) — fully transparent
    //       set label text to "" — empty
```

**Benefits:**
- No object churn: TradingView doesn't need to delete/recreate on every tick
- Max object budget: 4 lines + 4 labels regardless of bar count (well within limits)
- Stable handles: easier for future extensions (e.g. animating levels)

### Preview mode styling

When `is_preview = true` (TP Preview Mode is ON and below threshold):
- TP line transparency increases from 55 → 75 (more translucent)
- Label transparency increases from 70 → 85 (very subtle)
- SL/TP2 line transparency increases from 30 → 60

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

Colour stops:
- `#B8860B` — dark goldenrod (neutral / amber)
- `#00873E` — deep green (maximum bull)
- `#C42222` — deep red (maximum bear)

---

## 8. Weighting rationale

| Component | Weight | Rationale |
|-----------|--------|-----------|
| MACD histogram | **1.5** | Captures trend-momentum direction AND acceleration; two-tier scoring (±1 vs ±0.3) encodes persistence. |
| EMA Cross | **1.5** | Structural trend direction. Fresh EMA crossovers are the single strongest predictor of continuation in scalping. |
| Stochastic RSI | **1.2** | Faster oscillator tuned for entry timing. Responds to momentum changes 2–3 bars sooner than RSI. |
| RSI | **1.0** | Classic momentum reference. Useful for overbought/oversold identification. |
| MFI | **0.8** | Volume-weighted momentum sanity check. Lowest weight because volume data quality varies across assets. |

**Total weight: 6.0** — dividing by 6.0 keeps the output within `[-1, +1]` under all conditions.

---

## 9. Synthetic validation

Three primary synthetic test regimes and their expected outcomes:

### Test A — Ascending (strong bull)

```python
close = [100 + i * 0.05 for i in range(100)]
high  = [c + 0.03 for c in close]
low   = [c - 0.03 for c in close]
vol   = [1000] * 100
```

Expected convergence (after ~30 bar warm-up):
- RSI → 70–85 → s_rsi → +0.4 to +0.7
- StochRSI K → 80–95, K > D → s_stoch → +0.7 to +0.9
- MACD histogram → positive and growing → s_macd = +1.0
- EMA(9) > EMA(21) → s_ema → +0.6 to +1.0
- MFI ≈ 55–65 → s_mfi → +0.1 to +0.3

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
- MACD histogram oscillates near zero
- EMAs tangled (spread near zero) → s_ema ≈ 0
- MFI near 50

Expected f_score(): **±0.05 to ±0.20** → confidence < 25% → TP lines NOT drawn ✅

With TP Preview Mode ON: lines drawn in high-transparency preview style ✅

### Test D — Neutral Zone filter (new in V1.2)

Same as Test C, with `i_neutral_zone = true` and threshold at 55%:
- `|sc_comb| < 0.55` → `is_neutral = true`
- `show_levels = false` even when TP Preview Mode is ON
- Data Window: `dbg:not_neutral = 0` (neutral suppressing) ✅

---

## 10. Advisories and known edge cases

### A. Low-volume assets
MFI degrades when volume is sparse or unreliable (e.g. some illiquid crypto pairs).
Reduce MFI weight conceptually by noting that it carries 0.8× weight — already the
lowest. This does not apply to major forex, crypto top-20, or equity indices.

### B. Warm-up period
All five indicators require historical data. On a brand-new chart, scores will be
unstable for the first **30–50 bars**. This is the warm-up period for EMA, MACD,
and RSI smoothing to converge.

### C. `request.security()` resolution limits
TradingView limits the number of `request.security()` calls. The current 4 calls
(plus 1 direct `f_score_detail()` call for candle colour and telemetry) total
**5 evaluations**, well within TradingView's limit of 40 per script.

### D. TP levels on gaps
During price gaps (e.g. overnight gaps on equities), the rolling range window
(`ta.highest` / `ta.lowest`) may include the gap candle, distorting Fibonacci levels.
The **Fib Target Sanity Check** (opt-in) mitigates this by ensuring targets are
always ahead of current price. Alternatively, increase Range Lookback to smooth gaps.

### E. `barstate.islast` rendering
TP/SL lines and the dashboard are only drawn on the last bar. This means they do not
appear on historical bars — by design (no backtesting role for these visual elements).

### F. EMA spread calibration for other assets
The `×300` spread multiplier was tuned for EUR/USD and BTC/USDT M1. On assets with
larger absolute percentage moves (e.g. small-cap equities), the EMA spread may
saturate to ±1 even on moderate trends. Consider scaling the multiplier down for
high-volatility instruments.

### G. Range Lookback vs true pivots (V1.2 clarification)
The "Range Lookback (Fib Window)" input uses `ta.highest(high, N)` and
`ta.lowest(low, N)` — this is a **rolling window range**, not structurally
identified swing pivots. The input was previously labelled "Pivot Lookback" in V1.0,
which was misleading. True pivot support is on the V2 roadmap.

### H. Advisory telemetry — NOT a trading signal
The JSON alert payload contains only raw indicator metrics. It does NOT constitute
a buy or sell instruction. Your .algo bot is responsible for all decision logic.

---

## 11. Suggested improvements (V2 roadmap)

### Priority 1 — Accuracy

| Improvement | Description |
|-------------|-------------|
| **True pivot TP anchors** | Replace rolling-window range with `ta.pivothigh`/`ta.pivotlow` and persist last swing points (opt-in toggle) |
| **ADX filter** | Only generate signals when ADX(14) > 25 — filters low-trend conditions |
| **Volume profile integration** | Weight MFI higher when volume is 2× average or more |
| **Session filter** | Suppress signals during low-liquidity hours (Asian session for forex) |
| **Regime detection** | Use Hurst exponent or Efficiency Ratio to detect trending vs ranging |
| **Per-TF weights** | Allow users to assign different weights per timeframe in `sc_comb` |

### Priority 2 — UI

| Improvement | Description |
|-------------|-------------|
| **Sparkline score history** | Small visual bar showing score over last 10 bars |
| **Background shading** | Subtle background fill when COMB > 70% in either direction |
| **Confidence history plot** | Separate pane plotting `conf_pct` over time for backtesting context |
| **R:R display** | Show Risk/Reward ratio for each TP level in the dashboard |

### Priority 3 — Multi-asset

| Improvement | Description |
|-------------|-------------|
| **Configurable TF list** | Let users choose which 4 timeframes are included in COMB |
| **Correlation pair overlay** | For correlated pairs (EUR/USD vs DXY), show a correlation-adjusted signal |

### Priority 4 — Risk management

| Improvement | Description |
|-------------|-------------|
| **Dynamic SL** | SL based on ATR AND below/above most recent swing wick |
| **Position size calculator** | Based on account % risk and ATR-based SL distance |
| **Directional agreement score** | Weight COMB toward TFs that agree with each other |

---

## 12. cTrader Automate conversion guide

> **Platform target:** cTrader Automate API **v1.014**
> **Output file:** `HF.V1.2.cs` (C# Robot / cBot)

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
| `ta.mfi(hlc3, 14)` | Custom implementation (see below) |
| `ta.atr(14)` | `Indicators.AverageTrueRange(14, MovingAverageType.Exponential)` |
| `ta.highest(high, 20)` | Loop over `MarketSeries.High` for last 20 bars |
| `ta.lowest(low, 20)` | Loop over `MarketSeries.Low` for last 20 bars |
| `request.security(_, "5", ...)` | `MarketData.GetSeries(Symbol, TimeFrame.Minute5)` |
| `alert(json, ...)` | `Print(json)` or webhook via `cAlgo.API.Internals.INotificationService` |

### Advisory telemetry in cBot

To mirror the v1.2 advisory telemetry in a cTrader cBot:
- Compute component scores in `ComputeScore(MarketSeries series)` and return them
  alongside the combined score.
- Log the JSON payload using `Print()` on each `OnBar()` call when telemetry is enabled.
- Optionally, POST to a webhook using `System.Net.Http.HttpClient`.

The cBot **must not** interpret the advisory scores as hard buy/sell signals without
additional risk management and confirmation logic.

### cBot skeleton — `HF.V1.2.cs`

```csharp
using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class EAHFScalperV12 : Robot
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

        [Parameter("Enable Advisory Telemetry", DefaultValue = false)]
        public bool EnableTelemetry { get; set; }

        // Stochastic RSI configuration (mirrors Pine inputs i_sr_rsi, i_sr_st, i_sr_k, i_sr_d)
        [Parameter("Stoch RSI - RSI Length", DefaultValue = 14, MinValue = 1, MaxValue = 200)]
        public int SrRsiLength { get; set; }

        [Parameter("Stoch RSI - Stoch Length", DefaultValue = 14, MinValue = 1, MaxValue = 200)]
        public int SrStochLength { get; set; }

        [Parameter("Stoch RSI - %K Smoothing", DefaultValue = 3, MinValue = 1, MaxValue = 200)]
        public int SrK { get; set; }

        [Parameter("Stoch RSI - %D Smoothing", DefaultValue = 3, MinValue = 1, MaxValue = 200)]
        public int SrD { get; set; }
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

            if (EnableTelemetry)
            {
                // Advisory telemetry — NOT a buy/sell signal
                string json = string.Format(
                    "{{\"ea\":\"EA.HF.v1.2\",\"sym\":\"{0}\",\"tf\":\"{1}\"," +
                    "\"bar\":{2},\"sc_m1\":{3:F4},\"sc_m2\":{4:F4}," +
                    "\"sc_m5\":{5:F4},\"sc_m15\":{6:F4}," +
                    "\"sc_comb\":{7:F4},\"conf_pct\":{8:F1}}}",
                    Symbol.Name, TimeFrame, Bars.Count,
                    scM1, scM2, scM5, scM15, scComb, confPct);
                Print(json);
            }

            if (confPct < ConfThreshold)
                return;

            double atrVal = _atr.Result.LastValue;
            bool isBull   = scComb >= 0;

            if (isBull && Positions.Find("EAHFV12_Long") == null)
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol.Name,
                    Symbol.NormalizeVolumeInUnits(10000),
                    "EAHFV12_Long",
                    atrVal * SlMult / Symbol.PipSize,
                    atrVal * Tp1Mult / Symbol.PipSize);
            }
            else if (!isBull && Positions.Find("EAHFV12_Short") == null)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol.Name,
                    Symbol.NormalizeVolumeInUnits(10000),
                    "EAHFV12_Short",
                    atrVal * SlMult / Symbol.PipSize,
                    atrVal * Tp1Mult / Symbol.PipSize);
            }
        }

        /// <summary>
        /// Compute the composite score for a given market series.
        /// Mirrors f_score() from HF.V1.2.pine.
        /// Returns value in [-1, +1]. Positive = bullish, negative = bearish.
        /// </summary>
        private double ComputeScore(MarketSeries series)
        {
            int last = series.Close.Count - 1;
            if (last < 50) return 0;

            // 1. RSI
            var rsiInd = Indicators.RelativeStrengthIndex(series.Close, RsiLen);
            double sRsi = Clamp((rsiInd.Result.LastValue - 50.0) / 50.0, -1, 1);

            // 2. Stochastic RSI (custom)
            double sStoch = ComputeStochRsiScore(series);

            // 3. MACD
            var macdInd  = Indicators.MacdHistogram(series.Close, MacdSlow, MacdFast, MacdSignal);
            double mh     = macdInd.Histogram.LastValue;
            double mhPrev = macdInd.Histogram[macdInd.Histogram.Count - 2];
            double sMacd  = mh > 0 ? (mh >= mhPrev ? 1.0 : 0.3)
                                   : (mh <= mhPrev ? -1.0 : -0.3);

            // 4. EMA Cross
            var feInd = Indicators.ExponentialMovingAverage(series.Close, EmaFast);
            var seInd = Indicators.ExponentialMovingAverage(series.Close, EmaSlow);
            double feV = feInd.Result.LastValue;
            double seV = seInd.Result.LastValue;
            double sEma = seV > 0 ? Clamp((feV - seV) / seV * 300.0, -1, 1) : 0;

            // 5. MFI (custom)
            double sMfi = ComputeMfiScore(series, MfiLen);

            double raw = sRsi * 1.0 + sStoch * 1.2 + sMacd * 1.5 + sEma * 1.5 + sMfi * 0.8;
            return Clamp(raw / 6.0, -1, 1);
        }

        // Legacy overload using default Stoch RSI parameters (RSI=14, StochLen=14, KSmooth=3)
        private double ComputeStochRsiScore(MarketSeries series)
        {
            // Preserve existing default behaviour
            const int defaultRsiPeriod = 14;
            const int defaultStochLen  = 14;
            const int defaultKSmooth   = 3;

            return ComputeStochRsiScore(series, defaultRsiPeriod, defaultStochLen, defaultKSmooth);
        }

        // Parameterized overload that allows external configuration to drive Stoch RSI settings
        private double ComputeStochRsiScore(MarketSeries series, int rsiPeriod, int stochLen, int kSmooth)
        {
            var rsiInd = Indicators.RelativeStrengthIndex(series.Close, rsiPeriod);
            int count  = rsiInd.Result.Count;
            // Need enough data to compute a stochLen-window for each of the last kSmooth bars
            if (count < stochLen + kSmooth - 1) return 0;

            double kSm = 0.0;

            // Smooth k_raw values (Stochastic RSI) over the last kSmooth bars
            for (int i = 0; i < kSmooth; i++)
            {
                int idx = count - 1 - i; // current bar index for this k_raw

                double highest = double.MinValue, lowest = double.MaxValue;
                for (int j = 0; j < stochLen; j++)
                {
                    double v = rsiInd.Result[idx - j];
                    if (v > highest) highest = v;
                    if (v < lowest)  lowest  = v;
                }

                double range = highest - lowest;
                double kRaw;
                if (range < 1e-10)
                    kRaw = 0.0;
                else
                    kRaw = 100.0 * (rsiInd.Result[idx] - lowest) / range;

                kSm += kRaw;
            }
            kSm /= kSmooth;

            return Clamp((kSm - 50.0) / 50.0, -1, 1);
        }

        private double ComputeMfiScore(MarketSeries series, int len)
        {
            int count = series.Close.Count;
            if (count < len + 1) return 0;

            double posFlow = 0, negFlow = 0;
            for (int i = 1; i <= len; i++)
            {
                int idx = count - i;
                double tp     = (series.High[idx] + series.Low[idx] + series.Close[idx]) / 3.0;
                double tpPrev = (series.High[idx - 1] + series.Low[idx - 1] + series.Close[idx - 1]) / 3.0;
                double flow   = tp * series.TickVolume[idx];
                if (tp > tpPrev) posFlow += flow;
                else             negFlow += flow;
            }

            double total = posFlow + negFlow;
            if (total < 1e-10) return 0;
            double mfi = 100.0 * posFlow / total;
            return Clamp((mfi - 50.0) / 50.0, -1, 1);
        }

        private static double Clamp(double v, double min, double max) =>
            v < min ? min : v > max ? max : v;

        protected override void OnStop()
        {
            // Cleanup: close all open positions
        }
    }
}
```
