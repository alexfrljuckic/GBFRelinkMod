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
| **Ultrawide Fix** | [mods/ultrawide/](mods/ultrawide/) | Ultrawide/21:9, FOV, HUD, framerate (2.0 build of Lyall's GBFRelinkFix) | ✅ Working, in use |
| **RNG / Drop-rate tuner** | *planned* | Make Transmarvel/curio/boss RNG less punishing | 🔬 Design ([docs/12](docs/12-realtime-rng-ux-design.md), [mock](design/)) |

Downloads (binaries) are attached to **[GitHub Releases](https://github.com/alexfrljuckic/GBFRelinkMod/releases)**
under per-mod tags (e.g. `ultrawide-v1`). Each mod folder's `README.md` has full install steps.

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
- **Reloaded-II mods** (table edits, QoL, C# overlays): managed inside Reloaded-II —
  drag-drop + checkbox. This includes the planned RNG tuner.
- **ASI mods** (the Ultrawide Fix): a `.asi` loaded by a proxy `winmm.dll` you drop in the
  game folder. Runs **alongside** Reloaded-II — see [mods/ultrawide/](mods/ultrawide/).

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

`design/` holds UI/UX mocks · `scripts/` holds RE helper scripts · `patches/` holds our table-header fixes.
