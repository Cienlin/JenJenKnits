## ADDED Requirements

### Requirement: 首頁品牌展示

系統 SHALL 提供首頁展示品牌識別與精選商品，作為訪客首次接觸的入口。

#### Scenario: 訪客瀏覽首頁

- **WHEN** 未登入訪客造訪 `/`
- **THEN** 系統顯示品牌名稱、簡短介紹、至多 6 件精選商品縮圖，且可點入商品詳細頁

#### Scenario: 沒有精選商品時

- **WHEN** 資料庫中無標記為「精選」的商品
- **THEN** 系統顯示品牌介紹並改列出最新上架的 6 件商品

### Requirement: 商品目錄瀏覽

系統 SHALL 提供商品目錄頁，讓訪客瀏覽全部上架商品，並支援依分類篩選與關鍵字搜尋。

#### Scenario: 瀏覽全部商品

- **WHEN** 訪客造訪 `/products`
- **THEN** 系統顯示所有 `IsPublished = true` 的商品，依上架時間新到舊排列，每頁 12 件

#### Scenario: 依分類篩選

- **WHEN** 訪客點選左側分類「圍巾」（或於 URL 帶 `?category=scarves`）
- **THEN** 系統只顯示該分類下的已上架商品

#### Scenario: 關鍵字搜尋

- **WHEN** 訪客於搜尋框輸入「毛線」並送出
- **THEN** 系統顯示名稱或描述包含「毛線」的已上架商品（不區分大小寫）

#### Scenario: 搜尋無結果

- **WHEN** 搜尋關鍵字無任何符合商品
- **THEN** 系統顯示「找不到符合的商品」訊息，並提供回到全部商品的連結

### Requirement: 商品詳細頁

系統 SHALL 為每件商品提供獨立詳細頁，展示完整資訊與多張圖片，並提供「加入詢問清單」動作。

#### Scenario: 檢視商品詳細

- **WHEN** 訪客點選任一商品或造訪 `/products/{slug}`
- **THEN** 系統顯示商品名稱、價格、分類、材質、尺寸、故事描述，以及至少 1 張主圖（若有多圖則以 gallery 形式呈現）

#### Scenario: 商品不存在或未上架

- **WHEN** 訪客造訪不存在的 slug 或 `IsPublished = false` 的商品
- **THEN** 系統回傳 HTTP 404 並顯示友善錯誤頁

#### Scenario: 加入詢問清單

- **WHEN** 訪客於商品詳細頁點選「加入詢問清單」
- **THEN** 系統將該商品加入使用者 session，並顯示成功提示；同一商品重複加入僅計一次

### Requirement: 詢問清單管理

系統 SHALL 提供詢問清單頁，讓訪客檢視、移除已加入商品，並產生用於 LINE 詢問的訊息內容。

#### Scenario: 檢視詢問清單

- **WHEN** 訪客造訪 `/inquiry`
- **THEN** 系統顯示 session 中所有已加入的商品，含縮圖、名稱、價格，並顯示總數

#### Scenario: 移除單一商品

- **WHEN** 訪客點選任一商品旁的「移除」按鈕
- **THEN** 系統自 session 移除該商品並更新頁面

#### Scenario: 清空詢問清單

- **WHEN** 訪客點選「全部清空」
- **THEN** 系統移除 session 中所有商品

#### Scenario: 空清單

- **WHEN** 訪客造訪 `/inquiry` 且 session 中無商品
- **THEN** 系統顯示「尚無商品」訊息，並提供回到商品目錄的連結

### Requirement: LINE 詢問下單導流

系統 SHALL 於詢問清單頁提供「以 LINE 詢問」按鈕，開啟 LINE 官方帳號並附帶預填詢問內容。

#### Scenario: 產生 LINE 詢問連結

- **WHEN** 訪客於詢問清單頁點選「以 LINE 詢問」且清單非空
- **THEN** 系統開啟新分頁，導向 `{LineOA:Url}` 設定值，並於 URL query 或 clipboard 附帶預填訊息（列出商品名稱、數量、店家可辨識的詢問代碼）

#### Scenario: 清單為空時禁用按鈕

- **WHEN** 詢問清單為空
- **THEN** 「以 LINE 詢問」按鈕應顯示為 disabled 狀態

### Requirement: 關於我頁面

系統 SHALL 提供「關於我 / 製作理念」頁，內容來自資料庫（可由管理員編輯）。

#### Scenario: 檢視關於我頁

- **WHEN** 訪客造訪 `/about`
- **THEN** 系統顯示資料庫中 `AboutContent` 的最新內容（含品牌故事、製作理念、聯絡資訊）

#### Scenario: 尚無關於內容

- **WHEN** 資料庫中無 `AboutContent` 資料
- **THEN** 系統顯示佔位文字「內容準備中」而非拋出錯誤

### Requirement: 行動裝置優先呈現

系統 SHALL 以行動裝置為主要目標裝置設計 UI，並支援桌機瀏覽。

#### Scenario: 手機檢視

- **WHEN** 訪客以 375px 寬度裝置瀏覽任一頁面
- **THEN** 內容不出現水平捲軸，圖片、文字、按鈕皆清晰可觸控（按鈕最小 44x44px）

#### Scenario: 桌機檢視

- **WHEN** 訪客以 1280px 寬度裝置瀏覽商品目錄
- **THEN** 商品以格狀排列（每列 3-4 件），版面不留過大空白

### Requirement: 商品 URL 使用 slug

系統 SHALL 使用人類可讀的 slug 作為商品 URL 識別，而非資料庫 ID。

#### Scenario: slug 由商品名稱產生

- **WHEN** 管理員新增名稱為「秋葉黃圍巾」的商品
- **THEN** 系統自動產生 URL-safe slug（如 `qiu-ye-huang-scarf` 或系統選定策略），且可於後台手動修改

#### Scenario: slug 唯一性

- **WHEN** 管理員嘗試建立與現有商品相同 slug 的新商品
- **THEN** 系統拒絕並提示 slug 已被使用
