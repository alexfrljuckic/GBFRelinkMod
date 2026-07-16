using System.Buffers.Binary;

using Reloaded.Mod.Interfaces;

using gbfr.shop.voucherbadges.Configuration;
using gbfr.shop.voucherbadges.Template;

using gbfrelink.utility.manager.Interfaces;

namespace gbfr.shop.voucherbadges;

/// <summary>
/// Badges, Spellbooks &amp; Centrums for Vouchers — sells items in the Knickknack
/// Shack's Treasure Trade tab for Knickknack Vouchers, unlimited stock, with
/// per-item costs configurable in Reloaded-II (v2: previously static tables).
///
/// Shop cost chain (docs/20): trade.tbl (ItemPurchasable, ItemTierMapId, unique
/// SubKey, Key=3 = Treasure Trade) → item_tier_map (MaterialId1 = recipe key) →
/// item_material_list (the cost recipe). Per item the mod appends one recipe,
/// one tier-map row, and one trade row to the VANILLA tables at launch (via the
/// GBFR Mod Manager) — nothing on disk is modified, and row-size guards refuse
/// to patch after layout-changing game updates. Field quirks proven live in v1:
/// trade "SortOrder" is really the stock cap (0xFFFFFFFF = unlimited), shops
/// ignore rupies (CoinCost), and FREE items cap at ~200 purchases — hence cost
/// minimum 1.
/// </summary>
public class Mod : ModBase
{
    private readonly IModLoader _modLoader;
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private Config _configuration;

    private IDataManager _dataManager;

    private const uint EmptyHash = 0x887AE0B0;
    private const string VoucherItem = "ITEM_21_0000"; // Knickknack Voucher

    private const int TradeRowSize = 76;   // trade.headers (2.0)
    private const int TierRowSize = 60;    // item_tier_map.headers
    private const int RecipeRowSize = 176; // item_material_list.headers

    private record ShopItem(string Name, string ItemId, int RecipeKey, string TierId, Func<Config, int> Cost);

    private static readonly ShopItem[] Items =
    {
        new("Silver Dalia Badge", "ITEM_14_0031", 990001, "TMAP_VOUCHER_SILVER_BADGE", c => c.SilverDaliaBadgeCost),
        new("Gold Dalia Badge", "ITEM_14_0032", 990002, "TMAP_VOUCHER_GOLD_BADGE", c => c.GoldDaliaBadgeCost),
        new("Gold Spellbook", "ITEM_11_0002", 990003, "TMAP_VOUCHER_GOLD_SPELLBOOK", c => c.GoldSpellbookCost),
        new("Silver Centrum", "ITEM_14_0003", 990004, "TMAP_VOUCHER_SILVER_CENTRUM", c => c.SilverCentrumCost),
    };

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
            Log("ERROR: GBFR Mod Manager reports not initialized; cannot apply the voucher shop.");
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
        byte[] trade = _dataManager.GetArchiveFile("system/table/trade.tbl");
        byte[] tiers = _dataManager.GetArchiveFile("system/table/item_tier_map.tbl");
        byte[] recipes = _dataManager.GetArchiveFile("system/table/item_material_list.tbl");
        if (trade is null || trade.Length < 8 || tiers is null || tiers.Length < 8 || recipes is null || recipes.Length < 8)
        {
            Log("ERROR: could not read the shop tables from the game archive.");
            return;
        }
        long tradeRows = BinaryPrimitives.ReadInt64LittleEndian(trade);
        long tierRows = BinaryPrimitives.ReadInt64LittleEndian(tiers);
        long recipeRows = BinaryPrimitives.ReadInt64LittleEndian(recipes);
        if (8 + tradeRows * TradeRowSize != trade.Length ||
            8 + tierRows * TierRowSize != tiers.Length ||
            8 + recipeRows * RecipeRowSize != recipes.Length)
        {
            Log("ERROR: shop table layout changed (game update?) — refusing to patch the voucher shop.");
            return;
        }
        // collision guard: bail if a game update starts using our recipe key band
        for (long r = 0; r < recipeRows; r++)
        {
            int key = BinaryPrimitives.ReadInt32LittleEndian(recipes.AsSpan((int)(8 + r * RecipeRowSize + 0xA8), 4));
            if (key >= 990001 && key <= 990004)
            {
                Log($"ERROR: vanilla item_material_list now uses recipe key {key} (game update?) — refusing to patch.");
                return;
            }
        }

