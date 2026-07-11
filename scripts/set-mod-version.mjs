// Stamp a mod's ModVersion in BOTH the repo copy and the installed Reloaded-II
// copy, so the version shown in the mod loader always reflects what's running.
//
// Usage:  node scripts/set-mod-version.mjs <ModId> <version>
//   e.g.  node scripts/set-mod-version.mjs gbfr.transmarvel.overhaul 2.0-dev3
//
// Convention (see CLAUDE.md "Releases"):
//   - released state:  ModVersion == the release tag version ("1.0" for <mod>-v1)
//   - between releases: next version + "-devN", bumped on EVERY behavior change
//     installed into Reloaded (e.g. "2.0-dev1", "2.0-dev2", ... -> release "2.0")
// Reloaded-II displays the string as-is; we ship no update metadata, so
// prerelease suffixes are safe.
import { readFileSync, writeFileSync, readdirSync, existsSync } from 'node:fs';

const [modId, version] = process.argv.slice(2);
if (!modId || !version) {
  console.error('usage: node scripts/set-mod-version.mjs <ModId> <version>');
  process.exit(1);
}

const targets = [];
for (const dir of readdirSync('mods')) {
  const p = `mods/${dir}/${modId}/ModConfig.json`;
  if (existsSync(p)) targets.push(p);
}
const installed = `C:/Reloaded-II/Mods/${modId}/ModConfig.json`;
if (existsSync(installed)) targets.push(installed);
// mods-src copy (source of truth for C# mods' build output)
const src = `mods-src/${modId}/ModConfig.json`;
if (existsSync(src)) targets.push(src);

if (!targets.length) {
  console.error(`no ModConfig.json found for ${modId} (run from the repo root C:\\dev\\GBF)`);
  process.exit(1);
}
let failed = 0;
for (const p of targets) {
  try {
    const cfg = JSON.parse(readFileSync(p, 'utf8')); // validates while reading
    const old = cfg.ModVersion;
    cfg.ModVersion = version;
    writeFileSync(p, JSON.stringify(cfg, null, 2) + '\n');
    JSON.parse(readFileSync(p, 'utf8')); // paranoia: Reloaded silently skips bad JSON
    console.log(`${p}: ${old} -> ${version}`);
  } catch (e) {
    failed++;
    console.error(`FAILED ${p}: ${e.code || e.message} (game/launcher holding it open? close it and re-run)`);
  }
}
if (failed) process.exit(1);
console.log('NOTE: Reloaded-II reads ModConfig at launcher refresh; the new version shows next time the mod list reloads.');
