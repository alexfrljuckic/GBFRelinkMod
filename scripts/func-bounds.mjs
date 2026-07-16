// Look up the function containing a given RVA via the PE exception directory (.pdata).
// Usage: node scripts/func-bounds.mjs <exe> <rvaHex> [<rvaHex>...]
import { readFileSync } from 'node:fs';

const [, , exePath, ...rvas] = process.argv;
const exe = readFileSync(exePath);
const e_lfanew = exe.readUInt32LE(0x3c);
const numSections = exe.readUInt16LE(e_lfanew + 6);
const optSize = exe.readUInt16LE(e_lfanew + 20);
const opt = e_lfanew + 24;
// data directory 3 = exception
const dd = opt + 112 + 3 * 8; // PE32+: 112 bytes to first data dir
const excRva = exe.readUInt32LE(dd), excSize = exe.readUInt32LE(dd + 4);
const secTable = opt + optSize;
const sections = [];
for (let i = 0; i < numSections; i++) {
  const s = secTable + i * 40;
  sections.push({ rva: exe.readUInt32LE(s + 12), rawSize: exe.readUInt32LE(s + 16), rawPtr: exe.readUInt32LE(s + 20) });
}
const rvaToFile = (rva) => {
  for (const s of sections) if (rva >= s.rva && rva < s.rva + s.rawSize) return s.rawPtr + (rva - s.rva);
  return -1;
};
const excOff = rvaToFile(excRva);
const n = Math.floor(excSize / 12);
// binary search for entry containing target
function find(target) {
  let lo = 0, hi = n - 1, best = null;
  while (lo <= hi) {
    const mid = (lo + hi) >> 1;
    const off = excOff + mid * 12;
    const start = exe.readUInt32LE(off), end = exe.readUInt32LE(off + 4);
    if (target < start) hi = mid - 1;
    else if (target >= end) lo = mid + 1;
    else { best = { start, end, unwind: exe.readUInt32LE(off + 8) }; break; }
  }
  return best;
}
for (const arg of rvas) {
  const t = parseInt(arg, 16);
  let f = find(t);
  // chained unwind info: walk to the root function start
  let root = f;
  if (f) {
    let guard = 0;
    while (root && guard++ < 20) {
      const uoff = rvaToFile(root.unwind);
      const flags = exe[uoff] >> 3;
      if (!(flags & 0x4)) break; // UNW_FLAG_CHAININFO
      const cnt = exe[uoff + 2];
      const chained = uoff + 4 + ((cnt + 1) & ~1) * 2;
      root = { start: exe.readUInt32LE(chained), end: exe.readUInt32LE(chained + 4), unwind: exe.readUInt32LE(chained + 8) };
    }
  }
  console.log(`rva 0x${t.toString(16)} -> func 0x${f?.start.toString(16)}..0x${f?.end.toString(16)}` +
    (root && f && root.start !== f.start ? ` (root 0x${root.start.toString(16)}..0x${root.end.toString(16)})` : ''));
}
