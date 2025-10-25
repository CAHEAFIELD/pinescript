# CFB: Composite Fractal Behavior

[Pine Script Implementation of CFB](https://github.com/mihakralj/pinescript/blob/main/indicators/cycles/cfb.pine)

## Overview and Purpose

The Composite Fractal Behavior (CFB) indicator is an advanced cycle detection tool that analyzes fractal patterns across multiple depth levels to identify the dominant cycle period in price data. Developed as an extension of fractal analysis techniques, CFB examines how price behavior changes at different time scales, from short-term fluctuations to longer-term patterns, and combines these observations into a single composite measurement.

Unlike single-scale cycle detectors, CFB simultaneously analyzes multiple fractal depths (exponentially increasing lookback periods) and weights them according to their relative strength and consistency. This multi-scale approach provides a more robust cycle period estimate that adapts to changing market conditions and is less susceptible to noise at any single time scale.

The indicator is particularly valuable for adaptive trading systems where indicator parameters need to adjust to current market rhythm, and for identifying when market structure shifts from one dominant cycle to another.

## Core Concepts

* **Fractal Depth Analysis**: Examination of price patterns at multiple time scales using cumulative exponential periods (2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96... bars)
* **Weighted Difference Calculation**: Measures the cumulative difference between consecutive price points, with more weight given to recent differences, creating a fractal behavior signature
* **Level Comparison**: Compares the current price level against the average level over the depth period to assess deviation from the center
* **Multi-Depth Composite**: Combines fractal behavior measurements from all depth levels, with adaptive weighting based on each level's consistency and strength
* **Jurik Smoothing**: Applies double-pass Jurik Moving Average (JMA) for final smoothing, providing low-lag, adaptive filtering of the composite cycle period

## Common Settings and Parameters

| Parameter | Default | Function | When to Adjust |
|-----------|---------|----------|----------------|
| Source | hlcc4 | Price data to analyze | Use close for end-of-bar signals, hlcc4 for smoother intrabar analysis |
| CFB Depth | 10 | Maximum depth level (1-10) | Lower (3-5) for faster markets, higher (8-10) for slower trending markets |
| Smooth Length | 8 | EMA period for depth smoothing | Lower (3-5) for more responsive, higher (10-15) for more stable readings |
| JMA Period | 5 | Jurik smoothing period | Lower (3-5) for faster response, higher (7-10) for smoother output |
| JMA Phase | 0 | Phase adjustment (-100 to 100) | Negative for leading, positive for lagging behavior |

**Pro Tip:** Start with default settings and observe how the indicator responds to your market. If readings are too erratic, increase Smooth Length or JMA Period. If the indicator lags too much behind market changes, reduce these parameters or lower the CFB Depth to focus on shorter-term patterns.

## Calculation and Mathematical Foundation

**Simplified explanation:**
CFB calculates a fractal behavior score at multiple depth levels by measuring weighted price differences and level deviations. These scores are smoothed, weighted by consistency, and combined into a composite value representing the dominant cycle period.

**Technical formula:**

1. **Period array initialization** (executed once):
   ```
   For i = 0 to (maxDepth × 2 - 1):
     powerIdx = floor(i / 2)
     increment = 2^powerIdx
     period[i] = cumSum
     cumSum += increment
   
   Example with maxDepth=10:
   i=0,1: periods 2,3 (increment=2^0=1)
   i=2,3: periods 4,6 (increment=2^1=2)
   i=4,5: periods 8,12 (increment=2^2=4)
   i=6,7: periods 16,24 (increment=2^3=8)
   i=8,9: periods 32,48 (increment=2^4=16)
   ...continuing to 20 total depths
   ```

2. **For each period p in the periods array**:
   
   a. **Circular buffer management** (shared across all depths, O(1) updates):
   ```
   buffer[head] = current_price
   head = (head + 1) % maxBufferSize
   count = min(count + 1, maxBufferSize)
   effectiveDepth = min(d, count - 1)
   ```

   b. **Calculate weighted difference sum**:
   ```
   sumWeighted = Σ(i=0 to effectiveDepth-1) |price[i] - price[i+1]| × (effectiveDepth - i)
   sumLevel = Σ(i=0 to effectiveDepth-1) price[i+1]
   ```

   c. **Calculate level comparison**:
   ```
   levelCompare = |effectiveDepth × currentPrice - sumLevel|
   ```

   d. **Compute fractal behavior ratio with warmup compensation**:
   ```
   ratio = levelCompare / sumWeighted  (if sumWeighted > 0)
   compensator = (effectiveDepth + 1) / (depth + 1)
   CFB_aux[d] = ratio × compensator
   ```

3. **Smooth each depth's CFB value** using exponential moving average:
   ```
   alpha = 2 / (length + 1)
   smoothed[d] = alpha × CFB_aux[d] + (1 - alpha) × smoothed[d-1]
   ```

4. **Apply adaptive weighting** (alternating odd/even depth levels):
   ```
   factorA = 1.0, factorB = 1.0
   For even depths: weighted[d] = factorB × smoothed[d], factorB *= (1 - smoothed[d])
   For odd depths:  weighted[d] = factorA × smoothed[d], factorA *= (1 - smoothed[d])
   ```

5. **Calculate composite period** as weighted average:
   ```
   sumWeightedSq = Σ (weighted[d])² × period[d]
   sumSq = Σ (weighted[d])²
   compositePeriod = sumWeightedSq / sumSq  (if sumSq > 0)
   ```

6. **Apply double-pass Jurik smoothing**:
   ```
   cfb_smoothed = JMA(JMA(compositePeriod, period, phase), period, phase)
   cfb_final = max(1, ceil(cfb_smoothed))
   ```

> 🔍 **Technical Note:** The warmup compensator `(effectiveDepth + 1) / (depth + 1)` ensures valid output from bar 1 by scaling the ratio based on how much data is available. The alternating weight calculation (factorA/factorB) prevents any single depth level from dominating the composite result, creating a more balanced multi-scale analysis. The double-pass JMA provides exceptionally smooth output while maintaining low lag.

## Interpretation Details

CFB provides adaptive cycle period measurement with multiple applications:

* **Cycle Period Identification:**
  - Values 1-20 bars: Very short-term cycles, high-frequency price action
  - Values 20-50 bars: Short to medium-term cycles, typical intraday to daily swings
  - Values 50-100 bars: Medium to long-term cycles, weekly to monthly patterns
  - Values > 100 bars: Long-term cycles or trending behavior
  - Stable readings indicate consistent cyclical market structure
  - Rapidly changing readings suggest transitional periods or regime changes

* **Adaptive Indicator Optimization:**
  - Use detected period as input for other indicators' lookback lengths
  - Example: If CFB = 25, use 25-period moving averages, 25-period RSI
  - Automatically synchronizes indicators with current market rhythm
  - Particularly effective for oscillators and momentum indicators

* **Market Structure Assessment:**
  - Low CFB values (< 20): Fast-moving, high-frequency dominated market
  - Medium CFB values (20-50): Normal cycling behavior with clear swings
  - High CFB values (50-100): Slower cycles, longer-term structure
  - Very high CFB values (> 100): Strong trending or low-volatility consolidation
  - Increasing CFB: Market transitioning to longer-term patterns
  - Decreasing CFB: Market becoming more reactive and short-term focused

* **Trading System Adaptation:**
  - Short cycles (< 20): Use fast indicators, tight stops, quick profit targets
  - Medium cycles (20-50): Standard trading approaches, moderate timeframes
  - Long cycles (50-100): Wider stops, patience required, trend-following bias
  - Very long cycles (> 100): Position trading, reduced trading frequency

* **Reference Levels:**
  - Below 20 (Low Trend line): Very short-term structure, high noise potential
  - Around 50 (Medium Trend line): Balanced market with moderate cycle length
  - Above 100 (High Trend line): Extended cycles, trending or consolidating markets

## Limitations and Considerations

* **Computational Complexity**: O(n²) complexity due to nested loops across multiple depths means calculation time increases with depth parameter; not as efficient as single-scale cycle detectors
* **Initialization Period**: Requires significant data (100+ bars recommended) before producing fully stable readings due to multiple smoothing stages and depth accumulation
* **Smoothing Lag**: The combination of EMA smoothing per depth plus double-pass JMA creates substantial smoothing, introducing lag in detecting rapid cycle changes
* **Depth Parameter Sensitivity**: Results can vary significantly with different maxDepth settings; too low misses longer cycles, too high may include noise or irrelevant patterns
* **Trending vs Cycling**: Like all cycle detectors, CFB assumes cyclical behavior exists; during strong trending periods with minimal retracements, readings may be less meaningful
* **Not a Trading Signal**: CFB identifies cycle period, not cycle phase or entry points; must be combined with other indicators for actual trading decisions
* **Buffer Size Calculation**: Internal buffer size grows exponentially with depth (approximately 2^maxDepth), consuming more memory at higher depth settings

## References

* Fractal analysis techniques in financial markets
* Ehlers, J. F. (2013). *Cycle Analytics for Traders*. Wiley Trading. (General cycle detection methodology)
* Multi-scale analysis in technical indicators
* Jurik, M. (2006). Jurik Moving Average - adaptive smoothing techniques

## Validation Sources

**Patterns**: §1 (circular_buffer), §5 (state_management), §6 (defensive_division), §7 (na_handling), §12 (input_validation), §13 (constant_precomputation)

**Formulas**: Multi-depth fractal analysis with weighted difference calculation, level comparison, and composite weighting

**External**: Concept inspired by fractal analysis and multi-scale cycle detection techniques; implementation is original

**API**: ref-tools verified input.source, input.int signatures and parameter validation

**Planning**: Phases = validation, fix_critical_issues, add_warmup, document
