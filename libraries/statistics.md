# Statistics Indicators Library

Library of 20 statistical functions for analyzing data distributions, relationships, and properties.

## Installation

```pine
import mihakralj/statistics/1 as stats
```

## Statistical Functions by Category

### Central Tendency (Averages)

| Function | Description | Returns | Complexity |
|----------|-------------|---------|------------|
| `cummean(src)` | Cumulative Mean | Cumulative average from start | Cumulative |
| `geomean(src, len)` | Geometric Mean | Multiplicative average | O(1) circular buffer |
| `harmean(src, len)` | Harmonic Mean | Reciprocal average | O(1) circular buffer |
| `median(src, len)` | Median | Middle value | O(n log n) sorting |
| `mode(src, len, bins)` | Mode | Most frequent value | O(n) bucketing |

### Dispersion (Spread)

| Function | Description | Returns | Complexity |
|----------|-------------|---------|------------|
| `stddev(src, len)` | Standard Deviation | Square root of variance | O(1) circular buffer |
| `variance(src, len)` | Variance | Average squared deviation | O(1) circular buffer |
| `iqr(src, len)` | Interquartile Range | Q3 - Q1 | O(n log n) sorting |

### Distribution Shape

| Function | Description | Returns | Complexity |
|----------|-------------|---------|------------|
| `skew(src, len)` | Skewness | Measure of asymmetry | O(1) circular buffer |
| `kurtosis(src, len)` | Kurtosis | Measure of tail heaviness | O(1) circular buffer |

### Relationships (Two Series)

| Function | Description | Returns | Complexity |
|----------|-------------|---------|------------|
| `correlation(src1, src2, len)` | Pearson Correlation | Linear relationship (-1 to 1) | O(1) circular buffers |
| `covariance(src1, src2, len)` | Covariance | Joint variability | O(1) circular buffers |
| `beta(src1, src2, len)` | Financial Beta | Volatility ratio | O(1) circular buffers |

### Percentiles and Quantiles

| Function | Description | Returns | Complexity |
|----------|-------------|---------|------------|
| `percentile(src, len, p)` | Percentile | Value at p% (0-100) | O(n log n) sorting |
| `quantile(src, len, q)` | Quantile | Value at q level (0.0-1.0) | O(n log n) sorting |

### Standardization

| Function | Description | Returns | Complexity |
|----------|-------------|---------|------------|
| `zscore(src, len)` | Z-Score | Standardized value | O(1) circular buffer |
| `bias(src, len)` | Bias | Deviation from SMA | O(1) circular buffer |

### Regression

| Function | Description | Returns | Complexity |
|----------|-------------|---------|------------|
| `linreg(src, len)` | Linear Regression | [intercept, slope] | O(n) |

## Usage Examples

### Basic Central Tendency

```pine
import mihakralj/statistics/1 as stats

// Calculate cumulative mean
cum_avg = stats.cummean(close)
plot(cum_avg, "Cumulative Average", color=color.blue)

// Calculate geometric mean (for returns)
geom_avg = stats.geomean(close, 20)
plot(geom_avg, "Geometric Mean", color=color.green)

// Calculate harmonic mean
harm_avg = stats.harmean(close, 20)
plot(harm_avg, "Harmonic Mean", color=color.red)
```

### Dispersion Analysis

```pine
import mihakralj/statistics/1 as stats

// Calculate standard deviation
std = stats.stddev(close, 20)
plot(std, "StdDev", color=color.yellow)

// Calculate variance
var_val = stats.variance(close, 20)
plot(var_val, "Variance", color=color.orange)

// Calculate interquartile range
iqr_val = stats.iqr(close, 20)
plot(iqr_val, "IQR", color=color.purple)
```

### Distribution Shape

```pine
import mihakralj/statistics/1 as stats

// Calculate skewness
skew_val = stats.skew(close, 50)
plot(skew_val, "Skewness", color=color.yellow)
hline(0, "Zero", color=color.gray)

// Calculate kurtosis
kurt_val = stats.kurtosis(close, 50)
plot(kurt_val, "Kurtosis", color=color.orange)
hline(0, "Normal", color=color.gray)
hline(3, "Excess Kurtosis", color=color.red, linestyle=hline.style_dashed)
```

### Correlation Analysis

