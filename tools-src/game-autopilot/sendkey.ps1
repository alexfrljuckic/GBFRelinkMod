# Sends scancode-level key presses to the foreground game window.
# Usage: sendkey.ps1 -Keys "enter,down,down,enter" [-HoldMs 60] [-GapMs 400]
param(
    [Parameter(Mandatory = $true)][string]$Keys,
    [int]$HoldMs = 60,
    [int]$GapMs = 450
)

if (-not ("GameInput2" -as [type])) {
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class GameInput2 {
    // INPUT must be exactly 40 bytes on x64: 4 (type) + 4 (pad) + 32 (union, sized by
    // MOUSEINPUT). KEYBDINPUT is 24, so one trailing ulong pads the union to 32.
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT { public uint type; public KEYBDINPUT ki; public ulong pad1; }
    [StructLayout(LayoutKind.Sequential)]
    struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindowA(string cls, string title);

    const uint KEYEVENTF_SCANCODE = 0x0008;
    const uint KEYEVENTF_KEYUP = 0x0002;
    const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

    public static void Key(ushort scan, bool extended, int holdMs) {
        var down = new INPUT[1];
        down[0].type = 1; // INPUT_KEYBOARD
        down[0].ki.wScan = scan;
        down[0].ki.dwFlags = KEYEVENTF_SCANCODE | (extended ? KEYEVENTF_EXTENDEDKEY : 0);
        SendInput(1, down, Marshal.SizeOf(typeof(INPUT)));
        System.Threading.Thread.Sleep(holdMs);
        var up = new INPUT[1];
        up[0].type = 1;
        up[0].ki.wScan = scan;
        up[0].ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP | (extended ? KEYEVENTF_EXTENDEDKEY : 0);
        SendInput(1, up, Marshal.SizeOf(typeof(INPUT)));
    }
}
'@
}

$game = Get-Process -Name "granblue_fantasy_relink" -ErrorAction SilentlyContinue
if (-not $game) { throw "game not running" }
[GameInput2]::SetForegroundWindow($game.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 300

# scancode map (set 1): name -> scan, extended
$map = @{
    "enter" = @(0x1C, $false); "esc" = @(0x01, $false); "space" = @(0x39, $false)
    "up" = @(0x48, $true); "down" = @(0x50, $true); "left" = @(0x4B, $true); "right" = @(0x4D, $true)
    "tab" = @(0x0F, $false); "e" = @(0x12, $false); "q" = @(0x10, $false)
    "w" = @(0x11, $false); "a" = @(0x1E, $false); "s" = @(0x1F, $false); "d" = @(0x20, $false)
}

foreach ($k in ($Keys -split ",")) {
    $k = $k.Trim().ToLower()
    if (-not $map.ContainsKey($k)) { Write-Warning "unknown key: $k"; continue }
    [GameInput2]::Key($map[$k][0], $map[$k][1], $HoldMs)
    Start-Sleep -Milliseconds $GapMs
}
Write-Output "sent: $Keys"
