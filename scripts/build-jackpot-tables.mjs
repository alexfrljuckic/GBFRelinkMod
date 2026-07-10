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
//   1. Edit OWNED_WARPATH below — uncomment a line for each Warpath+ sigil you
//      own (the full id → name list is right there).
//   2. Open a terminal in the repo root (C:\dev\GBF) and run:
//        node scripts/build-jackpot-tables.mjs
//   3. It writes the rebuilt table(s) into mods/transmarvel-overhaul/... and,
//      if the mod is installed at C:\Reloaded-II\Mods\gbfr.transmarvel.overhaul,
//      updates the installed copy automatically.
//
// DOES IT WORK WHILE THE GAME IS RUNNING?
//   You can RUN the script any time (it only writes files), but the game reads
//   mod tables ONCE, at launch (the GBFR Mod Manager stages them during boot).
//   Changes take effect on the NEXT game launch — restart the game after
//   running this. Nothing else to click in Reloaded-II.
//
// =============================================================================

import { execSync } from 'node:child_process';
import { mkdirSync, rmSync, copyFileSync, existsSync, unlinkSync } from 'node:fs';

// ============================ CONFIG =========================================
// Uncomment each Warpath+ sigil you already own (these are the 28 in the pool):
const OWNED_WARPATH = [
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

// Adjust if your paths differ:
const GAME = 'D:\\Steam\\steamapps\\common\\Granblue Fantasy Relink';
const RELOADED_MOD = 'C:\\Reloaded-II\\Mods\\gbfr.transmarvel.overhaul\\GBFR\\data\\system\\table';
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
const owned = OWNED_WARPATH.map(s => s.toUpperCase().trim());
const bad = owned.filter(id => !WARPATH_NAMES[id]);
if (bad.length) {
  console.error(`Unknown OWNED_WARPATH id(s): ${bad.join(', ')}\n  → use ids from the list in this script (GEEN_xxx_93).`);
  process.exit(1);
}

// --- rebuild from vanilla game data ---
rmSync(WORK, { recursive: true, force: true });
mkdirSync(`${WORK}/tbl`, { recursive: true });
for (const t of ['gacha_rate_group', 'gacha_lot'])
  run(`${TOOL} extract -i "${GAME}\\data.i" -f "system/table/${t}.tbl" -o ${WORK}/x`);
run(`cmd /c copy /y ${WORK.replace(/\//g, '\\\\')}\\x\\system\\table\\*.tbl ${WORK.replace(/\//g, '\\\\')}\\tbl >nul`);
run(`${TOOL} tbl-to-sqlite -i ${WORK}/tbl -o ${WORK}/j.sqlite -v 2.0.2`);

if (owned.length) {
  q(`DELETE FROM gacha_lot WHERE Key='F527EF32' AND ItemId IN (${owned.map(s => `'${s}'`).join(',')});`);
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
