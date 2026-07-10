# Clicks the center of the game window (focuses it), via absolute SendInput.
if (-not ("GameMouse" -as [type])) {
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class GameMouse {
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT { public uint type; public MOUSEINPUT mi; }
    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
    [DllImport("user32.dll")] static extern uint SendInput(uint n, INPUT[] inputs, int size);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    public static void ClickAt(int x, int y) {
        // virtual-desktop absolute coords are 0..65535 over the virtual screen
        int vsX = GetSystemMetrics(76), vsY = GetSystemMetrics(77);
        int vsW = GetSystemMetrics(78), vsH = GetSystemMetrics(79);
        int ax = (int)((x - vsX) * 65535L / vsW);
        int ay = (int)((y - vsY) * 65535L / vsH);
        var move = new INPUT[3];
        move[0].type = 0; move[0].mi.dx = ax; move[0].mi.dy = ay;
        move[0].mi.dwFlags = 0x0001 | 0x8000 | 0x4000; // MOVE | ABSOLUTE | VIRTUALDESK
        move[1].type = 0; move[1].mi.dwFlags = 0x0002; // LEFTDOWN
        move[2].type = 0; move[2].mi.dwFlags = 0x0004; // LEFTUP
        SendInput(3, move, Marshal.SizeOf(typeof(INPUT)));
    }
    [DllImport("user32.dll")] static extern int GetSystemMetrics(int idx);
}
'@
}
$game = Get-Process -Name "granblue_fantasy_relink" -ErrorAction SilentlyContinue
if (-not $game) { throw "game not running" }
[GameMouse]::SetForegroundWindow($game.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 200
$rect = New-Object GameMouse+RECT
[GameMouse]::GetWindowRect($game.MainWindowHandle, [ref]$rect) | Out-Null
$cx = [int](($rect.Left + $rect.Right) / 2)
$cy = [int](($rect.Top + $rect.Bottom) / 2)
[GameMouse]::ClickAt($cx, $cy)
Write-Output ("clicked window center ({0},{1}) rect=({2},{3},{4},{5})" -f $cx, $cy, $rect.Left, $rect.Top, $rect.Right, $rect.Bottom)
