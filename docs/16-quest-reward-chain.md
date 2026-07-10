# The Quest → Reward Chain, Decoded (2026-07-09)

Reversed while building the voucher mod ([mods/voucher-quests/](../mods/voucher-quests/)).
This corrects/extends the provisional column names from [docs/11](11-droprate-modding-unlocked.md).

## The chain

```
quest_baseinfo_ex_data (the quest catalog, 576 rows)
  Key         = the quest id itself (hex_uint, e.g. 0x0040A314)
  RewardID/2  → reward.tbl rows that hold ONLY exp/gold/MSP point ids (not item drops)
  AdvisedPWR  = difficulty scale;  Unk2_0 (new 2.0 col @0x6C) = content flag (5 = ER)

reward.tbl (6292 rows) — keys follow a NAME CONVENTION hashed at runtime:
  RW_<questId>_100 / _101   per-clear reward packages (both patched by the voucher mod)
  RW_<questId>_300.._305    graded/badge draws
  RW_<questId>_SUBM_01..    side objectives;  RW_<questId>_EM<id> per-enemy packages
  each row: RewardLotId1..6 → lots;  slot 1 is subject to Lot1ExclusionChance

reward_lot.tbl (24561 rows) — docs/11 called these columns wrong; true semantics:
  @0x08 "ItemId"   = **LotId** — the hash that RewardLotIdN references (7984 distinct lots;
                     a lot's rows = its weighted outcome pool)
  @0x0C "WeaponId" = the granted ITEM or weapon (e.g. ITEM_10_0001, or a weapon hash)
  @0x10 "GemId"    = the granted sigil (GEEN_xxx_yy)
  Weight 10000 on a single-row lot = guaranteed outcome;  AmountGiven = count
  RewardRank/StoryDifficulty (-1/255 = any) filter rows within a lot
```

A "guaranteed item per clear" mod is therefore: append a single-row lot (Weight 10000)
to `reward_lot`, then point an empty `RewardLotIdN` slot of the quest's `RW_<qid>_100/_101`
rows at it. New id strings are fine — GBFRDataTools hashes unknown strings with the game's
custom XXHash32 on `sqlite-to-tbl` (verified byte-level).

## 2.0 difficulty tiers and quest-id bands

Text ids `TXT_QUEST_DIFFICULT_HELL1/2/3/INFINITY` name the post-Proud ladder; the quest
catalog's id bands + AdvisedPWR confirm the mapping:

| Tier | Band | Quests | AdvisedPWR |
|---|---|---|---|
| Chaos | `4083xx` | 11 | 23–25k |
| Chaos+ | `4093xx` | 14 | 26–30k |
| Chaos++ | `40A3xx` | 26 | 32–37k |
| Infinity | `40B3xx` ("On the Threshold of …") | 5 | 45–46k |

(`4073xx`, ER-flag 4, 18–30k = 2.0 additions to the Proud band — below Chaos, excluded.)

## New 2.0 header
`quest_baseinfo_ex_data`: 108 → 112 bytes, one uint appended (`Unk2_0` @0x6C, values 0–5 —
an Endless Ragnarok content flag). Header in [patches/headers-2.0/](../patches/headers-2.0/),
round-trips byte-identical. That makes **four** reversed 2.0 tables
(reward_lot, gacha_lot, reward, quest_baseinfo_ex_data).

## The voucher mod build (repeatable)
`node scripts/build-voucher-mod.mjs` — reads the fresh-extracted `reward`/`reward_lot`
sqlite, appends lots `RWL_TMV_1/2/3` (Transmarvel Voucher ×1/×2/×3 — `ITEM_21_0001`,
found via 2.0 text.msg, docs/15), and wires all 56 Chaos+ quests' `_100`/`_101` rows
(112 slots; slot 1 avoided). Verified: reward.tbl byte-diff = exactly the 112 patched
dwords; reward_lot grows by exactly 3 rows.

## Open questions (test live)
- Do `_100` and `_101` both fire per clear (or first-clear vs repeat)? Both are patched;
  worst case = one extra grant on first clear.
- `reward_lot.Key` int semantics (new rows copy Key=5 from a guaranteed-lot template).
