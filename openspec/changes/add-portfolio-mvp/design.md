## Context

此為 JenJenKnits 專案第一個實質功能 change。既有專案骨架已透過 `dotnet new webapp --auth Individual` 建立，含 ASP.NET Core Identity（`IdentityUser` + `ApplicationDbContext` + SQLite）與 Razor Pages 預設模板（Bootstrap + jQuery）。

使用者背景：
- Junior 開發者，日常寫 ASP.NET ZERO，此為第一個 Razor Pages 專案
- 家人（貞貞）為單一利害關係人 + 唯一內容維護者；使用者本人代為上架商品
- 家人尚無銷售紀錄，MVP 上線後預期低流量（親友試單為主）

技術棧限制：
- .NET 9（.NET 8 未安裝，.NET 10 為 preview）
- 本機部署為主，Phase 2 才考慮雲端
- 無外部付費相依（LLM、金流、CDN、雲端儲存皆延後）

## Goals / Non-Goals

**Goals:**

- 快速交付一個可讓貞貞立即使用的品牌門面（4-6 週內上線）
- 建立可延伸的 domain model，未來加入購物車/金流/會員時不需重寫
- 學習價值：使用者能從實作中理解 Razor Pages、EF Core、Identity 的協作關係
- 內容管理極簡：貞貞不需登入或學習後台，僅使用者代為上架

**Non-Goals:**

- 不做多店家 / 多租戶 —— 單一品牌單一 admin
- 不做客製化主題系統 —— 樣式改動皆走 code
- 不做 SEO 深度優化 —— 僅基本 meta tags；主要客源將來自 IG 而非搜尋引擎
- 不做圖片 CDN / 動態縮圖服務 —— MVP 直接 serve 原圖
- 不做 i18n —— 僅繁體中文
- 不做多幣別 / 稅務 / 發票 —— 一律 NT$，交易在 LINE 完成

## Decisions

### D1. Domain Model 結構

採用 4 個核心 entity：

```
Product         Category         ProductImage         AboutContent
  ├─ Id           ├─ Id             ├─ Id                ├─ Id
  ├─ Name         ├─ Name           ├─ ProductId (FK)    ├─ BrandStory
  ├─ Slug (uniq)  ├─ Slug (uniq)    ├─ FilePath          ├─ Philosophy
  ├─ Description ─┘  └─ DisplayOrder ├─ AltText          ├─ ContactInfo
  ├─ Material                       ├─ DisplayOrder      └─ UpdatedAt
  ├─ Dimensions                     └─ UploadedAt
  ├─ Price (decimal)
  ├─ CategoryId (FK)
  ├─ IsPublished
  ├─ IsFeatured
  ├─ CreatedAt
  └─ UpdatedAt
```

**Rationale:**
- `AboutContent` 為單一 row 而非多筆版本 —— MVP 不需要歷史。日後若要版本控管，加 `Version` 欄位即可
- `Price` 用 `decimal(10,2)`，符合台幣單位習慣（避免 float 精度問題）
- `ProductImage` 拆表而非 JSON 欄位 —— 為了 `DisplayOrder` 可用 SQL 排序、單張刪除方便

**Alternative considered:** 用 `JsonContent` 欄位存 `AboutContent` 或 `ProductImages`。拒絕，因為 SQLite 對 JSON 查詢弱，且未來加欄位需 migration；直接建表更長期友善。

### D2. Slug 產生策略

商品 slug 使用 **「後台可編輯 + 前台唯一鍵」** 混合模式：

- 建立商品時，系統預設從名稱推導 slug（英數字保留、空格轉 `-`、其他字元移除）
- 若推導結果為空（純中文名），系統以 `product-{shortGuid}` 為預設
- Admin 可於後台手動改 slug（帶唯一性驗證）

**Rationale:** 中文自動轉 pinyin 需引入 `Lucene.Net` 或 `NPinyin` 等 package，且拼音對客人不直觀。手動 slug 給 admin 決定「seasonal-scarf-2026」這種語義化 URL 才有價值。

**Alternative considered:** 純數字 ID URL（`/products/42`）。拒絕，因為分享 / SEO / 記憶度都輸給 slug。

### D3. 詢問清單使用 Session Cookie

詢問清單狀態存於 ASP.NET Core Session（Distributed memory cache）：

- 用 `ISession` 儲存 `List<int>`（product IDs）
- Session lifetime：滑動 30 天（Cookie 到期日）
- 不需登入即可使用

**Rationale:** 詢問清單本質是「暫存意向」，不是購物車，不需持久化。使用者關瀏覽器後保留 30 天即可，跨裝置同步屬於過度設計。

**Alternative considered:** LocalStorage + 純 JS。拒絕，因為 server-side session 允許之後加「訪客留言」等功能不需重寫。

### D4. LINE 導單機制

詢問清單頁點「以 LINE 詢問」時：
1. 系統產生訊息文字（列商品名、單價、代碼）
2. 開新分頁導向 `{LineOA:Url}` 官方帳號
3. 訊息內容**複製到 clipboard**（因 LINE URL 無 pre-fill message 標準機制）
4. 提示訪客「詢問內容已複製，請於 LINE 貼上」

