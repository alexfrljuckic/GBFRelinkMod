# CLAUDE.md

GBF Relink (2.0 / Endless Ragnarok) modding workspace. Orientation: [README.md](README.md)
→ [BACKLOG.md](BACKLOG.md) (single work tracker) → `docs/` (numbered research logs —
read the relevant ones before redoing any reverse-engineering; docs/11/15/16 cover the
table formats and decoded chains). Toolchain setup: docs/06. Multiple Claude sessions
may work this repo concurrently — `git fetch` before assuming state, and beware
`git add -A` sweeping another session's in-progress files.

## Releases — ALWAYS ship comparative release notes

Every GitHub release MUST tell users **what changed since the previous release**, not
just what the mod is. Checklist:

1. **Lead the notes with a `## Changes since <previous-tag>` section** — a bullet list
   of user-visible deltas (new features, odds changes, pool contents, fixed bugs,
   which `.tbl` files changed). Derive it from `git log <prev-tag>..HEAD -- mods/<mod>/`
   plus the session's own work; write it for a player, not a developer. First release
   of a mod: use `## Initial release` instead.
2. **Versioning**: any behavior change = a **new tag** (`<mod>-vX.Y`) with a fresh zip.
   Re-uploading an asset onto an existing tag (`gh release upload --clobber`) is for
   same-day hotfixes only, and REQUIRES appending a dated changelog line to the release
   notes in the same breath (`gh release edit <tag> --notes-file -`), e.g.
   `> Updated 2026-07-10: <what changed in the zip>`. Never silently swap an asset.
   **Docs/text-only changes need NO release action at all** (per Alex 2026-07-10):
   README, INSTALL.txt wording, ModConfig descriptions etc. just land in the repo and
   ride along with the next behavior release — don't re-upload zips for them.
3. **Keep versions in sync**: `ModVersion` in the mod's ModConfig.json == the tag
   version; the zip contains the mod folder + an up-to-date `INSTALL.txt`.
4. **Point the trail at it**: update the mod's README download link if the tag name
   changed, and the BACKLOG "Done" entry with the release link.

Rationale: we once clobbered a release zip with a fix and the release page said
nothing — a downloader had no way to know which behavior they had.

## Other conventions that bite

- `.tbl` edits: always round-trip vanilla byte-identical first, then byte-diff the edit
  (docs/11 workflow). Never hand-edit binaries.
- Data mods are staged by the GBFR Mod Manager **at game launch** — testing any change
  requires a game restart; the manager's `temp/` + `cached_files.txt` timestamps and a
  fresh log in `%APPDATA%\Reloaded-Mod-Loader-II\Logs` prove a real apply.
- PS5.1 Get/Set-Content mangles UTF-8 — use dedicated file tools. Multi-line SQL through
  `sqlite3.exe` on cmd must be flattened to one line.
