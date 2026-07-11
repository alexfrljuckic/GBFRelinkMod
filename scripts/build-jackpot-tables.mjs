// =============================================================================
// Transmarvel Overhaul — jackpot pool rebuilder (Warpath+ dupe pruning)
// =============================================================================
//
// WHAT THIS DOES
//   Rebuilds the mod's gacha tables so that Warpath+ sigils you ALREADY OWN are
//   removed from the Transmarvel pool (character-sigil dupes are worthless).
//   The remaining sigils' odds are rebalanced to stay perfectly equal.
//
// REQUIREMENTS (one-time setup — see docs/06-toolchain-setup.md for details)
//   - Windows, with this repo checked out (paths below assume C:\dev\GBF)
//   - Node.js 18+ on PATH (check: node --version)
//   - Granblue Fantasy: Relink 2.0 installed (GAME path below)
//   - The repo toolchain in tools\ (gitignored downloads):
//       tools\GBFRDataTools\  — with this repo's 2.0 headers already installed
//       tools\sqlite\sqlite3.exe
//       tools\dotnet10\       — user-local .NET 10 runtime for GBFRDataTools
//
// HOW TO RUN
//   1. Leave OWNED_WARPATH = 'auto' (default): the script reads your save file
//      (read-only) and prunes exactly the Warpath+ sigils that can no longer
//      roll anything new for you (see AUTO-DETECT below). Or set it to
//      MANUAL_WARPATH — uncomment a line per sigil you want pruned outright.
//   2. Open a terminal in the repo root (C:\dev\GBF) and run:
//        node scripts/build-jackpot-tables.mjs
//   3. It writes the rebuilt table(s) into mods/transmarvel-overhaul/... and,
//      if the mod is installed at C:\Reloaded-II\Mods\gbfr.transmarvel.overhaul,
//      updates the installed copy automatically.
//
// HOW AUTO-DETECT WORKS (verified against a real 2.0.2 save, 2026-07-10)
//   A dupe is only worthless per COMBO, not per sigil: Warpath + DMG Cap is a
//   different sigil than Warpath + Cascade (the random 2nd trait). So a
//   Warpath+ only leaves the pool once you own it with ALL 18 secondaries the
//   mod can roll (the SKL_TMV_GOOD list). Auto-detect reads both halves from
//   SaveData<N>.dat (docs/21 has the full format):
//     - sigil inventory: 24-byte records { listType, seq, 4, 1, hash, tag },
//       one per owned copy, hash = custom XXHash32 of the GEEN_* id;
//     - trait assignments: a parallel skill pool whose seq encodes
//       instance*100 + traitSlot; slot 0 = innate trait (its SKILL number
//       always equals the sigil's GEEN number — that redundancy is used to
//       cross-validate the alignment on every run), slot 1 = the random 2nd.
//   The save is only ever read, never written. Every run re-derives offsets
//   and self-validates; if the format shifts in a game update it fails loudly
//   and tells you to use the manual list.
//
// DOES IT WORK WHILE THE GAME IS RUNNING?
//   You can RUN the script any time (it only writes files), but the game reads
//   mod tables ONCE, at launch (the GBFR Mod Manager stages them during boot).
//   Changes take effect on the NEXT game launch — restart the game after
//   running this. Nothing else to click in Reloaded-II.
//
// =============================================================================

import { execSync } from 'node:child_process';
import { mkdirSync, rmSync, copyFileSync, existsSync, unlinkSync, readFileSync, readdirSync, statSync } from 'node:fs';

// ============================ CONFIG =========================================
// To pick by hand instead of auto-detecting, uncomment each Warpath+ sigil you
// already own (these are the 28 in the pool):
const MANUAL_WARPATH = [
  // 'GEEN_114_93', // Fearless Heart+
  // 'GEEN_115_93', // Guardian's Warpath+
  // 'GEEN_116_93', // Helmsman's Warpath+
  // 'GEEN_117_93', // Mage's Warpath+
  // 'GEEN_118_93', // Veteran's Warpath+
  // 'GEEN_119_93', // Rose's Warpath+
  // 'GEEN_120_93', // Phantasm's Warpath+
  // 'GEEN_121_93', // White Dragon's Warpath+
  // 'GEEN_122_93', // Hero's Warpath+
  // 'GEEN_123_93', // Lord's Warpath+
  // 'GEEN_124_93', // Dragonslayer's Warpath+
  // 'GEEN_125_93', // Holy Knight's Warpath+
  // 'GEEN_126_93', // Swordmaster's Warpath+
  // 'GEEN_127_93', // Butterfly's Warpath+
  // 'GEEN_128_93', // Eternal Rage's Warpath+
  // 'GEEN_129_93', // Founder's Warpath+
  // 'GEEN_130_93', // Versalis Heart+
  // 'GEEN_131_93', // Crimson's Warpath+
  // 'GEEN_132_93', // Ebony's Warpath+
  // 'GEEN_170_93', // Spirit Edge's Warpath+
  // 'GEEN_171_93', // Dark Huntress's Warpath+
  // 'GEEN_172_93', // Supreme Primarch's Warpath+
  // 'GEEN_173_93', // Gladiator's Warpath+
  // 'GEEN_174_93', // Bladequeen's Warpath+
  // 'GEEN_175_93', // Ultramarine's Warpath+
  // 'GEEN_176_93', // Thunderwolf's Warpath+
  // 'GEEN_177_93', // Enchantress's Warpath+
  // 'GEEN_178_93', // The Black's Warpath+
];

