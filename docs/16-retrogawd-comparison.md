# RetroGawd's ultrawide fix vs ours (2026-07-10)

Someone else is building the same thing, further along. Honest comparison so we don't
reinvent what's already shipped.

## What it is
[RetroGawd/granblue-fantasy-relink-ragnarok-ultrawide](https://github.com/RetroGawd/granblue-fantasy-relink-ragnarok-ultrawide)
— a 2.0 ultrawide fix, also based on Lyall's GBFRelinkFix. **Release-only** (README +
binaries, no source). 4 releases in 2 days; **v1.3 shipped 2026-07-10**. Their `.asi` is a
**GCC/MinGW build with symbols retained**, so the feature set is fully readable from the
binary (that's how the table below was recovered — not from source).

## Their feature set (from binary symbols + ini)
| Area | Their hooks / options | Our status |
|---|---|---|
| Resolution | `ApplyResolution`, `PatchResolutionTable`, `RenderResolution`, `Aspect Writer` (authoritative) | Custom Resolution ✓ (simpler) |
| FOV / camera | `AspectFOVFix` — gameplay/cutscene/roam/follow cam + FOV multiplier (many hooks) | FOV + cutscene ✓ (fewer) |
| HUD | `HUDFix` — `UIAspect`, `HUDConstraints`, `SpanAllHUD` | Span HUD ✓ |
| **Menu backgrounds** | **`UIBackgroundsWidth/HeightMidHook` + BackgroundWidth/HeightIDs** | ❌ we couldn't land it |
| **Combat screen effects** | **`GraphicalFixes → ScreenEffects1MidHook`** (`ScreenEffects` ini) | ❌ our target |
| **Speech bubbles / nameplates** | **`Nameplate Fix`** ("forcing HUD-projection aspect… else 16:9-projected") | ❌ our target |
| World markers | `MarkerProbe` (0–5), `MarkerScale` (multi-site), `UIMarkers` | HUD: Markers ✓ (basic) |
| Graphics | `Graphics Corruption 1/2`, Shadow, TAA, LOD, FPS cap | Shadow/TAA/LOD/FPS ✓ |

**They already solved the three things we were mid-reverse-engineering** — menu
backgrounds, combat screen-effect seams, and speech-bubble/nameplate projection.

## Verdict
For the **ultrawide experience**, RetroGawd's build is further along and actively
maintained — the pragmatic move is to just use it (or contribute), not to keep burning
capture/build cycles re-deriving `ScreenEffects` / `UIBackgroundsHeight` / `Nameplate`
ourselves. Our reverse-engineering (Press-Any-Key crash fix, HUD Markers, Span HUD, the
in-mod-diagnostic method) was solid learning and produced a working mod, but we're now the
*second* implementation of the same idea.

**Where we're still unique / higher-value:** the **gameplay-data side** — the reversed
2.0 `reward_lot`/`gacha_lot`/`reward` table headers, the drop-rate/Transmarvel/curio knob
map, and the RNG-tuner design. RetroGawd's mod doesn't touch any of that. That's the lane
worth continuing in.

## If we ever want their specific fixes
Their binary is symbol-rich; we could reverse `ScreenEffects1MidHook` / `Nameplate Fix` /
`UIBackgroundsHeightMidHook` from it and port to our source. But that's duplicating a
maintained mod — only worth it as a learning exercise, not for the end result.
