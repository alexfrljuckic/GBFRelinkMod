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

## 🔬 In design (validate before building)
- **Span the main-menu background to ultrawide** — feasible; master named the element
  (`Main menu bg = 2384707215`) and its 2.0 hook site still matches
  (`+0x3340b04`). Diagnostic staged (`design/GBFRelinkFix-diag-backgrounds.asi`) — run it
  with the main menu open to confirm the ID + 2.0 offsets, then port the targeted span.
  ([docs/14](docs/14-menu-background-spanning.md))
- **RNG / Drop-rate tuner mod** — make Transmarvel/curio/boss RNG less punishing.
  - Decided: **must be a Reloaded-II mod** (the platform the scene uses).
  - Decided: **goodness-based**, not rarity-based — boost the buckets/items on the
    curated good list (docs/15), don't blanket-boost `Unk4=1`.
  - Open: which tier — (1) config-multiplier mod, (2) revive the in-game ImGui overlay,
    (3) friendly slider panel. ([docs/12](docs/12-realtime-rng-ux-design.md))
  - Next: **UI/UX mocks** in `design/` to validate what data/controls we need.
  - Before publishing: verify live that FORGING_HIGH → group `27509C51` mapping holds
    (community 1.x lore contradicts the tables on War Elemental via Transmarvel).

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
