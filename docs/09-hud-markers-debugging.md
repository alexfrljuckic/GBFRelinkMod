# HUD: Markers — live debugging plan (2026-07-09)

Goal: find where the 2.0 build stores the HUD reference width/height (so ultrawide HUD
elements group into a 16:9 zone instead of pinning to the 21:9 edges), and recover the
correct register + struct offsets to rewrite the dead `HUD: Markers` hook.

## What we know (static)

- Original hook (demo build): mid-hook at a `vmulss`, body reads `ctx.xmm2`, writes
  `ctx.r14+0x1B4` (width ref) and `ctx.r14+0x1B8` (height ref).
- 2.0 build: the demo pattern misses. The only surviving lookalike is at
  **`granblue_fantasy_relink.exe+0x2657b9d`** (RVA; unique match):
  ```
  vmulss  xmm1, xmm1, [0.5]        ; C5 F0 59 0D ....
  vaddsubps xmm0, xmm0, xmm1        ; C5 FB D0 C1
  movzx   eax, byte [rsi+0x348]     ; 0F B6 86 48 03 00 00
  shl     rax, 3 ; or rax,0x54 ; vmovsd xmm1,[rbx+rax+0xcc0] ; vaddps xmm0,xmm0,xmm1
  ```
  Registers here are `xmm0/xmm1/rsi/rbx` — NOT the `xmm2/r14` the old body used. So this
  may be the right code with new regs, or the wrong function entirely. **The debugger
  decides.**

## Method (x64dbg, game already running with the mod)

x64dbg (x64): `C:\dev\GBF\tools\x64dbg\release\x64\x64dbg.exe`

Phase 1 — test the candidate site:
1. File → Attach → `granblue_fantasy_relink.exe`.
2. Ctrl+G → `granblue_fantasy_relink.exe+2657b9d` → Enter (lands on the `vmulss`).
3. F2 (breakpoint) → F9 (resume). Move around so the HUD renders.
4. When it breaks, screenshot the **Registers** panel + right-click `rsi` → Follow in Dump,
   and `rbx` → Follow in Dump (second dump). Report back.

We're looking for HUD reference floats near `rsi` (e.g. `3840.0`/`2160.0` or
`1920.0`/`1080.0`, hex `0x45700000`/`0x45070000` etc.) at some offset — especially
around `+0x1B4/+0x1B8`.

Phase 2 (if the candidate is wrong) — find the struct by value, then breakpoint access:
- Right-click dump → Search → Float, for `2160.0` or `3840.0`; find one whose neighbour is
  the paired dimension; hardware-breakpoint-on-write it; see which code touches it.

Outcome feeds the patch: new pattern (already unique) + corrected base register + offsets.

## RESULT — SOLVED without a debugger (2026-07-09)

x64dbg **crashed the game on attach** (anti-debug). Pivoted to an **in-mod diagnostic
hook** instead: added a one-shot `MAKE_MIDHOOK` at the candidate site that logged the
register context + scanned the struct, then read `GBFRelinkFix.log`. Output:

```
DIAG: AspectRatio=2.3888888
DIAG: xmm1=[3840, 2160, 0, 0]     <- the HUD reference resolution
```

Static decode of the instruction immediately preceding the site nailed the source:
`vmovsd xmm1, [r14+0x1BC]` (`c4 c1 7b 10 8e bc 01 00 00`). So:

- **Base register is still `r14`** (the `rsi` in the movzx was a per-element index — red herring).
- **Reference width @ `r14+0x1BC` (=3840), height @ `r14+0x1C0` (=2160)** — the demo's
  `0x1B4/0x1B8` shifted **+0x8** in the recompiled build.

Fix applied in `dllmain.cpp` (new unique pattern + these offsets); rebuilt; deployed.
Log now shows `HUD: Markers: Address is ...+2657b9d` (**15/15 patterns resolve**) and the
HUD lays out in proper proportion on 21:9 in-game. Lesson: **when a game has anti-debug,
your own already-injected mod is the debugger** — a mid-hook can dump any register/memory
to the log with zero detection surface.

## Bonus: Span HUD ported to 2.0 (2026-07-09)

The `HUD: Markers` fix corrects *world* markers, but the main HUD (party list, prompts)
still sat in a centered 16:9 band — the mod deliberately renders it undistorted there.
Naively rescaling the global HUD (HUD:Scale `NativeAspect → AspectRatio`) **stretched
everything** — wrong lever.

Right lever = master's `SpanAllHUD`. Master's "UI Constraints" pattern
(`48 ?? ?? ?? ?? ?? 00 48 ?? ?? 74 ?? C5..C5..C5.. EB ??`) **still matches uniquely on
2.0**. Same in-mod-diagnostic method dumped the element struct at `rax`:

```
[rax+0x1B4]=3840 [rax+0x1B8]=2160   reference w/h
[rax+0x1BC]=3840 [rax+0x1C0]=2160   constraint w/h (loaded into xmm2/xmm0 at +0x1C)
[rax+0x1CC]=0xFFFFFFFF               position-offset sentinel (unset for static HUD)
```

Hook at +0x1C: for any element with constraint `3840x2160`, override `xmm2 = 2160*aspect`
(register-only, no struct write). **Excluded world markers** by skipping elements whose
`rax+0x1CC` isn't the `0xFFFFFFFF` sentinel (markers use it as a live position offset).
Wired to a `[Span HUD]` config (Enabled + AspectRatio). **Menus/backgrounds left
16:9-centered** — that's master's `SpanAllBackgrounds` territory (flagged for visual
issues) and deliberately out of scope.
