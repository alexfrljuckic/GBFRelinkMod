# Summon Drops Maxed (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on
> future updates. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Two knobs for 2.0's summon inventory, both on by default and toggleable in
Reloaded-II (right-click the mod → *Configure Mod*):

| Feature | Vanilla | With this mod |
|---|---|---|
| Summon drop chance | 35/50/70/100% depending on the source | **always 100%** (71 of 177 sources raised) |
| Skill levels on dropped summons | ranges like Lv4–6 / Lv11–15, top level rarest (Lv15 at ~10%) | **always the top level of the pool** |

Which summon drops and which skills appear in its pools stay vanilla-random —
this mod only removes the drop coin-flip and the level lowball.

## Install

Requires [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II/releases) +
[GBFR Mod Manager](https://www.nexusmods.com/granbluefantasyrelink/mods/526) (**2.0.1+**).

1. Download **`summon-drops-v1.0.zip`** from the
   [summon-drops-v1.0 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/summon-drops-v1.0).
2. Extract into `Reloaded-II\Mods\` → `Mods\gbfr.summon.drops\`.
3. Enable **Summon Drops Maxed (2.0)** in the game's mod list; launch.
4. Every setting change needs a **game restart** — tables are read once, at launch.

> ⚠️ Disable any other mod shipping `reward_summon.tbl` or `summon_curve.tbl` —
> this mod generates both. (Safe alongside Transmarvel Overhaul — no shared tables.)

## How it works

Same architecture as [Transmarvel Overhaul](../transmarvel-overhaul/): a C#
Reloaded-II mod reads the **vanilla** tables from the game archive at launch via
the GBFR Mod Manager's `IDataManager`, patches them in memory, and registers the
result — nothing on disk is modified, and layout guards refuse to patch (loudly,
in the Reloaded console) if a game update changes a row size.

- **Guaranteed drops** — `reward_summon.tbl` (177 rows) carries each reward
  source's summon drop chance at offset 0x08 (35/50/70/100). All raised to 100.
- **Max skill levels** — `summon_curve.tbl` (73 rows, 37 lots) holds the weighted
  skill-LEVEL lots that summon skill rolls draw from (`Group, SkillLevel, Weight`).
  Each lot's top level gets weight 10000, the rest 0.

Full decode of the summon drop chain (`reward.tbl → reward_summon →
reward_summon_lot → summon.tbl → summon_lot → summon_curve`):
[docs/23](../../docs/23-summon-skill-drops.md).

Source: [mods-src/gbfr.summon.drops/](../../mods-src/gbfr.summon.drops/)
(build: `tools\dotnet9sdk\dotnet.exe build -c Release`).

## Uninstall
Untick the mod (or delete `Mods\gbfr.summon.drops\`). Game files and your save
are never modified.
