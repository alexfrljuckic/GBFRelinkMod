// Disambiguate the "Resolution List" pattern (4 matches in the 2.0 exe) by resolving,
// for each candidate, where its two `lea reg,[rip+disp32]` instructions point, and
// reading the DWORD stored there. The correct site's leas point at the resolution
// width/height the mod overwrites — so plausible values (e.g. 3840 / 2160) win.
//
// Usage: node disambiguate-reslist.mjs <target.exe>
import { readFileSync } from 'node:fs';

const exe = readFileSync(process.argv[2]);

// --- minimal PE parse: build file<->RVA section map ---
const peOff = exe.readUInt32LE(0x3c);
if (exe.toString('latin1', peOff, peOff + 4) !== 'PE\0\0') throw new Error('not a PE');
const numSections = exe.readUInt16LE(peOff + 6);
const optSize = exe.readUInt16LE(peOff + 20);
const secTableOff = peOff + 24 + optSize;
const sections = [];
for (let i = 0; i < numSections; i++) {
  const o = secTableOff + i * 40;
  sections.push({
    name: exe.toString('latin1', o, o + 8).replace(/\0+$/, ''),
    vaddr: exe.readUInt32LE(o + 12),
    vsize: exe.readUInt32LE(o + 8),
    raw: exe.readUInt32LE(o + 20),
    rawSize: exe.readUInt32LE(o + 16),
  });
}
const fileToRva = (f) => {
  for (const s of sections) if (f >= s.raw && f < s.raw + s.rawSize) return f - s.raw + s.vaddr;
  return null;
};
const rvaToFile = (r) => {
  for (const s of sections) if (r >= s.vaddr && r < s.vaddr + s.vsize) {
    const f = r - s.vaddr + s.raw;
    return f < s.raw + s.rawSize ? f : null;
  }
  return null;
};

// The mod treats ResolutionList+0x9 as a disp32 (first lea's displacement) and
// ResolutionList+0x9+0x14 (=+0x1D) as the second. GetRelativeAddr = addr_of_disp+4+disp.
function resolveLeaTarget(dispFileOff) {
  const disp = exe.readInt32LE(dispFileOff);
  const dispRva = fileToRva(dispFileOff);
  const targetRva = dispRva + 4 + disp;            // rip is the byte after the disp32
  const tf = rvaToFile(targetRva);
  return { targetRva, targetFile: tf, dword: tf != null ? exe.readUInt32LE(tf) : null };
}

const candidates = [0x17bf3b, 0x1c82e1, 0x215c09, 0x21b282];
for (const base of candidates) {
  const ctx = [...exe.subarray(base, base + 27)].map(b => b.toString(16).padStart(2, '0')).join(' ');
  const a = resolveLeaTarget(base + 0x9);
  const b = resolveLeaTarget(base + 0x9 + 0x14);
  const fmt = (t) => t.targetFile == null
    ? `rva=0x${t.targetRva.toString(16)} (not in file)`
    : `rva=0x${t.targetRva.toString(16)} value=${t.dword} (0x${t.dword.toString(16)})`;
  console.log(`\n=== candidate @ file 0x${base.toString(16)} (RVA 0x${fileToRva(base).toString(16)}) ===`);
  console.log(`  bytes: ${ctx}`);
  console.log(`  lea#1 (+0x9)  -> ${fmt(a)}`);
  console.log(`  lea#2 (+0x1D) -> ${fmt(b)}`);
}
