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
- **Complete open-source UI spanning** — **CONCLUDED 2026-07-10: Alex switched to
  [zhen's GBFRUltrawide](https://github.com/zhen469891/gbfr-ultrawide) v0.2.1 for
  stability** (installed in game folder; our build parked in
  `scripts/_disabled_GBFRelinkFix/`). Most features verified at 3440x1440 (backgrounds/
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

## 📋 Planned
- **Auto-loot / skip-result mod (2.0)** — researched 2026-07-10. The Nexus mod
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
- **Reverse `item` / `constant`** table layouts (next-most-wanted changed tables).
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
