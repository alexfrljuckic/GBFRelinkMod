// Extract id_hash_ -> text_ pairs from a GBFR .msg (MessagePack) text table.
// Usage: node scripts/extract-text-msg.mjs <file.msg> [filterPrefix]
import { readFileSync } from 'node:fs';

const buf = readFileSync(process.argv[2]);
const filter = process.argv[3] || '';

// minimal msgpack string reader at offset; returns [str, nextOffset] or null
function readStr(o) {
  const b = buf[o];
  if (b >= 0xa0 && b <= 0xbf) { const n = b - 0xa0; return [buf.toString('utf8', o + 1, o + 1 + n), o + 1 + n]; }
  if (b === 0xd9) { const n = buf[o + 1]; return [buf.toString('utf8', o + 2, o + 2 + n), o + 2 + n]; }
  if (b === 0xda) { const n = buf.readUInt16BE(o + 1); return [buf.toString('utf8', o + 3, o + 3 + n), o + 3 + n]; }
  if (b === 0xdb) { const n = buf.readUInt32BE(o + 1); return [buf.toString('utf8', o + 5, o + 5 + n), o + 5 + n]; }
  return null;
}

const ID = Buffer.from('id_hash_');
const TXT = Buffer.from('text_');
let o = 0, count = 0;
while ((o = buf.indexOf(ID, o)) !== -1) {
  o += ID.length;
  const id = readStr(o);
  if (!id) continue;
  // find the next text_ key after this id
  const t = buf.indexOf(TXT, o);
  if (t === -1) break;
  const val = readStr(t + TXT.length);
  if (!val) continue;
  if (!filter || id[0].includes(filter)) {
    console.log(`${id[0]}\t${val[0].replace(/[\r\n\t]+/g, ' ')}`);
    count++;
  }
  o = id[1];
}
console.error(`# ${count} entries`);
