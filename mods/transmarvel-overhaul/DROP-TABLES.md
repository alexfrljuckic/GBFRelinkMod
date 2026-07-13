# Transmarvel Overhaul — what can drop

**This shows the DEFAULT Configure Mod selection.** Your own ticks in
Reloaded-II → Transmarvel Overhaul → Configure Mod change all of it — the
Reloaded console prints your effective settings at every launch.
Two independent rolls decide a sigil: the **sigil** (table 1) and its random
**2nd trait** (table 2). Only combos the vanilla game could naturally produce
are ever generated.

## 1. Sigil pool — 41 ticked, equal ~2.44% each

75% of Transmarvels roll a sigil from this pool; 25% roll a wrightstone —
always a Lv20 main skill, with subs either fixed (Aegis 15/ATK 10) or rolled
at Lv15/Lv10 from your ticked traits, per the *Wrightstone drops* setting.

| Sigil | Bucket | Innate trait |
|---|---|---|
| Berserker Echo+ | Chase V+ | (its own trait, maxed) |
| Greater Aegis V+ | Chase V+ | (its own trait, maxed) |
| Spartan Echo+ | Chase V+ | (its own trait, maxed) |
| Supplementary Damage V+ | Chase V+ | (its own trait, maxed) |
| War Elemental+ | Chase V+ | (its own trait, maxed) |
| Celestial Aqua V+ | Chase V+ (2.0) | (its own trait, maxed) |
| Celestial Incendo V+ | Chase V+ (2.0) | (its own trait, maxed) |
| Celestial Lumen V+ | Chase V+ (2.0) | (its own trait, maxed) |
| Celestial Nyx V+ | Chase V+ (2.0) | (its own trait, maxed) |
| Celestial Terra V+ | Chase V+ (2.0) | (its own trait, maxed) |
| Celestial Ventus V+ | Chase V+ (2.0) | (its own trait, maxed) |
| Divergence V+ | Chase V+ (2.0) | (its own trait, maxed) |
| Fatebreaker V+ | Chase V+ (2.0) | (its own trait, maxed) |
| Bladequeen's Warpath+ | Warpath+ | Bladequeen's Warpath |
| Butterfly's Warpath+ | Warpath+ | Butterfly's Warpath |
| Crimson's Warpath+ | Warpath+ | Crimson's Warpath |
| Dark Huntress's Warpath+ | Warpath+ | Dark Huntress's Warpath |
| Dragonslayer's Warpath+ | Warpath+ | Dragonslayer's Warpath |
| Ebony's Warpath+ | Warpath+ | Ebony's Warpath |
| Enchantress's Warpath+ | Warpath+ | Enchantress's Warpath |
| Eternal Rage's Warpath+ | Warpath+ | Eternal Rage's Warpath |
| Fearless Heart+ | Warpath+ | Fearless Heart |
| Founder's Warpath+ | Warpath+ | Founder's Warpath |
| Gladiator's Warpath+ | Warpath+ | Gladiator's Warpath |
| Guardian's Warpath+ | Warpath+ | Guardian's Warpath |
| Helmsman's Warpath+ | Warpath+ | Helmsman's Warpath |
| Hero's Warpath+ | Warpath+ | Hero's Warpath |
| Holy Knight's Warpath+ | Warpath+ | Holy Knight's Warpath |
| Lord's Warpath+ | Warpath+ | Lord's Warpath |
| Mage's Warpath+ | Warpath+ | Mage's Warpath |
| Phantasm's Warpath+ | Warpath+ | Phantasm's Warpath |
| Rose's Warpath+ | Warpath+ | Rose's Warpath |
| Spirit Edge's Warpath+ | Warpath+ | Spirit Edge's Warpath |
| Supreme Primarch's Warpath+ | Warpath+ | Supreme Primarch's Warpath |
| Swordmaster's Warpath+ | Warpath+ | Swordmaster's Warpath |
| The Black's Warpath+ | Warpath+ | The Black's Warpath |
| Thunderwolf's Warpath+ | Warpath+ | Thunderwolf's Warpath |
| Ultramarine's Warpath+ | Warpath+ | Ultramarine's Warpath |
| Versalis Heart+ | Warpath+ | Versalis Heart |
| Veteran's Warpath+ | Warpath+ | Veteran's Warpath |
| White Dragon's Warpath+ | Warpath+ | White Dragon's Warpath |

Unticked (out of the pool): Fearless Soul+, Guardian's Awakening+, Helmsman's Awakening+, Mage's Awakening+, Veteran's Awakening+, Rose's Awakening+, Phantasm's Awakening+, White Dragon's Awakening+, Hero's Awakening+, Lord's Awakening+, Dragonslayer's Awakening+, Holy Knight's Awakening+, Swordmaster's Awakening+, Butterfly's Awakening+, Eternal Rage's Awakening+, Founder's Awakening+, Versalis Soul+, Crimson's Awakening+, Ebony's Awakening+, Attack Power V+, Health V+, Critical Hit Rate V+, Stun Power V+, Spirit Edge's Awakening+, Dark Huntress's Awakening+, Supreme Primarch's Awakening+, Gladiator's Awakening+, Bladequeen's Awakening+, Ultramarine's Awakening+, Thunderwolf's Awakening+, Enchantress's Awakening+, The Black's Awakening+.

## 2. Random 2nd trait — 11 ticked

On **Transmarvel pulls**, 11 of these can roll (~9.09% each):
vanilla legality caps what the Transmarvel path can produce, and ticking a
trait never adds it where the vanilla game couldn't roll it.

| 2nd trait | Category | On Transmarvel pulls? |
|---|---|---|
| Autorevive | Defense & Sustain | yes |
| Cascade | Defense & Sustain | yes |
| Guts | Defense & Sustain | yes |
| Quick Cooldown | Defense & Sustain | yes |
| Uplift | Defense & Sustain | yes |
| DMG Cap | Offense | yes |
| Quick Charge | Offense | yes |
| Stamina | Offense | yes |
| Tyranny | Offense | yes |
| Potion Hoarder | Utility | yes |
| Steady Focus | Utility | yes |

Combo math: 28 ticked Warpath+ × 11 rollable secondaries = 308 chase combos
(`scripts/build-jackpot-tables.mjs` unticks a Warpath+ automatically once you own all its combos).
