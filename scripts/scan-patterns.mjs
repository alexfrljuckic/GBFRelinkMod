// Scan a PE file for the IDA-style byte patterns used in GBFRelinkFix's source.
// Usage: node scan-patterns.mjs <dllmain.cpp> <target.exe>
import { readFileSync } from 'node:fs';

const [, , srcPath, exePath] = process.argv;
const src = readFileSync(srcPath, 'utf8');
const exe = readFileSync(exePath);

// Grab ("Name", "AA BB ?? ...") pairs from FindPattern / MAKE_MIDHOOK calls.
const re = /(?:FindPattern\(|MAKE_MIDHOOK\(\s*\w+,\s*)"([^"]+)",\s*"((?:[0-9A-Fa-f]{2}|\?\?)(?:\s+(?:[0-9A-Fa-f]{2}|\?\?))+)"/g;
const patterns = [];
for (const m of src.matchAll(re)) patterns.push({ name: m[1], sig: m[2] });

function findAll(buf, sig, cap = 5) {
  const toks = sig.trim().split(/\s+/);
  const bytes = toks.map(t => (t === '??' ? -1 : parseInt(t, 16)));
  const n = bytes.length;
  // anchor on the longest run of literal bytes for speed
  let bestStart = 0, bestLen = 0, runStart = -1;
  for (let i = 0; i <= n; i++) {
    if (i < n && bytes[i] >= 0) { if (runStart < 0) runStart = i; }
    else if (runStart >= 0) {
      if (i - runStart > bestLen) { bestLen = i - runStart; bestStart = runStart; }
      runStart = -1;
    }
  }
  const anchor = Buffer.from(bytes.slice(bestStart, bestStart + bestLen));
  const hits = [];
  let idx = exe.indexOf(anchor, 0);
  while (idx !== -1 && hits.length < cap) {
    const base = idx - bestStart;
    if (base >= 0 && base + n <= buf.length) {
      let ok = true;
      for (let j = 0; j < n; j++) {
        if (bytes[j] >= 0 && buf[base + j] !== bytes[j]) { ok = false; break; }
      }
      if (ok) hits.push(base);
    }
    idx = buf.indexOf(anchor, idx + 1);
  }
  return hits;
}

let matched = 0;
for (const p of patterns) {
  const hits = findAll(exe, p.sig);
  const status = hits.length === 1 ? 'UNIQUE ' : hits.length > 1 ? `MULTI:${hits.length}` : 'MISS   ';
  if (hits.length >= 1) matched++;
  console.log(`${status}  ${p.name}${hits.length ? '  @ 0x' + hits.map(h => h.toString(16)).join(', 0x') : ''}`);
}
console.log(`\n${matched}/${patterns.length} patterns found in ${exePath}`);
