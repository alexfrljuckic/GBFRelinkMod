# Transmarvel Jackpot (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on future
> updates and I won't patch it every time. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Makes every **Transmarvel** roll at Siero's Yorozu workshop actually worth the vouchers:

- **Sigil rolls (75% of pulls)**: always one of the **13 chase V+ sigils, equal odds
  (~7.7% each)** — War Elemental+, Supplementary Damage V+, Berserker Echo+, Spartan
  Echo+, Greater Aegis V+, Celestial Nyx/Lumen/Terra/Incendo/Aqua/Ventus V+,
  Fatebreaker V+, Divergence V+.
- **Wrightstone rolls (25% of pulls)**: always a **tier-3 "Transmarveled" wrightstone**
  (vanilla: a 0.1% outcome).
- **Regular curio transmutation is untouched** — only Transmarvel is changed, and the
  75/25 sigil-vs-wrightstone split stays vanilla.

Live-verified on the 2.0 (Endless Ragnarok) Steam build, 2026-07-09. How the pool was
decoded (with the full odds table): [docs/15](../../docs/15-transmarvel-pool-decoded.md).

## How it loads
This is a **Reloaded-II data mod** (one game table, no code). Unlike the
[ultrawide fix](../ultrawide/) (a standalone ASI), it needs the Reloaded-II mod stack:

- [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II/releases) (the mod loader)
- [GBFR Mod Manager](https://www.nexusmods.com/granbluefantasyrelink/mods/526)
  (Reloaded mod that injects data files — make sure you have the **Endless Ragnarok**
  version, 2.0.1+)

First time using Reloaded-II? Follow the community setup guide first:
[Installing Mods — relink-modding](https://nenkai.github.io/relink-modding/modding/installing_mods/).

## Install

1. Download **`transmarvel-jackpot-v1.zip`** from the
   [transmarvel-jackpot-v1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/transmarvel-jackpot-v1).
2. Extract it into your Reloaded-II **`Mods`** folder (e.g. `C:\Reloaded-II\Mods\`), so
   you end up with:
   ```
   Reloaded-II\Mods\gbfr.transmarvel.jackpot\
     ModConfig.json
     GBFR\data\system\table\gacha_rate_group.tbl
   ```
3. Open Reloaded-II, select **Granblue Fantasy: Relink** → *Configure Mods*, and tick
   **Transmarvel Jackpot (2.0)** (the Mod Manager gets auto-enabled as its dependency).
4. Launch the game and Transmarvel away.

> ⚠️ Disable any other mod that also ships `gacha_rate_group.tbl` or `gacha_lot.tbl`
> (e.g. "Transmarvel Wrightstone Max Level" — that one is 1.x-only and broken on 2.0
> anyway).

## If something blows up (troubleshooting)

- **"Oh Noes! Failed to Load Reloaded-II … An Application Control policy has blocked
  this file (0x800711C7)"** — Windows **Smart App Control** is blocking Reloaded's
  (unsigned) community DLLs. Fix: *Windows Security → App & browser control → Smart App
  Control settings → Off*. Heads-up: turning it off is **one-way** (re-enabling requires
  reinstalling Windows); regular SmartScreen stays active either way. This hits fresh
  Windows 11 installs where SAC silently switches itself to enforcing — the mod isn't
  doing anything wrong, Windows just distrusts unsigned mod DLLs.
- **Mods silently don't apply** (game runs, rolls look vanilla, no Reloaded console
  window): Reloaded-II never injected. GBFR's Steam DRM breaks normal launcher
  injection — in Reloaded-II use *Edit Application → Advanced Tools & Options → Deploy
  ASI Loader*, which drops `Reloaded.Mod.Loader.Bootstrapper.asi` (+ an ASI loader
  `winmm.dll`) into the game folder. With those present, mods load even when launching
  straight from Steam. A quick tell: `%APPDATA%\Reloaded-Mod-Loader-II\Logs` gets a new
  log per modded launch — no new log means no injection.
- **Verify it's actually on**: the Reloaded console at launch should list
  `Transmarvel Jackpot (2.0)` and the Mod Manager registering
  `system/table/gacha_rate_group.tbl`.

## Uninstall
Untick the mod in Reloaded-II (or delete `Mods\gbfr.transmarvel.jackpot\`). Your game
files are never modified on disk — the table is injected at runtime.

## How it works / build from source
One table edit: in `gacha_rate_group.tbl`, the two Transmarvel rate groups (`27509C51`
gem / `67716D8A` wrightstone) have every outcome bucket zero-weighted except the chase
buckets (`6E52A69A` w=5000 + `36879ED7` w=8000 — proportional to their 5/8 item counts so
all 13 sigils are equal; `BD1CBF1C` w=10000). Vanilla already uses 0-weight rows, so 0 =
never. Per-item odds inside a bucket were already uniform. The edit is 23 bytes vs
vanilla, verified byte-exact both ways. Rebuild with GBFRDataTools + the 2.0 headers in
[patches/headers-2.0/](../../patches/headers-2.0/) — workflow in
[docs/11](../../docs/11-droprate-modding-unlocked.md), decode methodology in
[docs/15](../../docs/15-transmarvel-pool-decoded.md).
