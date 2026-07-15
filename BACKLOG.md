# Backlog

Single tracker for GBFRelinkMod. Targets **game v2.0 / Endless Ragnarok**.
Last updated 2026-07-09.

> ⚠️ Hobby project — not actively maintained per game patch. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink).

## ✅ Done
- **Toolchain up** — GBFRDataTools 1.5.1 + master filelist, .NET 10, xmake, MSVC, x64dbg,
  SQLite CLI. Game at `D:\Steam\steamapps\common\Granblue Fantasy Relink` (2.0). ([docs/06](docs/06-toolchain-setup.md))
- **2.0 table audit** — 55 of 304 tables changed layout. ([docs/07](docs/07-2.0-table-compat-audit.md))
- **Ultrawide Fix ported to 2.0** — crash fix (Press-Any-Key sig), world markers, live-verified
  at 3440×1440. Shipped in [mods/ultrawide/](mods/ultrawide/). ([docs/08](docs/08-gbfrelinkfix-status.md), [docs/09](docs/09-hud-markers-debugging.md))
- **Span HUD** — configurable, spans gameplay HUD to ultrawide edges (world markers excluded).
- **Drop-rate editing unlocked** — reversed `reward_lot`/`gacha_lot` 2.0 layouts; new headers
  round-trip byte-exact. ([docs/11](docs/11-droprate-modding-unlocked.md), [patches/headers-2.0/](patches/headers-2.0/))
- **RNG knob map** — Transmarvel/curios = the Yorozu forging system; exact tables/columns
  identified. ([docs/13](docs/13-rng-knob-map.md))
- **Transmarvel pool fully decoded with names** — every `gacha_lot` row resolved to an
  English sigil/item name + per-item odds (custom-XXHash32 port + 2.0 `text.msg`
  extraction). Key finding: **rare-flag buckets ≠ good buckets** (War Elemental+ /
  Supp Dmg V+ sit unflagged at 4.2%). Community-firsts: Transmarvel Voucher =
  `ITEM_21_0001` / `58FC9B99`; 2.0 sigil ids `GEEN_320–327`. ([docs/15](docs/15-transmarvel-pool-decoded.md))
