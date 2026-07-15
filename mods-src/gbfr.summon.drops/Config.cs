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
}

/// <summary>
/// Allows overriding aspects of the configuration creation process.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
