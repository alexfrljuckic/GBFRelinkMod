# Transmarvel Overhaul (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on future
> updates and I won't patch it every time. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Transmarvel, fixed — and **fully configurable in Reloaded-II** (right-click the mod
→ *Configure Mod*). Everything below is a checkbox or a number; the mod rebuilds the
game's tables from vanilla data + your settings at **every game launch**, so nothing
on disk is ever modified and your settings apply instantly on the next launch.

| Feature | Vanilla | With this mod |
|---|---|---|
| Transmarvel sigil roll | ~1000-entry pool, chase odds 0.2–4% | only the chase sigils **you tick** (up to 41), equal odds |
| Random 2nd trait on "+" sigils | ~65% junk (resistances, Rupie Tycoon…) | only the traits **you tick**, always vanilla-legal combos |
| Transmarvel wrightstones | Lv20-main stones at 0.1–25% | **always Lv20 main**, subs fixed or rolled from your ticked traits (knob) |
| Duplicate protection | none | completed Warpath+ combos **auto-leave the pool** (reads your save) |
| Voucher income | none from quests | configurable per-clear grants, down to any difficulty tier |
| Roll cost | 150 points | configurable (1–9999) |

Quick references generated from the actual game data:
[DROP-TABLES.md](DROP-TABLES.md) (what can drop with the current settings) ·
[LEGAL-COMBOS.md](LEGAL-COMBOS.md) (every combo the vanilla game can produce).

## Install