        var active = Items.Where(i => i.Cost(_configuration) > 0).ToArray();
        var parts = new List<string>();

        byte[] newTrade = Grow(trade, tradeRows, active.Length, TradeRowSize);
        byte[] newTiers = Grow(tiers, tierRows, active.Length, TierRowSize);
        byte[] newRecipes = Grow(recipes, recipeRows, active.Length, RecipeRowSize);

        for (int i = 0; i < active.Length; i++)
        {
            var it = active[i];
            int cost = Math.Clamp(it.Cost(_configuration), 1, 9999);

            // 1) cost recipe: Item1 = voucher, ItemCount1 = cost, other 20 hash cols empty
            var rec = newRecipes.AsSpan(recipes.Length + i * RecipeRowSize, RecipeRowSize);
            for (int c = 0; c < 21; c++)
                BinaryPrimitives.WriteUInt32LittleEndian(rec.Slice(c * 4, 4), EmptyHash);
            BinaryPrimitives.WriteUInt32LittleEndian(rec.Slice(0x00, 4), XXHash32Custom(VoucherItem)); // Item1
            BinaryPrimitives.WriteInt32LittleEndian(rec.Slice(0x54, 4), cost);                         // ItemCount1
            BinaryPrimitives.WriteInt32LittleEndian(rec.Slice(0xA8, 4), it.RecipeKey);                 // Key
            // ItemCount2..12, common counts, CoinCost stay 0

            // 2) tier map: MaterialId1 -> recipe key
            var tier = newTiers.AsSpan(tiers.Length + i * TierRowSize, TierRowSize);
            BinaryPrimitives.WriteInt32LittleEndian(tier.Slice(0x00, 4), it.RecipeKey);                // MaterialId1
            BinaryPrimitives.WriteUInt32LittleEndian(tier.Slice(0x38, 4), XXHash32Custom(it.TierId));  // Key

            // 3) trade row (values byte-diffed from the live-verified v1 static tables)
            var trd = newTrade.AsSpan(trade.Length + i * TradeRowSize, TradeRowSize);
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x00, 4), 0x00100000);                  // MinQuestId (early, proven-visible)
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x04, 4), 0);                           // MaxQuestId (no gate)
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x08, 4), XXHash32Custom(it.TierId + "_SUBKEY")); // SubKey (unique)
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x0C, 4), EmptyHash);                   // Unk4
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x10, 4), EmptyHash);                   // GemPurchasable
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x14, 4), EmptyHash);                   // PendulumPurchasable
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x18, 4), XXHash32Custom(it.ItemId));   // ItemPurchasable
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x1C, 4), XXHash32Custom(it.TierId));   // ItemTierMapId
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x20, 4), 3);                           // Key = Treasure Trade tab
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x24, 4), 10000);                       // Unk10 (matches cat-3 rows)
            // IsRefreshable/MaxStockForRefresh/MaxStockOrAmountRefreshed/IsRandomFeatured = 0
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x38, 4), 401);                         // FeaturedWeight (template value)
            BinaryPrimitives.WriteUInt32LittleEndian(trd.Slice(0x44, 4), 0xFFFFFFFF);                  // "SortOrder" = stock cap: unlimited

            parts.Add($"{it.Name} = {cost}");
        }

        _dataManager.AddOrUpdateExternalFile("system/table/trade.tbl", newTrade);
        _dataManager.AddOrUpdateExternalFile("system/table/item_tier_map.tbl", newTiers);
        _dataManager.AddOrUpdateExternalFile("system/table/item_material_list.tbl", newRecipes);
        _dataManager.UpdateIndex();
        var removed = Items.Where(i => i.Cost(_configuration) <= 0).Select(i => i.Name).ToArray();
        Log($"Voucher shop applied ({active.Length} items, unlimited stock): {string.Join(", ", parts)} Knickknack Voucher(s)" +
            (removed.Length > 0 ? $"; removed: {string.Join(", ", removed)}" : "") + ".");
    }

    /// <summary>Copy of a table buffer with <paramref name="extra"/> zeroed rows appended and the count updated.</summary>
    private static byte[] Grow(byte[] tbl, long rows, int extra, int rowSize)
    {
        byte[] result = new byte[tbl.Length + extra * rowSize];
        tbl.CopyTo(result, 0);
        BinaryPrimitives.WriteInt64LittleEndian(result, rows + extra);
        return result;
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
