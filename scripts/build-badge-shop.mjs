// Build the "Badge & Spellbook Shop" mod: sell Silver/Gold Dalia Badge + Gold Spellbook in
// the Knickknack Shack's Treasure Trade tab, paid in Knickknack Vouchers, unlimited stock.
//
// The shop cost chain is: trade.tbl (ItemPurchasable, ItemTierMapId, unique SubKey,
// Key=shop tab) -> item_tier_map (MaterialId1 = a recipe key) -> item_material_list (the
// cost recipe: item materials; CoinCost=rupies is IGNORED by shops). Per item we add one
// recipe, one tier-map row, one trade row. The "SortOrder" column is really the STOCK cap
// (FFFFFFFF = unlimited). Shops don't honor rupies and free items cap at ~200.
//
// Reads extracted/shop_work/s.sqlite (trade + item_tier_map + item_material_list),
// edits in place, exports to extracted/shop_work/out. Verify then ship the 3 .tbl files.
import { execSync } from 'node:child_process';

const SQL = 'tools\\sqlite\\sqlite3.exe';
const DB = 'extracted/shop_work/s.sqlite';
const q = (sql) => execSync(`${SQL} ${DB} "${sql.replace(/\s+/g, ' ').replace(/"/g, '""')}"`).toString().trim();

// ===================== CONFIG =====================
// Sold in the TREASURE TRADE tab (Key=3, which trades item-materials), cost = Knickknack
// Vouchers (ITEM_21_0000). A real cost makes them unlimited (free items cap at ~200; no
// shop honors rupies). Adjust costCount to taste.
const ITEMS = [
  { name: 'Silver Dalia Badge', item: 'ITEM_14_0031', costItem: 'ITEM_21_0000', costCount: 1, recipeKey: 990001, tierId: 'TMAP_VOUCHER_SILVER_BADGE' },
  { name: 'Gold Dalia Badge',   item: 'ITEM_14_0032', costItem: 'ITEM_21_0000', costCount: 3, recipeKey: 990002, tierId: 'TMAP_VOUCHER_GOLD_BADGE' },
  { name: 'Gold Spellbook',     item: 'ITEM_11_0002', costItem: 'ITEM_21_0000', costCount: 5, recipeKey: 990003, tierId: 'TMAP_VOUCHER_GOLD_SPELLBOOK' },
];
const SHOP_CATEGORY = 3;   // Key=3 = the Treasure Trade tab (item-material costs)
const TEMPLATE_WHERE = `Key=3 AND ItemPurchasable!='' AND IsRefreshable=0`;  // clone a real Key=3 row
// =============================================================================

// column lists (from the 2.0 headers)
const IML_COLS = q(`PRAGMA table_info(item_material_list);`).split('\n').map(l => l.split('|')[1]);
const ITM_COLS = q(`PRAGMA table_info(item_tier_map);`).split('\n').map(l => l.split('|')[1]);
const TRD_COLS = q(`PRAGMA table_info(trade);`).split('\n').map(l => l.split('|')[1]);

// a real Key=5 trade row to clone the non-essential fields from
const tmplRow = q(`SELECT ${TRD_COLS.join(',')} FROM trade WHERE ${TEMPLATE_WHERE} LIMIT 1;`).split('|');
const goldObj = Object.fromEntries(TRD_COLS.map((c, i) => [c, tmplRow[i]]));

for (const it of ITEMS) {
  // 1) cost recipe: a trivial material cost (shops don't honor rupies; free items cap at ~200)
  const iml = Object.fromEntries(IML_COLS.map(c => [c, 0]));
  for (const c of IML_COLS) if (c.startsWith('Item') && !c.startsWith('ItemCount') && !c.startsWith('ItemMaterialCommon') || c.startsWith('ItemMaterialCommon') && !c.includes('Count')) iml[c] = '';
  iml.Key = it.recipeKey;
  iml.Item1 = it.costItem;
  iml.ItemCount1 = it.costCount;
  iml.CoinCost = 0;
  q(`INSERT INTO item_material_list (${IML_COLS.join(',')}) VALUES (${IML_COLS.map(c => typeof iml[c] === 'string' ? `'${iml[c]}'` : iml[c]).join(',')});`);

  // 2) tier map: MaterialId1 -> the recipe key
  const itm = Object.fromEntries(ITM_COLS.map(c => [c, 0]));
  itm.MaterialId1 = it.recipeKey;
  itm.Key = it.tierId;
  q(`INSERT INTO item_tier_map (${ITM_COLS.join(',')}) VALUES (${ITM_COLS.map(c => typeof itm[c] === 'string' ? `'${itm[c]}'` : itm[c]).join(',')});`);

  // 3) trade row: clone the working gold-badge row, override the essentials
  const trd = { ...goldObj };
  trd.ItemPurchasable = it.item;
  trd.ItemTierMapId = it.tierId;
  trd.SubKey = `${it.tierId}_SUBKEY`;   // MUST be unique per row (hash_string) — the shop keys entries by this
  trd.Key = SHOP_CATEGORY;
  // Stock config = vanilla unlimited-priced-item config (0/0/0). With a real cost, this is
  // unlimited (matches the 107 cat-5 badge-cost rows). IsRefreshable=1 hid them (featured shop).
  trd.IsRefreshable = 0;
  trd.MaxStockForRefresh = 0;
  trd.MaxStockOrAmountRefreshed = 0;
  trd.MinQuestId = '00100000';   // early quest id used by Key=3 items (proven-visible)
  trd.MaxQuestId = '00000000';   // no upper gate — always available
  // NOTE: the header calls this "SortOrder" but it's actually the STOCK / purchase cap.
  // Small values = limited stock (Alex saw exactly 200/201/202 = our old counter);
  // FFFFFFFF = unlimited (matches vanilla items that show no stock line).
  trd.SortOrder = 'FFFFFFFF';
  q(`INSERT INTO trade (${TRD_COLS.join(',')}) VALUES (${TRD_COLS.map(c => `'${String(trd[c]).replace(/'/g, "''")}'`).join(',')});`);

  console.log(`+ ${it.name}: ${it.costCount}x ${it.costItem}, unlimited (recipe ${it.recipeKey})`);
}

console.log('\nVerify counts:');
console.log('  item_material_list new:', q(`SELECT COUNT(*) FROM item_material_list WHERE Key IN (${ITEMS.map(i => i.recipeKey).join(',')});`));
console.log('  item_tier_map new:', q(`SELECT COUNT(*) FROM item_tier_map WHERE Key IN (${ITEMS.map(i => `'${i.tierId}'`).join(',')});`));
console.log('  trade new:', q(`SELECT COUNT(*) FROM trade WHERE ItemTierMapId IN (${ITEMS.map(i => `'${i.tierId}'`).join(',')});`));
