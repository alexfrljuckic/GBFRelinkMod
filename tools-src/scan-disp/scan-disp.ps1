<#
.SYNOPSIS
    Scans a PE executable's .text section for x64 instructions that access memory at a
    given struct offset (disp32), e.g. every `vmovss xmm0, [rax+0x1BC]` in the game exe.

.DESCRIPTION
    Static "who touches this field" search — the reverse-engineering primitive that found
    the UI Size Setter (see docs/18-ui-spanning-session2.md). Works on the raw file, no
    debugger needed (the game is anti-debug).

    Matches the 4-byte little-endian disp32, then classifies by the bytes before it.
    Only instructions with modrm mod=10 (disp32) and no SIB byte are reported:

      VEX2  C5 pp op modrm disp32          - AVX scalar/packed float (vmovss, vmulss, ...)
      VEX3  C4 mm pp op modrm disp32       - same, with r8-r15 base register
      SSE   66/F2/F3 0F op modrm disp32    - legacy SSE float
      INT   8B/89/39/3B/C7 modrm disp32    - integer mov/cmp/mov-imm

    Float classes (VEX2/VEX3/SSE) are restricted to opcodes 10/11 (mov), 58/59/5C/5E
    (add/mul/sub/div), 2E/2F (ucomiss/comiss).

    LIMITATIONS (misses are possible): SIB-form addressing ([rax+rcx*4+disp]), disp8 forms
    (offset < 0x80 after a pointer-walk), other opcode families, and false positives where
    the 4 bytes are actually an immediate. Treat output as candidates; confirm each site
    with tools/disasm/disasm.exe before hooking.

.PARAMETER ExePath
    PE file to scan. Defaults to the GBF Relink Steam install.

.PARAMETER Disp
    The struct offset to search for, e.g. 0x1BC.

.PARAMETER PairDisp
    Optional second offset. When given, only prints Disp hits that have a PairDisp hit
    within +/- Window bytes — finds functions that touch BOTH fields (e.g. a W/H pair),
    which is how you separate real consumers of a struct from coincidental disp values.

.PARAMETER Window
    Proximity window in bytes for -PairDisp clustering. Default 0x80.

.PARAMETER FloatOnly
    Only report VEX/SSE (float) accesses; drop the noisy INT class.

.EXAMPLE
    # every float reader/writer of the UI element constraint width
    .\scan-disp.ps1 -Disp 0x1BC -FloatOnly

.EXAMPLE
    # functions that touch constraint W AND H close together (real layout code)
    .\scan-disp.ps1 -Disp 0x1BC -PairDisp 0x1C0 -FloatOnly

.EXAMPLE
    # then read a candidate site (disasm.exe: rva is HEX, length is DECIMAL)
    tools\disasm\disasm.exe "<game>\granblue_fantasy_relink.exe" 334375b 160

