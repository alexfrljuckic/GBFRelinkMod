// Static security audit of a PE (DLL/asi): list imported DLLs+functions, flag sensitive
// APIs, and scan strings for network / persistence / suspicious indicators.
// Usage: node pe-audit.mjs <file.asi>
import { readFileSync } from 'node:fs';

const b = readFileSync(process.argv[2]);
const pe = b.readUInt32LE(0x3c);
const optSz = b.readUInt16LE(pe + 20), nSec = b.readUInt16LE(pe + 6), magic = b.readUInt16LE(pe + 24);
const plus = magic === 0x20b;
const dd = pe + 24 + (plus ? 112 : 96);
const impRva = b.readUInt32LE(dd + 8);
const secOff = pe + 24 + optSz; const secs = [];
for (let i = 0; i < nSec; i++) { const o = secOff + i * 40; secs.push({ v: b.readUInt32LE(o + 12), vs: b.readUInt32LE(o + 8), r: b.readUInt32LE(o + 20) }); }
const r2f = r => { for (const s of secs) if (r >= s.v && r < s.v + s.vs) return r - s.v + s.r; return null; };
const cstr = f => { let s = ''; while (f < b.length && b[f]) { s += String.fromCharCode(b[f++]); } return s; };

let io = r2f(impRva); const dlls = {};
while (io) {
  const nameRva = b.readUInt32LE(io + 12), ft = b.readUInt32LE(io + 16), ot = b.readUInt32LE(io);
  if (nameRva === 0 && ft === 0) break;
  const dll = cstr(r2f(nameRva)).toLowerCase(); const fns = [];
  let t = r2f(ot || ft);
  if (t != null) {
    while (true) {
      const e = plus ? b.readBigUInt64LE(t) : BigInt(b.readUInt32LE(t)); if (e === 0n) break;
      const ord = plus ? (e >> 63n) & 1n : (e >> 31n) & 1n;
      if (!ord) { const h = Number(e & (plus ? 0x7fffffffffffffffn : 0x7fffffffn)); fns.push(cstr(r2f(h) + 2)); } else fns.push('ord');
      t += plus ? 8 : 4;
    }
  }
  dlls[dll] = fns; io += 20;
}

console.log('=== IMPORTED DLLs ===');
for (const d of Object.keys(dlls).sort()) console.log('  ' + d + '  (' + dlls[d].length + ' fns)');
const all = Object.entries(dlls).flatMap(([d, f]) => f.map(x => d + '!' + x));
const sens = /wininet|winhttp|urlmon|ws2_32|socket|internetopen|urldownload|winexec|shellexecute|createprocess|writeprocessmemory|virtualallocex|setwindowshook|regsetvalue|regcreatekey|createservice|cryptencrypt|getasynckeystate|keybd_event|adjusttokenprivileges|openprocess/i;
console.log('\n=== SENSITIVE IMPORTS ===');
const hits = all.filter(x => sens.test(x));
console.log(hits.length ? hits.join('\n') : '  (none)');

// string scan for network / suspicious indicators
const strs = new Set(); let cur = '';
for (let i = 0; i < b.length; i++) { const c = b[i]; if (c >= 0x20 && c < 0x7e) cur += String.fromCharCode(c); else { if (cur.length >= 6) strs.add(cur); cur = ''; } }
const arr = [...strs];
const urlRe = /https?:\/\/|\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b|\.onion|discord\.com\/api|pastebin|\bAppData\b|\bStartup\b|\.exe\b|powershell|cmd\.exe/i;
console.log('\n=== SUSPICIOUS STRINGS (urls/ip/paths/exec) ===');
const susp = arr.filter(s => urlRe.test(s) && s.length < 120);
console.log(susp.length ? [...new Set(susp)].sort().join('\n') : '  (none)');
