# Sigil Synthesis Control (2.0)

Sigil synthesis at Siero's Knickknack Shack, fully under your control. Two
independent features, both toggleable in **Configure Mod**:

### 1. Always Grand Success (default ON)

Every synthesis rolls a Grand Success, so the result's traits come out **maxed at
Lv15**. Vanilla rolls Success/Great/Grand, with Grand odds between 45% and 85%
depending on the Prospect score of your two inputs; this pins Grand to 100% at
every score.

### 2. Choose result traits (default ON)

Vanilla picks the result's two traits **at random** from the four traits of your
two input sigils — that's exactly what the *"1/12 Prospects"* counter on the
synthesis screen is cycling through (4 traits → 4×3 = 12 ordered pairs). Turn this
on and you pick which two, every time:

- **result trait 1 (innate)** ← default: *Sigil A (1st / left), trait 1*
- **result trait 2 (secondary)** ← default: *Sigil B (2nd / right), trait 2*

**Sigil A is the first slot you fill (the left one); Sigil B is the second (right).**
The defaults give "first trait of the first sigil + second trait of the second".
**Trait 1 also decides which sigil you get** — if it's an ATK trait, the result is
an Attack Power sigil. Swapping the two inputs mirrors the outcome accordingly.

> Example (live-verified): *Attack Power V+* (ATK, Skilled Assault) first ⊕ *Steady
> Focus V+* (Steady Focus, Potion Hoarder) second → vanilla rolled **Potion Hoarder
> V+** with Potion Hoarder + Steady Focus; with this mod it deterministically
> produces **Attack Power V+ Lv15 with ATK + Potion Hoarder**. Feed the same two in
> the reverse order and you get **Steady Focus V+ with Steady Focus + Skilled
> Assault** instead.

Traits apply **immediately** (no restart). The Grand Success toggle needs a game
restart, since tables are read once at launch.

## Notes & limits

- The game only makes results with **two different traits**. If your two picks
  resolve to the same trait, that synthesis is left vanilla (random) rather than
  forging an impossible sigil — the log says so when it happens.
- Only applies when both inputs have two traits each (the normal Legendary (+)
  case); anything else is left vanilla.
- **Conflicts:** disable the Nexus mod *"Sigil Synthesis All Grand Success"*
  (`gbfr.gem_mix`) — both supply `gem_mix_success.tbl`.

## How it works

No static tables, nothing modified on disk.

- *Grand Success* reads the vanilla `gem_mix_success.tbl` from the game archive at
  every launch (via the GBFR Mod Manager) and rewrites all 28 Prospect rows to
  `Great 0 / Grand 10000`. A layout guard refuses to patch if a game update changes
  the row size.
- *Choose result traits* installs two tiny assembly hooks in the synthesis routine:
  one captures the four input traits while they're still in source order, the other
  imposes your chosen pair just before the game commits the result. Both sites are
  found by **unique byte-pattern scan** — if a game update moves that code, the
  pattern stops matching and the feature **disables itself with a warning** rather
  than injecting into the wrong place (Grand Success keeps working).

Full reverse-engineering write-up: [docs/24](../../docs/24-sigil-synthesis-recon.md).

## Install

1. Install [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) and the
   [GBFR Mod Manager 2.0.1+](https://www.nexusmods.com/granbluefantasyrelink/mods/526)
   (`gbfrelink.utility.manager`).
2. Extract the zip from the latest
   [synthesis-control release](https://github.com/alexfrljuckic/GBFRelinkMod/releases)
   — copy `gbfr.synthesis.control` into your `Reloaded-II/Mods/` folder.
3. Enable **Sigil Synthesis Control (2.0)** for Granblue Fantasy: Relink.
   (It also needs `reloaded.sharedlib.hooks`, which Reloaded pulls in automatically.)
4. Launch through Reloaded. The console reports both features, e.g.
   `Always Grand Success applied: all 28 Prospect rows set to 100% Grand.` and
   `Choose result traits: ON — trait 1 <- SigilA_Trait1, trait 2 <- SigilB_Trait2.`
