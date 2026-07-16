// Find RIP-relative code references to a file offset (e.g. a string) in the game exe.
// Scans .text for any disp32 such that RVA(next_insn) + disp == RVA(target).
// Usage: node scripts/find-xrefs.mjs <exe> <fileOffsetHexOrDec>
//        node scripts/find-xrefs.mjs <exe> rva:<hex>     (target given as RVA, e.g. a global)
import { readFileSync } from 'node:fs';

const [, , exePath, offArg] = process.argv;
const exe = readFileSync(exePath);
const rvaMode = offArg.startsWith('rva:');
const target = rvaMode ? parseInt(offArg.slice(4), 16)
  : offArg.startsWith('0x') ? parseInt(offArg, 16) : parseInt(offArg, 10);

// --- PE parse ---
const e_lfanew = exe.readUInt32LE(0x3c);
const numSections = exe.readUInt16LE(e_lfanew + 6);
const optSize = exe.readUInt16LE(e_lfanew + 20);
const secTable = e_lfanew + 24 + optSize;
const sections = [];
for (let i = 0; i < numSections; i++) {
  const s = secTable + i * 40;
  sections.push({
    name: exe.toString('ascii', s, s + 8).replace(/\0+$/, ''),
    vsize: exe.readUInt32LE(s + 8),
    rva: exe.readUInt32LE(s + 12),
    rawSize: exe.readUInt32LE(s + 16),
    rawPtr: exe.readUInt32LE(s + 20),
  });
}
const fileToRva = (off) => {
  for (const s of sections) if (off >= s.rawPtr && off < s.rawPtr + s.rawSize) return s.rva + (off - s.rawPtr);
  return -1;
};
const targetRva = rvaMode ? target : fileToRva(target);
if (targetRva < 0) { console.error('target offset not in any section'); process.exit(1); }
console.log(`target file=0x${target.toString(16)} rva=0x${targetRva.toString(16)} va=0x${(0x140000000 + targetRva).toString(16)}`);

const text = sections.find(s => s.name === '.text');
console.log(`.text rva=0x${text.rva.toString(16)} rawPtr=0x${text.rawPtr.toString(16)} size=0x${text.rawSize.toString(16)}`);

// --- scan: for each position p (file offset of a disp32), insn ends at p+4 ---
// RVA(p+4) + disp == targetRva  =>  disp == targetRva - (text.rva + (p+4 - text.rawPtr))
const end = text.rawPtr + text.rawSize - 4;
const hits = [];
for (let p = text.rawPtr; p < end; p++) {
  const disp = exe.readInt32LE(p);
  const rvaNext = text.rva + (p + 4 - text.rawPtr);
  if (rvaNext + disp === targetRva) {
    const insnCtx = exe.toString('hex', p - 3, p + 4);
    hits.push({ dispFile: p, rva: text.rva + (p - text.rawPtr), ctx: insnCtx });
  }
}
for (const h of hits)
  console.log(`xref disp@file=0x${h.dispFile.toString(16)} rva=0x${h.rva.toString(16)} ctx(-3..+4)=${h.ctx}`);
console.log(`${hits.length} xref(s)`);
