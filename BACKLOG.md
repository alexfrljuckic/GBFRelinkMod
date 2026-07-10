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
- **Reported the ultrawide fix to Lyall** upstream (Codeberg issue #1).

## 🔬 In design (validate before building)
- **RNG / Drop-rate tuner mod** — make Transmarvel/curio/boss RNG less punishing.
  - Decided: **must be a Reloaded-II mod** (the platform the scene uses).
  - Open: which tier — (1) config-multiplier mod, (2) revive the in-game ImGui overlay,
    (3) friendly slider panel. ([docs/12](docs/12-realtime-rng-ux-design.md))
  - Next: **UI/UX mocks** in `design/` to validate what data/controls we need.

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
