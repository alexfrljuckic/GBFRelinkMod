// Generate mods/summon-drops/DROP-TABLES.md — every stage that can drop a summon,
// which summons it drops, and the skill pools (stats) on each summon.
//
// Chain (docs/23): reward.tbl Key (RW_<qid>_<sfx>) → reward_summon (chance) →
// reward_summon_lot (which summon, weighted) → summon.tbl → summon_lot (skill
// pools) → summon_curve (skill level odds).
// Summon names: summon.tbl@0x14 → summon_param@0x34 (species number @0x40) →
// summon_info@0x48 (name-text hash @0x30) → TXT_SMN_So#### (text_sum.msg).
import { readFileSync, writeFileSync } from 'node:fs';

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
const hex = n => n.toString(16).toUpperCase().padStart(8, '0');

// ---------- names ----------
const skillNames = new Map();
for (const line of readFileSync('extracted/skill-names-en.tsv', 'utf8').split(/\r?\n/)) {
  const m = line.match(/^TXT_SKILL_(\d+)_00\t(.*)$/);
  if (m) skillNames.set(String(+m[1]), m[2]);
}
const skillByHash = new Map();
for (let f = 0; f < 400; f++) for (let s = 0; s < 5; s++)
  if (!skillByHash.has(xxh32(`SKILL_${String(f).padStart(3, '0')}_${String(s).padStart(2, '0')}`)))
    skillByHash.set(xxh32(`SKILL_${String(f).padStart(3, '0')}_${String(s).padStart(2, '0')}`), skillNames.get(String(f)) || `SKILL_${f}`);
const questNames = new Map();
for (const line of readFileSync('extracted/quest-names-en.tsv', 'utf8').split(/\r?\n/)) {
  const m = line.match(/^TXT_QR_([0-9A-Fa-f]{6})\t(.*)$/);
  if (m) questNames.set(m[1].toUpperCase(), m[2]);
}
const soName = new Map(), soCall = new Map();
for (const line of readFileSync('extracted/summon-names-en.tsv', 'utf8').split(/\r?\n/)) {
  let m = line.match(/^TXT_SMN_(So[0-9a-f]{4})\t(.*)$/);
  if (m && !soName.has(m[1])) soName.set(m[1], m[2]);
  m = line.match(/^TXT_SMN_BDY_(So[0-9a-f]{4})_SINGLE\t(.*)$/);
  if (m && !soCall.has(m[1])) soCall.set(m[1], m[2].replace(/^The summon /, '').replace(/!$/, ''));
}

// ---------- summon identity: summon.tbl row -> {name, soId, rarity} ----------
const info = T('summon_info.tbl'); const ni = Number(info.readBigInt64LE(0));
const speciesInfo = new Map(); // species number -> {name, soId}
const soByTxtHash = new Map([...soName.keys()].map(so => [xxh32('TXT_SMN_' + so), so]));
for (let r = 0; r < ni; r++) {
  const o = 8 + r * 104;
  const so = soByTxtHash.get(info.readUInt32LE(o + 0x30));
  speciesInfo.set(info.readUInt32LE(o + 0x48), { name: so ? soName.get(so) : '?', soId: so });
}
const param = T('summon_param.tbl'); const np = Number(param.readBigInt64LE(0));
const speciesByParamKey = new Map();
for (let r = 0; r < np; r++) {
  const o = 8 + r * 92;
  speciesByParamKey.set(param.readUInt32LE(o + 0x34), param.readUInt32LE(o + 0x40));
}
const smn = T('summon.tbl'); const ns = Number(smn.readBigInt64LE(0));
const summons = new Map(); // summon key -> {name, soId, rarity, lot1, lot2}
for (let r = 0; r < ns; r++) {
  const o = 8 + r * 36;
  const c = i => smn.readUInt32LE(o + i * 4);
  const sp = speciesInfo.get(speciesByParamKey.get(c(5))) || { name: '?', soId: null };
  summons.set(c(4), {
    name: sp.name, soId: sp.soId, rarity: c(6),
    lot1: [c(0), c(1)].find(v => v !== EMPTY) ?? null,
    lot2: [c(2), c(3)].find(v => v !== EMPTY) ?? null,
  });
}

