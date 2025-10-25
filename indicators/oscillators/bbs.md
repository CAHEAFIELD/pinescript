# BBS: Bollinger Band Squeeze

[Pine Script Implementation of BBS](https://github.com/mihakralj/pinescript/blob/main/indicators/oscillators/bbs.pine)

## Overview and Purpose

The Bollinger Band Squeeze (BBS), also known as the TTM Squeeze, is a volatility-based technical indicator developed by John Carter of Simpler Trading. It identifies periods of unusually low volatility that often precede significant price movements. The indicator combines two popular technical tools—Bollinger Bands and Keltner Channels—to detect market consolidation phases (the "squeeze") that typically lead to explosive breakouts in either direction. By signaling when markets transition from quiet consolidation to active trending, the BBS helps traders anticipate major price movements and position themselves before breakouts occur.

The indicator's power lies in its ability to quantify the market's state of compression. When volatility contracts to extremely low levels, price action becomes constrained within a narrow range, much like a coiled spring storing energy. The BBS identifies these compression periods and alerts traders to prepare for the inevitable expansion that follows. This makes it particularly valuable for swing traders and position traders who seek to capture substantial moves while avoiding the choppy, directionless price action that characterizes low-volatility consolidation.

## Core Concepts

* **Volatility Compression:** The squeeze occurs when Bollinger Bands (which measure price volatility) contract inside Keltner Channels (which measure average price range), indicating that 95% of price action is contained within 1.5 ATR of the moving average—an unusually tight condition.

* **Mean Reversion vs. Breakout:** While Bollinger Bands are often used for mean reversion strategies, the BBS specifically identifies periods when mean reversion is unlikely and breakouts become probable, representing a paradigm shift in market behavior.

* **Leading Indicator:** Unlike many technical indicators that lag price action, the BBS provides advance warning of potential volatility expansion, giving traders time to prepare positions before major moves begin.

* **Directional Neutrality:** The squeeze itself doesn't predict breakout direction—it only signals that a significant move is likely. Traders must use additional analysis (trend context, support/resistance, momentum indicators) to determine probable direction.

* **Market Efficiency:** The squeeze represents periods of market equilibrium where buyers and sellers are balanced. The eventual breakout reflects which side gains control, often triggered by fundamental catalysts or technical breakdowns.

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Bollinger Band Period | 20 | Controls the lookback window for volatility calculation | Increase to 30-50 for less sensitive, longer-term squeezes; decrease to 10-15 for short-term day trading applications |
| BB StdDev Multiplier | 2.0 | Determines the width of Bollinger Bands (95% of price action) | Rarely adjusted; higher values (2.5-3.0) make squeezes less frequent but more significant |
| Keltner Channel Period | 20 | Controls the ATR calculation period for channel width | Should typically match BB period for consistent comparison; adjust both together |
| KC ATR Multiplier | 1.5 | Determines Keltner Channel width based on ATR | Original Keltner method uses 1.5; increase to 2.0 for tighter squeeze detection |
| Source | Close | Price data used for calculations | Change to HLC3 or OHLC4 for smoother signals in volatile markets |

**Pro Tip:** The most reliable squeezes occur when multiple timeframes align. If daily and 4-hour charts both show squeezes simultaneously, the eventual breakout tends to be stronger and more sustained. Use the indicator on your primary trading timeframe, then confirm with at least one higher timeframe before taking positions.

## Calculation and Mathematical Foundation

**Simplified explanation:**
The BBS compares two volatility measurements: Bollinger Bands (based on standard deviation) and Keltner Channels (based on Average True Range). When the Bollinger Bands contract to the point where both the upper and lower bands fit inside the Keltner Channels, a squeeze is triggered. This condition indicates that statistical price dispersion (Bollinger Bands) has become smaller than the average price range (Keltner Channels)—a rare occurrence that signals extremely low volatility.

**Technical formula:**

Step 1: Calculate Bollinger Bands
```
Middle Band = SMA(close, 20)
Standard Deviation = √(Σ(close - SMA)² / 20)
Upper BB = Middle Band + (2 × Standard Deviation)
Lower BB = Middle Band - (2 × Standard Deviation)
```

Step 2: Calculate Keltner Channels
```
True Range = max(high - low, |high - close[1]|, |low - close[1]|)
ATR = EMA(True Range, 20)
Middle KC = SMA(close, 20)
Upper KC = Middle KC + (1.5 × ATR)
Lower KC = Middle KC - (1.5 × ATR)
```

Step 3: Detect Squeeze Condition
```
Squeeze ON = (Upper BB < Upper KC) AND (Lower BB > Lower KC)
```

Step 4: Calculate Bandwidth (optional, for intensity measurement)
```
BandWidth = ((Upper BB - Lower BB) / Middle Band) × 100
```

> 🔍 **Technical Note:** The indicator uses exponential warmup compensation for the ATR calculation to ensure valid output from the first bar. The Bollinger Bands employ count-based warmup for the SMA and standard deviation calculations. Both techniques ensure accurate squeeze detection even during the initial bars of data, following the repository's pattern of delivering reliable indicators from bar 1.

## Interpretation Details

Understanding how to interpret the BBS signals is crucial for successful trading:

**Squeeze Phases:**

* **Squeeze ON (Red Dots):** Volatility has contracted to very low levels. Price is consolidating, and both buyers and sellers are in equilibrium. This is typically NOT the time to enter new positions—instead, prepare watchlists and entry strategies for the coming breakout. The longer the squeeze persists, the more significant the eventual move tends to be.

* **Squeeze OFF (Green Dots):** Volatility is expanding, indicating that a directional move may have begun or is in progress. The first green dot after a series of red dots often signals the start of a new trend. This is when traders should look for entry opportunities in the direction of the breakout.

* **Background Coloring:** A subtle red background tint appears during squeeze conditions to make the consolidation phase immediately visible on the chart, helping traders avoid entering choppy markets.

**Determining Breakout Direction:**

The squeeze itself doesn't indicate direction—additional analysis is required:

* **Trend Context:** If the squeeze forms within an established uptrend, upside breakouts are more probable. Conversely, squeezes in downtrends favor downside breakouts.

* **Support/Resistance:** Squeezes occurring near major support levels favor upside breaks; near resistance, downside breaks are more likely.

* **Volume Analysis:** Watch for volume to increase during the breakout. Low-volume breakouts often fail; high-volume confirms genuine momentum.

* **Momentum Indicators:** Use RSI, MACD, or momentum oscillators. If momentum builds positively during the squeeze, expect an upside break. Negative momentum divergence suggests downside.

**Trading Strategies:**

* **Conservative Approach:** Wait for the squeeze to fire (green dot appears) and price to break a significant support/resistance level before entering. This reduces false breakout risk but may result in less favorable entry prices.

* **Aggressive Approach:** Enter during the squeeze when direction becomes clear through volume, momentum, or technical level breaks. This provides better entry prices but higher risk of false signals.

* **Multiple Timeframe Confirmation:** Only trade squeezes when multiple timeframes (e.g., daily and 4-hour) align. This significantly improves win rate.

**Head Fake Warning:**

John Bollinger notes that "head fake" breakouts are common—price may initially break in one direction, then quickly reverse and move strongly in the opposite direction. This phenomenon traps traders who enter too quickly. Use stop-losses and wait for confirmation before committing significant capital.

## Limitations and Considerations

* **No Directional Information:** The squeeze signals low volatility and potential breakouts but provides no information about breakout direction. Traders must use additional analysis to determine whether to go long or short, which can be challenging in directionless markets.

* **False Breakouts:** Not every squeeze leads to a sustained directional move. False breakouts are common, especially in ranging markets or near major news events. Always use stop-losses and wait for confirmation through volume, follow-through, or technical level breaks.

* **Squeeze Duration:** The indicator cannot predict how long a squeeze will last. Some squeezes resolve quickly (within days), while others persist for weeks or months. Patience is required, and traders must avoid forcing trades during extended consolidation periods.

* **Market Conditions Matter:** The BBS works best during periods transitioning from low to high volatility. In persistently high-volatility environments (market crashes, major news cycles), squeezes become rare and less reliable. Similarly, in extremely low-volatility environments, multiple false squeezes may occur.

* **Parameter Sensitivity:** While default settings (20, 2.0, 1.5) work well for most markets, different securities and timeframes may require optimization. Overfitting to historical data should be avoided—parameters should make logical sense for the asset's typical behavior.

* **Lagging Nature:** Despite being a relatively early signal, the squeeze still requires price to move significantly before firing. By the time green dots appear, some of the initial move may have already occurred. This is the trade-off for waiting for confirmation.

## References

- Carter, J. (2010). *Mastering the Trade: Proven Techniques for Profiting from Intraday and Swing Trading Setups*. McGraw-Hill Education.
- Bollinger, J. (2002). *Bollinger on Bollinger Bands*. McGraw-Hill Education.
- Keltner, C. W. (1960). *How to Make Money in Commodities*. The Keltner Statistical Service.
- StockCharts.com. "Bollinger Band Squeeze." ChartSchool Technical Analysis.
- StockCharts.com. "TTM Squeeze." ChartSchool Technical Indicators.
