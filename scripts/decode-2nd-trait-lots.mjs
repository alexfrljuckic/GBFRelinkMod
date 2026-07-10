// Decode the random-2nd-trait lots for sigils: gem.SkillTypeLotIdForRandom2ndSkill
// -> skill_type_lot (sub-lots + %) -> skill_lot (traits + weights), with English names.
// Usage: node scripts/decode-2nd-trait-lots.mjs
import { readFileSync } from 'node:fs';
import { execSync } from 'node:child_process';

const SQL = 'tools\\sqlite\\sqlite3.exe';
const DB = 'extracted/tables-2.0-partial.sqlite';
const q = (sql) => execSync(`${SQL} ${DB} "${sql.replace(/"/g, '""')}"`).toString().trim();

const names = new Map();
for (const l of readFileSync('extracted/skill-names-en.tsv', 'utf8').split('\n')) {
  const m = l.match(/^TXT_(SKILL_\w+)\t(.*)$/);
  if (m && !names.has(m[1])) names.set(m[1], m[2]);
}

// which type-lots do the pool sigils use?
const typeLots = q(`SELECT Key, SkillLotId1, SkillLotId2, SkillLotId3, SkillLotId4, SkillLotId5, SkillLotId6, ChancePercent1, ChancePercent2, ChancePercent3, ChancePercent4, ChancePercent5, ChancePercent6 FROM skill_type_lot;`)
  .split('\n').map(l => l.split('|'));

for (const wanted of ['2', '5']) {
  const row = typeLots.find(r => r[0] === wanted);
  if (!row) { console.log(`type-lot ${wanted}: NOT FOUND`); continue; }
  console.log(`\n===== skill_type_lot ${wanted} ${wanted === '2' ? '(generic V+ sigils)' : '(character sigils+ / Warpath+)'} =====`);
  for (let i = 0; i < 6; i++) {
    const key = row[1 + i], pct = row[7 + i];
    if (!key || pct === '0') continue;
    const rows = q(`SELECT SkillId, Unk3 FROM skill_lot WHERE Key='${key}' ORDER BY Unk3 DESC;`).split('\n').filter(Boolean);
    const tot = rows.reduce((s, r) => s + +r.split('|')[1], 0);
    console.log(`\n  sub-lot ${key} — ${pct}% of 2nd-trait rolls, ${rows.length} possible traits:`);
    for (const r of rows) {
      const [sid, w] = r.split('|');
      const inLot = (100 * w / tot).toFixed(1);
      const overall = (pct * w / tot).toFixed(2);
      console.log(`    ${(names.get(sid) ?? sid).padEnd(26)} ${String(inLot).padStart(5)}% of sub-lot = ${overall}% overall  (${sid}, w=${w})`);
    }
  }
}
