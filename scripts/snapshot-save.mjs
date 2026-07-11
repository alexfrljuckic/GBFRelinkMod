// Snapshot the newest GBFR save into extracted/save-snapshots/ for before/after
// diffing with: node scripts/audit-save-sigil-traits.mjs --since <snapshot>
// Run BEFORE pulling transmarvels you want to audit.
import { readdirSync, statSync, copyFileSync, mkdirSync } from 'node:fs';

const SAVE_DIR = `${process.env.LOCALAPPDATA}\\GBFR\\Saved\\SaveGames`;
const OUT_DIR = 'extracted/save-snapshots';
const slots = readdirSync(SAVE_DIR).filter(f => /^SaveData\d+\.dat$/i.test(f));
if (!slots.length) { console.error(`no SaveData<N>.dat in ${SAVE_DIR}`); process.exit(1); }
const newest = slots.map(f => `${SAVE_DIR}\\${f}`).sort((a, b) => statSync(b).mtimeMs - statSync(a).mtimeMs)[0];
const mtime = statSync(newest).mtime;
const stamp = mtime.toISOString().replace(/[:T]/g, '-').slice(0, 19);
mkdirSync(OUT_DIR, { recursive: true });
const out = `${OUT_DIR}/${newest.split('\\').pop().replace('.dat', '')}-${stamp}.dat`;
copyFileSync(newest, out);
console.log(`snapshot: ${out}`);
console.log(`(save last written ${mtime.toLocaleString()} — make sure the game has SAVED the state you think you're snapshotting)`);
console.log(`after pulling: node scripts/audit-save-sigil-traits.mjs --since "${out}"`);