**Rationale:** LINE Messaging API 的 `line://` scheme 不支援跨平台預填訊息（僅部分手機瀏覽器可用 `?text=`）。Clipboard fallback 是最普遍可用方案。

**Alternative considered:** 用 QR code + LINE Bot webhook 自動接單。拒絕，此為 Phase 2 / 全新 change 範疇。

### D5. 圖片儲存：本機 wwwroot

圖檔存於 `wwwroot/uploads/{yyyy}/{mm}/{guid}.{ext}`：

- 依日期分資料夾避免單資料夾檔案過多
- 檔名用 `Guid.NewGuid().ToString("N")` 避免衝突 / 猜測
- 副檔名保留原檔（僅接受 `.jpg / .jpeg / .png / .webp`）
- 上傳時**不做自動壓縮 / 縮圖**（MVP 階段，貞貞上傳前手機自帶壓縮已足夠）

**Rationale:** 本機路徑最單純，不需 Azure Blob / S3 帳號。Phase 2 若上雲端可直接抽 `IImageStorage` interface 替換。

**Risk 標註於下方。**

### D6. 初始 Admin Seed

於 `Program.cs` 啟動時執行 `SeedAdminAsync()`：

- 讀取 `appsettings.json` 的 `Seed:Admin:Email` / `Seed:Admin:Password`
- 若 Admin role 不存在則建立
- 若無任何 Admin role 使用者則建立指定帳號

**Rationale:** 避免「migration 完成但沒任何人能登入後台」的死鎖。使用者可於第一次登入後立即改密碼。

**Security note:** `appsettings.json` 的預設密碼**不可 commit 到 repo**。使用 `dotnet user-secrets` 存放本機開發密碼，正式部署用環境變數。

### D7. 分頁與搜尋

- 分頁：Razor Pages 內建 IQueryable + `.Skip(N).Take(12)`，query string 帶 `?page=2`
- 搜尋：`LIKE %keyword%`（SQLite），不引入 FTS5 —— MVP 商品數 < 100，效能無虞

**Alternative considered:** SQLite FTS5 全文搜尋。拒絕，因為商品少 + 中文分詞複雜，過度設計。

## Risks / Trade-offs

**[R1] 本機圖片儲存 = 單點失敗**
→ Mitigation：MVP 階段每週由使用者手動 backup `wwwroot/uploads/` + `*.db` 到雲端硬碟。Phase 2 遷移至 Azure Blob 或類似服務。

**[R2] Session 遺失 = 詢問清單消失**
→ Mitigation：接受此風險，詢問清單非交易紀錄。UI 提示「加入清單暫存 30 天，建議儘快透過 LINE 詢問完成訂購」。

**[R3] 中文 slug 手動維護負擔**
→ Mitigation：系統提供自動建議（去除中文後的 hash），admin 可覆寫。若貞貞商品量大幅增加（未來 100+）再考慮 pinyin lib。

**[R4] SQLite 單檔資料庫不支援併發寫入**
→ Mitigation：MVP 流量預期極低（admin 一人上架、訪客只讀）。Phase 2 若切商業運作，遷移 SQL Server / PostgreSQL（EF Core provider 更換即可）。

**[R5] 沒有自動圖片壓縮 = 使用者上傳大檔會拖慢頁面**
→ Mitigation：前端限制上傳 ≤ 5MB。若實際成問題，Phase 1.5 加入 `ImageSharp` 產生縮圖。

**[R6] `IsPublished` 沒有 audit log**
→ Mitigation：MVP 接受此權衡。單一 admin 環境，操作紀錄需求低。

## Migration Plan

此為 greenfield MVP，無資料需遷移。

**Deploy 流程（Phase 1，本機開發）：**
1. `dotnet ef database update` —— 建立 schema
2. 啟動時 `SeedAdminAsync()` 自動建立初始 admin
3. Admin 登入後上架商品、上傳圖片、編輯關於我
4. 本機 `dotnet run` 即可對外提供服務（`localhost` + LAN IP）

**Rollback：** 因為是新專案，rollback = 停止服務、還原 `app.db` backup 檔案。

## Open Questions

1. **關於我內容格式**：純 markdown 還是 rich text editor？（傾向 markdown，因為 admin 是使用者本人，工程師更習慣）
2. **首頁「精選商品」條件**：`IsFeatured = true` 手動標記 vs. 「最近 30 天上架」自動篩選？（傾向手動，避免時間邏輯 corner case）
3. **後台是否需要「草稿 / 排程上架」功能**？（MVP 傾向不做，僅 `IsPublished` 二值）
4. **是否需要收集匿名瀏覽統計**（GA / Umami）？（Phase 2 討論，隱私與需求平衡）
5. **CSS 框架選擇**：沿用模板的 Bootstrap 還是換 Tailwind？—— 這是**下一個 change 才要決定**的，MVP 骨架先用 Bootstrap 讓功能先跑起來
