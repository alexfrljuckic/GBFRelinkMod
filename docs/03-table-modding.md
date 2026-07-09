# Table Modding (Gameplay Mods)

*Sources: [Table Database](https://nenkai.github.io/relink-modding/tables/table_database/),
[Table List](https://nenkai.github.io/relink-modding/tables/table_list/),
[Creating Mods](https://nenkai.github.io/relink-modding/modding/creating_mods/). As of 2026-07-09.*

## What tables are

The `.tbl` files under **`system/table`** hold most of the game's live data: items,
abilities, gems/sigils, drop rates, quest parameters, and more. Editing them is how
gameplay/balance/QoL-data mods work — no code required.

Tables are binary; column layouts are described by `.headers` files shipped inside
GBFRDataTools. **Many columns are still undocumented** — community research fills them in
over time (another contribution surface). Hash values link rows across tables; a blank
hash column is `887AE0B0`.

## The workflow

```text
extract .tbl  →  convert to SQLite  →  edit in SQLiteStudio  →  convert back  →  ship as mod
```

1. **Extract** the table(s) from the game (see [06-toolchain-setup.md](06-toolchain-setup.md)
   for extraction):
   ```
   GBFRDataTools.exe extract-all -i "<game>\data.i" -f "system/table"
   ```
2. **Convert to SQLite** (GBFRDataTools ≥ 1.2.0; note the version flag):
   ```
   GBFRDataTools.exe tbl-to-sqlite -i <table folder> -o tables.sqlite -v <game version>
   ```
3. **Edit** in [SQLiteStudio](https://sqlitestudio.pl/). Tip from the docs: it supports a
   custom `find()` function to search a value across *all* tables — essential for research
   ("where does this item ID appear?").
4. **Convert back**:
   ```
   GBFRDataTools.exe sqlite-to-tbl -i tables.sqlite -o <output folder> -v <game version>
   ```
5. **Package**: put only the `.tbl` files you actually changed at
   `(Mod Directory)\GBFR\data\system\table\<name>.tbl` in a Reloaded-II mod that depends
   on the GBFR Mod Manager.

## The version flag is the whole ballgame

`-v <version>` selects the **schema for that game build**. This is exactly what Endless
Ragnarok broke: 2.0 added/rearranged columns ("column shifts"), so:

- A `.tbl` produced against the 1.x schema is **misaligned garbage** to a 2.0 game.
- Porting a mod = re-extract the **2.0 vanilla tables**, re-apply your row/value edits in
  the new schema, re-convert with the 2.0 version flag.
- ⚠️ **Gate (2026-07-09):** GBFRDataTools doesn't fully support ER yet (file list +
  presumably new schema versions pending). Check
  [releases](https://github.com/Nenkai/GBFRDataTools/releases) for a post-2.0 version
  before starting a port.

## Practical porting recipe (1.x mod → 2.0)

1. Get the mod's edited `.tbl` files and, if possible, the *vanilla 1.x* versions of the same tables.
2. Convert both to SQLite with `-v <1.x>`; **diff them** (SQL or a diff script) to recover
   the *intent* of the mod as a list of `(table, row, column, old→new)` edits.
3. Extract the vanilla **2.0** tables, convert with `-v <2.0>`.
4. Re-apply the edit list onto the 2.0 database — by column *name*, never by position.
5. Convert back with `-v <2.0>`, test in-game, publish with version metadata.

Step 2's diff-to-edit-list could be scripted once and reused for every mod port — a
genuinely useful community tool if none exists yet.

## Warnings

- **Ship only edited tables** and document which ones — table mods conflict wholesale
  (last-loaded wins per file), so users need to know what overlaps.
- **Never use table mods online** in ways that affect other players; keep the PlayLog
  telemetry opt-out in mind ([install guide](https://nenkai.github.io/relink-modding/modding/installing_mods/)).
- **Item caps**: over-capped modded items recap to 999 on version updates — a save-data
  hazard for users; warn in mod descriptions
  ([ER page](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/)).
- Always keep vanilla backups of any table you touch, per version.