// ---------- skill pools ----------
const curve = T('summon_curve.tbl'); const ncu = Number(curve.readBigInt64LE(0));
const curves = new Map(); // group -> [{level, w}]
for (let r = 0; r < ncu; r++) {
  const o = 8 + r * 12, g = curve.readUInt32LE(o);
  if (!curves.has(g)) curves.set(g, []);
  curves.get(g).push({ level: curve.readInt32LE(o + 4), w: curve.readInt32LE(o + 8) });
}
const lvlRange = g => {
  const ls = (curves.get(g) || []).map(x => x.level);
  if (!ls.length) return '?';
  const lo = Math.min(...ls), hi = Math.max(...ls);
  return lo === hi ? `Lv${hi}` : `Lv${lo}–${hi}`;
};
const lot = T('summon_lot.tbl'); const nl = Number(lot.readBigInt64LE(0));
const lots = new Map(); // group -> [{skill hash, curve, w}]
for (let r = 0; r < nl; r++) {
  const o = 8 + r * 20, g = lot.readUInt32LE(o);
  if (!lots.has(g)) lots.set(g, []);
  lots.get(g).push({ sk: lot.readUInt32LE(o + 4), curve: lot.readUInt32LE(o + 8), w: lot.readInt32LE(o + 12) });
}
function poolText(group, resolveNames) {
  const list = lots.get(group);
  if (!list) return '—';
  const tot = list.reduce((a, x) => a + x.w, 0);
  return list.map(x => {
    const nm = resolveNames ? (skillByHash.get(x.sk) || '?' + hex(x.sk)) : null;
    const pct = tot ? (100 * x.w / tot).toFixed(list.length > 1 ? 1 : 0).replace(/\.0$/, '') : '?';
    return `${nm ?? 'effect'} ${pct}% (${lvlRange(x.curve)})`;
  }).join(', ');
}

// ---------- drop routing ----------
const rsl = T('reward_summon_lot.tbl'); const nrsl = Number(rsl.readBigInt64LE(0));
const dropLots = new Map(); // group -> [{summon key, w}]
for (let r = 0; r < nrsl; r++) {
  const o = 8 + r * 16, g = rsl.readUInt32LE(o);
  if (!dropLots.has(g)) dropLots.set(g, []);
  dropLots.get(g).push({ key: rsl.readUInt32LE(o + 4), w: rsl.readInt32LE(o + 8) });
}
const rs = T('reward_summon.tbl'); const nrs = Number(rs.readBigInt64LE(0));
const sources = [];
for (let r = 0; r < nrs; r++) {
  const o = 8 + r * 20;
  sources.push({ key: rs.readUInt32LE(o), lot: rs.readUInt32LE(o + 4), chance: rs.readInt32LE(o + 8) });
}
// resolve source keys to RW_<qid>_<sfx>
const q = T('quest_baseinfo_ex_data.tbl'); const nq = Number(q.readBigInt64LE(0));
const qids = new Set();
for (let r = 0; r < nq; r++) qids.add((q.readUInt32LE(8 + r * 112 + 0x54) & 0xFFFFFF).toString(16).toUpperCase().padStart(6, '0'));
const srcName = new Map();
for (const qid of qids) for (let s = 0; s < 1000; s++) {
  const h = xxh32(`RW_${qid}_${String(s).padStart(3, '0')}`);
  if (!srcName.has(h)) srcName.set(h, { qid, sfx: s });
}
const SFX_LABEL = { 100: 'per-clear', 400: 'reward 400', 403: 'reward 403' };

// ---------- emit ----------
const out = [];
out.push(`# Summon drop tables — every stage, every summon, every stat

**Generated from the vanilla 2.0 tables** by \`scripts/gen-summon-drop-tables.mjs\`
(chain decode: [docs/23](../../docs/23-summon-skill-drops.md)). Drop % and skill
levels shown are VANILLA; with [Summon Drops Maxed](README.md) every listed source
drops at **100%** and every skill rolls at the **top of its level range**.

\`\`\`mermaid
flowchart LR
  Q[Quest clear<br/>reward row] -->|chance 35/50/70/100%| RS[reward_summon]
  RS --> RSL[reward_summon_lot<br/>which summon, weighted]
  RSL --> S[summon.tbl<br/>189 summons, 3–5★]
  S -->|slot 1| L1[summon_lot<br/>passive skill pool]
  S -->|slot 2| L2[summon_lot<br/>call effect pool]
  L1 --> C[summon_curve<br/>skill level odds]
  L2 --> C
\`\`\`
`);

