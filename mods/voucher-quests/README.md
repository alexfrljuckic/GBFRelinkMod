# Transmarvel Vouchers from Chaos+ Quests (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on future
> updates and I won't patch it every time. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Every quest clear at **Chaos difficulty and above** grants guaranteed
**Transmarvel Vouchers**, scaled by tier:

| Difficulty | Quest band | Vouchers per clear |
|---|---|---|
| Chaos | `4083xx` | **1** |
| Chaos+ | `4093xx` | **1** |
| Chaos++ | `40A3xx` | **2** |
| Infinity | `40B3xx` (the "On the Threshold of …" quests) | **3** |

All 56 Chaos+ quests in the 2.0 catalog are covered. Everything below Chaos is untouched.
Pairs naturally with [Transmarvel Jackpot](../transmarvel-jackpot/) (what the vouchers
roll into).

## Install

Same stack as the Jackpot mod — Reloaded-II +
[GBFR Mod Manager](https://www.nexusmods.com/granbluefantasyrelink/mods/526) (2.0.1+):

1. Download **`transmarvel-vouchers-v1.zip`** from the
   [transmarvel-vouchers-v1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/transmarvel-vouchers-v1).
2. Extract into `Reloaded-II\Mods\` so you get
   `Mods\gbfr.transmarvel.vouchers\GBFR\data\system\table\{reward.tbl, reward_lot.tbl}`.
3. Enable **Transmarvel Vouchers from Chaos+ Quests (2.0)** in the game's mod list.
4. Clear a Chaos+ quest, check the reward screen.

> ⚠️ Conflicts: disable any other mod that ships `reward.tbl` or `reward_lot.tbl`
> (e.g. **Endgame Rebalance Plus**, Custom Drop Rates mods) — file-level, last-loaded-wins.
> Troubleshooting (mods not loading at all): see the
> [Jackpot README](../transmarvel-jackpot/README.md#if-something-blows-up-troubleshooting).

## How it works

Two tables (both reversed for 2.0 in this repo — headers in
[patches/headers-2.0/](../../patches/headers-2.0/)):

- **`reward_lot.tbl`**: +3 appended "guaranteed lot" rows (`RWL_TMV_1/2/3` → Transmarvel
  Voucher `ITEM_21_0001` ×1/×2/×3, Weight 10000 = the lot's only outcome).
- **`reward.tbl`**: for each Chaos+ quest's per-clear reward rows (`RW_<questId>_100` and
  `_101`), one empty `RewardLotId` slot is pointed at the tier's voucher lot
  (112 rows patched; slot 1 avoided because of `Lot1ExclusionChance`).

Quest→tier mapping comes from `quest_baseinfo_ex_data` (also reversed for 2.0: +1 column;
`Key` = quest id, `AdvisedPWR` confirms band ordering) and the `TXT_QUEST_DIFFICULT_HELL1/2/3
/INFINITY` text ids (Chaos / Chaos+ / Chaos++ / Infinity).

Rebuild: `node scripts/build-voucher-mod.mjs` (see repo).

## Known unknowns (v1)
- `_100` and `_101` are both patched; if both fire on a first clear you may get a
  double grant once per quest. Harmless.
- The `reward_lot.Key` value (5) on the new rows is copied from an existing guaranteed
  lot; its exact semantics are unverified.
