using System.ComponentModel;
using gbfr.qol.instantloot.Template.Configuration;

namespace gbfr.qol.instantloot.Configuration;

public class Config : Configurable<Config>
{
    [Category("Instant Loot")]
    [DisplayName("Auto Loot Quest Chest")]
    [Description("Instantly finishes the end-of-quest treasure countdown (writes 0 to the\n" +
        "30-second chest timer) so loot is auto-collected the moment it appears.\n\n" +
        "Takes effect on the NEXT game launch.")]
    [DefaultValue(true)]
    public bool AutoLootChest { get; set; } = true;

    [Category("Instant Loot")]
    [DisplayName("Skip Result Screen")]
    [Description("Zeroes the post-quest result-screen countdown so the loot/XP result pages\n" +
        "advance immediately instead of playing out their ceremony.\n\n" +
        "Takes effect on the NEXT game launch.")]
    [DefaultValue(true)]
    public bool SkipResultScreen { get; set; } = true;
}

public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
