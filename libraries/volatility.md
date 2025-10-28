# Volatility Library

PineScript library containing 26 self-contained volatility measurement functions with no external dependencies. Functions using IIR smoothing include beta precomputation for optimal performance.

## Installation

```pine
import mihakralj/volatility/1 as vol
```

## Available Functions

### Basic Range Measures

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `tr()` | True Range | O(1) | Fundamental volatility measure |
| `atr()` | Average True Range | O(1) | Beta-optimized RMA smoothing |
| `natr()` | Normalized ATR | O(1) | ATR as percentage of price |
| `atrn()` | ATR Normalized | O(1) | ATR relative to max over period |
| `atrp()` | ATR Percentage | O(1) | ATR as percentage of close |
| `adr()` | Average Daily Range | O(1) | Mean high-low range |

### Log Return Volatility

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `hv()` | Historical Volatility | O(1) | Standard deviation of log returns |
| `rv()` | Realized Volatility | O(1) | RMS of log returns |
| `rsv()` | Rogers-Satchell Volatility | O(1) | OHLC-based, drift-independent |
| `gkv()` | Garman-Klass Volatility | O(1) | OHLC estimator, efficient |
| `yzv()` | Yang-Zhang Volatility | O(1) | Complete OHLC, overnight gaps |
| `hlv()` | High-Low Volatility | O(1) | Simple HL range volatility |
| `pv()` | Parkinson Volatility | O(1) | HL-based estimator |

### Volatility Indices and Ratios

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `rvi()` | Relative Volatility Index | O(1) | RSI applied to volatility |
| `cv()` | Close Volatility | O(1) | Simple close-based volatility |
| `ccv()` | Chaikin Volatility | O(1) | Rate of change in ATR |
| `cvi()` | Coefficient of Variation Index | O(1) | Normalized volatility |
| `vr()` | Volatility Ratio | O(1) | Short/long volatility ratio |
| `vov()` | Volatility of Volatility | O(1) | Second-order volatility |

### Bollinger Band Volatility

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `bbw()` | Bollinger Band Width | O(1) | Width between bands |
| `bbwn()` | BBW Normalized | O(1) | BBW as % of middle band |
| `bbwp()` | BBW Percentage | O(1) | BBW as % position |

### Specialized Volatility Indicators

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `ui()` | Ulcer Index | O(1) | Downside volatility measure |
| `massi()` | Mass Index | O(1) | Volatility expansion indicator |
| `ewma()` | EWMA Volatility | O(1) | Exponentially weighted variance |
| `jvolty()` | Jurik Volatility | O(1) | Adaptive volatility |
| `jvoltyn()` | Jurik Volatility Normalized | O(1) | JVolty as % of price |

## Key Optimizations

### Beta Precomputation for ATR Family
All ATR-based indicators (ATR, NATR, ATRN, ATRP) use precomputed beta values:

```pine
float alpha = 1.0 / float(length)
float beta = 1.0 - alpha  // Computed once
// ...
e *= beta  // Single multiplication in warmup loop
```

### Circular Buffers
FIR-based volatility measures use O(1) circular buffers:

```pine
var array<float> buffer = array.new_float(length, na)
var int head = 0
var float sum = 0.0
// Update in O(1) time
```

### Annualization
Log-return volatility measures support optional annualization:

```pine
float volatility = stdDev
if annualize
    volatility := volatility * math.sqrt(float(annualPeriods))
```

## Usage Examples

```pine
//@version=6
indicator("Volatility Examples")
import mihakralj/volatility/1 as vol

// Basic ATR
atr14 = vol.atr(14)
natr14 = vol.natr(14)

// Log return volatility
hv20 = vol.hv(close, 20, true, 252)  // Annualized
rv20 = vol.rv(close, 20, true, 252)

// OHLC estimators
gkv20 = vol.gkv(20, true, 252)
yzv20 = vol.yzv(20, true, 252)

// Bollinger width
bbw20 = vol.bbw(20, 2.0)

// Specialized
ui14 = vol.ui(14)
massi9 = vol.massi(9, 25)

plot(atr14, "ATR", color.blue)
plot(hv20, "HV", color.orange)
plot(bbw20, "BBW", color.green)
```

## Performance Notes

- ATR family: O(1) with beta-optimized warmup
- Log return measures: O(1) with circular buffers
- All functions handle NA values correctly
- Suitable for real-time trading applications
- Memory efficient with fixed state variables

## Mathematical Foundation

### True Range
```
TR = max(high - low, |high - close[1]|, |low - close[1]|)
```

### ATR (Wilder's Smoothing)
```
alpha = 1 / period
ATR = RMA(TR, period)
```

### Historical Volatility
```
HV = StdDev(log(close / close[1]), period) × sqrt(annualPeriods)
```

### Garman-Klass
```
GKV = sqrt(0.5 × log(H/L)² - (2×log(2)-1) × log(C/O)²)
```

### Yang-Zhang
```
YZV = sqrt(variance_overnight + k×variance_open + (1-k)×variance_close_to_close)
```

### Parkinson
```
PV = sqrt(log(H/L)² / (4×log(2)))
```

## Annualization Factors

Common annualization periods:
- **Daily data**: 252 trading days
- **Hourly data**: 252 × 6.5 = 1638 hours
- **Minute data**: 252 × 390 = 98280 minutes

## Volatility Estimator Efficiency

Relative efficiency compared to standard deviation:
- **Close-to-Close**: 1.00 (baseline)
- **Parkinson (HL)**: 5.2× more efficient
- **Garman-Klass (OHLC)**: 7.4× more efficient
- **Rogers-Satchell**: 8.5× more efficient
- **Yang-Zhang**: 14× more efficient (best)

## References

- Wilder, J. W. (1978). *New Concepts in Technical Trading Systems*
- Parkinson, M. (1980). "The Extreme Value Method for Estimating the Variance of the Rate of Return"
- Garman, M. B., & Klass, M. J. (1980). "On the Estimation of Security Price Volatilities from Historical Data"
- Rogers, L. C. G., & Satchell, S. E. (1991). "Estimating Variance from High, Low and Closing Prices"
- Yang, D., & Zhang, Q. (2000). "Drift-Independent Volatility Estimation Based on High, Low, Open, and Close Prices"
- Bollinger, J. (2002). *Bollinger on Bollinger Bands*
- Jurik, M. (2000). *Jurik Volatility*
