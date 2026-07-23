using System.Buffers.Binary;

using Reloaded.Mod.Interfaces;

using gbfr.summon.drops.Configuration;
using gbfr.summon.drops.Template;

using gbfrelink.utility.manager.Interfaces;

namespace gbfr.summon.drops;

/// <summary>
/// Summon Drops Maxed — two launch-time table patches, both derived from the
/// vanilla tables via the GBFR Mod Manager (docs/23 decode):
///
/// 1. Guaranteed drops — reward_summon.tbl rows carry the per-source summon
///    drop chance (35/50/70/100%); set them all to 100.
/// 2. Max skill levels — summon_curve.tbl holds the weighted skill-LEVEL lots
///    the summon skill rolls draw from (e.g. Lv11–15 with Lv15 at 10%); pin
///    every lot to its highest level (top row weight 10000, rest 0).
///
/// Which summon drops (reward_summon_lot) and which skills roll (summon_lot)
/// stay vanilla. Layout guards refuse to patch if a game update changes a row
/// size. The mod ships no static tables.
/// </summary>
public class Mod : ModBase
{
    private readonly IModLoader _modLoader;
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private Config _configuration;

    private IDataManager _dataManager;

    private const int RewardSummonRowSize = 20; // Key, LotGroup, Chance, 1, 1
    private const int SummonCurveRowSize = 12;  // Group, SkillLevel, Weight
    private const int SummonRowSize = 36;       // Skill1LotA/B, Skill2LotA/B, Key, Species, Rarity(0x18), ...
    private const int SummonLotRowSize = 20;    // Group, Skill, Curve, Weight(0x0C), -1
    private const uint EmptyHash = 0x887AE0B0;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _logger = context.Logger;
        _modConfig = context.ModConfig;
        _configuration = context.Configuration;

        var controller = _modLoader.GetController<IDataManager>();
        if (controller is null || !controller.TryGetTarget(out _dataManager))
        {
            Log("ERROR: GBFR Mod Manager (IDataManager) not available — is 'gbfrelink.utility.manager' installed and enabled?");
            return;
        }
        if (!_dataManager.Initialized)
        {
            Log("ERROR: GBFR Mod Manager reports not initialized; cannot apply summon drop patches.");
            return;
        }

