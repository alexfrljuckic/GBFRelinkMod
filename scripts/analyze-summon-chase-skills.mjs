// Analysis: for every 5-star summon, dump its slot-1 skill pool RAW weights, flag the
// "chase" (unique minimum-weight) skill, and verify the boost heuristic is safe.
// Reuses the same table model as gen-summon-drop-tables.mjs (docs/23). Read-only.
import { readFileSync } from 'node:fs';

function xxh32(str) {
  const d = Buffer.from(str, 'ascii');
  const P1 = 0x9e3779b1, P2 = 0x85ebca77, P3 = 0xc2b2ae3d, P4 = 0x27d4eb2f, P5 = 0x165667b1;
  const rotl = (x, r) => ((x << r) | (x >>> (32 - r))) >>> 0, mul = (a, b) => Math.imul(a, b) >>> 0;
  const round = (a, i) => mul(rotl((a + mul(i, P2)) >>> 0, 13), P1);
  const len = d.length; let h = 0x178A54A4, i = 0;
  if (len >= 16) { let v1 = 0x2557311B, v2 = 0x871FB76A, v3 = 0x0133ECF3, v4 = 0x62FC7342;
    do { v1 = round(v1, d.readUInt32LE(i)); v2 = round(v2, d.readUInt32LE(i + 4)); v3 = round(v3, d.readUInt32LE(i + 8)); v4 = round(v4, d.readUInt32LE(i + 12)); i += 16; } while (len - i > 16);
    h = (rotl(v1, 1) + rotl(v2, 7) + rotl(v3, 12) + rotl(v4, 18)) >>> 0; }
  h = (h + len) >>> 0;
  for (; len - i >= 4; i += 4) h = mul(rotl((h + mul(d.readUInt32LE(i), P3)) >>> 0, 17), P4);
  for (; i < len; i++) h = mul(rotl((h + mul(d[i], P5)) >>> 0, 11), P1);
  h = mul(h ^ (h >>> 15), P2); h = mul(h ^ (h >>> 13), P3); return (h ^ (h >>> 16)) >>> 0;
}
const T = f => readFileSync('extracted/2.0/system/table/' + f);
const EMPTY = 0x887AE0B0;

const skillNames = new Map();
for (const line of readFileSync('extracted/skill-names-en.tsv', 'utf8').split(/\r?\n/)) {
  const m = line.match(/^TXT_SKILL_(\d+)_00\t(.*)$/); if (m) skillNames.set(String(+m[1]), m[2]);
}
const skillByHash = new Map();
for (let f = 0; f < 400; f++) for (let s = 0; s < 5; s++) {
  const k = xxh32(`SKILL_${String(f).padStart(3, '0')}_${String(s).padStart(2, '0')}`);
  if (!skillByHash.has(k)) skillByHash.set(k, skillNames.get(String(f)) || `SKILL_${f}_${s}`);
}
const soName = new Map();
for (const line of readFileSync('extracted/summon-names-en.tsv', 'utf8').split(/\r?\n/)) {
  const m = line.match(/^TXT_SMN_(So[0-9a-f]{4})\t(.*)$/); if (m && !soName.has(m[1])) soName.set(m[1], m[2]);
}
// summon identity
const info = T('summon_info.tbl'), ni = Number(info.readBigInt64LE(0));
const soByTxt = new Map([...soName.keys()].map(so => [xxh32('TXT_SMN_' + so), so]));
const speciesInfo = new Map();
for (let r = 0; r < ni; r++) { const o = 8 + r * 104; const so = soByTxt.get(info.readUInt32LE(o + 0x30)); speciesInfo.set(info.readUInt32LE(o + 0x48), so ? soName.get(so) : '?'); }
const param = T('summon_param.tbl'), np = Number(param.readBigInt64LE(0));
const speciesByParam = new Map();
for (let r = 0; r < np; r++) { const o = 8 + r * 92; speciesByParam.set(param.readUInt32LE(o + 0x34), param.readUInt32LE(o + 0x40)); }
const smn = T('summon.tbl'), ns = Number(smn.readBigInt64LE(0));
const summons = [];
for (let r = 0; r < ns; r++) {
  const o = 8 + r * 36, c = i => smn.readUInt32LE(o + i * 4);
  summons.push({ name: speciesInfo.get(speciesByParam.get(c(5))) || '?', rarity: c(6), lot1: [c(0), c(1)].find(v => v !== EMPTY) ?? null });
}
// skill pools
const lot = T('summon_lot.tbl'), nl = Number(lot.readBigInt64LE(0));
const lots = new Map();
for (let r = 0; r < nl; r++) { const o = 8 + r * 20, g = lot.readUInt32LE(o); if (!lots.has(g)) lots.set(g, []); lots.get(g).push({ sk: lot.readUInt32LE(o + 4), w: lot.readInt32LE(o + 12) }); }

