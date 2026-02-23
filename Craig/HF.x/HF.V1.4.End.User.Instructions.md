# EA.HF Scalper v1.4 — End User Instructions

> **Version:** 1.4 · **Pine file:** `HF.V1.4.pine` · **Platform:** TradingView (Pine Script v6) · **Designed for:** 1m – 15m scalping

---

## Changelog

| Version | Summary |
|---------|---------|
| **v1.4** | Per-line TP1/TP2/TP3/SL width/style/color controls; symbol-aware pip sizing (FX=pips with "p", non-FX=ticks with "tk", Override available); ATR row removed from dashboard; TP summary text color → white; three settings hidden from UI (Neutral Zone Filter, Fib Sanity Check, Enable Telemetry); file renamed to HF.V1.4.pine |
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
3. Paste the full contents of `HF.V1.4.pine`.
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
║  TP1 +12.5p  TP2 +23.1p  TP3 +37.5p  SL -8.2p  ║
╚═════════════════════════════════════════════╝
```

### Pips distance row

The row beneath COMB shows the **distance** from the current close to each TP and SL level.

- Positive values = levels above current price (typical for bull TP targets)
- Negative values = levels below current price (typical for bull SL, bear TP targets)
- **On FX symbols (forex):** distances expressed in **pips** (mintick × 10), shown with `"p"` suffix
- **On non-FX symbols:** distances expressed in **ticks** (mintick), shown with `"tk"` suffix
- **Pip Size Override:** set Pip Size Mode to "Override" and enter a custom Pip Size Override value — result is always shown with `"p"` suffix
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

### Labels above lines

Labels are placed **above** each line by a configurable tick offset.

| Setting | Default | Description |
|---------|---------|-------------|
| TP/SL Label Vertical Offset (ticks) | **10** | Ticks above the line where the label box sits. Increase for wider spacing. |

### Per-line TP/SL controls (new in v1.4)

Each of TP1, TP2, TP3, and SL now has **independent** width, style, and color settings:

| Line | Width setting | Style setting | Color setting |
|------|---------------|---------------|---------------|
| TP1 | TP1 Line Width | TP1 Line Style | TP1 Color |
| TP2 | TP2 Line Width | TP2 Line Style | TP2 Color |
| TP3 | TP3 Line Width | TP3 Line Style | TP3 Color |
| SL  | SL Line Width  | SL Line Style  | SL Color  |

The previous shared "TP Line Width", "SL Line Width", "TP Base Color", and "SL Color" inputs have been removed.

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
| TP1 Line Width | **3** | TP1 thickness (1–5) |
| TP1 Line Style | dotted | solid / dashed / dotted |
| TP1 Color | Green | TP1 line color |
| TP2 Line Width | **3** | TP2 thickness (1–5) |
| TP2 Line Style | dashed | solid / dashed / dotted |
| TP2 Color | Green | TP2 line color |
| TP3 Line Width | **3** | TP3 thickness (1–5) |
| TP3 Line Style | solid | solid / dashed / dotted |
| TP3 Color | Green | TP3 line color |
| SL Line Width | **3** | SL thickness (1–5) |
| SL Line Style | dotted | solid / dashed / dotted |
| SL Color | Red | SL line color |
| Extend TP/SL Lines Left | OFF | Extend both directions |
| TP/SL Label Indent (bars) | 20 | Bars right of last bar |
| TP/SL Label Vertical Offset (ticks) | **10** | Ticks above line for label box |
| Pip Size Mode | Auto | "Auto" = symbol-aware; "Override" = use custom value |
| Pip Size Override | 0.0001 | Custom pip size; active only when Pip Size Mode = Override |

### Telemetry

| Setting | Default | Notes |
|---------|---------|-------|
| Enable Telemetry (Data Window) | OFF | *(hidden — not visible in Settings UI)* Master toggle. When OFF: 0 Style panel entries. When ON: 3 Data Window plots. |

> **Note:** Three settings are **not visible in the normal Settings UI** because they are marked hidden in the script: **Neutral Zone Filter**, **Fib Target Sanity Check**, and **Enable Telemetry**. They remain functional — they can be enabled via direct script editing or automation.

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
- The **Enable Telemetry** toggle is hidden from the Settings UI; it can be enabled via direct script editing or automation.

### JSON alert payload (unchanged)

```json
{
  "ea": "EA.HF.v1.4",
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
The row shows the distance from the current bar's close to each TP and SL level. The unit is **symbol-aware**:
- On **FX pairs** (forex): distances shown in **pips** (mintick × 10) with a `"p"` suffix — e.g. `+12.5p`
- On **non-FX instruments** (indices, crypto, commodities, etc.): distances shown in **ticks** (mintick) with a `"tk"` suffix — e.g. `+125tk`
- With **Pip Size Mode = Override**: uses your custom Pip Size Override value and always shows `"p"` suffix

This is display-only and does not affect the TP/SL calculation.

**Q: Can I set a custom pip size for non-standard instruments?**
Yes. Set **Pip Size Mode** to "Override" in the TP/SL Display settings and enter your desired **Pip Size Override** value. Distances will then use that divisor and display with the `"p"` suffix.

**Q: The "tk" suffix appears — why aren't pips shown for my FX pair?**
TradingView classifies some broker feeds (e.g. CFD FX symbols) as non-forex. If `syminfo.type` does not equal `"forex"` the auto mode falls back to ticks. Use **Pip Size Mode = Override** with `0.0001` (or your broker's pip size) to force pip display.

**Q: The labels are now above the lines — can I move them back to the line?**
Set **TP/SL Label Vertical Offset (ticks)** to `1` (minimum) to place labels very close to the line.

**Q: What is TP Preview Mode?**
When ON, TP/SL lines are drawn even when confidence is below the threshold, using more transparent colours. Useful for gauging target levels during low-confidence periods.

---

## 11. Known limitations & risk disclaimer

- Advisory only — NOT a standalone trading system.
- Fib Window uses rolling `highest`/`lowest`, not true swing pivots.
- Pip display is symbol-aware (forex auto=mintick×10); non-forex shown as ticks. Use Pip Size Override for custom instruments.
- Gaps and low liquidity can invalidate ATR-based levels.

> ⚠️ **Trading involves substantial risk of loss. This indicator is a decision-support tool, not financial advice.**
