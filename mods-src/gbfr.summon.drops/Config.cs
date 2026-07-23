using System.ComponentModel;
using gbfr.summon.drops.Template.Configuration;

namespace gbfr.summon.drops.Configuration;

public class Config : Configurable<Config>
{
    [Category("Summon Drops")]
    [DisplayName("Guaranteed summon drops")]
    [Description("Every reward source that can drop a summon always drops one.\n" +
        "Vanilla rolls the drop at 35/50/70/100% depending on the source; this\n" +
        "sets them all to 100%. Which summon you get stays vanilla-random.\n\n" +
        "RESTART THE GAME after changing this - tables are read once, at launch.")]
    [DefaultValue(true)]
    public bool GuaranteedDrops { get; set; } = true;

    [Category("Summon Drops")]
    [DisplayName("Max skill levels on dropped summons")]
    [Description("Skills rolled on a dropped summon always come out at the highest\n" +
        "level their pool allows. Vanilla level lots roll ranges like Lv4-6 or\n" +
        "Lv11-15 with the top level the rarest (e.g. Lv15 at 10%); this pins every\n" +
        "lot to its top level. WHICH skills roll stays vanilla-random.\n\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(true)]
    public bool MaxSkillLevels { get; set; } = true;

    [Category("Summon Drops")]
    [DisplayName("Boost 5-star chase-skill odds")]
    [Description("Every top-tier (5-star) summon has ONE premium 'chase' skill in its\n" +
        "passive pool throttled to 8% (Berserker Echo on Lucilius, War Elemental on\n" +
        "Goldslime/Lilith/Rolan, Spartan Echo on Beelzebub, Stout Heart on Behemoth,\n" +
        "etc.), while its filler skills sit at ~18-31%. This raises that one skill's\n" +
        "odds to the percentage below. Only 5-star pools with a throttled skill are\n" +
        "touched (19 summons); flat pools stay vanilla.\n\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(true)]
    public bool BoostChaseSkills { get; set; } = true;

    [Category("Summon Drops")]
    [DisplayName("  ... chase-skill odds %")]
    [Description("The share the chase skill gets when 'Boost 5-star chase-skill odds'\n" +
        "is on. Vanilla is 8. The filler skills split the rest, keeping their relative\n" +
        "proportions. 100 = the chase skill is the ONLY thing that can roll.\n\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(40)]
    public int ChaseSkillPercent { get; set; } = 40;
}

/// <summary>
/// Allows overriding aspects of the configuration creation process.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
