using System.ComponentModel;
using gbfr.transmarvel.overhaul.Template.Configuration;

namespace gbfr.transmarvel.overhaul.Configuration;

// The sigil/trait checkboxes below are GENERATED from game data (scratchpad
// gen-config-v2.mjs) — regenerate rather than hand-editing the lists.
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

    [Category("Sigil Pool - Auto")]
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

    [Category("Sigil Pool - Auto")]
    [DisplayName("Auto-remove completed sigils (reads your save)")]
    [Description("At every game launch the mod reads your newest save (read-only) and\n" +
        "removes from the Transmarvel pool: any TICKED Warpath+ you already own with\n" +
        "EVERY 2nd trait it can still roll (your ticked traits), and any TICKED\n" +
        "Awakening+ you own AT ALL (duplicates are useless). Warpath+ regain pool\n" +
        "slots automatically when you tick more traits (new combos to chase).\n" +
        "Unticked sigils stay out regardless. If the save can't be read the pool is\n" +
        "left unpruned (see the Reloaded console).")]
    [DefaultValue(true)]
    public bool AutoPruneCompleted { get; set; } = true;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Fearless Soul+")]
    [Description("Include Fearless Soul+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolFearlessSoul { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Guardian's Awakening+")]
    [Description("Include Guardian's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGuardiansAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Helmsman's Awakening+")]
    [Description("Include Helmsman's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHelmsmansAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Mage's Awakening+")]
    [Description("Include Mage's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolMagesAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Veteran's Awakening+")]
    [Description("Include Veteran's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolVeteransAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Rose's Awakening+")]
    [Description("Include Rose's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolRosesAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Phantasm's Awakening+")]
    [Description("Include Phantasm's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolPhantasmsAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("White Dragon's Awakening+")]
    [Description("Include White Dragon's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolWhiteDragonsAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Hero's Awakening+")]
    [Description("Include Hero's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHerosAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Lord's Awakening+")]
    [Description("Include Lord's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolLordsAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Dragonslayer's Awakening+")]
    [Description("Include Dragonslayer's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDragonslayersAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Holy Knight's Awakening+")]
    [Description("Include Holy Knight's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHolyKnightsAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Swordmaster's Awakening+")]
    [Description("Include Swordmaster's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSwordmastersAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Butterfly's Awakening+")]
    [Description("Include Butterfly's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolButterflysAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Eternal Rage's Awakening+")]
    [Description("Include Eternal Rage's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolEternalRagesAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Founder's Awakening+")]
    [Description("Include Founder's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolFoundersAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Versalis Soul+")]
    [Description("Include Versalis Soul+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolVersalisSoul { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Crimson's Awakening+")]
    [Description("Include Crimson's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolCrimsonsAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Ebony's Awakening+")]
    [Description("Include Ebony's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolEbonysAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+ (bucket extras)")]
    [DisplayName("Attack Power V+")]
    [Description("Not an Awakening sigil - vanilla keeps this stat single in the same chase bucket as the Awakening+, so it is listed here. Off by default; tick to add it to the pool (own-any auto-prune applies like the rest of the bucket). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolAttackPowerV { get; set; } = false;

    [Category("Sigil Pool - Awakening+ (bucket extras)")]
    [DisplayName("Health V+")]
    [Description("Not an Awakening sigil - vanilla keeps this stat single in the same chase bucket as the Awakening+, so it is listed here. Off by default; tick to add it to the pool (own-any auto-prune applies like the rest of the bucket). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolHealthV { get; set; } = false;

    [Category("Sigil Pool - Awakening+ (bucket extras)")]
    [DisplayName("Critical Hit Rate V+")]
    [Description("Not an Awakening sigil - vanilla keeps this stat single in the same chase bucket as the Awakening+, so it is listed here. Off by default; tick to add it to the pool (own-any auto-prune applies like the rest of the bucket). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolCriticalHitRateV { get; set; } = false;

    [Category("Sigil Pool - Awakening+ (bucket extras)")]
    [DisplayName("Stun Power V+")]
    [Description("Not an Awakening sigil - vanilla keeps this stat single in the same chase bucket as the Awakening+, so it is listed here. Off by default; tick to add it to the pool (own-any auto-prune applies like the rest of the bucket). RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolStunPowerV { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Spirit Edge's Awakening+")]
    [Description("Include Spirit Edge's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSpiritEdgesAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Dark Huntress's Awakening+")]
    [Description("Include Dark Huntress's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolDarkHuntresssAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Supreme Primarch's Awakening+")]
    [Description("Include Supreme Primarch's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolSupremePrimarchsAwakening { get; set; } = false;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Fearless Heart+")]
    [Description("Keep Fearless Heart+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolFearlessHeart { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Guardian's Warpath+")]
    [Description("Keep Guardian's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolGuardiansWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Helmsman's Warpath+")]
    [Description("Keep Helmsman's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolHelmsmansWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Mage's Warpath+")]
    [Description("Keep Mage's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolMagesWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Veteran's Warpath+")]
    [Description("Keep Veteran's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolVeteransWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Rose's Warpath+")]
    [Description("Keep Rose's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolRosesWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Holy Knight's Warpath+")]
    [Description("Keep Holy Knight's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolHolyKnightsWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Eternal Rage's Warpath+")]
    [Description("Keep Eternal Rage's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolEternalRagesWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Phantasm's Warpath+")]
    [Description("Keep Phantasm's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolPhantasmsWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Butterfly's Warpath+")]
    [Description("Keep Butterfly's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolButterflysWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("White Dragon's Warpath+")]
    [Description("Keep White Dragon's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolWhiteDragonsWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Hero's Warpath+")]
    [Description("Keep Hero's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolHerosWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Lord's Warpath+")]
    [Description("Keep Lord's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolLordsWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Dragonslayer's Warpath+")]
    [Description("Keep Dragonslayer's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolDragonslayersWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Founder's Warpath+")]
    [Description("Keep Founder's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolFoundersWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Swordmaster's Warpath+")]
    [Description("Keep Swordmaster's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSwordmastersWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Crimson's Warpath+")]
    [Description("Keep Crimson's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCrimsonsWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Ebony's Warpath+")]
    [Description("Keep Ebony's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolEbonysWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Spirit Edge's Warpath+")]
    [Description("Keep Spirit Edge's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSpiritEdgesWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Dark Huntress's Warpath+")]
    [Description("Keep Dark Huntress's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolDarkHuntresssWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Supreme Primarch's Warpath+")]
    [Description("Keep Supreme Primarch's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSupremePrimarchsWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Versalis Heart+")]
    [Description("Keep Versalis Heart+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolVersalisHeart { get; set; } = true;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Gladiator's Awakening+")]
    [Description("Include Gladiator's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolGladiatorsAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Bladequeen's Awakening+")]
    [Description("Include Bladequeen's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolBladequeensAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Ultramarine's Awakening+")]
    [Description("Include Ultramarine's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolUltramarinesAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Thunderwolf's Awakening+")]
    [Description("Include Thunderwolf's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolThunderwolfsAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("Enchantress's Awakening+")]
    [Description("Include Enchantress's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolEnchantresssAwakening { get; set; } = false;

    [Category("Sigil Pool - Awakening+")]
    [DisplayName("The Black's Awakening+")]
    [Description("Include The Black's Awakening+ in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoolTheBlacksAwakening { get; set; } = false;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Gladiator's Warpath+")]
    [Description("Keep Gladiator's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolGladiatorsWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Bladequeen's Warpath+")]
    [Description("Keep Bladequeen's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolBladequeensWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Ultramarine's Warpath+")]
    [Description("Keep Ultramarine's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolUltramarinesWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Thunderwolf's Warpath+")]
    [Description("Keep Thunderwolf's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolThunderwolfsWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("Enchantress's Warpath+")]
    [Description("Keep Enchantress's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolEnchantresssWarpath { get; set; } = true;

    [Category("Sigil Pool - Warpath+")]
    [DisplayName("The Black's Warpath+")]
    [Description("Keep The Black's Warpath+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolTheBlacksWarpath { get; set; } = true;

    [Category("Sigil Pool - Chase V+ (2.0)")]
    [DisplayName("Celestial Nyx V+")]
    [Description("Keep Celestial Nyx V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialNyxV { get; set; } = true;

    [Category("Sigil Pool - Chase V+ (2.0)")]
    [DisplayName("Celestial Lumen V+")]
    [Description("Keep Celestial Lumen V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialLumenV { get; set; } = true;

    [Category("Sigil Pool - Chase V+ (2.0)")]
    [DisplayName("Celestial Terra V+")]
    [Description("Keep Celestial Terra V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialTerraV { get; set; } = true;

    [Category("Sigil Pool - Chase V+ (2.0)")]
    [DisplayName("Celestial Incendo V+")]
    [Description("Keep Celestial Incendo V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialIncendoV { get; set; } = true;

    [Category("Sigil Pool - Chase V+ (2.0)")]
    [DisplayName("Celestial Aqua V+")]
    [Description("Keep Celestial Aqua V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialAquaV { get; set; } = true;

    [Category("Sigil Pool - Chase V+ (2.0)")]
    [DisplayName("Fatebreaker V+")]
    [Description("Keep Fatebreaker V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolFatebreakerV { get; set; } = true;

    [Category("Sigil Pool - Chase V+ (2.0)")]
    [DisplayName("Celestial Ventus V+")]
    [Description("Keep Celestial Ventus V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolCelestialVentusV { get; set; } = true;

    [Category("Sigil Pool - Chase V+ (2.0)")]
    [DisplayName("Divergence V+")]
    [Description("Keep Divergence V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolDivergenceV { get; set; } = true;

    [Category("Sigil Pool - Chase V+")]
    [DisplayName("War Elemental+")]
    [Description("Keep War Elemental+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolWarElemental { get; set; } = true;

    [Category("Sigil Pool - Chase V+")]
    [DisplayName("Supplementary Damage V+")]
    [Description("Keep Supplementary Damage V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSupplementaryDamageV { get; set; } = true;

    [Category("Sigil Pool - Chase V+")]
    [DisplayName("Berserker Echo+")]
    [Description("Keep Berserker Echo+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolBerserkerEcho { get; set; } = true;

    [Category("Sigil Pool - Chase V+")]
    [DisplayName("Spartan Echo+")]
    [Description("Keep Spartan Echo+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolSpartanEcho { get; set; } = true;

    [Category("Sigil Pool - Chase V+")]
    [DisplayName("Greater Aegis V+")]
    [Description("Keep Greater Aegis V+ in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PoolGreaterAegisV { get; set; } = true;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Aegis")]
    [Description("Allow Aegis as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Aegis { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Autorevive")]
    [Description("Allow Autorevive as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Autorevive { get; set; } = true;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Cascade")]
    [Description("Allow Cascade as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Cascade { get; set; } = true;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Drain")]
    [Description("Allow Drain as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Drain { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Firm Stance")]
    [Description("Allow Firm Stance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool FirmStance { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Garrison")]
    [Description("Allow Garrison as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Garrison { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Greater Aegis")]
    [Description("Allow Greater Aegis as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool GreaterAegis { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Guts")]
    [Description("Allow Guts as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Guts { get; set; } = true;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("HP")]
    [Description("Allow HP as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Hp { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Improved Dodge")]
    [Description("Allow Improved Dodge as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ImprovedDodge { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Improved Guard")]
    [Description("Allow Improved Guard as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ImprovedGuard { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Improved Healing")]
    [Description("Allow Improved Healing as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ImprovedHealing { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Low Profile")]
    [Description("Allow Low Profile as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LowProfile { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Nimble Defense")]
    [Description("Allow Nimble Defense as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool NimbleDefense { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Precise Resilience")]
    [Description("Allow Precise Resilience as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PreciseResilience { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Provoke")]
    [Description("Allow Provoke as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Provoke { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Quick Cooldown")]
    [Description("Allow Quick Cooldown as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool QuickCooldown { get; set; } = true;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Regen")]
    [Description("Allow Regen as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Regen { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Steel Nerves")]
    [Description("Allow Steel Nerves as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SteelNerves { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Stronghold")]
    [Description("Allow Stronghold as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Stronghold { get; set; } = false;

    [Category("2nd Traits - Defense & Sustain")]
    [DisplayName("Uplift")]
    [Description("Allow Uplift as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Uplift { get; set; } = true;

    [Category("2nd Traits - Offense")]
    [DisplayName("ATK")]
    [Description("Allow ATK as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Atk { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Berserker")]
    [Description("Allow Berserker as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Berserker { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Break Assassin")]
    [Description("Allow Break Assassin as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool BreakAssassin { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Charged Attack DMG")]
    [Description("Allow Charged Attack DMG as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ChargedAttackDmg { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Combo Booster")]
    [Description("Allow Combo Booster as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ComboBooster { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Combo Finisher DMG")]
    [Description("Allow Combo Finisher DMG as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ComboFinisherDmg { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Concentrated Fire")]
    [Description("Allow Concentrated Fire as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ConcentratedFire { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Critical Hit DMG")]
    [Description("Allow Critical Hit DMG as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool CriticalHitDmg { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Critical Hit Rate")]
    [Description("Allow Critical Hit Rate as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool CriticalHitRate { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("DMG Cap")]
    [Description("Allow DMG Cap as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool DmgCap { get; set; } = true;

    [Category("2nd Traits - Offense")]
    [DisplayName("Dodge Payback")]
    [Description("Allow Dodge Payback as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool DodgePayback { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Enmity")]
    [Description("Allow Enmity as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool Enmity { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Glass Cannon")]
    [Description("Allow Glass Cannon as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool GlassCannon { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Guard Payback")]
    [Description("Allow Guard Payback as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool GuardPayback { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Head Start")]
    [Description("Allow Head Start as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool HeadStart { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Injury to Insult")]
    [Description("Allow Injury to Insult as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool InjuryToInsult { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Less Is More")]
    [Description("Allow Less Is More as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LessIsMore { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Life on the Line")]
    [Description("Allow Life on the Line as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LifeOnTheLine { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Linked Together")]
    [Description("Allow Linked Together as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LinkedTogether { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Lucky Charge")]
    [Description("Allow Lucky Charge as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool LuckyCharge { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Nimble Onslaught")]
    [Description("Allow Nimble Onslaught as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool NimbleOnslaught { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Overdrive Assassin")]
    [Description("Allow Overdrive Assassin as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool OverdriveAssassin { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Power Hungry")]
    [Description("Allow Power Hungry as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PowerHungry { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Precise Wrath")]
    [Description("Allow Precise Wrath as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PreciseWrath { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Quick Charge")]
    [Description("Allow Quick Charge as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool QuickCharge { get; set; } = true;

    [Category("2nd Traits - Offense")]
    [DisplayName("Skilled Assault")]
    [Description("Allow Skilled Assault as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SkilledAssault { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Stamina")]
    [Description("Allow Stamina as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Stamina { get; set; } = true;

    [Category("2nd Traits - Offense")]
    [DisplayName("Stun Power")]
    [Description("Allow Stun Power as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool StunPower { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Supplementary DMG")]
    [Description("Allow Supplementary DMG as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SupplementaryDmg { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Throw DMG")]
    [Description("Allow Throw DMG as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ThrowDmg { get; set; } = false;

    [Category("2nd Traits - Offense")]
    [DisplayName("Tyranny")]
    [Description("Allow Tyranny as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool Tyranny { get; set; } = true;

    [Category("2nd Traits - Offense")]
    [DisplayName("Weak Point DMG")]
    [Description("Allow Weak Point DMG as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool WeakPointDmg { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("ATK↓ Resistance")]
    [Description("Allow ATK↓ Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool AtkDownResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Blight Resistance")]
    [Description("Allow Blight Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool BlightResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Burn Resistance")]
    [Description("Allow Burn Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool BurnResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Darkflame Resistance")]
    [Description("Allow Darkflame Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool DarkflameResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("DEF↓ Resistance")]
    [Description("Allow DEF↓ Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool DefDownResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Dizzy Resistance")]
    [Description("Allow Dizzy Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool DizzyResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Glaciate Resistance")]
    [Description("Allow Glaciate Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool GlaciateResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Held Under Resistance")]
    [Description("Allow Held Under Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool HeldUnderResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Paralysis Resistance")]
    [Description("Allow Paralysis Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool ParalysisResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Poison Resistance")]
    [Description("Allow Poison Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PoisonResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Sandtomb Resistance")]
    [Description("Allow Sandtomb Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SandtombResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("SBA Sealed Resistance")]
    [Description("Allow SBA Sealed Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SbaSealedResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Skill Sealed Resistance")]
    [Description("Allow Skill Sealed Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SkillSealedResistance { get; set; } = false;

    [Category("2nd Traits - Resistances")]
    [DisplayName("Slow Resistance")]
    [Description("Allow Slow Resistance as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool SlowResistance { get; set; } = false;

    [Category("2nd Traits - Utility")]
    [DisplayName("Fast Learner")]
    [Description("Allow Fast Learner as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool FastLearner { get; set; } = false;

    [Category("2nd Traits - Utility")]
    [DisplayName("Path to Mastery")]
    [Description("Allow Path to Mastery as a random 2nd trait, wherever it can naturally roll. NOTE: vanilla never rolls this on Transmarvel pulls (curio/drop-path sigils only) - ticking it does NOT add it to Transmarvel rolls. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool PathToMastery { get; set; } = false;

    [Category("2nd Traits - Utility")]
    [DisplayName("Potion Hoarder")]
    [Description("Allow Potion Hoarder as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool PotionHoarder { get; set; } = true;

    [Category("2nd Traits - Utility")]
    [DisplayName("Rupie Tycoon")]
    [Description("Allow Rupie Tycoon as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(false)]
    public bool RupieTycoon { get; set; } = false;

    [Category("2nd Traits - Utility")]
    [DisplayName("Steady Focus")]
    [Description("Allow Steady Focus as a random 2nd trait, wherever it can naturally roll. RESTART THE GAME after changing.")]
    [DefaultValue(true)]
    public bool SteadyFocus { get; set; } = true;

}

/// <summary>
/// Allows overriding aspects of the configuration creation process.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
