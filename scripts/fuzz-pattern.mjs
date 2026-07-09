// Try to rescue a missing byte pattern by scanning progressively truncated
// and ModRM-wildcarded variants against a target binary.
// Usage: node fuzz-pattern.mjs <target.exe> "<pattern>"
import { readFileSync } from 'node:fs';

const [, , exePath, sig] = process.argv;
const exe = readFileSync(exePath);

function scan(tokens, cap = 8) {
  const bytes = tokens.map(t => (t === '??' ? -1 : parseInt(t, 16)));
  const n = bytes.length;
  let bestStart = 0, bestLen = 0, runStart = -1;
  for (let i = 0; i <= n; i++) {
    if (i < n && bytes[i] >= 0) { if (runStart < 0) runStart = i; }
    else if (runStart >= 0) {
      if (i - runStart > bestLen) { bestLen = i - runStart; bestStart = runStart; }
      runStart = -1;
    }
  }
  if (bestLen === 0) return [];
  const anchor = Buffer.from(bytes.slice(bestStart, bestStart + bestLen));
  const hits = [];
  let idx = exe.indexOf(anchor, 0);
  while (idx !== -1 && hits.length < cap) {
    const base = idx - bestStart;
    if (base >= 0 && base + n <= exe.length) {
      let ok = true;
      for (let j = 0; j < n; j++) if (bytes[j] >= 0 && exe[base + j] !== bytes[j]) { ok = false; break; }
      if (ok) hits.push(base);
    }
    idx = exe.indexOf(anchor, idx + 1);
  }
  return hits;
}

const toks = sig.trim().split(/\s+/);
console.log(`full pattern (${toks.length} bytes): ${scan(toks).length} hits`);

// 1) Truncate from the end, keep at least 8 bytes
console.log('\n-- tail truncation --');
for (let len = toks.length - 1; len >= 8; len--) {
  const hits = scan(toks.slice(0, len));
  if (hits.length > 0) {
    console.log(`len=${len}: ${hits.length} hits @ ${hits.slice(0, 4).map(h => '0x' + h.toString(16)).join(', ')}`);
    if (hits.length <= 4) break; // good enough to inspect
  }
}

// 2) Truncate from the start (pattern may have gained a prefix instruction)
console.log('\n-- head truncation --');
for (let off = 1; off <= toks.length - 8; off++) {
  const hits = scan(toks.slice(off));
  if (hits.length > 0) {
    console.log(`skip=${off}: ${hits.length} hits @ ${hits.slice(0, 4).map(h => '0x' + h.toString(16)).join(', ')}`);
    if (hits.length <= 4) break;
  }
}

// 3) Wildcard each literal byte one at a time (single-byte drift, e.g. different reg encoding)
console.log('\n-- single-byte wildcard --');
for (let i = 0; i < toks.length; i++) {
  if (toks[i] === '??') continue;
  const v = toks.slice(); v[i] = '??';
  const hits = scan(v);
  if (hits.length > 0 && hits.length <= 4)
    console.log(`wildcard[${i}] (was ${toks[i]}): ${hits.length} hits @ ${hits.map(h => '0x' + h.toString(16)).join(', ')}`);
}