// analyze every 5-star summon's slot-1 pool
let withChase = 0, flat = 0, weird = 0;
const distinctGroups = new Set();
console.log('5★ summon slot-1 pools (raw weights; CHASE = unique min):\n');
for (const s of summons.filter(s => s.rarity === 5 && s.lot1)) {
  const list = lots.get(s.lot1); if (!list) continue;
  if (distinctGroups.has(s.lot1)) continue; // dedupe shared groups
  distinctGroups.add(s.lot1);
  const tot = list.reduce((a, x) => a + x.w, 0);
  const min = Math.min(...list.map(x => x.w)), max = Math.max(...list.map(x => x.w));
  const minCount = list.filter(x => x.w === min).length;
  const tag = list.length === 1 ? 'SINGLE' : min === max ? 'FLAT' : minCount === 1 ? 'CHASE' : `MULTI-MIN(${minCount})`;
  if (tag === 'CHASE') withChase++; else if (tag === 'FLAT' || tag === 'SINGLE') flat++; else weird++;
  const parts = list.map(x => { const nm = skillByHash.get(x.sk) || '?'; const pct = (100 * x.w / tot).toFixed(1); const mark = (x.w === min && minCount === 1 && min !== max) ? ' <<CHASE' : ''; return `${nm} w=${x.w} (${pct}%)${mark}`; });
  console.log(`[${tag}] ${s.name} (grp ${s.lot1.toString(16)}):`);
  for (const p of parts) console.log('    ' + p);
}
console.log(`\nsummary: CHASE=${withChase}  FLAT/SINGLE=${flat}  WEIRD(multi-min)=${weird}`);

// ---- SIMULATE the mod's boost (pct=40) and report new chase odds ----
const PCT = 40;
console.log(`\n=== SIMULATED boost to ${PCT}% (mirrors ApplyBoostChaseSkills) ===`);
const fiveStarGroups = new Set(summons.filter(s => s.rarity === 5 && s.lot1).map(s => s.lot1));
let boosted = 0, touchedSkills = [];
for (const g of fiveStarGroups) {
  const list = lots.get(g); if (!list || list.length < 2) continue;
  const min = Math.min(...list.map(x => x.w)), max = Math.max(...list.map(x => x.w));
  const minCount = list.filter(x => x.w === min).length;
  if (min === max || minCount !== 1) continue;
  const chase = list.find(x => x.w === min);
  const fillersTotal = list.filter(x => x !== chase).reduce((a, x) => a + x.w, 0);
  const newChase = Math.round(fillersTotal * PCT / (100 - PCT));
  const newTot = newChase + fillersTotal;
  boosted++;
  touchedSkills.push(`${skillByHash.get(chase.sk)}: ${(100*chase.w/(chase.w+fillersTotal)).toFixed(1)}% -> ${(100*newChase/newTot).toFixed(1)}%`);
}
console.log(`groups boosted: ${boosted}`);
for (const t of touchedSkills.sort()) console.log('  ' + t);
