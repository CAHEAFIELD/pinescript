# PGO: Pretty Good Oscillator

[Pine Script Implementation of PGO](https://github.com/mihakralj/pinescript/blob/main/indicators/oscillators/pgo.pine)

## Overview and Purpose

The Pretty Good Oscillator (PGO), developed by Mark Johnson, is a momentum oscillator that measures the distance of the current closing price from its N-period simple moving average, normalized by the Average True Range (ATR). This normalization makes PGO uniquely valuable as it expresses price deviation in terms of volatility, making it comparable across different securities and time periods regardless of their absolute price levels or volatility characteristics.

PGO effectively answers the question: "How many ATRs is the current price away from its moving average?" This volatility-adjusted approach provides context-aware overbought/oversold readings that adapt to market conditions. During high volatility periods, larger price deviations are considered normal, while the same absolute price movement during low volatility would generate a more significant PGO reading.

The indicator is particularly useful for identifying price extremes and potential reversal points, as extreme PGO values indicate that price has moved an unusual distance from its average when measured against typical price fluctuations. This makes PGO especially effective for mean reversion strategies and for confirming momentum shifts when price crosses back through the zero line.

## Core Concepts

* **Volatility Normalization:** PGO divides price deviation by ATR, making readings comparable across different securities and market conditions. A PGO reading of +3 means price is 3 ATRs above its moving average, regardless of whether the security trades at $10 or $1000.

* **Mean Reversion Signal:** Extreme PGO values (typically beyond ±3) suggest price has deviated significantly from its average and may be due for a retracement. The farther PGO moves from zero, the stronger the potential mean reversion setup.

* **Momentum Confirmation:** Zero-line crosses indicate momentum shifts as price moves from one side of its moving average to the other. Crossovers above zero suggest bullish momentum, while crossovers below zero indicate bearish momentum.

* **Adaptive Thresholds:** Because PGO is normalized by volatility, the overbought/oversold zones automatically adapt to changing market conditions without requiring manual parameter adjustments.

* **True Range Foundation:** Using ATR (based on True Range) ensures gap movements are properly accounted for, making PGO reliable even in markets that gap frequently between trading sessions.

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Period | 14 | Number of bars for SMA and ATR calculation | Increase for longer-term trends (20-30), decrease for faster signals (7-10). Match your trading timeframe. |
| Source | close | Price data to analyze | Use HL2 or HLC3 for smoother readings, or high/low for more aggressive signals at extremes. |

**Pro Tip:** The default 14-period setting works well for most timeframes and provides a good balance between responsiveness and noise reduction. For swing trading, consider 20 periods to align with typical 4-week cycles. For day trading, 10 periods may provide timelier signals. The key advantage of PGO over fixed-threshold oscillators is that you rarely need to adjust the ±3 thresholds even when changing periods, as the volatility normalization handles this automatically.

## Calculation and Mathematical Foundation

**Explanation:**
PGO measures how far the current price has moved from its average, expressed in units of typical price movement (ATR). First, calculate the deviation between current close and its simple moving average. Then, normalize this deviation by dividing by the exponentially smoothed Average True Range. The result indicates whether price is at a normal distance from its average (PGO near 0), extended above average (positive PGO), or extended below average (negative PGO).

**Technical formula:**

```
Step 1: Calculate True Range
TR = max(High - Low, |High - Close[1]|, |Low - Close[1]|)

Step 2: Calculate Simple Moving Average of Close
SMA = sum(Close, Period) / Period

Step 3: Calculate Average True Range (RMA of TR)
ATR = RMA(TR, Period)

Step 4: Calculate Pretty Good Oscillator
PGO = (Close - SMA) / ATR
```

> 🔍 **Technical Note:** This implementation uses RMA (Wilder's smoothing) for ATR calculation with exponential warmup compensation, ensuring valid output from bar 1. The first bar's True Range calculation handles the absence of a previous close by using only the high-low range, preventing artificial spikes. RMA uses alpha = 1/period (slower than EMA's alpha = 2/(period+1)) and includes the same exponential compensator as EMA (§2 pattern) since RMA is also an IIR filter. The SMA component uses count-based warmup (§3 pattern) with a circular buffer for O(1) performance. All NA values are explicitly checked before division to prevent calculation errors.

## Interpretation Details

**Primary Use Case: Overbought/Oversold Identification**

- **PGO > +3.0:** Price is more than 3 ATRs above its moving average, indicating an overbought condition. While strong uptrends can maintain elevated PGO readings, values above +3 suggest price has extended significantly and may be vulnerable to pullbacks.

- **PGO < -3.0:** Price is more than 3 ATRs below its moving average, indicating an oversold condition. This suggests price has declined substantially relative to typical volatility and may be approaching a support level or mean reversion opportunity.

- **PGO between -3 and +3:** Normal range where price is within typical volatility bounds of its moving average. Most price action occurs in this range, with no extreme condition present.

**Secondary Use Case: Momentum and Trend Direction**

- **Zero-Line Crosses:** When PGO crosses above zero, price has moved above its moving average, confirming bullish momentum. Crosses below zero indicate price has fallen below its average, confirming bearish momentum. These crosses often align with the early stages of trend changes.

- **Positive PGO Territory:** When PGO remains consistently above zero, it confirms an uptrend with price trading above its average. The farther above zero, the stronger the upward momentum.

- **Negative PGO Territory:** Sustained negative readings confirm a downtrend with price trading below its average. Increasingly negative values suggest strengthening downward momentum.

**Advanced Applications:**

- **Divergences:** Bullish divergence occurs when price makes lower lows but PGO makes higher lows, suggesting weakening downward pressure. Bearish divergence (price makes higher highs, PGO makes lower highs) warns of weakening upward momentum.

- **Extreme Reading Clusters:** Multiple consecutive readings beyond ±3 indicate exceptionally strong trends. While these are mean reversion opportunities, they also signal powerful momentum that may continue longer than expected.

- **Volatility Context:** During low volatility periods, PGO may reach ±3 with smaller absolute price moves. During high volatility, larger price swings may still produce moderate PGO readings. This adaptive quality is PGO's key strength.

## Limitations and Considerations

- **Lagging Nature:** PGO uses a simple moving average as its baseline, which lags price action. During strong trends, PGO may stay at extreme levels for extended periods before price actually reverses, leading to early entry signals in powerful trends.

- **Period Sensitivity:** While the ±3 thresholds remain relatively stable, the period setting significantly affects how quickly PGO responds to price changes. Shorter periods generate more frequent signals but increase whipsaw risk. Longer periods reduce noise but may miss shorter-term opportunities.

- **Sideways Markets:** In ranging, choppy markets with no clear trend, PGO oscillates around zero frequently, generating numerous zero-line crosses that may not indicate meaningful momentum shifts. The oscillator works best when price shows clear directional movement.

- **Extreme Trend Extensions:** In parabolic or climactic price moves, PGO can reach readings beyond ±5 or even ±10. These extreme readings don't necessarily signal immediate reversals; they confirm exceptional price extension that requires careful risk management.

- **Volatility Changes:** Sudden volatility spikes (like during news events) can cause ATR to expand rapidly, temporarily compressing PGO readings. Conversely, volatility contractions can inflate PGO readings without corresponding price extremes. Allow a few periods for ATR to stabilize after significant volatility shifts.

- **Not a Standalone System:** PGO should be used in conjunction with other analysis methods such as support/resistance levels, volume analysis, or trend indicators. Extreme PGO readings identify potential opportunity zones but don't guarantee reversals without confirming signals.

## References

* Johnson, M. (1994). "The Pretty Good Oscillator." Technical Analysis of Stocks & Commodities Magazine.
* Achelis, S. B. (2001). Technical Analysis from A to Z. McGraw-Hill.
* Trading Technologies. "Pretty Good Oscillator (PGO) - Technical Indicators."
* Pring, M. J. (2002). Technical Analysis Explained. McGraw-Hill.

## Validation Sources

**Patterns:** §1 (Circular Buffer for SMA), §2 (Exponential Warmup for RMA), §3 (Count-Based Warmup for SMA), §7 (NA handling), §10 (Wilder's Smoothing for RMA), §31 (True Range calculation)
**Wolfram:** Server unavailable during validation  
**External:** Trading Technologies PGO documentation, TechnicalResources.in PGO specification, GoCharting PGO guide, Pineify PGO implementation  
**API:** ref-tools verified input.int, input.source signatures, self-contained SMA/RMA implementation, defensive division pattern  
**Planning:** phases=research,formula_validation,implementation,documentation  
**Implementation:** No .ta namespace dependencies - all calculations self-contained
