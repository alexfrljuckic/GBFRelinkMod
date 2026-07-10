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

## 🧪 Built, needs live test
- **Transmarvel Jackpot mod v1** ([mods/transmarvel-jackpot/](mods/transmarvel-jackpot/)) —
  Transmarvel sigils always one of the 13 chase V+ (equal ~7.7%), wrightstones always
  tier-3 Transmarveled. One-table edit (`gacha_rate_group`), 23 bytes vs vanilla,
  installed + enabled in Reloaded-II. **Alex: do a few Transmarvel rolls to verify**
  (also settles the "can Transmarvel give War Elemental?" lore question).

## 🔬 In design (validate before building)
- **Voucher-per-Chaos-quest mod** (the other half of the split) — grant 1 Transmarvel
  Voucher (`ITEM_21_0001` / `58FC9B99`) from every Chaos-and-above clear.
  Unblocked findings: `reward.tbl` 2.0 header reversed (see docs/11); reward rows are
  `RW_<questId>_<slot>`; quest numbers recur across difficulty-band prefixes
  (`402xxx`/`407xxx`/`40Axxx`/`40Bxxx` — Chaos/Chaos++/Defy Infinity are the 2.0 tiers).
  Remaining: decode slot semantics (`_100` vs `_3xx` = fixed vs lottery?), pick the
  guaranteed-grant mechanism (fill an empty `RewardLotIdN` with a single-item
  100% group), enumerate Chaos+ quest ids (via `TXT_QR_*` in text_stage.msg +
  `quest_difficulty.tbl`, still unreversed).
- **Span the main-menu background to ultrawide** — feasible; master named the element
  (`Main menu bg = 2384707215`) and its 2.0 hook site still matches
  (`+0x3340b04`). Diagnostic staged (`design/GBFRelinkFix-diag-backgrounds.asi`) — run it
  with the main menu open to confirm the ID + 2.0 offsets, then port the targeted span.
  ([docs/14](docs/14-menu-background-spanning.md))
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
