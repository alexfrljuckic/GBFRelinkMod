# Item Cap 9999 (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0**. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

Raises the **999 held-item cap to 9999** for all ten normal item categories
(materials, drops, etc.). Deliberately untouched: key items (cap 1), the two 99-cap
categories (in-quest consumables), and the rupee-scale caps — only the 999s change.

## Install
Extract **`item-cap-v1.zip`** from the
[item-cap-v1 release](https://github.com/alexfrljuckic/GBFRelinkMod/releases/tag/item-cap-v1)
into `Reloaded-II\Mods\`, enable **Item Cap 9999 (2.0)**, launch. Requires the
[GBFR Mod Manager 2.0.1+](https://www.nexusmods.com/granbluefantasyrelink/mods/526).
Troubleshooting: [Overhaul README](../transmarvel-overhaul/README.md#if-something-blows-up-troubleshooting).

> ⚠️ Disable "Uncap Item Limit & Transmarvel Stock" (Nexus, 1.x) if you have it — it ships
> the same table (plus a 1.x `constant.tbl` that is broken/dangerous on 2.0).

> ⚠️ **Un-installing warning**: if you hold more than 999 of something and then remove
> this mod, the game clamps you back to 999 — the excess is lost. Spend down first.

## How it works
One table, `item_category.tbl` (layout unchanged in 2.0): `MaxHoldable` 999→9999 on the
ten categories that use it. Byte-diff verified: exactly 10 values changed, nothing else.
(The old 1.x Nexus mod set every category to 9,999,999 — including key items and
*lowering* the rupee cap; this mod is deliberately surgical.)
