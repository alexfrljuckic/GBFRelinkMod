# Drop-Rate & Gacha Table Modding — UNLOCKED on 2.0 (2026-07-09)

The two tables behind drop rates and gacha/Transmarvel rolls (`reward_lot`, `gacha_lot`)
changed layout in Endless Ragnarok, so GBFRDataTools' bundled 1.x schemas rejected them
(`did not match expected size, it's larger`). We reverse-engineered the new 2.0 layouts
and wrote updated `.headers` — **both now round-trip byte-identical**, and a live
weight-edit was verified end-to-end. Drop-rate mods can be made/updated on 2.0 today.

## How the format works

A `.tbl` is: `int64 rowCount` then `rowCount × RowSize` bytes. GBFRDataTools derives
`RowSize` from the table's `Headers/<name>.headers` file (lines `add_column|Name|type`;
`hash_string`/`int`/`uint`/`hex_uint` = 4 bytes, `byte` = 1) and asserts
`8 + RowSize×rowCount == fileLength`. When 2.0 adds a column, the file is bigger than the
header expects → the assert throws. Fix = update the header to the true 2.0 layout.

Reversing a layout: dump the file's implied row size (`(len−8)/rowCount`), then profile
every 4-byte column across all rows (distinct-count, empty-hash `0x887AE0B0` presence,
value range) to tell hashes from ints and locate the meaningful fields. Scripts:
[scripts/analyze-hud-markers.mjs](../scripts/analyze-hud-markers.mjs)-style profiling was
reused inline. **The proof of a correct header is a byte-identical `tbl→sqlite→tbl`.**

## `reward_lot` (drop tables) — 56 → 60 bytes, +1 column

RowSize 60, 24561 rows. New 2.0 header ([patches/headers-2.0/reward_lot.headers](../patches/headers-2.0/reward_lot.headers)):

| Offset | Column | Type | Notes |
|---|---|---|---|
| 0x00 | Key | int | reward-group key (0..750) |
| 0x04 | Unk04 | int | always 0 (likely hi-dword/pad) |
| 0x08 | **ItemId** | hash_string | the dropped item (7984 distinct) |
| 0x0C | WeaponId | hash_string | |
| 0x10 | GemId | hash_string | |
| 0x14 | Unk14 | hash_string | new/extra hash (mostly empty) |
| 0x18 | RewardRank | int | |
| 0x1C | PhaseItemIndex | int | |
| 0x20 | AmountGiven | int | |
| 0x24 | WeaponLevel | int | |
| 0x28 | WeaponUncap | int | |
| 0x2C | GemCount | int | |
| **0x30** | **Weight** | uint | **THE drop weight** (0..1,000,000) — edit this |
| 0x34 | NumExtraGemTraits | int | |
| 0x38 | StoryDifficulty + pad1..3 | byte×4 | |

Verified: setting one item's `Weight` 100→999999 in SQLite changed exactly its 0x30 field
in the exported `.tbl`, nothing else.

## `gacha_lot` (gacha / Transmarvel-Wrightstone rolls) — 24 → 28 bytes, +1 column

RowSize 28, 988 rows. 2.0 just **appended** one field; the first 6 columns are unchanged
([patches/headers-2.0/gacha_lot.headers](../patches/headers-2.0/gacha_lot.headers)):

`QuestIDMin`(hex_uint) · `QuestIDMax`(hex_uint) · `Key`(hash) · `ItemId`(hash) ·
**`Weight`(uint @0x10)** · `TraitLevel`(uint) · `Unk18`(int, new 0/1 flag).

`gacha_rate_group` was unchanged in 2.0, so the Transmarvel mod's other table already works.

## The workflow (make/update a drop-rate mod)

These headers are already installed in our local GBFRDataTools (`tools/GBFRDataTools/Headers/`,
1.x originals kept as `*.1x.bak`). SQLite CLI is at `tools/sqlite/sqlite3.exe`
(or use [SQLiteStudio](https://sqlitestudio.pl/)).

```bash
# 1. extract the vanilla 2.0 table
GBFRDataTools.exe extract -i "<game>\data.i" -f "system/table/reward_lot.tbl" -o work
# 2. convert to SQLite (2.0 header now handles it)
GBFRDataTools.exe tbl-to-sqlite -i work/system/table -o reward_lot.sqlite -v 2.0.2
# 3. edit Weight values in SQLite (sqlite3.exe / SQLiteStudio)
#    e.g.  UPDATE reward_lot SET Weight = Weight*10 WHERE Weight > 0;   (10x rarer-drop odds)
# 4. convert back
GBFRDataTools.exe sqlite-to-tbl -i reward_lot.sqlite -o out -v 2.0.2
# 5. ship out/reward_lot.tbl in a Reloaded-II mod at:
#    (Mod)\GBFR\data\system\table\reward_lot.tbl   (depends on the GBFR Mod Manager)
```

This is exactly what's needed to **re-port the broken loot mods** (Endgame Rebalance Plus,
Custom Drop Rates, More Fortitude, Slimepede) or build a fresh one.

## Upstream

The headers ([patches/headers-2.0/](../patches/headers-2.0/)) are a clean PR to
[GBFRDataTools](https://github.com/Nenkai/GBFRDataTools) — they unblock every drop-rate/
gacha mod for the whole community. Remaining changed tables that mods want next: `item`,
`constant` (item metadata / globals — bigger schemas, not yet reversed).

## `reward` (per-quest reward definitions) — 44 → 52 bytes, +2 columns (2026-07-09)

RowSize 52, 6292 rows. 2.0 inserted **two hash columns at 0x28/0x2C** (populated on only
159 rows — the Endless Ragnarok quests) between `RewardPointIdMSP` and
`Lot1ExclusionChance`; all 1.x columns keep their order. Header:
[patches/headers-2.0/reward.headers](../patches/headers-2.0/reward.headers) —
round-trips byte-identical. `Key` is `RW_<questId>_<slot>` (e.g. `RW_402314_100`;
resolves via ids.txt), and the same quest number recurs across difficulty-band id
prefixes (`402314`/`407314`/`40A314`/`40B314`) — the hook for per-difficulty reward
mods like "voucher per Chaos+ clear".

## Caveats
- `ItemId` exports as the raw XXHash (e.g. `403A1E7B`) when the hash isn't in the tool's
  id database — match items by hash, or extend `ids.txt`. Fine for weight edits.
- `Unk04/Unk14/Unk18` names are provisional; round-trip is lossless regardless, but confirm
  semantics before *interpreting* them.
