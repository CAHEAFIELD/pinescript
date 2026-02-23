# EA.HF Scalper v1.2 — End User Instructions

> **Version:** 1.2 · **Platform:** TradingView (Pine Script v6) · **Designed for:** 1m – 15m scalping

---

## Table of Contents

1. [What this indicator does](#1-what-this-indicator-does)
2. [Installation](#2-installation)
3. [Which chart to load it on](#3-which-chart-to-load-it-on)
4. [Reading the dashboard](#4-reading-the-dashboard)
5. [Reading the candle colours](#5-reading-the-candle-colours)
6. [Reading the TP / SL levels](#6-reading-the-tp--sl-levels)
7. [Settings reference (highest to lowest impact)](#7-settings-reference-highest-to-lowest-impact)
8. [Typical trade workflow](#8-typical-trade-workflow)
9. [Advisory telemetry for .algo bots](#9-advisory-telemetry-for-algo-bots)
10. [Frequently asked questions](#10-frequently-asked-questions)
11. [Known limitations & risk disclaimer](#11-known-limitations--risk-disclaimer)

---

## 1. What this indicator does

**EA.HF Scalper v1.2** is a **multi-timeframe confluence engine** for short-duration scalp trades.
Five indicators run silently in the background and vote on direction and strength.
You see three output layers on the chart, plus optional advisory telemetry:

```
┌─────────────────────────────────────────────────────────────┐
│  LAYER 1 – Dashboard table (configurable position)          │
│    Shows BUY / SELL + confidence % for M1/M2/M5/M15/COMB   │
│                                                             │
│  LAYER 2 – Gradient candle colours                          │
│    Deep green (strong bull) ← Amber (neutral) → Deep red   │
│                                                             │
│  LAYER 3 – TP1 / TP2 / TP3 + SL lines                      │
│    Dynamic levels drawn to the right of the last bar        │
│                                                             │
│  LAYER 4 – Advisory Telemetry (opt-in, Data Window only)    │
│    Component scores + JSON alert for .algo bot consumption  │
└─────────────────────────────────────────────────────────────┘
```

The five background engines are:

| # | Engine | Purpose |
|---|--------|---------|
| 1 | RSI (14) | Momentum — overbought / oversold |
| 2 | Stochastic RSI | Fast entry timing oscillator |
| 3 | MACD (12/26/9) | Trend direction + momentum |
| 4 | EMA Cross (9/21) | Structural trend baseline |
| 5 | MFI (14) | Volume-weighted confirmation |

Each engine contributes a normalised vote (+1 bullish → −1 bearish). The votes are weighted, summed, and displayed as a **confidence percentage**.

> ⚠️ **Advisory only.** EA.HF Scalper v1.2 is NOT a definitive buy/sell system. It provides advisory metrics designed for use alongside a separate .algo bot or manual analysis.

---

## 2. Installation

1. Open TradingView and navigate to **Pine Script Editor** (bottom toolbar).
2. Delete any default code in the editor.
3. Paste the full contents of `HF.V1.2.pine`.
4. Click **Save** (give it a name, e.g. *EA.HF Scalper v1.2*).
5. Click **Add to chart**.

The indicator attaches as an **overlay** directly on the price chart — no separate pane needed.

---

## 3. Which chart to load it on

### Recommended timeframes

```
Best performance:
  ✅  M1  (1 minute)   — highest signal resolution, most noise
  ✅  M2  (2 minute)   — excellent balance of speed and clarity
  ✅  M5  (5 minute)   — default starting point, recommended for beginners

Works well:
  ✅  M15 (15 minute)  — smoother signals, fewer but higher-quality entries

Suboptimal:
  ⚠️  M30 / H1 and above — indicators were tuned for scalping cadence;
                             works but is not the primary design target
```

### Asset classes

The indicator works on **any liquid asset** tradeable on TradingView with real-time or delayed data:

- **Forex pairs** (EUR/USD, GBP/USD, USD/JPY, etc.) — ideal; high tick frequency
- **Crypto spot / perps** (BTC/USDT, ETH/USDT) — works well; very liquid
- **Equity indices** (SPX500, NAS100, US30) — good; filter for session hours
- **Individual equities** — avoid pre/post-market, stick to liquid hours
- **Commodities** (Gold/XAUUSD, Oil) — good; respect news catalysts

> **Tip:** The COMB row averages M1/M2/M5/M15 regardless of which timeframe you have open.
> Load it on your **execution timeframe** (typically M1 or M2).

---

## 4. Reading the dashboard

The dashboard table appears in the position you choose (default: top-right).

```
╔══════════════════════════╗
║  ⚡  EA.HF SCALPER V1.2  ║
╠══════╦═════════╦═════════╣
║  TF  ║  Signal ║   Conf  ║
╠══════╬═════════╬═════════╣
║  M1  ║  ▲ BUY  ║  72.3%  ║
║  M2  ║  ▲ BUY  ║  68.1%  ║
║  M5  ║  ▲ BUY  ║  61.5%  ║
║  M15 ║  ▼ SELL ║  41.2%  ║  ← disagreement = caution
╠══════╬═════════╬═════════╣
║ COMB ║  ▲ BUY  ║  60.8%  ║  ← highlighted green/red
╠══════╩═════════╩═════════╣
║ ATR(14):      0.00041    ║
╚══════════════════════════╝
```

### Signal colours

| Colour | Meaning |
|--------|---------|
| 🟢 **Bright green** | BUY signal |
| 🔴 **Bright red**   | SELL signal |

### Confidence %

| Range | Interpretation |
|-------|---------------|
| 0 – 40% | Weak / noise — avoid trading |
| 40 – 55% | Borderline — wait for confirmation |
| 55 – 70% | **Moderate signal** — consider trading with caution |
| 70 – 85% | **Strong signal** — primary entry zone |
| 85 – 100% | **Very strong signal** — high-conviction move |

### Multi-timeframe agreement

The most reliable entries occur when **all rows agree**:

```
All green  →  Strong bull confluence  →  Best long entries
All red    →  Strong bear confluence  →  Best short entries
Mixed      →  Choppy / transition     →  Reduce size or skip
```

> The COMB row is a simple average of all four timeframes. A COMB confidence of 65%+
> with majority agreement across timeframes is a high-quality setup.

---

## 5. Reading the candle colours

Candles shift through a **5-stage gradient** based on signal strength:

```
  Score  │  Candle colour       │  What it means
─────────┼──────────────────────┼────────────────────────────────
  +0.8→+1│  ██ Deep green       │  Strong bullish momentum
  +0.4→+0.8  ██ Lime green      │  Moderate bullish
   0       ██ Dark amber        │  Neutral / transitioning
 -0.4→-0.8  ██ Orange-red       │  Moderate bearish
  -0.8→-1│  ██ Deep red         │  Strong bearish momentum
```

The transition uses a **square-root acceleration curve**, meaning colour saturates
quickly once conviction grows — even a moderately bullish score (e.g. +0.4) already
produces a clearly green candle.

**Trading tip:** Candles returning to amber from deep green/red are early warning of
exhaustion — consider tightening stops or partially closing positions.

---

## 6. Reading the TP / SL levels

When COMB confidence is at or above the threshold (default 55%), four horizontal
lines are drawn to the right of the last bar:

```
            ─────────────────── TP3  (161.8% fib extension)  ← solid bold
          ─ ─ ─ ─ ─ ─ ─ ─ ─ ─  TP2  (100% swing high/low)   ← dashed
        ·  ·  ·  ·  ·  ·  ·  · TP1  (61.8% fib level)       ← dotted
PRICE ──●
        ·  ·  ·  ·  ·  ·  ·  · SL   (ATR-based floor)       ← dotted red
```

Each TP level uses the **farther** of:
- An ATR-based minimum distance (default: 1.5× / 2.5× / 4.0× ATR)
- A Fibonacci extension of the recent price range (61.8% / 100% / 161.8%)

### TP Preview Mode (v1.2 — opt-in)

When **TP Preview Mode** is enabled, TP/SL lines are drawn even when confidence
is *below* the Signal Threshold. Preview-mode lines use more transparent (subtle)
colours to visually distinguish them from confirmed levels.

> This is useful for gauging where targets would be during low-confidence periods,
> without changing how signals are acted upon.

### TP level strategy guide

| Level | Usage |
|-------|-------|
| **TP1** | Take half or partial profit — first target, most reliable |
| **TP2** | Move stop to break-even after TP1 hit; hold for this level |
| **TP3** | Scale-in or hold runners; extended momentum target |
| **SL** | Hard stop-loss — exit immediately if price closes below (long) / above (short) |

> **Note:** TP/SL lines disappear when confidence drops below the threshold (unless
> TP Preview Mode is ON). This is intentional — low-confidence signals should not
> have prominent TP targets.

---

## 7. Settings reference (highest to lowest impact)

Open Settings via the ⚙️ icon next to the indicator name.

### 🔴 Highest impact — changes signal behaviour directly

#### EMA Cross (4)
| Setting | Default | Effect |
|---------|---------|--------|
| Fast EMA | **9** | Shorter = faster but more noise. Range: 5–15 |
| Slow EMA | **21** | Core trend filter. Try 20 (Fibonacci), 21, or 34 |

> This component has the **highest weight (1.5×)** in the scoring engine. Shortening
> the fast EMA increases sensitivity but generates more false signals on flat markets.
> Best change: try **8/13** for aggressive scalping or **9/21** (default) for balance.

#### MACD (3)
| Setting | Default | Effect |
|---------|---------|--------|
| Fast | **12** | Shortening accelerates signal generation |
| Slow | **26** | Controls trend detection window |
| Signal | **9** | Lower = more reactive histogram |

> Also weighted at **1.5×**. For M1/M2 scalping, `6/13/5` is a popular fast variant.
> Keep fast < slow at all times; this is a recommended constraint and is **not** automatically enforced by the script.

---

### 🟡 Moderate impact — tunes responsiveness

#### Stochastic RSI (2)
| Setting | Default | Effect |
|---------|---------|--------|
| RSI Period | **14** | Larger = smoother, fewer signals |
| Stoch Period | **14** | Controls lookback for min/max range |
| %K Smooth | **3** | Reduces oscillator jitter |
| %D Smooth | **3** | Signal line lag; increase to 5 for fewer whipsaws |

> Weighted at **1.2×**. On M1 charts, reducing RSI Period to **7** and Stoch to **7**
> significantly speeds up oscillator response. Trade-off: more noise.

#### RSI (1)
| Setting | Default | Effect |
|---------|---------|--------|
| Length | **14** | 7–9 for faster scalping, 14 for default |

> Weighted at **1.0×**. Reducing to 7 on M1 provides faster overbought/oversold readings.

---

### 🟢 Lower impact — fine-tuning only

#### MFI (5)
| Setting | Default | Effect |
|---------|---------|--------|
| Length | **14** | Volume-based; rarely needs changing |

> Weighted at only **0.8×** (least impactful). Change only if you are trading assets
> with unusual volume patterns (e.g. low-liquidity altcoins).

---

### TP / SL Settings

| Setting | Default | Effect |
|---------|---------|--------|
| ATR Length | **14** | ATR smoothing window; increase for less volatile markets |
| TP1 × ATR | **1.5** | First target multiplier — tight scalp exit |
| TP2 × ATR | **2.5** | Mid target — after partial profit taken |
| TP3 × ATR | **4.0** | Extended target — for high-momentum moves |
| SL × ATR | **1.0** | Stop distance — tight for scalping |
| Range Lookback (Fib Window) | **20** | Bars used for swing high/low rolling range (Fibonacci anchors) |
| Neutral Zone Filter | **OFF** | When ON: suppresses TP/SL if score is below threshold in both directions |
| Fib Target Sanity Check | **OFF** | When ON: ensures Fib targets are on the correct side of current price |

> For highly volatile assets (crypto), increase SL to **1.5×** to avoid premature stopouts.
> For low-volatility forex in Asian session, reduce TP1 to **1.0×**.

---

### Display Settings

| Setting | Default | Effect |
|---------|---------|--------|
| Show Signal Dashboard | ✅ On | Toggle the table |
| Show TP/SL Levels | ✅ On | Toggle TP/SL lines |
| Gradient Candle Colors | ✅ On | Toggle coloured candles |
| Signal Threshold (%) | **55** | Minimum confidence before TP/SL lines appear |
| Dashboard Position | top_right | Where the table sits on screen |
| TP Preview Mode | **OFF** | When ON: draw TP/SL even below threshold (subtle colours) |
| TP Gate Debug (Data Window) | **OFF** | When ON: shows gate-pass/fail flags in Data Window |

#### Supported Dashboard Positions

| Value | Description |
|-------|-------------|
| `top_right` | Top-right corner (default) |
| `top_left` | Top-left corner |
| `top_center` | Top-centre of the chart |
| `bottom_right` | Bottom-right corner |
| `bottom_left` | Bottom-left corner |
| `middle_right` | Middle of right edge |
| `middle_left` | Middle of left edge |

> **Signal Threshold** is a quality gate. Raising it to **65%** filters out marginal signals
> and shows TP lines only on high-conviction setups.
> Lowering it increases TP visibility but may show levels during indecision.

---

## 8. Typical trade workflow

```
Step 1 — Context check
  Look at the M15 row in the dashboard.
  Is it aligned with M5 and M2?
  If all three agree → good context. If M15 disagrees → consider smaller size.

Step 2 — Entry signal
  Wait for COMB confidence ≥ 65% with consistent colour (green candles for BUY,
  red for SELL). The candle currently forming should match the signal direction.

Step 3 — Entry execution
  Enter at market or on the next candle open.
  Your SL line is your maximum risk level.

Step 4 — Management
  TP1 hit → close 50% of position. Move stop to break-even.
  TP2 hit → close another 30%. Trail stop.
  TP3     → close remainder, or hold if momentum candles are still deeply coloured.

Step 5 — Exit signal
  Candles fading from green→amber or red→amber = conviction weakening.
  Dashboard flipping from BUY→SELL = exit immediately.
```

---

## 9. Advisory telemetry for .algo bots

EA.HF Scalper v1.2 includes an **Advisory Telemetry** mode designed for consumption
by a separate automated bot (e.g. a cTrader cBot or webhook receiver).

### Enabling telemetry

In Settings → **Telemetry** group, enable **Enable Advisory Telemetry**.

### What it provides

When enabled, the following become available:

**Data Window plots** (hover over the chart and open the Data Window panel):

| Plot name | Description |
|-----------|-------------|
| `t.s_rsi` | RSI component score (−1 to +1) |
| `t.s_stoch` | Stochastic RSI component score |
| `t.s_macd` | MACD component score |
| `t.s_ema` | EMA Cross component score |
| `t.s_mfi` | MFI component score |
| `t.sc_m1` / `t.sc_m2` / `t.sc_m5` / `t.sc_m15` | Per-timeframe composite scores |
| `t.sc_comb` | Combined composite score |
| `t.conf_pct` | Confidence % (0–100) |
| `t.meets_threshold` | 1 if conf_pct ≥ Signal Threshold, else 0 |
| `t.tp_visible` | 1 if TP levels are currently shown, else 0 |
| `t.direction_neutral` | 1 if Neutral Zone filter is active and suppressing |

### JSON alert payload

On each confirmed bar close, a compact JSON message is fired via `alert()`:

```json
{
  "ea": "EA.HF.v1.2",
  "sym": "EURUSD",
  "tf": "1",
  "bar": 12345,
  "sc_m1": 0.6734,
  "sc_m2": 0.5821,
  "sc_m5": 0.4902,
  "sc_m15": 0.3105,
  "sc_comb": 0.5141,
  "conf_pct": 51.4,
  "s_rsi": 0.4200,
  "s_stoch": 0.6100,
  "s_macd": 1.0000,
  "s_ema": 0.5500,
  "s_mfi": 0.1800
}
```

To receive this payload via webhook, create a TradingView Alert with:
- Condition: **EA Advisory Telemetry** (the named `alertcondition`)
- Webhook URL: your bot's endpoint
- Message: leave as default or customise

> ⚠️ **This is advisory telemetry only.** The JSON payload contains signal metrics,
> NOT buy/sell instructions. Your .algo bot is responsible for interpreting the data
> and making all trading decisions.

### TP gate debug

To diagnose why TP/SL lines are hidden, enable **TP Gate Debug (Data Window)**
in the Display settings. The following 1/0 flags appear in the Data Window:

| Flag | Meaning |
|------|---------|
| `dbg:show_tp` | 1 = Show TP/SL Levels input is ON |
| `dbg:is_last_bar` | 1 = currently on the last bar |
| `dbg:conf_ok` | 1 = confidence meets Signal Threshold |
| `dbg:not_neutral` | 1 = direction is NOT neutral (Neutral Zone not suppressing) |
| `dbg:tp_visible` | 1 = TP levels are visible (all gates passed) |

---

## 10. Frequently asked questions

**Q: Why does the M15 row sometimes disagree with M1/M2?**
Higher timeframes are slower to react. This is normal and useful — it shows the broader
context is still in transition. Use it as a caution signal, not a veto.

**Q: The TP lines disappeared — why?**
COMB confidence has fallen below the threshold (default 55%). The indicator is telling
you the signal is too weak to project targets reliably. Enable **TP Gate Debug** in
Display settings to see exactly which gate is failing.

**Q: What does TP Preview Mode do?**
When ON, TP/SL lines are drawn even when confidence is below the threshold. They appear
in more transparent/subtle colours so you can distinguish preview levels from
confirmed levels. Default is OFF to preserve the original gating behaviour.

**Q: What is the Neutral Zone Filter?**
When enabled, the indicator only treats a signal as bullish or bearish if the combined
score's absolute value meets or exceeds the Signal Threshold. Below that, the direction
is considered "neutral" and TP lines are suppressed. Default is OFF.

**Q: What is "Range Lookback (Fib Window)"?**
This is the number of bars used to calculate the rolling swing high/low range for
Fibonacci TP extensions. It was previously labelled "Pivot Lookback" in V1.0.
Note: this uses a rolling window (`ta.highest`/`ta.lowest`), not true swing pivots.

**Q: Can I use this on stocks?**
Yes, during liquid hours (e.g. US session for US equities). Avoid pre-market and
post-market where volume is low — the MFI component degrades without representative volume.

**Q: Does it repaint?**
The composite signal is evaluated on the **last completed bar** when requesting higher
timeframes (`lookahead_off` is enabled). The current-bar signal *does* update in real time
as the bar forms, which is normal for overlay indicators. It does **not** move historical
signals once bars close.

**Q: What is the ATR figure in the dashboard footer?**
Average True Range — the average pip/point range of the last 14 bars. It tells you the
current market volatility. Higher ATR = wider TP and SL levels.

---

## 11. Known limitations & risk disclaimer

- **Not a trading system in isolation.** Use with market structure analysis, news awareness, and position sizing.
- **Advisory only.** EA.HF Scalper v1.2 is designed as an input to a decision-making process, not a standalone signal generator.
- **Fib Window is not true pivots.** The "Range Lookback (Fib Window)" uses a rolling `highest`/`lowest` window, not structurally identified swing points. True pivot-based TP levels are a planned V2 improvement.
- **Gaps and low liquidity** can invalidate ATR-based levels. Be extra cautious around news events.
- **Crypto 24/7 markets** — the indicator performs well but the COMB signal may be slower to react during extreme gap moves.
- **Past performance of indicator settings** on historical data is not a guarantee of future results.
- **Always use a stop-loss.** The SL line is a suggested technical level; hard-set it in your broker/exchange.

> ⚠️ **Trading involves substantial risk of loss. This indicator is a decision-support tool, not financial advice.**
