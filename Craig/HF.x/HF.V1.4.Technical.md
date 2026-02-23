# EA.HF Scalper v1.4 — Technical Reference

> **Version:** 1.4 · **Pine file:** `HF.V4.pine` · **Platform:** TradingView (Pine Script v6)

---

## Changelog

| Version | Key changes |
|---------|-------------|
| **v1.4** | Telemetry plots consolidated to 3; `display.none` when OFF (Style panel clean); `i_label_y_ticks` offset for labels above lines; `i_tp_line_width` default 3, `i_sl_line_width` default 3; dashboard pips row (TP1/TP2/TP3/SL distances in ticks) |
| v1.3 | Dashboard size control; TP/SL line style inputs; single telemetry toggle; debug plot normalisation |
| v1.2 | TP Preview Mode; create-once TP objects; Neutral Zone; Fib Sanity |
| v1.1 | Compiler fixes |
| v1.0 | Initial release |

---

## Table of Contents

1. [Architecture overview](#1-architecture-overview)
2. [Input catalogue (v1.4 changes highlighted)](#2-input-catalogue)
3. [Composite scoring engine (unchanged)](#3-composite-scoring-engine)
4. [Multi-timeframe signals (unchanged)](#4-multi-timeframe-signals)
5. [TP / SL calculation (unchanged)](#5-tp--sl-calculation)
6. [Dashboard table (pips row added)](#6-dashboard-table)
7. [TP / SL object management (y_offset added)](#7-tp--sl-object-management)
8. [Telemetry system (consolidated)](#8-telemetry-system)
9. [Pine v6 notes](#9-pine-v6-notes)
10. [Synthetic validation](#10-synthetic-validation)

---

## 1. Architecture overview

```
Inputs → f_score_detail() [chart TF]
       → f_score() via request.security() [M1/M2/M5/M15]
       → sc_m1/m2/m5/m15/comb / conf_pct / direction
       → pip_sz = syminfo.mintick              ← new v1.4 (tick unit)
       → TP/SL math (ATR + Fib blend, unchanged)
       → y_offset = mintick × i_label_y_ticks  ← new v1.4
       → Dashboard table (barstate.islast)
           + pips distance row (row 7)          ← new v1.4
       → TP/SL objects: create-once/update-in-place
           label y = level + y_offset           ← new v1.4
       → Telemetry: 3 plots, display.none when OFF ← new v1.4
       → JSON alert (unchanged)
```

---

## 2. Input catalogue

### TP / SL Display group — v1.4 changes

| Input | Type | **v1.3 default** | **v1.4 default** | Notes |
|-------|------|-----------------|-----------------|-------|
| `i_tp_line_width` | int | 2 | **3** | TP line thickness (1–5) |
| `i_sl_line_width` | int | 1 | **3** | SL line thickness (1–5) |
| `i_label_y_ticks` | int | *(new)* | **10** | Label vertical offset in ticks (1–200) |

### Telemetry group — v1.4 changes

| Input | Type | Default | Notes |
|-------|------|---------|-------|
| `i_telemetry` | bool | false | Label changed to "Enable Telemetry (Data Window)" |

### All other inputs — unchanged from v1.3

---

## 3. Composite scoring engine

Unchanged from v1.3. See `HF.V1.3.Technical.md` for full details.

---

## 4. Multi-timeframe signals

Unchanged from v1.3. `request.security()` with `lookahead=barmerge.lookahead_off`.

---

## 5. TP / SL calculation

Unchanged. TP/SL math (`tp1_v`, `tp2_v`, `tp3_v`, `sl_v`) is identical to v1.3.

---

## 6. Dashboard table

### Structure: `table.new(tpos, 3, 9, ...)` — 3 cols, **9 rows** (was 8)

| Row | Content |
|-----|---------|
| 0 | Title: "⚡  EA.HF SCALPER V4" (merged) |
| 1 | Column headers: TF / Signal / Conf |
| 2–5 | M1, M2, M5, M15 per-TF signals |
| 6 | COMB highlighted row |
| **7** | **Pips distance footer (new in v1.4)** |
| 8 | ATR footer (was row 7) |

### Pips distance row (row 7)

```pine
float pip_sz  = syminfo.mintick          // universal tick unit
float _tp1p   = (tp1_v - close) / pip_sz
float _tp2p   = (tp2_v - close) / pip_sz
float _tp3p   = (tp3_v - close) / pip_sz
float _slp    = (sl_v  - close) / pip_sz

// f_pdist() helper
f_pdist(float v) =>
    string sgn = v >= 0.0 ? "+" : "-"
    sgn + str.tostring(math.abs(v), "#.0") + "tk"

string pips_txt = "TP1 " + f_pdist(_tp1p) +
    "  TP2 " + f_pdist(_tp2p) +
    "  TP3 " + f_pdist(_tp3p) +
    "  SL " + f_pdist(_slp)
```

- Distances are from current `close` (same reference used by TP math)
- Computed inside `if i_show_table and barstate.islast` block
- Row merged across all 3 columns; centred
- Uses muted colour `#556677` to visually separate from signal rows
- Does **not** depend on `show_levels` — always computed when table is visible

---

## 7. TP / SL object management

### v1.4 change: label y-offset

```pine
// Computed once per bar (outside show_levels block)
float y_offset = syminfo.mintick * i_label_y_ticks

// CREATE block — label y = level + offset
b_tp1 := label.new(xe, tp1_v + y_offset, ...)
b_tp2 := label.new(xe, tp2_v + y_offset, ...)
b_tp3 := label.new(xe, tp3_v + y_offset, ...)
b_sl  := label.new(xe, sl_v  + y_offset, ...)

// UPDATE block — label.set_xy with offset
label.set_xy(b_tp1, xe, tp1_v + y_offset)
label.set_xy(b_tp2, xe, tp2_v + y_offset)
label.set_xy(b_tp3, xe, tp3_v + y_offset)
label.set_xy(b_sl,  xe, sl_v  + y_offset)
```

- `y_offset` is always positive, placing the label box above the line for both bull and bear levels
- Line y-coordinates (`tp1_v`, `sl_v`, etc.) are **unchanged** — only the label y moves
- The `label.style_label_left` style ensures the text box extends to the right of the anchor point

### Create-once / update-in-place pattern (unchanged from v1.3)

- `var` handles: `l_tp1`, `l_tp2`, `l_tp3`, `l_sl`, `b_tp1`, `b_tp2`, `b_tp3`, `b_sl`
- Hidden via `color.new(color.gray, 100)` and `label.set_text("")` — never deleted

---

## 8. Telemetry system

### v1.4 consolidation: 18 plots → 3 plots

| Plot name | Description | v1.3 | v1.4 |
|-----------|-------------|------|------|
| `t.s_rsi`, `t.s_stoch`, `t.s_macd`, `t.s_ema`, `t.s_mfi` | Component scores | ✅ | ❌ removed (in JSON) |
| `t.sc_m1`, `t.sc_m2`, `t.sc_m5`, `t.sc_m15` | Per-TF scores | ✅ | ❌ removed (in JSON) |
| `t.sc_comb` | Combined score | ✅ | ✅ kept |
| `t.conf_pct` | Confidence % | ✅ | ✅ kept |
| `t.meets_threshold`, `t.direction_neutral` | Gate flags | ✅ | ❌ removed |
| `t.tp_visible` | TP gate status | ✅ | ✅ kept |
| `dbg_show_tp`, `dbg_is_last_bar`, `dbg_conf_ok`, `dbg_tp_visible` | Debug flags | ✅ | ❌ removed |

### Display argument pattern (key change)

```pine
// v1.3 (always data_window — still appears in Style panel)
plot(i_telemetry ? sc_comb : na, "t.sc_comb", display=display.data_window)

// v1.4 (display.none when OFF — removed from Style panel)
plot(sc_comb,
     "t.sc_comb",
     display = i_telemetry ? display.data_window : display.none)
```

When `i_telemetry = false`, `display=display.none` on every bar removes the series from the
Settings → Style tab entirely. When `true`, the plot appears only in the Data Window (no chart overlay).

### JSON alert payload (unchanged)

`ea` key updated to `"EA.HF.v4"`. All other fields identical to v1.3.

---

## 9. Pine v6 notes

- No line continuations — multi-line expressions use parentheses grouping.
- `display` parameter on `plot()` accepts runtime ternary: `i_telemetry ? display.data_window : display.none`.
- `float` variables declared inside `if barstate.islast` block are local to that scope (e.g. `_tp1p`).
- `f_pdist()` is a user-defined function returning a `string`; valid Pine v6 function syntax.
- `y_offset` computed globally (outside `if show_levels`) to be available in both CREATE and UPDATE branches.
- `position` is an enum namespace — mapped via `f_tbl_pos()` helper.

---

## 10. Synthetic validation

| Test | Series | Expected v1.4 behaviour |
|------|--------|------------------------|
| A — Bull | `close = 100 + i*0.05` | TP labels appear above lines; pips row shows positive values for TP1/2/3, negative for SL |
| B — Bear | `close = 100 - i*0.05` | TP labels appear above lines; pips row shows negative values for TP1/2/3, positive for SL |
| C — Flat | `close = 100 + sin(i*0.3)*0.2` | No TP lines; pips row still shows distances |
| D — Telemetry OFF | Any | 0 Style panel entries; no JSON alerts emitted (telemetry fully disabled) |
| E — Telemetry ON | Any | 3 Data Window entries: `t.sc_comb`, `t.conf_pct`, `t.tp_visible` |
