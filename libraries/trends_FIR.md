# FIR Trends Library

PineScript library containing 17 self-contained FIR (Finite Impulse Response) trend filter functions with no external dependencies.

## Installation

```pine
import mihakralj/trends_FIR/1 as fir
```

## Available Functions

### Simple & Weighted Moving Averages

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `sma()` | Simple Moving Average | O(1) | Circular buffer, arithmetic mean |
| `wma()` | Weighted Moving Average | O(1) | Dual running sums, cached denominator |
| `dwma()` | Double Weighted MA | O(1) | Two inline O(1) WMA passes |
| `epma()` | Endpoint MA | O(n) | Endpoint emphasis weighting |

### Triangular & Shaped Moving Averages

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `trima()` | Triangular MA | O(n) | Triangular weighting |
| `sinema()` | Sine-weighted MA | O(n) | Sine wave weighting |
| `pwma()` | Pascal Weighted MA | O(n) | Pascal's triangle coefficients |

### Gaussian & Window Function Moving Averages

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `alma()` | Arnaud Legoux MA | O(n) | Gaussian distribution, configurable offset/sigma |
| `gwma()` | Gaussian-Weighted MA | O(n) | Gaussian window, configurable sigma |
| `hamma()` | Hamming MA | O(n) | Hamming window (DSP filter) |
| `hanma()` | Hanning MA | O(n) | Hanning window (DSP filter) |
| `blma()` | Blackman MA | O(n) | Blackman window (DSP filter) |
| `bwma()` | Bessel-Weighted MA | O(n) | Bessel window, configurable order |

### Composite & Advanced Moving Averages

| Function | Description | Complexity | Key Feature |
|----------|-------------|------------|-------------|
| `hma()` | Hull MA | O(1) | Three O(1) WMA calculations, lag reduction |
| `hwma()` | Holt-Winters MA | O(1) | Triple exponential with velocity/acceleration |
| `lsma()` | Least Squares MA | O(n) | Linear regression fit |
| `sgma()` | Savitzky-Golay MA | O(n) | Polynomial fitting, preserves peaks |
| `conv()` | Convolution MA | O(n) | Custom kernel convolution |