// 'auto' = detect owned Warpath+ sigils from your save file (recommended, read-only).
// Change to MANUAL_WARPATH to use the hand-picked list above instead.
const OWNED_WARPATH = 'auto';

// Adjust if your paths differ:
const GAME = 'D:\\Steam\\steamapps\\common\\Granblue Fantasy Relink';
const RELOADED_MOD = 'C:\\Reloaded-II\\Mods\\gbfr.transmarvel.overhaul\\GBFR\\data\\system\\table';
const SAVE_DIR = `${process.env.LOCALAPPDATA}\\GBFR\\Saved\\SaveGames`; // for 'auto'
// =============================================================================

const WARPATH_NAMES = {
  GEEN_114_93: "Fearless Heart+", GEEN_115_93: "Guardian's Warpath+", GEEN_116_93: "Helmsman's Warpath+",
  GEEN_117_93: "Mage's Warpath+", GEEN_118_93: "Veteran's Warpath+", GEEN_119_93: "Rose's Warpath+",
  GEEN_120_93: "Phantasm's Warpath+", GEEN_121_93: "White Dragon's Warpath+", GEEN_122_93: "Hero's Warpath+",
  GEEN_123_93: "Lord's Warpath+", GEEN_124_93: "Dragonslayer's Warpath+", GEEN_125_93: "Holy Knight's Warpath+",
  GEEN_126_93: "Swordmaster's Warpath+", GEEN_127_93: "Butterfly's Warpath+", GEEN_128_93: "Eternal Rage's Warpath+",
  GEEN_129_93: "Founder's Warpath+", GEEN_130_93: "Versalis Heart+", GEEN_131_93: "Crimson's Warpath+",
  GEEN_132_93: "Ebony's Warpath+", GEEN_170_93: "Spirit Edge's Warpath+", GEEN_171_93: "Dark Huntress's Warpath+",
  GEEN_172_93: "Supreme Primarch's Warpath+", GEEN_173_93: "Gladiator's Warpath+", GEEN_174_93: "Bladequeen's Warpath+",
  GEEN_175_93: "Ultramarine's Warpath+", GEEN_176_93: "Thunderwolf's Warpath+", GEEN_177_93: "Enchantress's Warpath+",
  GEEN_178_93: "The Black's Warpath+",
};

const TOOL = 'tools\\GBFRDataTools\\GBFRDataTools.exe';
const SQL = 'tools\\sqlite\\sqlite3.exe';
const WORK = 'extracted/jackpot_build';
const MOD_TABLES = 'mods/transmarvel-overhaul/gbfr.transmarvel.overhaul/GBFR/data/system/table';
const run = (cmd) => execSync(cmd, { stdio: ['ignore', 'pipe', 'pipe'], env: { ...process.env, DOTNET_ROOT: 'C:\\dev\\GBF\\tools\\dotnet10' } }).toString();
const q = (sql) => run(`${SQL} ${WORK}/j.sqlite "${sql.replace(/\s+/g, ' ').replace(/"/g, '""')}"`).trim();

