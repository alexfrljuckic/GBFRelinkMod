// Build the UNIFIED skill catalog for the game-organized two-toggle Transmarvel config.
//
// Organization mirrors the in-game skill list: categories come from skill.tbl's
// GemCategory column (u32 @ row offset 92 of the 112-byte 2.0 rows: 0 Basic Stats,
// 1 Attack, 2 Defense, 3 Support, 4 Special) and ordering from its SortOrder
// (u32 @ offset 100). Each skill gets up to two toggles:
//   primary   — its V+ sigil can roll as a Transmarvel MAIN (the GEEN_<fam>_24
//               dual-slot form: maxed own trait + random 2nd trait). If vanilla's
//               Transmarvel buckets don't contain it, addToPool=true and Mod.cs
//               appends it to the Chase V+ bucket at launch.
//   secondary — the skill may roll as the random 2nd trait (trait filter).
// Character Warpath+/Awakening+ sigils stay as primary-only entries (two per
// character) under Special — which is where the game itself categorizes them.
// Stat V+ singles (GEEN_xxx_14, Awakening bucket) keep their released semantics;
// their families do NOT also get the _24 dual form (vanilla keeps those stats
// single, and the mod only ever grants vanilla-attested sigil forms).
//
// Output: mods/transmarvel-overhaul/skill-catalog.json (reviewable; drives
// scripts/gen-transmarvel-code.mjs which regenerates the C# sources).
import { readFileSync, writeFileSync } from 'node:fs';

const SIGILS = JSON.parse(readFileSync('mods/transmarvel-overhaul/sigil-catalog.json', 'utf8'));
const TRAITS = JSON.parse(readFileSync('mods/transmarvel-overhaul/trait-catalog.json', 'utf8'));

// --- GBFR custom XXHash32 (docs/15) ---
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
const hex = n => '0x' + n.toString(16).toUpperCase().padStart(8, '0');

// --- skill.tbl: family -> {category, sortOrder} (2.0 layout, 112-byte rows) ---
const CATEGORY_NAMES = { 0: 'Basic Stats', 1: 'Attack', 2: 'Defense', 3: 'Support', 4: 'Special' };
const tbl = readFileSync('extracted/2.0/system/table/skill.tbl');
const ROW = 112, ROWS = Number(tbl.readBigInt64LE(0));
if (8 + ROWS * ROW !== tbl.length) throw new Error('skill.tbl layout changed — re-derive offsets');
const famInfo = new Map(); // family -> {category, sortOrder}
for (let r = 0; r < ROWS; r++) {
  const o = 8 + r * ROW;
  const fam = String(+tbl.slice(o + 32, o + 48).toString('latin1').replace(/\0.*$/, ''));
  const category = tbl.readUInt32LE(o + 92);
  const sortOrder = tbl.readUInt32LE(o + 100);
  const cur = famInfo.get(fam);
  if (!cur || sortOrder < cur.sortOrder) famInfo.set(fam, { category, sortOrder });
}

// --- names ---
const skillNames = new Map();
for (const line of readFileSync('extracted/skill-names-en.tsv', 'utf8').split(/\r?\n/)) {
  const m = line.match(/^TXT_SKILL_(\d+)_00\t(.*)$/);
  if (m) skillNames.set(String(+m[1]), m[2]);
}
const vplusNames = new Map(); // family -> GEEN _24 display name
for (const line of readFileSync('extracted/geen-names-en.tsv', 'utf8').split(/\r?\n/)) {
  const m = line.match(/^TXT_GEEN_(\d+)_24\t(.*)$/);
  if (m) vplusNames.set(String(+m[1]), m[2]);
}

