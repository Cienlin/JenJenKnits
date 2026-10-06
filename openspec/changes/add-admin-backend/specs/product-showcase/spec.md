## REMOVED Requirements

### Requirement: 檔案式商品資料掃描

**Reason**: 商品文字資料改存資料庫並由後台編輯；照片仍依資料夾規則（見「圖片以檔名慣例識別」）。
**Migration**: 啟動時若資料庫沒有任何商品，系統自動從 `wwwroot/products/{slug}/` 的 `meta.json` 與 `story.md` 匯入，匯入完成後刪除這兩種檔案；之後一律在後台編輯。原「資料夾名稱以底線開頭視為草稿」改為後台的草稿狀態（匯入時忽略底線開頭的資料夾）。

## ADDED Requirements

### Requirement: 商品資料來源

系統 SHALL 以資料庫為商品文字資料的唯一來源，照片則依商品 slug 對應的資料夾取得；本規格其他需求中提到的 `meta.json` 欄位（`name`、`category`、`price`、`material`、`dimensions`、`shortDescription`、`buyUrl`、`featured`、`colorSimulator`），指資料庫中該商品對應的欄位。

#### Scenario: 前台只顯示已發布且有封面的商品

- **WHEN** 訪客瀏覽首頁、作品列表、分類或商品詳細頁
- **THEN** 系統只顯示資料庫中標為已發布、且 `wwwroot/products/{slug}/` 有 `cover.*` 的商品；其餘商品不出現，詳細頁回傳 404

#### Scenario: 缺少封面照片

- **WHEN** 已發布的商品在資料夾中找不到 `cover.*`
- **THEN** 該商品不出現在前台，並於 log 記錄 warning；後台列表標示「缺封面」

#### Scenario: 選填欄位缺失

- **WHEN** 商品的 `category`、`price`、`material`、`dimensions`、`shortDescription`、`buyUrl` 任一未填寫
- **THEN** 商品照常顯示，頁面上省略該欄位（不顯示空白或 TODO 字樣）；沒有 `buyUrl` 時購買按鈕依「購買按鈕依可用管道自動切換」處理

#### Scenario: 首次啟動匯入既有商品

- **WHEN** 應用程式啟動時資料庫中沒有任何商品，且 `wwwroot/products/` 下有含 `meta.json` 的資料夾（不含底線開頭者）
- **THEN** 系統把每個資料夾的 `meta.json` 欄位與 `story.md` 內容匯入為已發布商品（slug = 資料夾名稱），`meta.json` 格式錯誤或缺 `name` 的資料夾略過並記錄 warning；匯入後刪除已匯入資料夾中的 `meta.json` 與 `story.md`

#### Scenario: 資料庫已有商品

- **WHEN** 應用程式啟動時資料庫中已有商品
- **THEN** 系統不執行匯入，也不讀取任何 `meta.json`

## MODIFIED Requirements

### Requirement: 商品故事以 Markdown 渲染

系統 SHALL 讀取商品的故事 Markdown（若有填寫），以 Markdig 渲染為 HTML 供詳細頁使用。

#### Scenario: 故事已填寫

- **WHEN** 商品在資料庫中有故事內容
- **THEN** 系統以 Markdig 將其內容轉為安全的 HTML（可支援段落、標題、粗體、斜體、清單、連結），顯示於詳細頁的故事區塊

#### Scenario: 故事未填寫

- **WHEN** 商品的故事為空白或只有 HTML 註解
- **THEN** 系統仍將該商品視為有效（其他欄位齊全），僅詳細頁不顯示故事區塊
