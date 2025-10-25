# NATR: Normalized Average True Range

[Pine Script Implementation of NATR](https://github.com/mihakralj/pinescript/blob/main/indicators/volatility/natr.pine)

## Overview and Purpose

Normalized Average True Range (NATR) expresses the Average True Range as a percentage of the closing price, enabling direct volatility comparison across different securities, price levels, and timeframes. While J. Welles Wilder's original ATR (1978) measures volatility in absolute price units, NATR addresses a critical limitation: a $5 ATR means something very different for a $10 stock versus a $500 stock. By dividing ATR by the close price and multiplying by 100, NATR creates a scale-independent volatility metric that traders can use to rank markets, normalize position sizing, and compare historical volatility across different time periods.

NATR gained widespread adoption in the late 1990s and early 2000s as traders managing multi-asset portfolios needed a standardized way to compare volatility. A 2% NATR indicates that average daily volatility is 2% of the security's price, regardless of whether that security trades at $1 or $1,000. This normalization makes NATR particularly valuable for cross-market analysis, volatility screening, and adaptive trading systems that need to adjust to relative rather than absolute volatility.

## Core Concepts

* **Price-normalized volatility** — expresses ATR as percentage of close price for universal comparison
* **Cross-market comparability** — enables ranking securities by relative volatility regardless of price level
* **Scale-independent** — 2% NATR means the same thing for $10 and $1000 stocks
* **Regime detection** — identifies transitions between low and high volatility environments
* **Adaptive parameter input** — provides volatility measure suitable for normalizing trading parameters

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Length | 14 | ATR smoothing period using Wilder's method | Shorten (7-10) for faster response; lengthen (20-30) for smoother values |
| Source | HLC | Price data for True Range calculation | Rarely changed; standard definition uses high, low, close |

**Pro Tip:** Use NATR > 3% as a threshold to identify high-volatility securities suitable for breakout strategies, and NATR < 1% to identify low-volatility securities better suited for mean-reversion approaches. Many institutional traders use 20-period NATR to rank their watchlist and focus on the top 20% most volatile symbols.

## Calculation and Mathematical Foundation

**Simplified explanation:**  
First calculate the Average True Range using Wilder's smoothing method over the specified period. Then divide this ATR value by the current close price and multiply by 100 to express as a percentage. This normalization removes the price-scale dependency while preserving the volatility characteristics.

**Technical formula:**

```
Step 1: Calculate True Range
TR = max(H - L, |H - C[1]|, |L - C[1]|)

Step 2: Apply Wilder's smoothing to TR
ATR = Wilder_RMA(TR, length)
where Wilder_RMA uses alpha = 1/length

Step 3: Normalize by close price
NATR = (ATR / Close) × 100
```

Where:
- H = current high
- L = current low  
- C[1] = previous close
- Close = current close price
- length = smoothing period (default 14)

> 🔍 **Technical Note:** The implementation uses Wilder's original RMA smoothing with alpha = 1/period rather than the more common EMA formula. This preserves consistency with Wilder's original ATR definition. Exponential warmup compensation ensures valid output from bar 1. Division by zero is protected with a ternary operator checking if close != 0.

## Interpretation Details

**Volatility thresholds (general guidelines):**
- **NATR < 1%** — Very low volatility; consolidation phase, tight ranges
- **NATR 1-2%** — Normal to moderate volatility; typical market conditions  
- **NATR 2-3%** — Elevated volatility; trending or expanding ranges
- **NATR > 3%** — High volatility; strong trends, news-driven moves, potential reversals

**Cross-market comparison:**
- Screen multiple securities and rank by NATR to identify relative volatility
- Compare current NATR to historical NATR for the same security to detect regime changes
- Portfolio allocation: weight positions inversely to NATR for volatility parity

**Trading applications:**
- **Breakout strategies** — favor securities with NATR > 2% showing expanding volatility
- **Mean reversion** — favor securities with NATR < 1% in consolidation
- **Stop-loss sizing** — multiply NATR by close price to get appropriate stop distance
- **Position sizing** — reduce size proportionally as NATR increases to maintain consistent risk

**Timeframe considerations:**
- **Intraday** (1-minute to 1-hour): NATR typically 0.5-2%
- **Daily**: NATR typically 1-3%
- **Weekly**: NATR typically 2-5%
- Higher timeframes generally show higher NATR percentages

## Limitations and Considerations

* **Lagging indicator** — NATR inherits ATR's lag from Wilder smoothing; responds slowly to sudden volatility changes
* **Close price dependency** — extreme price moves or gaps in close price can temporarily distort NATR
* **Corporate actions** — stock splits, dividends, and mergers require adjustment for historical comparisons
* **Market-specific ranges** — different asset classes have different "normal" NATR ranges; compare within asset class
* **Not a directional indicator** — measures volatility magnitude but provides no information about trend direction
* **Percentage can be misleading** — penny stocks may show inflated NATR; use absolute price filters first

**Differences from related indicators:**
- **vs ATR** — NATR normalizes by price; ATR in absolute units
- **vs ATRN** — **Critical difference:** NATR = (ATR / close) × 100 normalizes ATR as percentage of current price for cross-market comparison. ATRN = (ATR - min(ATR)) / (max(ATR) - min(ATR)) normalizes ATR to [0,1] scale based on its own historical range (similar to Stochastic of ATR). Use NATR to compare volatility across different securities; use ATRN to compare current volatility to historical volatility of same security.
- **vs ATRP** — ATRP is identical to NATR (alternative naming in some platforms)
- **vs Bollinger Band Width** — BBW uses standard deviation; NATR uses True Range

**Complementary indicators:**
- Bollinger Bands (volatility envelope)
- ATR (absolute volatility for stop placement)
- ADX (trend strength)
- Historical Volatility (variance-based alternative)

## References

* Wilder, J. W. Jr. (1978). *New Concepts in Technical Trading Systems*. Trend Research.
* Kaufman, P. J. (2013). *Trading Systems and Methods* (5th ed.). Wiley.
* Pring, M. J. (2002). *Technical Analysis Explained* (4th ed.). McGraw-Hill.

## Validation Sources

**Patterns:** §2 (Exponential Warmup), §6 (Defensive Division), §7 (NA Handling), §10 (Wilder's Smoothing), §31 (True Range Calculation)  
**External:** pandas-ta NATR, TA-Lib NATR, Tulip Indicators NATR, QuantConnect NATR  
**Formula verified:** NATR = (ATR / close) × 100 confirmed across all libraries  
**Wolfram:** Wilder smoothing alpha = 1/period verified  
**API:** PineScript input.int, math.max, math.abs, nz() confirmed
