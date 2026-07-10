# Research: Spanning the main-menu background to ultrawide (2026-07-09)

Goal (Alex's idea): on ultrawide, extend the main menu's sky/cloud background to fill the
sides instead of sitting in a 16:9 band. Bonus idea: reuse the (already-spanning) loading
screen art. This is the menu/background piece we'd
[backlogged](../BACKLOG.md) — here's whether it's actually doable on 2.0.

## Verdict: feasible, and mostly a port job

Lyall's old `master` branch already solved this on 1.x with a `SpanAllBackgrounds` feature,
and it **named the exact element we want**:

```
Main menu bg = 2384707215   // plus: Title menu bg, Load save bg, Lyria's journal,
                            // Pause screen bg, Dialogue bg, fades, Title options bg 1/2 …
```

So the menu background is a real, addressable **UI element authored at 3840×2160** — the
same kind of object our Span HUD already widens, just processed at a separate **"UI
Backgrounds"** hook site. Master's mechanism: at that site, for an element whose width is
3840 and whose object-ID is a known background, set its width to `2160 × aspect` (the
correct, undistorted ultrawide width). Backgrounds not in the list are left alone — so
this is *targeted and safe*, unlike the broad Span HUD.

### What survived to 2.0
- **The hook site still matches uniquely** on the 2.0 exe (master's UI-Backgrounds pattern
  → `granblue_fantasy_relink.exe+0x3340b04`, hook at pattern+0x2F). The function wasn't
  removed by the recompile.
- Static decode shows it reads the same struct family we mapped for Span HUD
  (`rax+0x194/0x198/0x1BC/0x1C0`).

### What changed / is unknown (needs one runtime capture)
The struct was reorganized between 1.x and 2.0 (we already saw dims move `0x1F4→0x1B4`,
while other fields like the offset at `0x1CC` did *not* move). So master's static offsets
don't transfer directly. Two things can only be read at runtime, **with the main menu
open** (that's when the menu-bg element passes through the hook):
1. **The 2.0 object-ID offset** (master's `0x1FC`) and the **width field / register** to modify.
2. **Whether the main-menu-bg ID is still `2384707215`** — these IDs are hashes stored in
   the UI data files, not literals in the exe (confirmed: none appear as raw dwords), so
   they can't be verified statically.

## The plan
1. **Diagnostic** (built, staged): a hook at `+0x3340b04` that, for each element passing
   through with a 3840-ish width, scans the struct for any of master's 12 known background
   IDs and logs the offset + width/height it found them at. Run it **with the main menu
   open** → confirms the ID and pins the 2.0 offsets in one capture.
2. **Port the feature**: re-add master's targeted background span with the 2.0 offsets +
   confirmed IDs, gated to menu backgrounds only. Wire to a `[Span Menu Background]`
   config toggle.
3. **Test for stretch vs extend**: if the menu bg is a fixed-aspect image, widening it may
   *stretch* the clouds rather than *extend* them. If so, fall back to the tiling/anchor
   approach or Alex's loading-asset idea (`ui/atlas/loading_loading01/02` — the loading
   art that already spans).

## Combat is affected too (same root cause)

A 21:9 combat capture (2026-07-09) showed **intermittent vertical seams during full-screen
overlay effects** — a white screen-flash rendered in a centered 16:9 band with dark side
strips, and the red overdrive/"Cardinal" tint bright in the center but dimmed past a hard
vertical seam on the sides. Most of the fight is fine (true post-process effects like bloom
already span); only **full-screen overlay quads authored at 3840×2160** (flashes, screen
tints, fades) seam — the same element class as the menu backgrounds and master's
`Fade to black`/overlay IDs.

So the UI-Backgrounds span isn't menu-only — it should also remove these combat seams. **When
running the diagnostic, also trigger a big attack / overdrive** so the flash/tint element gets
logged alongside the menu backgrounds. This raises the value of the fix: one hook addresses
menu backgrounds *and* combat overlay seams.

## Caveats
- Not just menu — also fixes combat full-screen-overlay seams (above). Still won't touch true
  post-process effects (those already span).
- Still an ASI code mod → breaks on future game patches like everything else.
- The "reuse loading asset" idea is a real fallback but heavier (asset swap in the menu
  layout) — try the element-span first; it's cleaner if the art extends without stretching.