// --- sanity checks with actionable messages ---
for (const [what, path, hint] of [
  ['GBFRDataTools', TOOL, 'download it + install the 2.0 headers — docs/06 + docs/11'],
  ['sqlite3', SQL, 'download the sqlite CLI zip into tools\\sqlite\\ — docs/06'],
  ['the game', `${GAME}\\data.i`, 'fix the GAME path in the CONFIG block'],
  ['the mod folder', MOD_TABLES, 'run from the repo root: cd C:\\dev\\GBF'],
]) {
  if (!existsSync(path.replace(/\\/g, '/'))) {
    console.error(`MISSING ${what}: ${path}\n  → ${hint}`);
    process.exit(1);
  }
}
// --- GBFR custom XXHash32 (same port as scripts/decode-transmarvel-pool.mjs:
// seed 0x178A54A4, hardcoded v1..v4, do/while(p>16) quirk) ---
function xxh32(str) {
  const data = Buffer.from(str, 'ascii');
  const P1 = 0x9e3779b1, P2 = 0x85ebca77, P3 = 0xc2b2ae3d, P4 = 0x27d4eb2f, P5 = 0x165667b1;
  const rotl = (x, r) => ((x << r) | (x >>> (32 - r))) >>> 0;
  const mul = (a, b) => Math.imul(a, b) >>> 0;
  const round = (acc, input) => mul(rotl((acc + mul(input, P2)) >>> 0, 13), P1);
  const len = data.length;
  let h = 0x178A54A4, i = 0;
  if (len >= 16) {
    let v1 = 0x2557311B, v2 = 0x871FB76A, v3 = 0x0133ECF3, v4 = 0x62FC7342;
    do {
      v1 = round(v1, data.readUInt32LE(i));
      v2 = round(v2, data.readUInt32LE(i + 4));
      v3 = round(v3, data.readUInt32LE(i + 8));
      v4 = round(v4, data.readUInt32LE(i + 12));
      i += 16;
    } while (len - i > 16);
    h = (rotl(v1, 1) + rotl(v2, 7) + rotl(v3, 12) + rotl(v4, 18)) >>> 0;
  }
  h = (h + len) >>> 0;
  for (; len - i >= 4; i += 4) h = mul(rotl((h + mul(data.readUInt32LE(i), P3)) >>> 0, 17), P4);
  for (; i < len; i++) h = mul(rotl((h + mul(data[i], P5)) >>> 0, 11), P1);
  h = mul(h ^ (h >>> 15), P2);
  h = mul(h ^ (h >>> 13), P3);
  return (h ^ (h >>> 16)) >>> 0;
}

// The 18 secondaries the mod can roll (skill_lot sub-lot SKL_TMV_GOOD `485F6215`):
const CURATED_18 = {
  SKILL_020_00: 'DMG Cap', SKILL_027_00: 'Tyranny', SKILL_006_00: 'Stamina',
  SKILL_013_00: 'Critical Hit DMG', SKILL_014_00: 'Weak Point DMG',
  SKILL_030_00: 'Overdrive Assassin', SKILL_031_00: 'Break Assassin',
  SKILL_083_00: 'Skilled Assault', SKILL_029_00: 'Injury to Insult',
  SKILL_111_00: 'Quick Charge', SKILL_070_00: 'Cascade', SKILL_069_00: 'Quick Cooldown',
  SKILL_066_00: 'Regen', SKILL_072_00: 'Uplift', SKILL_045_00: 'Guts',
  SKILL_068_00: 'Autorevive', SKILL_073_00: 'Potion Hoarder', SKILL_094_00: 'Steady Focus',
};

