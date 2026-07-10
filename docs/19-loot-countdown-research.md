# Auto-Loot / Instant Loot-Countdown — Research (2026-07-10)

Goal (Alex): make the end-of-quest chest countdown finish instantly, so the game's own
auto-collect path fires immediately (cleaner than force-opening chests).

## Verdict: the countdown is ENGINE CODE, not data

Everything data-side was ruled out systematically:

1. **`constant.tbl`** — fully reversed for 2.0 in the process (see below). The promising
   `ResultRewardScreenWaitTime` turns out to be **0.5s** — a UI input-block delay, not
   the chest countdown. No 30/60-second value exists in the row.
2. **All 249 converted tables** — swept every column name for `time|wait|sec|timer|
   countdown`: only BGM/staffroll/speedrun-grading hits. Nothing loot-related.
3. **Quest FSM data** (`system/fsm/quest/quest_<id>_N_fsm_ingame.msg`, decodable
   MessagePack — new tool `scripts/msg-to-json.mjs`): the quest-clear node is
   `ClearWithDrop` and it carries **no timer parameter** — it just triggers the
   engine's clear-with-drop sequence. (`CheckTimer time_:60` nodes exist but gate
   voice-line events.) `quest/<id>/sectionlist.msg`/`baseinfo.msg` similarly clean.
   Note: archive paths must be **lowercase** for extraction.

So the ~60s treasure countdown duration is initialized in native code — same category
as the (broken-on-2.0) auto-loot mods.

## Existing mods landscape (researched)

- Nexus 583 "Infinite Repeat and Skip Loot" (PokeNomz): exactly this feature set
  (Auto Loot Chest / Skip Reward Page / Infinite Repeat) but **broken on 2.0**
  (retitled "pre-endless ragnarok", author "might fix"), closed source, restrictive
  permissions. Its one extractable AOB has zero matches in the 2.0 exe.
- **NidasBot's CE table** ([FearLess t=40001](https://fearlessrevolution.com/viewtopic.php?t=40001),
  v0.4.5, actively updated for 2.0.2): has "Auto Loot Quest Chest" + "Skip Result
  Screen". CE tables are plaintext — its 2.0 addresses/AOBs are a legitimate
  **feature map** for locating the code sites (credit it; don't lift script logic).
  Download needs a FearLess account (manual step).

## Plan for the mod (code route)

1. Get the 2.0 code site: either from the CE table's script (fast, needs the .CT
   file) or by tracing the treasure-screen UI text hash → exe xref → disasm with
   `tools/disasm` (slower, fully independent).
2. Prefer patching the **countdown initialization** (write ~0 instead of 60.0) —
   Alex's "timer instantly finishes" spin — over force-opening chests: it reuses the
   game's own auto-collect flow (which provably grants all loot; vanilla behavior on
   timer expiry).
3. Ship as our own small mod (ASI like the ultrawide fix, or C# Reloaded hook like
   the MSP multiplier). Verify with one quest run.

## Bonus: `constant.tbl` 2.0 layout REVERSED (5th unlocked table)

1 row × 84 bytes (1.3.x was 72). 2.0 **reversed the 1.3 mid-section column order** and
appended 3 new columns; names recovered by value-anchoring against a 1.3.x copy (the
old Nexus uncap mod's table). Header: [patches/headers-2.0/constant.headers](../patches/headers-2.0/constant.headers)
(FLAT 2.0-only — GBFRDataTools 1.5.1 ignored an appended `set_min_version|2.0.0`
section, so the versioned 1.x header is kept as `constant.headers.1x.bak`).
Round-trips byte-identical. Notable constants now editable on 2.0:
`MaxTransmarvelStock` (999), `MaxLevelVoucherReward` (3 — the lvl-100 voucher grant),
`MaxLevelMSPReward` (100), `MaxLevelRepeatXP` (400000), gem-mix message thresholds.
Ambiguity flag: `BlacksmithLevelDialogWaitTime`/`ResultRewardScreenWaitTime` are both
0.5f — order chosen by the reversal pattern, unverified individually.
