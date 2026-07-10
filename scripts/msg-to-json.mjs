// Decode a GBFR .msg (MessagePack) file to JSON on stdout.
// Usage: node scripts/msg-to-json.mjs <file.msg>
import { readFileSync } from 'node:fs';

const buf = readFileSync(process.argv[2]);
let o = 0;

function decode() {
  const b = buf[o++];
  // positive fixint / negative fixint
  if (b <= 0x7f) return b;
  if (b >= 0xe0) return b - 256;
  // fixmap / fixarray / fixstr
  if (b >= 0x80 && b <= 0x8f) return map(b - 0x80);
  if (b >= 0x90 && b <= 0x9f) return arr(b - 0x90);
  if (b >= 0xa0 && b <= 0xbf) return str(b - 0xa0);
  switch (b) {
    case 0xc0: return null;
    case 0xc2: return false;
    case 0xc3: return true;
    case 0xc4: return bin(buf[o++]);
    case 0xc5: { const n = buf.readUInt16BE(o); o += 2; return bin(n); }
    case 0xc6: { const n = buf.readUInt32BE(o); o += 4; return bin(n); }
    case 0xca: { const v = buf.readFloatBE(o); o += 4; return fl(v); }
    case 0xcb: { const v = buf.readDoubleBE(o); o += 8; return fl(v); }
    case 0xcc: return buf[o++];
    case 0xcd: { const v = buf.readUInt16BE(o); o += 2; return v; }
    case 0xce: { const v = buf.readUInt32BE(o); o += 4; return v; }
    case 0xcf: { const v = buf.readBigUInt64BE(o); o += 8; return Number(v); }
    case 0xd0: return buf.readInt8(o++);
    case 0xd1: { const v = buf.readInt16BE(o); o += 2; return v; }
    case 0xd2: { const v = buf.readInt32BE(o); o += 4; return v; }
    case 0xd3: { const v = buf.readBigInt64BE(o); o += 8; return Number(v); }
    case 0xd9: return str(buf[o++]);
    case 0xda: { const n = buf.readUInt16BE(o); o += 2; return str(n); }
    case 0xdb: { const n = buf.readUInt32BE(o); o += 4; return str(n); }
    case 0xdc: { const n = buf.readUInt16BE(o); o += 2; return arr(n); }
    case 0xdd: { const n = buf.readUInt32BE(o); o += 4; return arr(n); }
    case 0xde: { const n = buf.readUInt16BE(o); o += 2; return map(n); }
    case 0xdf: { const n = buf.readUInt32BE(o); o += 4; return map(n); }
    default: throw new Error(`unhandled msgpack byte 0x${b.toString(16)} @ ${o - 1}`);
  }
}
const fl = (v) => Math.abs(v) < 1e-30 ? 0 : +v.toPrecision(7);
const str = (n) => { const s = buf.toString('utf8', o, o + n); o += n; return s; };
const bin = (n) => { const s = buf.toString('hex', o, o + n); o += n; return `<bin:${s}>`; };
const arr = (n) => { const a = []; for (let i = 0; i < n; i++) a.push(decode()); return a; };
const map = (n) => { const m = {}; for (let i = 0; i < n; i++) { const k = decode(); m[k] = decode(); } return m; };

const out = [];
while (o < buf.length) out.push(decode());
console.log(JSON.stringify(out.length === 1 ? out[0] : out, null, 1));
