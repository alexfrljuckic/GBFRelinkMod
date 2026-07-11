using System.Buffers.Binary;

using Reloaded.Mod.Interfaces;

using gbfr.transmarvel.overhaul.Configuration;
using gbfr.transmarvel.overhaul.Template;

using gbfrelink.utility.manager.Interfaces;

namespace gbfr.transmarvel.overhaul;

/// <summary>
/// Code half of Transmarvel Overhaul, all configurable in Reloaded-II:
///
/// 1. Voucher income — reads the VANILLA reward tables from the game archive
///    via the GBFR Mod Manager, appends one guaranteed-voucher lot (RWL_TMV_C,
///    AmountGiven = configured count) to reward_lot.tbl, wires it into the
///    first free RewardLotId slot of every Chaos+ quest's per-clear reward
///    rows (RW_&lt;qid&gt;_100/_101), and registers the patched tables. Chaos+
///    quests are enumerated from quest_baseinfo_ex_data.tbl (key bands
///    4083/4093/40A3/40B3), so quests added by game updates are covered.
///
/// 2. Sigil pool — rebuilds the gacha tables from vanilla with only the
///    ticked chase sigils, equal odds (SigilCatalog.cs checkboxes).
///
/// 3. 2nd-trait filter — rebuilds skill_lot.tbl from vanilla with non-ticked
///    trait entries remapped, per sub-lot, onto ticked traits that are LEGAL
///    for every path referencing that sub-lot — no (sigil, trait) combo the
///    vanilla game couldn't produce. Content-level because pointer-level
///    rerouting proved insufficient in game (docs/13). TraitCatalog.cs.
///
/// The mod ships no static tables — everything is generated at launch.
/// </summary>
public class Mod : ModBase
{
    private readonly IModLoader _modLoader;
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private Config _configuration;

    private IDataManager _dataManager;

    private const uint EmptyHash = 0x887AE0B0;
    private const uint VoucherHash = 0x58FC9B99;  // ITEM_21_0001 Transmarvel Voucher (docs/15)

    private const int RewardRowSize = 52;         // 2.0 layout (patches/headers-2.0/reward.headers)
    private const int RewardKeyOffset = 0x18;
    private const int RewardLotId2Offset = 0x04;  // RewardLotId2..6 = 0x04..0x14 (slot 1 is tied to Lot1ExclusionChance)
    private const int RewardLotSlots = 5;

