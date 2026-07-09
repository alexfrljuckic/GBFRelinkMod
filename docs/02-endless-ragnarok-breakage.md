# What Endless Ragnarok Broke, and Why

*Status as of 2026-07-09 — one day post-launch. Primary source:
[Endless Ragnarok and Mods](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/).
This page WILL go stale fast; re-check the source before acting.*

*Endless Ragnarok* (paid expansion) + patch 2.0.2 (free, crossplay) shipped **2026-07-08**
([PlayStation Blog](https://blog.playstation.com/2026/06/18/granblue-fantasy-relink-endless-ragnarok-hands-on-report-demo-available-today/),
[Steam](https://store.steampowered.com/app/3839790/Granblue_Fantasy_Relink__Endless_Ragnarok_Upgrade_Kit_Standard_Edition/)).
Mod breakage on a version jump this big was expected; here's the per-category picture.

## Breakage by category

### Table / gameplay mods — mostly broken ❌
The expansion **shifted columns** in the `.tbl` structures (new columns for Master Traits,
Weapon Transcendence, new characters, etc.). A 1.x table written back into a 2.0 game
misaligns data. **Every table mod needs a manual re-port** — no automated migration.
This is our main opportunity category; see [03-table-modding.md](03-table-modding.md).

### Code-injection mods — mostly broken, recovering ❌→🔧
The 2.0 executable was **built with a different compiler**, so the generated machine code
changed shape everywhere. Mods that find their hook points by **byte-pattern scanning**
(all ASI mods, some Reloaded-II mods) fail to match — the well-behaved ones no-op,
the rest crash the game.

Already updated per the ER page: *Detailed SBA & Enemy Percentage*, *Auto-copy Session
ID*, *Automatic Power Adjustment*, *Discord Rich Presence*.

Not yet updated: [GBFRelinkFix](https://codeberg.org/Lyall/GBFRelinkFix) — crashes the
game at boot ([issue #1](https://codeberg.org/Lyall/GBFRelinkFix/issues/1), opened
2026-07-08: black screen then back to Steam library, even with all features disabled;
only fix is removing the mod). A **`ragnarok` branch exists** in the repo, so Lyall is on
it; a commenter notes the changes look substantial. See
[05-first-projects.md](05-first-projects.md).

### Model mods — broken but with a workaround ⚠️
Mods touching **material files (`.mmat`)** need updating, but loader v2.0.1 added an
**"Auto-Upgrade .mmat constant buffer"** config option that force-upgrades old model mods
at load time ([release notes](https://github.com/WistfulHopes/gbfrelink.utility.manager/releases)).

### Item-uncap mods — incompatible, with a save hazard ☠️
Incompatible until updated. **Save-safety note from the ER page**: consume modded
(over-capped) items *before* updating the game, or they recap to 999.

## Tool status

| Tool | 2.0 status (2026-07-09) |
|---|---|
| Mod loader `gbfrelink.utility.manager` | ✅ **v2.0.1** (released 2026-07-08) supports 1.3 *and* 2.0; adds `.mmat` auto-upgrade + `IUserDefinedParams` API for querying game version/type |
| GBFRDataTools | ⚠️ Works, but **lacks the ER file list** — new-content paths are unknown hashes until the list updates. Marked "pending contribution" on the ER page. Nenkai paused ER research during the closed beta, resuming after release ([README](https://github.com/Nenkai/GBFRDataTools)) |
| File Name Logger | 🔧 Updated locally by Nenkai; public release planned alongside the ER update |

## How the community recovers from an update (the playbook)

1. **Loader first** — update file redirection / index handling so *existing* known files
   still resolve (done, day one).
2. **Re-discover paths** — run the File Name Logger while playing new content to capture
   the new file paths the game requests; contribute them to the GBFRDataTools file list.
   Until this lands, new-expansion files can't be extracted by name.
3. **Re-map tables** — diff old vs new `.tbl` schemas, update the version-aware converters
   (`tbl-to-sqlite -v`), then mod authors re-port their edits column by column.
4. **Re-find patterns** — code-mod authors locate their hook sites in the new executable
   (new byte signatures) and re-release.

## Troubleshooting lore worth remembering

From the [ER page](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/)
and [install guide](https://nenkai.github.io/relink-modding/modding/installing_mods/):

- Game misbehaving after an update? Delete **`winmm.dll`, `dinput8.dll`, `version.dll`**
  and the **`scripts/`** folder from the game folder if present — that's the ASI-loader
  stack (i.e., mods like GBFRelinkFix), the usual crash suspect after a recompile.
- Before a game update: rename `orig_data.i` back to `data.i` (undo loader index swap).
- SpecialK/GBFRelinkFix + Reloaded-II coexistence: use Reloaded-II's
  *Edit Application → Advanced Tools & Options → Deploy ASI Loader*.
