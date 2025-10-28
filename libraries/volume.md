# Volume Indicators Library

Library of 26 volume-based technical indicators for analyzing trading volume patterns, money flow, and price-volume relationships.

## Installation

```pine
import mihakralj/volume/1 as vol
```

## Volume Functions by Category

### Money Flow Indicators

| Function | Description | Returns | Optimization |
|----------|-------------|---------|--------------|
| `adl()` | Accumulation/Distribution Line | Cumulative ADL value | Cumulative calculation |
| `adosc(short, long)` | Chaikin A/D Oscillator | ADOSC value | EMA warmup compensation |
| `cmf(len)` | Chaikin Money Flow | CMF value (-1 to 1) | Rolling sum calculation |
| `mfi(len)` | Money Flow Index | MFI value (0-100) | O(1) circular buffers |
| `wad()` | Williams A/D | Cumulative WAD value | Cumulative calculation |
| `vwad(period)` | Volume Weighted A/D | VWAD value | Volume-weighted MFM |

### On-Balance Volume (OBV) Indicators

| Function | Description | Returns | Optimization |
|----------|-------------|---------|--------------|
| `obv()` | On Balance Volume | Cumulative OBV | Cumulative calculation |
| `aobv(src, vol)` | Archer OBV | [Fast, Slow] | Beta-optimized dual EMA |
| `pvo(fast, slow, signal)` | Percentage Volume Oscillator | [PVO, Signal] | Beta-optimized triple EMA |

### Volume Index Indicators

| Function | Description | Returns | Optimization |
|----------|-------------|---------|--------------|
| `nvi(src, vol, start)` | Negative Volume Index | NVI value | Ratio accumulation |
| `pvi(src, vol, start)` | Positive Volume Index | PVI value | Ratio accumulation |
| `tvi(price, min_tick, vol)` | Trade Volume Index | TVI value | Directional accumulation |

### Volume Oscillators

| Function | Description | Returns | Optimization |
|----------|-------------|---------|--------------|
| `kvo(fast, slow, signal)` | Klinger Volume Oscillator | [KVO, Signal] | EMA warmup compensation |
| `vo(short, long, signal)` | Volume Oscillator | Smoothed VO value | Triple circular buffer |
| `vroc(period, calc_type, vol)` | Volume Rate of Change | VROC value | Point or percentage |

### Volume-Weighted Averages

| Function | Description | Returns | Optimization |
|----------|-------------|---------|--------------|
| `vwap(src, vol, reset)` | Volume Weighted Average Price | VWAP value | Session-based reset |
| `vwma(src, vol, period)` | Volume Weighted MA | VWMA value | Circular buffer |
| `twap(src, reset)` | Time Weighted Average Price | TWAP value | Session-based reset |

### Force and Pressure Indicators

| Function | Description | Returns | Optimization |
|----------|-------------|---------|--------------|
| `efi(len, src, vol)` | Elder's Force Index | EFI value | Beta-optimized EMA |
| `vf(len, src, vol)` | Volume Force | VF value | EMA smoothing |
| `va()` | Volume Accumulation | Cumulative VA | Price-midpoint volume |

### Divergence and Rank Indicators

| Function | Description | Returns | Optimization |
|----------|-------------|---------|--------------|
| `pvd(price_per, vol_per, smooth)` | Price Volume Divergence | Smoothed divergence | Circular buffer smoothing |
| `pvr(price, vol)` | Price Volume Rank | Rank (0-4) | State classification |
| `pvt(src, vol)` | Price Volume Trend | Cumulative PVT | Ratio accumulation |

### Specialized Volume Indicators

| Function | Description | Returns | Optimization |
|----------|-------------|---------|--------------|
| `eom(smooth, scale)` | Ease of Movement | EOM value | Box ratio calculation |
| `iii(period, cumulative)` | Intraday Intensity Index | III value | Position multiplier |

## Key Optimizations

### Beta Precomputation (IIR Indicators)
Indicators using EMA warmup compensation (AOBV, EFI, PVO, VF, KVO) precompute `beta = 1 - alpha` once to eliminate redundant subtraction operations in warmup loops:

```pine
float alpha = 2.0 / (period + 1)
float beta = 1.0 - alpha  // Precomputed once

// Warmup loop
e *= beta  // Single multiplication instead of (1 - alpha)
```

### Circular Buffers (FIR Indicators)
Indicators with lookback periods (CMF, MFI, VO, VWMA, VWAD) use O(1) circular buffers for constant-time sliding window operations.

