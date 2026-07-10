# Handoff: complete open-source UI spanning (start here next session)

> **UPDATE 2026-07-09 (late):** steps 1 and 2 are implemented and deployed, pending one
> in-game verification run — see [docs/18](18-ui-spanning-session2.md) for the findings
> (the "UI Size Setter" discovery) and the test checklist.

Goal: finish an **open-source, auditable** 2.0 ultrawide mod that also spans the remaining
UI — **menu backgrounds, combat screen-effects, speech-bubbles/nameplates, lock-on/dodge** —
by reverse-engineering the *game* ourselves (+ porting master's MIT code). **Do NOT decompile
RetroGawd's binary to lift his code** — we use his symbol list only as a *map of which
features/sites exist*, and derive our own implementations. (Why: [docs/16](16-retrogawd-comparison.md).)

## Tools ready (use all of these)
- **Disassembler** — `tools/disasm/disasm.exe "<game>\granblue_fantasy_relink.exe" <rva_hex> <len>`
  (source `tools-src/disasm/`, zydis+MSVC). Reads game code precisely. Image base `0x140000000`,
  so runtime addr = `0x140000000 + rva`. This is the key unlock — read, don't guess.
- **In-mod diagnostic method** — the game has anti-debug (x64dbg crashes on attach), so add a
  one-shot `MAKE_MIDHOOK` that `LOG_INFO`s registers/struct to `GBFRelinkFix.log`. Pattern:
  build → verify the string is in the `.asi` (`grep -c "LABEL" ...asi`) → deploy → capture.
  **Gotcha: two hooks can't share one pattern site** — if Span HUD is on, a diagnostic on the
  same "UI Constraints" pattern fails to scan. Set `[Span HUD] Enabled=false` when probing it.
  **Gotcha: always confirm `build ok` AND the label string is in the binary before deploying**
  (a compile error silently leaves the previous `.asi`; brace-commas inside `MAKE_MIDHOOK(...)`
  break the preprocessor — keep arrays at file scope).
- **Master's MIT source** — `git -C vendor/GBFRelinkFix show origin/master:src/dllmain.cpp`.
  Has the exact algorithms: `SpanAllBackgrounds` (bg IDs + span-to-`2160*aspect`), the
  `HUDFix` UI-Constraints handling incl. **Gameplay HUD / Guard&Lock-On / Dodge** offset logic.
- **RetroGawd feature MAP** (from his binary symbols — a to-do list, not code):
  `AspectFOVFix`, `HUDFix` (`UIAspect`, `HUDConstraints`, `SpanAllHUD`), `UIBackgroundsWidth` +
  `UIBackgroundsHeight`, `GraphicalFixes→ScreenEffects1`, `Nameplate Fix`, `MarkerProbe`/`MarkerScale`.

## Confirmed 2.0 facts (don't re-derive)
- **UI element struct** (shared by UI-Constraints and UI-Backgrounds sites), via `rax`:
  - `+0x194/0x198` = anchor factors (0.5)
  - `+0x1B4/0x1B8` = reference W/H (3840/2160)
  - `+0x1BC/0x1C0` = **constraint W/H (3840/2160)** ← the width/height fields
  - `+0x1C4` = **object-ID** (master's `0x1FC`; shift −0x38). IDs are **unchanged from master**.
  - `+0x1CC/0x1D0` = position offset (master used these for Lock-On/Dodge; `0x1CC` did NOT shift)
- **Known IDs** (verified present on 2.0 at `+0x1C4`): FadeBlack `0x7328174D`, TitleFadeBlack
  `0xECAD1DC1`, LoadSaveBg `0xEC983A58`, PauseBg `0x600A6C3E`, TitleFadeWhite `0x62230830`.
  UI-Constraints IDs (from master, unverified on 2.0): GameplayHUD `1719602056`, GuardLockOn
  `605904162`, Dodge `3550204025`. UNKNOWN full-width IDs seen (menu-bg/combat candidates):
  `0xC3D4410C 0x754D6673 0x55E347B5 0x7BF93DEE 0xE4555EFC`.
- **UI-Backgrounds function** at `exe+0x3340ad5` (hook master used = +0x2F = `exe+0x3340b04`).
  Disassembled + verified: `W=[rax+0x1BC]`; extents `= ±0.5*W` (const `0x80000000` sign-flip
  for left, `1.0` for right); result rect → `[rsp+0x40]` → render call. Overriding `W` (or
  `xmm1` at the hook) to `2160*aspect` is **provably correct for this function** — but the
  visible background did **not** move. ⇒ **this function's rect is not the visible bg.**

## The core blocker + how to crack it (ordered plan)
The visible backgrounds/effects/nameplates live in **multiple distinct functions** (RetroGawd
has 4+ separate hooks). Next session, do the tractable ones first:

1. **Lock-On + Dodge (most tractable — start here).** Same UI-Constraints site as our Span HUD.
   With `[Span HUD]=false`, add a diagnostic that logs elements whose `+0x1C4` id ∈
   {`1719602056`,`605904162`,`3550204025`} and dumps `+0x1CC/0x1D0`. Then port master's exact
   offset formula (`rax+0x1CC = -((2160*aspect-3840)/2)` for lock-on, `+…` for dodge), with 2.0
   offsets. This is a near-direct port — likely a quick win.
2. **Find the REAL background function.** The one at `0x3340ad5` is a decoy. Approaches:
   (a) broaden the UI-Backgrounds diagnostic to log EVERY function that stores a full-screen
   rect (hook more sites / scan for other `[rax+0x1BC]` readers via the disassembler);
   (b) use `disasm.exe` to find other functions referencing the aspect/resolution globals;
   (c) find the menu-bg *texture* draw path and trace back. Master's `UIBackgroundsHeightMidHook`
   (hook at scanResult **+0x28**) hints there's a second hook point in the same function — try
   hooking the height path too, and try modifying **both** W and H.
3. **Screen effects (combat seams).** Separate site (`ScreenEffects1`). Diagnostic-hunt the
   full-screen post/overlay quad that seams on the big-attack capture; likely same
   "override width to 2160*aspect" once the site is found.
4. **Nameplates / speech bubbles.** RetroGawd forces world-projected UI to the HUD aspect;
   find the world→screen projection for nameplates and override its aspect. Related to our
   `HUD: Markers` work (world-projected).

## Deliverable shape
- New config toggles: `[Span Backgrounds]`, `[Fix Lock-On]`/`[Fix Dodge]` (or fold into Fix HUD),
  `[Fix Screen Effects]`, `[Fix Nameplates]` — all default off (experimental), documented.
- Keep it in `vendor/GBFRelinkFix` on branch `ragnarok-2.0-fixes`; export to
  `docs/gbfrelinkfix-2.0-fixes.patch`; cut a new release when a feature verifies in-game.
- Verify each with a screenshot before claiming it works (the register-write can fire yet not
  move the pixels — always confirm visually).

## Current deployed state (Alex's game)
Clean release build (crash fix + res + FOV + HUD + markers), `[Span HUD]=false` (native HUD),
no experiments. `save-backups/STAGED-MOD-FILES.txt` lists the game-folder files.
