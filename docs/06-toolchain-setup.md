# Toolchain Setup Checklist

*Extraction stack executed 2026-07-09 on this machine (game found at
`D:\Steam\steamapps\common\Granblue Fantasy Relink`) — see **Actual setup log** at the
bottom for what was done and corrections to the published docs. Player-side (Reloaded-II)
and dev stacks still pending. Sources:
[file extraction](https://nenkai.github.io/relink-modding/tutorials/file_extraction/),
[installing mods](https://nenkai.github.io/relink-modding/modding/installing_mods/).*

## 0. Prerequisites

- [x] Granblue Fantasy: Relink installed (Steam):
      `D:\Steam\steamapps\common\Granblue Fantasy Relink` (2.0/ER build, files dated
      2026-07-08; `data.11` = the new 18 GB ER archive; exe is
      **`granblue_fantasy_relink.exe`**, lowercase — not the `Granblue Fantasy Relink.exe`
      the install guide mentions)
- [x] **.NET 10 runtime** — the docs say .NET 8, but GBFRDataTools **1.5.1 targets
      .NET 10** (launch error names `Microsoft.NETCore.App 10.0.0`); installed via
      `winget install Microsoft.DotNet.Runtime.10`
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

- [x] Download [GBFRDataTools](https://github.com/Nenkai/GBFRDataTools/releases) —
      installed **1.5.1** to `C:\dev\GBF\tools\GBFRDataTools\` (gitignored). No post-ER
      *release* exists yet, **but master got the ER file list on 2026-07-09**
      ("filelist: 369354/408165 (90.49%)") — we replaced the bundled `filelist.txt`
      with master's (377,590 paths; old one kept as `filelist.txt.v1.5.1.bak`).
      To refresh later:
      ```
      curl -sL -o filelist.txt https://raw.githubusercontent.com/Nenkai/GBFRDataTools/master/GBFRDataTools/filelist.txt
      ```
- [x] Extract the table folder for gameplay modding — worked against 2.0 (304 tables →
      `extracted/2.0/system/table`, gitignored):
      ```
      GBFRDataTools.exe extract-all -i "D:\Steam\steamapps\common\Granblue Fantasy Relink\data.i" -f "system/table" -o "C:\dev\GBF\extracted\2.0"
      ```
- [x] Convert to SQLite — **partially works on 2.0**: whole-folder conversion dies on
      the first table whose layout 2.0 changed (`ap_open_rank ... larger`). 249 of 304
      tables still convert (see [07-2.0-table-compat-audit.md](07-2.0-table-compat-audit.md));
      converting just those produced `extracted/tables-2.0-partial.sqlite`:
      ```
      GBFRDataTools.exe tbl-to-sqlite -i <folder of OK tables> -o tables-2.0-partial.sqlite -v 2.0.2
      ```
- [ ] Install [SQLiteStudio](https://sqlitestudio.pl/) (GUI, still pending — any SQLite
      client works on the generated db)
- [ ] Keep a **pristine vanilla copy** of everything extracted, per game version
      (you'll need 1.x *and* 2.0 vanillas to port mods)

### Actual setup log (2026-07-09)

- Tools live in `C:\dev\GBF\tools\` (gitignored): `GBFRDataTools\` (1.5.1 + master
  filelist) and `dotnet10\` (user-local .NET 10.0.9 runtime via the
  [dotnet-install script](https://dot.net/v1/dotnet-install.ps1) — winget wanted UAC).
- Run the tool with the local runtime:
  ```
  DOTNET_ROOT=C:\dev\GBF\tools\dotnet10 tools\GBFRDataTools\GBFRDataTools.exe ...
  ```
- Doc corrections found: tool targets **.NET 10** (not 8); game exe is
  `granblue_fantasy_relink.exe`; `tbl-to-sqlite -v` takes any version string and
  validates row sizes against its bundled `.headers` schemas.
- Upstream status at time of setup: ER **file list on master since 2026-07-09**
  (no release yet); **table schema headers NOT yet updated for 2.0**
  (`GBFRDataTools.Database` last touched 2026-03-04) — that's the current gate for
  modding the 55 changed tables.

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
