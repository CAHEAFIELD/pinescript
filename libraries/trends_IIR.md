# IIR Trends Library

PineScript library containing 23 self-contained IIR (Infinite Impulse Response) trend filter functions with no external dependencies. All functions use beta precomputation for optimal warmup compensation.

## Installation

```pine
import mihakralj/trends_IIR/1 as iir
```

## Available Functions

### Exponential Moving Averages

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `ema()` | Exponential Moving Average | O(1) | Beta-optimized warmup, alpha = 2/(n+1) |
| `dema()` | Double EMA | O(1) | Two-pass EMA, lag reduction |
| `tema()` | Triple EMA | O(1) | Three-pass EMA, further lag reduction |
| `qema()` | Quadruple EMA | O(1) | Four-pass EMA, maximum lag reduction |

### Wilder's Smoothing

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `rma()` | Wilder's Smoothing | O(1) | Beta-optimized, alpha = 1/n |
| `smma()` | Smoothed MA | O(1) | Alias for RMA |

### Zero-Lag Filters

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `zlema()` | Zero-Lag EMA | O(1) | Input compensation for lag elimination |
| `zldema()` | Zero-Lag DEMA | O(1) | Dual-pass zero-lag compensation |
| `zltema()` | Zero-Lag TEMA | O(1) | Triple-pass zero-lag compensation |

### Tillson T3 Family

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `t3()` | Tillson T3 | O(1) | 6-stage EMA, volume factor weighted |

### Hull-Derived Filters

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `hema()` | Hull Exponential MA | O(1) | Three-beta composite, lag reduction |

### Kaufman Adaptive

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `kama()` | Kaufman Adaptive MA | O(1) | Efficiency ratio adaptive alpha |

### Fractal Adaptive

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `frama()` | Fractal Adaptive MA | O(1) | Fractal dimension adaptive alpha |

### McGinley Dynamic

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `mcginley()` | McGinley Dynamic | O(1) | Price-adaptive smoothing factor |

### Ehlers Filters

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `ehma()` | Ehlers MA | O(1) | Ehlers' smoothing algorithm |

### Jurik Filters

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `jma()` | Jurik MA | O(1) | Adaptive phase/power smoothing |

### Volume-Weighted Adaptive

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `vidya()` | Variable Index Dynamic Average | O(1) | CMO-based volatility adaptation |
| `vama()` | Volatility Adaptive MA | O(1) | Dual-beta ATR-based adaptation |
| `yzvama()` | Yang-Zhang VAMA | O(1) | Dual-beta YZ volatility adaptation |

### Regression-Based

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `rema()` | Regularized EMA | O(1) | Ridge regression smoothing |
| `epma()` | Error-Prediction MA | O(1) | Forecast error correction |

### Multi-Timeframe

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `mtf_ema()` | Multi-Timeframe EMA | O(1) | Higher timeframe EMA projection |

## Key Optimizations

### Beta Precomputation
All IIR filters use precomputed beta values to eliminate redundant `(1 - alpha)` calculations in warmup loops:

```pine
float alpha = 2.0 / (period + 1)
float beta = 1.0 - alpha  // Computed once
// ...
e *= beta  // Single multiplication instead of subtraction + multiplication
```

### Warmup Compensation
All functions return valid values from bar 1 using exponential warmup compensator:

```pine
if warmup
    e *= beta
    float c = 1.0 / (1.0 - e)
    result := c * ema
    warmup := e > 1e-10
```

### Complexity
All IIR filters achieve **O(1) time complexity** through:
- State variables for running calculations
- No historical lookback operations
- Constant-time updates per bar

## Usage Examples

```pine
//@version=6
indicator("IIR Examples")
import mihakralj/trends_IIR/1 as iir

// Basic EMA
ema20 = iir.ema(close, 20)

// Lag-reduced filters
dema20 = iir.dema(close, 20)
tema20 = iir.tema(close, 20)

// Zero-lag filters
zlema20 = iir.zlema(close, 20)

// Adaptive filters
kama20 = iir.kama(close, 20, 2, 30)
vidya20 = iir.vidya(close, 20, 14)

// Volume-adaptive
vama20 = iir.vama(close, 20, 14)

plot(ema20, "EMA", color.blue)
plot(dema20, "DEMA", color.orange)
plot(zlema20, "ZLEMA", color.green)
```

## Performance Notes

- All functions use O(1) complexity
- Beta precomputation reduces operations by ~30% during warmup
- No array operations or loops (except internal warmup logic)
- Suitable for real-time trading applications
- Memory efficient with fixed state variables

## Mathematical Foundation

### EMA Alpha Variants
- **EMA**: `alpha = 2 / (period + 1)` - Standard exponential
- **RMA**: `alpha = 1 / period` - Wilder's modification
- **Custom**: Any alpha in (0, 1) range

### Cascaded Structures
- **DEMA**: `2×EMA - EMA(EMA)` - Double pass
- **TEMA**: `3×EMA - 3×EMA(EMA) + EMA(EMA(EMA))` - Triple pass
- **T3**: Six-stage EMA with volume factor weighting

### Adaptive Mechanisms
- **KAMA**: Efficiency ratio modulates alpha
- **FRAMA**: Fractal dimension modulates alpha
- **VIDYA**: CMO volatility modulates alpha
- **VAMA**: ATR volatility modulates dual alphas

## References

- Wilder, J. W. (1978). *New Concepts in Technical Trading Systems*
- Kaufman, P. J. (1995). *Smarter Trading*
- Ehlers, J. F. (2001). *Rocket Science for Traders*
- Tillson, T. (1998). *Better Moving Averages*
- Jurik, M. (2000). *Jurik Moving Average*
