## ADDED Requirements

### Requirement: 首頁品牌展示與精選商品

系統 SHALL 於首頁 `/` 展示品牌識別、簡介，以及依 `featured` 欄位排序的精選商品。

#### Scenario: 訪客瀏覽首頁（有精選商品）

- **WHEN** 訪客造訪 `/`
- **THEN** 系統顯示品牌 hero、品牌故事、購買方式說明，以及所有 `meta.json` 中 `featured` 為數字的商品（依 `featured` 值升冪排序，愈小愈前），每張商品卡可點入 `/products/{slug}` 詳細頁

#### Scenario: 精選商品不足時 fallback

- **WHEN** 資料夾中 `featured` 為數字的商品少於 3 件
- **THEN** 系統以最新 6 件商品（依資料夾修改時間降冪或名稱升冪）填補精選區

#### Scenario: 完全無商品時

- **WHEN** `wwwroot/products/` 中無任何有效商品
- **THEN** 系統仍正常顯示 hero 與品牌故事，精選區顯示「作品準備中」佔位文字

### Requirement: 商品列表頁

系統 SHALL 於 `/products` 頁面列出所有有效商品，並支援依 `category` query 過濾。

#### Scenario: 訪客瀏覽全部商品

- **WHEN** 訪客造訪 `/products`
- **THEN** 系統顯示所有 `wwwroot/products/` 下有效商品（含 `meta.json`、`cover.*`、非空 `buyUrl`）之 grid，並於頁上方列出分類 pills

#### Scenario: 依分類過濾

- **WHEN** 訪客點選分類 pill「純羊毛」或造訪 `/products?category=純羊毛`
- **THEN** 系統只顯示 `meta.json` 中 `category == "純羊毛"` 的商品，該 pill 呈 active 狀態

#### Scenario: 過濾無結果

- **WHEN** 訪客造訪 `/products?category=xxx` 且該 category 無任何商品
- **THEN** 系統顯示「此分類目前無商品」訊息，並提供「回全部商品」連結

### Requirement: 商品詳細頁

系統 SHALL 為每件有效商品提供獨立詳細頁 `/products/{slug}`，展示完整資訊 + 圖片 gallery + 「前往賣貨便」CTA。

#### Scenario: 檢視商品詳細

- **WHEN** 訪客造訪 `/products/{slug}` 且 slug 對應資料夾為有效商品
- **THEN** 系統顯示 `cover.*` 為主圖、`01.*, 02.*, ...` 為 gallery（依檔名字元升冪），以及 `name`、`category`、`price`（含 NT$ 符號）、`material`、`dimensions`、由 `story.md` 經 Markdig 渲染的 HTML，以及大型「前往賣貨便」按鈕

#### Scenario: 商品不存在

- **WHEN** 訪客造訪 `/products/{slug}` 且 slug 無對應資料夾，或該資料夾未通過有效性檢查
- **THEN** 系統回傳 HTTP 404 並顯示友善錯誤頁

#### Scenario: 「前往賣貨便」按鈕行為

- **WHEN** 訪客點選詳細頁的「前往賣貨便」按鈕
- **THEN** 系統於新分頁開啟該商品 `meta.json` 中的 `buyUrl`，`<a>` 帶 `target="_blank"` 與 `rel="noopener noreferrer"`

#### Scenario: 缺少 story.md 的商品

- **WHEN** 商品資料夾中無 `story.md`
- **THEN** 詳細頁正常顯示其他資訊，但省略故事區塊（不顯示空白或錯誤）

### Requirement: 檔案式商品資料掃描

系統 SHALL 以 `wwwroot/products/` 檔案系統為商品資料的單一 source of truth，不依賴任何資料庫。

#### Scenario: 掃描時機

- **WHEN** 應用程式於 production 啟動 或 每次 request 於 development
- **THEN** 系統掃描 `wwwroot/products/` 下所有子資料夾，讀取 `meta.json` + `story.md` + 圖檔清單，建立商品清單

#### Scenario: 資料夾名即為 slug

- **WHEN** 商品資料夾名為 `mustard-tote`
- **THEN** 該商品於系統中的 slug 為 `mustard-tote`，可透過 `/products/mustard-tote` 存取

#### Scenario: meta.json 缺失或 JSON 格式錯誤

