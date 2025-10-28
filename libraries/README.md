# PineScript Libraries

Optimized, self-contained PineScript libraries for technical analysis. All functions have no external dependencies and are designed for portability and performance.

## Available Libraries

| Library | Functions | Description | Installation |
|---------|-----------|-------------|--------------|
| **[trends_FIR](trends_FIR.md)** | 17 | FIR trend filters (SMA, WMA, ALMA, HMA, LSMA, etc.) | `import mihakralj/trends_FIR/1 as fir` |
| **[trends_IIR](trends_IIR.md)** | 23 | IIR trend filters with beta optimization (EMA, DEMA, KAMA, VIDYA, etc.) | `import mihakralj/trends_IIR/1 as iir` |
| **[volatility](volatility.md)** | 26 | Volatility measures (ATR, HV, Yang-Zhang, Bollinger Width, etc.) | `import mihakralj/volatility/1 as vol` |
| **[volume](volume.md)** | 26 | Volume indicators (OBV, MFI, CMF, VWAP, Klinger, etc.) | `import mihakralj/volume/1 as volume` |
| **[statistics](statistics.md)** | 20 | Statistical functions (correlation, stddev, regression, percentiles, etc.) | `import mihakralj/statistics/1 as stats` |

## Quick Start

```pine
//@version=6
indicator("Library Example")

import mihakralj/trends_IIR/1 as iir
import mihakralj/volatility/1 as vol

// Calculate EMA and ATR
ema20 = iir.ema(close, 20)
atr14 = vol.atr(14)

plot(ema20, "EMA(20)", color.blue)
plot(atr14, "ATR(14)", color.yellow)
```

## Library Categories

### Trend Filters

**[FIR (Finite Impulse Response)](trends_FIR.md)** - Process fixed window with various weighting schemes
- Simple & Weighted (O(1)): SMA, WMA, DWMA
- Composite (O(1)): HMA, HWMA
- Weighted (O(n)): EPMA, TRIMA, SINEMA, PWMA
- Gaussian & DSP (O(n)): ALMA, GWMA, HAMMA, HANMA, BLMA, BWMA
- Advanced (O(n)): LSMA, SGMA, CONV

**[IIR (Infinite Impulse Response)](trends_IIR.md)** - Recursive calculations with O(1) complexity
- Exponential: EMA, DEMA, TEMA, QEMA
- Wilder's: RMA, SMMA
- Zero-Lag: ZLEMA, ZLDEMA, ZLTEMA
- Adaptive: KAMA, FRAMA, McGinley, VIDYA, VAMA, YZVAMA
- Specialized: T3, HEMA, JMA, EHMA, REMA, EPMA, MTF_EMA

### Market Analysis

**[Volatility](volatility.md)** - Comprehensive volatility measurement
- Range-Based: TR, ATR (6 variants)
- Log Return: HV, RV, RSV, GKV, YZV (most efficient)
- Indices: RVI, CCV, CVI, VR, VOV
- Specialized: UI, MASSI, EWMA, JVOLTY

**[Volume](volume.md)** - Price-volume relationships
- Money Flow: ADL, CMF, MFI, WAD, VWAD
- OBV Family: OBV, AOBV, PVO
- Oscillators: KVO, VO, VROC
- Weighted Prices: VWAP, VWMA, TWAP
- Force & Pressure: EFI, VF, VA

**[Statistics](statistics.md)** - Distribution and correlation analysis
- Central Tendency: CUMMEAN, GEOMEAN, HARMEAN, MEDIAN, MODE
- Dispersion: STDDEV, VARIANCE, IQR
- Shape: SKEW, KURTOSIS
- Relationships: CORRELATION, COVARIANCE, BETA
- Regression: LINREG, ZSCORE, BIAS
