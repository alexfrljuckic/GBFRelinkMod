using System.Diagnostics;
using System.Runtime.InteropServices;

using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;

namespace gbfr.synthesis.control;

/// <summary>
/// Deterministic sigil-synthesis traits — the code-hook half of the mod (docs/24).
///
/// How vanilla synthesis picks traits (decoded + live-verified 2026-07-16):
///   1. The executor builds a combined trait pool in SOURCE order:
///        pool[0]=sigilA.trait1, [1]=sigilA.trait2, [2]=sigilB.trait1, [3]=sigilB.trait2
///   2. The pool is sorted, then Fisher-Yates SHUFFLED (destroying source order).
///   3. pool[0] becomes the result's innate trait (which decides the result sigil's
///      identity), pool[1] becomes its secondary. The UI's "1/12 Prospects" is exactly
///      the 4x3 = 12 ordered pairs of distinct traits.
///
/// So we hook twice:
///   SITE1 (pool just built, source order intact) -> copy all four trait ids into our
///         own buffer, before the sort/shuffle destroys which came from where.
///   SITE2 (post-shuffle, immediately before the game consumes pool[0]/pool[1]) ->
///         overwrite them with the user's chosen pair.
///
/// Both hooks are PURE ASSEMBLY that never calls back into managed code — that keeps
/// them free of shadow-space/stack-alignment/XMM-preservation hazards. They clobber only
/// rax/rcx/rdx and flags, all pushed and popped. The user's selection reaches the asm as
/// two pointers into our buffer, which C# rewrites whenever the config changes; the asm
/// itself never branches on config values beyond a single enabled byte.
/// </summary>
internal unsafe class SynthesisHook
{
    // Unique byte signatures for the two sites (verified single-match against the 2.0 exe).
    // If a game update moves this code these stop matching and we refuse to hook, rather
    // than injecting into whatever now lives at a hardcoded address.
    private const string Site1Pattern = // lea rcx,[rbp+300]; mov r8,[rbp+2F8]; lea rdx,[r8*4+300]; add rdx,rbp; call
        "48 8D 8D 00 03 00 00 4C 8B 85 F8 02 00 00 4A 8D 14 85 00 03 00 00 48 01 EA E8";
    private const string Site2Pattern = // mov edx,[rbp+300]; mov r8d,[rdi+270]; and r8d,edx; mov rcx,[rdi+248]; mov r9,[rdi+258]
        "8B 95 00 03 00 00 44 8B 87 70 02 00 00 41 21 D0 48 8B 8F 48 02 00 00 4C 8B 8F 58 02 00 00";

    // Shared buffer layout (see asm below).
    private const int OffStash0     = 0x00; // uint32 x4: A.t1, A.t2, B.t1, B.t2
    private const int OffInnatePtr  = 0x10; // uint64: -> one of the four stash slots
    private const int OffSecPtr     = 0x18; // uint64: -> one of the four stash slots
    private const int OffEnabled    = 0x20; // byte
    private const int BufferSize    = 0x28;

    private readonly nint _buf;
    private IAsmHook _stashHook;
    private IAsmHook _overrideHook;

    public SynthesisHook()
    {
        _buf = Marshal.AllocHGlobal(BufferSize);
        for (int i = 0; i < BufferSize; i++) ((byte*)_buf)[i] = 0;
    }

    /// <summary>Point the asm at the chosen stash slots. Safe to call any time.</summary>
    public void SetSelection(int innateIdx, int secondaryIdx, bool enabled)
    {
        innateIdx = Math.Clamp(innateIdx, 0, 3);
        secondaryIdx = Math.Clamp(secondaryIdx, 0, 3);
        *(ulong*)(_buf + OffInnatePtr) = (ulong)(_buf + OffStash0 + innateIdx * 4);
        *(ulong*)(_buf + OffSecPtr)    = (ulong)(_buf + OffStash0 + secondaryIdx * 4);
        // Write the enable byte last so the asm can never see a half-updated selection.
        *(byte*)(_buf + OffEnabled) = (byte)(enabled ? 1 : 0);
    }

