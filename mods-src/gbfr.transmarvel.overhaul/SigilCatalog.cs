using gbfr.transmarvel.overhaul.Configuration;

namespace gbfr.transmarvel.overhaul;

// GENERATED from game data (scratchpad gen-config-v2.mjs).
// BucketKey = the gacha_lot Key of the chase bucket the sigil sits in.
public static class SigilCatalog
{
    public record Sigil(uint Hash, uint BucketKey, string Name, Func<Config, bool> Enabled);

    public static readonly Sigil[] Pool =
    {
        new(0x0713D928, 0xF527EF32, "Fearless Heart+", c => c.PoolFearlessHeart),
        new(0xAC175924, 0xF527EF32, "Guardian's Warpath+", c => c.PoolGuardiansWarpath),
        new(0xBC53CE24, 0xF527EF32, "Helmsman's Warpath+", c => c.PoolHelmsmansWarpath),
        new(0x43F26A91, 0xF527EF32, "Mage's Warpath+", c => c.PoolMagesWarpath),
        new(0xCAAE3F9C, 0xF527EF32, "Veteran's Warpath+", c => c.PoolVeteransWarpath),
        new(0x515E693C, 0xF527EF32, "Rose's Warpath+", c => c.PoolRosesWarpath),
        new(0x4C28585A, 0xF527EF32, "Holy Knight's Warpath+", c => c.PoolHolyKnightsWarpath),
        new(0x3069C2FE, 0xF527EF32, "Eternal Rage's Warpath+", c => c.PoolEternalRagesWarpath),
        new(0xE496D882, 0xF527EF32, "Phantasm's Warpath+", c => c.PoolPhantasmsWarpath),
        new(0xCEF31894, 0xF527EF32, "Butterfly's Warpath+", c => c.PoolButterflysWarpath),
        new(0x8A3819C0, 0xF527EF32, "White Dragon's Warpath+", c => c.PoolWhiteDragonsWarpath),
        new(0xA490BADF, 0xF527EF32, "Hero's Warpath+", c => c.PoolHerosWarpath),
        new(0x4CDCE25B, 0xF527EF32, "Lord's Warpath+", c => c.PoolLordsWarpath),
        new(0xE21A4170, 0xF527EF32, "Dragonslayer's Warpath+", c => c.PoolDragonslayersWarpath),
        new(0x66F1B128, 0xF527EF32, "Founder's Warpath+", c => c.PoolFoundersWarpath),
        new(0x76D4716B, 0xF527EF32, "Swordmaster's Warpath+", c => c.PoolSwordmastersWarpath),
        new(0xBFDF838C, 0xF527EF32, "Crimson's Warpath+", c => c.PoolCrimsonsWarpath),
        new(0xB3AB43F3, 0xF527EF32, "Ebony's Warpath+", c => c.PoolEbonysWarpath),
        new(0x9F72BAE0, 0xF527EF32, "Spirit Edge's Warpath+", c => c.PoolSpiritEdgesWarpath),
        new(0xAD8CAEFB, 0xF527EF32, "Dark Huntress's Warpath+", c => c.PoolDarkHuntresssWarpath),
        new(0x5D592FDD, 0xF527EF32, "Supreme Primarch's Warpath+", c => c.PoolSupremePrimarchsWarpath),
        new(0x98E9E6EF, 0xF527EF32, "Versalis Heart+", c => c.PoolVersalisHeart),
        new(0x41AC1082, 0xF527EF32, "Gladiator's Warpath+", c => c.PoolGladiatorsWarpath),
        new(0xEB766D87, 0xF527EF32, "Bladequeen's Warpath+", c => c.PoolBladequeensWarpath),
        new(0x51E98A7C, 0xF527EF32, "Ultramarine's Warpath+", c => c.PoolUltramarinesWarpath),
        new(0xD8C61507, 0xF527EF32, "Thunderwolf's Warpath+", c => c.PoolThunderwolfsWarpath),
        new(0x2D70C37D, 0xF527EF32, "Enchantress's Warpath+", c => c.PoolEnchantresssWarpath),
        new(0x9ABD2DA5, 0xF527EF32, "The Black's Warpath+", c => c.PoolTheBlacksWarpath),
        new(0x8B8085C0, 0x36879ED7, "Celestial Nyx V+", c => c.PoolCelestialNyxV),
        new(0x20492635, 0x36879ED7, "Celestial Lumen V+", c => c.PoolCelestialLumenV),
        new(0xD29CD8E0, 0x36879ED7, "Celestial Terra V+", c => c.PoolCelestialTerraV),
        new(0x74061B0C, 0x36879ED7, "Celestial Incendo V+", c => c.PoolCelestialIncendoV),
        new(0xE14E1598, 0x36879ED7, "Celestial Aqua V+", c => c.PoolCelestialAquaV),
        new(0x5BF84FD1, 0x36879ED7, "Fatebreaker V+", c => c.PoolFatebreakerV),
        new(0x9300FADB, 0x36879ED7, "Celestial Ventus V+", c => c.PoolCelestialVentusV),
        new(0x7B4AAB30, 0x36879ED7, "Divergence V+", c => c.PoolDivergenceV),
        new(0x00612B10, 0x6E52A69A, "War Elemental+", c => c.PoolWarElemental),
        new(0x035A4DDD, 0x6E52A69A, "Supplementary Damage V+", c => c.PoolSupplementaryDamageV),
        new(0x332E9B30, 0x6E52A69A, "Berserker Echo+", c => c.PoolBerserkerEcho),
        new(0x938DB625, 0x6E52A69A, "Spartan Echo+", c => c.PoolSpartanEcho),
        new(0x6F1D0870, 0x6E52A69A, "Greater Aegis V+", c => c.PoolGreaterAegisV),
    };
}
