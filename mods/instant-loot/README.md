# Instant Loot (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0**. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Skips the end-of-quest loot ceremony. Two independently-toggleable features (both on by
default, set in **Configure Mod**):

- **Auto Loot Quest Chest** — instantly finishes the ~30-second treasure countdown so
  the game's own auto-collect grants everything the moment chests appear. (Vanilla
  already auto-grants on timer expiry; this just zeroes the wait.)
- **Skip Result Screen** — zeroes the post-quest result-page countdown so the loot/XP
  pages advance immediately instead of playing out.

## Install
Extract **`instant-loot-v1.zip`** from the
[instant-loot-v1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/instant-loot-v1)
into `Reloaded-II\Mods\`, enable **Instant Loot (2.0)**, launch. Requires
[Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II/releases) (no GBFR Mod
Manager dependency — this is a code mod, not a data mod).
Troubleshooting: [Overhaul README](../transmarvel-overhaul/README.md#if-something-blows-up-troubleshooting).

Toggle either feature in Reloaded-II → **Configure Mod** → restart the game (patches run
once, at launch). The console prints `[gbfr.qol.instantloot] Auto Loot Quest Chest:
patched …` for each active feature.

## How it works / build from source
Source: [mods-src/gbfr.qol.instantloot/](../../mods-src/gbfr.qol.instantloot/) (C#
Reloaded-II mod). At load it AOB-scans the game's `.text` for two sites and applies a
byte patch to each (with `VirtualProtect`), refusing to patch if a signature isn't
unique (game-update guard). Both sites were disassembled and confirmed
([docs/19](../../docs/19-loot-countdown-research.md)):

- **Auto Loot**: `mov rax, 0x41F0000041F00000` (two packed 30.0f chest timers) →
  overwritten with `xor eax,eax` + nops, so the following `mov [rsi+0x6F0], rax`
  stores 0.0.
- **Skip Result**: `vsubss xmm0, xmm2, xmm0` (result-screen countdown decrement) →
  overwritten with `xorps xmm0,xmm0` + nop, forcing remaining time to 0.

**Credit**: the two AOB signatures are ported from NidasBot's Endless Ragnarok
Cheat Engine table ([FearLess t=40001](https://fearlessrevolution.com/viewtopic.php?t=40001)),
used as a map of the code sites. The scan/patch implementation and packaging are ours.

Build: `dotnet build -c Release` with the .NET 9 SDK (`tools/dotnet9sdk`).