    /// <summary>Scan for both sites and install. Returns null on success, else a reason.</summary>
    public string Install(IReloadedHooks hooks, Action<string> log)
    {
        nint moduleBase = Process.GetCurrentProcess().MainModule.BaseAddress;

        nint site1 = FindUnique(Site1Pattern, out string err1);
        if (site1 == 0) return $"trait-pool site not found ({err1}) — game update?";
        nint site2 = FindUnique(Site2Pattern, out string err2);
        if (site2 == 0) return $"trait-select site not found ({err2}) — game update?";
        log($"Found synthesis trait code at .text+0x{site1 - moduleBase:X} and .text+0x{site2 - moduleBase:X}; installing hooks...");

        long bufAddr = _buf;

        // SITE1 — pool freshly built, source order intact. Copy all four ids out.
        // Only acts on the standard 2-traits-each case (count == 4).
        string[] stashAsm =
        {
            "use64",
            "push rax",
            "push rcx",
            "pushfq",
            $"mov rcx, 0x{bufAddr:X}",
            "cmp qword [rbp+0x2F8], 4",
            "jne _stash_skip",
            "mov eax, [rbp+0x300]", "mov [rcx+0x00], eax",   // A.trait1
            "mov eax, [rbp+0x304]", "mov [rcx+0x04], eax",   // A.trait2
            "mov eax, [rbp+0x308]", "mov [rcx+0x08], eax",   // B.trait1
            "mov eax, [rbp+0x30C]", "mov [rcx+0x0C], eax",   // B.trait2
            "_stash_skip:",
            "popfq",
            "pop rcx",
            "pop rax",
        };

        // SITE2 — post-shuffle, right before `mov edx,[rbp+0x300]` consumes the pick.
        // Impose the chosen pair. Skips if disabled, if the pool is too small, or if the
        // two chosen traits are identical (the 12 prospects are DISTINCT pairs, so an
        // identical pair isn't a legal outcome — leave vanilla rather than forge one).
        string[] overrideAsm =
        {
            "use64",
            "push rax",
            "push rcx",
            "push rdx",
            "pushfq",
            $"mov rcx, 0x{bufAddr:X}",
            $"cmp byte [rcx+{OffEnabled}], 0",
            "je _ovr_skip",
            "cmp qword [rbp+0x2F8], 2",
            "jb _ovr_skip",
            $"mov rax, [rcx+{OffInnatePtr}]",
            "mov eax, [rax]",
            $"mov rdx, [rcx+{OffSecPtr}]",
            "mov edx, [rdx]",
            "cmp eax, edx",
            "je _ovr_skip",
            "mov [rbp+0x300], eax",   // innate
            "mov [rbp+0x304], edx",   // secondary
            "_ovr_skip:",
            "popfq",
            "pop rdx",
            "pop rcx",
            "pop rax",
        };

        _stashHook = hooks.CreateAsmHook(stashAsm, site1, AsmHookBehaviour.ExecuteFirst).Activate();
        _overrideHook = hooks.CreateAsmHook(overrideAsm, site2, AsmHookBehaviour.ExecuteFirst).Activate();
        log("Synthesis trait hooks active.");
        return null;
    }

    /// <summary>
    /// Pattern-scan the game's EXECUTABLE sections only. Requires exactly one match.
    ///
    /// Scanning the module's whole virtual range (SizeOfImage) is not safe: it spans
    /// pages that aren't readable, and touching one is an access violation — which .NET
    /// cannot catch, so it takes the game down instantly. We therefore walk the PE
    /// section table and only read sections marked IMAGE_SCN_MEM_EXECUTE (i.e. .text),
    /// which is both safe and much faster. This mirrors what the C++ scanner does.
    /// </summary>
    private static nint FindUnique(string pattern, out string error)
    {
        error = null;
        string[] toks = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int n = toks.Length;
        var bytes = new byte[n];
        var mask = new bool[n];
        for (int i = 0; i < n; i++)
        {
            if (toks[i] == "??") { mask[i] = false; continue; }
            mask[i] = true;
            bytes[i] = Convert.ToByte(toks[i], 16);
        }

        byte* baseAddr = (byte*)Process.GetCurrentProcess().MainModule.BaseAddress;

        // PE headers -> section table
        int e_lfanew = *(int*)(baseAddr + 0x3C);
        byte* nt = baseAddr + e_lfanew;
        if (*(uint*)nt != 0x00004550) { error = "bad PE signature"; return 0; } // "PE\0\0"
        ushort numSections = *(ushort*)(nt + 4 + 2);
        ushort optHeaderSize = *(ushort*)(nt + 4 + 16);
        byte* secTable = nt + 4 + 20 + optHeaderSize;

        const uint IMAGE_SCN_MEM_EXECUTE = 0x20000000;
        nint found = 0;
        int hits = 0;

        for (int s = 0; s < numSections; s++)
        {
            byte* sec = secTable + s * 40;
            uint virtualSize = *(uint*)(sec + 8);
            uint virtualAddr = *(uint*)(sec + 12);
            uint characteristics = *(uint*)(sec + 36);
            if ((characteristics & IMAGE_SCN_MEM_EXECUTE) == 0) continue;
            if (virtualSize <= (uint)n) continue;

            byte* start = baseAddr + virtualAddr;
            long size = virtualSize;
            for (long i = 0; i <= size - n; i++)
            {
                if (mask[0] && start[i] != bytes[0]) continue; // cheap reject
                bool ok = true;
                for (int j = 1; j < n; j++)
                {
                    if (mask[j] && start[i + j] != bytes[j]) { ok = false; break; }
                }
                if (!ok) continue;
                hits++;
                if (hits == 1) found = (nint)(start + i);
                else { error = "2+ matches — ambiguous"; return 0; }
            }
        }
        if (hits == 0) { error = "no match"; return 0; }
        return found;
    }
}
