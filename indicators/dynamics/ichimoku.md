# ICHIMOKU: Ichimoku Cloud

[Pine Script Implementation of ICHIMOKU](https://github.com/mihakralj/pinescript/blob/main/indicators/dynamics/ichimoku.pine)

## Overview and Purpose

The Ichimoku Cloud (Ichimoku Kinko Hyo, meaning "one glance equilibrium chart") is a comprehensive technical analysis system developed by Japanese journalist Goichi Hosoda in the late 1930s and published in 1969. Unlike most technical indicators that focus on a single aspect of price action, Ichimoku provides a complete view of market dynamics including trend direction, momentum, support and resistance levels, and timing signals—all displayed simultaneously on a single chart.

The system consists of five distinct components that work together to create a multi-dimensional analysis framework. The visual "cloud" (Kumo) formed between two of these components provides an immediately recognizable representation of support/resistance zones and trend strength. This holistic approach makes Ichimoku particularly valuable for traders seeking a complete market picture without needing to reference multiple separate indicators.

Ichimoku remains widely used in Japanese markets and has gained international popularity among forex and cryptocurrency traders. Its self-contained nature and clear visual signals make it suitable for both discretionary traders and algorithmic systems. The indicator's time-shifted components provide both historical context and forward-looking projections, offering a unique temporal perspective on market equilibrium.

## Core Concepts

* **Donchian Midpoint Foundation:** All Ichimoku lines (except Chikou) are calculated as Donchian midpoints—the average of the highest high and lowest low over a specified period. This creates natural support/resistance levels based on price equilibrium.

* **Multi-Timeframe Structure:** Uses three different periods (9, 26, 52) representing short, medium, and long-term price equilibrium. These periods were originally designed for Japanese markets (6-day trading week) but work effectively across all markets.

* **Time Displacement:** Leading Spans (Senkou A and B) are projected 26 periods forward, while Lagging Span (Chikou) is shifted 26 periods backward. This creates a unique temporal framework showing past price action, current levels, and projected future zones simultaneously.

* **The Cloud (Kumo):** The filled area between Senkou Span A and B represents dynamic support/resistance. Cloud thickness indicates strength—thicker clouds are harder to penetrate. Cloud color (green when A > B, red when B > A) signals overall trend direction.

* **Equilibrium Philosophy:** The system is built on the concept that markets constantly seek equilibrium. Lines represent equilibrium levels at different timeframes, crossovers signal shifts in equilibrium, and the cloud shows where future equilibrium zones may form.

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|---------------|
| Tenkan Period | 9 | Conversion Line lookback (short-term equilibrium) | Increase for slower markets; decrease for faster intraday trading |
| Kijun Period | 26 | Base Line lookback (medium-term equilibrium) | Adjust proportionally with Tenkan (typically 3:1 ratio) |
| Senkou B Period | 52 | Leading Span B lookback (long-term equilibrium) | Double the Kijun period to maintain harmonic relationship |
| Displacement | 26 | Forward/backward shift for cloud and Chikou | Match to Kijun period for proper proportions |

**Pro Tip:** The 9-26-52 default settings work across most markets and timeframes. However, for cryptocurrency or forex trading on shorter timeframes (5-minute to 1-hour charts), consider using 20-60-120-30 for more responsive signals while maintaining the same mathematical relationships (1:3:6 ratio for the three periods).

## Calculation and Mathematical Foundation

**Explanation:**
Ichimoku calculates five separate lines using Donchian midpoint methodology. The Donchian midpoint finds the average of the highest high and lowest low over a period, creating a natural equilibrium level. Three lines use this calculation directly with different periods, one line combines two of these lines, and one line simply plots closing price with time displacement.

**Technical formula:**

```
Step 1: Calculate Tenkan-sen (Conversion Line)
Tenkan-sen = (Highest High over 9 periods + Lowest Low over 9 periods) / 2

Step 2: Calculate Kijun-sen (Base Line)
Kijun-sen = (Highest High over 26 periods + Lowest Low over 26 periods) / 2

Step 3: Calculate Senkou Span A (Leading Span A)
Senkou Span A = (Tenkan-sen + Kijun-sen) / 2
Plot 26 periods ahead

Step 4: Calculate Senkou Span B (Leading Span B)
Senkou Span B = (Highest High over 52 periods + Lowest Low over 52 periods) / 2
Plot 26 periods ahead

Step 5: Calculate Chikou Span (Lagging Span)
Chikou Span = Current Close
Plot 26 periods behind
```

> 🔍 **Technical Note:** The implementation uses O(n) loops to find highest/lowest values for each period, where n is the period length (9, 26, or 52). While pattern §35 (deque min/max) could optimize this to O(n) amortized, the straightforward loop approach provides clarity and maintainability for this multi-component indicator. Total complexity is O(52) per bar, which is acceptable for real-time chart updates.

## Interpretation Details

**Primary Signal: Tenkan/Kijun Cross**
- **Bullish Cross (TK Cross):** Tenkan-sen crosses above Kijun-sen—signals shift to bullish momentum. Strongest when occurring above the cloud.
- **Bearish Cross:** Tenkan-sen crosses below Kijun-sen—signals shift to bearish momentum. Strongest when occurring below the cloud.
- **Flat Kijun:** When Kijun-sen is horizontal, it acts as a strong support/resistance level representing medium-term equilibrium.

**Cloud Analysis (Kumo)**
- **Above Cloud:** Price trading above cloud indicates bullish trend. Cloud provides support. Top of cloud is first support, bottom is second support.
- **Below Cloud:** Price trading below cloud indicates bearish trend. Cloud provides resistance. Bottom of cloud is first resistance, top is second resistance.
- **Inside Cloud:** Price within cloud indicates consolidation or trend transition. No clear directional bias—wait for breakout.
- **Cloud Twist:** When Senkou A and B cross, cloud changes color—signals potential trend reversal. Thicker clouds at twist provide stronger support/resistance.

**Chikou Span Confirmation**
- **Bullish Confirmation:** Chikou Span above price 26 bars ago confirms bullish trend and momentum.
- **Bearish Confirmation:** Chikou Span below price 26 bars ago confirms bearish trend and momentum.
- **Chikou in Cloud:** When Chikou is trapped in historical cloud, current price movement lacks confirmation—trade cautiously.

**Advanced Pattern: Perfect Ichimoku Buy/Sell**
- **Perfect Buy:** (1) Price above cloud, (2) Tenkan above Kijun, (3) Chikou above price 26 bars ago, (4) Senkou A above Senkou B (green cloud). All elements aligned bullishly.
- **Perfect Sell:** All four conditions reversed—complete bearish alignment.

**Time Displacement Strategy**
- **Leading Spans:** Show projected support/resistance 26 bars ahead. Useful for setting profit targets and anticipating future pressure zones.
- **Chikou Span:** Compares current price to 26-bar historical price. Acts as momentum gauge—strong trending markets show Chikou far from historical price.

## Limitations and Considerations

- **Lagging Nature:** Despite the "cloud ahead" visual, Ichimoku is fundamentally a lagging indicator based on historical highs/lows. All components except displacement are reactive to past price action, not predictive.

- **Sideways Market Weakness:** In range-bound or choppy markets, Ichimoku generates frequent false signals. The cloud becomes thin and twists repeatedly, Tenkan/Kijun cross frequently without follow-through, and Chikou oscillates around historical price levels.

- **Complex Visual Interpretation:** The five lines plus cloud fill create visual complexity. Beginners may struggle with information overload. Takes practice to rapidly assess all components "at one glance" as intended.

- **Period Optimization Challenge:** The default 9-26-52 settings were designed for Japanese markets with 6-day trading weeks. While they work universally, they may not be optimal for specific instruments or timeframes. However, adjusting periods breaks the harmonic relationship and requires extensive testing.

- **Time Displacement Confusion:** Forward and backward shifting of lines can confuse traders unfamiliar with the system. Current bar's cloud position shows support/resistance calculated 26 bars ago, not current levels—this temporal offset requires mental adjustment.

- **Best Performance:** Works best in trending markets with clear directional bias. Excels at confirming trend strength and providing support/resistance in sustained moves. Particularly effective for swing trading and position trading over multi-day to multi-week periods.

## References

* Hosoda, G. (1969). Ichimoku Kinko Studies. (Original Japanese publication)
* Patel, M. (2010). *Trading with Ichimoku Clouds: The Essential Guide to Ichimoku Kinko Hyo Technical Analysis*. Wiley Trading.
* Murphy, J. J. (1999). *Technical Analysis of the Financial Markets*. New York Institute of Finance.

## Validation Sources

**Patterns:** §35 (deque_minmax considered), §11 (multi_return), §7 (na_handling), §6 (defensive_division), §3 (count_warmup)

**External Libraries:** Fidelity Investments technical guide, Investopedia definition, Python pandas implementations (multiple sources confirmed), Medium implementation articles

**Formulas Validated:** Tenkan-sen = (9-high + 9-low)/2, Kijun-sen = (26-high + 26-low)/2, Senkou A = (Tenkan + Kijun)/2 shifted +26, Senkou B = (52-high + 52-low)/2 shifted +26, Chikou = close shifted -26

**Sequential Thinking:** 8-phase analysis covering complexity (O(n) acceptable), displacement handling (PineScript offset), function structure (helper + main), warmup strategy (count-based), NA handling (historical references), plot strategy (5 lines + cloud fill), implementation plan confirmed

**MCP Validation:** qdrant-find (patterns retrieved), sequential-thinking (8 phases completed), tavily-search (5 external sources confirmed), ref-tools (PineScript v6 API verified)
