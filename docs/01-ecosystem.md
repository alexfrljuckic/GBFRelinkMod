# The Relink Modding Ecosystem

*Researched 2026-07-09. Primary source:
[nenkai.github.io/relink-modding](https://nenkai.github.io/relink-modding/).*

## How the game stores its data

Everything lives in the game folder
(`<Steam>\steamapps\common\Granblue Fantasy Relink`), per the
[file extraction tutorial](https://nenkai.github.io/relink-modding/tutorials/file_extraction/):

- **`data.i`** — the index. A FlatBuffers structure (internally named *FlatArk*) describing
  every file the game knows about. File **paths are hashed with XXHash64** (the index stores
  hashes, not names — which is why the community maintains a *file list* mapping hashes back
  to real paths), and content is **LZ4-compressed**.
- **`data.0, data.1, …`** — the archive containers holding the actual file contents.
- **`data/`** — loose external files (sound).

Two consequences that shape all modding:

1. **You can't enumerate file names from the archive** — only hashes. New game content is
   invisible to tooling until someone discovers the new paths (via a *File Name Logger*
   that records paths as the game requests them, plus guesswork from naming conventions)
   and contributes an updated file list. As of v1.3.2 the list covered
   [~98.3% of paths](https://github.com/Nenkai/GBFRDataTools).
2. **Mods don't repack the archive.** The mod loader adds files as *external files* and
   updates the index, so a mod is just a folder of files mirroring extracted paths.

## The toolchain

| Tool | Maintainer | Role |
|---|---|---|
| [GBFRDataTools](https://github.com/Nenkai/GBFRDataTools) | Nenkai | The swiss-army knife (.NET 8, CLI): extract from `data.i`, `.tbl`↔SQLite, textures (`wtb`/`tex`↔`dds`/`png`), `.bxm`↔XML, `.msg`↔JSON, YAML conversion of UI formats, save-file handling |
| [gbfrelink.utility.manager](https://github.com/WistfulHopes/gbfrelink.utility.manager) | WistfulHopes | **The mod loader** — a [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) mod every other mod depends on. Handles file redirection (no game-folder tampering since v1.3.0), auto-converts authoring formats (`.json`→`.msg`, `.xml`→`.bxm`), spoofs `.minfo` versions, warns on mod conflicts, exposes a C# API |
| GBFR File Name Logger | Nenkai | Logs file paths the game requests — how new paths get discovered after updates |
| [GBFR Blender Tools](https://nenkai.github.io/relink-modding/) | community | Import/export game models in Blender (out of scope for us; models/textures track) |
| GraniteTextureReader | community | Texture (Granite tiled-texture) extraction (same track) |

Mod management alternatives exist
([RelinkModOrganizer](https://github.com/RokyZevon/RelinkModOrganizer),
[Zetas-Workshop/Relink-Mod-Manager](https://github.com/Zetas-Workshop/Relink-Mod-Manager)),
but Reloaded-II + gbfrelink.utility.manager is the mainline path the docs assume.

## Anatomy of a mod

Per [Creating Mods](https://nenkai.github.io/relink-modding/modding/creating_mods/):

- A mod is a Reloaded-II mod project whose assets sit under **`(Mod Directory)\GBFR\data\...`**,
  mirroring the extracted path exactly. Example: to replace
  `model\pl\pl0101\pl0101.minfo`, ship
  `(Mod Directory)\GBFR\data\model\pl\pl0101\pl0101.minfo`.
- The mod declares the **GBFR Mod Manager as a Reloaded-II dependency**.
- Authoring conveniences: keep `.json` (for `.msg`) and `.xml` (for `.bxm`) in the mod;
  the loader converts them at load time. Since loader 1.3.0 files can even be edited
  live from mod folders.

## Publishing and etiquette

- Mods are published on [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink)
  (GameBanana also hosts some).
- The docs ask authors to state: **game version tested against**, **required loader
  version**, whether the mod **edits tables** or **UI textures** (both flagged as likely to
  break on game updates — prophetic, given 2.0), and to ship `fhd` + non-fhd variants for UI mods.
- **Table mods**: document exactly which tables you edited so users can spot conflicts;
  ship only the tables you changed.
- **Hard etiquette line**: no cheat-adjacent use online. The game has telemetry
  ("PlayLog") that the [install guide](https://nenkai.github.io/relink-modding/modding/installing_mods/)
  recommends opting out of (`Game Options → Other → Play Log → Do Not Agree`) when modding.
- Community hub: the Relink Modding **Discord** (linked from the docs site) — where
  file-list updates, table research, and post-patch triage coordinate.

## Who's who

- **Nenkai** — wrote GBFRDataTools and the documentation site; reverse-engineered the
  archive format. Declared a research pause during the ER closed beta, resuming
  post-release ([README](https://github.com/Nenkai/GBFRDataTools)).
- **WistfulHopes** — maintains the mod loader; shipped ER support day-one (v2.0.1).
- **Lyall** — maintains [GBFRelinkFix](https://codeberg.org/Lyall/GBFRelinkFix), the
  ultrawide/FOV/HUD ASI fix (and many similar fixes for other games).
