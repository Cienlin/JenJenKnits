## Context

此為 `add-portfolio-mvp`（未實作即廢除）之後的 pivot MVP。既有專案骨架已透過 `dotnet new webapp --auth Individual` 建立 Razor Pages + Identity + EF Core + SQLite，但本 change 會**大幅拆除**這些 scaffold（詳見 D7）。

Landing page 視覺已於當前未 commit 的 diff 中大致完成（hero + 4 張硬編商品卡 + 品牌故事 + 購買方式區塊 + ~440 行自訂 CSS + reveal-on-scroll 動畫）。本 change 的實作接在這些視覺基礎之上。

**使用者背景**
- Junior 開發者，日常寫 ASP.NET ZERO / Core MVC，此為第一個 Razor Pages 專案
- 家人（貞貞）為唯一內容擁有者，但**不操作網站**；使用者代為上架（照著資料夾範例做 `mkdir` + 貼 `meta.json` + 丟圖 + `git push`）
- 家人尚無銷售紀錄，MVP 上線後預期低流量（親友 + IG/Threads 導流試單為主）

**技術棧限制**
- .NET 9
- 無資料庫、無 auth、無外部付費相依
- 本機開發為主，deployment 為獨立議題

## Goals / Non-Goals

**Goals**
- 快速交付一個貞貞可分享（印名片、發 IG、貼 Threads）的品牌門面（2-3 週內可上線）
- 上架流程對「使用者」極簡：`mkdir` + 3-5 個檔案 + `git push`
- 專案結構保持極簡（乾淨 Program.cs、無 dead code），將來若加東西也不會被過度設計綁死
- 學習價值：使用者理解「最小 Razor Pages 專案」長什麼樣，而非在 scaffold 樣板上疊功能

**Non-Goals**
- 不做 admin 後台（使用者代為維護）
- 不做 SQLite / DB（檔案為單一 source of truth）
- 不做 auth（無機敏資料）
- 不做即時抓賣貨便庫存（爬蟲脆弱且無官方 API）
- 不做搜尋（商品少，列表即足夠）
- 不做真正的靜態站產出（SSG）—— 保留 ASP.NET Core runtime，允許之後加互動
- 不做 i18n（僅繁體中文）
- 不做多店家 / 多幣別 / 發票

## Decisions

### D1. 商品資料以檔案為主

一資料夾一商品，資料夾名即為 slug：

```
wwwroot/products/
├─ scarf-earth-tone/
│  ├─ meta.json     ← 結構化欄位
│  ├─ story.md      ← Markdown 故事
│  ├─ cover.jpg     ← 首頁與詳細頁主圖
│  ├─ 01.jpg        ← Gallery，依檔名字元升冪
│  ├─ 02.jpg
│  └─ 03.jpg
└─ mustard-tote/
   └─ ...
```

**Rationale**
- metadata + 圖片綁在同一資料夾，永遠不會失聯
- 刪商品 = 刪整個資料夾，乾淨
- 圖片排序靠檔名，不需在 json 手寫 order 欄位
- 每個商品自己一份 json，多商品同時修改也無 merge conflict
- slug 由資料夾名決定，`meta.json` 不需 slug 欄位

**Alternatives considered**
- 單一 `products.json` + 圖集中放：可讀性差，商品多時 json 臃腫，圖與 metadata 分兩處易失聯
- 純 Markdown（含 frontmatter）：型別安全較差；MVP 尚未需要純內容導向工作流
- C# hardcode：失去「丟資料夾即上架」的手感，每次上架都要 rebuild

### D2. `meta.json` schema

| 欄位 | 型別 | 必填 | 說明 |
|------|------|:----:|------|
| `name` | string | ✅ | 商品名稱 |
| `category` | string | ✅ | 分類（自由字串，用於 pill 過濾） |
| `price` | number | ✅ | NT$ 整數 |
| `material` | string | ✅ | 材質描述 |
| `dimensions` | string | ✅ | 尺寸描述 |
| `shortDescription` | string | ✅ | 首頁卡片一句話 |
| `buyUrl` | string | ✅ | 賣貨便連結（無 = 不上架） |
| `featured` | number \| null | ⭕ | 首頁精選排序權重，愈小愈前，null/省略 = 不上精選 |

