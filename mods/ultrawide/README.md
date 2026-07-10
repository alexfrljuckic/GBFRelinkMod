# Ultrawide Fix (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on future
> updates and I won't patch it every time. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Ultrawide + custom-resolution, FOV, HUD, and framerate fixes for **Granblue Fantasy:
Relink – Endless Ragnarok (game v2.0)**. This is a **2.0-updated build of
[Lyall's GBFRelinkFix](https://codeberg.org/Lyall/GBFRelinkFix)** (MIT, © Lyall 2024) with
our fixes for the expansion: the boot crash, world markers, and a configurable **Span HUD**.
What we changed and how is in [docs/08](../../docs/08-gbfrelinkfix-status.md) and
[docs/09](../../docs/09-hud-markers-debugging.md).

## What it does
- True ultrawide (21:9 / 32:9) and custom/desktop resolution
- Fixes cropped FOV, cutscene FOV, HUD
- **Span HUD** (optional): pushes the gameplay HUD out to the screen edges instead of a
  centered 16:9 band — toggle in the `.ini`
- Shadow-resolution / LOD / TAA / framerate-unlock options

## How it loads (and Reloaded-II compatibility)
This is an **ASI plugin** (`.asi`) — a different mechanism from a normal Reloaded-II mod. It
loads through an **ASI loader** (a proxy `winmm.dll` in the game folder). It runs
**alongside** your Reloaded-II mods with no conflict — the ASI loader and Reloaded-II are
independent. (Advanced: you can instead have Reloaded-II load it via *Edit Application →
Advanced Tools & Options → Deploy ASI Loader*, but the standalone `winmm.dll` method below
is simplest and is what we tested.)

## Install (step by step)

You need two downloads plus the config in this folder.

1. **Ultimate ASI Loader** (the `winmm.dll` loader):
   download `Ultimate-ASI-Loader_x64.zip` from
   [ThirteenAG releases](https://github.com/ThirteenAG/Ultimate-ASI-Loader/releases),
   extract it, and **rename `dinput8.dll` → `winmm.dll`**.
2. **The mod** `GBFRelinkFix.asi`: download from this repo's
   [Releases](https://github.com/alexfrljuckic/GBFRelinkMod/releases) (attached to the
   `ultrawide-vX` tag).
3. Open your game folder:
   `…\steamapps\common\Granblue Fantasy Relink\` (right-click the game in Steam →
   *Manage → Browse local files*).
4. Put **`winmm.dll`** in that folder (next to `granblue_fantasy_relink.exe`).
5. Make a **`scripts`** folder there, and put **`GBFRelinkFix.asi`** and
   **`GBFRelinkFix.ini`** (from this folder) inside `scripts\`.
6. Launch the game normally from Steam. Ultrawide should apply. In-game
   *Graphics → Resolution*, pick your ultrawide resolution (it replaces the old
   3840×2160 entry).

Your game folder should look like:
```
Granblue Fantasy Relink\
  granblue_fantasy_relink.exe
  winmm.dll                     ← Ultimate ASI Loader (renamed)
  scripts\
    GBFRelinkFix.asi            ← the mod (from Releases)
    GBFRelinkFix.ini            ← config (this folder)
```

## Configure
Edit `scripts\GBFRelinkFix.ini`. Highlights:
- `[Custom Resolution] Enabled = true`, `Width/Height = 0` → use your desktop resolution.
- `[Span HUD] Enabled` → `true` = HUD spans to the edges; `false` = centered 16:9 (default-safe).
- `[Gameplay Camera] FOV` → `1.25` = 25% wider, etc.

## Uninstall
Delete `winmm.dll` and the `scripts\` folder from the game folder. (Before any game update,
do this too — ASI mods break on patches until rebuilt.)

## Known limitation
Menus render 16:9-centered (backgrounds don't span). That's deliberate — full menu spanning
is on the [backlog](../../BACKLOG.md).

## Build from source (optional)
The 2.0 fixes are in the fork at [docs/gbfrelinkfix-2.0-fixes.patch](../../docs/gbfrelinkfix-2.0-fixes.patch)
(apply onto Lyall's `ragnarok` branch). Build with xmake + MSVC — see
[docs/06](../../docs/06-toolchain-setup.md).
