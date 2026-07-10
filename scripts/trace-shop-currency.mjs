import { execSync } from 'node:child_process';
const SQL = 'tools\\sqlite\\sqlite3.exe';
const V = 'extracted/tables-2.0-partial.sqlite';   // converted vanilla tables
const T = 'extracted/trade_work/t.sqlite';          // vanilla trade
const q = (db, s) => execSync(`${SQL} ${db} "${s.replace(/"/g, '""')}"`).toString().trim();

// recipes that charge rupies
const coinKeys = q(V, 'SELECT Key FROM item_material_list WHERE CoinCost>0;').split(/\r?\n/).filter(Boolean);
const tiers = coinKeys.length ? q(V, `SELECT Key FROM item_tier_map WHERE MaterialId1 IN (${coinKeys.join(',')});`).split(/\r?\n/).filter(Boolean) : [];
console.log(`rupie-cost recipes: ${coinKeys.length} | tier maps pointing at them: ${tiers.length}`);
if (tiers.length) {
  const inlist = tiers.map(t => `'${t}'`).join(',');
  const rows = q(T, `SELECT Key||': '||COUNT(*) FROM trade WHERE ItemTierMapId IN (${inlist}) GROUP BY Key;`);
  console.log('shops (trade.Key) that use rupie recipes:\n' + (rows || '  (NONE — rupie recipes are not sold in any shop; rupies are spent elsewhere e.g. upgrades)'));
}

// how are genuinely repeatable badge-shop items configured? (the vanilla gold-for-silver rows)
console.log('\nvanilla gold-badge-for-silver rows (repeatable) full stock config:');
console.log(q(T, `SELECT SubKey||' refr='||IsRefreshable||' maxStk='||MaxStockForRefresh||' amt='||MaxStockOrAmountRefreshed FROM trade WHERE ItemPurchasable='ITEM_14_0032';`));
