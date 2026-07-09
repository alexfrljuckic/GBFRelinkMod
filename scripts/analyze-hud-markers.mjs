// Static analysis of the HUD: Markers hook site in the 2.0 exe.
// - maps file offset <-> RVA (so we get the x64dbg address = imagebase + RVA)
// - dumps the region and the neighbouring resolved rip-relative constants
// Usage: node analyze-hud-markers.mjs <target.exe>
import { readFileSync } from 'node:fs';
const exe = readFileSync(process.argv[2]);

const peOff = exe.readUInt32LE(0x3c);
const numSec = exe.readUInt16LE(peOff + 6), optSize = exe.readUInt16LE(peOff + 20);
const imageBase = exe.readBigUInt64LE(peOff + 24 + 24); // opt header + 24 -> ImageBase (PE32+)
const st = peOff + 24 + optSize; const secs = [];
for (let i = 0; i < numSec; i++) { const o = st + i * 40;
  secs.push({ name: exe.toString('latin1', o, o+8).replace(/\0+$/,''), vaddr: exe.readUInt32LE(o+12), vsize: exe.readUInt32LE(o+8), raw: exe.readUInt32LE(o+20), rawSize: exe.readUInt32LE(o+16) }); }
const f2r = f => { for (const s of secs) if (f>=s.raw && f<s.raw+s.rawSize) return f-s.raw+s.vaddr; return null; };
const r2f = r => { for (const s of secs) if (r>=s.vaddr && r<s.vaddr+s.vsize){ const f=r-s.vaddr+s.raw; return f<s.raw+s.rawSize?f:null;} return null; };

const START = 0x2656f9d;                 // pattern start (vmulss)
const rva = f2r(START);
console.log(`image base       : 0x${imageBase.toString(16)}`);
console.log(`markers file off : 0x${START.toString(16)}`);
console.log(`markers RVA      : 0x${rva.toString(16)}  (x64dbg: <granblue_fantasy_relink.exe base> + 0x${rva.toString(16)})`);
console.log(`log-style addr   : granblue_fantasy_relink.exe+${rva.toString(16)}`);

// raw bytes
const bytes = [...exe.subarray(START, START + 44)];
console.log('\nbytes:');
console.log('  ' + bytes.map(b => b.toString(16).padStart(2,'0')).join(' '));

// resolve the vmulss rip-relative constant (C5 F0 59 0D disp32 at START+4)
const dispOff = START + 4;
const disp = exe.readInt32LE(dispOff);
const targetRva = f2r(dispOff) + 4 + disp;
const tf = r2f(targetRva);
console.log(`\nvmulss constant  : [rip+0x${disp.toString(16)}] -> RVA 0x${targetRva.toString(16)} = float ${tf!=null?exe.readFloatLE(tf):'?'}`);

// the movzx displacement (0F B6 86 disp32) — find it in the region
for (let i = 0; i < bytes.length-6; i++) {
  if (bytes[i]===0x0f && bytes[i+1]===0xb6 && bytes[i+2]===0x86) {
    const d = exe.readUInt32LE(START+i+3);
    console.log(`movzx eax,[rsi+0x${d.toString(16)}] at region+0x${i.toString(16)}`);
  }
}