// character name per family, parsed from skill summaries (hand-fallback for odd phrasings)
const summaries = new Map();
for (const line of readFileSync('extracted/skill-summaries-en.tsv', 'utf8').split(/\r?\n/)) {
  const m = line.match(/^TXT_SKILL_SUMMARY_(\d+)_00\t(.*)$/);
  if (m) summaries.set(String(+m[1]), m[2]);
}
const CHAR_FALLBACK = { // families whose summary doesn't cleanly name the character
  114: 'Gran/Djeeta', 116: 'Lancelot', 117: 'Io', 118: 'Eugen', 119: 'Rosetta',
  120: 'Ferry', 121: 'Lancelot', 123: 'Percival', 125: 'Charlotta', 126: 'Zeta',
  129: 'Rackam', 130: 'Cagliostro', 131: 'Yodarha', 132: 'Narmaya', 170: 'Id',
  171: 'Vaseraga', 172: 'Seofon', 173: 'Tweyen', 174: 'Nier', 175: 'Sandalphon',
  176: 'Eustace', 177: 'Beatrix', 178: 'Ferry',
};
function charOf(family) {
  const txt = summaries.get(family) || '';
  const m = txt.match(/\bto ([A-Z][a-z]+)\b/) || txt.match(/^([A-Z][a-z]+)'s\b/);
  const bad = new Set(['The', 'This', 'When', 'Grants', 'Increases', 'Reduces', 'Deals', 'Also', 'Stance', 'Effect', 'Gauge']);
  if (m && !bad.has(m[1])) return m[1];
  return CHAR_FALLBACK[+family] || null;
}

const fam = id => String(+(id.match(/_(\d+)_/) || [])[1]);
const pascal = name => name.replace(/\+$/, '').split(/[^A-Za-z0-9]+/).filter(Boolean)
  .map(w => w[0].toUpperCase() + w.slice(1)).join('');

const sigByFam = new Map(); // family -> existing pool sigils
for (const s of SIGILS) {
  const f = fam(s.id);
  if (!sigByFam.has(f)) sigByFam.set(f, []);
  sigByFam.get(f).push(s);
}
const traitByFam = new Map(TRAITS.map(t => [fam(t.id), t]));

const CHAR_BUCKETS = new Set(['Warpath+', 'Awakening+']);
const CHASE_BUCKET_KEY = '0x6E52A69A'; // Chase V+ — where added V+ mains go

const entries = [];
const families = new Set([...traitByFam.keys(), ...vplusNames.keys(), ...sigByFam.keys()]);
const skippedNoTbl = [], secondaryOnly = [], newPrimaries = [];
for (const f of [...families].sort((a, b) => +a - +b)) {
  const info = famInfo.get(f);
  if (!info) { skippedNoTbl.push(f); continue; }
  const trait = traitByFam.get(f);
  // the stat V+ singles (GEEN_xxx_14) sit in the Awakening+ bucket but are NOT
  // character sigils — they're the primary toggle of their Basic Stats skill
  const chars = (sigByFam.get(f) || []).filter(s => CHAR_BUCKETS.has(s.bucket) && !/_1\d$/.test(s.id));
  const base = {
    family: f,
    name: skillNames.get(f) || trait?.name || chars[0]?.name || vplusNames.get(f) || `SKILL_${f}`,
    category: info.category, categoryName: CATEGORY_NAMES[info.category] ?? `cat${info.category}`,
    sortOrder: info.sortOrder,
  };

  // character sigils: primary-only entries, one per variant (game puts them in Special)
  if (chars.length) {
    const character = charOf(f);
    for (const s of chars.sort((a, b) => (a.bucket === 'Warpath+' ? 0 : 1) - (b.bucket === 'Warpath+' ? 0 : 1))) {
      entries.push({ ...base, character, variant: s.bucket,
        primary: { prop: s.prop, displayName: s.name, hash: '0x' + s.hash.replace(/^0x/i, ''),
          bucketKey: '0x' + s.bucketKey.replace(/^0x/i, ''), bucket: s.bucket, addToPool: false, default: s.default },
        secondary: null });
    }
    continue;
  }

  // non-character: one entry, up to two toggles
  let primary = null;
  const existing = (sigByFam.get(f) || [])[0]; // stat V+ singles + chase V+ already in the pool
  if (existing) {
    primary = { prop: existing.prop, displayName: existing.name, hash: '0x' + existing.hash.replace(/^0x/i, ''),
      bucketKey: '0x' + existing.bucketKey.replace(/^0x/i, ''), bucket: existing.bucket, addToPool: false, default: existing.default };
  } else if (vplusNames.has(f)) {
    const geen = `GEEN_${f.padStart(3, '0')}_24`;
    const dn = vplusNames.get(f);
    primary = { prop: 'Pool' + pascal(dn), displayName: dn, hash: hex(xxh32(geen)),
      bucketKey: CHASE_BUCKET_KEY, bucket: 'Chase V+', addToPool: true, default: false };
    newPrimaries.push(dn);
  }
  const secondary = trait ? { prop: trait.prop, displayName: trait.name, hash: '0x' + trait.hash.replace(/^0x/i, ''),
    default: trait.default, legal: trait.legalOnTransmarvel } : null;
  if (!primary && secondary) secondaryOnly.push(base.name);
  if (!primary && !secondary) continue;
  entries.push({ ...base, primary, secondary });
}

entries.sort((a, b) => a.category - b.category || a.sortOrder - b.sortOrder ||
  a.name.localeCompare(b.name) || (a.variant === 'Warpath+' ? 0 : 1) - (b.variant === 'Warpath+' ? 0 : 1));

// collision guard: every config property name must be unique
const props = entries.flatMap(e => [e.primary?.prop, e.secondary?.prop]).filter(Boolean);
const dup = props.filter((p, i) => props.indexOf(p) !== i);
if (dup.length) throw new Error('duplicate config properties: ' + dup.join(', '));

writeFileSync('mods/transmarvel-overhaul/skill-catalog.json', JSON.stringify(entries, null, 1));
const byCat = {};
for (const e of entries) byCat[e.categoryName] = (byCat[e.categoryName] || 0) + 1;
console.log('unified skill catalog:', JSON.stringify({
  entries: entries.length,
  bothToggles: entries.filter(e => e.primary && e.secondary).length,
  primaryOnly: entries.filter(e => e.primary && !e.secondary).length,
  secondaryOnly: entries.filter(e => !e.primary && e.secondary).length,
  addToPool: entries.filter(e => e.primary?.addToPool).length,
  characterSigils: entries.filter(e => e.variant).length,
  byCategory: byCat,
}, null, 1));
if (skippedNoTbl.length) console.log('SKIPPED (no skill.tbl row):', skippedNoTbl.join(', '));
console.log('secondary-only (no V+ sigil form):', secondaryOnly.join(', ') || '(none)');
