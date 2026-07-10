# Transmarvel Overhaul (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on future
> updates and I won't patch it every time. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Transmarvel, fixed — one mod, three things:

1. **Jackpot sigil pool.** Every Transmarvel sigil roll is one of **41 chase sigils at
   equal ~2.4% odds**: the 13 top V+ generics (War Elemental+, Supplementary Damage V+,
   Berserker Echo+, Spartan Echo+, Greater Aegis V+, Celestial ×6 V+, Fatebreaker V+,
   Divergence V+) plus **all 28 character Warpath+ sigils** (max trait). Wrightstone
   rolls are always **tier-3 Transmarveled** (vanilla: 0.1%).
2. **No junk secondary traits.** The random 2nd trait on any "+" sigil (V+ or
   character+) rolls only from a curated 18-trait list, equal odds — 10 offense
   (DMG Cap, Tyranny, Stamina, Critical Hit DMG, Weak Point DMG, Overdrive Assassin,
   Break Assassin, Skilled Assault, Injury to Insult, Quick Charge), 4 sustain (Cascade,
   Quick Cooldown, Regen, Uplift), 4 utility (Guts, Autorevive, Potion Hoarder, Steady
   Focus). No resistances, no filler. Applies at acquisition from any source.
3. **Voucher income.** Every quest clear at **Chaos and above** grants guaranteed
   Transmarvel Vouchers: Chaos ×1, Chaos+ ×1, Chaos++ ×2, Infinity ×3 (all 56 Chaos+
   quests).

Regular curio transmutation and everything below Chaos are untouched; the 75/25
sigil-vs-wrightstone split stays vanilla.

## Install

Requires [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II/releases) +
[GBFR Mod Manager](https://www.nexusmods.com/granbluefantasyrelink/mods/526) (**2.0.1+**).
First Reloaded-II setup: [Installing Mods — relink-modding](https://nenkai.github.io/relink-modding/modding/installing_mods/).

1. Download **`transmarvel-overhaul-v1.zip`** from the
   [transmarvel-overhaul-v1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/transmarvel-overhaul-v1).
2. Extract into `Reloaded-II\Mods\` → `Mods\gbfr.transmarvel.overhaul\`.
3. Enable **Transmarvel Overhaul (2.0)** in the game's mod list; launch.

> ⚠️ Disable any other mod shipping `gacha_rate_group.tbl`, `gacha_lot.tbl`,
> `reward.tbl`, `reward_lot.tbl`, `skill_lot.tbl`, or `skill_type_lot.tbl`
> (e.g. Endgame Rebalance Plus, Transmarvel Wrightstone Max Level).

## If something blows up (troubleshooting)

- **"Oh Noes! … An Application Control policy has blocked this file (0x800711C7)"** —
  Windows **Smart App Control** is blocking Reloaded's unsigned community DLLs. Fix:
  *Windows Security → App & browser control → Smart App Control settings → Off*
  (one-way switch; regular SmartScreen stays on). Hits fresh Windows 11 installs where
  SAC silently flips itself to enforcing.
- **Mods silently don't apply** (rolls look vanilla, no Reloaded console window):
  Reloaded never injected — GBFR's Steam DRM breaks launcher injection. In Reloaded-II:
  *Edit Application → Advanced Tools & Options → Deploy ASI Loader*. With the deployed
  `Reloaded.Mod.Loader.Bootstrapper.asi` + `winmm.dll` in the game folder, mods load
  even from plain Steam launches. Tell: no new log in
  `%APPDATA%\Reloaded-Mod-Loader-II\Logs` = no injection.
- **Verify it's on**: the Reloaded console should list `Transmarvel Overhaul (2.0)` and
  the Mod Manager registering the six tables.

## Removing Warpath+ sigils you already own
Character-sigil dupes are worthless: add owned ids to `OWNED_WARPATH` in
[scripts/build-jackpot-tables.mjs](../../scripts/build-jackpot-tables.mjs) and re-run —
they leave the pool and the remaining sigils stay exactly equal (ships a trimmed
`gacha_lot.tbl` into this mod). Ids: `grep _93 extracted/geen-names-en.tsv`.

## Uninstall
Untick the mod (or delete `Mods\gbfr.transmarvel.overhaul\`). Game files on disk are
never modified — tables are injected at runtime.

## How it works
Six data tables, all byte-diff verified against vanilla 2.0:
- `gacha_rate_group.tbl` — Transmarvel groups `27509C51`/`67716D8A`: only the chase
  buckets keep weight (1250/2000/7000 ∝ 5/8/28 items = all sigils equal; wrightstone
  `BD1CBF1C` = 10000). Decode: [docs/15](../../docs/15-transmarvel-pool-decoded.md).
- `skill_lot.tbl` + `skill_type_lot.tbl` — new curated sub-lot `SKL_TMV_GOOD` (+18
  rows); type-lots 2 (generic V+) & 5 (character+) point at it 100%. Shared vanilla
  sub-lots untouched.
- `reward.tbl` + `reward_lot.tbl` — 3 appended guaranteed lots (`RWL_TMV_1/2/3` →
  Transmarvel Voucher ×1/×2/×3) wired into 112 per-clear reward slots across the 56
  Chaos+ quests. Decode: [docs/16](../../docs/16-quest-reward-chain.md).

Rebuild: `scripts/build-jackpot-tables.mjs` (pool), `scripts/build-voucher-mod.mjs`
(vouchers), 2.0 headers in [patches/headers-2.0/](../../patches/headers-2.0/).
