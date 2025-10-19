# FISHER: Fisher Transform

[Pine Script Implementation of FISHER](https://github.com/mihakralj/pinescript/blob/main/indicators/oscillators/fisher.pine)

## Overview and Purpose

The Fisher Transform is a technical indicator developed by John F. Ehlers and introduced in his 2002 article "Using The Fisher Transform" in Stocks & Commodities magazine. The indicator applies a mathematical transformation to price data that converts it into a Gaussian normal distribution, making price reversals more identifiable and predictable. Unlike traditional oscillators that simply normalize price, the Fisher Transform uses the inverse hyperbolic tangent function (arctanh) to stretch price extremes, creating sharper and more distinct turning points.

The indicator's primary strength lies in its ability to generate clear signals near market turning points. By transforming price data into a near-perfect Gaussian probability distribution function, the Fisher Transform provides traders with statistically significant signals that stand out from market noise. The transformation emphasizes extreme price movements while compressing normal price action, making potential reversals easier to identify.

The Fisher Transform has gained widespread adoption among technical traders for its mathematical elegance and practical effectiveness. It works particularly well in trending markets where it can identify potential entry and exit points with greater precision than traditional momentum oscillators. The indicator's unique transformation method represents a significant advancement in applying mathematical statistics to price analysis.

## Core Concepts

* **Gaussian Transformation:** Converts price data from its natural distribution into a Gaussian (normal) distribution using the inverse hyperbolic tangent function, making extreme values more prominent and easier to identify.

* **Price Normalization:** First normalizes price to a range of -1 to +1 using the highest high and lowest low over the lookback period, creating a bounded input suitable for the arctanh transformation.

* **Domain Protection:** Clamps the normalized value to [-0.999, 0.999] before transformation to avoid mathematical errors, as arctanh is undefined at exactly ±1.

* **EMA Smoothing:** Applies exponential smoothing to the normalized value before transformation to reduce noise and prevent whipsaw signals while maintaining responsiveness to price changes.

* **Signal Line:** Applies EMA smoothing (same alpha = 0.33) to the Fisher Transform to generate a signal line, providing smooth crossover signals that indicate potential entry and exit points.

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|----------------|
| Period | 10 | Lookback period for min/max normalization | Shorter (5-7) for more sensitive signals in fast markets; longer (14-20) for smoother signals in volatile markets |
| Source | hl2 | Price source for calculation | Use close for end-of-day analysis; use hl2 (default) for intraday balance; use hlc3 for triple-weighted center |

**Pro Tip:** The default period of 10 works well for swing trading on daily charts. Day traders may prefer shorter periods (5-7) for more responsive signals, while position traders might use longer periods (14-20) to filter out noise. The Fisher Transform is particularly effective when combined with trend-following indicators to confirm directional bias.

## Calculation and Mathematical Foundation

**Simplified Explanation:**
The Fisher Transform takes price data and applies a series of mathematical operations to convert it into a form that follows a normal (bell curve) distribution. First, it finds the highest and lowest prices over a lookback period and normalizes the current price between -1 and +1. Then it applies smoothing to reduce noise. Finally, it uses the arctanh (inverse hyperbolic tangent) function to transform the normalized value, which stretches out the extreme values and compresses the middle range. This transformation makes price reversals stand out more clearly as sharp peaks and valleys.

**Technical Formula:**

```
Step 1: Find highest high and lowest low over period
    highest = max(price[0], price[1], ..., price[period-1])
    lowest = min(price[0], price[1], ..., price[period-1])

Step 2: Normalize price to [-1, 1]
    normalized = 2 * ((price - lowest) / (highest - lowest)) - 1

Step 3: Apply EMA smoothing (alpha = 0.33)
    value = alpha * normalized + (1 - alpha) * value[1]

Step 4: Clamp to valid domain for arctanh
    value = max(-0.999, min(0.999, value))

Step 5: Apply Fisher Transform (arctanh)
    fisher = 0.5 * ln((1 + value) / (1 - value))

Step 6: Generate signal line (EMA smoothing)
    signal = alpha * fisher + (1 - alpha) * signal[1]
```

> 🔍 **Technical Note:** The Fisher Transform uses the arctanh (inverse hyperbolic tangent) function, mathematically expressed as arctanh(x) = 0.5 * ln((1+x)/(1-x)). This transformation is undefined at x = ±1, which is why the normalized value must be clamped to [-0.999, 0.999]. The EMA smoothing with alpha = 0.33 provides a balance between responsiveness and stability, though this can be adjusted based on trading timeframe and volatility conditions.

## Interpretation Details

The Fisher Transform provides multiple analytical perspectives:

* **Extreme Values:**
  - Fisher > +2: Strong overbought condition, watch for potential reversal down
  - Fisher < -2: Strong oversold condition, watch for potential reversal up
  - Values beyond ±3: Extreme conditions, high probability reversals
  - The more extreme the value, the stronger the reversal signal

* **Crossover Signals:**
  - Fisher crosses above signal line: Bullish signal, potential buy opportunity
  - Fisher crosses below signal line: Bearish signal, potential sell opportunity
  - Signal line is EMA-smoothed Fisher Transform, providing stable crossover points
  - Crossovers near extreme levels (+2/-2) provide higher probability signals
  - Crossovers at zero line indicate trend changes

* **Zero-Line Analysis:**
  - Fisher above zero: Bullish bias, price strength
  - Fisher below zero: Bearish bias, price weakness
  - Zero-line crossovers confirm trend direction changes
  - Extended periods above/below zero indicate strong trends

* **Divergence Detection:**
  - Bullish divergence: Price makes lower low while Fisher makes higher low (reversal up signal)
  - Bearish divergence: Price makes higher high while Fisher makes lower high (reversal down signal)
  - Divergences near extreme levels (+2/-2) are more reliable
  - Multiple divergences increase signal strength

* **Signal Confirmation:**
  - Best used with trend-following indicators for directional confirmation
  - Combine with volume analysis to confirm signal strength
  - Look for multiple timeframe alignment for higher probability trades
  - Use support/resistance levels to validate reversal zones

## Limitations and Considerations

- **Lag in Transformation:** The EMA smoothing and transformation process introduces some lag, meaning signals may occur after the actual turning point. This is particularly noticeable in fast-moving markets where price can reverse before the indicator generates a signal.

- **Lookback Period Sensitivity:** The indicator requires sufficient price history for accurate min/max calculation. During the initial bars (less than the period setting), the normalization may not accurately reflect true extremes, potentially generating unreliable signals.

- **Whipsaw in Ranging Markets:** In sideways or choppy markets without clear trends, the Fisher Transform can generate frequent crossover signals that result in false entries and exits. The indicator performs best in trending markets where price movements have clear direction.

- **Extreme Value Interpretation:** While values beyond ±2 indicate potential reversals, markets can remain overbought or oversold for extended periods during strong trends. Extreme readings alone should not be used as sole entry signals without additional confirmation.

- **Mathematical Domain Constraints:** The arctanh function requires careful input handling. The indicator must clamp values to prevent mathematical errors, which can theoretically limit sensitivity at extreme price movements, though this rarely impacts practical trading.

- **No Volume Consideration:** The Fisher Transform is purely price-based and does not incorporate volume data. This can lead to signals that lack confirmation from trading activity, particularly important at potential reversal points where volume should typically increase.

## References

* Ehlers, J. F. (2002). "Using The Fisher Transform." *Stocks & Commodities Magazine*, V. 20:11 (November).
* Ehlers, J. F. (2004). *Cybernetic Analysis for Stocks and Futures*. John Wiley & Sons.
* Murphy, J. J. (1999). *Technical Analysis of the Financial Markets*. New York Institute of Finance.
