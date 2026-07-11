# RNG Knob Map — Transmarvel, Curios, Boss Drops (2026-07-09)

Reversed from the live 2.0 tables so any drop-rate mod (config, preset, or overlay) targets
the *exact* values. All tables here are **OK/unchanged in 2.0** except `reward_lot`/`gacha_lot`
which we reversed ([11](11-droprate-modding-unlocked.md)) — so everything below is editable now.

## The Yorozu sigil-forging system (Curios **and** Transmarvel share it)

`gacha.tbl` is the Rosetta Stone: curios and Transmarvel are both "forging" tiers. Three
tables chain together:

### `gacha.tbl` — the forging tiers
| Tier (LevelNameTextId) | GemChance% | WrightstoneChance% | GemRateGroup | WrightstoneRateGroup | Cost |
|---|---|---|---|---|---|
| FORGING_01 | 100 | 0 | `489B6160` | — | 5 voucher |
| FORGING_02 | 95 | 5 | `B209049D` | `BBAEDB0D` | 10 voucher |
| FORGING_03 | 80 | 20 | `57AB19DE` | `B845CAC6` | 25 voucher |
| FORGING_HIGH | 75 | 25 | `27509C51` | `67716D8A` | 150 transmarvel |

Editable per tier: `GemChancePercent`/`WrightstoneChancePercent` (gem-vs-wrightstone odds),
`GemRateGroup`/`WrightstoneRateGroup` (which rate table), `TransmarvelCost`/`VoucherCost`.

### `gacha_rate_group` — **THE primary rarity dial**
Columns: `Key` (group), `GachaLotId` (→ outcome bucket), `Weight` (probability), **`Unk4`**.
**`Unk4 = 1` marks the rare/high-tier outcome** in each group; its `Weight` is the rarity.
The rarest dials in the game right now:

| Group | Rare outcome | Weight (Unk4=1) | vs group commons |
|---|---|---|---|
| `67716D8A` (HIGH wrightstone) | `BD1CBF1C` | **10** | commons in thousands |
| `57AB19DE`, `27509C51` | `1F44C95D` | **20** | vs 6400 |
| `489B6160` | `7D5CC8F6` | **700** | vs 6400/2900 |

→ **To make rare sigils/traits common: raise the `Weight` of `Unk4=1` rows** (e.g. 20 → 2000).

### `gacha_lot` — the outcome contents
`QuestIDMin/Max` (tier scope), `Key`, `ItemId`, `Weight` (**uniform 50** — not the dial),
`TraitLevel` (**0 = normal, 15 = max trait**), `Unk18` (new 2.0 flag). Rarity is *not* here;
it's in `gacha_rate_group`. `TraitLevel 15` rows are the max-trait sigils.

**Summary — the three Transmarvel/curio knobs:**
1. `gacha_rate_group.Weight` where `Unk4=1` — makes rare outcomes hit more often (main dial).
2. `gacha.tbl` `GemChancePercent`/`WrightstoneChancePercent` — better output odds per tier.
3. `gacha.tbl` `TransmarvelCost`/`VoucherCost` — cheaper rolls.

## Boss / quest drops

- **`reward_lot`** (unlocked, 60B): `Weight` (@0x30) per `(Key, ItemId, RewardRank)`. Rare =
  low weight → boost low-weight rows. Grouped by reward rank / `StoryDifficulty`
  (−1=any, 0=Story,1=Action,2=Hard,3=Ultimate). `Weight` ranges 0..1,000,000.
- **`reward_item_rare` / `reward_geen_rare`**: small `(MainQuestTypeMin/Max → Key)` lookups
  routing quest types to the rare reward group in `reward_lot`.
- **`enemy_reward`**: per-enemy (boss) drop definitions. Unchanged, converts fine.

## Endless Ragnarok endgame drops (new 2.0 content)

`endlessmode_lot`, `endlessmode_albacore_drop`, `endlessmode_area_lot`,
`endlessmode_treasurebox_point`, `endlessmode_buff_rank_lot`, `chest_reality_lot` — the
Conflux/endgame loot. All **OK/unchanged** → moddable now with the same workflow.

## Random 2nd trait on + sigils (corrected 2026-07-10)

The 2nd-trait chain `gem.SkillTypeLotIdForRandom2ndSkill → skill_type_lot → skill_lot`
only covers the gem-referenced type-lots **1–5**. **Transmarvel max-trait grants roll
through `skill_type_lot` Key=14 instead** (5 sub-lots × 20%), which no `gem` row
references — the grant path selects it directly. Proven empirically: with lots 2–5
rerouted and staged (manager log confirmed), fresh Transmarvel pulls still rolled
junk from exactly lot 14's five sub-lot families
(`scripts/audit-save-sigil-traits.mjs` against the save, docs/21 format).

**Second correction, same day**: rerouting lot 14 too was staged (log-verified) and
junk STILL rolled — so the grant path's lot selection doesn't honor `skill_type_lot`
keys the way any of our models assumed (row-index lookup? exe-side copy of the
routing? unknown). What DOES hold: the rolled traits always come from the
`skill_lot` sub-lot pool. The reliable knob is therefore **`skill_lot` row content**
(SkillId @+4, Weight @+8, 12-byte rows), not the type-lot pointers. transmarvel-overhaul
now enforces its trait filter there.

**Third correction — RESOLVED (2026-07-11)**: content-level filtering was live-proven
to drive the roll, and the residual ~33-35% junk across two live batches (30+ pulls,
snapshot-diffed) was traced to a single cause: sub-lot **`8F952AC1`** — the one the
strict per-path legality intersection left vanilla (its six referencing type-lots
5/6/15/16/26/27 share no common ticked trait). Every junk trait observed post-fix
survives ONLY there. So the roll consumes `skill_lot` content through some
`8F952AC1`-referencing lot (consistent with a row-INDEX lookup: index 5 = key-6 row,
whose 8F952AC1 slice is 33% ≈ observed junk rate) — and the lot-14 attribution was
family-overlap coincidence. Fix: relaxed legality fallback for empty-intersection
sub-lots (ticked ∩ union-of-referencing-unions). The exe never needed disassembling.

## Caveat: hash resolution
`ItemId`/`Key` are XXHash32 values; the tool's `Data/ids.txt` (26k entries) resolves mostly
ability/text IDs, not item names — so **item-level** targeting inside `reward_lot` means
matching hashes. But the **system-level** knobs (forging tiers, gacha rates, gem/wrightstone
odds) are fully human-readable via `gacha.tbl`'s `TextId`s — those are the digestible dials a
UX would expose.

## Implication for the tiered UX plan ([12](12-realtime-rng-ux-design.md))
The most digestible, high-impact knobs are all in the small `gacha.tbl` + `gacha_rate_group`
tables (not the 24k-row `reward_lot`). A config mod or overlay slider that boosts
`gacha_rate_group.Weight (Unk4=1)` and bumps `gacha.tbl` chances would directly deliver
"rare sigils/traits, less rare" with a handful of well-named values.
