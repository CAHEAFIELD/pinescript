# EA.HF Scalper v1.4 — End User Instructions

> **Version:** 1.4 · **Pine file:** `HF.V4.pine` · **Platform:** TradingView (Pine Script v6) · **Designed for:** 1m – 15m scalping

---

## Changelog

| Version | Summary |
|---------|---------|
| **v1.4** | Style panel clutter removed (telemetry plots consolidated, hidden when OFF); TP/SL labels shifted above lines via tick offset; default line widths 3/3; dashboard pips distance row |
| v1.3 | Dashboard size control; TP/SL line style inputs; single telemetry toggle |
| v1.2 | TP Preview Mode; Neutral Zone filter; Fib Target Sanity check |
| v1.1 | Compiler fixes |
| v1.0 | Initial release |

---

## Table of Contents

1. [What this indicator does](#1-what-this-indicator-does)
2. [Installation](#2-installation)
3. [Which chart to load it on](#3-which-chart-to-load-it-on)
4. [Reading the dashboard](#4-reading-the-dashboard)
5. [Reading the candle colours](#5-reading-the-candle-colours)
6. [Reading the TP / SL levels](#6-reading-the-tp--sl-levels)
7. [Settings reference](#7-settings-reference)
8. [Typical trade workflow](#8-typical-trade-workflow)
9. [Advisory telemetry for .algo bots](#9-advisory-telemetry-for-algo-bots)
10. [Frequently asked questions](#10-frequently-asked-questions)
11. [Known limitations & risk disclaimer](#11-known-limitations--risk-disclaimer)

---

## 1. What this indicator does

**EA.HF Scalper v1.4** is a **multi-timeframe confluence engine** for short-duration scalp trades.
Five indicators run silently in the background and vote on direction and strength.

```
┌─────────────────────────────────────────────────────────────┐
│  LAYER 1 – Dashboard table (with TP/SL pips distance row)   │
│    Shows BUY / SELL + confidence % for M1/M2/M5/M15/COMB   │
│    + compact TP1/TP2/TP3/SL tick distances from close       │
│                                                             │
│  LAYER 2 – Gradient candle colours                          │
│    Deep green (strong bull) ← Amber (neutral) → Deep red   │
│                                                             │
│  LAYER 3 – TP1 / TP2 / TP3 + SL lines                      │
│    Labels sit above each line (configurable tick offset)    │
│    Default line widths: TP=3, SL=3                          │
│                                                             │
│  LAYER 4 – Advisory Telemetry (opt-in, Data Window only)    │
│    3 consolidated plots; hidden from Style panel when OFF   │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Installation

1. Open TradingView and navigate to **Pine Script Editor** (bottom toolbar).
2. Delete any default code in the editor.
3. Paste the full contents of `HF.V4.pine`.
4. Click **Save** (give it a name, e.g. *EA.HF Scalper v1.4*).
5. Click **Add to chart**.

---

## 3. Which chart to load it on

Recommended: **M1, M2, M5**. Works well on M15.

> **Tip:** COMB averages all four timeframes regardless of which one you have open. Load the indicator on your execution timeframe (typically M1 or M2).

---

## 4. Reading the dashboard

```
╔═════════════════════════════════════════════╗
║          ⚡  EA.HF SCALPER V4               ║
╠══════╦══════════════════╦═══════════════════╣
║  TF  ║      Signal      ║       Conf        ║
╠══════╬══════════════════╬═══════════════════╣
║  M1  ║     ▲  BUY       ║      72.3%        ║
║  M2  ║     ▲  BUY       ║      68.1%        ║
║  M5  ║     ▲  BUY       ║      61.5%        ║
║  M15 ║     ▼  SELL      ║      41.2%        ║
╠══════╬══════════════════╬═══════════════════╣
║ COMB ║     ▲  BUY       ║      60.8%        ║
╠══════╩══════════════════╩═══════════════════╣
║  TP1 +125tk  TP2 +231tk  TP3 +375tk  SL -82tk  ║   ← new in v1.4
╠═════════════════════════════════════════════╣
║ ATR(14):              0.00041               ║
╚═════════════════════════════════════════════╝
```

### Pips distance row (new in v1.4)

The row beneath COMB shows the **tick distance** from the current close to each TP and SL level.

- Positive values = levels above current price (typical for bull TP targets)
- Negative values = levels below current price (typical for bull SL, bear TP targets)
- Unit: `tk` = ticks (multiples of `syminfo.mintick`)
- This is display-only; the TP/SL calculation is unchanged

---

## 5. Reading the candle colours

Candles shift through a gradient based on signal strength:

| Score | Colour | Meaning |
|-------|--------|---------|
| +0.8 → +1 | Deep green | Strong bullish momentum |
| +0.4 → +0.8 | Lime green | Moderate bullish |
| ~0 | Dark amber | Neutral / transitioning |
| -0.4 → -0.8 | Orange-red | Moderate bearish |
| -0.8 → -1 | Deep red | Strong bearish momentum |

---

## 6. Reading the TP / SL levels

When COMB confidence ≥ Signal Threshold (default 55%), four horizontal lines appear:

```
           ─────────────────  [TP3 label above line]  ← solid bold (width=3)
         ─ ─ ─ ─ ─ ─ ─ ─ ─   [TP2 label above line]  ← dashed (width=3)
        ·  ·  ·  ·  ·  ·  ·  [TP1 label above line]  ← dotted (width=3)
PRICE ──●
        ·  ·  ·  ·  ·  ·  ·  [SL  label above line]  ← dotted red (width=3)
```

### New in v1.4 — Labels above lines

Labels are now placed **above** each line by a configurable tick offset.

| Setting | Default | Description |
|---------|---------|-------------|
| TP/SL Label Vertical Offset (ticks) | **10** | Ticks above the line where the label box sits. Increase for wider spacing. |

### New in v1.4 — Default line widths

Both TP and SL default to **width=3** (previously TP=2, SL=1). Users may adjust via the TP/SL Display settings.

---

## 7. Settings reference

### Display

| Setting | Default | Effect |
|---------|---------|--------|
| Show Signal Dashboard | ✅ On | Toggle the table |
| Show TP/SL Levels | ✅ On | Toggle TP/SL lines |
| Gradient Candle Colors | ✅ On | Toggle candle gradient |
| Signal Threshold (%) | **55** | Minimum confidence before TP/SL drawn |
| Dashboard Position | top_right | Table position on chart |

### Dashboard

| Setting | Default | Effect |
|---------|---------|--------|
| Table Size | Medium | Small / Medium / Large |

### TP / SL Display

| Setting | Default | Notes |
|---------|---------|-------|
| TP/SL Preview Mode | OFF | Draw below threshold in subtle colours |
| TP/SL Text Size | Small | Tiny / Small / Normal / Large |
| TP Line Width | **3** | TP1/TP2/TP3 thickness (1–5) |
| SL Line Width | **3** | SL thickness (1–5) |
| TP Base Color | Green | Bull TP color |
| SL Color | Red | SL and bear TP color |
| Extend TP/SL Lines Left | OFF | Extend both directions |
| TP/SL Label Indent (bars) | 20 | Bars right of last bar |
| TP/SL Label Vertical Offset (ticks) | **10** | Ticks above line for label box **(new)** |

### Telemetry

| Setting | Default | Notes |
|---------|---------|-------|
| Enable Telemetry (Data Window) | OFF | Master toggle. When OFF: 0 Style panel entries. When ON: 3 Data Window plots. |

---

## 8. Typical trade workflow

1. **Context check** — M15 row aligned with M5/M2?
2. **Entry signal** — COMB ≥ 65% with green/red candles matching direction.
3. **Entry** — enter at market or next candle open; use SL line as hard stop.
4. **Management** — TP1 hit → close 50%, move stop to break-even.
5. **Scaling out** — TP2 hit → close 30%, trail stop; TP3 → close rest.
6. **Exit** — candles fading to amber or dashboard flipping direction.

---

## 9. Advisory telemetry for .algo bots

Enable **Enable Telemetry (Data Window)** in the Telemetry group.

### What changed in v1.4

- Style panel no longer lists telemetry/debug series when telemetry is OFF.
- Only 3 consolidated plots remain (down from 18 in v1.3):

| Plot | Description |
|------|-------------|
| `t.sc_comb` | Combined composite score (−1 to +1) |
| `t.conf_pct` | Confidence percentage (0–100) |
| `t.tp_visible` | TP/SL gate status (1 = visible, 0 = hidden) |

- Full per-component and per-TF data is still available in the **JSON alert payload** (unchanged).

### JSON alert payload (unchanged)

```json
{
  "ea": "EA.HF.v4",
  "sym": "EURUSD",
  "tf": "1",
  "bar": 12345,
  "sc_m1": 0.6734, "sc_m2": 0.5821, "sc_m5": 0.4902, "sc_m15": 0.3105,
  "sc_comb": 0.5141, "conf_pct": 51.4,
  "s_rsi": 0.4200, "s_stoch": 0.6100, "s_macd": 1.0000,
  "s_ema": 0.5500, "s_mfi": 0.1800
}
```

---

## 10. Frequently asked questions

**Q: Why are the telemetry plots no longer in the Style panel?**
In v1.4, telemetry plots use `display=display.none` when the Telemetry toggle is OFF. This fully removes them from the Style tab. When ON, they appear only in the Data Window.

**Q: What are the pips/ticks in the dashboard footer?**
The new row shows the distance from the current bar's close to each TP and SL level, measured in **ticks** (`syminfo.mintick` units). This is display-only and does not affect the TP/SL calculation.

**Q: Can I convert ticks to pips?**
For most FX 5-decimal pairs (e.g. EUR/USD), 1 pip = 10 ticks. Divide the `tk` value by 10 to get approximate pips.

**Q: The labels are now above the lines — can I move them back to the line?**
Set **TP/SL Label Vertical Offset (ticks)** to `1` (minimum) to place labels very close to the line.

**Q: What is TP Preview Mode?**
When ON, TP/SL lines are drawn even when confidence is below the threshold, using more transparent colours. Useful for gauging target levels during low-confidence periods.

---

## 11. Known limitations & risk disclaimer

- Advisory only — NOT a standalone trading system.
- Fib Window uses rolling `highest`/`lowest`, not true swing pivots.
- Tick-based pips display may not match broker pip definitions exactly for all instruments.
- Gaps and low liquidity can invalidate ATR-based levels.

> ⚠️ **Trading involves substantial risk of loss. This indicator is a decision-support tool, not financial advice.**
