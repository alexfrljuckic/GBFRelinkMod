# game-autopilot — drive GBFR without a human at the keyboard

PowerShell 5.1 helpers that let a Claude session launch, control, and visually verify the
game solo. Proven in the 2026-07-10 autonomous UI-spanning verification
([docs/18](../../docs/18-ui-spanning-session2.md)): booted the game, navigated Graphics
settings, loaded a save, opened menus, screenshotted every step, and quit — with the save
file hash-verified untouched.

## The pieces

| Script | What it does |
| --- | --- |
| `sendkey.ps1 -Keys "enter,down,esc" [-HoldMs 80] [-GapMs 450]` | Scancode-level key presses via `SendInput` (focuses the game window first). Keys: enter, esc, space, tab, arrows, wasd, q/e. |
| `clickwin.ps1` | Clicks the center of the game window (absolute-coordinate mouse `SendInput`) — focuses reliably. |
| `screenshot.ps1 -OutFile shot.png` | Captures the whole virtual desktop, downscaled to 1376px wide. Works for windowed/borderless; exclusive fullscreen may capture black. |

## The workflow that works

1. **Launch**: `Start-Process "steam://rungameid/881020"`, then poll for the
   `granblue_fantasy_relink` process and a fresh `GBFRelinkFix.log`.
2. **Back up saves first**: copy `%LOCALAPPDATA%\GBFR\Saved\SaveGames\` somewhere, and
   `Get-FileHash` SaveData1.dat before/after the session to prove it untouched.
3. **Testing ultrawide without an ultrawide monitor**: set
   `[Custom Resolution] Width=1920 Height=800` (2.4:1) in GBFRelinkFix.ini and run the
   game **Windowed** — fits a 1080p desktop, fully screenshotable, and all >16:9 code
   paths engage. The game's saved resolution slot maps to the replaced 4K entry, so it
   often boots straight into the custom resolution.
4. **Move the window where you can see it**: the game may open on the wrong monitor;
   `SetWindowPos(hwnd, 0, x, y, 0, 0, SWP_NOSIZE|SWP_NOZORDER)`.
5. **Quit**: `Process.CloseMainWindow()` (graceful); force-kill only if it lingers.
   Standing in town/menus does not autosave — do not accept quests or talk to NPCs.

## Landmines (each cost real debugging time)

- **`INPUT` struct must be exactly 40 bytes on x64** (4 type + 4 pad + 32 union). A wrong
  size makes `SendInput` fail silently — keys "send" but nothing happens. The union is
  sized by MOUSEINPUT (32); KEYBDINPUT (24) needs one trailing ulong of padding.
- Use **scancodes with `KEYEVENTF_SCANCODE`**, not virtual keys — the game reads scancodes.
  Extended keys (arrows) need `KEYEVENTF_EXTENDEDKEY`.
- The game **pauses/ignores input when unfocused**; always focus (click or
  `SetForegroundWindow`) before sending keys, and verify with `GetForegroundWindow`.
- Game settings persist to `%LOCALAPPDATA%\GBFR\Saved\SaveGames\GraphicSetting.vdat` on
  apply. Anything you change in Graphics options stays changed — note it for the user.
- PS 5.1 `Set-Content` mangles UTF-8 (em-dashes become mojibake) — use the Write/Edit
  file tools for ini edits, or plain ASCII.
