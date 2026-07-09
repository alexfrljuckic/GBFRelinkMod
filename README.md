# Granblue Fantasy: Relink — Modding Research

Research notes and working documentation for getting into **GBF Relink mod development**,
started 2026-07-09 — one day after the *Endless Ragnarok* expansion launched and broke a
large share of the existing mod ecosystem.

## State of the world (as of 2026-07-09)

[*Endless Ragnarok*](https://relink-ragnarok.granbluefantasy.com/en/dlc/) shipped
**2026-07-08** alongside the free 2.0.2 patch (crossplay, 6 new characters, endgame
overhaul). Per the community's official
[Endless Ragnarok and Mods](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/) page:

- The **mod loader** ([gbfrelink.utility.manager v2.0.1](https://github.com/WistfulHopes/gbfrelink.utility.manager/releases))
  was updated same-day and supports both game 1.3 and 2.0.
- **Table/gameplay mods are mostly broken** — the expansion shifted columns in the `.tbl`
  data structures; every affected mod needs manual re-porting.
- **Code-injection mods mostly broke** — the game was recompiled with a different
  compiler, so byte-pattern scans no longer match. Some Reloaded-II mods are already updated.
- **GBFRDataTools** (the core extraction tool) still needs a community-contributed
  **file list update** for the new content — several modding tasks are gated on it.

That breakage is the opportunity: lots of in-demand mods need updating right now.

## Documents

| Doc | What it covers |
|---|---|
| [01-ecosystem.md](docs/01-ecosystem.md) | The modding landscape: archives, toolchain, who maintains what, publishing, etiquette |
| [02-endless-ragnarok-breakage.md](docs/02-endless-ragnarok-breakage.md) | Exactly what 2.0 broke, why, and current tool-support status |
| [03-table-modding.md](docs/03-table-modding.md) | Gameplay modding via `.tbl` → SQLite → `.tbl`, and porting mods to 2.0 |
| [04-code-mods.md](docs/04-code-mods.md) | The two code-mod architectures: Reloaded-II C# vs ASI/C++ pattern-scan |
| [05-first-projects.md](docs/05-first-projects.md) | Ranked concrete entry points, anchored on updating GBFRelinkFix for 2.0 |
| [06-toolchain-setup.md](docs/06-toolchain-setup.md) | Setup checklist — extraction stack DONE 2026-07-09 (see its setup log); Reloaded-II + dev stacks pending |
| [07-2.0-table-compat-audit.md](docs/07-2.0-table-compat-audit.md) | **Original research**: per-table audit of what 2.0 changed — 55 of 304 tables have new layouts |
| [08-gbfrelinkfix-status.md](docs/08-gbfrelinkfix-status.md) | **Original research**: GBFRelinkFix ported to 2.0 — crash fix + world markers + Span HUD, all live-verified |
| [09-hud-markers-debugging.md](docs/09-hud-markers-debugging.md) | **Original research**: in-mod diagnostic method (beats anti-debug), HUD Markers + Span HUD struct RE |
| [10-modding-opportunities.md](docs/10-modding-opportunities.md) | Alex's Reloaded-II setup + per-mod 2.0 breakage, what's moddable now, ranked opportunities |
| [11-droprate-modding-unlocked.md](docs/11-droprate-modding-unlocked.md) | **Original research**: reversed the 2.0 `reward_lot`/`gacha_lot` layouts → new headers, byte-exact round-trip, drop-rate editing unlocked |

## Link index

- **Central modding documentation**: https://nenkai.github.io/relink-modding/ (Nenkai) —
  the authoritative community resource; also links the Relink Modding Discord
- **GBFRDataTools**: https://github.com/Nenkai/GBFRDataTools — extraction/conversion toolkit
- **Mod loader**: https://github.com/WistfulHopes/gbfrelink.utility.manager — Reloaded-II mod
- **Reloaded-II**: https://github.com/Reloaded-Project/Reloaded-II — the mod framework/manager
- **Nexus Mods game page**: https://www.nexusmods.com/games/granbluefantasyrelink — where mods publish
- **GBFRelinkFix** (ultrawide fix, our anchor project): https://codeberg.org/Lyall/GBFRelinkFix
- **Example C# API mod**: https://github.com/Nenkai/gbfr.qol.weaponglowcontrol

> Freshness warning: the scene is mid-scramble after the expansion. Anything here marked
> with a date reflects that day's status and may be stale within weeks — re-check the
> [Endless Ragnarok and Mods](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/)
> page before acting on compatibility claims.
