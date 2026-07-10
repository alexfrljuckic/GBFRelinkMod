# GBFRelinkMod — Granblue Fantasy: Relink mods & modding research

Mods and modding research for **Granblue Fantasy: Relink – Endless Ragnarok (game v2.0)**,
started 2026-07-09 (one day after the expansion launched and broke much of the mod scene).
Two things live here: **ready-to-install mods** (`mods/`) and the **research** behind them
(`docs/`).

> ## ⚠️ Hobby project — not actively maintained
> This is a personal, for-fun project. It targets **one game version (2.0)** and **will
> break when the game updates** — I don't plan to chase every patch. Treat anything here as
> a snapshot that may stop working at any time. For maintained, up-to-date mods, use
> **[Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink)** — official support
> for Endless Ragnarok is growing there. Use at your own risk; back up your saves.

---

## Mods (each is self-contained with its own install guide)

| Mod | Folder | What it does | Status |
|---|---|---|---|
| **Transmarvel Overhaul** | [mods/transmarvel-overhaul/](mods/transmarvel-overhaul/) | Transmarvel worth doing: sigil rolls come from a 41-chase-sigil jackpot pool, + sigils get curated 2nd traits, wrightstones always tier-3, and every Chaos+ clear guarantees vouchers | ✅ Released + live-verified |
| **Mastery Points Multiplier** | [mods/msp-multiplier/](mods/msp-multiplier/) | Configurable ×1–×100 MSP from quests (default ×5) — our first C# Reloaded mod | ✅ Released + live-verified |
| **Item Cap 9999** | [mods/item-cap/](mods/item-cap/) | Raises the 999 hold-cap to 9999 on normal item categories (key items/currencies untouched) | ✅ Released + live-verified |
| **Instant Loot** | [mods/instant-loot/](mods/instant-loot/) | Instantly finishes the end-of-quest treasure countdown (auto-collect) and skips the result-screen ceremony — two toggles. Code/AOB mod | ✅ Released |

