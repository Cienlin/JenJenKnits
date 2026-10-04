## ADDED Requirements

### Requirement: 首頁品牌展示與精選商品

系統 SHALL 於首頁 `/` 展示品牌識別、簡介，以及依 `featured` 欄位排序的精選商品。

#### Scenario: 訪客瀏覽首頁（有精選商品）

- **WHEN** 訪客造訪 `/`
- **THEN** 系統顯示品牌 hero、品牌故事、購買方式說明，以及所有 `meta.json` 中 `featured` 為數字的商品（依 `featured` 值升冪排序，愈小愈前），每張商品卡可點入 `/products/{slug}` 詳細頁

#### Scenario: 精選商品不足時 fallback

- **WHEN** 資料夾中 `featured` 為數字的商品少於 3 件
- **THEN** 系統先列出精選商品（依 `featured` 升冪），再以其餘商品（依資料夾名稱升冪）填補，精選區合計最多 6 件

#### Scenario: 完全無商品時

- **WHEN** `wwwroot/products/` 中無任何有效商品
- **THEN** 系統仍正常顯示 hero 與品牌故事，精選區顯示「作品準備中」佔位文字

### Requirement: 商品列表頁

系統 SHALL 於 `/products` 頁面列出所有有效商品，並支援依 `category` query 過濾。

#### Scenario: 訪客瀏覽全部商品

- **WHEN** 訪客造訪 `/products`
- **THEN** 系統顯示所有 `wwwroot/products/` 下有效商品（含 `meta.json` 且有 `name`、`cover.*`）之 grid，並於頁上方列出分類 pills

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
- **THEN** 系統顯示 `cover.*` 為主圖、`01.*, 02.*, ...` 為 gallery（依檔名數字升冪），以及 `name`、`category`、`price`（含 NT$ 符號）、`material`、`dimensions`（有值者才顯示）、由 `story.md` 經 Markdig 渲染的 HTML，以及大型購買按鈕

#### Scenario: 商品不存在

- **WHEN** 訪客造訪 `/products/{slug}` 且 slug 無對應資料夾，或該資料夾未通過有效性檢查
- **THEN** 系統回傳 HTTP 404 並顯示友善錯誤頁

#### Scenario: 購買按鈕依可用管道自動切換

- **WHEN** 商品 `meta.json` 有有效的 `buyUrl`
- **THEN** 主按鈕為「前往賣貨便」，於新分頁開啟 `buyUrl`，`<a>` 帶 `target="_blank"` 與 `rel="noopener noreferrer"`
- **WHEN** 商品沒有 `buyUrl`，但 `appsettings.json` 的 `Site:InstagramUrl` 有值
- **THEN** 主按鈕為「私訊訂購／客製顏色」，連到 Instagram，並註明「賣貨便賣場準備中」
- **WHEN** 商品沒有 `buyUrl`，且未設定 `Site:InstagramUrl`
- **THEN** 顯示不可點的「即將開賣」，並註明「賣貨便賣場準備中」

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

#### Scenario: name 為唯一必填欄位

- **WHEN** `meta.json` 中 `name` 為空字串、`null` 或欄位缺失
- **THEN** 系統跳過該商品 + log warning

#### Scenario: 選填欄位缺失

- **WHEN** `category`、`price`、`material`、`dimensions`、`shortDescription`、`buyUrl` 任一為空字串、`null` 或缺失（`price` 為 `0` 亦視為缺失）
- **THEN** 商品照常顯示，頁面上省略該欄位（不顯示空白或 TODO 字樣）；沒有 `buyUrl` 時購買按鈕依「購買按鈕依可用管道自動切換」處理

#### Scenario: buyUrl 不是有效網址

- **WHEN** `buyUrl` 有值但不是 `http://` 或 `https://` 開頭的絕對網址
- **THEN** 系統視同沒有 `buyUrl`，並 log warning

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

#### Scenario: 色卡圖

- **WHEN** 商品資料夾含 `colors.jpg` / `colors.jpeg` / `colors.png` / `colors.webp` 之一
- **THEN** 詳細頁於故事之後顯示「可選顏色」區塊與該色卡圖；色卡不列入 gallery

### Requirement: 配色模擬器

系統 SHALL 讓 `meta.json` 設定 `colorSimulator` 的商品，在詳細頁「可選顏色」區塊提供配色預覽。

#### Scenario: 選擇主色與配色

- **WHEN** 商品 `colorSimulator` 為 `"puff-flower"`，訪客在模擬器中點選主色與配色色點
- **THEN** 小花預覽圖即時換成所選顏色（主色 = 外圈，配色 = 花心與提把），並顯示兩個色號；頁面註明螢幕顏色以色卡照片為準

#### Scenario: 未設定模擬器

- **WHEN** 商品沒有 `colorSimulator`
- **THEN** 不顯示模擬器；若有 `colors.*` 仍顯示色卡圖

### Requirement: 商品照片瀏覽

系統 SHALL 讓訪客在詳細頁以滑動或縮圖瀏覽所有商品照片。

#### Scenario: 手機滑動

- **WHEN** 訪客在詳細頁大圖上左右滑動
- **THEN** 大圖逐張切換，縮圖列標示目前那一張

#### Scenario: 點縮圖

- **WHEN** 訪客點選縮圖
- **THEN** 大圖捲動到該張照片，該縮圖標示為目前選取

### Requirement: 頁尾版本號

系統 SHALL 於每頁頁尾以不起眼的小字顯示版本號，供確認線上是否為最新版。

#### Scenario: 顯示版本

- **WHEN** 訪客瀏覽任一頁面
- **THEN** 頁尾顯示 `v{csproj Version} ({建置時 git commit 前 7 碼})`，例如 `v1.0.0 (3f2a1c9)`；無法取得 commit 時只顯示 `v{Version}`

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