// ---- section 1: stages ----
out.push(`## 1. Stages → summons\n`);
out.push(`${sources.length} reward sources can drop a summon. ${[...srcName.keys()].filter(h => sources.some(s => s.key === h)).length ? '' : ''}Sources whose reward-row id
resolves to a quest are named (suffix _100 = the per-clear reward row; _400/_403 =
2.0-added rows, semantics unconfirmed); the rest are listed by reward-key hash.\n`);
out.push(`| Source | Drop % | Summons (weight) |`);
out.push(`|---|---|---|`);
const fmtDrop = g => (dropLots.get(g) || []).map(x => {
  const s = summons.get(x.key);
  const tot = dropLots.get(g).reduce((a, y) => a + y.w, 0);
  const pct = (100 * x.w / tot).toFixed(1).replace(/\.0$/, '');
  return s ? `${s.name} ${s.rarity}★ ${pct}%` : `?${hex(x.key)} ${pct}%`;
}).join(', ');
const named = sources.filter(s => srcName.has(s.key));
const unnamed = sources.filter(s => !srcName.has(s.key));
named.sort((a, b) => srcName.get(a.key).qid.localeCompare(srcName.get(b.key).qid) || srcName.get(a.key).sfx - srcName.get(b.key).sfx);
for (const s of named) {
  const { qid, sfx } = srcName.get(s.key);
  const qn = questNames.get(qid) || `quest ${qid}`;
  out.push(`| **${qn}** (${qid}, ${SFX_LABEL[sfx] || '_' + sfx}) | ${s.chance}% | ${fmtDrop(s.lot)} |`);
}
for (const s of unnamed)
  out.push(`| unidentified source \`${hex(s.key)}\` | ${s.chance}% | ${fmtDrop(s.lot)} |`);

// ---- section 2: summons ----
out.push(`\n## 2. Summons → possible stats\n`);
out.push(`Each summon rolls **slot 1** (a passive skill from the sigil-skill set) and
**slot 2** (its summon-specific call effect) when it drops. Percentages are the
in-pool odds; the level range in parentheses is what vanilla can roll — the mod
always gives the top of the range. Call-effect flavor text is the species'
in-battle announcement where the game defines one.\n`);
out.push(`| Summon | ★ | Slot 1 — passive skill pool | Slot 2 — call effect |`);
out.push(`|---|---|---|---|`);
const sorted = [...summons.values()].sort((a, b) => a.name.localeCompare(b.name) || a.rarity - b.rarity);
const seen = new Set();
for (const s of sorted) {
  const dupKey = s.name + '|' + s.rarity + '|' + s.lot1 + '|' + s.lot2;
  if (seen.has(dupKey)) continue;
  seen.add(dupKey);
  const call = (s.soId && soCall.get(s.soId)) ? soCall.get(s.soId) : (s.lot2 ? `${(lots.get(s.lot2) || []).length} effect variant(s)` : '—');
  const lot2txt = s.lot2 ? `${call} ${poolText(s.lot2, false).startsWith('—') ? '' : '(' + (lots.get(s.lot2) || []).map(x => lvlRange(x.curve)).filter((v, i, a) => a.indexOf(v) === i).join('/') + ')'}` : '—';
  out.push(`| **${s.name}** | ${s.rarity} | ${s.lot1 ? poolText(s.lot1, true) : '—'} | ${lot2txt} |`);
}

const stats = { sources: sources.length, namedSources: named.length, summonRows: summons.size, distinctListed: seen.size };
out.push(`\n---\n*${stats.sources} drop sources (${stats.namedSources} quest-named), ${stats.summonRows} summon variants (${stats.distinctListed} distinct listed). Regenerate: \`node scripts/gen-summon-drop-tables.mjs\`.*`);
writeFileSync('mods/summon-drops/DROP-TABLES.md', out.join('\n') + '\n');
console.log(JSON.stringify(stats));
