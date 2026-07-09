# Code Mods: Two Architectures

*Sources: [Mod Manager API](https://nenkai.github.io/relink-modding/modding/mod_manager_api/),
[Creating Mods](https://nenkai.github.io/relink-modding/modding/creating_mods/),
[GBFRelinkFix](https://codeberg.org/Lyall/GBFRelinkFix). As of 2026-07-09.*

Relink code mods come in two very different shapes. Both matter for us — the C# path is
the safer way to build original QoL mods; the C++/ASI path is what GBFRelinkFix uses and
what we'd need to help update it.

## At a glance

| | Reloaded-II C# mod | ASI plugin (C++) |
|---|---|---|
| Runs via | Reloaded-II mod framework | Ultimate ASI Loader (`winmm.dll` etc. in game folder) |
| Typical powers | Programmatic *file* edits via the loader's API; managed hooks | Direct engine hooks: FOV, camera, HUD scale, FPS cap, LOD |
| Hook discovery | Mostly not needed (file-level) | **Byte-pattern scanning** (safetyhook) against the exe |
| Update resilience | **High** — API abstracts the archive; explicitly recommended by the docs as less likely to break across game updates | **Low** — any recompile of the exe invalidates patterns (exactly what 2.0 did) |
| Language/stack | C#/.NET, Reloaded-II mod template | C++, CMake/VS, safetyhook, inipp-style config |
| Examples | [gbfr.qol.weaponglowcontrol](https://github.com/Nenkai/gbfr.qol.weaponglowcontrol), Discord Rich Presence | [GBFRelinkFix](https://codeberg.org/Lyall/GBFRelinkFix) (ultrawide) |

## Path A: Reloaded-II C# mods (the Mod Manager API)

Setup per the [API docs](https://nenkai.github.io/relink-modding/modding/mod_manager_api/):

1. Set up a Reloaded-II mod dev environment (their
   [Creating Mods tutorial](https://reloaded-project.github.io/Reloaded-II/)) and create a
   project from the **Reloaded II Mod template**.
2. Add the **`gbfrelink.utility.manager.Interfaces`** NuGet package.
3. Declare the GBFR Mod Manager as a dependency.
4. Get the API in your mod's constructor:
   ```csharp
   _modLoader.GetController<IDataManager>()?.TryGetTarget(out IDataManager dataManager!);
   ```

`IDataManager` operations:
- `FileExists(path, includeExternal)` — check a game file exists
- `GetArchiveFile(path)` — read file data out of the archives
- `AddOrUpdateExternalFile(path, data)` — inject a new/modified file
- `UpdateIndex()` — commit changes to the game's index

So a C# mod can **fetch a game file at runtime, patch bytes/fields, and hand it back** —
e.g. weaponglowcontrol edits effect files programmatically instead of shipping static
copies. That's why these survive updates better: they re-derive their output from
whatever the current game files are.

Since loader **v2.0.1** there's also **`IUserDefinedParams`** for querying game
version/type ([release notes](https://github.com/WistfulHopes/gbfrelink.utility.manager/releases))
— use it to branch behavior between 1.3 and 2.0 (the loader supports both).

Known API limitation: files added externally or altered by *other* mods can't be read
back yet.

## Path B: ASI plugins (GBFRelinkFix-style)

[GBFRelinkFix](https://codeberg.org/Lyall/GBFRelinkFix) is ~86% C++:

- **Injection**: [Ultimate ASI Loader](https://github.com/ThirteenAG/Ultimate-ASI-Loader)
  — a proxy DLL (`winmm.dll`/`dinput8.dll`/`version.dll`) in the game folder loads
  `.asi` plugins from `scripts/`. Config in `scripts\GBFRelinkFix.ini`.
- **Hooking**: [safetyhook](https://github.com/cursey/safetyhook). The mod finds each
  target function by scanning the exe for a **byte pattern** (a signature of the compiled
  code around the interesting instruction), then installs an inline hook or patches values.
- **What that unlocks** (from its README): ultrawide/custom resolutions, FOV & aspect
  scaling, gameplay camera changes, HUD scaled to 16:9, shadow/LOD/TAA tweaks,
  experimental 240 FPS cap — things no file edit can do.

### Why 2.0 killed it, and what "fixing" means

The 2.0 exe was **compiled with a different compiler**
([ER page](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/)),
so instruction sequences changed globally — patterns don't just miss, they can partially
match the wrong site. GBFRelinkFix v1.1.1 crashes the game at boot even with all features
disabled ([issue #1](https://codeberg.org/Lyall/GBFRelinkFix/issues/1)).

Re-porting an ASI mod is a reverse-engineering loop:

1. Open the new exe in **Ghidra/IDA** (static) and/or **x64dbg** (live).
2. For each old pattern: find the equivalent code in the new build — search for anchor
   constants (float literals like aspect ratios, strings, imported calls), or diff against
   the old exe with BinDiff/Diaphora-style tooling.
3. Write a new, hopefully more compiler-agnostic signature (shorter, wildcarded operands).
4. Re-test each feature independently (the .ini toggles help isolate).

Skills: x64 assembly reading, pattern craft, patience. High effort, high leverage —
every ultrawide user needs this mod.

## Choosing

- Original QoL idea that reduces to *data* the game reads → **C# + IDataManager**
  (resilient, shippable, matches an app-dev background).
- Behavior that only exists in engine code (camera, rendering, framerate) → **ASI/C++**,
  accepting you're signing up to re-port after big patches.