```pine
import mihakralj/statistics/1 as stats

// Get benchmark data
spy_close = request.security("SPY", timeframe.period, close)

// Calculate correlation
corr = stats.correlation(close, spy_close, 20)
plot(corr, "Correlation", color=color.yellow)
hline(0, "Zero", color=color.gray)
hline(0.7, "Strong Positive", color=color.green, linestyle=hline.style_dashed)
hline(-0.7, "Strong Negative", color=color.red, linestyle=hline.style_dashed)

// Calculate covariance
cov = stats.covariance(close, spy_close, 20)
plot(cov, "Covariance", color=color.blue)

// Calculate beta
beta_val = stats.beta(close, spy_close, 20)
plot(beta_val, "Beta", color=color.orange)
hline(1, "Market Beta", color=color.gray)
```

### Percentiles and Quantiles

```pine
import mihakralj/statistics/1 as stats

// Calculate median (50th percentile)
med = stats.median(close, 20)
plot(close, "Close", color=color.white)
plot(med, "Median", color=color.yellow, linewidth=2)

// Calculate quartiles using percentiles
q1 = stats.percentile(close, 20, 25)
q2 = stats.percentile(close, 20, 50)  // Same as median
q3 = stats.percentile(close, 20, 75)
plot(q1, "Q1", color=color.green)
plot(q2, "Q2", color=color.yellow)
plot(q3, "Q3", color=color.red)

// Using quantiles (0.0-1.0 scale)
q_low = stats.quantile(close, 20, 0.25)
q_high = stats.quantile(close, 20, 0.75)
plot(q_low, "Lower Quantile", color=color.green)
plot(q_high, "Upper Quantile", color=color.red)
```

### Standardization

```pine
import mihakralj/statistics/1 as stats

// Calculate Z-Score
z = stats.zscore(close, 20)
plot(z, "Z-Score", color=color.yellow)
hline(0, "Mean", color=color.gray)
hline(2, "2 Std", color=color.red, linestyle=hline.style_dashed)
hline(-2, "-2 Std", color=color.green, linestyle=hline.style_dashed)

// Calculate Bias
bias_val = stats.bias(close, 20)
plot(bias_val, "Bias", color=color.orange)
hline(0, "Zero Bias", color=color.gray)
```

### Linear Regression

```pine
import mihakralj/statistics/1 as stats

// Calculate linear regression
[intercept, slope] = stats.linreg(close, 20)

// Plot regression line
regression_value = intercept + slope * 19  // Current bar is at position 19 in 20-bar window
plot(close, "Close", color=color.white)
plot(regression_value, "Regression", color=color.yellow, linewidth=2)

// Plot slope as separate indicator
plot(slope, "Slope", color=slope > 0 ? color.green : color.red)
hline(0, "Zero Slope", color=color.gray)
```

### Mode Analysis

```pine
import mihakralj/statistics/1 as stats

// Calculate mode with different bin sizes
mode_20bins = stats.mode(close, 50, 20)
mode_50bins = stats.mode(close, 50, 50)

plot(close, "Close", color=color.white)
plot(mode_20bins, "Mode (20 bins)", color=color.yellow)
plot(mode_50bins, "Mode (50 bins)", color=color.orange)
```

## Function Categories

### O(1) Indicators - Circular Buffer
Constant-time complexity using running sums:
- **stddev, variance**: Single-pass variance algorithm
- **correlation, covariance, beta**: Two-series circular buffers
- **geomean, harmean**: Logarithmic/reciprocal circular buffers
- **skew, kurtosis**: Higher moment calculations
- **zscore, bias**: Standardization with running stats
- **cummean**: Cumulative state variable

### O(n) Indicators - Window Operations
Linear time complexity:
- **linreg**: Least squares regression over window

### O(n log n) Indicators - Sorting Required
Require sorting for order statistics:
- **median**: Middle value calculation
- **percentile, quantile**: Positional statistics
- **iqr**: Interquartile range (Q3 - Q1)
- **mode**: Most frequent value (with bucketing)

## Mathematical Foundations

### Standard Deviation / Variance
```
Variance = E[X²] - (E[X])²
StdDev = √Variance
```
Single-pass algorithm using Welford's method.

### Pearson Correlation
```
Correlation = Cov(X,Y) / (σ_X × σ_Y)
```
Where Cov(X,Y) is covariance and σ is standard deviation.

### Financial Beta
```
Beta = Cov(Asset, Market) / Var(Market)
```
Measures asset volatility relative to market.

### Skewness
```
Skewness = E[(X-μ)³] / σ³
```
Measures distribution asymmetry (0 = symmetric).

### Kurtosis
```
Kurtosis = E[(X-μ)⁴] / σ⁴ - 3
```
Measures tail heaviness (0 = normal distribution).

### Z-Score
```
Z = (X - μ) / σ
```
Standardizes values to mean=0, std=1.

