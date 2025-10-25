# BBB: Bollinger %B (Bollinger Percent B)

[Pine Script Implementation of BBB](https://github.com/mihakralj/pinescript/blob/main/indicators/oscillators/bbb.pine)

## Overview and Purpose

Bollinger %B (also known as Percent B or %b) is a normalized oscillator derived from Bollinger Bands that quantifies a security's price position relative to the upper and lower bands. Developed as a companion indicator to John Bollinger's Bollinger Bands, %B transforms the band relationship into a single value that oscillates in a predictable range, making it easier to identify overbought and oversold conditions, as well as trend strength.

Unlike traditional Bollinger Bands which display three lines (upper band, middle basis, lower band) on a price chart, Bollinger %B distills this information into a single oscillator that typically ranges from 0 to 1, though it can exceed these bounds when price moves outside the bands. When %B equals 0, price is at the lower band; when %B equals 1, price is at the upper band; when %B equals 0.5, price is at the middle basis line (the simple moving average).

The indicator's primary strength lies in its ability to normalize band relationships across different securities and timeframes. By expressing price position as a percentage of band width, %B enables direct comparisons and consistent interpretation regardless of the underlying security's price or volatility. This normalization makes %B particularly valuable for developing systematic trading strategies, identifying extreme conditions, and confirming breakouts or reversals signaled by other indicators.

## Core Concepts

* **Normalized Band Position:** Expresses price location as a percentage of the distance between upper and lower Bollinger Bands, with 0 = lower band, 0.5 = middle basis, 1 = upper band.

* **Band Breakthrough Detection:** Values above 1 indicate price is above the upper band (potential overbought extreme), while values below 0 indicate price is below the lower band (potential oversold extreme).

* **Volatility Context:** Unlike fixed oscillators like RSI or Stochastic, %B adjusts dynamically based on volatility changes since it's derived from standard deviation-based bands.

* **Mean Reversion Signals:** Readings near 0 or 1 suggest price is extended from the mean and may revert, while readings near 0.5 indicate price is at equilibrium (the moving average).

* **Trend Strength Indicator:** Sustained readings above 0.8 or below 0.2 can indicate strong trends ("walking the bands"), while oscillations between 0.2 and 0.8 suggest ranging conditions.

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|----------------|
| Period | 20 | Lookback period for SMA and standard deviation calculation | Shorter (10-15) for more sensitive signals in volatile markets; longer (30-50) for smoother signals and longer-term trend analysis |
| Source | close | Price data used for calculation | Use typical price (hlc3) for more balanced representation; use close for end-of-bar signals; use hl2 for intraday midpoint |
| StdDev Multiplier | 2.0 | Number of standard deviations for band width | Increase to 2.5 or 3.0 for wider bands (fewer false signals); decrease to 1.5 for narrower bands (more frequent signals) |

**Pro Tip:** The default 20-period setting with 2.0 standard deviations works well across most markets and timeframes. For trend-following strategies in strongly trending markets, consider using %B values of 0.8/0.2 as thresholds rather than the traditional overbought/oversold levels. When markets are "walking the bands" (staying consistently near 1 or 0), it indicates strong trends where mean reversion strategies should be avoided.

## Calculation and Mathematical Foundation

**Simplified Explanation:**
Bollinger %B measures where the current price sits within the Bollinger Bands as a percentage. First, it calculates the standard Bollinger Bands: a 20-period simple moving average (the basis) plus and minus 2 standard deviations (the upper and lower bands). Then it calculates how far price is above the lower band as a percentage of the total band width. If price is at the lower band, %B = 0%. If price is at the upper band, %B = 100% (or 1.0). If price is exactly at the middle line, %B = 50% (or 0.5).

**Technical Formula:**

```
Step 1: Calculate Simple Moving Average (Basis)
    basis = SMA(price, period)

Step 2: Calculate Standard Deviation
    variance = mean(price²) - mean(price)²
    stddev = sqrt(variance)

Step 3: Calculate Bollinger Bands
    upper_band = basis + (multiplier × stddev)
    lower_band = basis - (multiplier × stddev)

Step 4: Calculate Band Width
    band_width = upper_band - lower_band

Step 5: Calculate %B
    %B = (price - lower_band) / band_width
```

> 🔍 **Technical Note:** The implementation uses Welford's online algorithm with running sums for O(1) complexity variance calculation, ensuring valid output from bar 1. The formula `variance = sumSq/count - mean²` is mathematically equivalent to the traditional variance formula but computable incrementally. When band width approaches zero (extremely low volatility), %B defaults to 0.5 to avoid division by zero. Values outside the 0-1 range are valid and indicate price has breached the bands.

## Interpretation Details

Bollinger %B provides multiple analytical perspectives:

* **Band Position Analysis:**
  - %B = 1.0: Price exactly at upper Bollinger Band
  - %B > 1.0: Price above upper band (strong bullish momentum or overbought)
  - %B = 0.8-1.0: Price near upper band, potentially overbought in ranging markets
  - %B = 0.5: Price at middle basis line (moving average), neutral position
  - %B = 0.0-0.2: Price near lower band, potentially oversold in ranging markets
  - %B = 0.0: Price exactly at lower Bollinger Band
  - %B < 0.0: Price below lower band (strong bearish momentum or oversold)

* **Overbought/Oversold in Ranging Markets:**
  - %B > 0.8: Potential overbought condition, watch for reversal
  - %B < 0.2: Potential oversold condition, watch for reversal
  - Multiple touches of 0.8 or 0.2 without breakthrough suggest range-bound behavior
  - Best used when price oscillates regularly between bands

* **Trend Identification ("Walking the Bands"):**
  - Sustained readings above 0.8: Strong uptrend, price consistently near upper band
  - Sustained readings below 0.2: Strong downtrend, price consistently near lower band
  - In strong trends, traditional overbought/oversold signals fail; use continuation strategies instead
  - Trend weakens when %B starts oscillating back toward 0.5

* **Breakout Confirmation:**
  - %B moving from <0 to >0: Potential bullish breakout from lower band
  - %B moving from >1 to <1: Potential bearish rejection at upper band
  - %B crossing above 1.0 with expanding bands: Strong breakout confirmation
  - %B crossing below 0.0 with expanding bands: Strong breakdown confirmation

* **Volatility Context:**
  - Narrow band width (bands close together): Low volatility, expect expansion
  - Wide band width (bands far apart): High volatility, expect contraction
  - %B oscillations become more dramatic in low volatility environments
  - Consider band width along with %B reading for complete picture

* **Divergence Detection:**
  - Bullish divergence: Price makes lower low while %B makes higher low
  - Bearish divergence: Price makes higher high while %B makes lower high
  - Divergences most reliable near extreme %B levels (>0.8 or <0.2)
  - Multiple divergences increase signal strength

## Limitations and Considerations

- **Not a Standalone Signal:** Bollinger %B indicates where price is relative to bands but doesn't predict future direction. In strong trends, readings can stay extreme (>0.8 or <0.2) for extended periods. Always confirm signals with price action, volume, or other indicators.

- **Trend vs. Range Context Required:** The same %B reading means different things in different market conditions. Readings >0.8 suggest overbought in ranging markets but indicate strength in uptrends. Readings <0.2 suggest oversold in ranging markets but indicate weakness in downtrends. Misidentifying market context leads to counter-trend trades in strong trends.

- **Lagging Component:** Because %B is based on a simple moving average and standard deviation calculation, it inherits the lag of these components. In fast-moving markets, %B may not reach extreme levels until after the move has started. The default 20-period setting provides a balance but still lags immediate price action.

- **Band Squeeze Challenges:** During periods of extremely low volatility (tight bands), %B can give frequent whipsaw signals as price oscillates between 0 and 1 in a narrow range. Band width should be monitored alongside %B to avoid trading in low-volatility compression periods that precede breakouts.

- **Volatility Adjustment Limitations:** While %B adjusts for volatility by using standard deviation-based bands, it doesn't account for changing market regimes. What constitutes "normal" band position in a low-volatility period differs from high-volatility periods, even though %B readings may look similar.

- **No Volume Confirmation:** Bollinger %B is purely price-based and doesn't incorporate volume data. Breakouts or reversals signaled by %B should ideally be confirmed with volume analysis, as moves without volume support often fail. Strong %B readings on declining volume suggest weak commitment to the move.

## References

* Bollinger, J. (2001). *Bollinger on Bollinger Bands*. McGraw-Hill.
* Bollinger, J. (2002). "Using Bollinger Bands." *Technical Analysis of Stocks & Commodities Magazine*.
* Murphy, J. J. (1999). *Technical Analysis of the Financial Markets*. New York Institute of Finance.
* Achelis, S. B. (2001). *Technical Analysis from A to Z*. McGraw-Hill.
