# Real-Time RNG-Tuning UX — Feasibility & Design (2026-07-09)

Question: can drop tables be edited **in-game in real time** with a hideable overlay (the
Monster-Hunter/Insert-key pattern), so users configure their own drop rates / loot? And
if so, how do we make it **digestible**, focused on cumbersome RNG (Transmarvel, curios)?

## Feasibility: YES — and most of it already exists

- GBFR renders on **D3D11 + DXGI** (confirmed from exe imports) — the *easiest* target for
  an ImGui overlay hook.
- Nenkai already built the overlay: [`gbfr.utility.modtools`](https://github.com/Nenkai/gbfr.utility.modtools)
  (Nexus [#533](https://www.nexusmods.com/granbluefantasyrelink/mods/533)) — a C#
  Reloaded-II ImGui overlay with **real-time table editing straight from game memory**,
  plus a file logger and reflection dumper. This is exactly the MHW-style thing.

### The load-bearing connection to our work
The overlay locates each table in memory then reads its rows with **the same schema code
we just extended**:
```csharp
// TableManagerBase.AddTableMap
TableMappingReader.ReadColumnMappings(name, new Version(1, 3, 1), out int readSize);
```
It's hardcoded to **1.3.1**. On 2.0 that misreads every changed table (wrong `readSize` →
misaligned columns → garbage/crash) — the identical problem we solved for the CLI. **Our
reversed 2.0 headers are a required piece to make this overlay work on Endless Ragnarok.**
We're holding the key.

## What the existing overlay already covers (and doesn't)

Its manager hooks expose these live (relevant ones **bolded**):
- Item: `item`, `item_category`, **`item_junk_appear_rate`** (junk-drop rates), material tables…
- Gem/sigil: `gem`, **`gem_rare`**, **`gem_mix_success`** (sigil-synthesis grand-success rate), `gem_mix_ticket`…
- Weapon / Limit / Skill / Character / AP-tree stat tables.

Directly useful for your targets:
- ✅ **Curios / sigil RNG** — `gem_mix_success`, `gem_rare`, `item_junk_appear_rate` are
  *already exposed*. "Make rare sigils less rare" is a live edit today (once 2.0-updated).
- ❌ **Drop tables (`reward_lot`)** and **Transmarvel gacha (`gacha_lot`)** — NOT hooked.
  Exposing them needs a new manager hook (find the Reward/Gacha manager in memory), plus
  our 2.0 headers to read them.

## Three tiers (effort vs UX), pick per appetite

### Tier 1 — Config-driven multiplier mod (ships now, no overlay)
A small C# Reloaded-II mod using the loader's `IDataManager` API (documented in
[04-code-mods.md](04-code-mods.md)): read a user config, e.g.
```ini
CurioRareSigilMultiplier = 10
TransmarvelRareTraitBoost = 5
BossDropRarityMultiplier  = 3
```
…then at load, fetch `reward_lot`/`gacha_lot`, multiply the **Weight** of rare entries
(we know the exact columns: `reward_lot.Weight @0x30`, `gacha_lot.Weight @0x10` —
[11-droprate-modding-unlocked.md](11-droprate-modding-unlocked.md)), and hand them back.
Digestible (one ini, plain-language knobs), no SQLite, no overlay. Restart to apply.
**Buildable immediately with what we already have.** Also shippable as fixed **preset mods**
("Curios: rare 5× / 10× / guaranteed") for one-click users.

### Tier 2 — Revive the overlay on 2.0 (the real-time dream)
Fork/update `gbfr.utility.modtools`: re-find its manager/reflection byte-patterns for the
2.0 exe (the same signature-hunting we did for GBFRelinkFix) and switch its schema version
to 2.0 (**our headers**). That immediately gives an **in-game, hideable, real-time editor**
for the sigil/curio RNG tables it already exposes (`gem_mix_success`, `gem_rare`,
`item_junk_appear_rate`). Then add `reward_lot`/`gacha_lot` manager hooks to bring drop
tables + Transmarvel into the overlay. High-leverage: the framework exists and we own the
blocked piece.

### Tier 3 — A focused, friendly RNG panel
Instead of a raw table grid, a purpose-built overlay window with plain sliders — "Curio
rare-sigil chance", "Transmarvel rare-trait boost", "Boss drop rarity" — each mapping to
the underlying weights. Best UX, most bespoke work; natural follow-on to Tier 2.

## Recommendation
Start **Tier 1** (fast, shippable, digestible, proves the rarity math on the exact tables
you care about), and in parallel scope **Tier 2** (updating the community's real-time
overlay for 2.0 — our headers are the unlock, so it's high-value and partly done). Tier 3
is the polish once the plumbing works.

## Honest caveats
- Live memory editing changes the *loaded* tables; whether a given roll re-reads them each
  time (instant effect) vs caches them (needs a re-open of the menu/quest) is per-system —
  verify per table.
- Reviving the overlay = a real C# + pattern-hunting project; it will break again on each
  game patch (recompile), like all code mods.
- Anything touching gacha/economy stays single-player/offline (etiquette + our earlier notes).