### Cumulative Calculations
Indicators tracking cumulative values (ADL, OBV, PVT, VA, WAD, TVI) maintain running sums updated each bar for efficient calculation.

### Money Flow Multiplier Pattern
Multiple indicators (ADL, CMF, VWAD, WAD) share common Money Flow Multiplier calculation:
```pine
mfm = ((close - low) - (high - close)) / (high - low)
```

## Usage Examples

### Basic Money Flow Analysis
```pine
import mihakralj/volume/1 as vol

// Calculate Chaikin Money Flow
cmf_value = vol.cmf(20)
plot(cmf_value, "CMF", color=cmf_value > 0 ? color.green : color.red)

// Calculate Money Flow Index
mfi_value = vol.mfi(14)
hline(80, "Overbought", color=color.red)
hline(20, "Oversold", color=color.green)
plot(mfi_value, "MFI", color=color.yellow)
```

### OBV with Archer Smoothing
```pine
import mihakralj/volume/1 as vol

// Calculate raw OBV
obv_value = vol.obv()
plot(obv_value, "OBV", color=color.white)

// Calculate Archer OBV (smoothed)
[aobv_fast, aobv_slow] = vol.aobv(close, volume)
plot(aobv_fast, "AOBV Fast", color=color.blue)
plot(aobv_slow, "AOBV Slow", color=color.red)
```

### Volume Oscillator Analysis
```pine
import mihakralj/volume/1 as vol

// Calculate Percentage Volume Oscillator
[pvo_line, pvo_signal] = vol.pvo(12, 26, 9)
plot(pvo_line, "PVO", color=color.yellow)
plot(pvo_signal, "Signal", color=color.blue)

// Calculate histogram
histogram = pvo_line - pvo_signal
plot(histogram, "Histogram", style=plot.style_histogram,
     color=histogram >= 0 ? color.green : color.red)
```

### VWAP Analysis
```pine
import mihakralj/volume/1 as vol

// Calculate daily VWAP
is_new_day = ta.change(time('D'))
vwap_value = vol.vwap(hlc3, volume, is_new_day)
plot(vwap_value, "VWAP", color=color.yellow, linewidth=2)

// Calculate VWMA for comparison
vwma_value = vol.vwma(close, volume, 20)
plot(vwma_value, "VWMA(20)", color=color.blue)
```

### Klinger Volume Oscillator
```pine
import mihakralj/volume/1 as vol

// Calculate KVO
[kvo_line, kvo_signal] = vol.kvo(34, 55, 13)
plot(kvo_line, "KVO", color=color.yellow, linewidth=2)
plot(kvo_signal, "Signal", color=color.blue)
hline(0, "Zero", color=color.gray)

// Histogram
histogram = kvo_line - kvo_signal
plot(histogram, "Histogram", style=plot.style_histogram,
     color=histogram >= 0 ? color.green : color.red)
```

### Force Index Analysis
```pine
import mihakralj/volume/1 as vol

// Calculate Elder's Force Index
efi_value = vol.efi(13, close, volume)
plot(efi_value, "EFI", color=color.yellow)
hline(0, "Zero", color=color.gray)

// Calculate Volume Force for comparison
vf_value = vol.vf(14, close, volume)
plot(vf_value, "VF", color=color.blue)
```

### Volume Index Comparison
```pine
import mihakralj/volume/1 as vol

// Calculate both volume indices
nvi_value = vol.nvi(close, volume, 1000)
pvi_value = vol.pvi(close, volume, 1000)

plot(nvi_value, "NVI", color=color.red)
plot(pvi_value, "PVI", color=color.green)

// 255-day EMAs (not included in library, use external EMA)
```

### Price-Volume Divergence
```pine
import mihakralj/volume/1 as vol

// Calculate PVD
pvd_value = vol.pvd(14, 14, 5, close, volume)
plot(pvd_value, "PVD", color=color.yellow)
hline(0, "Zero", color=color.gray)

// Strong divergence zones
hline(50, "Strong Positive", color=color.green, linestyle=hline.style_dashed)
hline(-50, "Strong Negative", color=color.red, linestyle=hline.style_dashed)
```

## Function Categories

### Cumulative Indicators
These indicators maintain running totals that accumulate over the entire chart history:
- **ADL**: Cumulative money flow
- **OBV**: Cumulative directional volume
- **PVT**: Cumulative volume-adjusted price change
- **VA**: Cumulative volume accumulation
- **WAD**: Cumulative Williams A/D
- **TVI**: Cumulative directional volume based on tick direction