// --- read owned (Warpath+ × secondary) combos from the newest save (read-only) ---
function detectPrunableFromSave() {
  const die = (msg) => {
    console.error(`AUTO-DETECT: ${msg}\n  → set OWNED_WARPATH = MANUAL_WARPATH and pick sigils by hand instead.`);
    process.exit(1);
  };
  if (!existsSync(SAVE_DIR)) die(`save folder not found: ${SAVE_DIR}`);
  const slots = readdirSync(SAVE_DIR).filter(f => /^SaveData\d+\.dat$/i.test(f));
  if (!slots.length) die(`no SaveData<N>.dat in ${SAVE_DIR}`);
  const save = slots.map(f => `${SAVE_DIR}\\${f}`).sort((a, b) => statSync(b).mtimeMs - statSync(a).mtimeMs)[0];
  const buf = readFileSync(save);
  const EMPTY = 0x887ae0b0; // the game's blank-hash marker

  const geenRev = new Map(), skillRev = new Map();
  for (let a = 0; a <= 999; a++) {
    for (let b = 0; b <= 99; b++)
      geenRev.set(xxh32(`GEEN_${String(a).padStart(3, '0')}_${String(b).padStart(2, '0')}`), { n: a });
    for (let b = 0; b <= 10; b++)
      skillRev.set(xxh32(`SKILL_${String(a).padStart(3, '0')}_${String(b).padStart(2, '0')}`), { n: a, id: `SKILL_${String(a).padStart(3, '0')}_${String(b).padStart(2, '0')}` });
  }

  // find a contiguous array of 24-byte records [type, seq, 4, 1, hash, tag] whose
  // hashes match `rev` — requiring duplicate hashes rejects the all-unique catalog
  const locate = (rev, what) => {
    for (let off = 0; off + 24 <= buf.length; off += 4) {
      if (buf.readUInt32LE(off + 8) !== 4 || buf.readUInt32LE(off + 12) !== 1) continue;
      if (!rev.has(buf.readUInt32LE(off + 16))) continue;
      const type = buf.readUInt32LE(off);
      if (type === 0 || type > 0xffff) continue;
      let s = off, e = off;
      while (s - 24 >= 0 && buf.readUInt32LE(s - 24) === type) s -= 24;
      while (e + 48 <= buf.length && buf.readUInt32LE(e + 24) === type) e += 24;
      const count = (e - s) / 24 + 1;
      if (count >= 1000) {
        const seen = new Set(); let dup = 0;
        for (let i = 0; i < count; i++) {
          const h = buf.readUInt32LE(s + 24 * i + 16);
          if (h !== EMPTY) { if (seen.has(h)) dup++; else seen.add(h); }
        }
        if (dup > 10) return { start: s, count };
      }
      off = e;
    }
    die(`could not find the ${what} array — save layout changed?`);
  };
  const sig = locate(geenRev, 'sigil inventory');
  const skl = locate(skillRev, 'trait pool');
  const recAt = (a, i) => ({ seq: buf.readUInt32LE(a.start + 24 * i + 4), hash: buf.readUInt32LE(a.start + 24 * i + 16) });

  // trait pool: seq = instanceN*100 + slot (slot 0 innate, 1 secondary)
  const traitPairs = new Map();
  for (let i = 0; i < skl.count; i++) {
    const r = recAt(skl, i);
    const N = Math.floor(r.seq / 100);
    if (!traitPairs.has(N)) traitPairs.set(N, {});
    traitPairs.get(N)[r.seq % 100] = r.hash;
  }

  // align sigil slot j <-> trait pair (C - j): the innate trait's SKILL number
  // always equals the sigil's GEEN number, so C falls out by consensus vote
  const filled = [];
  for (let j = 0; j < sig.count; j++) {
    const g = geenRev.get(recAt(sig, j).hash);
    if (g) filled.push({ j, n: g.n, hash: recAt(sig, j).hash });
  }
  if (!filled.length) die('sigil inventory is empty?');
  const votes = new Map();
  for (const { j, n } of filled)
    for (const [N, p] of traitPairs) {
      const s0 = skillRev.get(p[0]);
      if (s0 && s0.n === n) votes.set(N + j, (votes.get(N + j) || 0) + 1);
    }
  const C = [...votes.entries()].sort((a, b) => b[1] - a[1])[0][0];
  let match = 0;
  for (const { j, n } of filled) {
    const s0 = skillRev.get((traitPairs.get(C - j) || {})[0]);
    if (s0 && s0.n === n) match++;
  }
  // demand near-perfect cross-validation (the odd _94 reissue sigil is exempt)
  if (match / filled.length < 0.99)
    die(`alignment cross-check failed (${match}/${filled.length} innate traits match) — save layout changed?`);

  // owned secondaries per Warpath+ sigil
  const warpathHash = new Map(Object.keys(WARPATH_NAMES).map(id => [xxh32(id), id]));
  const combos = new Map();
  for (const { j, hash } of filled) {
    const wid = warpathHash.get(hash);
    if (!wid) continue;
    if (!combos.has(wid)) combos.set(wid, new Set());
    const sec = skillRev.get((traitPairs.get(C - j) || {})[1]);
    if (sec && CURATED_18[sec.id]) combos.get(wid).add(sec.id);
  }

  console.log(`Auto-detected from ${save} (${filled.length} sigils, alignment ${match}/${filled.length}):`);
  const prunable = [];
  for (const id of Object.keys(WARPATH_NAMES)) {
    const own = combos.get(id);
    const cov = own ? own.size : 0;
    if (cov === Object.keys(CURATED_18).length) {
      prunable.push(id);
      console.log(`  ${WARPATH_NAMES[id]}: 18/18 combos owned -> PRUNED from pool`);
    } else {
      const missing = Object.keys(CURATED_18).filter(s => !own || !own.has(s));
      console.log(`  ${WARPATH_NAMES[id]}: ${cov}/18 combos — kept (missing: ${missing.slice(0, 4).map(s => CURATED_18[s]).join(', ')}${missing.length > 4 ? `, +${missing.length - 4} more` : ''})`);
    }
  }
  return prunable;
}

