# 21 — Save-file sigil inventory + trait assignments decoded (2026-07-10)

Found while making `build-jackpot-tables.mjs` auto-detect owned Warpath+ sigils
instead of a hand-edited list — then extended to per-copy **secondary traits**
after Alex pointed out dupes are per (sigil × 2nd trait) combo, not per sigil.
Verified against a real 2.0.2 save (23 MB `SaveData1.dat`, endgame profile,
2928 owned sigils).

## Where
`%localappdata%\GBFR\Saved\SaveGames\SaveData<N>.dat` (plus `_BackUp`/`_BackUp2`
siblings the game rotates). Not encrypted, not compressed — item hashes are
findable by raw byte scan.

## Record format
Sigils appear as 24-byte records, 4-byte aligned:

| offset | field | notes |
|---|---|---|
| 0x00 | listType u32 | which list this record belongs to (below) |
| 0x04 | seq u32 | unique, descending per list — meaning is per-list: plain instance/sort id in catalog+inventory, `instance*100+slot` in the trait pool |
| 0x08 | const u32 = 4 | category? (4 = sigil) — use as a scan guard |
| 0x0C | const u32 = 1 | count? always 1 for sigils — second scan guard |
| 0x10 | itemHash u32 | **custom XXHash32** of the `GEEN_xxx_yy` id (docs/15 hash, seed `0x178A54A4`) |
| 0x14 | tag u32 | increments by 0x18 per record — serializer bookkeeping, ignore |

Three lists matter for sigils (offsets differ per profile AND move between
saves of the same profile — the game rewrites the file as you play, so always
re-derive them in one read; listType constants may not survive game updates):

| listType | records here | meaning |
|---|---|---|
| `0x1F41` | 1032, all unique | **catalog** — one record per sigil that *exists* (matches the 1,024+ names in text.msg, docs/15). NOT ownership. |
| `0x0A8F` | 5100 slots (2928 filled + 2172 `887AE0B0` empties) | **inventory** — one record per *owned copy*; filled slots contiguous at the array's end |
| `0x06A5` | 25968 | **trait pool** — per-copy trait assignments (mostly `887AE0B0` empties; 4178 real SKILL hashes here) |

The inventory object is a family of parallel 5100-slot arrays (`0x0A8D`,
`0x0A8E` per-instance counter, `0x0A8F` sigil hash, `0x0A90`, `0x0A92`,
`0x0A93` …) — sibling arrays sit back-to-back, same index = same sigil slot.

**Trait pool linkage** (the non-obvious part): a pool record's `seq` encodes
`instanceN * 100 + traitSlot`. Trait slot 0 = the sigil's **innate** trait,
slot 1 = the random **secondary**, slot 2 = third trait (5256 records; none on
Warpath+ copies). Sigil slot `j` in the inventory array maps to pool instance
`N = C − j` for a constant `C` derivable by consensus, because of a lucky
redundancy: **an innate trait's SKILL number equals the sigil's GEEN number**
(`GEEN_121_93` White Dragon's Warpath+ → `SKILL_121_02` "White Dragon's
Warpath", names confirm via `extracted/skill-names-en.tsv`). Voting C across
all filled slots gave 2927/2928 agreement — the lone exception is a `_94`
reissued-single (`GEEN_151_94` → `SKILL_156_00`), a real quirk, not a mapping
error. Empty inventory slots map to all-empty pool pairs (2172/2172 ✓).

Ownership of a **combo** = filled slot with that GEEN hash whose pool pair
slot 1 is that SKILL hash. Plain-string ids and UTF-16 both absent — hashes
only. `ITEM_*` currency/consumable hashes live elsewhere (seen near 0x2da630
in a different structure, not decoded).

## Consumers
- `scripts/build-jackpot-tables.mjs` — `OWNED_WARPATH = 'auto'` reads the newest
  slot and prunes a Warpath+ from the Transmarvel pool only when **all 18
  curated secondaries** (SKL_TMV_GOOD) are owned for it — dupes are per
  (sigil × secondary) combo, and the pool's granularity is per sigil, so
  "prunable" = "no pull can roll anything new". Locates arrays dynamically
  (duplicate-hash test rejects the catalog), re-derives C, and demands ≥99%
  innate-number agreement before trusting anything; fails loudly → manual
  list fallback. Read-only.

## Gotchas hit along the way
- **GBFRDataTools `ids.txt` is stale for the newest sigils**: `GEEN_173_93`–
  `GEEN_178_93` export from `tbl-to-sqlite` as raw hash strings (`41AC1082`…),
  so SQL matching by id name silently misses them — match on both the id and its
  uppercase-hex hash (build-jackpot-tables does now). Extending the tool's
  ids.txt would fix it at the source (contribution surface, see docs/13).
- The running game holds `data.0` locked → archive extraction fails while
  playing; build-jackpot-tables falls back to the vanilla tables cached in
  `extracted/2.0/system/table/`.
- Unknowns kept unknowns: whether the `0x1F41`/`0x0A8F` type ids are stable
  across game versions is unverified — hence the loud sanity check instead of
  silent trust.
