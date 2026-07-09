# Toolchain Setup Checklist (deferred)

*Not executed yet — this session was research-only. Written 2026-07-09 so a future
session (or a human with a terminal) can run it top to bottom on the machine that has
the game installed. Sources:
[file extraction](https://nenkai.github.io/relink-modding/tutorials/file_extraction/),
[installing mods](https://nenkai.github.io/relink-modding/modding/installing_mods/).*

## 0. Prerequisites

- [ ] Granblue Fantasy: Relink installed (Steam). Note the path:
      `<Steam>\steamapps\common\Granblue Fantasy Relink`
- [ ] **.NET 8.0 SDK/runtime** (GBFRDataTools requirement)
- [ ] Disk headroom: a full `extract-all` roughly doubles the game's footprint — prefer
      filtered extraction (`-f`) unless researching broadly
- [ ] In-game: `Game Options → Other → Play Log → Do Not Agree` (telemetry opt-out
      recommended by the modding docs)

## 1. Player-side stack (run mods)

- [ ] Download Reloaded-II (`Setup.exe` or `Release.zip`) from
      [releases](https://github.com/Reloaded-Project/Reloaded-II/releases); install/extract,
      launch `Reloaded-II.exe`
- [ ] `+` → add application → select `Granblue Fantasy Relink.exe`
- [ ] Install the mod loader: download `gbfrelink.utility.manager.7z` from
      [GitHub releases](https://github.com/WistfulHopes/gbfrelink.utility.manager/releases)
      (want ≥ **2.0.1** for Endless Ragnarok) and drag-drop into Reloaded-II
      (not running as admin — Windows blocks drag-drop onto elevated processes), or
      extract manually into Reloaded-II's `Mods\` folder
- [ ] Enable "Granblue Fantasy Relink Mod Manager" checkbox
- [ ] Smoke test: install any current 2.0-compatible mod from
      [Nexus](https://www.nexusmods.com/games/granbluefantasyrelink) (drag-drop the zip),
      enable, **Launch Application** from Reloaded-II, verify in-game

## 2. Extraction/authoring stack (make mods)

- [ ] Download [GBFRDataTools](https://github.com/Nenkai/GBFRDataTools/releases) —
      **check first whether a post-2026-07-08 release with Endless Ragnarok file-list
      support exists**; extract next to the game exe (docs' recommendation)
- [ ] Test single-file extraction:
      ```
      GBFRDataTools.exe extract -i "<game>\data.i" -f "system/table/item.tbl"
      ```
      (any known path works; tables are under `system/table`)
- [ ] Extract the table folder for gameplay modding:
      ```
      GBFRDataTools.exe extract-all -i "<game>\data.i" -f "system/table" -o <research dir>
      ```
- [ ] Install [SQLiteStudio](https://sqlitestudio.pl/); convert and open:
      ```
      GBFRDataTools.exe tbl-to-sqlite -i <table folder> -o tables.sqlite -v <version>
      ```
- [ ] Keep a **pristine vanilla copy** of everything extracted, per game version
      (you'll need 1.x *and* 2.0 vanillas to port mods)

## 3. C# mod dev stack (Path A in [04-code-mods.md](04-code-mods.md))

- [ ] Visual Studio 2022 (or Rider) with .NET desktop workload
- [ ] Follow the [Reloaded-II mod creation tutorial](https://reloaded-project.github.io/Reloaded-II/)
      — create a project from the Reloaded II Mod template
- [ ] Add NuGet: `gbfrelink.utility.manager.Interfaces`; set the GBFR Mod Manager as the
      mod's dependency
- [ ] Clone the reference mod:
      `git clone https://github.com/Nenkai/gbfr.qol.weaponglowcontrol`

## 4. C++/ASI dev stack (Path B — GBFRelinkFix work)

- [ ] Visual Studio 2022 C++ workload + CMake
- [ ] `git clone https://codeberg.org/Lyall/GBFRelinkFix` — build `main`, then check out
      the **`ragnarok`** branch
- [ ] Reverse-engineering tools: [Ghidra](https://ghidra-sre.org/) (free) and/or IDA;
      [x64dbg](https://x64dbg.com/) for live debugging
- [ ] Know the uninstall path: mods of this type = `winmm.dll`/`dinput8.dll`/`version.dll`
      + `scripts\` in the game folder; delete those to restore vanilla
- [ ] If mixing ASI mods with Reloaded-II: Reloaded-II → Edit Application →
      Advanced Tools & Options → **Deploy ASI Loader**

## 5. Community

- [ ] Join the Relink Modding Discord (invite linked from
      [nenkai.github.io/relink-modding](https://nenkai.github.io/relink-modding/))
- [ ] Create a [Nexus Mods](https://www.nexusmods.com/) account (publishing)

## Update-day hygiene (before any future game patch)

- [ ] Rename `orig_data.i` → `data.i` (undo the loader's index swap)
- [ ] Remove ASI loader DLLs + `scripts\` folder
- [ ] Consume any over-capped modded items (they recap to 999)
- [ ] Expect table + code mods to break; re-check the
      [compatibility page](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/)
