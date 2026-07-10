# Good Sigil Secondary Traits (Endless Ragnarok / 2.0)

> ⚠️ **Hobby project, not maintained.** Built for game **v2.0** — it *will* break on future
> updates and I won't patch it every time. For maintained mods, see
> [Nexus Mods](https://www.nexusmods.com/games/granbluefantasyrelink). Use at your own risk.

When a **"+" sigil** (generic V+ or character sigil+) rolls its random **secondary trait**,
it now always comes from a curated 18-trait list — the four vanilla 25% sub-lots
(offense / defense-resists / sustain / utility, ~58 mostly-junk traits) are replaced:

- **Offense (10)**: DMG Cap, Tyranny, Stamina, Critical Hit DMG, Weak Point DMG,
  Overdrive Assassin, Break Assassin, Skilled Assault, Injury to Insult, Quick Charge
- **Sustain (4)**: Cascade, Quick Cooldown, Regen, Uplift
- **Utility (4)**: Guts, Autorevive, Potion Hoarder, Steady Focus

All 18 equally likely (~5.6% each). Killed: every status resistance, ATK↓/DEF↓ Resistance,
Garrison/Aegis-type defense, Enmity, Throw DMG, Lucky Charge, Rupie Tycoon, Fast Learner,
Low Profile, Provoke, etc.

**Scope**: applies to the 2nd-trait roll on every + sigil acquisition (Transmarvel, curios,
drops) — pairs with [Transmarvel Jackpot](../transmarvel-jackpot/). Already-owned sigils
are unaffected (the trait is rolled once, at acquisition). Non-"+" sigils and other
random-trait systems (type-lots 1/3/4/14/15/26/27…) are untouched.

## Install
Same stack as the other mods (Reloaded-II +
[GBFR Mod Manager 2.0](https://www.nexusmods.com/granbluefantasyrelink/mods/526)):
extract the release zip into `Reloaded-II\Mods\`, enable
**Good Sigil Secondary Traits (2.0)**, launch. Troubleshooting: see the
[Jackpot README](../transmarvel-jackpot/README.md#if-something-blows-up-troubleshooting).

## How it works
Two unchanged-layout tables (1.x headers fine):
- **`skill_lot.tbl`**: +18 appended rows under a new sub-lot key `SKL_TMV_GOOD`
  (hash `485F6215`), weight 1 each.
- **`skill_type_lot.tbl`**: type-lots **2** (generic V+ sigils) and **5** (character
  sigils+) repointed to `SKL_TMV_GOOD` at 100% (their four vanilla sub-lots zeroed out).
  The shared vanilla sub-lots themselves are untouched, so other systems keep vanilla
  behavior.

Chain decoded in `scripts/decode-2nd-trait-lots.mjs`:
`gem.SkillTypeLotIdForRandom2ndSkill` → `skill_type_lot` (sub-lots + %) → `skill_lot`
(traits + weights). Verified byte-exact round-trips; diff = exactly the 2 repointed rows
+ 18 appended rows.

## Tweak the list
Edit the INSERT in the build steps (or ask): each trait is one `skill_lot` row —
add/remove rows under `SKL_TMV_GOOD` and re-export with GBFRDataTools.
