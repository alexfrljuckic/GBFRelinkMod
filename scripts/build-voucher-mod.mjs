// ⚠️ SUPERSEDED 2026-07-10: voucher income is now generated at load time by the
// C# component in mods-src/gbfr.transmarvel.overhaul (count configurable in
// Reloaded-II, default x10). Kept for reference — the C# port was verified to
// patch the exact same 112 reward rows/slots this script did.
//
// Build the "Transmarvel Vouchers from Chaos+ quests" mod tables.
// Adds guaranteed Transmarvel Voucher (ITEM_21_0001) lots to every Chaos-and-above
// quest's per-clear reward rows (RW_<qid>_100 / _101), scaled by tier:
//   Chaos (4083xx) & Chaos+ (4093xx) = 1, Chaos++ (40A3xx) = 2, Infinity (40B3xx) = 3.
// Reads extracted/vch_work/vch.sqlite (reward + reward_lot) and edits it in place.
// Usage: node scripts/build-voucher-mod.mjs
import { execSync } from 'node:child_process';
import { readFileSync } from 'node:fs';

const SQL = 'tools\\sqlite\\sqlite3.exe';
const DB = 'extracted/vch_work/vch.sqlite';
const QB = 'extracted/qb_work/qb.sqlite';
const q = (db, sql) => execSync(`${SQL} ${db} "${sql.replace(/"/g, '""')}"`).toString().trim();

// --- GBFR custom XXHash32 (verified against ids.txt) ---
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
const hex = (n) => n.toString(16).toUpperCase().padStart(8, '0');

// quest names for logging
const names = new Map();
for (const line of readFileSync('extracted/quest-names-en.tsv', 'utf8').split('\n')) {
  const m = line.match(/^TXT_QR_(\w+)\t(.*)$/);
  if (m) names.set(m[1], m[2]);
}

// --- 1. Chaos+ quest list from the quest catalog ---
const BANDS = { '004083': 1, '004093': 1, '0040A3': 2, '0040B3': 3 }; // band -> voucher count
const quests = q(QB, `SELECT Key, AdvisedPWR FROM quest_baseinfo_ex_data WHERE substr(Key,1,6) IN ('004083','004093','0040A3','0040B3') ORDER BY Key;`)
  .split('\n').filter(Boolean).map(l => { const [k, p] = l.split('|'); return { qid: k.slice(2), band: k.slice(0, 6), pwr: +p }; });
console.log(`Chaos+ quests in catalog: ${quests.length}`);

// --- 2. add the three guaranteed voucher lots to reward_lot ---
// Template = the guaranteed-item lot shape (e.g. lot BE7B60BF: ITEM_10_0001 x1, Weight 10000).
for (const n of [1, 2, 3]) {
  q(DB, `INSERT INTO reward_lot (Key, Unk04, ItemId, WeaponId, GemId, Unk14, RewardRank, PhaseItemIndex, AmountGiven, WeaponLevel, WeaponUncap, GemCount, Weight, NumExtraGemTraits, StoryDifficulty, pad1, pad2, pad3) VALUES (5, 0, 'RWL_TMV_${n}', 'ITEM_21_0001', '', '', -1, -1, ${n}, 0, 0, 1, 10000, -1, 255, 0, 0, 0);`);
  console.log(`reward_lot += RWL_TMV_${n} (${hex(xxh32('RWL_TMV_' + n))}) = ${n}x Transmarvel Voucher`);
}

// --- 3. wire each quest's per-clear reward rows to the right lot ---
const SLOTS = ['100', '101'];
const LOTCOLS = ['RewardLotId2', 'RewardLotId3', 'RewardLotId4', 'RewardLotId5', 'RewardLotId6']; // skip slot1 (Lot1ExclusionChance)
let patched = 0, missing = [], full = [];
for (const { qid, band } of quests) {
  const lot = `RWL_TMV_${BANDS[band]}`;
  let hitAny = false;
  for (const slot of SLOTS) {
    const name = `RW_${qid}_${slot}`;
    const keys = [name, hex(xxh32(name))];
    const row = q(DB, `SELECT Key, ${LOTCOLS.join(',')} FROM reward WHERE Key IN ('${keys[0]}','${keys[1]}');`);
    if (!row) continue;
    hitAny = true;
    const [key, ...lots] = row.split('|');
    const emptyIdx = lots.findIndex(v => v === '');
    if (emptyIdx === -1) { full.push(`${qid}_${slot}`); continue; }
    q(DB, `UPDATE reward SET ${LOTCOLS[emptyIdx]}='${lot}' WHERE Key='${key}';`);
    patched++;
  }
  if (!hitAny) missing.push(qid);
}
console.log(`reward rows patched: ${patched}`);
if (full.length) console.log(`rows with no free slot (skipped): ${full.join(', ')}`);
if (missing.length) console.log(`quests with no _100/_101 reward row: ${missing.length}: ${missing.map(m => m + (names.has(m) ? ` (${names.get(m)})` : '')).join(', ')}`);
