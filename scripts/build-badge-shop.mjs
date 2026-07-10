// Build the "Badge & Spellbook Shop" mod: add Silver Dalia Badge, Gold Dalia Badge, and
// Gold Spellbook as rupie-priced purchases at the Knickknack Shack.
//
// The shop cost chain is: trade.tbl (ItemPurchasable, ItemTierMapId) -> item_tier_map
// (MaterialId1 = a recipe key) -> item_material_list (the cost recipe: item materials +
// CoinCost rupies). We add, per item: one pure-rupie recipe (CoinCost only), one tier-map
// row pointing at it, and one trade row selling the item for that cost.
//
// Reads extracted/shop_work/s.sqlite (trade + item_tier_map + item_material_list),
// edits in place, exports to extracted/shop_work/out. Verify then ship the 3 .tbl files.
import { execSync } from 'node:child_process';

const SQL = 'tools\\sqlite\\sqlite3.exe';
const DB = 'extracted/shop_work/s.sqlite';
const q = (sql) => execSync(`${SQL} ${DB} "${sql.replace(/\s+/g, ' ').replace(/"/g, '""')}"`).toString().trim();

// ===================== CONFIG (default prices, in rupies) =====================
const ITEMS = [
  { name: 'Silver Dalia Badge', item: 'ITEM_14_0031', price: 5000,   recipeKey: 990001, tierId: 'TMAP_RUPIE_SILVER_BADGE' },
  { name: 'Gold Dalia Badge',   item: 'ITEM_14_0032', price: 50000,  recipeKey: 990002, tierId: 'TMAP_RUPIE_GOLD_BADGE' },
  { name: 'Gold Spellbook',     item: 'ITEM_11_0002', price: 100000, recipeKey: 990003, tierId: 'TMAP_RUPIE_GOLD_SPELLBOOK' },
];
const SHOP_CATEGORY = 5;   // Key=5 = the badge shop (where the Gold Badge already sells)
// =============================================================================

// column lists (from the 2.0 headers)
const IML_COLS = q(`PRAGMA table_info(item_material_list);`).split('\n').map(l => l.split('|')[1]);
const ITM_COLS = q(`PRAGMA table_info(item_tier_map);`).split('\n').map(l => l.split('|')[1]);
const TRD_COLS = q(`PRAGMA table_info(trade);`).split('\n').map(l => l.split('|')[1]);

// a real Key=5 trade row to clone the non-essential fields from
const goldRow = q(`SELECT ${TRD_COLS.join(',')} FROM trade WHERE ItemPurchasable='ITEM_14_0032' AND Key=5 LIMIT 1;`).split('|');
const goldObj = Object.fromEntries(TRD_COLS.map((c, i) => [c, goldRow[i]]));

let sortOrder = 200;
for (const it of ITEMS) {
  // 1) cost recipe: pure rupies, no item materials
  const iml = Object.fromEntries(IML_COLS.map(c => [c, 0]));
  for (const c of IML_COLS) if (c.startsWith('Item') && !c.startsWith('ItemCount') && !c.startsWith('ItemMaterialCommon') || c.startsWith('ItemMaterialCommon') && !c.includes('Count')) iml[c] = '';
  iml.Key = it.recipeKey;
  iml.CoinCost = it.price;
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
  trd.Key = SHOP_CATEGORY;
  trd.MinQuestId = '00000000';   // always available (once the shop category is unlocked)
  trd.MaxQuestId = '00000000';
  trd.SortOrder = (sortOrder++).toString(16).toUpperCase().padStart(8, '0');   // hex_uint
  q(`INSERT INTO trade (${TRD_COLS.join(',')}) VALUES (${TRD_COLS.map(c => `'${String(trd[c]).replace(/'/g, "''")}'`).join(',')});`);

  console.log(`+ ${it.name}: ${it.price} rupies (recipe ${it.recipeKey}, tier ${it.tierId})`);
}

console.log('\nVerify counts:');
console.log('  item_material_list new:', q(`SELECT COUNT(*) FROM item_material_list WHERE Key IN (${ITEMS.map(i => i.recipeKey).join(',')});`));
console.log('  item_tier_map new:', q(`SELECT COUNT(*) FROM item_tier_map WHERE Key IN (${ITEMS.map(i => `'${i.tierId}'`).join(',')});`));
console.log('  trade new:', q(`SELECT COUNT(*) FROM trade WHERE ItemTierMapId IN (${ITEMS.map(i => `'${i.tierId}'`).join(',')});`));
