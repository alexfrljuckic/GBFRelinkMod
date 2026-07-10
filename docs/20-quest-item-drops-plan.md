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

## Status
Phase 1 (scoping + item ids + table-route ruled out + CE entry points located) DONE.
Phase 2 (the live-diagnostic RE loop) needs a game session with Alex. `constant.tbl`
also reversed this session (docs/19) as a side effect.
