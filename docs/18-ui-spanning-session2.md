# UI spanning session 2 (2026-07-09 late): Lock-On/Dodge port + the UI Size Setter

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
`scan1bc.ps1` (PE-parsing byte scanner, session scratchpad; rewrite from this doc if needed).

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
