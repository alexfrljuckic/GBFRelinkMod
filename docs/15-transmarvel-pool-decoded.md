# Transmarvel Pool — Fully Decoded, With Names (2026-07-09)

We can now read the **entire Yorozu forging pool (curios + Transmarvel) with English
sigil names and exact per-item odds**, on live 2.0 data. This turns "make rare things
less rare" into "make *the good things* more common" — the two are NOT the same, and
the tables prove it (see [Rare ≠ good](#rare--good) below).

Full decoded dump: [extracted/transmarvel-pool-decoded.txt](../extracted/transmarvel-pool-decoded.txt)
(regenerate with `node scripts/decode-transmarvel-pool.mjs`).

## How names were resolved (the pipeline)

1. **English strings**: `system/table/text/en/text.msg` is MessagePack;
   [scripts/extract-text-msg.mjs](../scripts/extract-text-msg.mjs) walks `id_hash_` →
   `text_` pairs → `extracted/geen-names-en.tsv` (1,024 sigil names incl. all 2.0 ones)
   and `extracted/item-names-en.tsv`. This is **fresher than every public mapping** —
   Nenkai's `sigil_id.csv` (1,349 rows) is 1.3.x-era and has zero Endless Ragnarok sigils.
2. **Hashes**: GBFR ids are hashed with a **custom XXHash32** — seed `0x178A54A4` and
   hardcoded accumulators `0x2557311B/0x871FB76A/0x0133ECF3/0x62FC7342`
   (`GBFRDataTools.Hashing/XXHash32Custom.cs`; standard xxh32 does NOT match). Ported to
   JS inside [scripts/decode-transmarvel-pool.mjs](../scripts/decode-transmarvel-pool.mjs),
   verified against known `ids.txt` pairs (short + ≥16-byte strings).
3. **Brute force**: hashing the whole `GEEN_%03d_%02d` + `ITEM_%02d_%04d` namespaces
   resolved every hash in `gacha_lot` (988/988 rows named).

### Community-first finds
- **Transmarvel Voucher = `ITEM_21_0001`, hash `58FC9B99`** (new 2.0.2 item; not in any
  public item list yet — Nenkai's `item_id.csv` stops at `ITEM_21_0000` Knickknack
  Voucher `09370A3A`). Worth contributing upstream.
- The four 0.1%-bucket wrightstones are `ITEM_25..28_0132` ("Transmarveled" tier-3
  wrightstone variants).
- GEEN suffix scheme (confirmed against names): last digit = trait tier (0–4 → I–V);
  tens digit = family: `0x` base, `1x`/`2x` "+" variants, `3x`/`4x` "+" variants of
  utility traits (no own text entry — display name derives from base), `9x` max-trait
  character-sigil "+" forms (`_90..._93` = the four name variants), `_94` = reissued
  standalone V+ singles (`GEEN_210–223`).

## The decoded Transmarvel pool (FORGING_HIGH, 150 vouchers/roll)

**Gem side** (`gacha.tbl` GemChance 75% → group `27509C51`, total weight 10000):

| Weight | Odds | Bucket | Contents (per-item odds) |
|---|---|---|---|
| 4940 | 49.4% | `9092654F` | 20 utility/defense V+ singles: Enmity V+, Stamina V+, Damage Cap V+, Tyranny V+, Regen+/Drain+/Autorevive+/Cascade+/Quick Cooldown+… (2.47% each) |
| 1000 | 10% | `B3976B98` | 28 character sigils+ **max-trait lv15**, name-family 1 (Fearless Drive+, …) (0.36% each) |
| 1000 | 10% | `090F0E91` | 28 character sigils+ max-trait, family 2 (Fearless Spirit+, …) |
| 1000 | 10% | `F527EF32` | 28 character **Warpath+** max-trait (0.36% each) |
| 700 | 7% | `5AD4ADAD` ⚑Unk4=1 | 32 character **Awakening+** max-trait + Attack/Health/Crit/Stun V+ (0.22% each) |
| 640 | 6.4% | `81216A95` | Attack Power V+, Health V+, Crit Rate V+, Stun Power V+ (1.6% each) |
| 420 | 4.2% | `6E52A69A` | **War Elemental+ (max-trait), Supplementary Damage V+, Berserker Echo+, Spartan Echo+, Greater Aegis V+** (0.84% each) ← the community-jackpot bucket, NOT flagged rare |
| 280 | 2.8% | `36879ED7` | **2.0 Celestial V+ ×6, Fatebreaker V+, Divergence V+** (0.35% each) |
| 20 | 0.2% | `1F44C95D` ⚑Unk4=1 | 9 supreme V+ singles: Damage Cap V+, Tyranny V+, Enmity V+, Stamina V+, Health V+, Crit V+, Stun V+, Regen V+(_44), Drain V+(_44) (0.022% each) |

**Wrightstone side** (25% → group `67716D8A`): 74.9% `ITEM_25..28_0130`,
25% `_0131`, **0.1%** ⚑ `_0132` (Transmarveled tier-3 wrightstones, 0.025% each).

Curio tiers (FORGING_01/02/03) decode the same way in the full dump — notably
FORGING_03's 5%-rare bucket `07F595BE` contains **base War Elemental (max-trait) and
Supplementary Damage V at 1%/roll each**, plus 2.0 Celestial V at 1% via `4756D3C7`.

> ⚠️ Table-vs-community discrepancy: 1.x community lore says War Elemental /
> Supplementary Damage "can't come from Transmarvel, curios only". The 2.0 `gacha_lot`
> contents clearly place their "+" versions in the Transmarvel gem group. Either 2.0
> changed the pools or the lore was wrong — verify live before publishing claims.

## Rare ≠ good

The `Unk4=1` "rare flag" buckets (0.2% supremes, 7% Awakening+) are NOT where the
community-best outcomes live: **War Elemental+ / Supplementary Damage V+ sit in an
unflagged 4.2% bucket, and the new 2.0 Celestial/Fatebreaker/Divergence sigils in an
unflagged 2.8% bucket**. So the docs/13 plan of "boost Unk4=1 weights" would mostly
pump stat-stick V+ singles. A goodness-based mod instead needs **two dials**:

1. **Bucket dial** — `gacha_rate_group.Weight` per bucket, chosen by *contents* (e.g.
   boost `6E52A69A` + `36879ED7` + `1F44C95D`, shrink the 49.4% junk bucket).
2. **Item dial** — `gacha_lot.Weight` *within* a bucket (uniform 50 today; raising War
   Elemental+ to 500 inside its bucket makes it 10× the bucket's other items). This is
   the precision knob nobody's mod has used — it wasn't even readable before our headers.

Both tables round-trip byte-exact with our 2.0 headers ([docs/11](11-droprate-modding-unlocked.md)).

### Draft "good list" (community consensus, 2.0-day-2 — refine as guides mature)
- **S**: Supplementary Damage V+ (`GEEN_151_24`), War Elemental+ (`GEEN_146_24`),
  character Warpath+ (`GEEN_1xx_93`, S-tier per Game8; note 2.0.2 nerfed Guardian's
  Warpath/Phantasm's Concord/Lord's Ambition)
- **A**: 2.0 sigils — Celestial ×6 / Fatebreaker / Divergence V+ (`GEEN_320–327_24`),
  Damage Cap V+, character Awakening+ (`GEEN_1xx_90`) for mains
- **B**: Tyranny V+, remaining max-trait character sigils (families `_91`/`_92`)
- **Junk**: the 49.4% bucket's defensive singles, non-Transmarveled wrightstones

## Adding items to the game (the voucher question)

Adding a **brand-new item type** = reversing `item.tbl` (changed in 2.0, unreversed) +
text + icon + behavior — heavy, and not needed for any current goal. Granting/injecting
**existing items** has four viable routes today:

| Route | Status on 2.0 |
|---|---|
| **`reward_lot` row inserts** — make any quest drop Transmarvel Vouchers (`58FC9B99`); vouchers currently appear in NO drop table (they're transmute/level-up rewards only) | ✅ works now with our headers; pure data mod, our lane |
| `gacha.tbl` `TransmarvelCost`/`VoucherCost` → 1 (or 0) — sidesteps granting entirely | ✅ works now (table unchanged in 2.0) |
| Cheat Engine table [fearlessrevolution t=40001](https://fearlessrevolution.com/viewtopic.php?t=40001) (new ER thread) — direct inventory count edit | ✅ per thread existence; manual tool |
| Save editor [xcier/GBFR-Save-Editor](https://github.com/xcier/GBFR-Save-Editor) (has item/sigil add) | ⚠️ last pushed pre-ER (2026-06-22); 2.0 saves unverified |

Refs: mod loader updated for 2.0 day-one ([gbfrelink.utility.manager](https://github.com/WistfulHopes/gbfrelink.utility.manager) / Nexus 526);
Nenkai's overlay modtools (Nexus 533) still 1.x. Saves at `%localappdata%\GBFR\Saved\SaveGames`.

## Next steps
- Build the **goodness-based Transmarvel tuner** (Tier 1 config mod): ship edited
  `gacha_rate_group.tbl` + `gacha_lot.tbl` presets ("2× good stuff", "jackpot mode",
  "no junk") as a Reloaded-II data mod. All data is in hand.
- Optionally add a `reward_lot` preset that makes Proud-tier quests drop Transmarvel
  Vouchers.
- Contribute upstream: 2.0 headers + `ITEM_21_0001`/`GEEN_320–327` id mappings
  (Nenkai's `sigil_id.csv`/`item_id.csv` are stale).
- Verify live once before publishing: one boosted-weight roll session to confirm the
  Transmarvel pool mapping (the community-lore discrepancy above).
