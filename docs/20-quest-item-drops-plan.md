# Quest Item Drops (Dalia Badges + Gold Spellbook) — Plan & Findings (2026-07-10)

Goal (Alex): every below-Chaos quest drops **10 Silver + 1 Gold Dalia Badge**; every
Chaos-and-above quest drops **30 Silver + 10 Gold + 1 Gold Spellbook**. Default on,
amounts adjustable in the mod manager. Alex chose the **code-hook** route (over a
reduced table version).

## Item ids (confirmed, hashed with GBFR custom XXHash32)
| Item | id | hash |
|---|---|---|
| Silver Dalia Badge | `ITEM_14_0031` | `81BF605C` |
| Gold Dalia Badge | `ITEM_14_0032` | `5C07839A` |
| Gold Spellbook | `ITEM_11_0002` | `AD856C0C` |

Verified the reward system grants these fine — vanilla lot `CA4912BC` already gives one
Gold Dalia Badge (single-row, weight 10000).

## Why the reward-TABLE approach is blocked (measured, not guessed)
`scripts/analyze-reward-slots.mjs` — each guaranteed item needs its own `RewardLotIdN`
slot (multi-row lots are weighted lotteries, not multi-grants), rows cap at 6 slots:

| Tier / row | free slots |
|---|---|
| **Chaos+ every-clear (`_101`)** | **exactly 2** on all 56 (need 4: voucher+silver+gold+spellbook) |
| Chaos+ first-clear (`_100`) | 4–6 (but first-clear only, not farmable) |
| below-Chaos `_101` | mostly **1** free; and **412 of 520** below-Chaos quests have **no `_101` row at all** |

→ Can't add silver+gold to every quest, nor 4 drops to Chaos+ every-clear. Table route
is out for this ask.

## Code-hook plan
A C# Reloaded mod that, on quest completion, grants the configured items by difficulty —
no slot limit, config-adjustable. Two functions to locate (the RE work):

1. **Quest-complete / reward-grant trigger** — where the game finalizes a quest clear.
   Entry points mapped from NidasBot's CE table (t=40001, saved at
   `extracted/GFR_Public (v0.4.5).CT`), located in the 2.0 exe:
   - Instant Shrouded Treasure → RVA `0x32f5b8b` (reward/treasure neighborhood; calls a
     hash-lookup `0x140A8A9E0` with `r8d=0x97FDF6CA`, iterates a list).
   - Auto Complete Side Quests (NBGFR25A/B), Auto Loot Chest (`.text+0x1fd0f3b`, already
     used by Instant Loot), Skip Result Screen — all in the quest-flow region.
2. **Inventory item-add** — the CE "Spawn Item" uses an **obfuscated** `NBLib.IF05()`
   (no address exposed), so we RE this ourselves.

### The RE is a live-diagnostic loop (needs Alex at the keyboard)
This binary is anti-debug (x64dbg crashes it), so the method is our in-mod diagnostic
(docs/09): hook a candidate site, log register/memory context to a file, Alex runs a
quest, we read the log, refine. Static analysis alone won't yield the calling
conventions on this stripped recompile. Concretely:
- **Step 1**: build a diagnostic hook at the reward-grant neighborhood that logs the
  item-id / count / inventory-ptr registers each time an item is granted on clear.
- **Step 2**: Alex clears one quest at a known difficulty; we read the dump → identify
  the item-add function signature + the "current difficulty" source.
- **Step 3**: implement the grant (call the function, or increment the inventory count)
  gated by difficulty + config; verify live.

### Alternative to evaluate in Step 1
If the inventory item-count array is easier to find than the add-function's calling
convention (the CE `[Inventory]`/`Highlighted Item` pointers are a lead), the mod can
**increment counts directly** on a quest-complete signal instead of calling the
grant function — simpler and robust.

## PLAN PIVOT (2026-07-10): exchange + drop-silver-only (Alex chose this)

