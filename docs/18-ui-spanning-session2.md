# UI spanning session 2 (2026-07-09 late): Lock-On/Dodge port + the UI Size Setter

> **FINAL OUTCOME (2026-07-10 ~14:00): Alex switched to
> [zhen469891's GBFRUltrawide](https://github.com/zhen469891/gbfr-ultrawide) v0.2.1**
> (installed in the game folder; our build parked in `scripts/_disabled_GBFRelinkFix`).
> The spanned-UI experiment ended on the **shared-layer conflict**: quest-flow screens
> need ~50 element layers excluded from spanning, and at least one glow/backing layer is
> SHARED with the main menu with opposite needs (must span on menus, must not on the
> quest board) — per-id exclusion can't reconcile that, and per-screen context isn't
> observable from the hooks. Everything else verified: backgrounds/menus/loading spanned,
> combat VFX (ScreenEffects), lock-on/dodge, bubbles, results screens. The research —
> full element struct map, reader-hook architecture, id censuses, the conflict analysis —
> lives in this doc and vendor branch `ragnarok-2.0-fixes` (final commit `5fce26f`).
> No ultrawide-v2 release; ultrawide-v1 stays up with a pointer to zhen's mod as the
> maintained option. If anyone resumes: the fix needs screen-context awareness (e.g.
> hooking the screen-transition state machine to toggle exclusions per scene), or zhen's
> canvas-table approach (span the 41 named canvases individually at 0x05A3CA70, per-canvas
> opt-out — same conflict may exist there though).

> **RUN 1 RESULTS (2026-07-09 23:36, 3440x1440):** only team icons (Span HUD) moved; menus
> and battle UI unchanged. Log evidence: (a) all 3 setter hooks installed but ZERO elements
> passed at 3840x2160 — and dropping the trailing-`ret` from the pattern reveals the setter
> is inlined at **26 sites** with per-copy register allocation (exe+0xa7fbfd uses xmm4/xmm3);
> (b) Guard/Lock-On (0x241d5d22) and Dodge (0xd39bd079) ids CONFIRMED at +0x1C4 on 2.0, but
> `+0x1CC/+0x1D0` hold byte-flags for them (0x03/0x33/0x01000101), NOT master's offset floats
> — the docs/17 "+0x1CC position offset" claim is wrong for these elements; (c) GameplayHUD
> id 1719602056 never hits the constraints site on 2.0. Deployed **v2 (vendor `1dc1d13`)**:
> all 26 setter sites hooked after the stores with struct writes, a size census (units
> question: authored vs HUD-pixel space), one-shot +0x140..+0x220 struct dumps for
> GuardLockOn/Dodge/StaticHUD3840, offset writes disabled, `PixelSpace` span key added.
> **Next: run 2, then read the census + dumps in GBFRelinkFix.log.**

> **RUN 2 RESULTS + v3 (2026-07-09 ~23:47):** census + struct dumps landed everything.
> (a) Setter sizes are authored units but full-screen elements NEVER pass it — and the
> StaticHUD3840 dump shows the struct keeps W=3840, proving menus stayed 16:9 because our
> SpanHUD port is register-only while the menu-bg rect-builders read the STRUCT (master's
> SpanAllHUD wrote the struct; that's the missing piece). (b) Field map corrections:
> `+0x150/+0x154` = element position in authored space (GuardLockOn x=432, Dodge x=3008,
> y=1058.67), `+0x1CC` = int index (-1 for full-frame statics — why the sentinel filter
> works), `+0x1D0` = byte-flags 0x01000101, `+0x1A4..+0x1B0` = anchor min/max pairs the
> setter compares. (c) Video frames confirm: menus/quest-board clamped to the 16:9 band,
> big-attack cut-in seams, Guard/Lock-On cluster floating ~440px inboard while the rest of
> the HUD spans. **v3 deployed (vendor `fe7482b`):** [Span Backgrounds] now struct-writes
> +0x1BC at the constraints site (write-once by construction); FixLockOn/FixDodge shift
> +0x150 X by -/+(2160*aspect-3840)/2 from a cached original; setter hooks census-only.

> **RUNS 3–5 + LIVE AUTONOMOUS VERIFICATION (2026-07-10, vendor `ce8aae4`): BACKGROUNDS
> SOLVED.** With Alex AFK and the ultrawide disconnected, Claude ran the game solo —
> windowed 1920x800 (2.4:1) via the Custom Resolution feature on the 1080p monitor,
> scancode-level SendInput for menus, desktop screenshots for verification, save files
> hash-verified untouched. Findings: (run 3) full-screen backgrounds never pass the
> UI-Constraints site either — struct-writes there don't reach them; (v4) hook every VEX
> float load of `[rax|rcx+0x1BC]` (4 encoding forms, ~50 sites) and write the struct at
> read time, value-gated on 3840x2160; (run 4) FadeBlack reached the readers with a live
> int at +0x1CC — the -1 sentinel gate is constraints-site-only, dropped for readers (v5).
> **Visually verified spanned: title bg, Load Game bg, in-town Main Menu.** Census mapped
> 10 background ids incl. docs/17's unknowns (see `ce8aae4` message / archived log
> [GBFRelinkFix-claude-live-2026-07-10.log](GBFRelinkFix-claude-live-2026-07-10.log)).
> Remaining: loading-screen tips bg still 16:9 (unhooked encoding form); lock-on/dodge
> +0x150 shift needs a combat run; screen-effects seams need a big-attack capture; Alex's
> game display settings were left Windowed on device 2 (LG) — re-pick when the ultrawide
> is reconnected.

> **v6 (vendor `c66286a`): FULL reader coverage — 168 sites.** Replaced the four
> hand-picked forms with decode-and-dispatch over all VEX2 + VEX3 loads of `[reg+0x1BC]`
> (base register decoded from the matched bytes, per-register callback table). Re-verified
> live: **Main Menu now spans completely** (v5 left one layer's strip at the right edge);
> loading-tips bg mostly spans (one cloud layer still short on the left — minor).
> Deployed ini restored to desktop-resolution config with Span HUD + Span Backgrounds +
> FixLockOn/FixDodge on; Alex's save hash-verified untouched after both live sessions.
>
> **State for Alex's next ultrawide session:** everything is deployed — just play. Check
> (1) menus/pause/quest-board span at 3440x1440, (2) lock-on/dodge indicator positions in
> combat (the +0x150 shift is implemented but combat-untested — set FixLockOn/FixDodge
> false if they look wrong), (3) big-attack screen-effect seams (likely still present —
> next work item). Game display settings need re-picking (Windowed/LG was set during
> testing). If all verifies, cut `ultrawide-v2` per CLAUDE.md release conventions.

> **LIVE ULTRAWIDE SESSION WITH ALEX (2026-07-10 09:00-10:00, vendor `ea278e2`, builds
> v7-v10): VERIFIED at 3440x1440** — main menu/pause/quest-board/load/results/title all
> span; **lock-on/dodge correct with the shift OFF** (Span Backgrounds spanning their
> canvas is the whole fix — the +0x150 shift clipped them and is retired to
> default-false); **NPC bubble flicker fixed** (world-projection canvas excluded from
> reader hooks via id learned in the Markers hook — one writer per canvas); cinematic
> letterbox strips (3840x400) span via the generalized [3840,4032)-any-height gate.
> **REMAINING (next session):** (1) in-battle BREAK/skill side vignette — CONFIRMED not a
> UI element (survived reader/setter/constraints interception at 10 builds); it's the
> separate screen-effects pipeline = RetroGawd's `ScreenEffects1`. Hunt the effect-quad
> construction in the graphics pipeline (aspect/resolution consumers outside the UI
> system). (2) slight left-side menu vignette (minor cosmetic, one more overlay layer).
> Diagnostics for both are in the build: `LogOddFullscreenOnRead` + edge-child census.
> **Release call: the verified set is `ultrawide-v2`-worthy** — vignettes are documented
> known-issues.

> **FINAL (2026-07-10 ~10:15, vendor `9f762a6`): ALL VERIFIED — including combat VFX.**
> Alex pointed at [zhen469891/gbfr-ultrawide](https://github.com/zhen469891/gbfr-ultrawide)
> (MIT, an independent v2.0.2 rebuild of Lyall's mod). Their ADR-0012 solved our battle
> vignette: it's the view-CB `+0x59C` shader factor sizing combat-VFX quads — and the fix
> is **master's own Screen Effects hook that our branch never ported**. Site re-verified
> against the exe ourselves (unique pattern, register-only store override at
> exe+0x20d117b), ported, and **Alex verified in combat: BREAK/skill bars GONE**, menus
> clean. Their ADR-0009 also documents a nameplate-scale fix (pattern-based) if we ever
> see nameplate drift — currently nothing reported. The UI-spanning project is COMPLETE
> pending release packaging (`ultrawide-v2`).

Follow-up to [docs/17](17-ui-spanning-handoff.md). Both features below are built, deployed
to the game folder, and **awaiting one in-game verification run** (checklist at the bottom).
Vendor commit: `ca259f6` on `ragnarok-2.0-fixes`; patch re-exported to
[gbfrelinkfix-2.0-fixes.patch](gbfrelinkfix-2.0-fixes.patch).

## 1. Lock-On + Dodge (docs/17 step 1) — ported, needs in-game check

Master's exact offset formulas, folded into the existing "HUD: Span" UI-Constraints hook
(one hook per pattern site — the second scan fails, so span + offsets share one lambda):

- Guard & Lock-On (`605904162`): `+0x1CC = -((2160*aspect-3840)/2)` at >16:9
- Dodge (`3550204025`): same magnitude, positive sign
- 2.0 slots: id at `+0x1C4`, offsets at `+0x1CC/+0x1D0` (docs/17 struct map)

Config: `[Span HUD] FixLockOn / FixDodge` (default off; enabled in the deployed test ini).
One-shot diagnostics (`HUD: Span: GuardLockOn seen: ...`) log each element's pre-write
state, so the same run that shows the fix also proves the ids exist on 2.0.

## 2. THE BIG FIND: the "UI Size Setter" (docs/17 step 2 cracked statically)

Static scan of the exe (no game run needed) for float readers/writers of `[reg+0x1BC]`
(constraint W) found, besides the known decoy rect-builder family around `exe+0x334xxxx`,
a tiny function that **writes** the constraint size — the single source every consumer
(including whatever draws the visible background) reads from:

```
; exe+0x334375b (entry), rcx = element, r8 = rect {x0,y0,x1,y1}
W = [r8+8] - [r8+0] ; H = [r8+0xC] - [r8+4]
if |[rax+0x1AC]-[rax+0x1A4]| <= eps: [rax+0x1B4] = W   ; reference W (conditional)
if |[rax+0x1B0]-[rax+0x1A8]| <= eps: [rax+0x1B8] = H   ; reference H (conditional)
[rax+0x1BC] = W                                        ; constraint W (always)
[rax+0x1C0] = H                                        ; constraint H (always)
ret
```

The same code is stamped out at **three byte-identical sites** (modulo rip displacements):
`exe+0x33437b6`, `exe+0x3965bf6`, `exe+0x4259049` (pattern starts at the `vandps`; the
`[rax+0x1BC]` store is +0x1A). Since patterns can't tell them apart, new infra hooks all
matches: `Memory::PatternScanAll`/`FindPatternAll` + `MAKE_MIDHOOK_ALL` (memory.h/.cpp).

Hook behavior (`[Span Backgrounds]`, default off; enabled in deployed test ini): for any
element being sized exactly 3840x2160, override the W register to `2160*aspect` before the
constraint store (H at <16:9), keeping reference W/H original — mirroring master's
span semantics. World-positioned elements (live `+0x1CC` offset) are skipped like Span HUD.
Every 3840x2160 element id passing through is logged one-shot
(`Span Backgrounds: 3840x2160 element: id=...`, capped at 48) — this maps the UNKNOWN
menu-bg/combat-overlay candidates from docs/17 in a single run.

Why this beats hooking readers: the earlier attempt overrode W inside one rect-builder
(`exe+0x3340ad5`) and the write provably fired without moving pixels — wrong consumer.
Writing at the setter changes the value for **all** consumers.

Open question the log will answer: is `+0x1C4` (id) already populated when the setter runs?
If ids come out as garbage/zero, id-gated spanning needs a different anchor (log will show).

## Sibling rect-builder family (for reference, from the same static scan)

Near-identical consumers reading `[rax+0x1BC]` + anchors `0x194/0x198`: `exe+0x3340615`,
`0x3340797`, `0x3340afc` (the decoy master hooked), `0x3340d80`, `0x33415de`. If the setter
approach fails, these are the next candidates to diagnostic-hunt. Scan tooling:
[tools-src/scan-disp](../tools-src/scan-disp/README.md) (tracked, documented — reproduces
all of the above with `scan-disp.ps1 -Disp 0x1BC -PairDisp 0x1C0 -FloatOnly`).

## Deployed state (Alex's game folder)

`scripts/GBFRelinkFix.asi` rebuilt (build ok + all label strings verified in binary) and
`scripts/GBFRelinkFix.ini` set to TEST CONFIG: `[Span HUD] Enabled=true, FixLockOn=true,
FixDodge=true` + `[Span Backgrounds] Enabled=true`. Revert to non-experimental by setting
those four to false.

## Verification checklist (one game run)

1. Launch game → main menu / pause menu / load-save screen: do the **backgrounds now span**
   the full width (no 16:9 pillarbox seams)? Screenshot each.
2. Combat: lock on to an enemy (does the lock-on/guard indicator sit at the correct
   position, not floating mid-screen?) and dodge (same for dodge indicator). Screenshots.
3. Big-attack screen effects: still seamed or fixed? (Setter hook may or may not cover
   these — separate `ScreenEffects1` site per RetroGawd's map.)
4. Watch for regressions: markers/NPC bubbles pinned correctly, menus not stretched oddly.
5. Grab `GBFRelinkFix.log` — the `Span Backgrounds:` id lines (map them against docs/17's
   known + UNKNOWN id lists) and the three `UI Size Setter: Address is ...` lines, plus
   `HUD: Span: GuardLockOn/Dodge seen:` lines (confirm ids + pre-write offsets on 2.0).
6. If backgrounds still don't move: the setter wrote struct fields but the visible bg is
   driven elsewhere — fall back to the sibling rect-builder family above.
