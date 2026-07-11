# 22 — Config vs programmatic: evaluation (2026-07-11, overnight session)

Alex asked: is the checkbox config too restricting? Should legality + "remove
character combos as you get them" be handled programmatically from a library of
legal combos instead — and can that coexist with a config at all?

## TL;DR recommendation

**Hybrid, with the config demoted to *preferences* and everything mechanical
moved into the dll.** Implemented tonight as 2.0-dev9:

- **Legality is already programmatic** (since dev6) — the per-sub-lot
  intersection rule derives it from vanilla tables at every launch; no checkbox
  can create an illegal combo. The formal **legal-combo library** now exists as
  a generated artifact: [mods/transmarvel-overhaul/legal-combos.json](../mods/transmarvel-overhaul/legal-combos.json)
  / [LEGAL-COMBOS.md](../mods/transmarvel-overhaul/LEGAL-COMBOS.md) —
  41 mains (28 character-specific + 13 offensive-generic) × 62 legal
  secondaries = **2,542 legal combos**, with vanilla odds per secondary.
- **Combo-completion pruning is now baked into the dll** (`AutoPruneCompleted`,
  default on): at every launch it reads the newest save via the new
  [SaveReader.cs](../mods-src/gbfr.transmarvel.overhaul/SaveReader.cs) and drops
  any *ticked* Warpath+ whose every rollable secondary (ticked ∩ lot-14 legal)
  is already owned. No script run, no config rewrite, no restart dance beyond
  the one the tables already need. The offline
  `scripts/build-jackpot-tables.mjs` config-writer is now redundant for this
  (kept as a manual/inspection tool).
- **The config stays, but only for genuine preferences**: which traits you
  *want* (taste), which sigils you *want* (taste), voucher income, cost. The
  machine handles what's *possible* (legality) and what's *pointless*
  (completed combos). Preference and mechanism no longer fight over the same
  checkboxes.

## Why not fully programmatic (no config)?

Considered and rejected:
- "Good traits" is taste, not derivable — tonight alone the curated list went
  18 → 12 → 11 → 7 by Alex's choices. Hardcoding it means a rebuild per whim;
  that's strictly worse than checkboxes.
- The config *operational* pain we hit tonight wasn't caused by configurability
  itself but by two sharp edges, both now mitigated:
  1. the Reloaded launcher silently drops unknown keys when saving from a
     stale session (documented; fields now change rarely since mechanics moved
     out of config);
  2. the pruner used to *write* the user's checkboxes (contention) — now it
     doesn't write anything; pruning is a launch-time computation.

## Why not fully config (dev8 status quo)?

- Combo-completion state changes every play session; a checkbox snapshot of it
  is stale by design and needs a script run + restart to refresh. As a
  launch-time computation it is always exactly as fresh as the save.
- 114 checkboxes as the *only* interface invites exactly the clobbering/staleness
  failures observed live.

## Precedence model (dev9)

pool = (ticked sigils) − (combo-complete Warpath+, if AutoPruneCompleted)
traits per sub-lot = (ticked traits) ∩ (legal for every path referencing it)

User untick always wins (stays out). Auto-prune only ever *removes*, never
re-adds an unticked sigil, and reverses itself automatically when new traits
are ticked (new combos to chase). If pruning would empty the pool, it backs
off and keeps the unpruned pool (logged). If the save can't be parsed, it
fails open (no pruning) and says so in the console.

## Tests run (all passing)

| Test | Result |
|---|---|
| Differential: C# SaveReader vs JS reference, current save (2,289 sigils) | byte-identical output |
| Differential: same, pre-session snapshot fixture | byte-identical output |
| Negative: SystemData.dat (wrong file) | clean refusal, "could not find the sigil inventory array" |
| Negative: truncated save (1.5 MB cut) | clean refusal, same guard |
| Auto-prune simulation on live save + live config (7 ticked traits) | 27 user-unticked respected; Butterfly's Warpath+ 0/7 → stays; 0 pruned (correct) |
| Build | 0 errors |

Harness: [tools-src/savereader-test/](../tools-src/savereader-test/) — compiles
the mod's actual `SaveReader.cs` against real save fixtures; keep running it
after game updates before trusting auto-prune.

## Still open (not overnight-solvable)

- **The "5 vouchers from a 99 grant" engine clamp** — chunked ≤20 grants
  (`RWL_TMV_S1..S5`) shipped in dev8b; Alex's next quest clear is the test.
- **The 3 impossible junk secondaries in the 10-pull video** — pre-rolled
  "Transmarvel Stock" theory; needs a big pull batch (vouchers above) and a
  `--since` snapshot diff to confirm junk drains to zero.