.NOTES
    Output objects have: InstrRva (approx instruction start, hex), DispRva (address of the
    disp32 bytes), Class, Op, ModRM. InstrRva assumes no extra prefixes; for VEX3/REX forms
    disassemble a few bytes earlier to be safe. Runtime address = 0x140000000 + rva.
    Windows PowerShell 5.1 compatible (no Span; scanning is done in inline C#).
#>
param(
    [string]$ExePath = "D:\Steam\steamapps\common\Granblue Fantasy Relink\granblue_fantasy_relink.exe",
    [Parameter(Mandatory = $true)][uint32]$Disp,
    [uint32]$PairDisp = 0,
    [uint32]$Window = 0x80,
    [switch]$FloatOnly
)

if (-not ("DispScanner" -as [type])) {
Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
public class DispHit {
    public uint InstrRva; public uint DispRva; public string Class; public byte Op; public byte ModRM;
}
public static class DispScanner {
    // Finds mod=10/no-SIB disp32 accesses and classifies by preceding opcode bytes.
    // Layouts (disp32 at f): VEX2 C5@f-4, VEX3 C4@f-5, SSE prefix@f-4 0F@f-3, INT op@f-2.
    public static List<DispHit> Scan(byte[] bytes, int textRaw, int textRawSize, uint textVA, uint disp) {
        var results = new List<DispHit>();
        byte d0 = (byte)(disp & 0xFF), d1 = (byte)((disp >> 8) & 0xFF);
        byte d2 = (byte)((disp >> 16) & 0xFF), d3 = (byte)((disp >> 24) & 0xFF);
        int end = textRaw + textRawSize - 4;
        for (int f = textRaw + 8; f < end; f++) {
            if (bytes[f] != d0 || bytes[f+1] != d1 || bytes[f+2] != d2 || bytes[f+3] != d3) continue;
            byte modrm = bytes[f - 1];
            if ((modrm & 0xC0) != 0x80 || (modrm & 0x07) == 4) continue;
            byte b2 = bytes[f - 2], b3 = bytes[f - 3], b4 = bytes[f - 4], b5 = bytes[f - 5];
            string cls = null; int opLen = 0;
            bool fp = b2==0x10||b2==0x11||b2==0x58||b2==0x59||b2==0x5C||b2==0x5E||b2==0x2E||b2==0x2F;
            if (b4 == 0xC5 && fp) { cls = "VEX2"; opLen = 3; }
            else if (b5 == 0xC4 && fp) { cls = "VEX3"; opLen = 4; }
            else if (b3 == 0x0F && fp && (b4==0xF3||b4==0xF2||b4==0x66)) { cls = "SSE"; opLen = 4; }
            else if (b2==0x8B||b2==0x89||b2==0x39||b2==0x3B||b2==0xC7) { cls = "INT"; opLen = 2; }
            if (cls == null) continue;
            uint rva = (uint)(textVA + (f - textRaw));
            results.Add(new DispHit {
                InstrRva = (uint)(rva - opLen - 1), DispRva = rva, Class = cls, Op = b2, ModRM = modrm
            });
        }
        return results;
    }
}
'@
}

$bytes = [System.IO.File]::ReadAllBytes($ExePath)

# PE parse: locate .text
$e_lfanew = [BitConverter]::ToInt32($bytes, 0x3C)
$numSections = [BitConverter]::ToUInt16($bytes, $e_lfanew + 6)
$optHdrSize = [BitConverter]::ToUInt16($bytes, $e_lfanew + 20)
$secTable = $e_lfanew + 24 + $optHdrSize
$textVA = 0; $textRaw = 0; $textRawSize = 0
for ($i = 0; $i -lt $numSections; $i++) {
    $off = $secTable + $i * 40
    $name = [System.Text.Encoding]::ASCII.GetString($bytes, $off, 8).TrimEnd([char]0)
    if ($name -eq ".text") {
        $textVA = [BitConverter]::ToUInt32($bytes, $off + 12)
        $textRawSize = [BitConverter]::ToUInt32($bytes, $off + 16)
        $textRaw = [BitConverter]::ToUInt32($bytes, $off + 20)
        break
    }
}
if ($textRawSize -eq 0) { throw "no .text section found in $ExePath" }
Write-Verbose (".text: rva=0x{0:x} raw=0x{1:x} size=0x{2:x}" -f $textVA, $textRaw, $textRawSize)

function Format-Hits($hits) {
    foreach ($h in $hits) {
        [pscustomobject]@{
            InstrRva = "0x{0:x}" -f $h.InstrRva
            DispRva  = "0x{0:x}" -f $h.DispRva
            Class    = $h.Class
            Op       = "0x{0:X2}" -f $h.Op
            ModRM    = "0x{0:X2}" -f $h.ModRM
        }
    }
}

$hits = [DispScanner]::Scan($bytes, $textRaw, $textRawSize, $textVA, $Disp)
if ($FloatOnly) { $hits = @($hits | Where-Object { $_.Class -ne "INT" }) }

if ($PairDisp -eq 0) {
    Format-Hits $hits
    Write-Verbose ("{0} hits for disp 0x{1:x}" -f @($hits).Count, $Disp)
    return
}

# clustering mode: keep Disp hits with a PairDisp hit within +/- Window
$pairHits = [DispScanner]::Scan($bytes, $textRaw, $textRawSize, $textVA, $PairDisp)
if ($FloatOnly) { $pairHits = @($pairHits | Where-Object { $_.Class -ne "INT" }) }
$pairRvas = @($pairHits | ForEach-Object { [int64]$_.InstrRva }) | Sort-Object

foreach ($h in $hits) {
    $near = $pairRvas | Where-Object { [Math]::Abs($_ - [int64]$h.InstrRva) -le $Window } | Select-Object -First 1
    if ($null -ne $near) {
        [pscustomobject]@{
            InstrRva     = "0x{0:x}" -f $h.InstrRva
            Class        = $h.Class
            PairInstrRva = "0x{0:x}" -f $near
        }
    }
}