Research (agent) settled it: **no native Dalia badge conversion exists**; only **two
tiers** (Silver `ITEM_14_0031`, Gold `ITEM_14_0032`) — "Copper Dalia Badge"
(`ITEM_14_0030`) is dead/unused text. BUT the exchange is moddable via `trade.tbl` —
Nexus mod #536 does 6 Silver → 1 Gold at the Knickknack Shack. And Gold badges already
**buy** Gold Spellbooks there.

So Alex's chosen design (avoids the assembly code-hook entirely):
1. **Drop only Silver badges** from quests → reward.tbl + reward_lot.tbl. Silver = **1
   slot**, which FITS: below-Chaos `_101` mostly has 1 free slot; Chaos+ `_101` has 2
   free (voucher takes 1, silver takes the other). (Caveat: the 412/520 below-Chaos
   quests with no `_101` row still can't get drops — likely non-repeatable story quests.)
2. **Silver → Gold exchange** at the Knickknack Shack → `trade.tbl` edit.
3. **Unlimited Gold Spellbook** purchase (Alex's explicit requirement) → `trade.tbl`
   (remove the spellbook entry's stock cap / make it infinitely refreshable).

`trade.tbl` = the shop table (MinQuestId/MaxQuestId, {Gem,Pendulum,Item}Purchasable,
IsRefreshable, MaxStockForRefresh, ...) — **2.0-CHANGED, still to be reversed** (blocked:
game holds `data.0` locked while running — need it closed).

Architecture (Alex's "one drop rate overhaul, usable separately"): a config-driven C#
mod owning reward.tbl + reward_lot.tbl + trade.tbl with per-feature toggles (vouchers,
silver drops, exchange, unlimited spellbook). This means **moving the voucher tables OUT
of the Transmarvel Overhaul** into this unified mod (overhaul keeps gacha_rate_group +
skill tables). Same IDataManager pattern as the MSP mod — NOT the assembly code-hook.

## SHOP APPROACH (2026-07-10) — the winning path, but with hard constraints

Pivoted again: instead of quest drops, sell badges + Gold Spellbook at the Knickknack
Shack. Reversed **`trade.tbl`** (6th 2.0 table, docs/patches). The shop cost chain:
`trade` (ItemPurchasable, ItemTierMapId, unique **SubKey**) → `item_tier_map.MaterialId1`
→ `item_material_list` (Item1..12 + ItemCount + **CoinCost**=rupies). Mod ships all 3
tables (`gbfr.shop.rupiebadges` folder, ModName "Cheap Badges & Spellbooks").

**Two hard constraints, both data-proven (scripts/trace-shop-currency.mjs,
analyze-shop-costs.mjs), both learned via live tests with Alex:**
1. **No shop charges rupies.** All 526 CoinCost>0 recipes feed upgrade/transmutation,
   never a `trade` row. So rupie pricing is impossible — CoinCost is ignored by shops
   (an item with only CoinCost shows as FREE).
2. **Free shop items get a ~200 purchase cap.** Every vanilla shop item has a cost;
   priced items at stock 0/0/0 are unlimited, free ones are capped.
Also learned live: **SubKey must be unique** (first build collided → invisible), and
**IsRefreshable=1 moves an item into the featured/rotating shop** (→ disappears from the
normal list).

**v2 (current, awaiting retest):** each entry has a trivial 1× MATERIAL cost →
unlimited like every real shop item. Cheap 1:1 chain: Silver badge = 1 common crystal
(`ITEM_10_0000`), Gold badge = 1 silver, Gold Spellbook = 1 gold; stock 0/0/0,
IsRefreshable=0, in shop category 5. Rebuild: `scripts/build-badge-shop.mjs`.
NEEDS Alex retest: do the 3 entries show, and are they now unlimited?

## Status
Phase 1 DONE (ids, table-slot proof, CE entry points, `constant.tbl` reversed docs/19).
Quest-drop + code-hook routes SHELVED for the shop approach. Badge-shop v2 deployed,
awaiting Alex's live retest. Config (adjustable costs, per-item toggles) deferred until
the shop mechanism is confirmed working.
