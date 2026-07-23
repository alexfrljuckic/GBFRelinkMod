# Summon Drops Maxed (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on
> future updates. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Three knobs for 2.0's summon inventory, all on by default and toggleable in
Reloaded-II (right-click the mod → *Configure Mod*):

| Feature | Vanilla | With this mod |
|---|---|---|
| **Guaranteed drops** | 35/50/70/100% depending on the source | **always 100%** (71 of 177 sources raised) |
| **Max skill levels** | ranges like Lv4–6 / Lv11–15, top level rarest (Lv15 at ~10%) | **always the top level of the pool** |
| **Boost 5★ chase skills** *(v1.1)* | each top summon's best passive throttled to **8%** | that skill raised to a **configurable share** (default 40%, up to 100% = guaranteed) |

Which *summon* drops from each source stays vanilla-random. What the mod removes is
the drop coin-flip, the level lowball, and — new in v1.1 — the deliberate 8% throttle
on each 5★ summon's best passive skill.

### The chase skills (what "Boost 5★ chase skills" targets)

Every top-tier summon has **one** premium passive in its slot-1 pool pinned to 8%,
while its filler skills sit at ~18–31%. This feature finds that throttled skill and
raises it to your chosen odds (the filler skills split the rest; at 100% only the
chase skill can roll). The 19 summons with a chase skill:

| Summon(s) | Chase skill (vanilla 8%) | Filler skills |
|---|---|---|
| Lucilius | **Berserker Echo** | Alpha, Beta, Gamma, Tyranny, Celestial Terra |
| Wilinus Icewyrm | **Berserker Echo** | Guts, Glaciate Res., Nimble Defense, Celestial Lumen, Guard Payback |
| Goldslime | **War Elemental** | Path to Mastery, Rupie Tycoon, Fast Learner |
| Lilith | **War Elemental** | Uplift, Potion Hoarder, Tyranny, Linked Together, Improved Healing |
| Rolan | **War Elemental** | Uplift, Autorevive, Quick Cooldown, Drain, Aegis |
| Beelzebub | **Spartan Echo** | DMG Cap, Supplementary DMG, Drain, Celestial Lumen, Improved Guard |
| Evyl Blackwyrm | **Spartan Echo** | Divergence, Stronghold, Celestial Incendo, Slow Res., Steel Nerves |
| Behemoth | **Stout Heart** | Supplementary DMG, Uplift, Celestial Ventus, Less Is More, Crit Hit Rate |
| Vrazarek Firewyrm | **Stout Heart** | DMG Cap, Uplift, Burn Res., Quick Cooldown, ATK |
| Ancient Dragon | **Auto Potion** | Greater Aegis, Guts, Less Is More, Drain, HP |
| Wheel of Fate | **Auto Potion** | Supplementary DMG, Quick Charge, Precise Wrath |
| Silverslime | **Improved Dodge** | Path to Mastery, Rupie Tycoon, Fast Learner |
| Pyet-A | **Improved Dodge** | Nimble Onslaught, Stronghold, Celestial Terra, Autorevive, Improved Guard |
| Radis Whitewyrm | **Improved Dodge** | Stronghold, Potion Hoarder, Celestial Incendo, Uplift, Celestial Aqua |
| Albacore | **Natural Defenses** | Path to Mastery, Rupie Tycoon, Fast Learner |
| Elusious Windwyrm | **Natural Defenses** | Supplementary DMG, Divergence, Celestial Terra, Cascade, Improved Guard |
| Corvell Earthwyrm | **Potent Greens** | Autorevive, DMG Cap, Sandtomb Res., Celestial Terra, Celestial Nyx |
| Ennugi | **Potent Greens** | Divergence, Greater Aegis, Less Is More, Slow Res., Glass Cannon |
| Wee Pincer | **Crabvestment Returns** | DMG Cap, Cascade, Steel Nerves |

*(5★ pools with no single throttled skill — everything already equal odds — are left
vanilla. Skill **levels** on every summon, chase or filler, are maxed regardless.)*

**Full reference generated from the game data**: [DROP-TABLES.md](DROP-TABLES.md) —
every stage that can drop a summon, which summons (with weights), and each
summon's complete skill pools with odds and level ranges.

## Install

Requires [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II/releases) +
[GBFR Mod Manager](https://www.nexusmods.com/granbluefantasyrelink/mods/526) (**2.0.1+**).

1. Download **`summon-drops-v1.1.zip`** from the
   [summon-drops-v1.1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/summon-drops-v1.1).
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
