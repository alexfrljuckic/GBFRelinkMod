using System.Buffers.Binary;

using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

using gbfr.synthesis.control.Configuration;
using gbfr.synthesis.control.Template;

using gbfrelink.utility.manager.Interfaces;

namespace gbfr.synthesis.control;

/// <summary>
/// Sigil Synthesis Control — two independent features (research: docs/24).
///
/// 1. Always Grand Success (data). gem_mix_success.tbl holds the success-grade
///    weights, one row per Prospect score (keys 33-60): Great and Grand out of
///    10000, remainder a plain Success. Vanilla Grand odds run 4500-8500 on the
///    scored rows (even keys 44-60) and 0 elsewhere; we set every row to 0/10000,
///    so every synthesis rolls Grand and the result's traits come out maxed
///    (Lv15). Patched at launch from the vanilla table via the GBFR Mod Manager;
///    a layout guard refuses to patch if a game update changes the row size, and
///    the mod ships no static tables.
///
/// 2. Choose result traits (code). Vanilla picks the result's two traits at
///    random from the four traits of the two input sigils; this pins them to the
///    user's choice. See SynthesisHook for the decoded mechanism and the hooks.
///
/// Feature 1 needs a game restart (tables are read once, at launch); feature 2 is
/// live (the hooks read the selection through a pointer on every synthesis).
/// </summary>
public class Mod : ModBase
{
    private readonly IModLoader _modLoader;
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private Config _configuration;

    private IDataManager _dataManager;
    private SynthesisHook _synthesisHook;

    private const int GemMixSuccessRowSize = 12; // GreatWeight, GrandWeight, Key

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _logger = context.Logger;
        _modConfig = context.ModConfig;
        _configuration = context.Configuration;

        InstallTraitHooks();

        var controller = _modLoader.GetController<IDataManager>();
        if (controller is null || !controller.TryGetTarget(out _dataManager))
        {
            Log("ERROR: GBFR Mod Manager (IDataManager) not available — is 'gbfrelink.utility.manager' installed and enabled?");
            return;
        }
        if (!_dataManager.Initialized)
        {
            Log("ERROR: GBFR Mod Manager reports not initialized; cannot apply synthesis patches.");
            return;
        }

        Apply();
    }

    /// <summary>
    /// Feature 2 — the trait-choice code hooks (independent of the table patch).
    /// Everything here is best-effort: if anything goes wrong the feature turns itself
    /// off and Grand Success carries on. Each step logs BEFORE it runs, so if this ever
    /// does take the process down, the last line in the log names the culprit.
    /// </summary>
    private void InstallTraitHooks()
    {
        try
        {
            var hooksController = _modLoader.GetController<IReloadedHooks>();
            if (hooksController is null || !hooksController.TryGetTarget(out IReloadedHooks hooks))
            {
                Log("WARNING: Reloaded.Hooks not available — 'Choose result traits' disabled. Is 'reloaded.sharedlib.hooks' enabled?");
                return;
            }

            Log("Scanning for the synthesis trait code...");
            _synthesisHook = new SynthesisHook();
            string err = _synthesisHook.Install(hooks, Log);
            if (err is not null)
            {
                _synthesisHook = null;
                Log($"WARNING: 'Choose result traits' disabled — {err}. Grand Success is unaffected.");
                return;
            }
            ApplyTraitSelection();
        }
        catch (Exception ex)
        {
            _synthesisHook = null;
            Log($"WARNING: 'Choose result traits' disabled — {ex.GetType().Name}: {ex.Message}. Grand Success is unaffected.");
        }
    }

    private void ApplyTraitSelection()
    {
        if (_synthesisHook is null)
            return;
        int innate = (int)_configuration.InnateFrom;
        int secondary = (int)_configuration.SecondaryFrom;
        _synthesisHook.SetSelection(innate, secondary, _configuration.ChooseResultTraits);
        if (!_configuration.ChooseResultTraits)
        {
            Log("Choose result traits: OFF — synthesis traits stay vanilla-random.");
            return;
        }
        if (innate == secondary)
            Log($"Choose result traits: both slots are set to {_configuration.InnateFrom} — the game only makes pairs of two DIFFERENT traits, so syntheses will stay vanilla-random until you pick two different sources.");
        else
            Log($"Choose result traits: ON — trait 1 <- {_configuration.InnateFrom}, trait 2 <- {_configuration.SecondaryFrom}.");
    }

    public override void ConfigurationUpdated(Config configuration)
    {
        _configuration = configuration;
        // The hooks read the selection live through a pointer — no restart needed.
        ApplyTraitSelection();
        if (_dataManager is null)
            return;
        Apply();
        Log("Grand Success setting changed — RESTART THE GAME for that part to take effect (tables are read once, at launch). Trait choices apply immediately.");
    }

    private void Apply()
    {
        ApplyAlwaysGrandSuccess();
        _dataManager.UpdateIndex();
    }

    /// <summary>gem_mix_success.tbl: every success-grade roll → 100% Grand.</summary>
    private void ApplyAlwaysGrandSuccess()
    {
        byte[] tbl = _dataManager.GetArchiveFile("system/table/gem_mix_success.tbl");
        if (tbl is null || tbl.Length < 8)
        {
            Log("ERROR: could not read gem_mix_success.tbl from the game archive.");
            return;
        }
        long rows = BinaryPrimitives.ReadInt64LittleEndian(tbl);
        if (8 + rows * GemMixSuccessRowSize != tbl.Length)
        {
            Log("ERROR: gem_mix_success.tbl layout changed (game update?) — refusing to patch success weights.");
            return;
        }
        if (!_configuration.AlwaysGrandSuccess)
        {
            // register the untouched vanilla table so a stale patched copy can't linger
            _dataManager.AddOrUpdateExternalFile("system/table/gem_mix_success.tbl", tbl);
            Log("Always Grand Success OFF — vanilla gem_mix_success.tbl registered.");
            return;
        }
        for (long r = 0; r < rows; r++)
        {
            int off = (int)(8 + r * GemMixSuccessRowSize);
            BinaryPrimitives.WriteUInt32LittleEndian(tbl.AsSpan(off + 0, 4), 0);     // GreatSuccessChanceWeight
            BinaryPrimitives.WriteUInt32LittleEndian(tbl.AsSpan(off + 4, 4), 10000); // GrandSuccessChanceWeight
        }
        _dataManager.AddOrUpdateExternalFile("system/table/gem_mix_success.tbl", tbl);
        Log($"Always Grand Success applied: all {rows} Prospect rows set to 100% Grand.");
    }

    private void Log(string message) => _logger.WriteLine($"[{_modConfig.ModId}] {message}");

    #region Standard Overrides / For Exports, Serialization etc.
#pragma warning disable CS8618
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}
