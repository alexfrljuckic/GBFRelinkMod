# 24 — Sigil synthesis: full decode + shipped mod (2026-07-15 → 16)

Goal: hardwire the synthesis result's traits to **input A trait 1 + input B trait 2**
(per Alex). Table-impossible (input-dependent) → needed a code hook.

**Outcome: DONE and released** as `gbfr.synthesis.control` v1.0 (Always Grand Success +
Choose result traits). This doc is the whole trail in order: static recon → runtime
watchpoint → trait mechanism → the shipped hook → two crash post-mortems worth reading
before writing the next code mod.

Game exe: 2.0, 123,522,016 bytes, image base `0x140000000`. All addresses are RVAs
unless prefixed `0x14...`. **These shift on every game patch — re-derive via the
scripts, never hardcode.**

## New tools (this session)

- [scripts/find-xrefs.mjs](../scripts/find-xrefs.mjs) — RIP-relative xref scanner:
  `node scripts/find-xrefs.mjs <exe> <fileOffset>` (string/data) or `rva:<hex>`
  (globals). Finds lea/mov/call/jmp references by disp32 math over `.text`.
- [scripts/func-bounds.mjs](../scripts/func-bounds.mjs) — exact function boundaries
  from the PE exception directory (`.pdata`), follows chained unwind info:
  `node scripts/func-bounds.mjs <exe> <rvaHex>...`. Use this INSTEAD of the
  int3-padding heuristic (which cold-block clusters defeat).

## Architecture decoded

The engine loads every `.tbl` through per-family **loader functions** that build
runtime structures; gameplay code never touches the raw tables again:

1. **Gem-family loader = function `0x331e20`..`0x337d71`** (24 KB, everything
   inlined). Takes the manager/context object in `rcx`. Called ONCE from the global
   init (call site `0xb8e8e`, inside the 320 KB init function `0x985a0`..`0xe6bb4`).
   The object passed is read from **global `[0x147C20940]`** (the game context;
   subsystems live at fixed offsets inside it, e.g. `lea rsi,[rax+0x37FC0]` nearby).
2. Per table, the loader: frees the old singleton → loads via the generic resource
   system (`0x936c50` → kind `0x0E` create at `0x1422DAA80`, path as
   {ptr,len} string_view) → stores the resource object and parsed `{count,rows}`
   pointer in **per-table globals** → walks the rows and builds an
   **FNV-1a(key-uint) → row-pointer hashmap** stored in the subsystem object.
   - FNV-1a over the 4 key bytes, constants `0xCBF29CE484222325` / `0x100000001B3`.
   - gem_mix_success map header observed at **subsystem+0x180..0x1B8**
     (list head +0x180, buckets +0x188, count +0x190, hash mask +0x198).
3. **gem_mix_success specifics**: path string xref (unique) at `0x33619b`;
   resource singleton `[0x147029E70]`; parsed data `[0x147029E78]` (points at raw
   tbl bytes: qword row count, then 28 × 12-byte rows, Key at +8 — matches our
   headers). Rows are iterated with `rsi += 0x0C`. Adjacent globals `0x147029E88+`
   = next table's cluster (the loader does the whole gem family sequentially).
   Per-table atexit destructor thunks live around `0x331d00` (gem_mix_success's
   at `0x331d60`).
4. **No direct consumers exist for any per-table global** (xref-verified for
   gem_mix_success, skill_lot, skill_type_lot, gem, gem_rare — each has exactly one
   code reference: its own load site). Consumers go through the context/subsystem
   hashmaps. That kills the "find the consumer statically via the global" shortcut.
5. **skill_lot / skill_type_lot** have their own family loader `0x2fcee0`..`0x300ab0`
   (same shape, globals `0x147029A90`/`0x147029A98`).
6. **RNG**: MT19937 constants found. Seeder cluster (file `0x18c769`+, 4 inlined
   copies); bulk-generate function `0x27e830`..`0x27eb76` (AVX twist, state in an
   object at `[0x147C24980]+0x129F88/0x129F98/0x129FB0`). This looks like a
   fill-a-buffer service, not a per-roll API — the per-roll PRNG for gameplay may
   be elsewhere/inlined.
