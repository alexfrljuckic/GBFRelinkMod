# scan-disp — "who touches this struct field" scanner

Static scanner that finds every x64 instruction in the game exe's `.text` section that
reads/writes memory at a given struct offset (disp32), e.g. every `vmovss xmm0, [rax+0x1BC]`.
No debugger, no game launch — works on the raw file, which matters because the game is
anti-debug (x64dbg crashes on attach).

This is the tool that cracked the "real background function" problem: scanning for float
accesses to `+0x1BC` (UI element constraint width) and clustering with `+0x1C0` (height)
surfaced both the decoy rect-builder family and the **UI Size Setter** — the function every
UI element's size flows through. Full story: [docs/18](../../docs/18-ui-spanning-session2.md).

## Requirements

Windows PowerShell 5.1+ (uses inline C# for the scan — fast even on the 123 MB exe).
No dependencies.

## Usage

```powershell
# every float reader/writer of [reg+0x1BC] (candidates only — see Limitations)
.\scan-disp.ps1 -Disp 0x1BC -FloatOnly

# the money query: instructions touching BOTH 0x1BC and 0x1C0 within 0x80 bytes.
# Coincidental disp values almost never pair up; real layout code touches W and H.
.\scan-disp.ps1 -Disp 0x1BC -PairDisp 0x1C0 -FloatOnly

# other exe / wider cluster window / include integer accesses (id compares, mov-imm)
.\scan-disp.ps1 -ExePath "C:\path\to\other.exe" -Disp 0x1C4 -Window 0x180
```

Output is objects (pipe to `Format-Table`, `Where-Object`, `Export-Csv`):

| Column | Meaning |
| --- | --- |
| `InstrRva` | approximate instruction start RVA (see caveat below) |
| `DispRva` | exact RVA of the disp32 bytes |
| `Class` | `VEX2`/`VEX3`/`SSE` (float) or `INT` (mov/cmp/mov-imm) |
| `Op` | opcode byte: `0x10` load, `0x11` store, `0x59` mul, `0x2E` ucomiss, ... |
| `ModRM` | modrm byte — low 3 bits = base reg (0=rax, 1=rcx, ..., 6=rsi, 7=rdi) |

## Workflow: pair it with the disassembler

The scanner gives you *candidate sites*; always confirm each one by reading the actual
code with the zydis disassembler before hooking:

```powershell
# NOTE: rva argument is HEX (no 0x prefix), length argument is DECIMAL
tools\disasm\disasm.exe "D:\Steam\steamapps\common\Granblue Fantasy Relink\granblue_fantasy_relink.exe" 334375b 160
```

Runtime address = `0x140000000 + rva` (image base). Start disassembly a little before
`InstrRva` if output looks misaligned — zydis resyncs within a few instructions.

To turn a confirmed site into a hook pattern: dump the raw bytes around it, wildcard all
rip-relative displacements (`?? ?? ?? ??` after the opcode/modrm), and verify uniqueness by
re-scanning — if the logic is inlined at several byte-identical sites (the UI Size Setter
exists 3×), use `MAKE_MIDHOOK_ALL` (vendor/GBFRelinkFix `src/memory.h`) to hook every match.

## What it matches (and misses)

Matches only `modrm mod=10` (disp32) with no SIB byte:

- `VEX2` — `C5 pp op modrm disp32` (AVX scalar float, rax–rdi base)
- `VEX3` — `C4 mm pp op modrm disp32` (AVX, r8–r15 base — e.g. `[r14+0x1BC]`)
- `SSE` — `66/F2/F3 0F op modrm disp32` (legacy float)
- `INT` — `8B/89/39/3B/C7 modrm disp32` (mov r32, mov [m],r32, cmp, mov-imm)

Float classes are limited to opcodes `10/11` (mov), `58/59/5C/5E` (arith), `2E/2F` (compare).

**Misses:** SIB addressing (`[rax+rcx*4+disp]`), disp8 forms (small offsets), any other
opcode family, and accesses computed via a pointer to the field. **False positives:** the
4 disp bytes can coincidentally appear as an immediate or data. This is a candidate
generator, not ground truth — confirm with the disassembler.

`InstrRva` assumes no extra prefixes before the sequence; treat it as approximate.

## Background

Built 2026-07-09 for the ultrawide UI-spanning work
([docs/17](../../docs/17-ui-spanning-handoff.md), [docs/18](../../docs/18-ui-spanning-session2.md)).
Generic by design: any struct-field hunt in any PE works, e.g. mapping which functions
consume a value you found with Cheat Engine-style knowledge of a struct layout.