**Rationale**
- `buyUrl` 必填 = 交易通路單一，沒賣貨便就不上網站（避免訪客無所適從）
- `featured` 用 number 而非 boolean，兼容「顯示與否」與「排序」兩個需求
- 未加 `availability`（售完 / 訂製中）欄位：即時狀態以賣貨便為準，網站不同步（見 D5）
- 未加 `slug` / `images` / `createdAt` 欄位：分別由資料夾名 / 檔案掃描 / git log 提供

### D3. 圖片以檔名慣例識別

- `cover.jpg` / `cover.png` / `cover.jpeg` / `cover.webp` → 首頁縮圖與詳細頁主圖（必要）
- `01.jpg`, `02.jpg`, `03.jpg`, ... → 詳細頁 gallery，依檔名字元升冪排序
- 副檔名支援：`.jpg`, `.jpeg`, `.png`, `.webp`

**Rationale**
- 不需在 json 中列 image array（省一份維護，避免遺漏）
- 排序不需另外欄位，改順序 = 改檔名

### D4. 商品狀態不同步賣貨便

網站僅展示商品資訊，不顯示「有貨 / 售完 / 補貨中」等即時狀態。訪客點「前往賣貨便」CTA 至賣貨便頁面查看真實狀態。

**Rationale**
- 賣貨便無公開 API，只能爬 HTML，脆弱且違反直覺（HTML 結構隨時改）
- 手動同步 `availability` 欄位負擔重，且延遲反而讓網站顯示錯誤狀態，信任度更差
- 賣貨便自己的頁面才是 source of truth，導過去看最準

**Trade-off**：訪客要多點一次才知道有沒有貨。UI 可明示「即時狀態以賣貨便為準」，降低期待落差。

### D5. Story 用 Markdown（Markdig）

商品資料夾中的 `story.md` 於 request time 由 Markdig 渲染成 HTML，顯示在詳細頁。

**Rationale**
- 商品故事可能有段落 / 粗體 / 清單等排版需求，純文字表達力不足
- Markdig 是 .NET 生態最主流的 Markdown parser，穩定且輕量
- `story.md` 分離出來（不塞在 json 字串裡）避免跳脫符號困擾，也對 markdown editor 友善

**Alternative rejected**：純文字段落陣列 `["第一段", "第二段"]`。表達力不足，若之後想加粗體或連結還是要加 parser，不如一次用 markdown。

### D6. 拆除 Identity + EF Core + SQLite scaffold

移除項目：
- `Areas/Identity/`
- `Data/`（含 `ApplicationDbContext.cs` 與 Migrations）
- `app.db*`
- 5 個 NuGet packages（Identity.EFCore, Identity.UI, EFCore.Sqlite, EFCore.Tools, Diagnostics.EFCore）
- `appsettings.json` 中 `ConnectionStrings` section
- `Program.cs` 中相關註冊，簡化為 ~15 行

**Rationale**
- 純介紹站無 auth 需求 → Identity 是負債不是資產
- Dead code = attack surface（`/Identity/Account/Login` 公開頁被 bot 掃是常態） + 心智負擔
- .NET 9 minimal Razor Pages 專案結構清楚，有學習價值
- 未來若真的要加後台，`dotnet aspnet-codegenerator` 或 `dotnet new webapp --auth Individual` 到暫存資料夾複製 scaffold 都很快；且未來 admin 更可能走 headless CMS（Decap / Sanity）而非自幹 admin

### D7. 三路由結構

| Route | 內容 |
|-------|------|
| `/` | Hero + 精選商品 grid + 品牌故事 + 購買方式（賣貨便說明 + CTA） |
| `/products` | 所有商品 grid，支援 `?category=xxx` query 過濾，頁上方有分類 pills |
| `/products/{slug}` | Cover + gallery + metadata（name/category/price/material/dimensions） + story markdown 渲染 + 「前往賣貨便」大按鈕 |