- **WHEN** 某商品資料夾中 `meta.json` 不存在，或 JSON 解析失敗
- **THEN** 系統跳過該商品（不出現於任何頁面），並於 log 記錄 warning 含資料夾名與錯誤原因；其他商品不受影響

#### Scenario: cover 圖缺失

- **WHEN** 某商品資料夾中無 `cover.jpg`, `cover.jpeg`, `cover.png` 或 `cover.webp`
- **THEN** 系統跳過該商品 + log warning

#### Scenario: buyUrl 為必要欄位

- **WHEN** `meta.json` 中 `buyUrl` 為空字串、`null` 或欄位缺失
- **THEN** 系統將該商品視為未上架（不顯示於首頁、列表、詳細頁），並 log warning

#### Scenario: 下劃線前綴視為草稿（可選）

- **WHEN** 商品資料夾名以底線 `_` 開頭（例如 `_wip-hat`）
- **THEN** 系統忽略該資料夾，不出現於任何頁面（提供給使用者的草稿 escape hatch）

### Requirement: 圖片以檔名慣例識別

系統 SHALL 依商品資料夾中的圖檔命名慣例決定 cover 與 gallery 順序，不使用 `meta.json` 內嵌 image array。

#### Scenario: Cover 圖識別

- **WHEN** 商品資料夾含 `cover.jpg` / `cover.jpeg` / `cover.png` / `cover.webp` 之一
- **THEN** 系統將其作為首頁卡片縮圖與詳細頁主圖

#### Scenario: Gallery 排序

- **WHEN** 商品資料夾含 `01.jpg`, `02.jpg`, `03.jpg` 或對應副檔名的檔案
- **THEN** 詳細頁 gallery 依檔名字元升冪排序顯示

#### Scenario: 支援的圖片格式

- **WHEN** 商品資料夾中圖檔副檔名為 `.jpg`, `.jpeg`, `.png`, `.webp`
- **THEN** 系統識別並提供 serve；其他副檔名（如 `.gif`, `.bmp`, `.heic`）被忽略

### Requirement: 精選排序與 featured 欄位

系統 SHALL 使用 `meta.json` 的 `featured` 欄位決定商品是否於首頁精選區出現及其順序。

#### Scenario: featured 為數字

- **WHEN** 商品 `meta.json` 中 `featured` 為整數（例如 `1`, `2`, `10`）
- **THEN** 該商品出現於首頁精選區，依 `featured` 值升冪排序（愈小愈前）

#### Scenario: featured 為 null 或省略

- **WHEN** 商品 `meta.json` 中 `featured` 為 `null` 或欄位不存在
- **THEN** 該商品不出現於首頁精選區，但仍出現於 `/products` 全部商品列表（若 `buyUrl` 有效）

### Requirement: 商品故事以 Markdown 渲染

系統 SHALL 讀取商品資料夾中的 `story.md`（若存在），以 Markdig 渲染為 HTML 供詳細頁使用。

#### Scenario: story.md 存在

- **WHEN** 商品資料夾中含 `story.md`
- **THEN** 系統以 Markdig 將其內容轉為安全的 HTML（可支援段落、標題、粗體、斜體、清單、連結），顯示於詳細頁的故事區塊

#### Scenario: story.md 不存在

- **WHEN** 商品資料夾中無 `story.md`
- **THEN** 系統仍將該商品視為有效（其他欄位齊全），僅詳細頁不顯示故事區塊

### Requirement: 行動裝置優先呈現

系統 SHALL 以行動裝置為主要目標裝置設計 UI，並支援桌機瀏覽。

#### Scenario: 手機瀏覽

- **WHEN** 訪客以 375px 寬度裝置瀏覽任一頁面（`/`, `/products`, `/products/{slug}`）
- **THEN** 內容不出現水平捲軸，圖片、文字、按鈕清晰可觸控（按鈕最小 44×44px，內文行高 ≥ 1.5）

#### Scenario: 商品列表桌機排版

- **WHEN** 訪客以 1280px 寬度裝置瀏覽 `/products`
- **THEN** 商品以 3-4 欄格狀排列，版面不留過大空白

#### Scenario: 商品詳細頁桌機排版

- **WHEN** 訪客以 1280px 寬度裝置瀏覽 `/products/{slug}`
- **THEN** 圖片區位於左側（含 gallery），資訊區位於右側（含 story）
