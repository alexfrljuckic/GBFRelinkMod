// Diagnostic: what Awakening+ (GEEN_xxx_90) sigils does the save-reader think you own,
// and is the GEEN_175_90 (Ultramarine's Awakening+) record real or a mis-parse?
// Mirrors SaveReader.cs's Locate + inventory read. Read-only.
import { readFileSync, readdirSync, statSync } from 'node:fs';

function xxh32(str) {
  const d = Buffer.from(str, 'ascii');
  const P1 = 0x9e3779b1, P2 = 0x85ebca77, P3 = 0xc2b2ae3d, P4 = 0x27d4eb2f, P5 = 0x165667b1;
  const rotl = (x, r) => ((x << r) | (x >>> (32 - r))) >>> 0;
  const mul = (a, b) => Math.imul(a, b) >>> 0;
  const round = (a, v) => mul(rotl((a + mul(v, P2)) >>> 0, 13), P1);
  const len = d.length; let i = 0, h = 0x178A54A4;
  if (len >= 16) {
    let v1 = 0x2557311B, v2 = 0x871FB76A, v3 = 0x0133ECF3, v4 = 0x62FC7342;
    do { v1 = round(v1, d.readUInt32LE(i)); v2 = round(v2, d.readUInt32LE(i + 4)); v3 = round(v3, d.readUInt32LE(i + 8)); v4 = round(v4, d.readUInt32LE(i + 12)); i += 16; } while (len - i > 16);
    h = (rotl(v1, 1) + rotl(v2, 7) + rotl(v3, 12) + rotl(v4, 18)) >>> 0;
  }
  h = (h + len) >>> 0;
  for (; len - i >= 4; i += 4) h = mul(rotl((h + mul(d.readUInt32LE(i), P3)) >>> 0, 17), P4);
  for (; i < len; i++) h = mul(rotl((h + mul(d[i], P5)) >>> 0, 11), P1);
  h = mul(h ^ (h >>> 15), P2); h = mul(h ^ (h >>> 13), P3); return (h ^ (h >>> 16)) >>> 0;
}

const EMPTY = 0x887AE0B0;
// reverse maps
const geenRev = new Map(), skillRev = new Map();
for (let a = 0; a <= 999; a++) {
  for (let b = 0; b <= 99; b++) geenRev.set(xxh32(`GEEN_${String(a).padStart(3,'0')}_${String(b).padStart(2,'0')}`), `GEEN_${a}_${b}`);
  for (let b = 0; b <= 10; b++) skillRev.set(xxh32(`SKILL_${String(a).padStart(3,'0')}_${String(b).padStart(2,'0')}`), a);
}
// GEEN name map
const geenNames = new Map();
for (const line of readFileSync('extracted/geen-names-en.tsv', 'utf8').split('\n')) {
  const m = line.match(/^TXT_(GEEN_\d+_\d+)\t(.*)$/);
  if (m) geenNames.set(m[1].replace(/_0*(\d)/g, '_$1').replace('GEEN_', 'GEEN_'), m[2]); // normalize later
}
function nameOf(geenStr) { // geenStr like GEEN_175_90 -> pad to match tsv key TXT_GEEN_175_90
  const m = geenStr.match(/GEEN_(\d+)_(\d+)/); if (!m) return '?';
  const key = `GEEN_${String(m[1]).padStart(3,'0')}_${String(m[2]).padStart(2,'0')}`;
  for (const line of geenLines) { const mm = line.match(/^TXT_(GEEN_\d+_\d+)\t(.*)$/); if (mm && mm[1] === key) return mm[2]; }
  return '(unnamed)';
}
const geenLines = readFileSync('extracted/geen-names-en.tsv', 'utf8').split('\n');

const rU32 = (b, o) => b.readUInt32LE(o);

function locate(buf, rev) {
  for (let off = 0; off + 24 <= buf.length; off += 4) {
    if (rU32(buf, off + 8) !== 4 || rU32(buf, off + 12) !== 1) continue;
    if (!rev.has(rU32(buf, off + 16))) continue;
    const type = rU32(buf, off);
    if (type === 0 || type > 0xffff) continue;
    let s = off, e = off;
    while (s - 24 >= 0 && rU32(buf, s - 24) === type) s -= 24;
    while (e + 48 <= buf.length && rU32(buf, e + 24) === type) e += 24;
    const count = (e - s) / 24 + 1;
    if (count >= 1000) {
      const seen = new Set(); let dup = 0;
      for (let i = 0; i < count; i++) { const h = rU32(buf, s + 24 * i + 16); if (h === EMPTY) continue; if (seen.has(h)) dup++; else seen.add(h); }
      if (dup > 10) return { start: s, count };
    }
    off = e;
  }
  throw new Error('sigil inventory not found');
}

let newest;
if (process.argv[2]) {
  newest = process.argv[2]; // explicit save path (e.g. a shared-read copy of a locked save)
} else {
  const dir = `${process.env.LOCALAPPDATA}\\GBFR\\Saved\\SaveGames`;
  const files = readdirSync(dir).filter(f => /^SaveData\d+\.dat$/.test(f)).map(f => `${dir}\\${f}`);
  newest = files.sort((a, b) => statSync(b).mtimeMs - statSync(a).mtimeMs)[0];
}
console.log('save:', newest, '\nmtime:', statSync(newest).mtime.toLocaleString());
const buf = readFileSync(newest);

