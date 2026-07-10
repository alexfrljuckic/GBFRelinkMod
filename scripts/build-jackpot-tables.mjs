// Regenerate the Transmarvel Jackpot mod tables (gacha_rate_group + optionally gacha_lot).
//
// As you collect character Warpath+ sigils, add their GEEN ids to OWNED_WARPATH below and
// re-run: they're removed from the pool (dupes are worthless) and the remaining sigils'
// odds stay perfectly equal.
//
// Usage: node scripts/build-jackpot-tables.mjs
//   (needs the game at GAME, GBFRDataTools with 2.0 headers, DOTNET_ROOT set — see docs/11)
import { execSync } from 'node:child_process';
import { readFileSync, mkdirSync, rmSync, copyFileSync, existsSync, unlinkSync } from 'node:fs';

// ===================== CONFIG =====================
const OWNED_WARPATH = [
  // e.g. 'GEEN_125_93',  // Holy Knight's Warpath+ (Vane)
  // e.g. 'GEEN_114_93',  // Fearless Heart+... — full list: grep _93 extracted/geen-names-en.tsv
];
// ==================================================

const GAME = 'D:\\Steam\\steamapps\\common\\Granblue Fantasy Relink';
const TOOL = 'tools\\GBFRDataTools\\GBFRDataTools.exe';
const SQL = 'tools\\sqlite\\sqlite3.exe';
const WORK = 'extracted/jackpot_build';
const MOD_TABLES = 'mods/transmarvel-overhaul/gbfr.transmarvel.overhaul/GBFR/data/system/table';
const run = (cmd) => execSync(cmd, { stdio: ['ignore', 'pipe', 'pipe'], env: { ...process.env, DOTNET_ROOT: 'C:\\dev\\GBF\\tools\\dotnet10' } }).toString();
const q = (sql) => run(`${SQL} ${WORK}/j.sqlite "${sql.replace(/\s+/g, ' ').replace(/"/g, '""')}"`).trim();

rmSync(WORK, { recursive: true, force: true });
mkdirSync(`${WORK}/tbl`, { recursive: true });
for (const t of ['gacha_rate_group', 'gacha_lot'])
  run(`${TOOL} extract -i "${GAME}\\data.i" -f "system/table/${t}.tbl" -o ${WORK}/x`);
run(`cmd /c copy /y ${WORK.replace(/\//g, '\\\\')}\\x\\system\\table\\*.tbl ${WORK.replace(/\//g, '\\\\')}\\tbl >nul`);
run(`${TOOL} tbl-to-sqlite -i ${WORK}/tbl -o ${WORK}/j.sqlite -v 2.0.2`);

// remove owned Warpath+ sigils from the Warpath bucket
if (OWNED_WARPATH.length) {
  const list = OWNED_WARPATH.map(s => `'${s}'`).join(',');
  q(`DELETE FROM gacha_lot WHERE Key='F527EF32' AND ItemId IN (${list});`);
}
const remaining = +q(`SELECT COUNT(*) FROM gacha_lot WHERE Key='F527EF32';`);
console.log(`Warpath+ sigils remaining in pool: ${remaining} (removed: ${OWNED_WARPATH.length})`);

// per-item weight 250 everywhere → all sigils equal regardless of removals
q(`UPDATE gacha_rate_group SET Weight = CASE GachaLotId
    WHEN '6E52A69A' THEN 1250 WHEN '36879ED7' THEN 2000 WHEN 'F527EF32' THEN ${250 * remaining}
    ELSE 0 END WHERE Key = '27509C51';`);
q(`UPDATE gacha_rate_group SET Weight = CASE GachaLotId WHEN 'BD1CBF1C' THEN 10000 ELSE 0 END WHERE Key = '67716D8A';`);

run(`${TOOL} sqlite-to-tbl -i ${WORK}/j.sqlite -o ${WORK}/out -v 2.0.2`);
copyFileSync(`${WORK}/out/gacha_rate_group.tbl`, `${MOD_TABLES}/gacha_rate_group.tbl`);
if (OWNED_WARPATH.length) {
  copyFileSync(`${WORK}/out/gacha_lot.tbl`, `${MOD_TABLES}/gacha_lot.tbl`);
  console.log('Wrote gacha_rate_group.tbl + gacha_lot.tbl to the mod folder.');
} else {
  if (existsSync(`${MOD_TABLES}/gacha_lot.tbl`)) unlinkSync(`${MOD_TABLES}/gacha_lot.tbl`);
  console.log('Wrote gacha_rate_group.tbl (no removals → vanilla gacha_lot, not shipped).');
}
console.log(`Total sigil pool: ${5 + 8 + remaining} options at ~${(100 / (5 + 8 + remaining)).toFixed(2)}% each.`);
console.log('Remember to copy the mod folder to Reloaded-II\\Mods (or re-run the install copy).');
