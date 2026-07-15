# Summon skill drops — table chain decoded (2026-07-14)

2.0 / Endless Ragnarok added a **summon inventory**: summons drop from rewards like
sigils, each with randomly rolled skills. Goal: a mod to adjust the drop rate of the
skills (and skill levels) that roll on summons. This doc maps the data chain; no
mod is built yet. All offsets verified against `extracted/2.0/system/table/*.tbl`.

UI naming (from `text_ui.msg`): "summon inventory", "Call Summon", HUD "Summon {0}",
Yorozu "Trade Summons" (`TXT_DLG_TTL_YRZ_SMNMSP_UNLOCK` — MSP trade). Summon display
names live in **`system/table/text/en/text_sum.msg`** — not yet extracted (game was
running, archive locked; re-run `GBFRDataTools extract -f "system/table/text/en/text_sum.msg"`
with `DOTNET_ROOT=tools\dotnet10`).

## The chain (drop → summon → skills)

```
reward.tbl row (Key, e.g. RW_<qid>_100)
  └─ reward_summon.tbl        Key == reward.tbl Key; grants a summon roll
       └─ reward_summon_lot.tbl   which summon drops (weighted)
            └─ summon.tbl             the summon definition
                 ├─ summon_lot.tbl        skill pool per skill slot (weighted)   ← THE SKILL-RATE DIAL
                 │    └─ summon_curve.tbl     skill LEVEL distribution (weighted) ← THE LEVEL DIAL
                 └─ (slot 2 = summon-exclusive skills, ids unresolved)
```

## Tables (2.0 layouts, `int64 rowCount` + fixed rows)

### `reward_summon.tbl` — 177 rows × 20 B
| off | field | notes |
|---|---|---|
| 0x00 | Key (hash) | == a `reward.tbl` row Key. 23/177 resolve as quest per-clear rows `RW_<qid>_100`; the other 154 are other reward sources (Endless packages? unresolved) |
| 0x04 | LotGroup (hash) | → `reward_summon_lot` group |
| 0x08 | Chance | 100 / 70 / 50 / 35 (%) |
| 0x0C, 0x10 | 1, 1 | count? |

### `reward_summon_lot.tbl` — 587 rows × 16 B
| off | field | notes |
|---|---|---|
| 0x00 | Group (hash) | keyed from `reward_summon` |
| 0x04 | SummonKey (hash) | == `summon.tbl` Key @0x10. 174 distinct summons reachable (of 189) |
| 0x08 | Weight | 10000-basis |
| 0x0C | −1 | |

### `summon.tbl` — 189 rows × 36 B (the summon definitions)
| off | field | notes |
|---|---|---|
| 0x00 | Skill1LotA (hash or empty `887AE0B0`) | 152 rows use A-columns … |
| 0x04 | Skill1LotB (hash or empty) | … 37 rows use B-columns (why two positions: unknown; both → `summon_lot` groups) |
| 0x08 | Skill2LotA (hash or empty) | slot-2 pools contain ONLY the unresolved skill ids |
| 0x0C | Skill2LotB (hash or empty) | |
| 0x10 | **Key** (hash) | referenced by `reward_summon_lot` |
| 0x14 | Unk (hash) | maybe name/text or info ref — resolve after text_sum.msg |
| 0x18 | Rarity | 3★=46, 4★=78, 5★=65 rows |
| 0x1C | Series/model id | 0–250 |
| 0x20 | 0 | |

### `summon_lot.tbl` — 726 rows × 20 B ← **the skill-rate dial**
| off | field | notes |
|---|---|---|
| 0x00 | Group (hash) | 224 groups; 186 slot-1, 38 slot-2 |
| 0x04 | SkillId (hash) | slot-1: `SKILL_xxx_00` ids — the same skill hashes as the sigil 2nd-trait system (Aegis `E0ABFDFE` etc.). 22 slot-2 ids unresolved (summon-exclusive; no `SKILL_/SUM_/SMN_` pattern matched) |
| 0x08 | CurveId (hash) | → `summon_curve` group (level distribution) |
| 0x0C | Weight | basis points; groups sum to 10000 |
| 0x10 | 0 | |

Example slot-1 pool (group `cc0b0ef1`): Berserker Echo **8%** (fixed Lv15) +
Alpha/Beta/Gamma/Tyranny/Celestial Terra 18.4% each (Lv11–15 curve). Rare-skill
rows (weight 800) pair with the Lv15-only curve — the "legend" tier.

