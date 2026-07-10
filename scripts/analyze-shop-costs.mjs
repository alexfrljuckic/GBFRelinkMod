// For every trade (shop) row, resolve its cost recipe and stock config, to find the
// pattern that distinguishes "unlimited" from "limited" shop items.
import { execSync } from 'node:child_process';
const SQL = 'tools\\sqlite\\sqlite3.exe';
const T = 'extracted/trade_work/t.sqlite';
const V = 'extracted/tables-2.0-partial.sqlite';
const q = (db, s) => execSync(`${SQL} ${db} "${s.replace(/"/g, '""')}"`).toString().trim();

// recipe key -> "cost description"
const recipes = new Map();
for (const line of q(V, `SELECT Key||'|'||Item1||'|'||ItemCount1||'|'||CoinCost FROM item_material_list;`).split(/\r?\n/)) {
  const [k, item1, cnt1, coin] = line.split('|');
  let cost = 'FREE';
  if (item1 && item1 !== '') cost = `${cnt1}x ${item1}`;
  else if (+coin > 0) cost = `${coin} rupies`;
  recipes.set(k, cost);
}
// tier map hash -> recipe key
const tier = new Map();
for (const line of q(V, `SELECT Key||'|'||MaterialId1 FROM item_tier_map;`).split(/\r?\n/)) {
  const [k, m1] = line.split('|'); tier.set(k, m1);
}

// every shop row: category, item, cost, stock config
const rows = q(T, `SELECT Key,ItemPurchasable,GemPurchasable,ItemTierMapId,IsRefreshable,MaxStockForRefresh,MaxStockOrAmountRefreshed FROM trade;`).split(/\r?\n/);
const buckets = {};
for (const line of rows) {
  const [cat, item, gem, tid, refr, maxstk, amt] = line.split('|');
  const recipeKey = tier.get(tid);
  const cost = recipeKey ? (recipes.get(recipeKey) ?? '(recipe missing)') : '(no tiermap)';
  const costType = cost === 'FREE' ? 'FREE' : cost.includes('rupies') ? 'RUPIES' : cost.includes('ITEM_14_003') ? 'BADGES' : 'MATERIALS';
  const key = `cat${cat} cost=${costType} stock=${refr}/${maxstk}/${amt}`;
  buckets[key] = (buckets[key] || 0) + 1;
}
console.log('shop rows by (category, cost-type, stock-config):');
for (const [k, n] of Object.entries(buckets).sort()) console.log(`  ${k}  ->  ${n} rows`);

// are there any FREE vanilla shop items? if so, their stock config is the answer
console.log('\nFREE shop items (cost recipe has no materials + no coin):');
let anyFree = false;
for (const line of rows) {
  const [cat, item, gem, tid, refr, maxstk, amt] = line.split('|');
  const recipeKey = tier.get(tid);
  if (recipeKey && recipes.get(recipeKey) === 'FREE') {
    anyFree = true;
    console.log(`  cat${cat} ${item || gem} stock=${refr}/${maxstk}/${amt}`);
  }
}
if (!anyFree) console.log('  (NONE — every vanilla shop item has a cost; free items are our injection only)');
