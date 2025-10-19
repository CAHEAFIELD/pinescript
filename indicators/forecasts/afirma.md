# AFIRMA: Autoregressive Finite Impulse Response Moving Average

[Pine Script Implementation of AFIRMA](https://github.com/mihakralj/pinescript/blob/main/indicators/forecasts/afirma.pine)

## Overview and Purpose

The Autoregressive Finite Impulse Response Moving Average (AFIRMA) is a sophisticated moving average that implements multiple digital filter windows borrowed from signal processing theory, with optional least squares cubic polynomial fitting for predictive analysis. It was developed as an application of digital signal processing techniques to financial markets, providing traders with enhanced noise filtering capabilities while preserving important price action signals. AFIRMA offers multiple windowing functions (Hanning, Hamming, Blackman, and Blackman-Harris) that can be selected based on specific market conditions and analysis requirements.

When the least squares method is enabled, AFIRMA transitions from a purely trailing indicator to a leading/predictive indicator by fitting a cubic polynomial to recent price data and projecting the trend forward. This autoregressive capability makes AFIRMA suitable for forecasting applications in addition to traditional trend following.

## Core Concepts

* **Multiple window options:** AFIRMA provides four different windowing functions, each with specific spectral characteristics suitable for various market conditions
* **Enhanced noise filtering:** The windowing functions offer superior noise reduction compared to traditional moving averages while preserving important price signals
* **Autoregressive prediction:** Optional least squares cubic polynomial fitting enables predictive/leading behavior by projecting price trends forward
* **Dual mode operation:** Functions as trailing indicator (windowing only) or leading indicator (with least squares method)
* **Timeframe adaptability:** Functions effectively across all timeframes, with longer periods providing cleaner signals in higher timeframes

The core innovation of AFIRMA is its implementation of established windowing functions from digital signal processing combined with optional polynomial regression. The windowing functions shape the frequency response of the filter, allowing for precise control over which price movements are preserved and which are filtered out as noise. When least squares fitting is enabled, AFIRMA fits a cubic polynomial (y = a₀ + a₁x + a₂x² + a₃x³) to the windowed data and returns the fitted value at the current bar, effectively predicting where price should be based on recent trend dynamics.

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Period | 20 | Controls the lookback period | Increase for smoother signals in volatile markets, decrease for responsiveness in trending markets |
| Source | close | Price data used for calculation | Typically left at default; can be changed to hlc3 for more balanced price representation |
| Window Type | Blackman-Harris | Determines filter characteristics | Change based on market conditions and desired balance between smoothness and responsiveness |
| Least Squares | false | Enables autoregressive prediction | Enable for leading/predictive behavior; disable for traditional trailing moving average |

**Pro Tip:** For trend following, use windowing only (Least Squares off) with Blackman-Harris window. For price forecasting and early trend detection, enable Least Squares method - this transforms AFIRMA into a predictive indicator that can lead price movements.

## Calculation and Mathematical Foundation

**Simplified explanation:**
AFIRMA applies carefully designed weighting patterns (windows) to price data. These windows give different importance to different prices in the lookback period, with the weights determined by mathematical functions that optimize signal processing characteristics. When the least squares method is enabled, AFIRMA then fits a smooth cubic curve through this windowed data and returns the fitted value at the current bar, effectively predicting where price should be based on recent trend dynamics.

**Technical formula:**

**Step 1 - Windowing:** AFIRMA applies specialized windowing functions to price data:

1. **Hanning Window**: w(k) = 0.50 - 0.50 × cos(2π × k / n)
2. **Hamming Window**: w(k) = 0.54 - 0.46 × cos(2π × k / n)
3. **Blackman Window**: w(k) = 0.42 - 0.50 × cos(2π × k / n) + 0.08 × cos(4π × k / n)
4. **Blackman-Harris Window**: w(k) = 0.35875 - 0.48829 × cos(2π × k / n) + 0.14128 × cos(4π × k / n) - 0.01168 × cos(6π × k / n)

Where:
- k is the position in the window (0 to n-1)
- n is the window size (period)

Windowed AFIRMA: AFIRMA = Σ(Price[i] × Window_Weight[i]) / Σ(Window_Weight[i])

**Step 2 - Least Squares Fitting (optional):**

When enabled, AFIRMA fits a cubic polynomial to the most recent m = ⌊(period-1)/2⌋ bars:

y = a₀ + a₁x + a₂x² + a₃x³

The coefficients (a₀, a₁, a₂, a₃) are calculated using the normal equations for least squares regression:

- Precompute: Σx, Σx², Σx³, Σx⁴, Σx⁵, Σx⁶ for x = 0 to m-1
- Each bar: Calculate Σy, Σxy, Σx²y, Σx³y from recent prices
- Solve 4×4 linear system using determinant method
- Return fitted value at x=0 (current bar prediction)

> 🔍 **Technical Note:** The windowing functions minimize "spectral leakage" in frequency domain analysis. The cubic polynomial fitting extends the indicator from descriptive (trailing) to predictive (leading) by extrapolating the recent price trend. The combination provides both noise reduction (windowing) and trend projection (least squares).

## Interpretation Details

AFIRMA can be used in various trading strategies, with interpretation varying by mode:

**Trailing Mode (Least Squares off):**
* **Trend identification:** The direction of AFIRMA indicates the prevailing trend
* **Signal generation:** Crossovers between price and AFIRMA can generate trade signals
* **Trend strength assessment:** The distance between price and AFIRMA can indicate trend strength
* **Filter selection:** Different window types can be used for different market conditions:
  - Hanning for general trend following
  - Hamming for markets with sharp transitions
  - Blackman for noisy markets requiring more smoothing
  - Blackman-Harris for extracting weak trends in choppy conditions

**Leading Mode (Least Squares on):**
* **Early trend detection:** AFIRMA predicts where price should be, leading actual price movements
* **Deviation analysis:** When price deviates significantly from AFIRMA prediction, it signals potential trend exhaustion or reversal
* **Forecast validation:** Compare AFIRMA's prediction with actual price - persistent deviations indicate changing market dynamics
* **Entry timing:** Price crossing above predicted AFIRMA suggests bullish momentum; crossing below suggests bearish momentum
* **Mean reversion:** In ranging markets, price tends to revert toward AFIRMA's predicted value

## Limitations and Considerations

* **Mode selection:** Trailing mode best for trend following; leading mode best for forecasting but more sensitive to noise
* **Market conditions:** Windowing-only mode less effective in ranging markets; least squares mode can over-predict in choppy conditions
* **Lag vs lead trade-off:** Trailing mode has lag; leading mode can give false signals during trend transitions
* **Polynomial assumptions:** Least squares method assumes cubic trend continuation, which may not hold during sharp reversals or regime changes
* **Period sensitivity:** Shorter periods make predictions more responsive but noisier; longer periods smoother but less adaptive
* **Complementary tools:** Best used with momentum oscillators, volatility measures, and volume confirmation

## References

* Harris, F.J. "On the Use of Windows for Harmonic Analysis with the Discrete Fourier Transform", Proceedings of the IEEE, 1978
* Ehlers, John F. "Cycle Analytics for Traders", Wiley, 2013
* Skender Stock Indicators for .NET (C# implementation reference)

## Validation Sources

**Patterns:** §27 (Window Functions), §26 (Linear Regression extended to cubic), §04 (Running Sums)
**External:** Skender.Stock.Indicators AFIRMA C# implementation with least_squares_method
**Formulas:** Cubic polynomial least squares normal equations, determinant-based coefficient solving
**API:** input.bool verified for leastSquares parameter