### `summon_curve.tbl` — 73 rows × 12 B, 37 groups ← **the level dial**
`(Group, SkillLevel, Weight)`. Fixed-level groups (`[n, 10000]` for n=1..15) serve
the deterministic entries; random curves e.g. `cd4bea10` = Lv4 35% / Lv5 33% / Lv6 32%,
`8e4a0e03` = Lv11 22% / Lv12 21% / Lv13 20% / Lv14 19% / Lv15 **10%**.

### `summon_legend_skill.tbl` — 9 rows × 4 B
The "legend" skill list (the 8%-tier passives): Berserker Echo, Spartan Echo,
War Elemental, Improved Dodge, Stout Heart, Potent Greens, Auto Potion,
Natural Defenses, `SKILL_141_04` (Crabby Resonance family variant).

### Others
- `summon_info.tbl` 77×104 — probably the species/gallery list (189 summon rows ≈
  variants per species). Undecoded.
- `summon_preset.tbl` 513×48 — starts with UTF-8 "プリセット１…" strings: equip
  preset definitions, not drop-related.
- `summon_param*/base_param/constant/sell` — stats/pricing, not drop-related.
- `gacha` tables: summons do NOT appear in the Yorozu gacha buckets; the Yorozu
  "Trade Summons" (SMNMSP) route is likely shop/trade tables — unexplored.

## Mod design sketch (same architecture as transmarvel-overhaul)

All knobs are plain `.tbl` content → launch-time rebuild via the GBFR Mod Manager
`IDataManager`, layout-guarded, nothing on disk. Candidate knobs:

1. **Skill filter/boost** — rebuild `summon_lot.tbl`: per slot-1 group, zero/boost
   weights of ticked skills (re-normalize to 10000), analogous to the 2nd-trait
   filter. Skills reuse the SAME hashes as the trait catalog → the existing
   skill-catalog.json infrastructure can drive checkboxes.
2. **Level floor** — rebuild `summon_curve.tbl`: e.g. clamp random curves to
   Lv15-only (`[15,10000]`), or boost the Lv15 weight.
3. **Which summon drops** — `reward_summon_lot.tbl` weights, and/or raise
   `reward_summon.Chance` (35/50/70 → 100).

## text_sum.msg extracted (2026-07-14, later the same session)

`extracted/summon-names-en.tsv` (532 keys). Findings:
- **Summon names**: `TXT_SMN_So<hexid>` — 77 distinct `So####` ids (== the 77
  `summon_info` rows). Summons are enemy-based: Lucilius, Beelzebub, Rolan,
  Proto Bahamut Transcendent Blue, Goblin Gladiator, Timber Wolf, Wyvern, …
- **`summon_info` @0x30 (offset 48) = xxh32 of the name key** (`TXT_SMN_So####`),
  77/77 — summon_info is the species table.
- **Slot-2 skills are summon CALL EFFECTS**: `TXT_SMN_BDY_So####` texts describe
  them ("summon gave healing potions!", "grants Regen/Jammed/Stout Heart/Debuff
  Immunity/Supplementary DMG", "restored the critical gauge", "increased the
  link level") — ~23 texts vs the 22 unresolved slot-2 skill ids. Their id
  string scheme is still unguessed (no `SKILL_/SMN_/SUMMON_+So####` pattern
  hashes to them), but semantically slot 2 = the summon's active call effect.

## Open items (before building)

1. Link `summon.tbl` rows ↔ `summon_info`/So-ids (no shared hash column found
   yet; try `summon_param` or the int series id @0x1C) — needed only for
   per-summon config names; global skill/level knobs don't require it.
2. Slot-2 (call effect) id scheme — brute patterns failed; not needed for a
   slot-1 skill-rate mod.
3. Resolve the remaining 154 `reward_summon` keys (what non-quest sources drop
   summons — Endless packages?).
4. **When is the skill rolled?** Presumably at drop time (like sigil traits) —
   confirm live: edit a summon_lot group to a single skill, drop a summon, check.
5. Save-side summon inventory format (for a future auto-prune analog) — docs/21
   covers sigils only.
6. `summon.tbl` A/B column split (152 vs 37 rows) — meaning unknown.
