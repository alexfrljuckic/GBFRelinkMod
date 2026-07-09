<!--
Ready-to-post comment for https://codeberg.org/Lyall/GBFRelinkFix/issues/1
Alex: review, then paste into the issue (or the Relink Modding Discord). Requires your
Codeberg login — Claude has no credentials for codeberg.org, so it can't post this.
Adjust the "happy to test" offer to taste.
-->

I had a look at how the `ragnarok` branch holds up against the **release 2.0 build**
(Steam, exe dated 2026-07-08). I scanned the shipped exe with the 15 signatures from
that branch — **13 still match, 11 of them uniquely.** So the rewrite is very close;
it looks like it just predates the final build. Details on the three that need attention:

**1. `Press Any Key` — one instruction drifted.** The trailing `mov r?, [r14+0x1A0]`
became a `cmp` in the release build (`49 8B` → `49 3B`). The bytes the patch NOPs (the
leading `jz`) are unchanged, so only the signature needs the last `mov` swapped for the
`cmp`:

```
0F 84 ?? ?? ?? ?? 4D 85 ?? 0F 84 ?? ?? ?? ?? 49 8B ?? ?? ?? ?? ?? 49 3B ?? ?? ?? ?? ??
```

This matches uniquely in the release exe.

**2. `HUD: Markers` — `vdivss` folded out.** The sequence survives as
`vmulss` + `vaddsubps` + `movzx`, but the `movzx` is now `rsi`-relative
(`0F B6 86 48 03 00 00` = `movzx eax, byte [rsi+0x348]`) rather than the r14 base the
demo build used. So both the signature *and* the hook body's struct offsets
(`r14+0x1B4/+0x1B8`) likely need revisiting — I didn't want to guess those without a
live debugger.

**3. `Resolution List` — now ambiguous.** The current signature matches **4 sites** in
the release exe, so first-match could patch the wrong one; probably worth lengthening it.

Everything else (Current Resolution, Resolution String, Startup Aspect Ratio, both FOV
hooks, HUD Scale, Shadow Resolution, TAA, LOD, both Framerate hooks, Intro Logos) still
resolves uniquely.

I'm on a 2.0 install and happy to test builds / help chase down the Markers offsets in
x64dbg if that's useful.
