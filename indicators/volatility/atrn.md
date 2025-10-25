# ATRN: Average True Range Normalized

[Pine Script Implementation of ATRN](https://github.com/mihakralj/pinescript/blob/main/indicators/volatility/atrn.pine)

## Overview and Purpose

Average True Range Normalized (ATRN) normalizes ATR to a [0,1] scale based on its own historical range, similar to applying a Stochastic oscillator to ATR values. Unlike NATR which expresses ATR as a percentage of close price, ATRN shows where current volatility stands relative to its recent minimum and maximum values. This creates a relative volatility indicator that identifies when current ATR is at historical extremes (near 0 or 1) versus middle ranges (near 0.5), making it particularly useful for detecting volatility breakouts and compressions within a single security's historical context.

ATRN uses a lookback window (default 10× the ATR period) to establish the historical range of ATR values, then normalizes the current ATR reading to show its position within that range. A value of 1.0 indicates ATR is at its highest level in the lookback period (maximum volatility expansion), while 0.0 indicates the lowest level (maximum volatility compression). This normalization makes ATRN especially valuable for volatility-based trading signals that trigger when volatility moves from one extreme to another.

## Core Concepts

* **Range-normalized volatility** — ATR scaled to [0,1] based on historical min/max of ATR itself
* **Relative volatility position** — shows where current volatility sits within recent range
* **Volatility breakout detection** — identifies when ATR moves from low to high within its own range
* **Single-security focus** — designed to compare current volatility to historical volatility of same security
* **Stochastic-like behavior** — similar to %K calculation but applied to ATR values

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Length | 14 | ATR smoothing period | Shorten for fast regimes; lengthen for stability |
| Source | HLC | Data for True Range | Rarely changed |
| Lookback | 10 × Length | Historical range window for normalization | Longer for broader context; shorter for recent volatility focus |

**Pro Tip:** Use ATRN crossing above 0.8 as a signal that volatility is expanding toward extreme levels, potentially indicating trend acceleration or breakout confirmation. ATRN dropping below 0.2 suggests volatility compression and potential consolidation.

## Calculation and Mathematical Foundation

**Simplified explanation:**  
First calculate ATR using Wilder's smoothing method. Then find the highest and lowest ATR values over the lookback period. Normalize current ATR to [0,1] scale: (ATR - min) / (max - min).

**Technical formula:**

```
Step 1: Calculate ATR
ATR = Wilder_RMA(TR, length)
where TR = max(H - L, |H - C[1]|, |L - C[1]|)

Step 2: Find historical range
max_ATR = highest(ATR, lookback_window)
min_ATR = lowest(ATR, lookback_window)
where lookback_window = 10 × length

Step 3: Normalize to [0,1]
ATRN = (ATR - min_ATR) / (max_ATR - min_ATR)
```

When max_ATR = min_ATR (flat volatility), ATRN returns 0.5 as a neutral value.

> 🔍 **Technical Note:** ATRN behaves like a Stochastic oscillator applied to ATR values rather than prices. This makes it mean-reverting - high ATRN readings tend to revert toward middle range, and low readings tend to expand. The lookback period determines sensitivity: shorter periods make ATRN more responsive but noisier.

## Interpretation Details

**ATRN levels:**
- **ATRN = 1.0** — ATR at maximum for lookback period; extreme volatility expansion
- **ATRN > 0.8** — High relative volatility; potential exhaustion or continuation
- **ATRN 0.4-0.6** — Normal volatility range; middle of historical context
- **ATRN < 0.2** — Low relative volatility; compression phase; potential breakout setup
- **ATRN = 0.0** — ATR at minimum for lookback period; extreme volatility compression

**Trading applications:**
- **Volatility breakouts** — ATRN moving from <0.2 to >0.5 signals expansion from compression
- **Trend exhaustion** — ATRN >0.9 combined with price extremes may indicate overextension
- **Consolidation detection** — Sustained ATRN <0.3 identifies range-bound conditions
- **Risk adjustment** — Increase position size when ATRN low; decrease when ATRN high

## Limitations and Considerations

* **Lag from smoothing** — inherits ATR's lag from Wilder smoothing
* **Lookback dependency** — normalization range changes with lookback period selection
* **Not cross-market comparable** — compares volatility within same security only; use NATR for cross-market comparison
* **Mean-reverting nature** — extreme ATRN readings tend to revert, unlike trending indicators
* **Price jumps** — corporate actions can create false extreme readings

**Critical difference from NATR:**
- **ATRN** = (ATR - min(ATR)) / (max(ATR) - min(ATR)) normalizes to [0,1] based on ATR's own historical range
- **NATR** = (ATR / close) × 100 expresses ATR as percentage of current price
- **Use ATRN** when you want to compare current volatility to historical volatility of the **same security**
- **Use NATR** when you want to compare volatility **across different securities** or price levels

**Complementary indicators:**
- NATR (for cross-market volatility comparison)
- ATR (for absolute volatility values)
- Bollinger Bands (volatility envelope)
- ADX (trend strength)

## References

* Wilder, J. W. Jr. (1978). *New Concepts in Technical Trading Systems*. Trend Research.
* Kaufman, P. J. (2013). *Trading Systems and Methods* (5th ed.). Wiley.
