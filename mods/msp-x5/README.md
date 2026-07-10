# Mastery Points ×5 (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on future
> updates and I won't patch it every time. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Every quest grants **5× mastery points (MSP)** on clear — story, side, and all
difficulties. EXP and rupie payouts are untouched (their reward entries are fully
separate in the data; verified zero overlap).

## Install
Same stack as the other mods (Reloaded-II +
[GBFR Mod Manager 2.0.1+](https://www.nexusmods.com/granbluefantasyrelink/mods/526)):

1. Download **`msp-x5-v1.zip`** from the
   [msp-x5-v1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/msp-x5-v1).
2. Extract into `Reloaded-II\Mods\` → `Mods\gbfr.quest.msp5x\`.
3. Enable **Mastery Points x5 (2.0)** in the game's mod list; launch.

Composes cleanly with [Transmarvel Overhaul](../transmarvel-overhaul/) (different
table). ⚠️ Disable any other mod shipping `reward_point.tbl`.
Troubleshooting (mods not loading at all): see the
[Overhaul README](../transmarvel-overhaul/README.md#if-something-blows-up-troubleshooting).

## How it works
One table: `reward_point.tbl` (layout unchanged in 2.0). Quest rewards reference
per-quest point entries via `reward.tbl`'s `RewardPointIdExp/Gold/MSP` — the 3,407
MSP entries share zero keys with EXP/gold, so multiplying their `Min`/`Max` ×5
(3,882 rows) touches nothing else. Byte-diff verified: every changed field is a
`Min`/`Max` at exactly 5× vanilla. Change the multiplier by re-running the UPDATE
in [docs/11](../../docs/11-droprate-modding-unlocked.md)'s workflow:
`UPDATE reward_point SET Min=Min*5, Max=Max*5 WHERE Key IN (SELECT DISTINCT
RewardPointIdMSP FROM reward WHERE RewardPointIdMSP!='');`
