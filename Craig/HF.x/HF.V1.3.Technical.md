# EA.HF Scalper v1.3 — Technical Reference

> **Version:** 1.3 · **Pine file:** `HF.V3.pine` · **Platform:** TradingView (Pine Script v6)

---

## Changelog

| Version | Key changes |
|---------|-------------|
| **v1.3** | Dashboard table size control; TP/SL line style inputs; single telemetry toggle; debug plot name normalisation; no-delete TP object update pattern extended to width/extend |
| v1.2 | TP Preview Mode; Data Window gate debug flags; create-once TP objects; Neutral Zone filter; Fib Sanity check |
| v1.1 | Compiler fixes |
| v1.0 | Initial release |

---

## Table of Contents

1. [Architecture overview](#1-architecture-overview)
2. [Input catalogue](#2-input-catalogue)
3. [Composite scoring engine](#3-composite-scoring-engine)
4. [Multi-timeframe signals](#4-multi-timeframe-signals)
5. [TP / SL calculation](#5-tp--sl-calculation)
6. [Dashboard table](#6-dashboard-table)
7. [TP / SL object management](#7-tp--sl-object-management)
8. [Telemetry system](#8-telemetry-system)
9. [Pine v6 notes](#9-pine-v6-notes)
10. [Synthetic validation](#10-synthetic-validation)

---

## 1. Architecture overview

```
Inputs → f_score_detail() [chart TF]
       → f_score() via request.security() [M1/M2/M5/M15]
       → sc_m1/m2/m5/m15/comb
       → conf_pct + direction
       → TP/SL math (ATR + Fib blend)
       → Dashboard table (barstate.islast)
       → TP/SL objects (create-once/update-in-place)
       → Telemetry plots + JSON alert (opt-in)
```

---

## 2. Input catalogue

### Display group
| Input | Type | Default | Notes |
|-------|------|---------|-------|
| `i_show_table` | bool | true | Toggles dashboard |
| `i_show_tp` | bool | true | Toggles TP/SL objects |
| `i_color_candles` | bool | true | Toggles candle gradient |
| `i_conf_min` | float | 55.0 | Signal threshold (%) |
| `i_tbl_pos` | string | top_right | Dashboard position |

### Dashboard group
| Input | Type | Default | Notes |
|-------|------|---------|-------|
| `i_tbl_size` | string | Medium | Table text size: Small/Medium/Large |

### TP / SL Display group (new in v1.3)
| Input | Type | Default | Notes |
|-------|------|---------|-------|
| `i_tp_preview` | bool | false | Draw below threshold in preview colours |
| `i_tp_text_size` | string | Small | Label text size |
| `i_tp_line_width` | int | 2 | TP line width (1–5) |
| `i_sl_line_width` | int | 1 | SL line width (1–5) |
| `i_tp_color` | color | #00C853 | Bull TP color |
| `i_sl_color` | color | #FF3D3D | SL / bear TP color |
| `i_extend_left` | bool | false | Extend lines left |
| `i_tp_indent` | int | 20 | Label indent (bars) |

### Telemetry group (consolidated in v1.3)
| Input | Type | Default | Notes |
|-------|------|---------|-------|
| `i_telemetry` | bool | false | Master toggle for all Data Window plots |

---

## 3. Composite scoring engine

### `f_score_detail()` — returns `[combined, s_rsi, s_stoch, s_macd, s_ema, s_mfi]`

| Component | Weight | Score range | Method |
|-----------|--------|-------------|--------|
| RSI (14) | 1.0 | −1 to +1 | `(rsi - 50) / 50` |
| Stochastic RSI | 1.2 | −1 to +1 | k%D blend, clamped |
| MACD histogram | 1.5 | ±1 or ±0.3 | Sign + momentum |
| EMA Cross | 1.5 | −1 to +1 | Spread ratio × 300 |
| MFI | 0.8 | −1 to +1 | `(mfi - 50) / 50` |
| **Total weight** | **6.0** | | Divided out to keep result in [−1, +1] |

### `f_score()` — thin wrapper for use inside `request.security()`

---

## 4. Multi-timeframe signals

```pine
sc_m1  = request.security(syminfo.tickerid, "1",  f_score(), lookahead=barmerge.lookahead_off)
sc_m2  = request.security(syminfo.tickerid, "2",  f_score(), lookahead=barmerge.lookahead_off)
sc_m5  = request.security(syminfo.tickerid, "5",  f_score(), lookahead=barmerge.lookahead_off)
sc_m15 = request.security(syminfo.tickerid, "15", f_score(), lookahead=barmerge.lookahead_off)
sc_comb = (sc_m1 + sc_m2 + sc_m5 + sc_m15) / 4.0
```

`lookahead_off` prevents future-bar leakage. Each `request.security()` call runs `f_score()` in an independent context with correct OHLCV for that timeframe.

---

## 5. TP / SL calculation

### Direction

```pine
is_bull = i_neutral_zone ? (sc_comb >= i_conf_min/100) : (sc_comb >= 0.0)
is_neutral = i_neutral_zone and (abs(sc_comb) < i_conf_min/100)
```

### ATR floors

```pine
raw1 = atr_val * i_tp1_mult   // default: ATR × 1.5
raw2 = atr_val * i_tp2_mult   // default: ATR × 2.5
raw3 = atr_val * i_tp3_mult   // default: ATR × 4.0
raw_sl = atr_val * i_sl_mult  // default: ATR × 1.0
```

### Fibonacci targets

```pine
swg_hi = ta.highest(high, i_tp_lookb)
swg_lo = ta.lowest(low,  i_tp_lookb)
swg_rng = swg_hi - swg_lo
// Bull targets
fib_tp1_b = swg_lo + swg_rng * 0.618
fib_tp2_b = swg_hi
fib_tp3_b = swg_lo + swg_rng * 1.618
```

### Final level (farther of ATR or Fib)

```pine
tp1_v = is_bull ? max(close + raw1, safe_fib_tp1_b) : min(close - raw1, safe_fib_tp1_s)
```

---

## 6. Dashboard table

- `table.new(tpos, 3, 8, ...)` — 3 columns, 8 rows
- Rows: Title | Headers | M1 | M2 | M5 | M15 | COMB | ATR footer
- Size helper: `f_tbl_size()` maps string → `size.*` constant
- Position helper: `f_tbl_pos()` maps string → `position.*` constant
- Rebuilt on `barstate.islast` only; entire table replaced each update

---

## 7. TP / SL object management

**Pattern: create-once / update-in-place** (`var` handles, `if na(l_tp1)` guard)

```pine
var line  l_tp1 = na
var label b_tp1 = na

if show_levels
    if na(l_tp1)
        l_tp1 := line.new(...)   // created once
        b_tp1 := label.new(...)
    else
        line.set_xy1(l_tp1, ...)  // updated in place
        line.set_width(l_tp1, i_tp_line_width)
        line.set_extend(l_tp1, ext_mode)
        label.set_xy(b_tp1, ...)
        label.set_size(b_tp1, lbl_sz)
else
    line.set_color(l_tp1, color.new(color.gray, 100))  // hide via transparency
    label.set_text(b_tp1, "")
```

**Why:** Prevents object count exhaustion (`max_lines_count=50`). Objects are never deleted; hidden via 100% transparent color.

---

## 8. Telemetry system

### v1.3 consolidation

In v1.3, the previous separate `i_tp_debug` boolean was removed. A single `i_telemetry` toggle gates:
- All per-component score plots (`t.s_rsi`, etc.)
- All per-TF score plots (`t.sc_m1`, etc.)
- Combined score and confidence plots
- Gate status flags (`t.meets_threshold`, `t.tp_visible`, `t.direction_neutral`)
- TP debug flags (`dbg_show_tp`, `dbg_is_last_bar`, `dbg_conf_ok`, `dbg_tp_visible`)

All telemetry plots use `display=display.data_window`.

### JSON alert payload

```json
{
  "ea": "EA.HF.v3",
  "sym": "EURUSD",
  "tf": "1",
  "bar": 12345,
  "sc_m1": 0.6734, "sc_m2": 0.5821, "sc_m5": 0.4902, "sc_m15": 0.3105,
  "sc_comb": 0.5141, "conf_pct": 51.4,
  "s_rsi": 0.4200, "s_stoch": 0.6100, "s_macd": 1.0000,
  "s_ema": 0.5500, "s_mfi": 0.1800
}
```

Fires via `alert(t_json, alert.freq_once_per_bar_close)` on each confirmed bar when `i_telemetry = true`.

---

## 9. Pine v6 notes

- No line continuations — all multi-line expressions use parentheses grouping.
- `position` is an enum namespace; mapped via `f_tbl_pos()` helper to avoid type annotation errors.
- `ta.mfi(hlc3, len)` — 2-argument form required in Pine v6.
- `request.security()` with `lookahead=barmerge.lookahead_off` is the correct anti-lookahead syntax.

---

## 10. Synthetic validation

| Test | Series | Expected |
|------|--------|---------|
| A — Bull | `close = 100 + i*0.05` | f_score → +0.7–+0.9, BUY 70–90% |
| B — Bear | `close = 100 - i*0.05` | f_score → -0.7–-0.9, SELL 70–90% |
| C — Flat | `close = 100 + sin(i*0.3)*0.2` | f_score near 0, conf < 55%, no TP |
| D — Neutral Zone | Same as C + `i_neutral_zone=true` | `is_neutral=true`, TP suppressed |