7. Anti-debug stands (x64dbg crashes on attach) — runtime work stays in-mod
   (docs/09 method). Hook anchor for the gem-family loader: 56-byte prologue at
   `0x331e20` is unique
   (`55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC D8 00 00 00 48 8D AC 24 80 ...`);
   wildcard the trailing `vmovups [rip+disp]` disp32 for patch tolerance.

## What Grand Success is (settled earlier this session)

Synthesis = 2 legendary (+) sigils → 1 new sigil, traits transferred (tutorial
`TU_WIN_BODY_GENECOMB_00`). `gem_mix_success` keys 33–60 = the UI "Prospect" score;
Great/Grand weights out of 10000 (remainder = plain Success), scored rows only on
even keys 44–60 (5500/4500 → 1500/8500). Grand ⇒ result's skills maxed (Lv15, per
Alex). Guaranteed-grand shipped in `gbfr.synthesis.control` (all rows 0/10000 — safe
under both weight interpretations, unlike Nexus 574's 0/1000 on scored rows only). Cost chain `gem_mix → item_tier_map → item_material_list` =
rupies/vouchers, NOT the result pool. `gem.CanGemMix` = input eligibility.

## Phase 1 DONE — live table dump confirmed (2026-07-15, autonomous run)

Diagnostic ASI built into the GBFRelinkFix harness (`vendor/GBFRelinkFix/src/synthdiag.cpp`
— **local-only: `vendor/` is gitignored and is its own checkout, so the harness is not
part of this repo**; see "Rebuilding the harness" at the bottom),
config `[Synth Diag] DumpTable/Watch`, deployed to the game's `scripts/`
(UAL/`winmm.dll` loads it), launched via Steam. Result:

- `gem_mix_success` resolved at **base+0x7029E78 → heap ptr**, `count=28`, keys
  **33..60** exactly as predicted. Base+RVA math is correct on the live 2.0 process.
- All 28 rows read **Great=0 / Grand=10000** → the `gbfr.synthesis.control` mod is
  ALSO confirmed working end-to-end (the game loaded the patched table). Bonus.
- Save mtime untouched (menu-only). GBFRUltrawide.asi co-loaded harmlessly.
- Note: the dump's "base+0x…" offset for the DATA pointer is meaningless (heap
  alloc, not module-relative) — only the GLOBAL is at base+RVA. Watchpoint RIP
  logging uses `rip - Base()`, which IS module-relative and correct.

## Phase 2 DONE — synthesis function caught live (2026-07-15)

Guard-page watchpoint (VEH, F8-armed on the confirm screen) fired during a real
synthesis. **Fix that mattered:** the first watch build crashed the game at boot —
the VEH returned `EXCEPTION_CONTINUE_EXECUTION` for *every* guard-page fault, but
Windows uses guard-page faults for thread-stack growth. Corrected to no-op unless
armed AND the fault is on our own page; also defer `AddVectoredExceptionHandler`
until arm-time so boot runs with no handler at all. Then it worked first try.

Captured reads (Prospect key 44, the low row): `rows+0x84` then `rows+0x88` from:

- **`0x346080` = `GetGemMixSuccessWeights(map, keyGetterArgs, out)`** — FNV-map
  lookup; copies row's Great→`out[0]`, Grand→`out[4]`. A leaf getter, not the logic.
- Two callers (xref of `0x346080`):
  - **`0x1ce5e80`..`0x1ce697d` (~2.8 KB) = THE SYNTHESIS GRADE ROLL.** Right after
    the getter: `great=out[0]; grand=out[4]; total=great+grand; div total; sub;
    setnl` — a weighted RNG bucket that picks the grade. Calls `0x140295090`
    (RNG?) before and `0x14038F310` (edx=0x81) after. With our 0/10000 patch this
    always buckets Grand — confirms the mod flows through here. **This is the hook
    target for deterministic trait transfer** (big enough to also hold the trait
    selection + result write).
  - `0x4029090`..`0x40293c4 (~0.8 KB) = UI PROSPECT PREDICTION.` Compares grand
    weight against a threshold list (`[r14+0x4D0/0x4D8]`) to choose the
    `COMP_PROB_LOW/MID/HIGH` "good feeling" message. Not the roll.

### Synthesis executor `0x1ce5e80` — full flow decoded

