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

## 🔬 In design (validate before building)
- **Complete open-source UI spanning** — **DONE 2026-07-10, all features verified at 3440x1440** (menu backgrounds, combat screen-effects,
  speech-bubbles/nameplates, lock-on/dodge) — session log in
  [docs/18](docs/18-ui-spanning-session2.md). **2026-07-10: menu/title/load BACKGROUNDS
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
- Reloaded-II at `C:\Reloaded-II`; game profile has 4 mods enabled (loader, Perfect
  Overmasteries, Uncap Item Limit ⚠, gem_mix). Per-mod 2.0 breakage in [docs/10](docs/10-modding-opportunities.md).
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
