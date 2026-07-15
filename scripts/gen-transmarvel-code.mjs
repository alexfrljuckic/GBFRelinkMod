// Regenerate the transmarvel-overhaul C# sources from skill-catalog.json
// (which scripts/gen-unified-skill-catalog.mjs derives from game data).
//
// Emits:
//   mods-src/gbfr.transmarvel.overhaul/Config.cs        (settings + per-skill toggles)
//   mods-src/gbfr.transmarvel.overhaul/SigilCatalog.cs  (primary toggles -> pool)
//   mods-src/gbfr.transmarvel.overhaul/TraitCatalog.cs  (secondary toggles -> 2nd-trait filter)
//
// Config organization mirrors the in-game skill list: 1. Basic Stats, 2. Attack,
// 3. Defense, 4. Support, 5. Special — each skill with a "(primary sigil)" and/or
// "(2nd trait)" toggle. Never hand-edit the generated sections; rerun this.
import { readFileSync, writeFileSync } from 'node:fs';

const CATALOG = JSON.parse(readFileSync('mods/transmarvel-overhaul/skill-catalog.json', 'utf8'));
const SRC = 'mods-src/gbfr.transmarvel.overhaul';

const catLabel = e => `${e.category + 1}. ${e.categoryName}`;
// C# string-literal escape for generated [DisplayName]/[Description] attributes
const esc = s => s.replace(/\\/g, '\\\\').replace(/"/g, '\\"');

function primaryDescription(e) {
  const p = e.primary;
  if (e.variant === 'Warpath+')
    return `${e.character ? e.character + "'s" : 'A'} character sigil. Keep ${p.displayName} in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.`;
  if (e.variant === 'Awakening+')
    return `${e.character ? e.character + "'s" : 'A'} character sigil. Include ${p.displayName} in the Transmarvel sigil pool (opt-in bucket, off by default). With auto-prune on, it leaves the pool as soon as you own ANY copy - duplicates are useless. RESTART THE GAME after changing.`;
  if (p.bucket === 'Awakening+') // stat V+ singles riding the Awakening bucket
    return `Roll ${p.displayName} as a Transmarvel main. Vanilla keeps this stat single (no 2nd trait slot) in the same chase bucket as the Awakening+ sigils, so it is opt-in like them, with the same own-any auto-prune. RESTART THE GAME after changing.`;
  if (p.addToPool)
    return `Roll ${p.displayName} as a Transmarvel main - a mod extension: vanilla Transmarvel never offers this sigil (the item itself is a vanilla V+ sigil obtainable elsewhere). It joins the pool at equal odds and gets a random 2nd trait from your ticked traits; with auto-prune on it leaves the pool once you own every rollable combo. RESTART THE GAME after changing.`;
  return `Keep ${p.displayName} in the Transmarvel sigil pool. Unticked = removed; remaining sigils stay at equal odds. Untick everything = vanilla pool (overhaul off). RESTART THE GAME after changing.`;
}

function secondaryDescription(e) {
  const note = e.secondary.legal ? '' :
    ' (Vanilla itself never rolls this on Transmarvel pulls - the mod\'s filter makes it rollable, a mod extension.)';
  return `Allow ${e.secondary.displayName} to roll as the random 2nd trait on '+' sigils.${note} Your ticked 2nd traits are the ONLY secondaries that can roll, from any source - tick several for variety (a single ticked trait means every sigil rolls that one trait). RESTART THE GAME after changing.`;
}

// ---------- Config.cs ----------
const head = `using System.ComponentModel;
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
    [Description("Transmarvel Vouchers granted on every eligible quest clear.\\n" +
        "0 = vanilla (no guaranteed vouchers), default 10.\\n" +
        "ENGINE LIMIT: the game clamps each reward grant to 5, and a reward row has\\n" +
        "at most 5 slots - so the real ceiling is 25 per clear, and quests whose\\n" +
        "reward rows have fewer free slots grant less. For BULK testing use\\n" +
        "'Transmarvel cost' = 1 instead (curio transmutes then fund 1-15 rolls each).\\n\\n" +
        "RESTART THE GAME after changing this - reward tables are read once, at launch.")]
    [DefaultValue(10)]
    public int VouchersPerClear { get; set; } = 10;

    [Category("Voucher Income")]
    [DisplayName("Transmarvel cost (points)")]
    [Description("Cost of one Transmarvel roll in Transmarvel points (vanilla: 150).\\n" +
        "Lower = cheaper rolls. MINIMUM 1 - a cost of 0 crashes the Transmarvel menu,\\n" +
        "so the mod clamps it to 1.\\n\\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(150)]
    public int TransmarvelCost { get; set; } = 150;

    [Category("Voucher Income")]
    [DisplayName("Voucher quests: minimum difficulty tier")]
    [Description("Lowest quest-board difficulty tier whose clears grant the vouchers.\\n" +
        "8 = Chaos and above (default). Lower it to farm vouchers on easier quests:\\n" +
        "  1 = everything from the earliest board quests (PWR 200+)\\n" +
        "  5 = PWR ~5k+, 6 = PWR ~8k+, 7 = PWR ~12k+ (Proud-era)\\n" +
        "Tiers above Chaos (Chaos+/Chaos++/Infinity) always grant.\\n\\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(8)]
    public int VoucherMinTier { get; set; } = 8;

    [Category("General")]
    [DisplayName("Wrightstone drops")]
    [Description("Which wrightstones the 25% wrightstone side of a Transmarvel can roll.\\n" +
        "All options guarantee a LEVEL 20 main skill:\\n" +
        "  0 = Transmarveled fixed subs: always Aegis 15 / ATK 10 (vanilla 0.1% stones; default)\\n" +
        "  1 = RANDOM subs at Lv15/Lv10 - the sub skills roll from your ticked 2nd traits\\n" +
        "      (vanilla high-tier stones; the mod's trait filter applies to them)\\n" +
        "  2 = both, 50/50\\n\\n" +
        "RESTART THE GAME after changing this.")]
    [DefaultValue(0)]
    public int WrightstoneDrops { get; set; } = 0;

    [Category("General")]
    [DisplayName("Auto-remove completed sigils (reads your save)")]
    [Description("At every game launch the mod reads your newest save (read-only) and\\n" +
        "removes from the Transmarvel pool: any TICKED Warpath+ or mod-added V+ sigil\\n" +
        "you already own with EVERY 2nd trait it can still roll (your ticked traits),\\n" +
        "and any TICKED Awakening+/stat V+ you own AT ALL (duplicates are useless).\\n" +
        "Pruned sigils regain pool slots automatically when you tick more traits\\n" +
        "(new combos to chase). Unticked sigils stay out regardless. If the save\\n" +
        "can't be read the pool is left unpruned (see the Reloaded console).")]
    [DefaultValue(true)]
    public bool AutoPruneCompleted { get; set; } = true;
`;

const parts = [head];
for (const e of CATALOG) {
  if (e.primary) {
    parts.push(`
    [Category("${catLabel(e)}")]
    [DisplayName("${esc(e.primary.displayName)} (primary sigil)")]
    [Description("${esc(primaryDescription(e))}")]
    [DefaultValue(${e.primary.default})]
    public bool ${e.primary.prop} { get; set; } = ${e.primary.default};
`);
  }
  if (e.secondary) {
    parts.push(`
    [Category("${catLabel(e)}")]
    [DisplayName("${esc(e.secondary.displayName)} (2nd trait)")]
    [Description("${esc(secondaryDescription(e))}")]
    [DefaultValue(${e.secondary.default})]
    public bool ${e.secondary.prop} { get; set; } = ${e.secondary.default};
`);
  }
}
parts.push(`}

/// <summary>
/// Allows overriding aspects of the configuration creation process.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
}
`);
writeFileSync(`${SRC}/Config.cs`, parts.join(''));

// ---------- SigilCatalog.cs ----------
const sigils = CATALOG.filter(e => e.primary).map(e => {
  const p = e.primary;
  return `        new(${p.hash}, ${p.bucketKey}, "${p.displayName}", ${p.addToPool}, c => c.${p.prop}),`;
});
writeFileSync(`${SRC}/SigilCatalog.cs`, `using gbfr.transmarvel.overhaul.Configuration;

namespace gbfr.transmarvel.overhaul;

// GENERATED from game data (scripts/gen-transmarvel-code.mjs) - do not hand-edit.
// BucketKey = the gacha_lot Key of the chase bucket the sigil sits in (or joins).
// AddToPool = vanilla Transmarvel has no gacha_lot row for it; Mod.cs appends one.
public static class SigilCatalog
{
    public record Sigil(uint Hash, uint BucketKey, string Name, bool AddToPool, Func<Config, bool> Enabled);

    public static readonly Sigil[] Pool =
    {
${sigils.join('\n')}
    };
}
`);

// ---------- TraitCatalog.cs ----------
const traits = CATALOG.filter(e => e.secondary).map(e =>
  `        new(${e.secondary.hash}, "${e.secondary.displayName}", c => c.${e.secondary.prop}),`);
writeFileSync(`${SRC}/TraitCatalog.cs`, `using gbfr.transmarvel.overhaul.Configuration;

namespace gbfr.transmarvel.overhaul;

// GENERATED from game data (scripts/gen-transmarvel-code.mjs) - do not hand-edit.
public static class TraitCatalog
{
    public record Trait(uint Hash, string Name, Func<Config, bool> Enabled);

    public static readonly Trait[] All =
    {
${traits.join('\n')}
    };
}
`);

console.log(`Config.cs: ${CATALOG.filter(e => e.primary).length} primary + ${CATALOG.filter(e => e.secondary).length} secondary toggles`);
console.log(`SigilCatalog.cs: ${sigils.length} sigils (${CATALOG.filter(e => e.primary?.addToPool).length} mod-added)`);
console.log(`TraitCatalog.cs: ${traits.length} traits`);