Params/state: `rdi = ctx [0x147C20940]`; synth-state `= [0x147C228D0]`;
**`ebx = [state+0x7B4]` = input sigil A id**, **`esi = [state+0x7B8]` = input sigil B id**
(both cleared to 0 at the end). Sequence:

1. `call 0x346830(ctx, A, B)` → `[rbp+0x2EC]` = **Prospect key** (33–60).
2. getter `0x346080` with that key → Great/Grand weights → `total=G+g; div; setnl`
   ⇒ `r12d` = **grade tier** (the caught read; our 0/10000 patch forces Grand here).
3. `call 0x346760(ctx, A, B)` → `[rbp+0x320]` = **result sigil** (handle/count).
4. `call 0x345D30(ctx, &out@rbp+0x2F8, A, B)` → builds the **combined trait pool**:
   `[rbp+0x2F8]` = count, `[rbp+0x300…]` = trait-id array (u32 each).
5. loop over the pool (`0x1ce6263`) + `call 0x346150` = **select/apply traits** to the
   result. ← the trait decision lives here + inside 0x346760/0x345D30.
6. Tail: `0x344E80(ctx, A)→r12d`, `0x344E80(ctx, B)→r15d` (per-sigil FNV-map lookup of
   the sigil id → its value/level); `0x33ED00 ×2` = **consume input sigils**;
   `0x33E290` = commit; a 15-entry jump table at `0x1461A8E48` computes the **result
   level/rarity** from `r12+r15`; clears `[state+0x7B4/0x7B8]`; consumes the synth cost
   (`0x356460`, item `0x9370A3A`).

### Trait mechanism fully decoded (static, 2026-07-15 overnight)

Disassembled steps 3–5. What synthesis actually does with traits:

- **`0x346760`** = result-sigil **type/identity** = `hash(value(A) + value(B))` looked up
  in a map (`ctx+0x148/0x158/0x170`). `value(sigil)` = `0x344E80` (per-sigil FNV lookup
  in `ctx+0x25C8` map). So the result sigil — and therefore its **innate (slot-0)
  trait — is a deterministic function of the two inputs**, not freely chosen.
- **`0x345D30`** = builds the **combined trait pool** (inline u32 array: count@`rbp+0x2F8`,
  data@`rbp+0x300`) by FNV-looking-up each input in `ctx+0x37F88` and copying its traits.
- **`0x346150`** = **insertion sort** of that pool by trait-id value (NOT an applier). So
  after this, source association (which came from A vs B) is **lost** — the A1+B2 rule
  cannot be read off the sorted pool.
- **Loop `0x1CE6221`..`0x1CE6287`** = **RNG pick** of the result's **secondary (slot-1)
  trait** from the pool: `call 0x38F310` (RNG on state `[0x147C23E40]`, range 0x81) with
  rejection sampling (`div`), leaving the chosen id in `pool[0]` = `[rbp+0x300]`.
- **`0x1CE6287`+** = resolves `[rbp+0x300]` (validates the trait via `ctx+0x248` map) and
  **applies it to the result sigil via the inventory object at `ctx+0x320`** (virtual
  call `[rax+0x18]`).

**Consequence for the feature.** Synthesis gives the result an innate fixed by the
(A,B)-derived sigil type and **one** freely-varying slot — the **secondary**, RNG-picked
from A's+B's combined traits. So "trait1 from sigil A, trait2 from sigil B" maps cleanly
onto reality as: **force the secondary pick = B's secondary trait** (and the innate is
already A-influenced via the type hash). Full arbitrary control of BOTH slots would also
require overriding the result sigil type (`0x346760`) — a bigger change; confirm whether
the user needs that after seeing a real trace.

### Hook point (once trace confirms)

