# Badges, Spellbooks & Centrums for Vouchers (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0**. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Adds four **unlimited** purchases to the **Treasure Trade** tab at Siero's Knickknack
Shack, paid in **Knickknack Vouchers** — with every cost **configurable in Reloaded-II**
(right-click the mod → *Configure Mod*) since v2:

| Item | Default cost |
|---|---|
| Silver Dalia Badge | 1 Knickknack Voucher |
| Gold Dalia Badge | 3 Knickknack Vouchers |
| Gold Spellbook | 5 Knickknack Vouchers |
| **Silver Centrum** (new in v2) | 5 Knickknack Vouchers |

Set a cost to **0** to remove that item from the shop. Minimum real cost is 1 —
the game caps *free* shop items at ~200 purchases; any paid cost is unlimited.
Every change needs a **game restart** (tables are read once, at launch).

## Install
Extract the zip from the latest
[badge-shop release](https://github.com/alexfrljuckic/GBFRelinkMod/releases)
into `Reloaded-II\Mods\`, enable **Badges, Spellbooks & Centrums for Vouchers (2.0)**,
launch. Requires the [GBFR Mod Manager 2.0.1+](https://www.nexusmods.com/granbluefantasyrelink/mods/526).
The entries appear in the shack's **Treasure Trade** tab.
Troubleshooting: [Overhaul README](../transmarvel-overhaul/README.md#if-something-blows-up-troubleshooting).

> ⚠️ Conflicts: disable any other mod that ships `trade.tbl`, `item_tier_map.tbl`, or
> `item_material_list.tbl`.

## How it works

Since v2 this is a C# Reloaded-II mod (was: static tables): at every game launch it
reads the **vanilla** `trade.tbl` / `item_tier_map.tbl` / `item_material_list.tbl`
from the game archive via the GBFR Mod Manager's `IDataManager`, appends one cost
recipe + one tier-map row + one trade row per configured item, and registers the
result — nothing on disk is modified, and row-size guards refuse to patch (loudly,
in the console) if a game update changes a layout. The generated rows are
byte-identical to the live-verified v1 static tables.

The shop cost chain is `trade` → `item_tier_map` → `item_material_list`. Each trade
row uses a **unique SubKey** (the shop keys entries by it) and stock cap
**`0xFFFFFFFF` = unlimited** (the `trade.tbl` column the community header calls
"SortOrder" is actually the per-entry stock cap).

Notes learned building v1 (see [docs/20](../../docs/20-quest-item-drops-plan.md)):
GBFR shops **can't charge rupies** (no shop uses the `CoinCost` field), and **free
items get a ~200 cap** — hence the minimum cost of 1, which also keeps them unlimited.

Source: [mods-src/gbfr.shop.voucherbadges/](../../mods-src/gbfr.shop.voucherbadges/)
(build: `tools\dotnet9sdk\dotnet.exe build -c Release`). The original static-table
builder is kept for reference: [scripts/build-badge-shop.mjs](../../scripts/build-badge-shop.mjs).

## Uninstall
Untick the mod (or delete `Mods\gbfr.shop.voucherbadges\`). Game files and your save
are never modified.