Requires [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II/releases) +
[GBFR Mod Manager](https://www.nexusmods.com/granbluefantasyrelink/mods/526) (**2.0.1+**).
First Reloaded-II setup: [Installing Mods — relink-modding](https://nenkai.github.io/relink-modding/modding/installing_mods/).

1. Download **`transmarvel-overhaul-v2.2.zip`** from the
   [transmarvel-overhaul-v2.2 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/transmarvel-overhaul-v2.2).
2. Extract into `Reloaded-II\Mods\` → `Mods\gbfr.transmarvel.overhaul\`.
3. Enable **Transmarvel Overhaul (2.0)** in the game's mod list; launch.

> ⚠️ Disable any other mod shipping `gacha_rate_group.tbl`, `gacha_lot.tbl`, `gacha.tbl`,
> `reward.tbl`, `reward_lot.tbl`, `skill_lot.tbl`, or `skill_type_lot.tbl`
> (e.g. Endgame Rebalance Plus, Transmarvel Wrightstone Max Level) — this mod
> generates all of them.

## Configuration guide (Configure Mod)

Every setting needs a **game restart** — the game reads tables once, at launch.
The Reloaded console prints exactly what was applied, e.g.:

```
[gbfr.transmarvel.overhaul] Voucher income applied: 25x Transmarvel Voucher (5+5+5+5+5 across slots) on 302 rows across 175 quests (tier >= 1).
[gbfr.transmarvel.overhaul] Auto-prune: read 2311 sigils from the save; no Warpath+ is combo-complete yet.
[gbfr.transmarvel.overhaul] Sigil pool applied: 13/41 sigils at ~7.69% each; wrightstones pinned tier-3 Transmarveled.
[gbfr.transmarvel.overhaul] 2nd-trait filter applied: 7 traits ticked (…); 390 lot entries remapped across 36 sub-lots.
[gbfr.transmarvel.overhaul] Transmarvel cost set to 150 points.
```

### Sigil Pool (73 checkboxes; 41 on by default)
On by default: the 13 top V+ generics (War Elemental+, Supplementary Damage V+,
Berserker Echo+, Spartan Echo+, Greater Aegis V+, Celestial ×6 V+, Fatebreaker V+,
Divergence V+) and all 28 character **Warpath+** (max trait). **Opt-in bucket
(off by default): the 28 character Awakening+** (max trait; the bucket's four
stat-V+ singles are also listed, also off). Ticked sigils share the pool at equal
odds; untick to remove. **Untick everything = vanilla pool** (overhaul off).
The 75/25 sigil-vs-wrightstone split stays vanilla.

### Wrightstone drops
Wrightstone skills are **not rolled from the sigil trait pool** — each wrightstone
item has its skills defined in `item_pendulum.tbl`. The *Wrightstone drops* setting
picks which Lv20-main stones the 25% wrightstone side can grant:
`0` = the Transmarveled fixed-sub stones (always Aegis 15 / ATK 10; vanilla's 0.1%
tier, default), `1` = the vanilla high-tier stones whose **two subs roll at
Lv15/Lv10 — from your ticked 2nd traits**, since their sub rolls go through the
same filtered trait lots, `2` = both 50/50. Either way, no more sub-Lv20 mains.

### Sigil Pool — Auto (duplicate protection)
*Auto-remove completed sigils* (default on): at every launch the mod reads your
newest save — **read-only, it never writes your save** — and removes from the pool:
- any ticked **Warpath+** you own with *every* 2nd trait it can still roll (your
  ticked traits) — a pull could give you nothing new; it returns automatically
  when you tick more traits (new combos to chase);
- any ticked **Awakening+** you own **at all** — duplicates are useless, so one
  copy retires it.

Your manual unticks always win. If the save can't be parsed (e.g. after a game
update changes the format), the mod **fails open**: pool left unpruned, reason
printed to the console.

### 2nd Traits (72 checkboxes)
One checkbox per trait that can roll as a random secondary on any "+" sigil.
Default: a curated 11 (DMG Cap, Tyranny, Stamina, Quick Charge, Cascade, Quick
Cooldown, Uplift, Guts, Autorevive, Potion Hoarder, Steady Focus) — no
resistances, no filler. **Untick everything = vanilla trait lots** (filter off).

Two guarantees, enforced by construction:
- **Only your ticks can roll.** Verified live: an 80-pull test produced 67/67
  sigils inside the ticked sets.
- **Only vanilla-legal combos are generated.** A (sigil × 2nd trait) pair is
  produced only if the unmodded game could produce it. Ten "supreme" traits
  (Supplementary DMG, Glass Cannon, Berserker, …) can never roll on Transmarvel
  pulls in vanilla, so ticking them doesn't add them there — the checkbox
  descriptions mark these. See [LEGAL-COMBOS.md](LEGAL-COMBOS.md) for the full
  legality table (41 mains × 62 legal secondaries = 2,542 combos).

Scope note: the 2nd-trait roll is a property of the sigil, not of where it
dropped — so the filter applies to "+" sigils from **any** source (curios
included). There is no per-source hook in the game data; the upside is junk
secondaries can't roll anywhere.

### Voucher Income
- *Vouchers per quest clear* (default 10): guaranteed Transmarvel Vouchers on
  every eligible quest clear. **Engine limit**: the game clamps each reward
  grant to 5 and a reward row has at most 5 slots, so the effective ceiling is
  **25 per clear** — and quests whose reward rows are already full of vanilla
  drops grant less. Counterintuitively, **easy early-board quests pay the most**
  (their reward rows are nearly empty).
- *Voucher quests: minimum difficulty tier* (default 8 = Chaos and above):
  lower it to farm on easier boards — tier 1 covers everything from the PWR-200
  starter quests up. Tiers above Chaos always grant.
- *Transmarvel cost* (default 150 = vanilla): points per roll. Set **1** to make
  every curio transmute fund 1–15 rolls (they earn 1/5/15 points) — the fastest
  way to bulk-test your settings. Don't ask for 0; the game's menu divides by
  the cost, so the mod clamps to 1 to protect you from a crash we found the
  hard way.

## How it works (for the curious / other modders)

**Nothing is patched on disk and no static tables ship with the mod.** A single
C# Reloaded-II mod runs at game launch, reads the *vanilla* tables out of the
game archive via the GBFR Mod Manager's `IDataManager`, applies your config, and
registers the rebuilt tables in memory. Because everything derives from vanilla
data each launch, the mod survives minor game-data updates — and every patch
step is guarded by table-layout checks that **refuse to patch** (loudly, in the
console) if a game update changes a row size, rather than corrupting anything.

The four generators, in launch order:

1. **Voucher income** (`ApplyVouchers`) — enumerates quest-board quests from
   `quest_baseinfo_ex_data.tbl` (key band `0040T3xx`, `T` = difficulty tier
   1–B), appends guaranteed-voucher lots to `reward_lot.tbl` (split into ≤5
   chunks because the engine clamps any single grant to 5), and wires them into
   the free `RewardLotId` slots of each quest's per-clear reward rows in
   `reward.tbl`. New quests added by game updates are covered automatically.
   Reward-chain decode: [docs/16](../../docs/16-quest-reward-chain.md).
2. **Sigil pool** (`ApplySigilPool`) — filters the three Transmarvel chase
   buckets in `gacha_lot.tbl` to your ticked sigils and reweights
   `gacha_rate_group.tbl` at 250/sigil (equal per-item odds; wrightstone side
   pinned to the tier-3 Transmarveled lot). Before filtering, the auto-pruner
   parses your save's sigil inventory + per-copy trait assignments
   (`SaveReader.cs`; save format reverse-engineered in
   [docs/21](../../docs/21-save-sigil-inventory.md)) and drops combo-complete
   Warpath+. The parser self-validates on every read — a sigil's innate-trait id
   number always equals its sigil id number in vanilla data, and if less than
   99% of the inventory satisfies that, the parse is rejected and pruning is
   skipped. Pool decode: [docs/15](../../docs/15-transmarvel-pool-decoded.md).
3. **2nd-trait filter** (`ApplyTraitFilter`) — the subtle one. The game rolls a
   secondary by picking a *sub-lot* (a weighted bucket in `skill_lot.tbl`) and
   then a trait inside it. Pointer-level rerouting of the bucket *selection*
   (`skill_type_lot.tbl`) turned out not to affect live rolls, so the filter
   works at **content level**: every trait entry inside every sub-lot that isn't
   ticked is remapped onto ticked traits — with a **legality rule**: a sub-lot
   only receives traits that the vanilla game could roll through the paths
   referencing that sub-lot (strict: legal on *every* referencing path; if no
   ticked trait qualifies, relaxed: legal on *at least one*). Whatever bucket
   the game picks, only ticked, vanilla-legal traits can come out. The full
   diagnosis story — including the single unfiltered bucket that leaked
   resistances at a 33% rate until an 80-pull live test pinned it — is in
   [docs/13](../../docs/13-rng-knob-map.md).
4. **Roll cost** (`ApplyTransmarvelCost`) — one cell in `gacha.tbl`
   (the Transmarvel tier's `TransmarvelCost`), clamped to ≥1.

The checkbox catalogs (`SigilCatalog.cs`, `TraitCatalog.cs`) and the JSON
mirrors (`sigil-catalog.json`, `trait-catalog.json`) are **generated from the
game's own data + English text dump** — no hand-typed ids anywhere. Hashing is
the game's custom XXHash32 (seed `0x178A54A4`; decode in
[docs/15](../../docs/15-transmarvel-pool-decoded.md)).

Source: [mods-src/gbfr.transmarvel.overhaul/](../../mods-src/gbfr.transmarvel.overhaul/)
(build: `tools\dotnet9sdk\dotnet.exe build -c Release`). Save-parser test
harness: [tools-src/savereader-test/](../../tools-src/savereader-test/) —
differentially tested against an independent JS implementation (byte-identical
on real saves); run it after game updates before trusting auto-prune.
Verification tooling: `scripts/snapshot-save.mjs` before a pull session, then
`scripts/audit-save-sigil-traits.mjs --since <snapshot>` for a per-pull verdict
of everything you rolled. Architecture rationale (config = preferences,
mechanics = programmatic): [docs/22](../../docs/22-config-vs-programmatic.md).

## If something blows up (troubleshooting)

- **"Oh Noes! … An Application Control policy has blocked this file (0x800711C7)"** —
  Windows **Smart App Control** is blocking Reloaded's unsigned community DLLs. Fix:
  *Windows Security → App & browser control → Smart App Control settings → Off*
  (one-way switch; regular SmartScreen stays on). Hits fresh Windows 11 installs where
  SAC silently flips itself to enforcing.
- **Mods silently don't apply** (rolls look vanilla, no Reloaded console window):
  Reloaded never injected — GBFR's Steam DRM breaks launcher injection. In Reloaded-II:
  *Edit Application → Advanced Tools & Options → Deploy ASI Loader*. Tell: no new log in
  `%APPDATA%\Reloaded-Mod-Loader-II\Logs` = no injection.
- **A setting didn't take** — check the console lines (screenshot above): every
  generator logs what it applied or *why it refused*. Two known traps:
  changing config requires a **game restart**, and after updating the mod's dll,
  restart the **Reloaded launcher too** before opening Configure Mod (a stale
  launcher session silently drops config fields it doesn't know yet).
- **Verify it's on**: the Reloaded console should show the five
  `[gbfr.transmarvel.overhaul]` lines from the configuration guide above.

## Uninstall
Untick the mod (or delete `Mods\gbfr.transmarvel.overhaul\`). Game files and your
save are never modified — all tables are injected in memory at launch, and the
save is only ever read.
