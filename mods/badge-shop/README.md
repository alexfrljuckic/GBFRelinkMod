# Badges & Spellbooks for Vouchers (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0**. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Adds three **unlimited** purchases to the **Treasure Trade** tab at Siero's Knickknack
Shack, paid in **Knickknack Vouchers**:

| Item | Cost |
|---|---|
| Silver Dalia Badge | 1 Knickknack Voucher |
| Gold Dalia Badge | 3 Knickknack Vouchers |
| Gold Spellbook | 5 Knickknack Vouchers |

No stock limit — buy as many as you have vouchers for. Great for stocking up on badge
currency and Gold Spellbooks without grinding Quick Quests.

## Install
Extract **`badge-shop-v1.zip`** from the
[badge-shop-v1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/badge-shop-v1)
into `Reloaded-II\Mods\`, enable **Badges & Spellbooks for Vouchers (2.0)**, launch.
Requires the [GBFR Mod Manager 2.0.1+](https://www.nexusmods.com/granbluefantasyrelink/mods/526).
The entries appear in the shack's **Treasure Trade** tab.
Troubleshooting: [Overhaul README](../transmarvel-overhaul/README.md#if-something-blows-up-troubleshooting).

> ⚠️ Conflicts: disable any other mod that ships `trade.tbl`, `item_tier_map.tbl`, or
> `item_material_list.tbl`.

## How it works
Three tables (`trade` reversed for 2.0 in this repo; the other two unchanged). The shop
cost chain is `trade` → `item_tier_map` → `item_material_list`. Per item we append: a cost
recipe (N× Knickknack Voucher), a tier-map row, and a trade row selling it in the Treasure
Trade tab (shop category 3). Each new row uses a **unique SubKey** (the shop keys entries
by it) and stock cap **`0xFFFFFFFF` = unlimited** (the `trade.tbl` column the community
header calls "SortOrder" is actually the per-entry stock cap).

Notes learned building this (see [docs/20](../../docs/20-quest-item-drops-plan.md)):
GBFR shops **can't charge rupies** (no shop uses the `CoinCost` field), and **free items
get a ~200 cap** — hence the small voucher cost, which also keeps them unlimited.

Rebuild / retune the voucher costs: edit the `costCount` values in
[scripts/build-badge-shop.mjs](../../scripts/build-badge-shop.mjs) and re-run.