let owned;
if (OWNED_WARPATH === 'auto') {
  owned = detectPrunableFromSave();
} else {
  owned = OWNED_WARPATH.map(s => s.toUpperCase().trim());
  const bad = owned.filter(id => !WARPATH_NAMES[id]);
  if (bad.length) {
    console.error(`Unknown OWNED_WARPATH id(s): ${bad.join(', ')}\n  → use ids from the list in this script (GEEN_xxx_93).`);
    process.exit(1);
  }
}

// --- rebuild from vanilla game data ---
rmSync(WORK, { recursive: true, force: true });
mkdirSync(`${WORK}/tbl`, { recursive: true });
const VANILLA_CACHE = 'extracted/2.0/system/table'; // repo copy of vanilla 2.0 tables
try {
  for (const t of ['gacha_rate_group', 'gacha_lot'])
    run(`${TOOL} extract -i "${GAME}\\data.i" -f "system/table/${t}.tbl" -o ${WORK}/x`);
  run(`cmd /c copy /y ${WORK.replace(/\//g, '\\\\')}\\x\\system\\table\\*.tbl ${WORK.replace(/\//g, '\\\\')}\\tbl >nul`);
} catch (e) {
  // the running game locks data.0 — fall back to the repo's vanilla extraction
  if (['gacha_rate_group', 'gacha_lot'].every(t => existsSync(`${VANILLA_CACHE}/${t}.tbl`))) {
    console.log('NOTE: could not read the game archive (game running?) — using the vanilla tables cached in extracted/2.0/.');
    for (const t of ['gacha_rate_group', 'gacha_lot'])
      copyFileSync(`${VANILLA_CACHE}/${t}.tbl`, `${WORK}/tbl/${t}.tbl`);
  } else {
    console.error('Could not extract vanilla tables from the game archive — close the game (it locks data.0) or fix the GAME path, then re-run.');
    process.exit(1);
  }
}
run(`${TOOL} tbl-to-sqlite -i ${WORK}/tbl -o ${WORK}/j.sqlite -v 2.0.2`);

if (owned.length) {
  // rows whose id is missing from the tool's ids.txt export as the raw hash
  // (e.g. GEEN_178_93 -> '9ABD2DA5'), so match both spellings
  const keys = owned.flatMap(s => [s, xxh32(s).toString(16).toUpperCase().padStart(8, '0')]);
  q(`DELETE FROM gacha_lot WHERE Key='F527EF32' AND ItemId IN (${keys.map(s => `'${s}'`).join(',')});`);
  console.log('Removed from pool:');
  for (const id of owned) console.log(`  - ${WARPATH_NAMES[id]} (${id})`);
}
const remaining = +q(`SELECT COUNT(*) FROM gacha_lot WHERE Key='F527EF32';`);

// per-item weight 250 everywhere → all sigils stay equal regardless of removals
q(`UPDATE gacha_rate_group SET Weight = CASE GachaLotId
    WHEN '6E52A69A' THEN 1250 WHEN '36879ED7' THEN 2000 WHEN 'F527EF32' THEN ${250 * remaining}
    ELSE 0 END WHERE Key = '27509C51';`);
q(`UPDATE gacha_rate_group SET Weight = CASE GachaLotId WHEN 'BD1CBF1C' THEN 10000 ELSE 0 END WHERE Key = '67716D8A';`);

run(`${TOOL} sqlite-to-tbl -i ${WORK}/j.sqlite -o ${WORK}/out -v 2.0.2`);

// --- write into the repo mod folder + the installed Reloaded-II copy ---
const dests = [MOD_TABLES];
if (existsSync(RELOADED_MOD)) dests.push(RELOADED_MOD);
for (const d of dests) {
  copyFileSync(`${WORK}/out/gacha_rate_group.tbl`, `${d}/gacha_rate_group.tbl`);
  if (owned.length) copyFileSync(`${WORK}/out/gacha_lot.tbl`, `${d}/gacha_lot.tbl`);
  else if (existsSync(`${d}/gacha_lot.tbl`)) unlinkSync(`${d}/gacha_lot.tbl`);
}
console.log(`\nPool: ${5 + 8 + remaining} sigils at ~${(100 / (5 + 8 + remaining)).toFixed(2)}% each (${remaining}/28 Warpath+ remain).`);
console.log(`Updated: ${dests.join('  and  ')}`);
if (!existsSync(RELOADED_MOD))
  console.log(`NOTE: installed mod not found at ${RELOADED_MOD} — copy the mod folder to Reloaded-II\\Mods yourself.`);
console.log('RESTART THE GAME to apply (mod tables are read once, at launch).');