Cleanest override: **mid-hook at `0x1CE6287`** and overwrite `[rbp+0x300]` with the
desired trait id before it's consumed — one 4-byte write, no struct knowledge needed,
downstream apply (`ctx+0x320`) does the rest. The desired id (e.g. B's secondary) comes
from the trace-confirmed pool/sigil mapping. Config selector on the mod:
`Secondary = B.slot1 / A.slot1 / specific-trait / off`.

### Phase 3 — read-only trait trace (BUILT + DEPLOYED, awaiting one test synthesis)

`[Synth Diag] TraitTrace=true` installs 3 read-only mid-hooks in the executor
(`synthdiag.cpp`, local-only — see above): logs the two input sigil ids
(`rbx`,`rsi`), the pool after build (`0x1CE61ED`) and after sort (`0x1CE620B`), and the
final RNG-picked secondary (`0x1CE6287`). Fires automatically on any synthesis — **no F8**.
Deployed to the game's `scripts/`; ini has `DumpTable=true, Watch=false, TraitTrace=true`.

## Phase 3 RESULT — model CONFIRMED live (2026-07-16, Alex's test synthesis)

Inputs (from Alex's screenshots): **A = Attack Power V+** (ATK, Skilled Assault),
**B = Steady Focus V+** (Steady Focus, Potion Hoarder), both Lv11 Legendary.
**Result: Potion Hoarder V+ Lv15**, traits Potion Hoarder + Steady Focus (T.Lv15 both —
the grand-success mod visibly working).

Trace log:
```
POOL(after build) A=0x1895 B=0x1BE1  count=4  pool: 50079A1C EAE321EB 0053599E 24883AF3
POOL(after sort)                     count=4  pool: 0053599E 24883AF3 50079A1C EAE321EB
SELECTED                             = 0x24883AF3
```
Hashes resolved by brute-forcing the custom XXHash32 over `extracted/skill-names-en.tsv`
(`SKILL_nnn_00` form):

| built idx | hash | trait | source |
|---|---|---|---|
| 0 | `50079A1C` | ATK | **A.slot0** |
| 1 | `EAE321EB` | Skilled Assault | A.slot1 |
| 2 | `0053599E` | Steady Focus | B.slot0 |
| 3 | `24883AF3` | Potion Hoarder | **B.slot1** |

**Confirmed model:**
- **A = the FIRST slot filled (the LEFT one when picking); B = the second (right).**
  Per Alex 2026-07-16: the selection UI is left/right, left being the "first" position;
  the Synthesis Request confirmation then stacks them A-above-B (which is what the first
  screenshot showed, and why an early draft mislabelled these "top/bottom"). Verified by
  re-running the same two sigils in REVERSE order — the result mirrors accordingly.
  `A = [synthState+0x7B4]`, `B = [synthState+0x7B8]`.
- Pool is built in **source order `[A.s0, A.s1, B.s0, B.s1]`** (sort+shuffle then destroy it).
- Sorted ascending (`0x346150`), then Fisher-Yates **shuffled** by the RNG loop.
- **`pool[0]` = result INNATE** (→ sigil identity): selected `0x24883AF3` = Potion Hoarder
  ⇒ result "Potion Hoarder V+" ✓ matches the screenshot exactly.
- **`pool[1]` = result SECONDARY** (Steady Focus in this run — inferred from the result;
  the only link not directly logged, now logged by the updated trace).
- The UI's **"Prospect 1 … 1/12"** = **4 traits → 4×3 = 12 ordered DISTINCT pairs**
  (innate, secondary). The game shuffles and takes the first two. LB/RB previews them.

## Feature: `ForceA1B2` — ✅ VERIFIED WORKING (2026-07-16)

Live test, same pairing (A = Attack Power V+ [ATK, Skilled Assault], B = Steady Focus V+
[Steady Focus, Potion Hoarder]):
```
PICKED (vanilla RNG) innate=0x24883AF3 secondary=0x0053599E   <- would have been Potion Hoarder + Steady Focus
ForceA1B2: result forced to innate=0x50079A1C (A.trait1), secondary=0x24883AF3 (B.trait2)
```
**In-game result: "Attack Power V+", Lv15/15 Legendary, ATK (T.Lv15) + Potion Hoarder
(T.Lv15)** — exactly the forced pair. Two things proven by this run:
1. `pool[1]` = SECONDARY is now **measured, not inferred** (the vanilla line shows
   innate/secondary = the previous run's actual result).
2. Overriding `pool[0]`/`pool[1]` at `0x1CE6287` fully controls the result sigil's
   identity AND its secondary trait. The RNG is defeated deterministically.

### Design notes (details below)

Implements Alex's rule "first trait of the first sigil + second trait of the second":
- **Hook `0x1CE61ED`** (pool built, source order intact): stash `pool[0]`→A.s0,
  `pool[3]`→B.s1 (+ rbp, for frame matching).
- **Hook `0x1CE6287`** (post-shuffle, before consume): write `pool[0]=A.s0`,
  `pool[1]=B.s1`.
- Guards: only when `count==4` (both sigils have 2 traits); skip if `A.s0==B.s1`
  (pairs must be distinct); rbp must match the stashing frame.
- Failure mode is benign: both writes are inside the verified 4-element stack array, so a
  wrong `pool[1]` semantic ⇒ correct innate + random secondary, no crash.
- Config `[Synth Diag] ForceA1B2`; trace logs vanilla-RNG pick AND the forced values.

On the test pairing above it yielded **Attack Power V+ with ATK + Potion Hoarder** ✓.

## Shipped: `gbfr.synthesis.control` 1.0-dev3 (2026-07-16)

Hook lifted out of this diagnostic harness into the real mod (harness kept for future
recon; its `[Synth Diag]` flags are all OFF so nothing double-hooks). See
[SynthesisHook.cs](../mods-src/gbfr.synthesis.control/SynthesisHook.cs).

- Two Reloaded `IAsmHook`s of **pure asm, no managed callback** — deliberately avoids
  shadow-space / stack-alignment / XMM-preservation hazards. Clobbers only rax/rcx/rdx
  + flags, all pushed/popped. No CALL ⇒ no alignment constraint.
- Selection reaches the asm as **two pointers** into an unmanaged buffer that C# rewrites
  on config change (enable byte written LAST so the asm can't see a half-update) ⇒ any of
  the 12 prospects is choosable, live, no restart.
- Sites located by **unique byte-pattern scan**, not hardcoded RVAs; live scan resolved to
  exactly the statically-derived `.text+0x1CE61ED` / `.text+0x1CE6287`. On a game patch
  the pattern misses ⇒ feature self-disables with a warning, Grand Success unaffected.
- Guards: `count==4`; chosen traits must differ; frame match.

### ⚠️ Crash lesson (dev2 — cost a game launch)

The first C# scanner walked the **whole module** (`ModuleMemorySize` ≈ SizeOfImage,
123 MB) with raw pointers. That range includes non-readable pages → **access violation**,
which **.NET cannot catch** (corrupted-state), so the process died instantly inside the
mod constructor. Diagnostic signature: the Reloaded log stops right after
`Loading: <mod>` / `- Location: ...` with **no `LoadTime`** and none of the mod's own
lines ⇒ crash in that mod's ctor.

Fix: parse the PE section table and scan **only `IMAGE_SCN_MEM_EXECUTE` sections** —
which is exactly what `vendor/GBFRelinkFix/src/memory.cpp` (`GetModuleSections`) has
always done on this game. **Mirror the proven scanner; never hand-roll a module-wide byte
walk.** Also added log-before-each-step + try/catch so failures degrade instead of dying.

## Rebuilding the diagnostic harness (local-only)

The recon ASI is **not in this repo**: `vendor/` is gitignored and
`vendor/GBFRelinkFix` is its own checkout, so `src/synthdiag.cpp` lives only on the
machine that built it. Nothing in the shipped mod depends on it — it exists purely to
re-derive addresses if a game patch breaks the byte patterns. To recreate it:

1. Clone GBFRelinkFix into `vendor/GBFRelinkFix`, add `src/synthdiag.cpp` with the
   three feature blocks described above (`DumpTable`, `Watch`, `TraitTrace`), register
   the config items in `src/config.h` under a `[Synth Diag]` section, and call
   `SynthDiag()` from `Main()` in `src/dllmain.cpp`.
2. Build with `tools\xmake\xmake.exe build`; deploy `GBFRelinkFix.asi` + a
   `GBFRelinkFix.ini` (everything off except the diag flags) into the game's
   `scripts/` folder — Ultimate ASI Loader (`winmm.dll`) picks it up. Log lands in the
   game root as `GBFRelinkFix.log`.
3. Remember the two hazards that cost real time: the **VEH guard-page rule** and the
   **executable-sections-only scan rule** (both above).

Faster path for re-deriving the patterns after a patch: `scripts/find-xrefs.mjs` on the
`system/table/gem_mix_success.tbl` path string → the gem-family loader, then
`scripts/func-bounds.mjs` for exact function bounds, then re-cut the two signatures
around the `lea rcx,[rbp+0x300]` / `mov edx,[rbp+0x300]` sites and re-verify each
matches exactly once.
