# FORECAST: Time Series Forecast (TSF)

[Pine Script Implementation of TSF](https://github.com/mihakralj/pinescript/blob/main/indicators/forecasts/tsf.pine)

## Overview and Purpose

The Time Series Forecast (TSF) is a predictive technical indicator that uses linear regression analysis to project the continuation of a trend one period into the future. Unlike moving averages that smooth past prices, TSF extrapolates the best-fit line forward, providing a forward-looking estimate of where price is expected to move based on recent linear momentum. This makes it particularly useful for identifying trend direction and potential support/resistance levels ahead of price action.

TSF calculates a linear regression line over a specified period (typically 14 bars) and extends that line one bar forward. The regression line is fitted using the least squares method, which minimizes the sum of squared distances between actual prices and the fitted line. The forecasted value represents the mathematical continuation of the recent trend, assuming linear momentum persists into the next period.

The indicator is widely used as a dynamic trend line that adapts to changing market conditions. When price is above TSF, it suggests upward momentum; when below, it indicates downward momentum. TSF can also serve as a dynamic support/resistance level, with price often respecting the forecast line during trending markets. The indicator appears as an overlay line on the price chart, providing visual guidance for trend direction and potential entry/exit points.

## Core Concepts

* **Linear Regression Projection:** Uses least squares method to fit a straight line through recent price data, then extrapolates that line one period forward to generate the forecast value.

* **Trend-Following Nature:** TSF naturally follows trends because it projects the current momentum forward—rising prices produce rising forecasts, falling prices produce falling forecasts.

* **Dynamic Support/Resistance:** The forecast line often acts as a moving support level in uptrends and resistance in downtrends, with price bouncing off or respecting the TSF line.

* **Lead Time Advantage:** Unlike moving averages that lag price, TSF provides a one-period forward estimate, potentially offering earlier signals of trend continuation or reversal.

* **Slope Sensitivity:** The steepness of TSF reflects recent trend strength—steep angles indicate strong momentum, flat lines suggest consolidation or ranging markets.

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Period | 14 | Number of bars used for linear regression calculation | Shorter periods (7-10) increase responsiveness for day trading and fast markets; longer periods (20-30) provide smoother forecasts for swing trading |
| Forecast Periods | 1 | Number of periods to project ahead | Use 1 for immediate next-bar prediction; increase to 2-5 for longer-term projections; higher values extend the forecast further into the future |
| Source | close | Price data used for regression analysis | Change to hlc3 for typical price, ohlc4 for comprehensive averaging, or high/low for range-based forecasts |

**Pro Tip:** The 14-period default works well for most timeframes, but consider the volatility cycle of your asset. In highly volatile markets (crypto, small caps), reduce to 10 periods to capture rapid trend changes. For stable markets (large-cap stocks, indices), extend to 20-25 periods to filter noise and focus on significant trend movements. For the forecast parameter, 1 period ahead is most reliable; increasing to 3-5 periods can identify longer-term trend direction but with reduced accuracy.

## Calculation and Mathematical Foundation

**Explanation:**
TSF performs linear regression on the most recent N price bars, fitting a straight line using the least squares method. The x-axis represents time (bar indices 0, 1, 2, ..., N-1) and the y-axis represents price. The regression produces a slope (rate of price change) and intercept (starting point). The forecast is calculated by evaluating the regression line at position N-1 (the endpoint), which projects the trend one bar forward from the current position.

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

Step 3: Time Series Forecast (F periods ahead)
TSF = intercept + slope × (N - 1 + F - 1)
    = intercept + slope × (N + F - 2)

For F=1 (default): TSF = intercept + slope × (N-1)
For F=2: TSF = intercept + slope × N
For F=3: TSF = intercept + slope × (N+1)
```

> 🔍 **Technical Note:** The implementation achieves O(1) computational complexity through incremental maintenance of sumXY. During warmup (filling phase), sumXY is updated via `sumXY += count × y_new` where count is the current 0-based index. In the sliding window phase, sumXY is updated using `sumXY' = sumXY - sumY + (N-1) × y_new`, eliminating the need to loop through the buffer each bar. The indicator returns `na` during warmup (until N bars accumulated) to ensure valid regression. NA values in the source are properly gated and skipped rather than converted to zeros, preventing bias in the regression calculation. The forecast represents the value at position N-1, which is one step beyond the last observed price.

## Interpretation Details

### Primary Use: Trend Direction Identification

- **Price Above TSF:** Indicates bullish momentum. The current price exceeds the linear projection, suggesting buyers are pushing price higher than the recent trend predicts. This confirms uptrend strength.

- **Price Below TSF:** Indicates bearish momentum. The current price falls short of the linear projection, suggesting sellers are driving price lower than the trend anticipates. This confirms downtrend strength.

- **Price Crossing TSF:** Crossovers signal potential trend changes. Price crossing above TSF suggests emerging bullish momentum; crossing below suggests emerging bearish momentum. These crossovers work similarly to moving average crossovers but with forward-looking context.

### Secondary Use: Dynamic Support and Resistance

- **Support in Uptrends:** During bullish trends, TSF often acts as a rising support line. Price pullbacks frequently find support at or near the TSF level, providing entry opportunities for trend-following trades.

- **Resistance in Downtrends:** During bearish trends, TSF serves as a declining resistance line. Price rallies typically stall at or near TSF, offering short-entry opportunities or exit signals for longs.

- **Bounces and Rejections:** Watch for price reactions at TSF. A bounce off TSF confirms trend continuation; a break through TSF may signal trend exhaustion or reversal.

### Signal Generation and Trading Applications

- **Trend Following:**
  - Entry Signal: Enter long when price crosses above TSF with increasing volume
  - Exit Signal: Exit long when price crosses below TSF
  - Reverse for short positions

- **Pullback Trading:**
  - In uptrends: Buy when price pulls back to touch TSF and bounces
  - In downtrends: Sell when price rallies to touch TSF and rejects
  - Requires confirmation (candlestick patterns, volume)

- **Divergence Analysis:**
  - Price making higher highs while TSF flattens or declines suggests weakening momentum
  - Price making lower lows while TSF flattens or rises suggests potential bottom formation

### Advanced Interpretation

- **TSF Slope Analysis:** The angle of TSF reveals trend strength. Steep upward slopes indicate powerful bullish momentum; steep downward slopes suggest strong bearish pressure. Flattening slopes warn of potential consolidation or trend exhaustion.

- **Distance from TSF:** The gap between price and TSF measures trend extension. Large gaps (price far above/below TSF) indicate potential overextension and increased probability of mean reversion back to the forecast line.

- **Multiple Timeframe Alignment:** When TSF on higher timeframes (daily, weekly) aligns with lower timeframe TSF (hourly, 4H), it confirms strong trend strength. Divergence between timeframes may signal approaching reversals.

- **TSF as Trailing Stop:** In trending markets, TSF can serve as a dynamic trailing stop level. Exit positions when price closes below TSF in uptrends or above TSF in downtrends, allowing profits to run while protecting against major reversals.

## Limitations and Considerations

- **Linear Assumption Weakness:** TSF assumes price movement follows a straight line, making it ineffective during choppy, non-linear markets. Sudden regime changes, news events, or volatility spikes invalidate the linear projection.

- **Lag in Trend Changes:** Despite being "forward-looking," TSF still uses historical data to generate forecasts. During sharp reversals, TSF may continue projecting the old trend direction for several bars before adapting to the new trend.

- **Whipsaw Risk in Ranging Markets:** During sideways consolidation, TSF generates frequent crossover signals as price oscillates above and below the forecast line. These signals produce false entries and exits, resulting in losses.

- **No Probability Bounds:** TSF provides a point estimate without confidence intervals. It doesn't indicate the likelihood or reliability of the forecast, making it difficult to assess prediction certainty.

- **Period Sensitivity Impact:** Short periods (5-10) make TSF highly responsive but prone to noise and false signals. Long periods (25+) smooth TSF but introduce excessive lag, causing missed opportunities at trend changes.

- **Not a Standalone System:** TSF should not be used in isolation. Combine with volume analysis, momentum oscillators (RSI, MACD), or price pattern recognition to confirm signals and filter false entries.

## References

* Kaufman, P. J. (2013). *Trading Systems and Methods* (5th ed.). John Wiley & Sons.
* Achelis, S. B. (2001). *Technical Analysis from A to Z* (2nd ed.). McGraw-Hill.
* Bulkowski, T. N. (2005). *Encyclopedia of Chart Patterns* (2nd ed.). John Wiley & Sons.
* TradingView. Time Series Forecast (TSF) Technical Indicator Documentation.
* StockCharts.com. Time Series Forecast Technical Analysis Reference.

## Validation Sources

**Patterns:** §1 (circular_buffer), §3 (count_warmup), §7 (na_handling), §12 (input_validation), §13 (constant_precomputation), §26 (linear_regression)

**External Sources:** TradingView TSF documentation, StockCharts.com Time Series Forecast reference, Tulip Indicators tsf implementation, TA-Lib LINEARREG_SLOPE + LINEARREG_INTERCEPT combination

**Formula Verification:** Linear regression normal equations confirmed: slope = (N×Σ(xy) - Σx×Σy)/(N×Σ(x²) - (Σx)²), intercept = (Σy - slope×Σx)/N, projection = intercept + slope×(N-1)

**API Verification:** PineScript input.int(minval, maxval), input.source(), plot() overlay signatures confirmed

**Planning:** Sequential thinking phases: pattern retrieval → mathematical validation → external cross-reference → O(1) implementation → documentation
