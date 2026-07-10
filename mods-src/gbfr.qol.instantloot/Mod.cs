using System.Diagnostics;
using System.Runtime.InteropServices;

using Reloaded.Mod.Interfaces;

using gbfr.qol.instantloot.Configuration;
using gbfr.qol.instantloot.Template;

namespace gbfr.qol.instantloot;

/// <summary>
/// Instant end-of-quest loot: two AOB byte-patches on the game's .text, applied once at
/// load. Both signatures are ported from NidasBot's Endless Ragnarok Cheat Engine table
/// (FearLess t=40001) — used only as a map of the code sites; the patch bytes and the
/// scan/apply code here are our own. Each site was disassembled and confirmed
/// (GBFRelinkMod docs/19):
///
///   Auto Loot Quest Chest — a `mov rax, 0x41F0000041F00000` (two packed 30.0f chest
///     timers) followed by `mov [rsi+0x6F0], rax`. Overwriting the mov-imm with
///     `xor eax,eax` + nops makes the store write 0.0 → timer instantly expired.
///
///   Skip Result Screen — a `vsubss xmm0, xmm2, xmm0` decrement stored to the
///     result-screen countdown, then compared. Replacing the vsubss with
///     `xorps xmm0,xmm0` (+nop) forces the remaining time to 0 → screen advances.
/// </summary>
public class Mod : ModBase
{
    private readonly ILogger _logger;
    private readonly IModConfig _modConfig;
    private Config _configuration;

    // sig, patch, and the max wildcard-safe length; '??' = wildcard.
    private const string AutoLootSig =
        "48 B8 ?? ?? ?? ?? ?? ?? ?? ?? 48 89 ?? ?? ?? ?? ?? 48 8B ?? ?? ?? ?? ?? BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 84";
    private static readonly byte[] AutoLootPatch = { 0x31, 0xC0, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };

    private const string SkipResultSig =
        "C5 ?? ?? ?? C5 ?? ?? ?? ?? ?? ?? ?? C5 ?? ?? ?? 72 ?? 48 8B ?? ?? ?? ?? ?? 48 8B";
    // The vsubss (C5 EA 5C C0) is at the match start; the following vmovss + vucomiss
    // are just there to make the signature unique. Patch offset 0.
    private const int SkipResultPatchOffset = 0;
    private static readonly byte[] SkipResultPatch = { 0x0F, 0x57, 0xC0, 0x90 };

    public Mod(ModContext context)
    {
        _logger = context.Logger;
        _modConfig = context.ModConfig;
        _configuration = context.Configuration;
        ApplyPatches();
    }

    private void ApplyPatches()
    {
        ProcessModule main = Process.GetCurrentProcess().MainModule!;
        nint baseAddr = main.BaseAddress;
        int size = main.ModuleMemorySize;
        var mem = new byte[size];
        Marshal.Copy(baseAddr, mem, 0, size);

        if (_configuration.AutoLootChest)
            Patch("Auto Loot Quest Chest", mem, baseAddr, AutoLootSig, 0, AutoLootPatch);
        if (_configuration.SkipResultScreen)
            Patch("Skip Result Screen", mem, baseAddr, SkipResultSig, SkipResultPatchOffset, SkipResultPatch);
    }

    private void Patch(string name, byte[] mem, nint baseAddr, string sig, int patchOffset, byte[] patch)
    {
        int match = FindUnique(mem, sig, name);
        if (match < 0)
            return;

        nint target = baseAddr + match + patchOffset;
        if (!VirtualProtect(target, patch.Length, PAGE_EXECUTE_READWRITE, out uint old))
        {
            Log($"{name}: VirtualProtect failed — patch NOT applied.");
            return;
        }
        Marshal.Copy(patch, 0, target, patch.Length);
        VirtualProtect(target, patch.Length, old, out _);
        Log($"{name}: patched at +0x{(match + patchOffset):X} ({patch.Length} bytes).");
    }

    /// <summary>Scan for an IDA-style "AA BB ?? .." signature. Returns match start or -1;
    /// warns if the signature is not unique (a game update may have shifted it).</summary>
    private int FindUnique(byte[] mem, string sig, string name)
    {
        string[] toks = sig.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int n = toks.Length;
        var bytes = new int[n];
        for (int i = 0; i < n; i++) bytes[i] = toks[i] == "??" ? -1 : Convert.ToInt32(toks[i], 16);

        int found = -1, count = 0;
        for (int i = 0; i <= mem.Length - n; i++)
        {
            bool ok = true;
            for (int j = 0; j < n; j++)
            {
                if (bytes[j] >= 0 && mem[i + j] != bytes[j]) { ok = false; break; }
            }
            if (ok)
            {
                if (found < 0) found = i;
                if (++count > 1) break;
            }
        }

        if (found < 0) { Log($"{name}: signature not found (game update?) — feature skipped."); return -1; }
        if (count > 1) { Log($"{name}: signature matched {count}+ times — ambiguous, feature skipped for safety."); return -1; }
        return found;
    }

    public override void ConfigurationUpdated(Config configuration)
    {
        _configuration = configuration;
        Log("Settings changed — RESTART THE GAME to apply (patches run once, at launch).");
    }

    private void Log(string message) => _logger.WriteLine($"[{_modConfig.ModId}] {message}");

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtect(nint address, int size, uint newProtect, out uint oldProtect);
    private const uint PAGE_EXECUTE_READWRITE = 0x40;

    #region Standard Overrides
#pragma warning disable CS8618
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}
