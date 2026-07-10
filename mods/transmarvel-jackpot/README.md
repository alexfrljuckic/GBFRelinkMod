# Transmarvel Jackpot (2.0)

> ⚠️ **HOBBY PROJECT** — not maintained per game patch, WILL break on game updates.
> For maintained mods, use [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink).

Makes every **Transmarvel** roll (FORGING_HIGH at the Yorozu workshop) pay out:

- **Sigil side (75%)**: always one of the **13 chase V+ sigils, equal odds (~7.7% each)**:
  War Elemental+, Supplementary Damage V+, Berserker Echo+, Spartan Echo+,
  Greater Aegis V+, Celestial Nyx/Lumen/Terra/Incendo/Aqua/Ventus V+,
  Fatebreaker V+, Divergence V+.
- **Wrightstone side (25%)**: always a **tier-3 "Transmarveled" wrightstone**
  (`ITEM_25..28_0132` — previously a 0.1% outcome).

**Curios (FORGING_01–03) are untouched** — regular knickknack transmutation stays vanilla.
The 75/25 gem-vs-wrightstone split is also unchanged.

## How it works

One file: `gacha_rate_group.tbl` (layout unchanged in 2.0). In the two Transmarvel
rate groups (`27509C51` gem / `67716D8A` wrightstone), all outcome buckets are
zero-weighted except the chase buckets (`6E52A69A` w=5000 + `36879ED7` w=8000 —
proportional to their 5/8 item counts so all 13 sigils are equal; `BD1CBF1C` w=10000).
Vanilla already uses Weight=0 rows, so zero = never. Per-item odds inside a bucket were
already uniform (`gacha_lot.Weight` = 50 everywhere). Verified: byte-exact round-trip;
edit touches exactly the 12 weight fields (23 bytes).

Full pool decode + methodology: [docs/15](../../docs/15-transmarvel-pool-decoded.md).

## Install

Requires [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) +
[GBFR Mod Manager](https://github.com/WistfulHopes/gbfrelink.utility.manager) (2.0 version).

1. Copy `gbfr.transmarvel.jackpot/` into `Reloaded-II\Mods\`.
2. Enable **Transmarvel Jackpot (2.0)** in the game's mod list.
3. ⚠️ Disable any other mod that ships `gacha_rate_group.tbl` or `gacha_lot.tbl`
   (e.g. "Transmarvel Wrightstone Max Level" — 1.x layout, broken on 2.0 anyway).

## Rebuild from source

```bash
GBFRDataTools.exe extract -i <game>\data.i -f system/table/gacha_rate_group.tbl -o work
GBFRDataTools.exe tbl-to-sqlite -i work/system/table -o grg.sqlite -v 2.0.2
# apply the UPDATEs documented above (see docs/15), then:
GBFRDataTools.exe sqlite-to-tbl -i grg.sqlite -o out -v 2.0.2
```
