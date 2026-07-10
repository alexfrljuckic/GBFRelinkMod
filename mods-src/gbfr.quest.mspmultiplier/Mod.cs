using System.Buffers.Binary;

using Reloaded.Mod.Interfaces;

using gbfr.quest.mspmultiplier.Configuration;
using gbfr.quest.mspmultiplier.Template;

using gbfrelink.utility.manager.Interfaces;

namespace gbfr.quest.mspmultiplier;

/// <summary>
/// Multiplies the mastery points (MSP) granted by every quest clear.
///
/// At load time this reads the VANILLA reward tables from the game archive via the
/// GBFR Mod Manager, multiplies Min/Max on exactly the reward_point entries that are
/// referenced as MSP rewards (reward.tbl RewardPointIdMSP — these share zero keys
/// with EXP/gold entries), and registers the patched table. Robust to game data
/// updates as long as the two table layouts are unchanged (guarded below).
/// </summary>
public class Mod : ModBase
{
    private readonly IModLoader _modLoader;
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private Config _configuration;

    private IDataManager _dataManager;

    private const uint EmptyHash = 0x887AE0B0;
    private const int RewardRowSize = 52;       // 2.0 layout (docs/11)
    private const int RewardMspOffset = 0x24;   // RewardPointIdMSP
    private const int PointRowSize = 24;        // Key, RewardRank, StoryDifficulty, Min, Max, Weight
    private const int PointMinOffset = 0x0C;
    private const int PointMaxOffset = 0x10;

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
            Log("ERROR: GBFR Mod Manager reports not initialized; cannot apply MSP multiplier.");
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
        Log("Multiplier changed — RESTART THE GAME for it to take effect (tables are read once, at launch).");
    }

    private void Apply()
    {
        int mult = Math.Clamp(_configuration.Multiplier, 1, 100);

        byte[] reward = _dataManager.GetArchiveFile("system/table/reward.tbl");
        byte[] points = _dataManager.GetArchiveFile("system/table/reward_point.tbl");
        if (reward is null || reward.Length < 8 || points is null || points.Length < 8)
        {
            Log("ERROR: could not read reward tables from the game archive.");
            return;
        }

        // Layout guards: if a game update changes either row size, bail out loudly
        // instead of corrupting rewards.
        long rewardRows = BinaryPrimitives.ReadInt64LittleEndian(reward);
        long pointRows = BinaryPrimitives.ReadInt64LittleEndian(points);
        if (8 + rewardRows * RewardRowSize != reward.Length || 8 + pointRows * PointRowSize != points.Length)
        {
            Log($"ERROR: reward table layout changed (game update?) — refusing to patch. " +
                $"reward.tbl: {reward.Length} bytes / {rewardRows} rows; reward_point.tbl: {points.Length} bytes / {pointRows} rows.");
            return;
        }

        // 1) collect the MSP point keys referenced by quest rewards
        var mspKeys = new HashSet<uint>();
        for (long r = 0; r < rewardRows; r++)
        {
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(reward.AsSpan((int)(8 + r * RewardRowSize + RewardMspOffset), 4));
            if (key != EmptyHash && key != 0)
                mspKeys.Add(key);
        }

        // 2) multiply Min/Max on exactly those entries
        int patched = 0;
        for (long r = 0; r < pointRows; r++)
        {
            int rowOffset = (int)(8 + r * PointRowSize);
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(points.AsSpan(rowOffset, 4));
            if (!mspKeys.Contains(key))
                continue;

            var min = points.AsSpan(rowOffset + PointMinOffset, 4);
            var max = points.AsSpan(rowOffset + PointMaxOffset, 4);
            BinaryPrimitives.WriteUInt32LittleEndian(min, BinaryPrimitives.ReadUInt32LittleEndian(min) * (uint)mult);
            BinaryPrimitives.WriteUInt32LittleEndian(max, BinaryPrimitives.ReadUInt32LittleEndian(max) * (uint)mult);
            patched++;
        }

        // Always register (also at x1: overwrites any stale patched copy with vanilla values).
        _dataManager.AddOrUpdateExternalFile("system/table/reward_point.tbl", points);
        _dataManager.UpdateIndex();
        Log($"MSP x{mult} applied — {patched} reward_point entries patched ({mspKeys.Count} MSP reward keys).");
    }

    private void Log(string message) => _logger.WriteLine($"[{_modConfig.ModId}] {message}");

    #region Standard Overrides / For Exports, Serialization etc.
#pragma warning disable CS8618
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}
