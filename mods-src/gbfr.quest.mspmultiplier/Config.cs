using System.ComponentModel;
using gbfr.quest.mspmultiplier.Template.Configuration;

namespace gbfr.quest.mspmultiplier.Configuration;

public class Config : Configurable<Config>
{
    [Category("Mastery Points")]
    [DisplayName("MSP Multiplier")]
    [Description("Multiplier applied to the mastery points (MSP) every quest grants on clear.\n" +
        "1 = vanilla, 5 = default, up to 100.\n\n" +
        "RESTART THE GAME after changing this — reward tables are read once, at launch.\n" +
        "EXP and rupie rewards are never affected.")]
    [DefaultValue(5)]
    public int Multiplier { get; set; } = 5;
}

/// <summary>
/// Allows overriding aspects of the configuration creation process.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
