// ⚠️ SUPERSEDED 2026-07-10 (same day it was written): the trait filter now
// lives in the mod's C# component (mods-src/gbfr.transmarvel.overhaul,
// Mod.ApplyTraitFilter) with the allowed set configurable per-trait in
// Reloaded-II Configure Mod. Kept for reference — same algorithm.
//
// Build the transmarvel-overhaul 2nd-trait filter tables from vanilla:
//   skill_lot.tbl      — every non-curated SkillId in every vanilla trait
//                        sub-lot is remapped onto the curated list (per-sub-lot
//                        round-robin; keys/weights/row-count untouched), plus
//                        the appended SKL_TMV_GOOD sub-lot (one row per curated
//                        trait). Content-level because pointer-level rerouting
//                        alone proved insufficient in game (docs/13).
//   skill_type_lot.tbl — type-lots 2/3/4/5/14 -> SKL_TMV_GOOD 100% (kept for
//                        equal odds on whatever path does honor them).
// Writes into the repo mod folder and the installed Reloaded-II copy.
// Edit CURATED below to change the trait list, then: node scripts/build-trait-filter.mjs
// RESTART THE GAME afterwards (tables are read once, at launch).
import { readFileSync, writeFileSync, existsSync } from 'node:fs';

// The curated traits (trimmed to 12 per Alex 2026-07-10 — dropped Crit Hit DMG,
// Weak Point DMG, Overdrive/Break Assassin, Skilled Assault, Injury to Insult):
const CURATED = {
  SKILL_020_00: 'DMG Cap',        SKILL_027_00: 'Tyranny',
  SKILL_006_00: 'Stamina',        SKILL_111_00: 'Quick Charge',
  SKILL_070_00: 'Cascade',        SKILL_069_00: 'Quick Cooldown',
  SKILL_066_00: 'Regen',          SKILL_072_00: 'Uplift',
  SKILL_045_00: 'Guts',           SKILL_068_00: 'Autorevive',
  SKILL_073_00: 'Potion Hoarder', SKILL_094_00: 'Steady Focus',
};

const VANILLA = 'extracted/2.0/system/table';
const MOD_TABLES = 'mods/transmarvel-overhaul/gbfr.transmarvel.overhaul/GBFR/data/system/table';
const RELOADED = 'C:/Reloaded-II/Mods/gbfr.transmarvel.overhaul/GBFR/data/system/table';
const EMPTY = 0x887ae0b0;

// --- GBFR custom XXHash32 (docs/15) ---
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
      v1 = round(v1, data.readUInt32LE(i)); v2 = round(v2, data.readUInt32LE(i + 4));
      v3 = round(v3, data.readUInt32LE(i + 8)); v4 = round(v4, data.readUInt32LE(i + 12));
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

// name sanity: catch id/name drift before it ships
if (existsSync('extracted/skill-names-en.tsv')) {
  const names = new Map();
  for (const line of readFileSync('extracted/skill-names-en.tsv', 'utf8').split('\n')) {
    const [k, v] = line.trim().split('\t');
    if (k && !names.has(k.replace('TXT_', ''))) names.set(k.replace('TXT_', ''), v);
  }
  for (const [id, expect] of Object.entries(CURATED)) {
    const actual = names.get(id);
    if (actual !== expect) {
      console.error(`CURATED name mismatch: ${id} is "${actual}" in text.msg, script says "${expect}" — fix the list.`);
      process.exit(1);
    }
  }
}

const curatedHashes = Object.keys(CURATED).map(xxh32);
const curatedSet = new Set(curatedHashes);
const GOOD_LOT = xxh32('SKL_TMV_GOOD'); // 485F6215

// --- skill_lot: remap contents + append SKL_TMV_GOOD (rows: Key,SkillId,Weight — 12B) ---
const vlot = readFileSync(`${VANILLA}/skill_lot.tbl`);
const vRows = Number(vlot.readBigInt64LE(0));
if (8 + vRows * 12 !== vlot.length) { console.error('skill_lot layout changed — refusing.'); process.exit(1); }
const out = Buffer.alloc(vlot.length + curatedHashes.length * 12);
vlot.copy(out, 0);
out.writeBigInt64LE(BigInt(vRows + curatedHashes.length), 0);
const perKey = new Map();
let replaced = 0;
for (let r = 0; r < vRows; r++) {
  const off = 8 + r * 12;
  const sid = out.readUInt32LE(off + 4);
  if (sid === EMPTY || sid === 0 || curatedSet.has(sid)) continue;
  const key = out.readUInt32LE(off);
  const i = perKey.get(key) || 0;
  perKey.set(key, i + 1);
  out.writeUInt32LE(curatedHashes[i % curatedHashes.length], off + 4);
  replaced++;
}
curatedHashes.forEach((h, i) => {
  const off = 8 + (vRows + i) * 12;
  out.writeUInt32LE(GOOD_LOT, off);
  out.writeUInt32LE(h, off + 4);
  out.writeUInt32LE(1, off + 8);
});

// --- skill_type_lot: rows [SkillLotId1..6][Chance1..6][Key], 52B; keys 2/3/4/5/14 -> GOOD 100% ---
const vtype = readFileSync(`${VANILLA}/skill_type_lot.tbl`);
const tRows = Number(vtype.readBigInt64LE(0));
if (8 + tRows * 52 !== vtype.length) { console.error('skill_type_lot layout changed — refusing.'); process.exit(1); }
const tout = Buffer.from(vtype);
const REROUTE = new Set([2, 3, 4, 5, 14]);
let rerouted = 0;
for (let r = 0; r < tRows; r++) {
  const off = 8 + r * 52;
  if (!REROUTE.has(tout.readInt32LE(off + 0x30))) continue;
  tout.writeUInt32LE(GOOD_LOT, off);
  for (let k = 1; k < 6; k++) tout.writeUInt32LE(EMPTY, off + 4 * k);
  tout.writeInt32LE(100, off + 0x18);
  for (let k = 1; k < 6; k++) tout.writeInt32LE(0, off + 0x18 + 4 * k);
  rerouted++;
}
if (rerouted !== 5) { console.error(`expected to reroute 5 type-lots, hit ${rerouted} — refusing.`); process.exit(1); }

const dests = [MOD_TABLES];
if (existsSync(RELOADED)) dests.push(RELOADED);
for (const d of dests) {
  writeFileSync(`${d}/skill_lot.tbl`, out);
  writeFileSync(`${d}/skill_type_lot.tbl`, tout);
}
console.log(`skill_lot: ${vRows} vanilla rows, ${replaced} SkillIds remapped -> curated ${curatedHashes.length}, +${curatedHashes.length} SKL_TMV_GOOD rows`);
console.log(`skill_type_lot: type-lots 2/3/4/5/14 -> SKL_TMV_GOOD 100%`);
console.log(`written to: ${dests.join('  and  ')}`);
console.log('RESTART THE GAME to apply.');