        Apply();
    }

    public override void ConfigurationUpdated(Config configuration)
    {
        _configuration = configuration;
        if (_dataManager is null)
            return;
        Apply();
        Log("Config changed — RESTART THE GAME for it to take effect (tables are read once, at launch).");
    }

    private void Apply()
    {
        ApplyGuaranteedDrops();
        ApplyMaxSkillLevels();
        ApplyBoostChaseSkills();
        _dataManager.UpdateIndex();
    }

    /// <summary>
    /// summon_lot.tbl: raise the throttled "chase" passive skill on 5-star summons.
    ///
    /// Every 5-star summon's slot-1 skill pool that has a chase skill follows one shape:
    /// exactly ONE entry with a uniquely-minimum weight (vanilla 800 = 8%), the rest
    /// higher and equal. We find each 5-star summon's slot-1 lot group (summon.tbl), and
    /// in summon_lot.tbl rewrite that group's minimum-weight entry so the chase skill
    /// takes ChaseSkillPercent of the pool — keeping the filler weights (and thus their
    /// relative odds) untouched. Flat pools (no unique minimum) are left alone. Verified
    /// against the vanilla tables: 19 groups match this shape, 0 are ambiguous
    /// (scripts/analyze-summon-chase-skills.mjs).
    /// </summary>
    private void ApplyBoostChaseSkills()
    {
        byte[] summon = _dataManager.GetArchiveFile("system/table/summon.tbl");
        byte[] lot = _dataManager.GetArchiveFile("system/table/summon_lot.tbl");
        if (summon is null || summon.Length < 8 || lot is null || lot.Length < 8)
        {
            Log("ERROR: could not read summon.tbl / summon_lot.tbl from the game archive.");
            return;
        }
        long summonRows = BinaryPrimitives.ReadInt64LittleEndian(summon);
        long lotRows = BinaryPrimitives.ReadInt64LittleEndian(lot);
        if (8 + summonRows * SummonRowSize != summon.Length ||
            8 + lotRows * SummonLotRowSize != lot.Length)
        {
            Log("ERROR: summon.tbl / summon_lot.tbl layout changed (game update?) — refusing to patch chase skills.");
            return;
        }
        if (!_configuration.BoostChaseSkills)
        {
            _dataManager.AddOrUpdateExternalFile("system/table/summon_lot.tbl", lot);
            Log("Boost chase skills OFF — vanilla summon_lot.tbl registered.");
            return;
        }
        int pct = Math.Clamp(_configuration.ChaseSkillPercent, 8, 100);

        // 1) collect the slot-1 lot groups belonging to 5-star summons
        var fiveStarGroups = new HashSet<uint>();
        for (long r = 0; r < summonRows; r++)
        {
            int off = (int)(8 + r * SummonRowSize);
            uint rarity = BinaryPrimitives.ReadUInt32LittleEndian(summon.AsSpan(off + 0x18, 4));
            if (rarity != 5) continue;
            uint a = BinaryPrimitives.ReadUInt32LittleEndian(summon.AsSpan(off + 0x00, 4)); // Skill1LotA
            uint b = BinaryPrimitives.ReadUInt32LittleEndian(summon.AsSpan(off + 0x04, 4)); // Skill1LotB
            uint group = a != EmptyHash ? a : b;
            if (group != EmptyHash) fiveStarGroups.Add(group);
        }

        // 2) index summon_lot rows by group (row index list)
        var rowsByGroup = new Dictionary<uint, List<long>>();
        for (long r = 0; r < lotRows; r++)
        {
            int off = (int)(8 + r * SummonLotRowSize);
            uint group = BinaryPrimitives.ReadUInt32LittleEndian(lot.AsSpan(off, 4));
            if (!fiveStarGroups.Contains(group)) continue;
            if (!rowsByGroup.TryGetValue(group, out var list)) rowsByGroup[group] = list = new List<long>();
            list.Add(r);
        }

        int WeightAt(long r) => BinaryPrimitives.ReadInt32LittleEndian(lot.AsSpan((int)(8 + r * SummonLotRowSize + 0x0C), 4));

        // 3) for each group, boost the unique-minimum (chase) entry to pct% of the pool
        int boosted = 0;
        foreach (var (group, rowList) in rowsByGroup)
        {
            if (rowList.Count < 2) continue;                       // single-entry pool: nothing to boost
            int min = int.MaxValue, max = int.MinValue;
            long chaseRow = -1; int minCount = 0;
            long fillersTotal = 0;
            foreach (var r in rowList)
            {
                int w = WeightAt(r);
                if (w < min) { min = w; chaseRow = r; minCount = 1; }
                else if (w == min) minCount++;
                if (w > max) max = w;
            }
            if (min == max || minCount != 1) continue;             // flat / ambiguous: leave vanilla

            void SetWeight(long r, int w) => BinaryPrimitives.WriteInt32LittleEndian(lot.AsSpan((int)(8 + r * SummonLotRowSize + 0x0C), 4), w);

            if (pct >= 100)
            {
                // true guarantee: zero every filler so only the chase skill can roll
                foreach (var r in rowList) if (r != chaseRow) SetWeight(r, 0);
                SetWeight(chaseRow, 10000);
            }
            else
            {
                foreach (var r in rowList) if (r != chaseRow) fillersTotal += WeightAt(r);
                // chase / (chase + fillersTotal) == pct/100  =>  chase = fillersTotal*pct/(100-pct)
                long newChase = Math.Clamp((long)Math.Round(fillersTotal * (double)pct / (100 - pct)), 1, int.MaxValue);
                SetWeight(chaseRow, (int)newChase);
            }
            boosted++;
        }

        _dataManager.AddOrUpdateExternalFile("system/table/summon_lot.tbl", lot);
        Log($"Chase skills boosted: {boosted} five-star pools set to ~{pct}% for their throttled skill " +
            $"(from {fiveStarGroups.Count} five-star slot-1 groups; flat pools left vanilla).");
    }

    /// <summary>reward_summon.tbl: per-source summon drop chance → 100%.</summary>
    private void ApplyGuaranteedDrops()
    {
        byte[] tbl = _dataManager.GetArchiveFile("system/table/reward_summon.tbl");
        if (tbl is null || tbl.Length < 8)
        {
            Log("ERROR: could not read reward_summon.tbl from the game archive.");
            return;
        }
        long rows = BinaryPrimitives.ReadInt64LittleEndian(tbl);
        if (8 + rows * RewardSummonRowSize != tbl.Length)
        {
            Log("ERROR: reward_summon.tbl layout changed (game update?) — refusing to patch drop chances.");
            return;
        }
        if (!_configuration.GuaranteedDrops)
        {
            // register the untouched vanilla table so a stale patched copy can't linger
            _dataManager.AddOrUpdateExternalFile("system/table/reward_summon.tbl", tbl);
            Log("Guaranteed drops OFF — vanilla reward_summon.tbl registered.");
            return;
        }
        int patched = 0;
        for (long r = 0; r < rows; r++)
        {
            var chance = tbl.AsSpan((int)(8 + r * RewardSummonRowSize + 0x08), 4);
            if (BinaryPrimitives.ReadInt32LittleEndian(chance) < 100)
            {
                BinaryPrimitives.WriteInt32LittleEndian(chance, 100);
                patched++;
            }
        }
        _dataManager.AddOrUpdateExternalFile("system/table/reward_summon.tbl", tbl);
        Log($"Guaranteed drops applied: {patched}/{rows} summon reward sources raised to 100% (rest were already 100%).");
    }

    /// <summary>summon_curve.tbl: every skill-level lot pinned to its highest level.</summary>
    private void ApplyMaxSkillLevels()
    {
        byte[] tbl = _dataManager.GetArchiveFile("system/table/summon_curve.tbl");
        if (tbl is null || tbl.Length < 8)
        {
            Log("ERROR: could not read summon_curve.tbl from the game archive.");
            return;
        }
        long rows = BinaryPrimitives.ReadInt64LittleEndian(tbl);
        if (8 + rows * SummonCurveRowSize != tbl.Length)
        {
            Log("ERROR: summon_curve.tbl layout changed (game update?) — refusing to patch skill levels.");
            return;
        }
        if (!_configuration.MaxSkillLevels)
        {
            _dataManager.AddOrUpdateExternalFile("system/table/summon_curve.tbl", tbl);
            Log("Max skill levels OFF — vanilla summon_curve.tbl registered.");
            return;
        }
        // find the max level per lot group, then weight: max row 10000, others 0
        var maxLevel = new Dictionary<uint, int>();
        for (long r = 0; r < rows; r++)
        {
            int off = (int)(8 + r * SummonCurveRowSize);
            uint group = BinaryPrimitives.ReadUInt32LittleEndian(tbl.AsSpan(off, 4));
            int level = BinaryPrimitives.ReadInt32LittleEndian(tbl.AsSpan(off + 4, 4));
            if (!maxLevel.TryGetValue(group, out int cur) || level > cur)
                maxLevel[group] = level;
        }
        int pinned = 0;
        for (long r = 0; r < rows; r++)
        {
            int off = (int)(8 + r * SummonCurveRowSize);
            uint group = BinaryPrimitives.ReadUInt32LittleEndian(tbl.AsSpan(off, 4));
            int level = BinaryPrimitives.ReadInt32LittleEndian(tbl.AsSpan(off + 4, 4));
            bool isMax = level == maxLevel[group];
            BinaryPrimitives.WriteInt32LittleEndian(tbl.AsSpan(off + 8, 4), isMax ? 10000 : 0);
            if (isMax) pinned++;
        }
        _dataManager.AddOrUpdateExternalFile("system/table/summon_curve.tbl", tbl);
        Log($"Max skill levels applied: {maxLevel.Count} level lots pinned to their top level ({rows} rows).");
    }

    private void Log(string message) => _logger.WriteLine($"[{_modConfig.ModId}] {message}");

    #region Standard Overrides / For Exports, Serialization etc.
#pragma warning disable CS8618
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}
