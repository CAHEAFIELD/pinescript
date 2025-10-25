# TR: True Range

[Pine Script Implementation of TR](https://github.com/mihakralj/pinescript/blob/main/indicators/volatility/tr.pine)

## Overview and Purpose

True Range (TR) is a single-bar volatility measurement that captures the full extent of price movement including gaps between trading sessions. Introduced by J. Welles Wilder Jr. in his seminal 1978 work "New Concepts in Technical Trading Systems," TR forms the foundation for many volatility-based indicators, most notably the Average True Range (ATR). Unlike simple high-low range, True Range accounts for overnight gaps and price discontinuities, providing a more complete picture of actual market volatility on each individual bar.

The True Range is calculated as the maximum of three values: the current high-low range, the absolute value of the current high minus the previous close, and the absolute value of the current low minus the previous close. This three-way comparison ensures that gaps up or down are properly reflected in the volatility measurement, making TR particularly valuable in markets prone to overnight or weekend gaps.

## Core Concepts

* **Gap-aware volatility** — captures price movement including overnight and weekend gaps
* **Maximum of three values** — ensures complete volatility measurement regardless of gap direction
* **Building block for indicators** — foundation for ATR, Keltner Channels, Chandelier Exit, and other volatility-based tools
* **Absolute price units** — measured in the same units as the underlying security's price
* **Single-bar calculation** — no smoothing or averaging, represents volatility of current bar only

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Source | HLC | Price data used (high, low, close) | Rarely changed; standard definition uses HLC |

**Pro Tip:** TR itself is rarely used alone for trading signals. Instead, it serves as the raw input for smoothed volatility indicators like ATR. However, examining individual TR spikes can identify unusually volatile sessions that may precede trend changes or indicate news-driven volatility.

## Calculation and Mathematical Foundation

**Simplified explanation:**  
True Range measures the largest price movement on the current bar, considering three scenarios: the regular high-low range, gaps up from the previous close, and gaps down from the previous close. The maximum of these three values represents the "true" range of price movement.

**Technical formula:**

```
TR = max(H - L, |H - C[1]|, |L - C[1]|)
```

Where:
- H = current high
- L = current low
- C[1] = previous close
- | | = absolute value

**Three scenarios captured:**

1. **Normal range** (H - L): When current trading stays within previous close range
2. **Gap up** (|H - C[1]|): When market opens above previous close, ensuring upward gap is measured
3. **Gap down** (|L - C[1]|): When market opens below previous close, ensuring downward gap is measured

> 🔍 **Technical Note:** The first bar uses nz(close[1], close) to handle the NA value for previous close, defaulting to current close. This ensures TR works correctly from bar 1 without producing NA values.

## Interpretation Details

**Absolute volatility measurement:**
- Higher TR values indicate greater price volatility on that specific bar
- Lower TR values suggest quieter, more contained price action
- TR spikes often correlate with important news events or technical breakouts

**Market context:**
- **Trending markets** — TR tends to expand as trends accelerate
- **Range-bound markets** — TR remains relatively stable and low
- **Breakout sessions** — TR spikes mark volatility expansion at trend initiation
- **Exhaustion moves** — Extremely high TR at trend extremes may signal reversal

**Comparative analysis:**
- Compare current TR to recent TR values to identify volatility regime changes
- TR substantially above recent average suggests increased uncertainty
- TR substantially below recent average suggests consolidation

**Gap handling:**
- Weekend gaps in forex or crypto may show minimal TR impact (24-hour markets)
- Stock market gaps between sessions are properly captured
- Futures markets with evening sessions benefit from gap-aware measurement

## Limitations and Considerations

* **Not normalized** — TR values differ across securities based on price level; $100 stock has larger TR than $10 stock
* **Single-bar measurement** — highly sensitive to individual volatile bars; no smoothing
* **No directional information** — TR measures magnitude of movement but not direction
* **Price-dependent** — comparing TR across different securities or time periods requires normalization (see NATR)
* **Requires smoothing** — most practical applications smooth TR (via ATR) before use in trading systems
* **Historical comparison** — TR values become less comparable as security price changes over time

**Complementary indicators:**
- ATR (smoothed version for practical use)
- NATR (normalized as percentage of close)
- Bollinger Bands (volatility envelope)
- ADX (trend strength incorporating TR)

## References

* Wilder, J. W. Jr. (1978). *New Concepts in Technical Trading Systems*. Trend Research.
* Kaufman, P. J. (2013). *Trading Systems and Methods* (5th ed.). Wiley.

## Validation Sources

**Patterns:** §31 (True Range Calculation)  
**External:** pandas-ta, TA-Lib, Tulip Indicators, QuantConnect  
**Formula verified:** Wilder 1978 definition TR = max(H-L, |H-C[1]|, |L-C[1]|)  
**API:** PineScript math.max, math.abs, nz() confirmed