- **Reported the ultrawide fix to Lyall** upstream (Codeberg issue #1).
- **Transmarvel Overhaul v1 — RELEASED** ([mods/transmarvel-overhaul/](mods/transmarvel-overhaul/)):
  the three Transmarvel features consolidated into ONE mod per Alex (originally shipped
  as separate jackpot/vouchers/second-traits mods via PRs #1–#4, then merged):
  1. sigil rolls = one of **41 chase sigils** (13 top V+ + all 28 character Warpath+,
     equal ~2.4%); wrightstones always tier-3 Transmarveled; jackpot v1 was
     live-verified in game;
  2. random 2nd trait on + sigils = curated 18-trait list (junk resistances gone;
     new `SKL_TMV_GOOD` sub-lot, type-lots 2/5 repointed);
  3. guaranteed vouchers per Chaos+ clear (Chaos/Chaos+ ×1, Chaos++ ×2, Infinity ×3,
     all 56 quests; quest→reward chain decoded in [docs/16](docs/16-quest-reward-chain.md)).
  `scripts/build-jackpot-tables.mjs` prunes already-owned Warpath+ (dupes worthless),
  keeping odds equal. Setup landmines from v1 bring-up: see standing notes
  (bootstrapper ASI + SAC).
- **Mastery Points Multiplier v1 — RELEASED + LIVE-VERIFIED 2026-07-10**
  ([mods/msp-multiplier/](mods/msp-multiplier/), source [mods-src/](mods-src/gbfr.quest.mspmultiplier/),
  release `msp-multiplier-v1`): our first C# Reloaded mod — patches vanilla
  `reward_point` MSP entries at launch via the Mod Manager's `IDataManager`;
  Configure-Mod multiplier 1–100 (default ×5). Verified in game (Wings of
  Conflagration ~2.3k vs ~510 vanilla ceiling). Replaced the brief static msp-x5.
- **Item Cap 9999 v1 — RELEASED + LIVE-VERIFIED 2026-07-10**
  ([mods/item-cap/](mods/item-cap/), release `item-cap-v1`): `item_category.MaxHoldable`
  999→9999 on the ten normal categories (key items/currencies/consumable caps
  untouched — unlike the old 1.x Nexus uncap mod). Un-install clamps back to 999.
  Post-release fix: ModConfig.json JSON escape bug made Reloaded skip the mod
  (now a CLAUDE.md convention: validate every ModConfig, no heredoc JSON).

## 🔬 In design (validate before building)
- **Summon skill drop rates** (research started 2026-07-14, per Alex): 2.0's summon
  inventory decoded — full chain `reward.tbl → reward_summon → reward_summon_lot →
  summon.tbl (189 summons, 2 skill slots, rarity 3–5★) → summon_lot (weighted skill
  pools, basis points) → summon_curve (skill LEVEL distribution)`. Slot 1 rolls the
  SAME `SKILL_xxx_00` hashes as the sigil trait system (+ 9-skill legend tier at 8%,
  `summon_legend_skill.tbl`); slot 2 = 22 summon-exclusive ids (unresolved). Mod
  knobs identified: summon_lot weights (skill filter/boost), summon_curve (level
  floor/Lv15 boost), reward_summon_lot + Chance (which summon, how often). Same
  IDataManager launch-rebuild architecture as transmarvel. ([docs/23](docs/23-summon-skill-drops.md))
  UPDATE later that session: `text_sum.msg` extracted → `extracted/summon-names-en.tsv`;
  summons are enemy-based (77 So-ids == summon_info rows; names like Lucilius, Proto
  Bahamut, Goblin Gladiator); `summon_info@0x30` = name-text hash; slot-2 skills =
  summon CALL EFFECTS (`TXT_SMN_BDY_So####`: potions/Regen/Stout Heart/crit gauge…).
  Still open: summon.tbl↔So-id link (per-summon config names), live confirm skills
  roll at drop time.
- **Complete open-source UI spanning** — **CONCLUDED 2026-07-10 (final): ALL ultrawide
  mods UNINSTALLED — game runs vanilla rendering** (both builds parked in
  `save-backups/parked-ultrawide-mods/`; a persistent town-bubble offset survived every
  mod configuration INCLUDING [zhen's GBFRUltrawide](https://github.com/zhen469891/gbfr-ultrawide)
  v0.2.1, so Alex pulled everything — first vanilla launch should confirm whether the
  bubbles were ever a mod bug at all; NOTE the mid-day tests were contaminated by
  Ultimate ASI Loader loading .asi from scripts SUBFOLDERS, i.e. "parked" mods co-ran).
  Most features verified at 3440x1440 (backgrounds/
  menus/loading spanned, combat VFX via ScreenEffects, lock-on/dodge, bubbles), but
  spanned-UI died on a shared-layer conflict in the quest-flow screens (same layer needs
  opposite treatment on menus vs quest board — per-id exclusion can't reconcile; a
  resume needs screen-context awareness). Post-mortem atop
  [docs/18](docs/18-ui-spanning-session2.md); source: vendor `ragnarok-2.0-fixes`
  (`5fce26f`). Session history: **2026-07-10: menu/title/load BACKGROUNDS
  SOLVED + live-verified** (span-at-the-readers, vendor `ce8aae4`; Claude ran the game
  autonomously at 2.4:1 windowed — save hash-verified untouched). Remaining before an
  ultrawide-v2 release: loading-tips bg (unhooked reader form), lock-on/dodge `+0x150`
  shift (needs a combat run on real ultrawide), screen effects, nameplates. Older notes
  ([docs/17](docs/17-ui-spanning-handoff.md)):
  - Built a real **zydis disassembler** (`tools/disasm/disasm.exe`, src `tools-src/disasm/`) —
    read game code precisely, no more guessing.
  - Confirmed 2.0 struct: object-ID @ `rax+0x1C4`, width/height @ `0x1BC/0x1C0`, master's
    background IDs unchanged. Disassembled the UI-Backgrounds fn — the width→rect math checks
    out but **that function isn't the visible background** (register-write fires, pixels don't
    move). The real bg/effects/nameplates live in **multiple distinct sites** (matches
    RetroGawd's 4+ hooks).
  - **Next-session order**: (1) Lock-On/Dodge (near-direct port of master's offset formula —
    quick win, do with `[Span HUD]=false`); (2) find the REAL background function; (3) screen
    effects; (4) nameplates. Rule: verify each with a screenshot (writes can fire without
    moving pixels). Build the mod ourselves; use RetroGawd's symbols only as a feature map,
    never lift his code. ([docs/16](docs/16-retrogawd-comparison.md), [docs/14](docs/14-menu-background-spanning.md))
- ~~RNG / Drop-rate tuner mod (sliders/overlay)~~ — SUPERSEDED 2026-07-09: Alex decided
  no sliders needed; split into two simple data mods instead (Transmarvel Jackpot above +
  Voucher-per-Chaos-quest). docs/12's overlay research stays shelved for later.

## ✅ More done (2026-07-10 session 3)
- **Mastery Points Multiplier** (C# Reloaded, config ×1–100) — RELEASED, live-verified.
- **Item Cap 9999** — RELEASED, live-verified. **Instant Loot** (C# AOB code mod:
  auto-loot chest + skip result screen) — RELEASED (fixed an instant-crash: scan .text
  only, not the whole module). **Transmarvel Overhaul** consolidated (jackpot + traits +
  vouchers in one). 6-mod cleanup of stale/broken Reloaded mods.
- **Badges & Spellbooks for Vouchers** ([mods/badge-shop/](mods/badge-shop/)) — RELEASED
  + live-verified: unlimited Silver/Gold Dalia Badge + Gold Spellbook in the Treasure
  Trade tab for Knickknack Vouchers. Decoded the whole shop cost system (docs/20).
- **Save-file sigil inventory + per-copy traits decoded** ([docs/21](docs/21-save-sigil-inventory.md)) —
  `build-jackpot-tables.mjs` now auto-detects from `SaveData<N>.dat` (read-only, loud
  sanity checks) and prunes per **combo** (per Alex: Warpath+DMG Cap ≠ Warpath+Cascade):
  a Warpath+ leaves the pool only when all 18 curated secondaries are owned for it.
  Key decode: trait pool seq = instance×100+slot; innate SKILL number == sigil GEEN
  number (2927/2928 cross-validation). Also fixed: stale GBFRDataTools ids.txt made
  `GEEN_173_93+` export as raw hashes, silently surviving the dedup DELETE; and
  archive-locked-while-playing now falls back to the `extracted/2.0/` vanilla cache.
  Alex's current coverage: best 3/18 → nothing prunable yet, pool stays 41
  (repo-tooling change only — released zip unaffected, no release action).
- **Transmarvel Overhaul: 2nd-trait filter leak — escalated to content-level fix** —
  Alex reported junk secondaries still rolling. Investigation trail (new
  [scripts/audit-save-sigil-traits.mjs](scripts/audit-save-sigil-traits.mjs) + Reloaded
  logs): (1) v1's reroute of gem-referenced type-lots 2/3/4/5 never worked in game —
  fresh pulls rolled junk matching **skill_type_lot 14**'s five sub-lot families
  (lot 14 is referenced by the grant path, not by any `gem` row); (2) rerouting lot 14
  → `SKL_TMV_GOOD` was staged (log-verified 22:36 launch) and **junk STILL rolled** —
  so the grant path doesn't select trait lots via the skill_type_lot keys we assumed.
  Final fix at the altitude that can't miss: **cleansed skill_lot.tbl contents** —
  all 314 non-curated SkillIds across the 36 vanilla sub-lots replaced with curated-18
  traits (per-sub-lot round-robin; keys/weights/row-count untouched, byte-diff
  verified: 314 SkillId dwords + the 18 appended rows). Whatever lot the game picks,
  it can only contain curated traits. Side effect (documented): anything else rolling
  from these sub-lots (wrightstone traits?) now also rolls curated. Type-lot reroutes
  kept for equal odds where they do apply. ⚠️ needs restart + fresh-pull audit; if
  junk STILL appears, the roll bypasses skill_lot entirely → exe disasm next. Part of
  pending v2 release.
- **Transmarvel Overhaul 2.0-dev9: auto-prune baked into the dll + legal-combo
  library** (overnight session per Alex; full evaluation in
  [docs/22](docs/22-config-vs-programmatic.md)) — answer to "is config too
  restricting?": hybrid. Config = preferences only (trait/sigil taste, voucher knobs);
  mechanics = programmatic: legality already derived from vanilla tables (dev6), and
  now **combo-completion pruning runs in the dll at every launch** via new
  `SaveReader.cs` (C# port of the docs/21 save parser; `AutoPruneCompleted` default
  on, fail-open with console log). Generated
  [legal-combos.json](mods/transmarvel-overhaul/legal-combos.json) /
  [LEGAL-COMBOS.md](mods/transmarvel-overhaul/LEGAL-COMBOS.md): 41 mains × 62 legal
  secondaries = 2,542 combos with vanilla odds. Tests: C# vs JS differential
  byte-identical on 2 save fixtures; wrong-file + truncated-save refuse cleanly;
  auto-prune simulation respects user unticks, prunes 0 (correct for current save).
  New harness [tools-src/savereader-test/](tools-src/savereader-test/).
  `build-jackpot-tables.mjs` now redundant as pruner (kept for inspection).
- **Transmarvel Overhaul 3.0-dev1: game-mirrored config + any skill as a main**
  (2026-07-14, per Alex — "organize like the game; two toggles per skill"). Config
  reorganized into the game's own skill categories (**1. Basic Stats / 2. Attack /
  3. Defense / 4. Support / 5. Special**, order included — decoded from `skill.tbl`:
  GemCategory u32 @ row offset 92, SortOrder @ 100, 112-byte 2.0 rows); every skill
  now has **(primary sigil)** + **(2nd trait)** toggles. NEW capability: 73 ordinary
  skills' V+ sigils (vanilla `GEEN_<fam>_24` dual-slot forms — same form as War
  Elemental+; `_14` = the single-slot stat V+) can join the pool as mains —
  `ApplySigilPool` appends cloned Chase-V+-bucket rows; combo auto-prune applies to
  them like Warpath+. Catalog pipeline now in-repo:
  `scripts/gen-unified-skill-catalog.mjs` → [skill-catalog.json](mods/transmarvel-overhaul/skill-catalog.json)
  → `scripts/gen-transmarvel-code.mjs` → Config/SigilCatalog/TraitCatalog.cs
  (146 primary + 72 secondary toggles; old prop names kept so user configs survive).
  Defaults unchanged from v2.2. Installed as 3.0-dev1. ⚠️ needs a live pull test of
  an added V+ main (e.g. tick Aegis V+) before release as v3.
- **Transmarvel Overhaul v3.0 — RELEASED**
  ([release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/transmarvel-overhaul-v3.0)):
  the dev1 (game-mirrored two-toggle config + any skill as a main) + dev2 (forced
  trait-filter coverage) work below, shipped after Alex's live verification
  (12-trait config, full coverage, added V+ mains dropping correctly).
- **Transmarvel Overhaul 3.0-dev2: trait filter reevaluated after live evidence**
  (2026-07-14, from Alex's "majority supp damage + random stamina" report). Alex's
  live config (only SupplementaryDmg ticked as 2nd trait + 21 primaries) exposed two
  things: (1) the 10 sub-lots left VANILLA (no legal ticked trait) leaked unticked
  junk (Stamina) — now closed with a third **forced** remap tier (ticked set written
  even where not vanilla-legal → full coverage, only ticked traits can EVER roll);
  (2) **the "vanilla-legal combos only" guarantee is empirically dead** — save audit
  showed SuppDmg (not lot-14-legal) landing on Celestial/Fatebreaker/added-V+ mains
  via the relaxed tier, i.e. the engine consumes lot content regardless of the
  reference model. RollableSecondaries simplified to = ticked (auto-prune no longer
  self-disables on non-lot-14 tick sets). Config descriptions + README guarantees
  rewritten; generator now escapes C# strings (a literal quote in the new text broke
  Config.cs — caught before build). Also live-confirmed from the same save audit:
  **3.0-dev1 mod-added V+ mains DROP CORRECTLY** (Damage Cap V+, Precise Wrath V+,
  Quick Cooldown V+, Cascade V+ etc. rolled with secondaries) — the v3 release gate
  test is done. Side note for Alex's config: his old curated trait ticks are gone
  (only SuppDmg ticked); re-tick desired "(2nd trait)" boxes. Installed as 3.0-dev2
  (watcher pending game exit).
- **Transmarvel Overhaul v2.2 — RELEASED**
  ([release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/transmarvel-overhaul-v2.2),
  commit `f29e08f`): wrightstone drops knob. `item_pendulum.tbl` decoded (wrightstone
  = "pendulum"; per-item FIXED Main/Sub skills + level lots; 74 defs). Key find: tier
  `_0131` (gacha lot `D2CCD4EC`) = **guaranteed Lv20 main** (level lot 17 = 100% Lv20)
  + TWO RANDOM subs Lv15/Lv10 rolled through the trait lots THE MOD ALREADY FILTERS →
  random subs come from ticked traits. `WrightstoneDrops`: 0 = fixed Aegis15/ATK10
  (`_0132`, default), 1 = random subs (`_0131`, Alex's setting, live-verified), 2 =
  50/50. Explains the "identical stones" mystery: `_0132` defs are fully deterministic.
- **Transmarvel Overhaul v2.1 — RELEASED**
  ([release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/transmarvel-overhaul-v2.1)):
  Awakening+ bucket (per Alex) — vanilla bucket `5AD4ADAD` decoded: 28 character
  Awakening+ (`GEEN_1xx_90`, incl. Fearless/Versalis "Soul+" names) + 4 stat-V+
  singles (surfaced honestly as "bucket extras" checkboxes). OPT-IN (default off →
  v2 defaults unchanged; Alex's config: 28 awakening ticked). Auto-prune per-bucket
  rules: Warpath+ = combo-complete, **Awakening+ = own-any** (dupes useless).
  Simulated on Alex's save: 21/28 owned → pruned instantly, 7 join the chase.
  Catalogs/docs regenerated (73 mains × 62 = 4,526 legal combos).
- **Transmarvel Overhaul v2 — RELEASED**
  ([release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/transmarvel-overhaul-v2),
  commit `fb6e2ef`): fully configurable (41 sigil + 72 trait checkboxes, voucher
  count/tier, cost knob), combo-legality enforced, auto-prune via save reading, no
  static tables (all generated at launch). LIVE-VERIFIED with an 80-pull test:
  67/67 within ticked sets, ZERO junk (under the old 33% leak that's ~(2/3)^67 ≈
  10⁻¹²). README/INSTALL rewritten for the public; player docs DROP-TABLES.md +
  LEGAL-COMBOS.md generated from game data.
- **Transmarvel Overhaul 2.0-dev11: TRAIT JUNK ROOT CAUSE FOUND & FIXED** — 30-pull
  snapshot-diff test (first clean live sample): pool 20/20 within ticked 13 ✓;
  traits 13/20 ticked + 7 junk, junk = EXCLUSIVELY the defense/resist family →
  traced to sub-lot `8F952AC1`, the ONE bucket strict legality left vanilla; every
  observed junk trait survives post-remap ONLY there (docs/13 third correction).
  Pre-rolled-stock theory dead; content filtering PROVEN to drive the roll. Fix:
  relaxed legality fallback (ticked ∩ union of referencing lots' unions) when the
  strict intersection is empty — still vanilla-legal per combo. Expect 0 junk next
  batch; if any appears, model's wrong again.
- **Transmarvel Overhaul dev10: voucher grant clamp SOLVED = 5/grant** — two live
  data points (single lot AmountGiven=99 → 5; five ≤20-chunk lots on a slot-starved
  Extreme row → 5) pin it: **the engine clamps every reward-lot item grant to 5**,
  and rows vary in free slots (207/302 rows slot-starved). dev10 chunks at ≤5 → real
  ceiling 25/clear on empty rows; VouchersPerClear description documents the limit.
  **Bulk-roll lever is the cost knob instead**: TransmarvelCost=1 → every curio
  transmute funds 1–15 rolls (Alex's config set: cost=1, vouchers=25). Also dev9
  auto-prune got its first LIVE verification this launch ("read 2289 sigils; no
  Warpath+ combo-complete"). dev10 dll install pending game close (background watcher).
- **Transmarvel Overhaul 2.0-dev8: voucher difficulty floor** — quest-board key scheme
  decoded: band `0040T3xx`, T = tier nibble 1–B (1 = PWR 200 starter board … 8 = Chaos,
  9/A/B = Chaos+/++/Infinity). New Configure Mod setting `VoucherMinTier` (1–8,
  default 8 = old Chaos-only behavior); Alex set to 1 + VouchersPerClear 99 for fast
  farming (99 vouchers per trivial PWR-200 clear).
- **Transmarvel Overhaul 2.0-dev7: Transmarvel cost knob** — `gacha.tbl` decoded (48B
  rows; Transmarvel tier = row with GemRateGroup `27509C51`; TransmarvelCost @+0x24,
  vanilla 150) → new Configure Mod setting. ⚠️ **GOTCHA: cost=0 CRASHES the
  Transmarvel menu** (live-confirmed; presumably rolls-available = points/cost
  div-by-zero) — the mod now clamps to min 1. Also: the Reloaded launcher SILENTLY
  DROPS unknown config keys when saving Configure Mod from a stale (pre-update)
  session — restart the launcher after installing a dll with new config fields.
  Wide-test route instead: VouchersPerClear=99. **Live-verified from Alex's 10-pull video (dev6)**:
  sigil pool 9/9 within his ticked 14, wrightstone tier-3 traits ✓; 2nd traits 6/9
  ticked + 3 junk that are IMPOSSIBLE from the registered tables → leading theory:
  the game PRE-ROLLS results into the "Transmarvel Stock" (junk = stale pre-rolls
  from older builds); wide free-roll test will confirm (junk should drain to zero).
- **Transmarvel Overhaul 2.0-dev6: sigil-pool checkboxes + combo legality** — per
  Alex: pick the primaries too, and never generate combos vanilla couldn't. Configure
  Mod now has 41 sigil checkboxes (pool rebuilt from vanilla gacha tables at launch,
  `Mod.ApplySigilPool`; layouts: rate rows 16B [Group,LotId,Weight,Flag], lot rows 28B
  [QMin,QMax,Key,ItemId,Weight,TraitLevel,Unk]) + 72 trait checkboxes with **legality
  enforcement**: per sub-lot, remap targets = ticked ∩ (∩ vanilla unions of referencing
  type-lots). Key decode: lot-14 union = 62 traits = what Transmarvel pulls can roll;
  10 supreme traits (Supplementary DMG, Glass Cannon, …) are curio/drop-path-only;
  Perfect Dodge is in NO vanilla lot → dropped (was dev5's experimental inject).
  Defaults: curated 11 — simulation-verified to cover all 5 Transmarvel sub-lots
  (one shared sub-lot `8F952AC1` on lots 5/6/15/16/26/27 stays vanilla under defaults,
  logged at launch). Mod ships NO static tables now. `build-jackpot-tables.mjs`
  repurposed: save → unticks combo-complete Warpath+ in the Reloaded Config.json
  (denominator = ticked ∩ transmarvel-legal). New `snapshot-save.mjs` +
  `audit --since <snapshot>` (multiset diff) because **instance ids get recycled
  after selling** — seq-based recency is unreliable. ⚠️ STILL no live-verified pull
  under ANY table-level trait filter; Alex's "Eternal Rage + HP" report from the
  dev4-cleanse session suggests the roll may not read staged skill_lot at all —
  next failed verification = exe disasm (docs/13).
- **Transmarvel Overhaul: per-trait checkboxes in Configure Mod (2.0-dev5)** — the
  2nd-trait filter moved into the C# component (`Mod.ApplyTraitFilter`): rebuilds the
  skill tables from vanilla at every launch with only the ticked traits (73 checkboxes
  — all 72 vanilla-rollable + experimental Super Ultimate Perfect Dodge `SKILL_235_00`,
  which vanilla never rolls; ⚠️ injection unverified in game). Defaults = curated 12
  with Regen→off, Perfect Dodge→on per Alex. Static skill tables removed from the mod
  (generated now); `build-trait-filter.mjs` superseded same-day; catalog generated into
  `TraitCatalog.cs` + `mods/transmarvel-overhaul/trait-catalog.json`; jackpot/audit
  scripts read the LIVE selection (catalog defaults + `Reloaded-II\User\Mods\...\Config.json`).
  ⚠️ trait filter still needs its first live-verified pull (restart + audit).
- **Transmarvel Overhaul: curated trait list trimmed 18 → 12** (per Alex 2026-07-10:
  dropped Crit Hit DMG, Weak Point DMG, Overdrive/Break Assassin, Skilled Assault,
  Injury to Insult → 4 offense / 4 sustain / 4 utility at ~8.33% each). Trait-filter
  tables now built by the new [scripts/build-trait-filter.mjs](scripts/build-trait-filter.mjs)
  (rebuilds both skill tables from vanilla — the filter finally has a proper builder;
  355 remapped entries, +12 SKL_TMV_GOOD rows). CURATED list must stay in sync across
  build-trait-filter / build-jackpot-tables / audit-save-sigil-traits (all note it).
  Installed as 2.0-dev4. [DROP-TABLES.md](mods/transmarvel-overhaul/DROP-TABLES.md)
  added — player-facing reference of the 41-sigil pool + curated secondaries.
- **Transmarvel Overhaul: configurable voucher income (×10 default)** — voucher drops
  moved from static `reward.tbl`/`reward_lot.tbl` (fixed 1/1/2/3) to a C# component
  ([mods-src/gbfr.transmarvel.overhaul/](mods-src/gbfr.transmarvel.overhaul/), msp-multiplier
  pattern): patches vanilla tables at load via IDataManager, count set in Reloaded-II
  Configure Mod (0–99, 0 = off), Chaos+ quests enumerated live from
  `quest_baseinfo_ex_data.tbl`. Node-simulated: patches the exact 112 rows/slots v1 did.
  Built + installed locally; ⚠️ NOT yet live-verified in game, NOT yet released as v2
  (needs tag `transmarvel-overhaul-v2` + zip + comparative notes when shipped).
  `quest_baseinfo_ex_data`. All byte-identical round-trips; headers in
  [patches/headers-2.0/](patches/headers-2.0/). Tool gotcha: `uint` columns holding
  0xFFFFFFFF overflow on export — retype them `hex_uint`. Also corrected the community
  `trade.tbl` header: its "SortOrder" column is really the per-entry stock cap.

## 📋 Planned
- **Auto-loot / skip-result mod (2.0)** — ⚠️ SUPERSEDED by the shipped **Instant Loot**
  mod (built clean-room from the CE table AOBs). Original research below kept for record.
- **Auto-loot research (historical)** — researched 2026-07-10. The Nexus mod
  ([583 "Infinite Repeat and Skip Loot"](https://www.nexusmods.com/granbluefantasyrelink/mods/583),
  PokeNomz) has exactly the features (Auto Loot Chest / Skip Reward Page / Infinite
  Repeat) but is **confirmed broken on 2.0** (retitled "pre-endless ragnarok", author
  "might fix"), closed-source, restrictive perms; its one extractable AOB has 0 hits in
  the 2.0 exe. The only working 2.0 option today:
  **[NidasBot's CE table, FearLess t=40001](https://fearlessrevolution.com/viewtopic.php?t=40001)**
  (v0.4.5, actively updated) — has "Auto Loot Quest Chest" + "Skip Result Screen".
  Vanilla has no skip (chests DO auto-grant on timer; the ceremony is what drags).
  Plan if we build our own: find the 2.0 sites with our scanner/disasm/in-mod-diagnostic
  stack (clean-room; CE table as feature map only), ship as our own Reloaded/ASI mod.
- **Cut GitHub Releases** for the Ultrawide Fix (attach `GBFRelinkFix.asi` to an
  `ultrawide-v1` tag) so the mod folder's download link is live.
- **Upstream the 2.0 table headers** (`reward_lot`, `gacha_lot`) as a PR to GBFRDataTools —
  unblocks every drop-rate mod for the community.
- **Follow up with Lyall** now that the fix is fully working (needs a fresh Codeberg token;
  revoke the old one).
- ~~Reverse `item` / `constant` table layouts~~ — DONE (both reversed 2026-07-10).
- **Re-port broken loot mods** using the unlocked headers (Endgame Rebalance Plus, etc.).

## 🧊 Backlogged (deferred on purpose)
- **Targeted Span HUD (exclude world prompts)** — the broad Span HUD also spans world
  prompts (e.g. "Quest Counter"). Fix needs a per-element discriminator. Diagnostic build
  `DiagSpanElements` is staged; plan = capture two scenes (empty vs quest-counter), diff
  the element fingerprints. Deferred by Alex 2026-07-09.
- **Full menu / background spanning** — master's `SpanAllBackgrounds`; flagged for visual
  issues. Out of scope for now.
- **Damage Cap Bypass** — an AOB/ASI code mod (the most-requested gameplay mod); recurring
  work as it breaks each patch.

## 🧰 Standing setup notes
- Reloaded-II at `C:\Reloaded-II`. Enabled: loader (`gbfrelink.utility.manager`) +
  our 4 mods (`gbfr.transmarvel.overhaul`, `gbfr.quest.mspmultiplier`,
  `gbfr.qol.itemcap9999`, `gbfr.qol.instantloot`).
- **Mod cleanup (2026-07-10)**: `Mods\` is now ONLY the loader + our 5 mods + 4
  required Reloaded libraries (`reloaded.sharedlib.hooks`, `reloaded.universal.redirector`,
  `Reloaded.Memory.SigScan.ReloadedII`, `SharedScans.Reloaded` — these show as "off"
  but are dependencies; never remove). All 13 other third-party mods (8 broken/
  superseded + 5 other disabled ones incl. still-working Perfect Overmasteries, Sigil
  Synthesis, Return Imbued Stones) moved to `C:\Reloaded-II\Mods_stale_backup\`
  (recoverable; all re-downloadable from Nexus). Broken verified by table row size
  (reward_lot 56 vs 2.0's 60, gacha_lot 24 vs 28, constant 72 vs 84).
- Anti-debug: x64dbg crashes the game on attach → use the **in-mod diagnostic** method
  ([docs/09](docs/09-hud-markers-debugging.md)).
- **Reloaded-II injection on GBFR needs `Reloaded.Mod.Loader.Bootstrapper.asi` in the
  game folder** (SteamStub DRM breaks launcher injection; AppConfig has DontInject:true).
  It's loaded by the Ultimate ASI Loader `winmm.dll`, so even plain Steam launches load
  Reloaded mods. Missing ASI = game boots silently UNMODDED (tell-tale: no session log in
  `%APPDATA%\Reloaded-Mod-Loader-II\Logs`).
- **Windows Smart App Control must be OFF** to run Reloaded (unsigned community DLLs →
  error 0x800711C7). SAC ships in silent evaluation mode on fresh Win11 installs and
  auto-flipped to enforcing on this machine 2026-07-09/10; turning it off is one-way
  (Windows Security → App & browser control). Turned OFF 2026-07-10.
