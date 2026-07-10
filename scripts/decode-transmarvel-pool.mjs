// Decode the Transmarvel/curio forging pools: join gacha_rate_group + gacha_lot
// with English sigil names (extracted/geen-names-en.tsv), brute-forcing any raw
// XXHash32 ItemIds against the GEEN_* namespace.
// Usage: node scripts/decode-transmarvel-pool.mjs
import { readFileSync } from 'node:fs';
import { execSync } from 'node:child_process';

const SQLITE = 'tools\\sqlite\\sqlite3.exe';

// --- GBFR custom XXHash32 (GBFRDataTools.Hashing/XXHash32Custom.cs):
// seed 0x178A54A4, hardcoded v1..v4, do/while(p>16) quirk ---
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
      v1 = round(v1, data.readUInt32LE(i));
      v2 = round(v2, data.readUInt32LE(i + 4));
      v3 = round(v3, data.readUInt32LE(i + 8));
      v4 = round(v4, data.readUInt32LE(i + 12));
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

// --- load name maps: GEEN_xxx_yy / ITEM_xx_yyyy -> English ---
const names = new Map();
for (const file of ['extracted/geen-names-en.tsv', 'extracted/item-names-en.tsv']) {
  for (const line of readFileSync(file, 'utf8').split('\n')) {
    const m = line.match(/^TXT_((?:GEEN|ITEM)_\w+)\t(.*)$/);
    if (m && !names.has(m[1])) names.set(m[1], m[2]);
  }
}

// --- brute-force table: hash -> GEEN id (cover generous id space) ---
const hashToGeen = new Map();
for (let n = 0; n <= 400; n++) {
  for (let s = 0; s <= 99; s++) {
    const id = `GEEN_${String(n).padStart(3, '0')}_${String(s).padStart(2, '0')}`;
    hashToGeen.set(xxh32(id).toString(16).toUpperCase().padStart(8, '0'), id);
  }
}
// item namespace: ITEM_<02d>_<04d> (wrightstones are ITEM_25..28_xxxx)
for (let a = 0; a <= 99; a++) {
  for (let b = 0; b <= 9999; b++) {
    const id = `ITEM_${String(a).padStart(2, '0')}_${String(b).padStart(4, '0')}`;
    hashToGeen.set(xxh32(id).toString(16).toUpperCase().padStart(8, '0'), id);
  }
}

function resolve(itemId) {
  if (names.has(itemId)) return { id: itemId, name: names.get(itemId) };
  if (/^[0-9A-F]{8}$/.test(itemId)) {
    const geen = hashToGeen.get(itemId);
    if (geen) return { id: geen, name: names.get(geen) ?? '(no text)' };
    return { id: itemId, name: '(unresolved hash)' };
  }
  return { id: itemId, name: names.get(itemId) ?? '(non-GEEN id)' };
}

// --- pull rate groups + lots ---
const groups = {
  '489B6160': 'FORGING_01 gem', 'B209049D': 'FORGING_02 gem', 'BBAEDB0D': 'FORGING_02 wrightstone',
  '57AB19DE': 'FORGING_03 gem', 'B845CAC6': 'FORGING_03 wrightstone',
  '27509C51': 'FORGING_HIGH (Transmarvel) gem', '67716D8A': 'FORGING_HIGH (Transmarvel) wrightstone',
};
const grg = execSync(`${SQLITE} extracted/grg_test/grg.sqlite -tabs "SELECT Key,GachaLotId,Weight,Unk4 FROM gacha_rate_group;"`)
  .toString().trim().split('\n').map(l => l.split('\t'));
const lots = execSync(`${SQLITE} extracted/gl_test/gacha_lot.sqlite -tabs "SELECT Key,ItemId,Weight,TraitLevel,Unk18 FROM gacha_lot;"`)
  .toString().trim().split('\n').map(l => l.split('\t'));
const lotByKey = new Map();
for (const [key, itemId, w, tl, u] of lots) {
  if (!lotByKey.has(key)) lotByKey.set(key, []);
  lotByKey.get(key).push({ itemId, w: +w, tl: +tl, u: +u });
}

for (const [gkey, glabel] of Object.entries(groups)) {
  const rows = grg.filter(r => r[0] === gkey);
  const total = rows.reduce((s, r) => s + +r[2], 0);
  console.log(`\n=== ${glabel} [${gkey}] — total weight ${total} ===`);
  for (const [, lotId, weight, unk4] of rows.sort((a, b) => +b[2] - +a[2])) {
    const pct = (100 * +weight / total).toFixed(2);
    const items = lotByKey.get(lotId) ?? [];
    console.log(`  bucket ${lotId}  w=${weight} (${pct}%)${+unk4 ? '  [RARE flag]' : ''}  — ${items.length} items:`);
    for (const it of items) {
      const r = resolve(it.itemId);
      const per = items.length ? (pct / items.length).toFixed(3) : 0;
      console.log(`      ${r.name}${it.tl === 15 ? ' [+MAX TRAIT lv15]' : it.tl ? ` [trait lv${it.tl}]` : ''}  (${r.id})  ≈${per}%/roll`);
    }
  }
}
