// Search every TEXT column of every table in a sqlite DB for given value(s).
// Usage: node scripts/find-hash-in-tables.mjs <db> <value> [value2 ...]
import { execSync } from 'node:child_process';

const SQL = 'tools\\sqlite\\sqlite3.exe';
const [db, ...targets] = process.argv.slice(2);
const q = (sql) => execSync(`${SQL} ${db} "${sql.replace(/"/g, '""')}"`).toString().trim();

const tables = q('.tables').split(/\s+/).filter(Boolean);
for (const t of tables) {
  let cols;
  try {
    cols = q(`PRAGMA table_info(${t});`).split('\n').map(l => l.split('|')).filter(c => c[2] === 'TEXT').map(c => c[1]);
  } catch { continue; }
  for (const c of cols) {
    for (const target of targets) {
      try {
        const n = q(`SELECT COUNT(*) FROM ${t} WHERE ${c}='${target}';`);
        if (+n > 0) console.log(`FOUND ${target} in ${t}.${c} rows=${n}`);
      } catch {}
    }
  }
}
console.log('search done');
