# ZTEST: Z-Test

[Pine Script Implementation of ZTEST](https://github.com/mihakralj/pinescript/blob/main/indicators/statistics/ztest.pine)

## Overview and Purpose

The Z-Test (technically a one-sample t-test in this implementation) is a statistical hypothesis test used to determine whether a sample mean differs significantly from a hypothesized population mean. Originally developed as a cornerstone of inferential statistics, this test provides a rigorous method for testing hypotheses about population parameters when the population standard deviation must be estimated from the sample.

In trading applications, the Z-Test helps traders assess whether observed price behavior deviates significantly from expected values. For example, it can test whether average returns over a period differ from zero (indicating directional bias), whether current price levels differ from historical averages, or whether volatility-adjusted price movements suggest statistical anomalies worth investigating.

The test's strength lies in its ability to quantify statistical significance through a standardized metric (the t-statistic), making it possible to objectively assess whether observed patterns are likely due to chance or represent meaningful market behavior. For large samples (n ≥ 30), the t-distribution approximates the normal distribution, making critical values of ±1.96 (95% confidence) and ±2.576 (99% confidence) reasonable approximations.

**Technical Note**: This implementation calculates a t-statistic (not a true z-statistic) because it estimates the population standard deviation from the sample data using Bessel's correction. A true z-test requires known population variance, which is rarely available in financial markets.

## Core Concepts

* **Sample Mean vs. Hypothesized Mean:** Tests whether the average value of a series over a lookback period (sample mean) differs from a specified reference value (hypothesized mean μ₀)
* **Standard Error:** Quantifies the uncertainty in the sample mean estimate, calculated as σ / √n, where σ is the sample standard deviation and n is the sample size
* **Z-Statistic:** The standardized test statistic calculated as (x̄ - μ₀) / SE, representing how many standard errors the sample mean is from the hypothesized mean
* **Statistical Significance:** Critical values at ±1.96 (95% confidence) and ±2.576 (99% confidence) determine whether to reject the null hypothesis that the sample mean equals μ₀
* **One-Sample Test:** Unlike two-sample comparisons, the Z-Test evaluates a single series against a theoretical benchmark, making it ideal for testing market hypotheses

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Source | close | Price series to test | **Best practice**: Use returns (close/close[1] - 1) or log returns for meaningful statistical tests |
| Period | 30 | Lookback length for calculating sample statistics | Minimum 30 recommended for t-distribution to approximate normal; increase for longer-term tests (60+) |
| μ₀ (mu0) | 0.0 | Hypothesized population mean | Keep at 0 when testing returns for directional bias; rarely meaningful for raw prices |

**Pro Tip:** This indicator is most meaningful when testing **returns** against zero. Calculate returns as `(close / close[1]) - 1` for simple returns or `math.log(close / close[1])` for log returns, then test against μ₀ = 0 to detect statistically significant directional movement. Testing raw price levels against arbitrary means is statistically problematic due to non-stationarity.

## Calculation and Mathematical Foundation

**Explanation:**
The Z-Test calculates how many standard errors the observed sample mean deviates from a hypothesized value. First, it computes the sample mean and standard deviation over the lookback period using efficient running sums. Then it calculates the standard error (uncertainty of the mean estimate) by dividing the standard deviation by the square root of the sample size. Finally, it standardizes the difference between the sample mean and hypothesized mean by dividing by the standard error, producing the z-statistic.

**Technical formula:**

```
Step 1: Calculate sample mean
x̄ = (1/n) × Σ(xᵢ)

Step 2: Calculate sample variance (with Bessel correction)
s² = [(1/n) × Σ(xᵢ²) - x̄²] × [n/(n-1)]

Step 3: Calculate sample standard deviation
s = √(max(0, s²))

Step 4: Calculate standard error
SE = s / √n

Step 5: Calculate t-statistic
t = (x̄ - μ₀) / SE

Where:
- x̄ = sample mean over period
- μ₀ = hypothesized population mean (user parameter)
- s = sample standard deviation (with Bessel correction)
- n = actual count of non-NA values in window
- SE = standard error of the mean
- n/(n-1) = Bessel correction for unbiased variance estimate
```

> 🔍 **Technical Note:** The implementation uses running sums for numerically stable variance calculation with Bessel's correction (n/(n-1)) to provide an unbiased estimate of population variance. The standard error incorporates the √n adjustment that distinguishes the t-test from the Z-Score (which normalizes individual values without this adjustment). When standard error approaches zero (flat window with no variance), the indicator returns na rather than risking division by zero. The buffer dynamically handles NA values and reinitializes when the period parameter changes.

## Interpretation Details

- **Hypothesis Testing:**
  - **Null Hypothesis (H₀):** The sample mean equals the hypothesized mean (μ = μ₀)
  - **Alternative Hypothesis (H₁):** The sample mean differs from the hypothesized mean (μ ≠ μ₀)
  - **Decision Rule:** If |z| > critical value, reject H₀ and conclude the difference is statistically significant

- **Statistical Significance Levels:**
  - **|z| < 1.96:** Not statistically significant at 95% confidence (fail to reject H₀)
  - **|z| > 1.96:** Statistically significant at 95% confidence (p < 0.05)
  - **|z| > 2.576:** Statistically significant at 99% confidence (p < 0.01)
  - **|z| > 3.29:** Statistically significant at 99.9% confidence (p < 0.001)

- **Directional Interpretation:**
  - **Positive z-values:** Sample mean exceeds hypothesized mean (bullish when testing returns against 0)
  - **Negative z-values:** Sample mean below hypothesized mean (bearish when testing returns against 0)
  - **Near-zero z-values:** Sample mean close to hypothesized mean (no significant deviation)

- **Trading Applications:**
  - **Mean Reversion:** Test if current price level differs significantly from historical average; high |z| values suggest reversion opportunity
  - **Trend Detection:** Test if recent returns differ from zero; sustained high |z| indicates statistically significant trend
  - **Regime Change:** Sudden shifts from low to high |z| values may indicate market regime changes worth investigating
  - **Risk Assessment:** Periods of high |z| values indicate price behavior deviating from normal expectations

## Limitations and Considerations

- **Assumption of Normality:** Z-Test assumes the underlying data follows a normal distribution; financial returns often exhibit fat tails and skewness that violate this assumption
- **Independence Requirement:** Assumes observations are independent; autocorrelation in price series can affect the validity of significance tests
- **Known vs. Estimated Standard Deviation:** Classical z-test assumes known population standard deviation; using sample standard deviation (as required in trading) technically makes this a t-test, though with period > 30 the distinction becomes negligible
- **Multiple Testing Problem:** Running continuous z-tests on overlapping windows increases false positive rates; apply appropriate corrections for repeated testing
- **Parameter Sensitivity:** Choice of lookback period significantly affects results; shorter periods more sensitive to noise, longer periods lag structural changes
- **Not a Trading Signal:** Statistical significance doesn't guarantee profitable trades; must be combined with other technical/fundamental analysis and proper risk management

## References

* Hogg, R. V., Tanis, E. A., & Rao Jammalamadaka, S. (2014). Probability and Statistical Inference (9th ed.). Pearson.
* Casella, G., & Berger, R. L. (2002). Statistical Inference (2nd ed.). Duxbury Press.
* DeGroot, M. H., & Schervish, M. J. (2012). Probability and Statistics (4th ed.). Addison-Wesley.
* Montgomery, D. C., & Runger, G. C. (2014). Applied Statistics and Probability for Engineers (6th ed.). Wiley.

## Validation Sources

**Patterns:** §1 (circular_buffer), §3 (count_warmup), §6 (defensive_division), §7 (na_handling), §8 (variance_calculation), §12 (input_validation)

**Wolfram:** "z-test formula" (confirmed one-sample z-test structure with sample mean, hypothesized mean, standard deviation, sample size)

**Tavily:** "z-test formula one sample standard error calculation statistics" (confirmed SE = σ / √n, z = (x̄ - μ₀) / SE)

**External:** Statistics By Jim, Cuemath, Penn State STAT 200, NCSS statistical documentation

**API:** ref-tools verified input.source, input.int, input.float signatures

**Planning:** sequential-thinking phases = formula_validation, pattern_selection, implementation, documentation
