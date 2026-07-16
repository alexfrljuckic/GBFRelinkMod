using System.ComponentModel;
using gbfr.synthesis.control.Template.Configuration;

namespace gbfr.synthesis.control.Configuration;

/// <summary>
/// Which of the four input traits to use. The game shows the possibilities as
/// "1/12 Prospects" — 4 traits, 12 ordered pairs.
///
/// Sigil A = the FIRST slot you fill (the left one when picking); B = the second
/// (right). On the Synthesis Request confirmation they're stacked, A above B.
/// Trait 1 is the upper of a sigil's two traits, trait 2 the lower.
/// </summary>
public enum TraitSource
{
    [Description("Sigil A (1st / left), trait 1")]  SigilA_Trait1 = 0,
    [Description("Sigil A (1st / left), trait 2")]  SigilA_Trait2 = 1,
    [Description("Sigil B (2nd / right), trait 1")] SigilB_Trait1 = 2,
    [Description("Sigil B (2nd / right), trait 2")] SigilB_Trait2 = 3,
}

public class Config : Configurable<Config>
{
    [Category("Sigil Synthesis")]
    [DisplayName("Always Grand Success")]
    [Description("Every sigil synthesis at Siero's is a Grand Success (traits come\n" +
        "out maxed at Lv15). Vanilla rolls Success/Great/Grand with Grand odds\n" +
        "between 45% and 85% depending on the Prospect score of your two input\n" +
        "sigils; this sets Grand to 100% at every score.\n\n" +
        "RESTART THE GAME after changing this - tables are read once, at launch.")]
    [DefaultValue(true)]
    public bool AlwaysGrandSuccess { get; set; } = true;

    [Category("Sigil Synthesis")]
    [DisplayName("Choose result traits")]
    [Description("Stop the dice. Vanilla picks the result's two traits at random from\n" +
        "the four traits of your two input sigils (that's the '1/12 Prospects' the\n" +
        "screen cycles). With this on, you decide exactly which two, every time.\n\n" +
        "Takes effect immediately - no restart needed.")]
    [DefaultValue(true)]
    public bool ChooseResultTraits { get; set; } = true;

    [Category("Sigil Synthesis")]
    [DisplayName("  ... result trait 1 (innate) comes from")]
    [Description("Which input trait becomes the result's FIRST trait. This also decides\n" +
        "which sigil you get: if it's an ATK trait, the result is an Attack Power sigil.\n\n" +
        "Sigil A = the first slot you fill (left); Sigil B = the second (right).\n\n" +
        "Ignored unless 'Choose result traits' is on.")]
    [DefaultValue(TraitSource.SigilA_Trait1)]
    public TraitSource InnateFrom { get; set; } = TraitSource.SigilA_Trait1;

    [Category("Sigil Synthesis")]
    [DisplayName("  ... result trait 2 (secondary) comes from")]
    [Description("Which input trait becomes the result's SECOND trait.\n\n" +
        "Must differ from the trait chosen above - the game only produces pairs of two\n" +
        "DIFFERENT traits, so if your two picks resolve to the same trait, that\n" +
        "synthesis is left vanilla (random) instead.\n\n" +
        "Ignored unless 'Choose result traits' is on.")]
    [DefaultValue(TraitSource.SigilB_Trait2)]
    public TraitSource SecondaryFrom { get; set; } = TraitSource.SigilB_Trait2;
}

/// <summary>
/// Allows overriding aspects of the configuration creation process.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