// find ALL qualifying arrays, not just the first — instability here would be the bug
function locateAll(buf, rev) {
  const runs = [];
  for (let off = 0; off + 24 <= buf.length; off += 4) {
    if (rU32(buf, off + 8) !== 4 || rU32(buf, off + 12) !== 1) continue;
    if (!rev.has(rU32(buf, off + 16))) continue;
    const type = rU32(buf, off);
    if (type === 0 || type > 0xffff) continue;
    let s = off, e = off;
    while (s - 24 >= 0 && rU32(buf, s - 24) === type) s -= 24;
    while (e + 48 <= buf.length && rU32(buf, e + 24) === type) e += 24;
    const count = (e - s) / 24 + 1;
    if (count >= 1000) {
      const seen = new Set(); let dup = 0;
      for (let i = 0; i < count; i++) { const h = rU32(buf, s + 24 * i + 16); if (h === EMPTY) continue; if (seen.has(h)) dup++; else seen.add(h); }
      if (dup > 10) runs.push({ start: s, count, type, dup });
      off = e;
    }
  }
  // dedup by start
  return runs.filter((r, i) => runs.findIndex(x => x.start === r.start) === i);
}
const allRuns = locateAll(buf, geenRev);
console.log(`qualifying sigil-inventory arrays found: ${allRuns.length}`);
for (const r of allRuns) console.log(`  start=0x${r.start.toString(16)} count=${r.count} type=${r.type} dupHashes=${r.dup}`);

const sig = locate(buf, geenRev);
console.log(`\nLocate() picked: start=0x${sig.start.toString(16)} count=${sig.count}`);

// count distribution over valid-GEEN records
const perSigil = new Map();
let filled = 0;
for (let j = 0; j < sig.count; j++) {
  const h = rU32(buf, sig.start + 24 * j + 16);
  if (!geenRev.has(h)) continue;
  filled++;
  perSigil.set(h, (perSigil.get(h) || 0) + 1);
}
console.log(`filled (valid-GEEN) records = ${filled}; distinct sigils = ${perSigil.size}`);
const top = [...perSigil.entries()].sort((a, b) => b[1] - a[1]).slice(0, 8);
console.log('top sigils by copy count:');
for (const [h, c] of top) console.log(`  x${c}  ${nameOf(geenRev.get(h))}`);
const singles = [...perSigil.values()].filter(c => c === 1).length;
console.log(`sigils owned exactly x1: ${singles} / ${perSigil.size}`);

// tally all GEEN_xxx_90 (Awakening) owned
const awakeningCounts = new Map();
const target = xxh32('GEEN_175_90');
const targetRecords = [];
for (let j = 0; j < sig.count; j++) {
  const h = rU32(buf, sig.start + 24 * j + 16);
  const g = geenRev.get(h); if (!g) continue;
  const mm = g.match(/GEEN_(\d+)_(\d+)/);
  if (mm && mm[2] === '90') awakeningCounts.set(g, (awakeningCounts.get(g) || 0) + 1);
  if (h === target) targetRecords.push({ j, off: sig.start + 24 * j, seq: rU32(buf, sig.start + 24 * j + 4), type: rU32(buf, sig.start + 24 * j) });
}
console.log(`\n=== all Awakening+ (GEEN_*_90) the reader counts as OWNED ===`);
for (const [g, c] of [...awakeningCounts].sort((a, b) => a[0].localeCompare(b[0])))
  console.log(`  ${g.padEnd(14)} x${c}  ${nameOf(g)}`);
// Is the Ultramarine record backed by a real innate trait? Awakening sigils carry an
// innate SKILL_<geenNum>. Locate the trait pool and count slot0 (innate) SKILL_175 entries.
const skl = locate(buf, skillRev);
let innate175 = 0;
const otherInnate = {};
for (const num of [114, 128, 175, 176]) otherInnate[num] = 0;
for (let i = 0; i < skl.count; i++) {
  const seq = rU32(buf, skl.start + 24 * i + 4);
  if (seq % 100 !== 0) continue; // slot0 = innate
  const sn = skillRev.get(rU32(buf, skl.start + 24 * i + 16));
  if (sn === 175) innate175++;
  if (sn in otherInnate) otherInnate[sn]++;
}
console.log(`\n=== innate-trait backing (trait pool @0x${skl.start.toString(16)}, count=${skl.count}) ===`);
console.log(`SKILL_175 innate (Ultramarine) slot0 entries = ${innate175}`);
console.log(`comparison innate counts: ${JSON.stringify(otherInnate)}  (128=Eternal Rage, absent from owned list)`);

console.log(`\n=== GEEN_175_90 (Ultramarine's Awakening+) records: ${targetRecords.length} ===`);
for (const r of targetRecords) {
  const bytes = buf.subarray(r.off, r.off + 24).toString('hex').replace(/(..)/g, '$1 ').trim();
  console.log(`  record #${r.j}  off=0x${r.off.toString(16)}  type=${r.type}  seq=${r.seq}  bytes: ${bytes}`);
}
