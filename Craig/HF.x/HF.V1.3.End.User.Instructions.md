# EA.HF Scalper v1.3 — End User Instructions

> **Version:** 1.3 · **Pine file:** `HF.V3.pine` · **Platform:** TradingView (Pine Script v6) · **Designed for:** 1m – 15m scalping

---

## Changelog

| Version | Summary |
|---------|---------|
| **v1.3** | Dashboard table size control; TP/SL label text size; TP/SL line style controls (width, color); extend-left option; label indent control; telemetry consolidated to single toggle; debug plot names normalised |
| v1.2 | TP Preview Mode; Data Window debug flags; create-once/update-in-place TP objects; Neutral Zone filter; Fib Target Sanity check; top_center dashboard position |
| v1.1 | Compiler fixes (ta.mfi, math.round, ATR scope, sl_col always red) |
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

**EA.HF Scalper v1.3** is a **multi-timeframe confluence engine** for short-duration scalp trades.
Five indicators run silently in the background and vote on direction and strength.
You see three output layers on the chart, plus optional advisory telemetry:

```
┌─────────────────────────────────────────────────────────────┐
│  LAYER 1 – Dashboard table (configurable position and size)  │
│    Shows BUY / SELL + confidence % for M1/M2/M5/M15/COMB   │
│                                                             │
│  LAYER 2 – Gradient candle colours                          │
│    Deep green (strong bull) ← Amber (neutral) → Deep red   │
│                                                             │
│  LAYER 3 – TP1 / TP2 / TP3 + SL lines                      │
│    Dynamic levels with configurable width, color, style     │
│                                                             │
│  LAYER 4 – Advisory Telemetry (opt-in, Data Window only)    │
│    Component scores + JSON alert for .algo bot consumption  │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Installation

1. Open TradingView and navigate to **Pine Script Editor** (bottom toolbar).
2. Delete any default code in the editor.
3. Paste the full contents of `HF.V3.pine`.
4. Click **Save** (give it a name, e.g. *EA.HF Scalper v1.3*).
5. Click **Add to chart**.

---

## 3. Which chart to load it on

Recommended: **M1, M2, M5** (primary design target). Works well on M15.

---

## 4. Reading the dashboard

```
╔═════════════════════════╗
║  ⚡  EA.HF SCALPER V3   ║
╠══════╦═════════╦════════╣
║  TF  ║  Signal ║  Conf  ║
╠══════╬═════════╬════════╣
║  M1  ║  ▲ BUY  ║ 72.3%  ║
║  M2  ║  ▲ BUY  ║ 68.1%  ║
║  M5  ║  ▲ BUY  ║ 61.5%  ║
║  M15 ║  ▼ SELL ║ 41.2%  ║
╠══════╬═════════╬════════╣
║ COMB ║  ▲ BUY  ║ 60.8%  ║
╠══════╩═════════╩════════╣
║ ATR(14):      0.00041   ║
╚═════════════════════════╝
```

Use **Table Size** (Dashboard group) to scale text: Small / Medium / Large.

---

## 5. Reading the candle colours

Candles shift through a gradient: deep green (strong bull) → amber (neutral) → deep red (strong bear).

---

## 6. Reading the TP / SL levels

Three horizontal lines drawn to the right of the last bar when COMB confidence ≥ Signal Threshold:

- **TP1** — dotted, 61.8% Fibonacci level
- **TP2** — dashed, 100% swing high/low
- **TP3** — solid, 161.8% extension
- **SL** — dotted red, ATR-based

### New in v1.3 — Line style controls

| Setting | Default | Description |
|---------|---------|-------------|
| TP Line Width | 2 | 1–5; controls TP1/TP2/TP3 thickness |
| SL Line Width | 1 | 1–5; controls SL thickness |
| TP Base Color | Green | Color for bull-direction TP lines |
| SL Color | Red | Color for SL and bear-direction TP lines |
| Extend TP/SL Lines Left | OFF | Extends lines both directions when ON |
| TP/SL Label Indent (bars) | 20 | How far right of last bar the labels sit |
| TP/SL Text Size | Small | Tiny / Small / Normal / Large |

---

## 7. Settings reference

### Display

| Setting | Default | Effect |
|---------|---------|--------|
| Show Signal Dashboard | ✅ On | Toggle the table |
| Show TP/SL Levels | ✅ On | Toggle TP/SL lines |
| Gradient Candle Colors | ✅ On | Toggle coloured candles |
| Signal Threshold (%) | **55** | Minimum confidence before TP/SL lines appear |
| Dashboard Position | top_right | Where the table sits |

### Dashboard

| Setting | Default | Effect |
|---------|---------|--------|
| Table Size | Medium | Small=tiny text, Medium=small, Large=normal |

### TP / SL Display (new in v1.3)

| Setting | Default | Effect |
|---------|---------|--------|
| TP/SL Preview Mode | OFF | Draw below threshold in subtle colours |
| TP/SL Text Size | Small | Label text size |
| TP Line Width | 2 | TP line thickness |
| SL Line Width | 1 | SL line thickness |
| TP Base Color | Green | Bull TP color |
| SL Color | Red | SL and bear TP color |
| Extend TP/SL Lines Left | OFF | extend.both when ON |
| TP/SL Label Indent (bars) | 20 | Label x-position |

### Telemetry (consolidated in v1.3)

| Setting | Default | Effect |
|---------|---------|--------|
| Enable Advisory Telemetry | OFF | Single toggle for all Data Window plots + JSON alert |

> In v1.3 the previous separate debug toggle was removed. One master toggle now gates all telemetry.

---

## 8. Typical trade workflow

1. Check M15 context — is it aligned with M5/M2?
2. Wait for COMB ≥ 65% with matching candle colour.
3. Enter at market; use SL line as hard stop.
4. TP1 hit → take 50%, move stop to break-even.
5. TP2 hit → take 30%, trail remainder.
6. TP3 → close rest or hold if candles stay deeply coloured.

---

## 9. Advisory telemetry for .algo bots

Enable **Enable Advisory Telemetry** in the Telemetry group.

**Data Window plots** available when ON:

| Plot | Description |
|------|-------------|
| `t.s_rsi` | RSI component score |
| `t.s_stoch` | Stochastic RSI score |
| `t.s_macd` | MACD score |
| `t.s_ema` | EMA Cross score |
| `t.s_mfi` | MFI score |
| `t.sc_m1/m2/m5/m15` | Per-TF composite scores |
| `t.sc_comb` | Combined score |
| `t.conf_pct` | Confidence % |
| `t.meets_threshold` | 1 if conf ≥ threshold |
| `t.tp_visible` | 1 if TP levels shown |
| `t.direction_neutral` | 1 if neutral zone suppressing |
| `dbg_show_tp/is_last_bar/conf_ok/tp_visible` | Gate debug flags |

**JSON alert** fires on each confirmed bar close via `alert()`.

---

## 10. Frequently asked questions

**Q: Why are there no individual plot toggles in the Style panel?**
In v1.3, all telemetry plots are controlled by the single *Enable Advisory Telemetry* toggle. This reduces Style panel clutter.

**Q: What is the Table Size setting?**
Controls the text size across the entire dashboard. Small = tiny (compact), Medium = small (default), Large = normal.

---

## 11. Known limitations & risk disclaimer

- Advisory only — NOT a standalone trading system.
- Fib Window uses rolling `highest`/`lowest`, not true swing pivots.
- Gaps and low liquidity can invalidate ATR-based levels.

> ⚠️ **Trading involves substantial risk of loss. This indicator is a decision-support tool, not financial advice.**
