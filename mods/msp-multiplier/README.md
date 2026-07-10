# Mastery Points Multiplier (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0**. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Every quest grants **multiplied mastery points (MSP)** on clear — **configurable in
Reloaded-II, default ×5** (1–100). EXP and rupie payouts are untouched (their reward
entries are fully separate in the data; verified zero overlap).

Unlike a table-replacement mod, this is a small code mod: at game launch it reads the
**vanilla** reward tables from the game archive, multiplies exactly the MSP entries,
and hands the patched table to the GBFR Mod Manager. That means it survives minor
game-data updates (and refuses to patch — loudly, in the console — if a game update
ever changes the table layouts, instead of corrupting rewards).

## Install

1. Download **`msp-multiplier-v1.zip`** from the
   [msp-multiplier-v1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/msp-multiplier-v1).
2. Extract into `Reloaded-II\Mods\` → `Mods\gbfr.quest.mspmultiplier\`.
3. Enable **Mastery Points Multiplier (2.0)** in the game's mod list; launch.

Requires [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II/releases) +
[GBFR Mod Manager 2.0.1+](https://www.nexusmods.com/granbluefantasyrelink/mods/526).
Composes with [Transmarvel Overhaul](../transmarvel-overhaul/) (no shared files).
Conflicts with any mod shipping `reward_point.tbl`.
Troubleshooting: [Overhaul README](../transmarvel-overhaul/README.md#if-something-blows-up-troubleshooting).

## Change the multiplier

Reloaded-II → select the game → select **Mastery Points Multiplier (2.0)** →
**Configure Mod** → set **MSP Multiplier** (1–100, default 5) →
**restart the game** (reward tables are read once, at launch). The console line
`[gbfr.quest.mspmultiplier] MSP x<N> applied — 3882 reward_point entries patched`
confirms it's active.

## How it works / build from source

Source: [mods-src/gbfr.quest.mspmultiplier/](../../mods-src/gbfr.quest.mspmultiplier/)
(C# Reloaded-II mod). Uses the Mod Manager's `IDataManager` API:
`GetArchiveFile()` → patch `Min`/`Max` on the `reward_point` entries referenced by
`reward.tbl`'s `RewardPointIdMSP` (3,407 keys, zero overlap with EXP/gold) →
`AddOrUpdateExternalFile()` + `UpdateIndex()`. Layout guards bail out if row sizes
change. The algorithm is verified byte-identical to the (retired) static-table
version of this mod. Build: `dotnet build -c Release` with .NET 9 SDK
(`tools/dotnet9sdk`), needs the Mod Manager installed for the interfaces reference.