Downloads (binaries) are attached to **[GitHub Releases](https://github.com/alexfrljuckic/GBFRelinkMod/releases)**
under per-mod tags (e.g. `transmarvel-overhaul-v1`). Each mod folder's `README.md` has full install steps.

> **Ultrawide?** We shipped a 2.0 ultrawide fix (`ultrawide-v1`) and then spent a day
> pushing spanned-UI further than any existing mod — the research is in
> [docs/17](docs/17-ui-spanning-handoff.md)/[docs/18](docs/18-ui-spanning-session2.md) —
> but for day-to-day play we now recommend and use
> **[zhen469891's GBFRUltrawide](https://github.com/zhen469891/gbfr-ultrawide)** (MIT,
> actively maintained for 2.0.2). Our fork's source stays available for the curious.

---

## First time? Install the toolchain from scratch (dummy-proof)

Most GBFR mods run through **Reloaded-II** (a mod manager) plus the **GBFR Mod Manager**
(a mod-loader mod). Do this once:

1. **Install Reloaded-II.** Download `Setup.exe` from
   [Reloaded-II releases](https://github.com/Reloaded-Project/Reloaded-II/releases), run it,
   launch **Reloaded-II.exe**. Click the **`+`** → add application → select
   `granblue_fantasy_relink.exe` from your game folder
   (`…\steamapps\common\Granblue Fantasy Relink\`).
2. **Install the GBFR Mod Manager** (needed by table/QoL mods). Download
   `gbfrelink.utility.manager.7z` from
   [its releases](https://github.com/WistfulHopes/gbfrelink.utility.manager/releases)
   (get **v2.0.1+** for Endless Ragnarok). With Reloaded-II **not** run as admin, drag the
   file onto the Reloaded-II window. Tick its checkbox to enable it.
3. **Install a mod:** drag its zip onto Reloaded-II, tick it, click **Launch Application**.

Turn off telemetry while modding: in-game **Options → Other → Play Log → Do Not Agree**.

### Two kinds of mods (why install steps differ)
- **Reloaded-II mods** (all our released mods — table edits, QoL, C# mods): managed inside
  Reloaded-II — drag-drop + checkbox.
- **ASI mods** (ultrawide fixes like GBFRUltrawide): a `.asi` loaded by a proxy `winmm.dll`
  you drop in the game folder. Runs **alongside** Reloaded-II.

---

## Backlog & status
See **[BACKLOG.md](BACKLOG.md)** for what's done, in progress, and planned.

## Tools we use / built
- **[Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II)** — mod manager
- **[GBFR Mod Manager](https://github.com/WistfulHopes/gbfrelink.utility.manager)** — file-mod loader (Reloaded-II mod)
- **[GBFRDataTools](https://github.com/Nenkai/GBFRDataTools)** — extract/convert `data.i` + tables. Our **2.0 table-header fixes** that unlock drop-rate editing are in [patches/headers-2.0/](patches/headers-2.0/)
- **[gbfr.utility.modtools](https://github.com/Nenkai/gbfr.utility.modtools)** — Nenkai's in-game ImGui overlay (the real-time table editor we'd build on)
- **[Ultimate ASI Loader](https://github.com/ThirteenAG/Ultimate-ASI-Loader)** — loads `.asi` mods
- Central docs: **[nenkai.github.io/relink-modding](https://nenkai.github.io/relink-modding/)** (+ the Relink Modding Discord)

---

## Research docs (the "why" behind the mods)

| Doc | What it covers |
|---|---|
| [01-ecosystem.md](docs/01-ecosystem.md) | The modding landscape: archives, toolchain, publishing, etiquette |
| [02-endless-ragnarok-breakage.md](docs/02-endless-ragnarok-breakage.md) | Exactly what 2.0 broke, why, tool-support status |
| [03-table-modding.md](docs/03-table-modding.md) | `.tbl` → SQLite → `.tbl` workflow, porting to 2.0 |
| [04-code-mods.md](docs/04-code-mods.md) | Reloaded-II C# vs ASI/C++ pattern-scan mods |
| [05-first-projects.md](docs/05-first-projects.md) | Ranked entry points |
| [06-toolchain-setup.md](docs/06-toolchain-setup.md) | Full dev/extraction setup (done + log) |
| [07-2.0-table-compat-audit.md](docs/07-2.0-table-compat-audit.md) | **Research**: 55 of 304 tables changed in 2.0 |
| [08-gbfrelinkfix-status.md](docs/08-gbfrelinkfix-status.md) | **Research**: ultrawide fix ported to 2.0 (live-verified) |
| [09-hud-markers-debugging.md](docs/09-hud-markers-debugging.md) | **Research**: in-mod diagnostic method (beats anti-debug) |
| [10-modding-opportunities.md](docs/10-modding-opportunities.md) | Your Reloaded-II setup + per-mod 2.0 breakage |
| [11-droprate-modding-unlocked.md](docs/11-droprate-modding-unlocked.md) | **Research**: reversed `reward_lot`/`gacha_lot` → drop-rate editing unlocked |
| [12-realtime-rng-ux-design.md](docs/12-realtime-rng-ux-design.md) | In-game RNG UX feasibility + tiered plan |
| [13-rng-knob-map.md](docs/13-rng-knob-map.md) | **Research**: exact knobs for Transmarvel, curios, boss drops |
| [14-menu-background-spanning.md](docs/14-menu-background-spanning.md) | **Research**: spanning the main-menu background to ultrawide — early feasibility work |
| [15-transmarvel-pool-decoded.md](docs/15-transmarvel-pool-decoded.md) | **Research**: the full Transmarvel pool decoded with names + odds (basis of the Overhaul mod) |
| [16-quest-reward-chain.md](docs/16-quest-reward-chain.md) | **Research**: quest→reward table chain decoded (basis of the voucher feature) |
| [16-retrogawd-comparison.md](docs/16-retrogawd-comparison.md) | Honest comparison with RetroGawd's ultrawide fix |
| [17-ui-spanning-handoff.md](docs/17-ui-spanning-handoff.md) | UI-spanning research handoff: tools, struct maps, plan |
| [18-ui-spanning-session2.md](docs/18-ui-spanning-session2.md) | **Research**: the full spanned-UI campaign + post-mortem (why we ultimately adopted zhen's GBFRUltrawide) |
| [19-loot-countdown-research.md](docs/19-loot-countdown-research.md) | **Research**: loot countdown is engine code not data (basis of Instant Loot); `constant.tbl` 2.0 layout reversed |

`design/` holds UI/UX mocks · `scripts/` holds RE helper scripts · `patches/` holds our table-header fixes.