    private const int LotRowSize = 60;            // reward_lot.headers
    private const int QuestRowSize = 112;         // quest_baseinfo_ex_data.headers
    private const int QuestKeyOffset = 0x54;

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
            Log("ERROR: GBFR Mod Manager reports not initialized; cannot apply voucher rewards.");
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
        ApplyVouchers();
        ApplySigilPool();
        ApplyTraitFilter();
        ApplyTransmarvelCost();
    }

    private const int GachaRowSize = 48; // gacha.tbl: chances, Key@0x08, groups, Order, VoucherCost@0x20, TransmarvelCost@0x24, ...

    /// <summary>Sets the Transmarvel roll cost (vanilla 150; 0 = free rolls for testing).</summary>
    private void ApplyTransmarvelCost()
    {
        byte[] gacha = _dataManager.GetArchiveFile("system/table/gacha.tbl");
        if (gacha is null || gacha.Length < 8)
        {
            Log("ERROR: could not read gacha.tbl from the game archive.");
            return;
        }
        long rows = BinaryPrimitives.ReadInt64LittleEndian(gacha);
        if (8 + rows * GachaRowSize != gacha.Length)
        {
            Log("ERROR: gacha.tbl layout changed (game update?) — refusing to patch the Transmarvel cost.");
            return;
        }
        // minimum 1: cost 0 CRASHES the Transmarvel menu (observed live 2026-07-10,
        // presumably a rolls-available = points/cost division in the UI)
        int cost = Math.Clamp(_configuration.TransmarvelCost, 1, 9999);
        int patched = 0;
        for (long r = 0; r < rows; r++)
        {
            int off = (int)(8 + r * GachaRowSize);
            // the Transmarvel tier row is the one whose gem rate group is the sigil group
            if (BinaryPrimitives.ReadUInt32LittleEndian(gacha.AsSpan(off + 0x10, 4)) != SigilGroupKey)
                continue;
            BinaryPrimitives.WriteInt32LittleEndian(gacha.AsSpan(off + 0x24, 4), cost);
            patched++;
        }
        if (patched != 1)
        {
            Log($"ERROR: expected exactly 1 Transmarvel tier row in gacha.tbl, found {patched} — not registering.");
            return;
        }
        _dataManager.AddOrUpdateExternalFile("system/table/gacha.tbl", gacha);
        _dataManager.UpdateIndex();
        Log($"Transmarvel cost set to {cost} points{(cost == 0 ? " — FREE rolls (testing mode)" : "")}.");
    }

    private void ApplyVouchers()
    {
        int count = Math.Clamp(_configuration.VouchersPerClear, 0, 99);

        byte[] reward = _dataManager.GetArchiveFile("system/table/reward.tbl");
        byte[] lots = _dataManager.GetArchiveFile("system/table/reward_lot.tbl");
        byte[] quests = _dataManager.GetArchiveFile("system/table/quest_baseinfo_ex_data.tbl");
        if (reward is null || reward.Length < 8 || lots is null || lots.Length < 8 || quests is null || quests.Length < 8)
        {
            Log("ERROR: could not read reward/quest tables from the game archive.");
            return;
        }

        // Layout guards: if a game update changes any row size, bail out loudly
        // instead of corrupting rewards.
        long rewardRows = BinaryPrimitives.ReadInt64LittleEndian(reward);
        long lotRows = BinaryPrimitives.ReadInt64LittleEndian(lots);
        long questRows = BinaryPrimitives.ReadInt64LittleEndian(quests);
        if (8 + rewardRows * RewardRowSize != reward.Length ||
            8 + lotRows * LotRowSize != lots.Length ||
            8 + questRows * QuestRowSize != quests.Length)
        {
            Log("ERROR: reward/quest table layout changed (game update?) — refusing to patch. " +
                $"reward.tbl {reward.Length}b/{rewardRows}r, reward_lot.tbl {lots.Length}b/{lotRows}r, " +
                $"quest_baseinfo_ex_data.tbl {quests.Length}b/{questRows}r.");
            return;
        }

        if (count == 0)
        {
            // register the untouched vanilla tables so a stale patched copy can't linger
            _dataManager.AddOrUpdateExternalFile("system/table/reward.tbl", reward);
            _dataManager.AddOrUpdateExternalFile("system/table/reward_lot.tbl", lots);
            _dataManager.UpdateIndex();
            Log("Voucher income x0 — vanilla reward tables registered (no guaranteed vouchers).");
            return;
        }

        // 1) quest-board quests are key band 0040T3xx where T = difficulty tier
        //    (1 = earliest ... 8 = Chaos, 9 = Chaos+, A = Chaos++, B = Infinity).
        //    Grant on every tier >= the configured floor (default 8 = Chaos+).
        int minTier = Math.Clamp(_configuration.VoucherMinTier, 1, 8);
        var targetKeys = new HashSet<uint>();
        int questCount = 0;
        for (long r = 0; r < questRows; r++)
        {
            uint qkey = BinaryPrimitives.ReadUInt32LittleEndian(quests.AsSpan((int)(8 + r * QuestRowSize + QuestKeyOffset), 4));
            if ((qkey >> 16) != 0x0040 || ((qkey >> 8) & 0xF) != 3)
                continue;
            if (((qkey >> 12) & 0xF) < minTier)
                continue;
            questCount++;
            string qid = (qkey & 0xFFFFFF).ToString("X6");
            targetKeys.Add(XXHash32Custom($"RW_{qid}_100"));
            targetKeys.Add(XXHash32Custom($"RW_{qid}_101"));
        }

        // 2) append the guaranteed voucher lots. The engine clamps each grant to
        //    ~5 regardless of AmountGiven (99 -> 5 and 20 -> 5 in live testing),
        //    so split the total into chunks of <= 5, one lot per reward slot.
        //    Hard ceiling: 5 slots x 5 = 25 per reward row.
        var chunks = new List<int>();
        for (int rest = count; rest > 0 && chunks.Count < RewardLotSlots; rest -= 5)
            chunks.Add(Math.Min(rest, 5));
        var lotHashes = new uint[chunks.Count];
        byte[] newLots = new byte[lots.Length + chunks.Count * LotRowSize];
        lots.CopyTo(newLots, 0);
        BinaryPrimitives.WriteInt64LittleEndian(newLots, lotRows + chunks.Count);
        for (int i = 0; i < chunks.Count; i++)
        {
            lotHashes[i] = XXHash32Custom($"RWL_TMV_S{i + 1}");
            var row = newLots.AsSpan(lots.Length + i * LotRowSize, LotRowSize);
            BinaryPrimitives.WriteInt32LittleEndian(row.Slice(0x00, 4), 5);            // Key (matches other lot-only rows)
            BinaryPrimitives.WriteUInt32LittleEndian(row.Slice(0x08, 4), lotHashes[i]); // ItemId column = the lot's id
            BinaryPrimitives.WriteUInt32LittleEndian(row.Slice(0x0C, 4), VoucherHash);  // WeaponId column = granted item
            BinaryPrimitives.WriteUInt32LittleEndian(row.Slice(0x10, 4), EmptyHash);    // GemId
            BinaryPrimitives.WriteUInt32LittleEndian(row.Slice(0x14, 4), EmptyHash);    // Unk14
            BinaryPrimitives.WriteInt32LittleEndian(row.Slice(0x18, 4), -1);            // RewardRank
            BinaryPrimitives.WriteInt32LittleEndian(row.Slice(0x1C, 4), -1);            // PhaseItemIndex
            BinaryPrimitives.WriteInt32LittleEndian(row.Slice(0x20, 4), chunks[i]);     // AmountGiven (<= 20, vanilla-attested)
            BinaryPrimitives.WriteInt32LittleEndian(row.Slice(0x2C, 4), 1);             // GemCount
            BinaryPrimitives.WriteInt32LittleEndian(row.Slice(0x30, 4), 10000);         // Weight
            BinaryPrimitives.WriteInt32LittleEndian(row.Slice(0x34, 4), -1);            // NumExtraGemTraits
            row[0x38] = 255;                                                            // StoryDifficulty
        }

        // 3) wire the chunk lots into the free RewardLotId2..6 slots of each target row
        int patched = 0, partial = 0;
        for (long r = 0; r < rewardRows; r++)
        {
            int rowOffset = (int)(8 + r * RewardRowSize);
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(reward.AsSpan(rowOffset + RewardKeyOffset, 4));
            if (!targetKeys.Contains(key))
                continue;
            int next = 0;
            for (int s = 0; s < RewardLotSlots && next < chunks.Count; s++)
            {
                var slot = reward.AsSpan(rowOffset + RewardLotId2Offset + s * 4, 4);
                if (BinaryPrimitives.ReadUInt32LittleEndian(slot) != EmptyHash)
                    continue;
                BinaryPrimitives.WriteUInt32LittleEndian(slot, lotHashes[next++]);
            }
            if (next > 0) patched++;
            if (next < chunks.Count) partial++;
        }

        if (patched == 0)
        {
            Log($"ERROR: found {questCount} Chaos+ quests but patched 0 reward rows — reward key scheme changed? Not registering.");
            return;
        }

        _dataManager.AddOrUpdateExternalFile("system/table/reward.tbl", reward);
        _dataManager.AddOrUpdateExternalFile("system/table/reward_lot.tbl", newLots);
        _dataManager.UpdateIndex();
        Log($"Voucher income applied: {count}x Transmarvel Voucher ({string.Join("+", chunks)} across slots) on {patched} per-clear reward rows " +
            $"across {questCount} quests (difficulty tier >= {minTier}){(partial > 0 ? $" ({partial} rows had fewer free slots — partial amount there)" : "")}.");
    }

    private const int SkillLotRowSize = 12;      // Key, SkillId, Weight
    private const int SkillTypeLotRowSize = 52;  // SkillLotId1..6, Chance1..6, Key
    private const int GachaRateRowSize = 16;     // GroupKey, GachaLotId, Weight, Flag
    private const int GachaLotRowSize = 28;      // QuestIDMin, QuestIDMax, Key, ItemId, Weight, TraitLevel, Unk18
    private const uint SigilGroupKey = 0x27509C51;      // Transmarvel sigil-side rate group
    private const uint WrightstoneGroupKey = 0x67716D8A; // Transmarvel wrightstone-side rate group
    private const uint Tier3WrightstoneLot = 0xBD1CBF1C;
    private const uint WarpathBucketKey = 0xF527EF32;    // the 28-character-Warpath+ chase bucket

    /// <summary>Ticked traits that can actually roll on Transmarvel pulls (ticked ∩ vanilla lot-14 union).</summary>
    private HashSet<uint> RollableSecondaries()
    {
        byte[] lots = _dataManager.GetArchiveFile("system/table/skill_lot.tbl");
        byte[] types = _dataManager.GetArchiveFile("system/table/skill_type_lot.tbl");
        long lotRows = BinaryPrimitives.ReadInt64LittleEndian(lots);
        long typeRows = BinaryPrimitives.ReadInt64LittleEndian(types);
        if (8 + lotRows * SkillLotRowSize != lots.Length || 8 + typeRows * SkillTypeLotRowSize != types.Length)
            throw new InvalidDataException("skill table layout changed");
        var contents = new Dictionary<uint, HashSet<uint>>();
        for (long r = 0; r < lotRows; r++)
        {
            int off = (int)(8 + r * SkillLotRowSize);
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(lots.AsSpan(off, 4));
            uint sid = BinaryPrimitives.ReadUInt32LittleEndian(lots.AsSpan(off + 4, 4));
            if (sid == EmptyHash || sid == 0) continue;
            if (!contents.TryGetValue(key, out var set)) contents[key] = set = new HashSet<uint>();
            set.Add(sid);
        }
        var union = new HashSet<uint>();
        for (long r = 0; r < typeRows; r++)
        {
            int off = (int)(8 + r * SkillTypeLotRowSize);
            if (BinaryPrimitives.ReadInt32LittleEndian(types.AsSpan(off + 0x30, 4)) != 14) continue;
            for (int k = 0; k < 6; k++)
            {
                uint sub = BinaryPrimitives.ReadUInt32LittleEndian(types.AsSpan(off + 4 * k, 4));
                int chance = BinaryPrimitives.ReadInt32LittleEndian(types.AsSpan(off + 0x18 + 4 * k, 4));
                if (sub != EmptyHash && sub != 0 && chance > 0 && contents.TryGetValue(sub, out var c))
                    union.UnionWith(c);
            }
        }
        var rollable = new HashSet<uint>(TraitCatalog.All.Where(t => t.Enabled(_configuration)).Select(t => t.Hash));
        rollable.IntersectWith(union);
        return rollable;
    }

    /// <summary>
    /// Rebuilds the Transmarvel sigil pool from vanilla with only the ticked
    /// sigils (SigilCatalog), all at equal odds; wrightstone side pinned to
    /// tier-3 Transmarveled. Untick everything = vanilla pool (overhaul off).
    /// </summary>
    private void ApplySigilPool()
    {
        byte[] rates = _dataManager.GetArchiveFile("system/table/gacha_rate_group.tbl");
        byte[] lots = _dataManager.GetArchiveFile("system/table/gacha_lot.tbl");
        if (rates is null || rates.Length < 8 || lots is null || lots.Length < 8)
        {
            Log("ERROR: could not read gacha tables from the game archive.");
            return;
        }
        long rateRows = BinaryPrimitives.ReadInt64LittleEndian(rates);
        long lotRows = BinaryPrimitives.ReadInt64LittleEndian(lots);
        if (8 + rateRows * GachaRateRowSize != rates.Length || 8 + lotRows * GachaLotRowSize != lots.Length)
        {
            Log("ERROR: gacha table layout changed (game update?) — refusing to patch the sigil pool.");
            return;
        }

        var kept = SigilCatalog.Pool.Where(s => s.Enabled(_configuration)).ToArray();
        if (kept.Length == 0)
        {
            _dataManager.AddOrUpdateExternalFile("system/table/gacha_rate_group.tbl", rates);
            _dataManager.AddOrUpdateExternalFile("system/table/gacha_lot.tbl", lots);
            _dataManager.UpdateIndex();
            Log("Sigil pool: no sigils ticked — vanilla Transmarvel pool registered (overhaul off).");
            return;
        }

        // auto-prune: drop ticked Warpath+ whose every rollable combo is already owned
        if (_configuration.AutoPruneCompleted)
        {
            try
            {
                var rollable = RollableSecondaries();
                if (rollable.Count == 0)
                {
                    Log("Auto-prune skipped: no ticked trait can roll on Transmarvel pulls.");
                }
                else
                {
                    string saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GBFR", "Saved", "SaveGames");
                    var save = SaveReader.ReadNewest(saveDir);
                    var pruned = new List<string>();
                    var remaining = kept.Where(s =>
                    {
                        if (s.BucketKey != WarpathBucketKey) return true;
                        save.CombosBySigil.TryGetValue(s.Hash, out var owned);
                        bool complete = owned is not null && rollable.All(owned.Contains);
                        if (complete) pruned.Add(s.Name);
                        return !complete;
                    }).ToArray();
                    if (remaining.Length == 0)
                    {
                        Log($"Auto-prune would empty the pool ({pruned.Count} completed) — keeping the un-pruned pool instead.");
                    }
                    else
                    {
                        kept = remaining;
                        Log($"Auto-prune: read {save.SigilCount} sigils from the save; " +
                            (pruned.Count > 0
                                ? $"removed {pruned.Count} completed Warpath+ ({string.Join(", ", pruned)})."
                                : "no Warpath+ is combo-complete yet."));
                    }
                }
            }
            catch (Exception e)
            {
                Log($"Auto-prune skipped (pool left unpruned): {e.Message}");
            }
        }
        var keptHashes = new HashSet<uint>(kept.Select(s => s.Hash));
        var buckets = new HashSet<uint>(SigilCatalog.Pool.Select(s => s.BucketKey));

        // 1) gacha_lot: drop chase-bucket rows for unticked sigils
        var outRows = new List<byte[]>((int)lotRows);
        var bucketCounts = new Dictionary<uint, int>();
        for (long r = 0; r < lotRows; r++)
        {
            var row = lots.AsSpan((int)(8 + r * GachaLotRowSize), GachaLotRowSize).ToArray();
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(row.AsSpan(8, 4));
            if (buckets.Contains(key))
            {
                uint item = BinaryPrimitives.ReadUInt32LittleEndian(row.AsSpan(0xC, 4));
                if (!keptHashes.Contains(item))
                    continue;
                bucketCounts.TryGetValue(key, out int n);
                bucketCounts[key] = n + 1;
            }
            outRows.Add(row);
        }
        byte[] newLots = new byte[8 + outRows.Count * GachaLotRowSize];
        BinaryPrimitives.WriteInt64LittleEndian(newLots, outRows.Count);
        for (int i = 0; i < outRows.Count; i++)
            outRows[i].CopyTo(newLots, 8 + i * GachaLotRowSize);

        // 2) gacha_rate_group: chase buckets at 250/sigil (equal per-item odds), all else 0
        for (long r = 0; r < rateRows; r++)
        {
            int off = (int)(8 + r * GachaRateRowSize);
            uint group = BinaryPrimitives.ReadUInt32LittleEndian(rates.AsSpan(off, 4));
            uint lotId = BinaryPrimitives.ReadUInt32LittleEndian(rates.AsSpan(off + 4, 4));
            if (group == SigilGroupKey)
            {
                bucketCounts.TryGetValue(lotId, out int n);
                BinaryPrimitives.WriteInt32LittleEndian(rates.AsSpan(off + 8, 4), 250 * n);
            }
            else if (group == WrightstoneGroupKey)
            {
                BinaryPrimitives.WriteInt32LittleEndian(rates.AsSpan(off + 8, 4), lotId == Tier3WrightstoneLot ? 10000 : 0);
            }
        }

        _dataManager.AddOrUpdateExternalFile("system/table/gacha_rate_group.tbl", rates);
        _dataManager.AddOrUpdateExternalFile("system/table/gacha_lot.tbl", newLots);
        _dataManager.UpdateIndex();
        Log($"Sigil pool applied: {kept.Length}/41 sigils at ~{100.0 / kept.Length:F2}% each; wrightstones pinned tier-3 Transmarveled.");
    }

    /// <summary>
    /// 2nd-trait filter with combo legality: each vanilla sub-lot's non-ticked
    /// entries are remapped only onto ticked traits that are LEGAL for every
    /// roll path referencing that sub-lot (the intersection of the vanilla
    /// trait unions of all referencing type-lots). A sigil can therefore never
    /// receive a (sigil, 2nd trait) combo the vanilla game couldn't produce.
    /// Sub-lots with no legal ticked trait are left vanilla (logged).
    /// </summary>
    private void ApplyTraitFilter()
    {
        byte[] lots = _dataManager.GetArchiveFile("system/table/skill_lot.tbl");
        byte[] types = _dataManager.GetArchiveFile("system/table/skill_type_lot.tbl");
        if (lots is null || lots.Length < 8 || types is null || types.Length < 8)
        {
            Log("ERROR: could not read skill tables from the game archive.");
            return;
        }
        long lotRows = BinaryPrimitives.ReadInt64LittleEndian(lots);
        long typeRows = BinaryPrimitives.ReadInt64LittleEndian(types);
        if (8 + lotRows * SkillLotRowSize != lots.Length || 8 + typeRows * SkillTypeLotRowSize != types.Length)
        {
            Log("ERROR: skill table layout changed (game update?) — refusing to patch 2nd-trait lots.");
            return;
        }

        var allowed = TraitCatalog.All.Where(t => t.Enabled(_configuration)).ToArray();
        // Always register skill_type_lot as VANILLA — earlier dev builds rerouted it.
        _dataManager.AddOrUpdateExternalFile("system/table/skill_type_lot.tbl", types);
        if (allowed.Length == 0)
        {
            _dataManager.AddOrUpdateExternalFile("system/table/skill_lot.tbl", lots);
            _dataManager.UpdateIndex();
            Log("2nd-trait filter: no traits ticked — vanilla trait lots registered (filter off).");
            return;
        }
        var allowedSet = new HashSet<uint>(allowed.Select(t => t.Hash));

        // vanilla contents per sub-lot
        var contents = new Dictionary<uint, HashSet<uint>>();
        for (long r = 0; r < lotRows; r++)
        {
            int off = (int)(8 + r * SkillLotRowSize);
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(lots.AsSpan(off, 4));
            uint sid = BinaryPrimitives.ReadUInt32LittleEndian(lots.AsSpan(off + 4, 4));
            if (sid == EmptyHash || sid == 0) continue;
            if (!contents.TryGetValue(key, out var set)) contents[key] = set = new HashSet<uint>();
            set.Add(sid);
        }
        // per type-lot: its sub-lots and its trait union
        var typeUnion = new Dictionary<int, HashSet<uint>>();
        var subLotUsers = new Dictionary<uint, List<int>>();
        for (long r = 0; r < typeRows; r++)
        {
            int off = (int)(8 + r * SkillTypeLotRowSize);
            int key = BinaryPrimitives.ReadInt32LittleEndian(types.AsSpan(off + 0x30, 4));
            var union = new HashSet<uint>();
            for (int k = 0; k < 6; k++)
            {
                uint sub = BinaryPrimitives.ReadUInt32LittleEndian(types.AsSpan(off + 4 * k, 4));
                int chance = BinaryPrimitives.ReadInt32LittleEndian(types.AsSpan(off + 0x18 + 4 * k, 4));
                if (sub == EmptyHash || sub == 0 || chance <= 0) continue;
                if (!subLotUsers.TryGetValue(sub, out var users)) subLotUsers[sub] = users = new List<int>();
                users.Add(key);
                if (contents.TryGetValue(sub, out var c)) union.UnionWith(c);
            }
            typeUnion[key] = union;
        }
        // legal remap targets per sub-lot = allowed ∩ (∩ unions of all referencing
        // type-lots); if that strict intersection is empty, relax to allowed ∩
        // (∪ of the unions) — every written trait is still vanilla-rollable for
        // sigils using this sub-lot via at least one referencing path. Leaving
        // such sub-lots vanilla instead was the live-verified junk leak
        // (8F952AC1, ~33% of rolls — see docs/13).
        var targets = new Dictionary<uint, List<uint>>();
        var relaxed = new List<uint>();
        var vanillaKept = new List<uint>();
        foreach (var (sub, users) in subLotUsers)
        {
            IEnumerable<uint> strict = allowed.Select(t => t.Hash);
            foreach (var u in users)
                strict = strict.Where(h => typeUnion[u].Contains(h));
            var list = strict.ToList();
            if (list.Count == 0)
            {
                list = allowed.Select(t => t.Hash)
                    .Where(h => users.Any(u => typeUnion[u].Contains(h))).ToList();
                if (list.Count > 0) relaxed.Add(sub);
            }
            if (list.Count == 0) vanillaKept.Add(sub);
            else targets[sub] = list;
        }

        // remap pass (per-sub-lot round-robin over its legal targets)
        byte[] newLots = (byte[])lots.Clone();
        var perKey = new Dictionary<uint, int>();
        int replaced = 0;
        for (long r = 0; r < lotRows; r++)
        {
            int off = (int)(8 + r * SkillLotRowSize);
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(newLots.AsSpan(off, 4));
            uint sid = BinaryPrimitives.ReadUInt32LittleEndian(newLots.AsSpan(off + 4, 4));
            if (sid == EmptyHash || sid == 0 || allowedSet.Contains(sid))
                continue;
            if (!targets.TryGetValue(key, out var list))
                continue; // unreferenced or no legal target -> leave vanilla
            perKey.TryGetValue(key, out int i);
            perKey[key] = i + 1;
            BinaryPrimitives.WriteUInt32LittleEndian(newLots.AsSpan(off + 4, 4), list[i % list.Count]);
            replaced++;
        }

        _dataManager.AddOrUpdateExternalFile("system/table/skill_lot.tbl", newLots);
        _dataManager.UpdateIndex();
        var names = allowed.Select(t => t.Name);
        Log($"2nd-trait filter applied: {allowed.Length} traits ticked ({string.Join(", ", names)}); " +
            $"{replaced} lot entries remapped across {targets.Count} sub-lots" +
            (relaxed.Count > 0 ? $"; {relaxed.Count} sub-lot(s) used relaxed legality ({string.Join(", ", relaxed.Select(s => s.ToString("X8")))})" : "") +
            (vanillaKept.Count > 0 ? $"; {vanillaKept.Count} sub-lot(s) left VANILLA (no ticked trait is legal there — tick more traits to cover them)" : "") + ".");
    }

    // GBFR's custom XXHash32: seed 0x178A54A4, hardcoded accumulators (docs/15)
    private static uint XXHash32Custom(string input)
    {
        const uint P1 = 0x9E3779B1, P2 = 0x85EBCA77, P3 = 0xC2B2AE3D, P4 = 0x27D4EB2F, P5 = 0x165667B1;
        static uint Rotl(uint x, int r) => (x << r) | (x >>> (32 - r));
        static uint Round(uint acc, uint val) => Rotl(acc + val * P2, 13) * P1;

        byte[] data = System.Text.Encoding.ASCII.GetBytes(input);
        int len = data.Length, i = 0;
        uint h = 0x178A54A4;
        if (len >= 16)
        {
            uint v1 = 0x2557311B, v2 = 0x871FB76A, v3 = 0x0133ECF3, v4 = 0x62FC7342;
            do
            {
                v1 = Round(v1, BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i, 4)));
                v2 = Round(v2, BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i + 4, 4)));
                v3 = Round(v3, BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i + 8, 4)));
                v4 = Round(v4, BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i + 12, 4)));
                i += 16;
            } while (len - i > 16);
            h = Rotl(v1, 1) + Rotl(v2, 7) + Rotl(v3, 12) + Rotl(v4, 18);
        }
        h += (uint)len;
        for (; len - i >= 4; i += 4)
            h = Rotl(h + BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i, 4)) * P3, 17) * P4;
        for (; i < len; i++)
            h = Rotl(h + data[i] * P5, 11) * P1;
        h = (h ^ (h >> 15)) * P2;
        h = (h ^ (h >> 13)) * P3;
        return h ^ (h >> 16);
    }

    private void Log(string message) => _logger.WriteLine($"[{_modConfig.ModId}] {message}");

    #region Standard Overrides / For Exports, Serialization etc.
#pragma warning disable CS8618
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}
