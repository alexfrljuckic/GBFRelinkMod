# First Projects (Ranked)

*As of 2026-07-09. The ranking optimizes for: real demand right now, learning value,
and match to a software-dev background.*

## 1. Anchor: help update GBFRelinkFix for 2.0 🎯

**Repo**: https://codeberg.org/Lyall/GBFRelinkFix ·
**Issue**: [#1 — crashes after the 7/8 update](https://codeberg.org/Lyall/GBFRelinkFix/issues/1)

- v1.1.1 crashes 2.0 at boot (black screen → back to Steam library) even with all
  features disabled; only workaround is removing the mod. 9+ users confirming; **no
  maintainer response in-thread yet, but a `ragnarok` branch exists** in the repo — Lyall
  is aware and a commenter notes the changes look substantial.
- The work: re-find byte patterns in the recompiled 2.0 exe (see
  [04-code-mods.md](04-code-mods.md), Path B). Contributions could be: testing the
  `ragnarok` branch, bisecting which hook crashes (the .ini toggles + a debugger),
  or re-finding specific signatures and PRing them.
- **First step**: build the repo as-is (CMake/VS), read how patterns are declared, watch
  the `ragnarok` branch. Even *reproducing + diagnosing* the crash precisely (which scan
  mismatches) is a valuable issue comment.
- Skills gained: safetyhook, pattern scanning, x64dbg/Ghidra basics — the core toolkit of
  the entire fix-mod genre.

## 2. Port a broken table mod to 2.0 🥇 (fastest first win)

- Pick a small, popular, broken gameplay mod on
  [Nexus](https://www.nexusmods.com/games/granbluefantasyrelink) (drop-rate or
  ability-tweak scale; with the author's blessing or as an update PR/patch).
- Follow the porting recipe in [03-table-modding.md](03-table-modding.md): diff the mod
  against vanilla 1.x in SQLite, re-apply edits onto 2.0 tables by column name.
- **Gated on**: GBFRDataTools shipping ER support (file list / 2.0 table schemas) —
  watch [releases](https://github.com/Nenkai/GBFRDataTools/releases). Until then, do the
  1.x-side diff work; it carries over.
- Bonus idea: script the "diff two table SQLites into an edit list, replay onto a new
  schema" step generically. Every table-mod author needs this *right now*; it may not
  exist yet.

## 3. An original Reloaded-II C# QoL mod 🛠️ (safest architecture)

- Build something small end-to-end with the
  [Mod Manager API](https://nenkai.github.io/relink-modding/modding/mod_manager_api/):
  template → `Interfaces` NuGet → `IDataManager` fetch/edit/return → publish on Nexus.
- Study [gbfr.qol.weaponglowcontrol](https://github.com/Nenkai/gbfr.qol.weaponglowcontrol)
  as the reference implementation.
- Use `IUserDefinedParams` (loader ≥2.0.1) to be dual-version (1.3/2.0) from day one —
  few mods will bother; it's a differentiator.
- Idea sourcing: read Nexus comment sections of broken QoL mods for "please update"
  demand signals.

## 4. Watch items / community contributions 👀

- **GBFRDataTools ER file list** — the ecosystem's current bottleneck ("pending
  contribution" per the [ER page](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/)).
  When Nenkai releases the updated File Name Logger, running it while playing ER content
  and contributing captured paths is high-value, low-skill-floor work. (We opted for
  "just making mods," but this one unblocks *our own* table work too.)
- **Table column research** — new 2.0 columns (Master Traits, Weapon Transcendence) will
  be undocumented; documenting them helps every gameplay modder.
- Join the **Relink Modding Discord** (linked from
  [the docs site](https://nenkai.github.io/relink-modding/)) before starting any of the
  above — post-patch triage is coordinated there, and someone may already be mid-way on
  any given port.

## Suggested sequence

1. Toolchain setup session ([06-toolchain-setup.md](06-toolchain-setup.md)) + join Discord.
2. Start #1 (build GBFRelinkFix, reproduce the crash) — not gated on anything.
3. When GBFRDataTools ER support lands → do #2 for the quick shipped win.
4. #3 whenever a good idea crystallizes.
