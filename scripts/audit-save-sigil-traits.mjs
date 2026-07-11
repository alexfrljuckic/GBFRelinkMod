// Audit sigil instances in the save: sigil + secondary trait + whether the
// secondary is in the mod's LIVE ticked-trait selection (Reloaded config over
// catalog defaults). Save format: docs/21.
//
// Modes:
//   node scripts/audit-save-sigil-traits.mjs
//     -> newest 40 instances by instance id. ⚠️ instance ids get RECYCLED
//        after selling sigils, so this ordering is only trustworthy if you
//        haven't sold since the pulls you're auditing.
//   node scripts/audit-save-sigil-traits.mjs --since <snapshot.dat>
//     -> conclusive before/after: lists only (sigil, traits) copies that are
//        NEW versus the snapshot (multiset diff — immune to id recycling).
//        Take snapshots with: node scripts/snapshot-save.mjs
import { readFileSync, readdirSync, statSync, existsSync } from 'node:fs';

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

const ROOT = 'C:/dev/GBF';
const EMPTY = 0x887ae0b0;

// LIVE ticked-trait selection
const TRAIT_CATALOG = JSON.parse(readFileSync(`${ROOT}/mods/transmarvel-overhaul/trait-catalog.json`, 'utf8'));
let userCfg = {};
try { userCfg = JSON.parse(readFileSync('C:/Reloaded-II/User/Mods/gbfr.transmarvel.overhaul/Config.json', 'utf8')); } catch { /* defaults */ }
const TICKED = new Set(TRAIT_CATALOG.filter(t => userCfg[t.prop] ?? t.default).map(t => t.id));

const skillNames = new Map(), geenNames = new Map();
for (const line of readFileSync(`${ROOT}/extracted/skill-names-en.tsv`, 'utf8').split('\n')) {
  const [k, v] = line.trim().split('\t');
  if (k && !skillNames.has(k.replace('TXT_', ''))) skillNames.set(k.replace('TXT_', ''), v);
}
for (const line of readFileSync(`${ROOT}/extracted/geen-names-en.tsv`, 'utf8').split('\n')) {
  const [k, v] = line.trim().split('\t');
  if (k) geenNames.set(k.replace('TXT_', ''), v);
}
const skillRev = new Map(), geenRev = new Map();
for (let a = 0; a <= 999; a++) {
  for (let b = 0; b <= 10; b++) {
    const id = `SKILL_${String(a).padStart(3, '0')}_${String(b).padStart(2, '0')}`;
    skillRev.set(xxh32(id), { id, n: a });
  }
  for (let b = 0; b <= 99; b++) {
    const id = `GEEN_${String(a).padStart(3, '0')}_${String(b).padStart(2, '0')}`;
    geenRev.set(xxh32(id), { id, n: a });
  }
}

// parse a save buffer -> instances [{sigId, sigName, seq, innate, sec, secId}]
function parseSave(buf) {
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
    throw new Error(`could not find the ${what} array — save layout changed?`);
  };
  const sig = locate(geenRev, 'sigil inventory');
  const skl = locate(skillRev, 'trait pool');
  const recAt = (a, i) => ({ seq: buf.readUInt32LE(a.start + 24 * i + 4), hash: buf.readUInt32LE(a.start + 24 * i + 16) });
  const pairs = new Map();
  for (let i = 0; i < skl.count; i++) {
    const r = recAt(skl, i);
    const N = Math.floor(r.seq / 100);
    if (!pairs.has(N)) pairs.set(N, {});
    pairs.get(N)[r.seq % 100] = r.hash;
  }
  const filled = [];
  for (let j = 0; j < sig.count; j++) {
    const r = recAt(sig, j);
    const g = geenRev.get(r.hash);
    if (g) filled.push({ j, seq: r.seq, g });
  }
  const votes = new Map();
  for (const { j, g } of filled)
    for (const [N, p] of pairs) {
      const s0 = skillRev.get(p[0]);
      if (s0 && s0.n === g.n) votes.set(N + j, (votes.get(N + j) || 0) + 1);
    }
  const C = [...votes.entries()].sort((a, b) => b[1] - a[1])[0][0];
  let match = 0;
  const out = [];
  for (const { j, seq, g } of filled) {
    const p = pairs.get(C - j) || {};
    const s0 = skillRev.get(p[0]);
    if (s0 && s0.n === g.n) match++;
    const sec = skillRev.get(p[1]);
    out.push({
      sigId: g.id, sigName: geenNames.get(g.id) || g.id, seq,
      innate: p[0] === EMPTY || p[0] === undefined ? '' : (s0?.id || p[0].toString(16)),
      secId: p[1] === EMPTY || p[1] === undefined ? '' : (sec?.id || p[1].toString(16)),
    });
  }
  if (match / filled.length < 0.99) throw new Error(`alignment cross-check failed (${match}/${filled.length})`);
  return out;
}

const fmt = (x) => {
  const secName = x.secId === '' ? '(none)' : (skillNames.get(x.secId) || x.secId);
  const flag = x.secId !== '' && !TICKED.has(x.secId) ? '  <<< NOT IN TICKED SET' : '';
  return `${x.sigName.padEnd(28)} 2nd: ${secName}${flag}`;
};

const SAVE_DIR = `${process.env.LOCALAPPDATA}\\GBFR\\Saved\\SaveGames`;
const newest = readdirSync(SAVE_DIR).filter(f => /^SaveData\d+\.dat$/i.test(f))
  .map(f => `${SAVE_DIR}\\${f}`).sort((a, b) => statSync(b).mtimeMs - statSync(a).mtimeMs)[0];
const current = parseSave(readFileSync(newest));
console.log(`current save: ${newest} (${current.length} sigils; ticked traits: ${TICKED.size})`);

const sinceIdx = process.argv.indexOf('--since');
if (sinceIdx !== -1) {
  const snapPath = process.argv[sinceIdx + 1];
  if (!snapPath || !existsSync(snapPath)) { console.error('usage: --since <snapshot.dat>'); process.exit(1); }
  const snap = parseSave(readFileSync(snapPath));
  const key = (x) => `${x.sigId}|${x.innate}|${x.secId}`;
  const baseline = new Map();
  for (const x of snap) baseline.set(key(x), (baseline.get(key(x)) || 0) + 1);
  const fresh = [];
  for (const x of current) {
    const k = key(x), n = baseline.get(k) || 0;
    if (n > 0) baseline.set(k, n - 1);
    else fresh.push(x);
  }
  console.log(`snapshot: ${snapPath} (${snap.length} sigils)`);
  console.log(`\n=== ${fresh.length} NEW sigil copies since snapshot ===`);
  for (const x of fresh) console.log(fmt(x));
  const bad = fresh.filter(x => x.secId !== '' && !TICKED.has(x.secId));
  console.log(bad.length
    ? `\n${bad.length} new copies rolled OUTSIDE the ticked set — the filter is not effective.`
    : fresh.length ? '\nAll new copies within the ticked set — filter verified for this batch.' : '\n(no new sigils — pull some first, and make sure the game saved)');
} else {
  console.log('\n=== 40 newest by instance id (⚠️ unreliable if you sold sigils — prefer --since) ===');
  for (const x of [...current].sort((a, b) => b.seq - a.seq).slice(0, 40)) console.log(fmt(x));
}