### Bounded Oscillators (0-100)
These indicators oscillate within a fixed range:
- **MFI**: Money Flow Index (0-100, overbought >80, oversold <20)

### Unbounded Oscillators
These indicators oscillate around zero with no fixed bounds:
- **ADOSC**: A/D Oscillator (EMA difference)
- **CMF**: Chaikin Money Flow (-1 to +1 typical range)
- **EFI**: Elder's Force Index
- **KVO**: Klinger Volume Oscillator
- **PVO**: Percentage Volume Oscillator
- **VO**: Volume Oscillator
- **VF**: Volume Force
- **PVD**: Price Volume Divergence

### Volume-Weighted Prices
These indicators calculate volume-weighted price averages:
- **VWAP**: Intraday volume-weighted average (session reset)
- **VWMA**: Volume-weighted moving average (fixed period)
- **TWAP**: Time-weighted average price (session reset)

### Ratio/Index Indicators
These indicators track price changes on specific volume conditions:
- **NVI**: Updates only when volume decreases
- **PVI**: Updates only when volume increases
- **PVR**: Classifies price-volume relationships (0-4)

## Performance Characteristics

### O(1) Indicators
Constant-time complexity using circular buffers and running sums:
- CMF, MFI, VO, VWMA, VWAD, EOM, III, PVD

### O(1) with Warmup
EMA-based indicators with warmup compensation:
- AOBV, ADOSC, EFI, KVO, PVO, VF

### Cumulative (State-Based)
Single state variable updated each bar:
- ADL, OBV, NVI, PVI, PVT, TVI, VA, WAD

### Session-Based
Reset on session boundary:
- VWAP, TWAP

## Mathematical Foundations

### Money Flow Multiplier
Used by ADL, CMF, VWAD, WAD:
```
MFM = ((Close - Low) - (High - Close)) / (High - Low)
MFV = MFM × Volume
```

### True Range Components
Used by WAD:
```
TR High = max(High, Close[1])
TR Low = min(Low, Close[1])
```

### Volume Direction
Used by OBV, AOBV, PVT:
```
Direction = sign(Close - Close[1])
Volume Adjustment = Direction × Volume
```

### Typical Price
Used by MFI:
```
Typical Price = (High + Low + Close) / 3
```

## Common Patterns

### Beta Optimization
All EMA-based indicators (AOBV, EFI, PVO, VF, KVO) use precomputed beta:
```pine
float alpha = 2.0 / (period + 1)
float beta = 1.0 - alpha  // Computed once
e *= beta  // Used in warmup loop
```

### Circular Buffer Template
Standard pattern for O(1) sliding windows:
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

### Session Reset Pattern
Used by VWAP and TWAP:
```pine
if reset_condition
    sum := initial_value
    count := initial_count
else
    sum += current
    count += 1
```

## Dependencies

This library has **no external dependencies**. All functions are self-contained and do not rely on the `ta.*` namespace or other libraries.

## Related Libraries

- **trends_FIR**: Simple and weighted moving averages (SMA, WMA, DWMA, HMA)
- **trends_IIR**: Exponential moving averages and derivatives (EMA, DEMA, TEMA, ZLEMA, etc.)
- **volatility**: Volatility indicators (ATR, Bollinger Bands, True Range, etc.)

## References

### Primary Sources
- Chaikin, Marc - Accumulation/Distribution Line and Oscillator
- Wilder, J. Welles (1978) - Money Flow concepts in "New Concepts in Technical Trading Systems"
- Elder, Alexander - Force Index in "Trading for a Living"
- Granville, Joseph (1963) - On Balance Volume
- Klinger, Stephen - Klinger Volume Oscillator
- Archer, David - Archer On-Balance Volume (AOBV)
- Williams, Larry - Williams Accumulation/Distribution

### Technical Analysis Libraries
- pandas-ta (Python) - Volume indicator implementations
- TA-Lib (C) - Technical Analysis Library
- Tulip Indicators (C) - Technical indicators
- ta (Python, Bukosabino) - Technical analysis library

### Documentation
- StockCharts.com - Volume indicator definitions and usage
- Investopedia - Volume analysis and interpretation
- TradingView - Volume indicator documentation

## Version History

### v1 (Current)
- 26 volume indicators with optimized implementations
- Beta precomputation for EMA-based indicators (AOBV, EFI, PVO, VF, KVO)
- O(1) circular buffers for lookback indicators
- Session-based reset support (VWAP, TWAP)
- Comprehensive money flow analysis suite
- No external dependencies

---

**License**: MIT License
**Author**: mihakralj
**Repository**: https://github.com/mihakralj/pinescript
