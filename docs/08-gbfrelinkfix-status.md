# GBFRelinkFix × 2.0: Ragnarok-Branch Analysis (original research)

*Produced 2026-07-09 by scanning the release 2.0 exe
(`granblue_fantasy_relink.exe`, dated 2026-07-08, 123.5 MB) on disk with the patterns
from [Lyall/GBFRelinkFix](https://codeberg.org/Lyall/GBFRelinkFix)'s `ragnarok` branch.
Tools: [scripts/scan-patterns.mjs](../scripts/scan-patterns.mjs),
[scripts/fuzz-pattern.mjs](../scripts/fuzz-pattern.mjs). Repo cloned at
`vendor/GBFRelinkFix` (gitignored).*

## Branch state

The `ragnarok` branch (7 commits, 2026-06-19, i.e. **written against the ER demo**,
untouched since) is a **complete rewrite**, not a patch-up: new
`memory`/`logger`/`screen`/`config` modules, xmake build producing a safetyhook-based
`.asi`, ~70k lines of vendored deps dropped. Feature parity with master is already in:
intro skip, custom resolution, FOV fix + camera multipliers, HUD scale/markers, shadow
resolution, TAA off, LOD distance, FPS unlock. Meanwhile `master` (= the crashing v1.1.1)
is a different, older codebase — the fix for
[issue #1](https://codeberg.org/Lyall/GBFRelinkFix/issues/1) is clearly "finish ragnarok".

## Scan result: 13/15 demo-era patterns still match the release exe

| Pattern | Result (file offset) |
|---|---|
| Intro Logos | ✅ unique @ `0x3f485b7` |
| Press Any Key | ❌ miss — **fixed below** |
| Current Resolution | ✅ unique @ `0x6c072e` |
| Resolution List | ⚠️ **4 matches** (`0x17bf3b`, `0x1c82e1`, `0x215c09`, `0x21b282`) — ambiguous, needs disambiguation before trusting first-match |
| Resolution String | ✅ unique @ `0x4269140` |
| Startup Aspect Ratio | ✅ unique @ `0x230fe3` |
| Gameplay Camera FOV/Distance | ✅ unique @ `0x94d91b` |
| Cutscene FOV | ✅ unique @ `0x946ffd` |
| HUD: Scale | ✅ unique @ `0x15eec8` |
| HUD: Markers | ❌ miss — candidate found below |
| Shadow Resolution | ✅ unique @ `0x21a7b93` |
| TAA Setting | ✅ unique @ `0x2164550` |
| LOD Distance | ✅ unique @ `0x806ce6` |
| Framerate: Limiter | ✅ unique @ `0x1b62b3` |
| Framerate: Game Speed Floor | ✅ unique @ `0x1b64ee` |

(Offsets are raw file offsets, not RVAs. Caveat: on-disk scan ≈ runtime scan for
unpacked `.text`, but only a live run proves hook correctness.)

## Fix 1: Press Any Key — repaired signature, verified unique ✅

At `0x29ff1df` the release code is identical to the demo pattern except one instruction:
`mov rax, [r14+0x198]` is followed by **`cmp rax, [r14+0x1A0]`** (`49 3B 86`) where the
old pattern expected another `mov` (`49 8B`):

```
0f 84 c5 00 00 00   jz   +0xC5          <- bytes the mod NOPs (unchanged)
4d 85 f6            test r14, r14
0f 84 bc 00 00 00   jz   +0xBC
49 8b 86 98 01 00 00   mov rax, [r14+0x198]
49 3b 86 a0 01 00 00   cmp rax, [r14+0x1A0]   <- was 49 8B in the demo sig
```

Proposed replacement (verified **unique** in the release exe; the patched `jz` prefix is
untouched, so the existing 6-NOP patch logic stands):

```
0F 84 ?? ?? ?? ?? 4D 85 ?? 0F 84 ?? ?? ?? ?? 49 8B ?? ?? ?? ?? ?? 49 3B ?? ?? ?? ?? ??
```

## Fix 2: HUD: Markers — one strong candidate, needs a live check ⚠️

Fuzzing located the pattern's tail at `0x2656fa4`; in context the release code is the
original sequence **minus the `vdivss`** (compiler folded it), with a different base
register:

```
c5 f0 59 0d eb ce e4 02   vmulss xmm1, xmm1, [rip+...]
c5 fb d0 c1               vaddsubps xmm0, xmm0, xmm1
0f b6 86 48 03 00 00      movzx eax, byte [rsi+0x348]   <- was r14-relative before
```

A second textual match (`0x49369da`) disassembles as integer code — coincidence, ruled
out. Because the hook body reads `ctx.xmm2` and writes `r14+0x1B4`/`+0x1B8`, the
register/offset drift (`rsi+0x348` here) means **both the signature and the hook body
need updating**, and confirming the right xmm/struct offsets requires a debugger session
(x64dbg) — next session's job.

## Remaining work items

1. **Live-verify** the 13 matching hooks actually behave (build the branch, run the game).
2. **Disambiguate `Resolution List`** (4 matches — inspect which site the demo build's
   unique match corresponds to, or lengthen the sig).
3. **Finish `HUD: Markers`** via x64dbg at `0x2656fa4`-equivalent RVA.
4. Build toolchain: the branch uses **xmake + MSVC** — neither installed here yet
   (no VS/cl/cmake/xmake found 2026-07-09); install VS 2022 Build Tools + xmake first.

## Draft message for Lyall (issue #1 or Discord) — Alex to review/post

> I scanned the release 2.0 exe (2026-07-08 Steam build) with the patterns from the
> `ragnarok` branch: 13/15 still match (11 unique). The two misses:
> **Press Any Key** — one instruction changed from `mov` (`49 8B`) to `cmp` (`49 3B`);
> `0F 84 ?? ?? ?? ?? 4D 85 ?? 0F 84 ?? ?? ?? ?? 49 8B ?? ?? ?? ?? ?? 49 3B ?? ?? ?? ?? ??`
> matches uniquely and the patched `jz` is unchanged.
> **HUD: Markers** — the `vdivss` was folded out; the sequence survives as
> `vmulss` + `vaddsubps` + `movzx` at one site, but the movzx base moved to `rsi+0x348`
> so the hook body offsets need re-checking. Also **Resolution List** matches 4 sites
> in the release build, so first-match may be unsafe. Happy to test builds on a
> 2.0 install.
