# Modding Opportunities & Your Current Setup (2026-07-09)

Research done while AFK. Three parts: (1) your installed Reloaded-II mods and which the
2.0 expansion broke, (2) what's actually moddable given the data we can extract, (3) where
the opportunities are.

## 1. Your Reloaded-II setup — per-mod 2.0 status

Reloaded-II lives at `C:\Reloaded-II`; the GBFR profile
(`Apps\granblue_fantasy_relink.exe\AppConfig.json`) has **4 mods enabled**, ~10 more
installed-but-disabled. I cross-referenced each mod's shipped `.tbl` files against our
[2.0 table audit](07-2.0-table-compat-audit.md) (55 of 304 tables changed layout).

### Enabled (4)
| Mod | Edits | 2.0 status |
|---|---|---|
| `gbfrelink.utility.manager` (loader) | — (code) | ✅ Updated for 2.0 (v2.0.1) |
| `gbfrelink.perfect.overmasteries` | `limit_bonus_meditation_weight.tbl` | ✅ **OK** — table unchanged, likely still works |
| `gbfr.gem_mix` | `gem_mix_success.tbl` | ✅ **OK** — table unchanged, likely still works |
| `gbfr.qol.uncapitemlimit` | `constant.tbl` + `item_category.tbl` | ⚠️ **RISKY** — `constant.tbl` **changed** in 2.0 (wrong schema injected); also item-uncap mods recap over-capped items to 999 on version updates (save hazard per the ER notes) |

**Takeaway:** of your enabled mods, overmasteries and gem_mix should be fine; **uncapitemlimit
is the one to watch** — it writes a table whose layout changed, and it's the category the
modding docs specifically warn about across updates. Consider disabling it until updated.

### Installed but disabled — mostly broken by 2.0
| Mod | Edits | 2.0 status |
|---|---|---|
| Endgame Rebalance Plus | `reward_lot.tbl` | ❌ **Broken** (reward_lot changed) |
| Custom Drop Rates: Behemoth | `reward_lot.tbl` | ❌ **Broken** |
| More Fortitude | `reward_lot.tbl` | ❌ **Broken** |
| Slimepede Centrum | `reward_lot.tbl` | ❌ **Broken** |
| Show Curio Tier | `item.tbl` (+ UI textures) | ❌ **Broken** (item changed) |
| Transmarvel Wrightstone Max Level | `gacha_lot.tbl` + `gacha_rate_group.tbl` | ❌ **Broken** (gacha_lot changed; gacha_rate_group OK) |
| Sigil Synthesis All Grand Success | `gem_mix_success.tbl` | ✅ OK |
| qqcurio balance | `text/en/text.json` | ✅ OK (text, not schema) |
| Skip Intros | `ui_boot_fsm`/`ui_title` `.msg` | 🔶 Probably OK (our own build also skips intros) |

**The pattern:** everything touching **`reward_lot`** (drop/loot rates), **`item`**,
**`constant`**, or **`gacha_lot`** is broken, because those are exactly the tables 2.0
re-shaped. Mods on `gem_mix_success`, `limit_bonus_meditation_weight`, `gacha_rate_group`,
and text/UI survive.

## 2. What's moddable right now (given our data)

We can extract every table from the 2.0 `data.i`, but `tbl-to-sqlite` only works on the
**249 unchanged tables** — the 55 changed ones need GBFRDataTools to ship updated
`.headers` schemas first ([audit](07-2.0-table-compat-audit.md)).

| You asked about | Where it lives | Moddable now? |
|---|---|---|
| **Drop rates** | `reward_lot.tbl` (+ `gacha_lot`) | ❌ Blocked — both changed; can't convert to SQLite until headers update |
| **Transmarvel / Wrightstone** | `gacha_lot.tbl` | ❌ Blocked — changed |
| **Damage numbers** | NOT a table — runtime calc (damage-cap is an assembly instruction) | 🔧 Code/ASI mod only (like our GBFRelinkFix work); or edit base-stat tables (`weapon_status` etc., some OK) |
| **Sigil/gem synthesis** | `gem_mix_success.tbl` | ✅ Yes — unchanged |
| **Overmastery weights** | `limit_bonus_meditation_weight.tbl` | ✅ Yes — unchanged |
| **Item/economy** | `item.tbl`, `trade.tbl` | ❌ Blocked — changed |
| **Text / UI strings** | `text/*/text.json` (msgpack) | ✅ Yes — not schema-sensitive |

So the honest picture: **the most-wanted gameplay knobs (drop rates, loot, transmarvel,
items) are exactly the blocked ones** right now. The unchanged 249 tables are open.

"Damage numbers" specifically = the **Damage Cap Bypass** everyone wants: it's an
AOB/assembly hook that nullifies the damage-ceiling instruction, not a table edit. That's
the same class of work as our GBFRelinkFix signature hunting — and it breaks on every patch
(recompile shifts the instruction), so it's a recurring update job. You now have the exact
toolchain and skills for it.

## 3. Opportunities (ranked)

1. **Wait-and-pounce on the 55-table header update.** The moment GBFRDataTools ships 2.0
   `.headers` for `reward_lot`/`item`/`gacha_lot`/`constant`, every broken drop-rate/loot/
   transmarvel mod (yours + the popular Nexus ones like Endgame Rebalance Plus, Community
   Rebalance) becomes re-portable. Being early on re-shipping a popular one = high impact.
   Watch [GBFRDataTools commits](https://github.com/Nenkai/GBFRDataTools/commits/master).
2. **Contribute the header updates yourself.** Our audit already names the exact 55 tables
   that need new schemas. Doing even a few (`reward_lot` first — it's what most loot mods
   need) unblocks the whole community *and* your own drop-rate mods. Biggest leverage.
3. **Mod the unblocked tables now.** Sigil/gem, overmastery weights, and the other 249
   OK tables are moddable today with our existing GBFRDataTools + SQLite workflow
   ([03-table-modding.md](03-table-modding.md)).
4. **A Damage Cap Bypass ASI mod.** The most-demanded gameplay mod, and squarely in the
   code-mod lane you now own (find the damage-ceiling instruction in the 2.0 exe, patch it).
   Fragile across patches, but that's the recurring value.

## Sources
- Local: `C:\Reloaded-II\Apps\granblue_fantasy_relink.exe\AppConfig.json` + each mod's
  shipped `.tbl` files, cross-referenced with [our 2.0 audit](07-2.0-table-compat-audit.md).
- [Endless Ragnarok and Mods](https://nenkai.github.io/relink-modding/modding/endless_ragnarok_and_mods/)
  (table/column-shift breakage; damage-cap is AOB, not tables).
