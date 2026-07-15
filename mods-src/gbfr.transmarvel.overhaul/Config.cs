using System.ComponentModel;
using gbfr.transmarvel.overhaul.Template.Configuration;

namespace gbfr.transmarvel.overhaul.Configuration;

// The per-skill toggles below are GENERATED from game data:
//   node scripts/gen-unified-skill-catalog.mjs   (game tables -> skill-catalog.json)
//   node scripts/gen-transmarvel-code.mjs        (skill-catalog.json -> C#)
// Regenerate rather than hand-editing the lists. Categories and ordering mirror
// the in-game skill list (skill.tbl GemCategory/SortOrder).
public class Config : Configurable<Config>
{
    [Category("Voucher Income")]
    [DisplayName("Vouchers per quest clear")]
    [Description("Transmarvel Vouchers granted on every eligible quest clear.\n" +
        "0 = vanilla (no guaranteed vouchers), default 10.\n" +
        "ENGINE LIMIT: the game clamps each reward grant to 5, and a reward row has\n" +
        "at most 5 slots - so the real ceiling is 25 per clear, and quests whose\n" +
        "reward rows have fewer free slots grant less. For BULK testing use\n" +
        "'Transmarvel cost' = 1 instead (curio transmutes then fund 1-15 rolls each).\n\n" +
        "RESTART THE GAME after changing this - reward tables are read once, at launch.")]
    [DefaultValue(10)]
    public int VouchersPerClear { get; set; } = 10;

