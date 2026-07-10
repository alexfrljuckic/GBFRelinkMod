// Measure free RewardLotId slots on the clear-reward rows (_100 first, _101 every) of
// every quest, split by tier — to gauge feasibility of adding badge/spellbook/voucher lots.
import { execSync } from 'node:child_process';

const SQL = 'tools\\sqlite\\sqlite3.exe';
const q = (db, sql) => execSync(`${SQL} ${db} "${sql.replace(/\s+/g, ' ').replace(/"/g, '""')}"`).toString().trim();

function xxh32(str) {
  const data = Buffer.from(str, 'ascii');
  const P1 = 0x9e3779b1, P2 = 0x85ebca77, P3 = 0xc2b2ae3d, P4 = 0x27d4eb2f, P5 = 0x165667b1;
  const rotl = (x, r) => ((x << r) | (x >>> (32 - r))) >>> 0, mul = (a, b) => Math.imul(a, b) >>> 0;
  const round = (a, i) => mul(rotl((a + mul(i, P2)) >>> 0, 13), P1);
  const len = data.length; let h = 0x178A54A4, i = 0;
  if (len >= 16) { let v1 = 0x2557311B, v2 = 0x871FB76A, v3 = 0x0133ECF3, v4 = 0x62FC7342;
    do { v1 = round(v1, data.readUInt32LE(i)); v2 = round(v2, data.readUInt32LE(i + 4)); v3 = round(v3, data.readUInt32LE(i + 8)); v4 = round(v4, data.readUInt32LE(i + 12)); i += 16; } while (len - i > 16);
    h = (rotl(v1, 1) + rotl(v2, 7) + rotl(v3, 12) + rotl(v4, 18)) >>> 0; }
  h = (h + len) >>> 0;
  for (; len - i >= 4; i += 4) h = mul(rotl((h + mul(data.readUInt32LE(i), P3)) >>> 0, 17), P4);
  for (; i < len; i++) h = mul(rotl((h + mul(data[i], P5)) >>> 0, 11), P1);
  h = mul(h ^ (h >>> 15), P2); h = mul(h ^ (h >>> 13), P3); return (h ^ (h >>> 16)) >>> 0;
}
const hex = n => n.toString(16).toUpperCase().padStart(8, '0');

const QB = 'extracted/qb_work/qb.sqlite';
const RW = 'extracted/tmj_work/reward.sqlite';   // vanilla reward.tbl
const CHAOS = new Set(['004083', '004093', '0040A3', '0040B3']);

// quest id (hex) -> tier
const quests = q(QB, `SELECT Key FROM quest_baseinfo_ex_data;`).split(/\r?\n/).map(k => ({
  qid: k.slice(2), tier: CHAOS.has(k.slice(0, 6)) ? 'CHAOS+' : 'below',
}));

// pull all reward rows once, index by Key hash
const rows = q(RW, `SELECT Key, RewardLotId1,RewardLotId2,RewardLotId3,RewardLotId4,RewardLotId5,RewardLotId6 FROM reward;`)
  .split(/\r?\n/).map(l => l.split('|'));
const byKey = new Map();
for (const r of rows) byKey.set(r[0], r.slice(1)); // key -> [6 lots]  (key may be readable OR hash)

function freeSlots(qid, slot) {
  const name = `RW_${qid}_${slot}`;
  const lots = byKey.get(name) || byKey.get(hex(xxh32(name)));
  if (!lots) return null;
  const used = lots.filter(x => x && x !== '').length;
  return 6 - used;
}

const stats = {};
for (const { qid, tier } of quests) {
  for (const slot of ['100', '101']) {
    const free = freeSlots(qid, slot);
    const bucket = `${tier} _${slot}`;
    stats[bucket] ??= { present: 0, missing: 0, free: {} };
    if (free === null) { stats[bucket].missing++; continue; }
    stats[bucket].present++;
    stats[bucket].free[free] = (stats[bucket].free[free] || 0) + 1;
  }
}
for (const [b, s] of Object.entries(stats)) {
  const dist = Object.entries(s.free).sort((a, c) => +a[0] - +c[0]).map(([f, n]) => `${f}free:${n}`).join(' ');
  console.log(`${b.padEnd(14)} present=${s.present} missing=${s.missing} | ${dist}`);
}