**Rationale**
- 極簡但兼顧「看全部」需求
- 不需獨立 `/about`：首頁的品牌故事區塊即為關於我
- 未來若商品超過 30 件再考慮分頁（Non-goal for MVP）

### D8. 商品掃描時機

- **Development**：`AddScoped<IProductCatalog>`，每個 request 重掃資料夾（方便看修改）
- **Production**：`AddSingleton<IProductCatalog>` + application startup 掃一次快取，`dotnet run` restart 時重讀

**Rationale**
- 資料量小（預期 < 50 商品），in-memory 完全夠
- 不需 `FileSystemWatcher`（增加複雜度且 prod 部署後每次改動本來就 restart）

### D9. 錯誤處理策略：單品失敗不阻擋其他商品

掃描 `wwwroot/products/` 時：
- `meta.json` 缺失 / JSON parse 失敗 → 跳過該商品 + `ILogger.LogWarning`
- `cover.*` 缺失 → 跳過該商品 + warning
- `buyUrl` 為空 / null / 缺失 → 跳過該商品 + warning
- `story.md` 缺失 → 商品仍上架，詳細頁不顯示故事區塊（此為 optional）

**Rationale**：MVP 由使用者手動維護，容錯優於嚴格；避免一個資料夾打錯字讓整個網站掛掉。

## Risks / Trade-offs

**[R1] 上架流程需要 `git push` → 貞貞無法自主上架**
→ Mitigation：已於 goals 對齊「使用者代為上架」。未來若貞貞想自主，走 headless CMS（Decap 直接編 GitHub repo），不新開後台。

**[R2] 沒即時庫存 → 訪客點了才發現售完**
→ Mitigation：UI 明示「即時狀態以賣貨便為準」；賣貨便頁面即 source of truth。

**[R3] 圖片直接 serve、無 CDN、無動態縮圖**
→ Mitigation：deployment 階段前置 Cloudflare 免費 CDN；使用者上傳前手動壓縮。

**[R4] 檔案為 source of truth → 無 audit log**
→ Mitigation：`git log` 就是 audit log，實際上比 DB audit 更完整。

**[R5] 未來若需 server-side state（例如客製化詢問表單），現架構無 session/DB**
→ Mitigation：那時再加即可（Session middleware + optional DB），不為 hypothetical 需求預留。

**[R6] `wwwroot/products/` 直接對外可訪問 → 使用者可能不小心 commit 敏感檔（例如某個 note.txt）**
→ Mitigation：README 明確說明資料夾規範；`.gitignore` 加入 `wwwroot/products/**/_*`（下劃線前綴視為草稿）作為 escape hatch。

## Migration Plan

此為 greenfield（`add-portfolio-mvp` 未實作），無資料需遷移。

**實作順序建議**
1. 先執行 D6 scaffold 拆除，讓專案回到乾淨基底
2. 加 Markdig + 建 `IProductCatalog` 骨架 + 一份 dev 範例資料
3. 修改 `Index.cshtml` 讓現有 landing page 讀取真實商品
4. 加 `/products` 與 `/products/{slug}` 兩頁
5. Verification + README

**Rollback**：因為未實作任何生產部署，rollback = `git reset` 即可。

## Open Questions

1. `/products` 是否需分頁？（傾向 MVP 不做，商品數 < 30 都能一頁 grid）
2. 分類 pills 顯示順序：資料夾出現順序 vs. 手動於某設定檔指定？（傾向前者，簡單）
3. 商品詳細頁是否加 IG/Threads 分享按鈕？（傾向 phase 2，MVP 先聚焦「介紹 → 賣貨便」單一路徑）
4. 未來若加「其他銷售通路」（Pinkoi、蝦皮等），`buyUrl` 是否要改成 array？（現階段 single string 足夠，未來需要再遷移，schema 變更成本低）
5. 圖片 alt text：使用 `meta.json.shortDescription` 或另設 `imageAlts` 欄位？（傾向前者，可接受簡化）