    [Category("Voucher Income")]
    [DisplayName("Transmarvel cost (points)")]
    [Description("Cost of one Transmarvel roll in Transmarvel points (vanilla: 150).\n" +
        "Lower = cheaper rolls. MINIMUM 1 - a cost of 0 crashes the Transmarvel menu,\n" +
        "so the mod clamps it to 1.\n\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(150)]
    public int TransmarvelCost { get; set; } = 150;

    [Category("Voucher Income")]
    [DisplayName("Voucher quests: minimum difficulty tier")]
    [Description("Lowest quest-board difficulty tier whose clears grant the vouchers.\n" +
        "8 = Chaos and above (default). Lower it to farm vouchers on easier quests:\n" +
        "  1 = everything from the earliest board quests (PWR 200+)\n" +
        "  5 = PWR ~5k+, 6 = PWR ~8k+, 7 = PWR ~12k+ (Proud-era)\n" +
        "Tiers above Chaos (Chaos+/Chaos++/Infinity) always grant.\n\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(8)]
    public int VoucherMinTier { get; set; } = 8;

    [Category("General")]
    [DisplayName("Wrightstone drops")]
    [Description("Which wrightstones the 25% wrightstone side of a Transmarvel can roll.\n" +
        "All options guarantee a LEVEL 20 main skill:\n" +
        "  0 = Transmarveled fixed subs: always Aegis 15 / ATK 10 (vanilla 0.1% stones; default)\n" +
        "  1 = RANDOM subs at Lv15/Lv10 - the sub skills roll from your ticked 2nd traits\n" +
        "      (vanilla high-tier stones; the mod's trait filter applies to them)\n" +
        "  2 = both, 50/50\n\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(0)]
    public int WrightstoneDrops { get; set; } = 0;

    [Category("General")]
    [DisplayName("Auto-remove completed sigils (reads your save)")]
    [Description("At every game launch the mod reads your newest save (read-only) and\n" +
        "removes from the Transmarvel pool: any TICKED Warpath+ or mod-added V+ sigil\n" +
        "you already own with EVERY 2nd trait it can still roll (your ticked traits),\n" +
        "and any TICKED Awakening+/stat V+ you own AT ALL (duplicates are useless).\n" +
        "Pruned sigils regain pool slots automatically when you tick more traits\n" +
        "(new combos to chase). Unticked sigils stay out regardless. If the save\n" +
        "can't be read the pool is left unpruned (see the Reloaded console).")]
    [DefaultValue(true)]
    public bool AutoPruneCompleted { get; set; } = true;

    [Category("1. Basic Stats")]
    [DisplayName("Attack Power V+ (primary sigil)")]
    [Description("Roll Attack Power V+ as a Transmarvel main. Vanilla keeps this stat single (no 2nd trait slot) in the same chase bucket as the Awakening+ sigils, so it is opt-in like them, with the same own-any auto-prune. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolAttackPowerV { get; set; } = false;

    [Category("1. Basic Stats")]
    [DisplayName("ATK (2nd trait)")]
    [Description("Allow ATK to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Atk { get; set; } = false;

    [Category("1. Basic Stats")]
    [DisplayName("Health V+ (primary sigil)")]
    [Description("Roll Health V+ as a Transmarvel main. Vanilla keeps this stat single (no 2nd trait slot) in the same chase bucket as the Awakening+ sigils, so it is opt-in like them, with the same own-any auto-prune. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHealthV { get; set; } = false;

    [Category("1. Basic Stats")]
    [DisplayName("HP (2nd trait)")]
    [Description("Allow HP to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Hp { get; set; } = false;

    [Category("1. Basic Stats")]
    [DisplayName("Critical Hit Rate V+ (primary sigil)")]
    [Description("Roll Critical Hit Rate V+ as a Transmarvel main. Vanilla keeps this stat single (no 2nd trait slot) in the same chase bucket as the Awakening+ sigils, so it is opt-in like them, with the same own-any auto-prune. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolCriticalHitRateV { get; set; } = false;

    [Category("1. Basic Stats")]
    [DisplayName("Critical Hit Rate (2nd trait)")]
    [Description("Allow Critical Hit Rate to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool CriticalHitRate { get; set; } = false;

    [Category("1. Basic Stats")]
    [DisplayName("Stun Power V+ (primary sigil)")]
    [Description("Roll Stun Power V+ as a Transmarvel main. Vanilla keeps this stat single (no 2nd trait slot) in the same chase bucket as the Awakening+ sigils, so it is opt-in like them, with the same own-any auto-prune. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolStunPowerV { get; set; } = false;

    [Category("1. Basic Stats")]
    [DisplayName("Stun Power (2nd trait)")]
    [Description("Allow Stun Power to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool StunPower { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Enmity V+ (primary sigil)")]
    [Description("Roll Enmity V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolEnmityV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Enmity (2nd trait)")]
    [Description("Allow Enmity to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Enmity { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Celestial Nyx V+ (primary sigil)")]
    [Description("Keep Celestial Nyx V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialNyxV { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Stamina V+ (primary sigil)")]
    [Description("Roll Stamina V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolStaminaV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Stamina (2nd trait)")]
    [Description("Allow Stamina to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Stamina { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Celestial Lumen V+ (primary sigil)")]
    [Description("Keep Celestial Lumen V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialLumenV { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Critical Damage V+ (primary sigil)")]
    [Description("Roll Critical Damage V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolCriticalDamageV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Critical Hit DMG (2nd trait)")]
    [Description("Allow Critical Hit DMG to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool CriticalHitDmg { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Exploiter V+ (primary sigil)")]
    [Description("Roll Exploiter V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolExploiterV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Weak Point DMG (2nd trait)")]
    [Description("Allow Weak Point DMG to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool WeakPointDmg { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Tyranny V+ (primary sigil)")]
    [Description("Roll Tyranny V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolTyrannyV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Tyranny (2nd trait)")]
    [Description("Allow Tyranny to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Tyranny { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Celestial Terra V+ (primary sigil)")]
    [Description("Keep Celestial Terra V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialTerraV { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Life on the Line V+ (primary sigil)")]
    [Description("Roll Life on the Line V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolLifeOnTheLineV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Life on the Line (2nd trait)")]
    [Description("Allow Life on the Line to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LifeOnTheLine { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Celestial Ventus V+ (primary sigil)")]
    [Description("Keep Celestial Ventus V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialVentusV { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Damage Cap V+ (primary sigil)")]
    [Description("Roll Damage Cap V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDamageCapV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("DMG Cap (2nd trait)")]
    [Description("Allow DMG Cap to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool DmgCap { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Skilled Assault V+ (primary sigil)")]
    [Description("Roll Skilled Assault V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSkilledAssaultV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Skilled Assault (2nd trait)")]
    [Description("Allow Skilled Assault to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SkilledAssault { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Linked Together V+ (primary sigil)")]
    [Description("Roll Linked Together V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolLinkedTogetherV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Linked Together (2nd trait)")]
    [Description("Allow Linked Together to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LinkedTogether { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Combo Booster V+ (primary sigil)")]
    [Description("Roll Combo Booster V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolComboBoosterV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Combo Booster (2nd trait)")]
    [Description("Allow Combo Booster to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ComboBooster { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Supplementary Damage V+ (primary sigil)")]
    [Description("Keep Supplementary Damage V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSupplementaryDamageV { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Supplementary DMG (2nd trait)")]
    [Description("Allow Supplementary DMG to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SupplementaryDmg { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Berserker Echo+ (primary sigil)")]
    [Description("Keep Berserker Echo+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolBerserkerEcho { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Spartan Echo+ (primary sigil)")]
    [Description("Keep Spartan Echo+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSpartanEcho { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Combo Finisher V+ (primary sigil)")]
    [Description("Roll Combo Finisher V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolComboFinisherV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Combo Finisher DMG (2nd trait)")]
    [Description("Allow Combo Finisher DMG to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ComboFinisherDmg { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Charged Attack V+ (primary sigil)")]
    [Description("Roll Charged Attack V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolChargedAttackV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Charged Attack DMG (2nd trait)")]
    [Description("Allow Charged Attack DMG to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ChargedAttackDmg { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Lucky Charge V+ (primary sigil)")]
    [Description("Roll Lucky Charge V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolLuckyChargeV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Lucky Charge (2nd trait)")]
    [Description("Allow Lucky Charge to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LuckyCharge { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Quick Charge V+ (primary sigil)")]
    [Description("Roll Quick Charge V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolQuickChargeV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Quick Charge (2nd trait)")]
    [Description("Allow Quick Charge to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool QuickCharge { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Concentrated Fire V+ (primary sigil)")]
    [Description("Roll Concentrated Fire V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolConcentratedFireV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Concentrated Fire (2nd trait)")]
    [Description("Allow Concentrated Fire to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ConcentratedFire { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Throw V+ (primary sigil)")]
    [Description("Roll Throw V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolThrowV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Throw DMG (2nd trait)")]
    [Description("Allow Throw DMG to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ThrowDmg { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Injury to Insult V+ (primary sigil)")]
    [Description("Roll Injury to Insult V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolInjuryToInsultV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Injury to Insult (2nd trait)")]
    [Description("Allow Injury to Insult to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool InjuryToInsult { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Overdrive Assassin V+ (primary sigil)")]
    [Description("Roll Overdrive Assassin V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolOverdriveAssassinV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Overdrive Assassin (2nd trait)")]
    [Description("Allow Overdrive Assassin to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool OverdriveAssassin { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Celestial Incendo V+ (primary sigil)")]
    [Description("Keep Celestial Incendo V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialIncendoV { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Break Assassin V+ (primary sigil)")]
    [Description("Roll Break Assassin V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolBreakAssassinV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Break Assassin (2nd trait)")]
    [Description("Allow Break Assassin to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool BreakAssassin { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Celestial Aqua V+ (primary sigil)")]
    [Description("Keep Celestial Aqua V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialAquaV { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Fatebreaker V+ (primary sigil)")]
    [Description("Keep Fatebreaker V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolFatebreakerV { get; set; } = true;

    [Category("2. Attack")]
    [DisplayName("Head Start V+ (primary sigil)")]
    [Description("Roll Head Start V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHeadStartV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Head Start (2nd trait)")]
    [Description("Allow Head Start to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool HeadStart { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Power Hungry V+ (primary sigil)")]
    [Description("Roll Power Hungry V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPowerHungryV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Power Hungry (2nd trait)")]
    [Description("Allow Power Hungry to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PowerHungry { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Guard Payback V+ (primary sigil)")]
    [Description("Roll Guard Payback V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGuardPaybackV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Guard Payback (2nd trait)")]
    [Description("Allow Guard Payback to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool GuardPayback { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Dodge Payback V+ (primary sigil)")]
    [Description("Roll Dodge Payback V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDodgePaybackV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Dodge Payback (2nd trait)")]
    [Description("Allow Dodge Payback to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool DodgePayback { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Super Ultimate Perfect Dodge+ (primary sigil)")]
    [Description("Roll Super Ultimate Perfect Dodge+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSuperUltimatePerfectDodge { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Less Is More V+ (primary sigil)")]
    [Description("Roll Less Is More V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolLessIsMoreV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Less Is More (2nd trait)")]
    [Description("Allow Less Is More to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LessIsMore { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Berserker V+ (primary sigil)")]
    [Description("Roll Berserker V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolBerserkerV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Berserker (2nd trait)")]
    [Description("Allow Berserker to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Berserker { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Glass Cannon V+ (primary sigil)")]
    [Description("Roll Glass Cannon V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGlassCannonV { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Glass Cannon (2nd trait)")]
    [Description("Allow Glass Cannon to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool GlassCannon { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("Roll of the Die+ (primary sigil)")]
    [Description("Roll Roll of the Die+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolRollOfTheDie { get; set; } = false;

    [Category("2. Attack")]
    [DisplayName("War Elemental+ (primary sigil)")]
    [Description("Keep War Elemental+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolWarElemental { get; set; } = true;

    [Category("3. Defense")]
    [DisplayName("Aegis V+ (primary sigil)")]
    [Description("Roll Aegis V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolAegisV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Aegis (2nd trait)")]
    [Description("Allow Aegis to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Aegis { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Greater Aegis V+ (primary sigil)")]
    [Description("Keep Greater Aegis V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolGreaterAegisV { get; set; } = true;

    [Category("3. Defense")]
    [DisplayName("Greater Aegis (2nd trait)")]
    [Description("Allow Greater Aegis to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool GreaterAegis { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Garrison V+ (primary sigil)")]
    [Description("Roll Garrison V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGarrisonV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Garrison (2nd trait)")]
    [Description("Allow Garrison to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Garrison { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Stronghold V+ (primary sigil)")]
    [Description("Roll Stronghold V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolStrongholdV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Stronghold (2nd trait)")]
    [Description("Allow Stronghold to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Stronghold { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Steel Nerves V+ (primary sigil)")]
    [Description("Roll Steel Nerves V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSteelNervesV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Steel Nerves (2nd trait)")]
    [Description("Allow Steel Nerves to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SteelNerves { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Improved Guard V+ (primary sigil)")]
    [Description("Roll Improved Guard V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolImprovedGuardV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Improved Guard (2nd trait)")]
    [Description("Allow Improved Guard to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ImprovedGuard { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Improved Dodge+ (primary sigil)")]
    [Description("Roll Improved Dodge+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolImprovedDodge { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Improved Dodge (2nd trait)")]
    [Description("Allow Improved Dodge to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ImprovedDodge { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Precise Resilience V+ (primary sigil)")]
    [Description("Roll Precise Resilience V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPreciseResilienceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Precise Resilience (2nd trait)")]
    [Description("Allow Precise Resilience to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PreciseResilience { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Nimble Defense V+ (primary sigil)")]
    [Description("Roll Nimble Defense V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolNimbleDefenseV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Nimble Defense (2nd trait)")]
    [Description("Allow Nimble Defense to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool NimbleDefense { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Untouchable+ (primary sigil)")]
    [Description("Roll Untouchable+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolUntouchable { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Flight over Fight+ (primary sigil)")]
    [Description("Roll Flight over Fight+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolFlightOverFight { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Firm Stance V+ (primary sigil)")]
    [Description("Roll Firm Stance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolFirmStanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Firm Stance (2nd trait)")]
    [Description("Allow Firm Stance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool FirmStance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Dizzy Resistance V+ (primary sigil)")]
    [Description("Roll Dizzy Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDizzyResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Dizzy Resistance (2nd trait)")]
    [Description("Allow Dizzy Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool DizzyResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Glaciate Resistance V+ (primary sigil)")]
    [Description("Roll Glaciate Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGlaciateResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Glaciate Resistance (2nd trait)")]
    [Description("Allow Glaciate Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool GlaciateResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Held Under Resistance V+ (primary sigil)")]
    [Description("Roll Held Under Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHeldUnderResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Held Under Resistance (2nd trait)")]
    [Description("Allow Held Under Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool HeldUnderResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Sandtomb Resistance V+ (primary sigil)")]
    [Description("Roll Sandtomb Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSandtombResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Sandtomb Resistance (2nd trait)")]
    [Description("Allow Sandtomb Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SandtombResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Poison Resistance V+ (primary sigil)")]
    [Description("Roll Poison Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPoisonResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Poison Resistance (2nd trait)")]
    [Description("Allow Poison Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoisonResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Slow Resistance V+ (primary sigil)")]
    [Description("Roll Slow Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSlowResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Slow Resistance (2nd trait)")]
    [Description("Allow Slow Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SlowResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Burn Resistance V+ (primary sigil)")]
    [Description("Roll Burn Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolBurnResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Burn Resistance (2nd trait)")]
    [Description("Allow Burn Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool BurnResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Paralysis Resistance V+ (primary sigil)")]
    [Description("Roll Paralysis Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolParalysisResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Paralysis Resistance (2nd trait)")]
    [Description("Allow Paralysis Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ParalysisResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Blight Resistance V+ (primary sigil)")]
    [Description("Roll Blight Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolBlightResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Blight Resistance (2nd trait)")]
    [Description("Allow Blight Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool BlightResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Skill Sealed Resistance V+ (primary sigil)")]
    [Description("Roll Skill Sealed Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSkillSealedResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Skill Sealed Resistance (2nd trait)")]
    [Description("Allow Skill Sealed Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SkillSealedResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("SBA Sealed Resistance V+ (primary sigil)")]
    [Description("Roll SBA Sealed Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSBASealedResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("SBA Sealed Resistance (2nd trait)")]
    [Description("Allow SBA Sealed Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SbaSealedResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Darkflame Resistance V+ (primary sigil)")]
    [Description("Roll Darkflame Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDarkflameResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Darkflame Resistance (2nd trait)")]
    [Description("Allow Darkflame Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool DarkflameResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Attack Down Resistance V+ (primary sigil)")]
    [Description("Roll Attack Down Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolAttackDownResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("ATK↓ Resistance (2nd trait)")]
    [Description("Allow ATK↓ Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool AtkDownResistance { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("Defense Down Resistance V+ (primary sigil)")]
    [Description("Roll Defense Down Resistance V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDefenseDownResistanceV { get; set; } = false;

    [Category("3. Defense")]
    [DisplayName("DEF↓ Resistance (2nd trait)")]
    [Description("Allow DEF↓ Resistance to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool DefDownResistance { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Improved Healing V+ (primary sigil)")]
    [Description("Roll Improved Healing V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolImprovedHealingV { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Improved Healing (2nd trait)")]
    [Description("Allow Improved Healing to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ImprovedHealing { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Regen V+ (primary sigil)")]
    [Description("Roll Regen V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolRegenV { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Regen (2nd trait)")]
    [Description("Allow Regen to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Regen { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Divergence V+ (primary sigil)")]
    [Description("Keep Divergence V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolDivergenceV { get; set; } = true;

    [Category("4. Support")]
    [DisplayName("Drain V+ (primary sigil)")]
    [Description("Roll Drain V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDrainV { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Drain (2nd trait)")]
    [Description("Allow Drain to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Drain { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Quick Cooldown V+ (primary sigil)")]
    [Description("Roll Quick Cooldown V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolQuickCooldownV { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Quick Cooldown (2nd trait)")]
    [Description("Allow Quick Cooldown to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool QuickCooldown { get; set; } = true;

    [Category("4. Support")]
    [DisplayName("Cascade V+ (primary sigil)")]
    [Description("Roll Cascade V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolCascadeV { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Cascade (2nd trait)")]
    [Description("Allow Cascade to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Cascade { get; set; } = true;

    [Category("4. Support")]
    [DisplayName("Uplift V+ (primary sigil)")]
    [Description("Roll Uplift V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolUpliftV { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Uplift (2nd trait)")]
    [Description("Allow Uplift to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Uplift { get; set; } = true;

    [Category("4. Support")]
    [DisplayName("Precise Wrath V+ (primary sigil)")]
    [Description("Roll Precise Wrath V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPreciseWrathV { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Precise Wrath (2nd trait)")]
    [Description("Allow Precise Wrath to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PreciseWrath { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Nimble Onslaught V+ (primary sigil)")]
    [Description("Roll Nimble Onslaught V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolNimbleOnslaughtV { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Nimble Onslaught (2nd trait)")]
    [Description("Allow Nimble Onslaught to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool NimbleOnslaught { get; set; } = false;

    [Category("4. Support")]
    [DisplayName("Potent Greens+ (primary sigil)")]
    [Description("Roll Potent Greens+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPotentGreens { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Potion Hoarder V+ (primary sigil)")]
    [Description("Roll Potion Hoarder V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPotionHoarderV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Potion Hoarder (2nd trait)")]
    [Description("Allow Potion Hoarder to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PotionHoarder { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Auto Potion+ (primary sigil)")]
    [Description("Roll Auto Potion+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolAutoPotion { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Guts V+ (primary sigil)")]
    [Description("Roll Guts V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGutsV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Guts (2nd trait)")]
    [Description("Allow Guts to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Guts { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Autorevive V+ (primary sigil)")]
    [Description("Roll Autorevive V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolAutoreviveV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Autorevive (2nd trait)")]
    [Description("Allow Autorevive to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Autorevive { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Steady Focus V+ (primary sigil)")]
    [Description("Roll Steady Focus V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSteadyFocusV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Steady Focus (2nd trait)")]
    [Description("Allow Steady Focus to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool SteadyFocus { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Provoke V+ (primary sigil)")]
    [Description("Roll Provoke V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolProvokeV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Provoke (2nd trait)")]
    [Description("Allow Provoke to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Provoke { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Low Profile V+ (primary sigil)")]
    [Description("Roll Low Profile V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolLowProfileV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Low Profile (2nd trait)")]
    [Description("Allow Low Profile to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LowProfile { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Fast Learner V+ (primary sigil)")]
    [Description("Roll Fast Learner V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolFastLearnerV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Fast Learner (2nd trait)")]
    [Description("Allow Fast Learner to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool FastLearner { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Rupie Tycoon V+ (primary sigil)")]
    [Description("Roll Rupie Tycoon V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolRupieTycoonV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Rupie Tycoon (2nd trait)")]
    [Description("Allow Rupie Tycoon to roll as the random 2nd trait on '+' sigils. Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool RupieTycoon { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Path to Mastery V+ (primary sigil)")]
    [Description("Roll Path to Mastery V+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPathToMasteryV { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Path to Mastery (2nd trait)")]
    [Description("Allow Path to Mastery to roll as the random 2nd trait on '+' sigils. (Vanilla itself never rolls this on Transmarvel pulls - the mod's filter makes it rollable, a mod extension.) Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PathToMastery { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Stout Heart+ (primary sigil)")]
    [Description("Roll Stout Heart+ as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolStoutHeart { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Fearless Heart+ (primary sigil)")]
    [Description("Gran/Djeeta's character sigil. Keep Fearless Heart+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolFearlessHeart { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Fearless Soul+ (primary sigil)")]
    [Description("Gran/Djeeta's character sigil. Include Fearless Soul+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolFearlessSoul { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Guardian's Warpath+ (primary sigil)")]
    [Description("Katalina's character sigil. Keep Guardian's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolGuardiansWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Guardian's Awakening+ (primary sigil)")]
    [Description("Katalina's character sigil. Include Guardian's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGuardiansAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Helmsman's Warpath+ (primary sigil)")]
    [Description("Lancelot's character sigil. Keep Helmsman's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolHelmsmansWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Helmsman's Awakening+ (primary sigil)")]
    [Description("Lancelot's character sigil. Include Helmsman's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHelmsmansAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Mage's Warpath+ (primary sigil)")]
    [Description("Io's character sigil. Keep Mage's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolMagesWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Mage's Awakening+ (primary sigil)")]
    [Description("Io's character sigil. Include Mage's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolMagesAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Veteran's Warpath+ (primary sigil)")]
    [Description("Eugen's character sigil. Keep Veteran's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolVeteransWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Veteran's Awakening+ (primary sigil)")]
    [Description("Eugen's character sigil. Include Veteran's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolVeteransAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Rose's Warpath+ (primary sigil)")]
    [Description("Rosetta's character sigil. Keep Rose's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolRosesWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Rose's Awakening+ (primary sigil)")]
    [Description("Rosetta's character sigil. Include Rose's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolRosesAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Holy Knight's Warpath+ (primary sigil)")]
    [Description("Charlotta's character sigil. Keep Holy Knight's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolHolyKnightsWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Holy Knight's Awakening+ (primary sigil)")]
    [Description("Charlotta's character sigil. Include Holy Knight's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHolyKnightsAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Eternal Rage's Warpath+ (primary sigil)")]
    [Description("A character sigil. Keep Eternal Rage's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolEternalRagesWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Eternal Rage's Awakening+ (primary sigil)")]
    [Description("A character sigil. Include Eternal Rage's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolEternalRagesAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Phantasm's Warpath+ (primary sigil)")]
    [Description("Ferry's character sigil. Keep Phantasm's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolPhantasmsWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Phantasm's Awakening+ (primary sigil)")]
    [Description("Ferry's character sigil. Include Phantasm's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPhantasmsAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Butterfly's Warpath+ (primary sigil)")]
    [Description("Narmaya's character sigil. Keep Butterfly's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolButterflysWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Butterfly's Awakening+ (primary sigil)")]
    [Description("Narmaya's character sigil. Include Butterfly's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolButterflysAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("White Dragon's Warpath+ (primary sigil)")]
    [Description("Lancelot's character sigil. Keep White Dragon's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolWhiteDragonsWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("White Dragon's Awakening+ (primary sigil)")]
    [Description("Lancelot's character sigil. Include White Dragon's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolWhiteDragonsAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Hero's Warpath+ (primary sigil)")]
    [Description("Vane's character sigil. Keep Hero's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolHerosWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Hero's Awakening+ (primary sigil)")]
    [Description("Vane's character sigil. Include Hero's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHerosAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Lord's Warpath+ (primary sigil)")]
    [Description("Percival's character sigil. Keep Lord's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolLordsWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Lord's Awakening+ (primary sigil)")]
    [Description("Percival's character sigil. Include Lord's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolLordsAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Dragonslayer's Warpath+ (primary sigil)")]
    [Description("A character sigil. Keep Dragonslayer's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolDragonslayersWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Dragonslayer's Awakening+ (primary sigil)")]
    [Description("A character sigil. Include Dragonslayer's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDragonslayersAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Founder's Warpath+ (primary sigil)")]
    [Description("Rackam's character sigil. Keep Founder's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolFoundersWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Founder's Awakening+ (primary sigil)")]
    [Description("Rackam's character sigil. Include Founder's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolFoundersAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Swordmaster's Warpath+ (primary sigil)")]
    [Description("Zeta's character sigil. Keep Swordmaster's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSwordmastersWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Swordmaster's Awakening+ (primary sigil)")]
    [Description("Zeta's character sigil. Include Swordmaster's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSwordmastersAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Crimson's Warpath+ (primary sigil)")]
    [Description("Yodarha's character sigil. Keep Crimson's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCrimsonsWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Crimson's Awakening+ (primary sigil)")]
    [Description("Yodarha's character sigil. Include Crimson's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolCrimsonsAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Ebony's Warpath+ (primary sigil)")]
    [Description("Narmaya's character sigil. Keep Ebony's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolEbonysWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Ebony's Awakening+ (primary sigil)")]
    [Description("Narmaya's character sigil. Include Ebony's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolEbonysAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Ultramarine's Warpath+ (primary sigil)")]
    [Description("Sandalphon's character sigil. Keep Ultramarine's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolUltramarinesWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Ultramarine's Awakening+ (primary sigil)")]
    [Description("Sandalphon's character sigil. Include Ultramarine's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolUltramarinesAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Thunderwolf's Warpath+ (primary sigil)")]
    [Description("Eustace's character sigil. Keep Thunderwolf's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolThunderwolfsWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Thunderwolf's Awakening+ (primary sigil)")]
    [Description("Eustace's character sigil. Include Thunderwolf's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolThunderwolfsAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Spirit Edge's Warpath+ (primary sigil)")]
    [Description("Id's character sigil. Keep Spirit Edge's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSpiritEdgesWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Spirit Edge's Awakening+ (primary sigil)")]
    [Description("Id's character sigil. Include Spirit Edge's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSpiritEdgesAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Dark Huntress's Warpath+ (primary sigil)")]
    [Description("Vaseraga's character sigil. Keep Dark Huntress's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolDarkHuntresssWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Dark Huntress's Awakening+ (primary sigil)")]
    [Description("Vaseraga's character sigil. Include Dark Huntress's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDarkHuntresssAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Supreme Primarch's Warpath+ (primary sigil)")]
    [Description("Seofon's character sigil. Keep Supreme Primarch's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSupremePrimarchsWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Supreme Primarch's Awakening+ (primary sigil)")]
    [Description("Seofon's character sigil. Include Supreme Primarch's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSupremePrimarchsAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Enchantress's Warpath+ (primary sigil)")]
    [Description("Beatrix's character sigil. Keep Enchantress's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolEnchantresssWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Enchantress's Awakening+ (primary sigil)")]
    [Description("Beatrix's character sigil. Include Enchantress's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolEnchantresssAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("The Black's Warpath+ (primary sigil)")]
    [Description("Ferry's character sigil. Keep The Black's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolTheBlacksWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("The Black's Awakening+ (primary sigil)")]
    [Description("Ferry's character sigil. Include The Black's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolTheBlacksAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Versalis Heart+ (primary sigil)")]
    [Description("Cagliostro's character sigil. Keep Versalis Heart+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolVersalisHeart { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Versalis Soul+ (primary sigil)")]
    [Description("Cagliostro's character sigil. Include Versalis Soul+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolVersalisSoul { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Gladiator's Warpath+ (primary sigil)")]
    [Description("Tweyen's character sigil. Keep Gladiator's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolGladiatorsWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Gladiator's Awakening+ (primary sigil)")]
    [Description("Tweyen's character sigil. Include Gladiator's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGladiatorsAwakening { get; set; } = false;

    [Category("5. Special")]
    [DisplayName("Bladequeen's Warpath+ (primary sigil)")]
    [Description("Nier's character sigil. Keep Bladequeen's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolBladequeensWarpath { get; set; } = true;

    [Category("5. Special")]
    [DisplayName("Bladequeen's Awakening+ (primary sigil)")]
    [Description("Nier's character sigil. Include Bladequeen's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolBladequeensAwakening { get; set; } = false;
}

/// <summary>
/// Allows overriding aspects of the configuration creation process.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
