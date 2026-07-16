using System.ComponentModel;
using gbfr.shop.voucherbadges.Template.Configuration;

namespace gbfr.shop.voucherbadges.Configuration;

public class Config : Configurable<Config>
{
    private const string CostNote =
        "Cost in Knickknack Vouchers at the Treasure Trade tab (unlimited stock).\n" +
        "0 = remove this item from the shop. Minimum real cost is 1 - the game\n" +
        "caps FREE shop items at ~200 purchases, a paid cost keeps it unlimited.\n\n" +
        "RESTART THE GAME after changing this - shop tables are read once, at launch.";

    [Category("Shop Items")]
    [DisplayName("Silver Dalia Badge cost")]
    [Description(CostNote)]
    [DefaultValue(1)]
    public int SilverDaliaBadgeCost { get; set; } = 1;

    [Category("Shop Items")]
    [DisplayName("Gold Dalia Badge cost")]
    [Description(CostNote)]
    [DefaultValue(3)]
    public int GoldDaliaBadgeCost { get; set; } = 3;

    [Category("Shop Items")]
    [DisplayName("Gold Spellbook cost")]
    [Description(CostNote)]
    [DefaultValue(5)]
    public int GoldSpellbookCost { get; set; } = 5;

    [Category("Shop Items")]
    [DisplayName("Silver Centrum cost")]
    [Description(CostNote)]
    [DefaultValue(5)]
    public int SilverCentrumCost { get; set; } = 5;
}

/// <summary>
/// Allows overriding aspects of the configuration creation process.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