### Linear Regression
```
Slope = (n×ΣXY - ΣX×ΣY) / (n×ΣX² - (ΣX)²)
Intercept = (ΣY - Slope×ΣX) / n
```

### Geometric Mean
```
GeomMean = exp(Σlog(X_i) / n)
```
Used for multiplicative returns.

### Harmonic Mean
```
HarmMean = n / Σ(1/X_i)
```
Used for rates and ratios.

### Interquartile Range
```
IQR = Q3 - Q1
```
Where Q1 is 25th percentile, Q3 is 75th percentile.

## Performance Characteristics

### Circular Buffer Pattern (O(1))
Most efficient - constant time per bar:
```pine
var array<float> buffer = array.new_float(period, na)
var int head = 0
var float sum = 0.0

float oldest = array.get(buffer, head)
if not na(oldest)
    sum -= oldest

sum += current
array.set(buffer, head, current)
head := (head + 1) % period
```

### Sorting Pattern (O(n log n))
Required for order statistics:
```pine
var array<float> values = array.new_float(0)
array.clear(values)
for i = 0 to len - 1
    if not na(src[i])
        array.push(values, src[i])
array.sort(values)
```

## Common Use Cases

### Distribution Analysis
- **median** vs **mean** (cummean): Robustness to outliers
- **stddev/variance**: Volatility measurement
- **skew**: Asymmetry detection
- **kurtosis**: Fat tails detection

### Relative Performance
- **correlation**: Relationship strength
- **beta**: Systematic risk measurement
- **covariance**: Joint movement

### Outlier Detection
- **zscore**: Identify values >2σ from mean
- **iqr**: Detect values beyond Q1-1.5×IQR or Q3+1.5×IQR
- **percentile**: Extreme value thresholds

### Trend Analysis
- **linreg**: Linear trend fitting
- **bias**: Deviation from moving average

## Complex Indicators Not Included

The following 7 indicators remain as standalone indicators due to complexity:

| Indicator | Reason | Use Case |
|-----------|--------|----------|
| **cointegration** | Requires multi-symbol input + embedded helpers (sma, stddev, correlation) | Pairs trading, statistical arbitrage |
| **entropy** | Complex binning and normalization logic | Market efficiency, randomness |
| **granger** | Multiple embedded helper functions (mean, variance, covariance) | Causality testing |
| **hurst** | Requires long-range dependency calculations | Trend persistence analysis |
| **jb** | Jarque-Bera test with chi-square distribution | Normality testing |
| **kendall** | Tau correlation with concordant pairs | Non-parametric correlation |
| **spearman** | Rank correlation requiring ranking logic | Non-parametric correlation |
| **theil** | Theil-Sen estimator with median of slopes | Robust regression |
| **ztest** | Hypothesis testing with significance levels | Statistical testing |

These indicators are best used directly from `indicators/statistics/` directory.

## Dependencies

This library has **no external dependencies**. All functions are self-contained and do not rely on the `ta.*` namespace or other libraries.

## Related Libraries

- **trends_FIR**: Moving averages (SMA, WMA, DWMA, HMA)
- **trends_IIR**: Exponential moving averages (EMA, DEMA, TEMA, etc.)
- **volatility**: Volatility indicators (ATR, Bollinger Bands, True Range)
- **volume**: Volume-based indicators (OBV, CMF, MFI, VWAP)

## References

### Statistical Theory
- Pearson, Karl - Correlation coefficient (1895)
- Student (Gosset, W.S.) - t-distribution and statistical testing
- Fisher, R.A. - Statistical methods and distribution theory
- Welford, B.P. - Single-pass variance algorithm (1962)

### Financial Applications
- Sharpe, William - Beta coefficient and CAPM (1964)
- Markowitz, Harry - Portfolio theory and variance (1952)

### Technical Analysis
- Murphy, John J. - Technical Analysis of the Financial Markets
- Kaufman, Perry J. - Trading Systems and Methods

### Implementation References
- pandas (Python) - Statistical functions
- NumPy (Python) - Numerical computing
- SciPy (Python) - Scientific computing
- Apache Commons Math (Java) - Statistical library

## Version History

### v1 (Current)
- 20 statistical functions with optimized implementations
- O(1) circular buffers for standard deviation, variance, correlation, covariance, beta
- O(n log n) sorting for median, percentiles, quantiles
- Single-pass algorithms for skewness and kurtosis
- Cumulative mean with running state
- No external dependencies
- 7 complex indicators remain as standalone (cointegration, entropy, granger, hurst, jb, kendall, spearman, theil, ztest)

---

**License**: MIT License
**Author**: mihakralj
**Repository**: https://github.com/mihakralj/pinescript
