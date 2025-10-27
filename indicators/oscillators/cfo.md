# OSCILLATOR: Chande Forecast Oscillator (CFO)

[Pine Script Implementation of CFO](https://github.com/mihakralj/pinescript/blob/main/indicators/oscillators/cfo.pine)

## Overview and Purpose

The Chande Forecast Oscillator (CFO) is a momentum-based technical indicator developed by Dr. Tushar Chande that measures the percentage difference between an asset's actual closing price and its forecasted price. The forecast is generated using linear regression analysis, specifically a Time Series Forecast (TSF) that projects the best-fit line one period into the future. By quantifying how much the actual price deviates from the predicted price, the CFO provides insights into momentum strength and potential trend continuations or reversals.

Unlike traditional oscillators that compare current price to historical prices, the CFO compares current price to what the price was expected to be based on recent trend analysis. This forward-looking approach makes it particularly sensitive to changes in momentum. When prices consistently exceed their forecasts, it indicates strong bullish momentum; when prices consistently fall short of forecasts, it signals bearish momentum or weakness.

The indicator oscillates around a zero line, with positive values indicating prices above forecast (bullish surprise) and negative values indicating prices below forecast (bearish surprise). The magnitude of the oscillator value reflects the strength of the deviation, making it useful for identifying overbought/oversold conditions and potential trend reversals.

## Core Concepts

* **Time Series Forecast (TSF):** A linear regression-based prediction that projects the best-fit trend line one bar into the future, representing the expected price based on recent price action over the specified period.

* **Forecast Error Measurement:** The oscillator quantifies prediction accuracy by measuring the percentage deviation of actual price from the forecasted price, with the formula CFO = 100 × (Price - TSF[1]) / Price.

* **Momentum Indication:** Positive oscillator values signal that price is exceeding expectations (bullish momentum), while negative values indicate price is underperforming expectations (bearish momentum or weakness).

* **Zero-Line Crossovers:** Transitions from negative to positive territory suggest strengthening momentum and potential trend continuation, while moves from positive to negative indicate weakening momentum and possible trend exhaustion.

* **Divergence Detection:** When the oscillator makes higher lows while price makes lower lows (bullish divergence), or lower highs while price makes higher highs (bearish divergence), it can signal impending trend reversals.

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Period | 14 | Number of bars used for linear regression calculation and TSF generation | Shorter periods (7-10) increase sensitivity for day trading; longer periods (20-30) provide smoother signals for swing trading |
| Source | close | Price data used for calculations | Change to hlc3 or ohlc4 for less volatility, or typical price for volume-weighted analysis |

**Pro Tip:** The standard 14-period setting balances responsiveness with reliability, but consider market context. In fast-moving markets or lower timeframes, reduce the period to 7-10 for quicker signals. For position trading on daily charts, extend to 20-30 periods to filter out noise and focus on significant momentum shifts.

## Calculation and Mathematical Foundation

**Explanation:**
The Chande Forecast Oscillator uses linear regression to predict the next period's price, then measures how the actual current price compares to the previous period's prediction. The calculation involves three main steps: computing a linear regression trend line over N periods, projecting that line one period forward to generate the Time Series Forecast (TSF), and calculating the percentage difference between the actual price and yesterday's forecast.

**Technical formula:**

```
Step 1: Linear Regression Components
sumX = Σ(i) for i from 0 to N-1 = N(N-1)/2
sumX² = Σ(i²) for i from 0 to N-1 = N(N-1)(2N-1)/6
sumY = Σ(price[i]) for i from 0 to N-1
sumXY = Σ(i × price[i]) for i from 0 to N-1

Step 2: Regression Line Parameters
slope = (N × sumXY - sumX × sumY) / (N × sumX² - sumX²)
intercept = (sumY - slope × sumX) / N

Step 3: Time Series Forecast (one period ahead)
TSF[t] = intercept + slope × (N-1)

Step 4: Chande Forecast Oscillator
CFO[t] = 100 × (Price[t] - TSF[t]) / Price[t]
```

> 🔍 **Technical Note:** The implementation achieves O(1) computational complexity through incremental maintenance of sumXY. During warmup (filling phase), sumXY is updated via `sumXY += count × y_new` where count is the current fill index (0-based). In the sliding window phase, sumXY is updated using `sumXY' = sumXY - sumY + (N-1) × y_new`, eliminating the need to loop through the buffer. The indicator returns `na` during warmup (until N bars accumulated) to match standard Time Series Forecast behavior. NA values in the source are properly gated and skipped rather than converted to zeros, preventing bias. Division by zero is handled by returning `na` when Price equals zero.

## Interpretation Details

### Primary Use: Momentum Confirmation

- **Positive Values (0 to +100):** Price exceeded the forecast, indicating bullish momentum and trend strength. The larger the positive value, the stronger the upward momentum.
- **Negative Values (0 to -100):** Price fell short of the forecast, indicating bearish momentum or trend weakness. Larger negative values suggest stronger downward pressure.
- **Near Zero:** Price is tracking the forecast closely, indicating balanced momentum or consolidation. This often occurs during sideways markets or tight ranges.

### Secondary Use: Overbought/Oversold Identification

- **Overbought Zone (>+10 to +15):** Extended positive readings suggest price has significantly exceeded forecasts, potentially indicating overextension and increased probability of mean reversion or pullback.
- **Oversold Zone (<-10 to -15):** Prolonged negative readings indicate sustained underperformance versus forecasts, suggesting potential oversold conditions and bounce opportunities.
- **Threshold Guidelines:** Unlike bounded oscillators (RSI, Stochastic), CFO has no fixed upper/lower limits. Extreme values are relative to the specific asset and timeframe being analyzed.

### Signal Generation and Trading Applications

- **Zero-Line Crossovers:**
  - Bullish Signal: CFO crosses above zero indicates price beginning to exceed forecasts, suggesting strengthening upward momentum
  - Bearish Signal: CFO crosses below zero indicates price failing to meet forecasts, suggesting weakening or reversing momentum

- **Divergence Analysis:**
  - Bullish Divergence: Price makes lower lows while CFO makes higher lows—forecast errors are decreasing, suggesting selling pressure exhaustion
  - Bearish Divergence: Price makes higher highs while CFO makes lower highs—forecast errors are increasing, suggesting buying momentum waning

- **Trend Confirmation:** When CFO and price move in the same direction, it confirms trend strength. For example, rising prices with rising CFO values validate uptrend continuation.

### Advanced Interpretation

- **Persistence of Forecast Error:** Consecutive periods of positive or negative CFO values indicate sustained momentum trends. When CFO remains consistently positive for 5+ bars, it suggests a strong trending move.
- **Forecast Error Volatility:** Rapid swings in CFO values (e.g., from +15 to -15 within 3-5 bars) indicate unstable momentum and choppy market conditions unsuitable for trend-following strategies.
- **Magnitude Changes:** Watch for decreasing absolute values—if CFO moves from +20 to +5, momentum is weakening even though still positive.

## Limitations and Considerations

- **Linear Assumption Limitation:** The indicator assumes price movement follows a linear trend, making it less effective during highly volatile or non-linear price action. Sudden gaps, news events, or regime changes can produce misleading signals.

- **Lagging Component:** Despite measuring forecast error, the linear regression calculation inherently uses past data, introducing lag. In rapidly changing markets, the TSF may not accurately predict near-term price movements.

- **No Fixed Boundaries:** Unlike RSI (0-100) or Stochastic (0-100), CFO has no predetermined overbought/oversold levels. Traders must establish context-specific thresholds based on historical behavior of the specific asset and timeframe.

- **Whipsaw Risk in Ranging Markets:** During sideways consolidation, CFO generates frequent zero-line crossovers that produce false signals. The indicator performs best in clearly trending markets where linear regression assumptions hold.

- **Period Sensitivity:** The choice of period significantly impacts signal quality. Short periods (5-10) generate numerous signals with high noise, while long periods (25+) may miss timely entries due to excessive smoothing.

- **First Bar Initialization:** The indicator returns 0 on the first bar and builds up validity over the warmup period. Signals within the first N bars (where N = period) may be less reliable due to insufficient data for robust linear regression.

## References

* Chande, T. S., & Kroll, S. (1994). *The New Technical Trader*. John Wiley & Sons.
* Chande, T. S. (1997). *Beyond Technical Analysis: How to Develop and Implement a Winning Trading System*. John Wiley & Sons.
* Murphy, J. J. (1999). *Technical Analysis of the Financial Markets*. New York Institute of Finance.
* WallStreet.io Resources. Chande Forecast Oscillator Technical Documentation.
* TradingTechnologies. Chande Forecast Oscillator (CFO) Charts Help and Tutorials.

## Validation Sources

**Patterns:** §3 (count_warmup), §6 (defensive_division), §7 (na_handling), §12 (input_validation), §13 (constant_precomputation), §16 (embedded_functions), §26 (linear_regression)

**External Sources:** TradingTechnologies CFO documentation, WallStreet.io CFO definition, LightningChart CFO trading strategies, StockManiacs CFO formula guide, TradingStrategy.ai pandas_ta.momentum.cfo reference

**Formula Verification:** Linear regression normal equations confirmed: slope = (N×Σ(xy) - Σx×Σy)/(N×Σ(x²) - (Σx)²), TSF projection validated as intercept + slope × N

**API Verification:** PineScript input.int(minval, maxval), input.source(), plot(), hline() signatures confirmed

**Planning:** Sequential thinking phases: pattern retrieval → mathematical validation → external cross-reference → implementation → documentation
