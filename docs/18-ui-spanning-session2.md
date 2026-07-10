# UI spanning session 2 (2026-07-09 late): Lock-On/Dodge port + the UI Size Setter

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
