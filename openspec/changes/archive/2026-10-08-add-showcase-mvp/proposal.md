## Why

原本的 `add-portfolio-mvp`（已廢除）將網站定位為「電商基底 + LINE 導單 + admin 後台」，含 SQLite、Identity、Session 詢問清單、圖片上傳後台等基礎設施。經與家人（品牌經營者貞貞）重新對齊實際銷售路徑後，決定將網站定位大幅收窄：

- **交易通路**：7-11 賣貨便（外部平台，訪客點連結跳出去完成購買）
- **社群互動**：Instagram + Threads（不在此網站內處理）
- **網站本身**：純商品介紹（品牌門面 + 作品展示）

在此定位下，網站不涉及任何機敏資料，因此**不需要後台、認證、資料庫**。內容維護由使用者（工程師本人）代為上架，透過檔案系統 + git push 完成，貞貞不需登入或操作介面。

此 change 為新的 MVP 起點，取代 `add-portfolio-mvp`（後者未實作任何 task，直接廢除）。

## What Changes

**加入**

- 檔案為主的商品資料層：每個商品一個資料夾 `wwwroot/products/{slug}/`，內含 `meta.json`（結構化欄位）、`story.md`（Markdown 故事）、`cover.*`（首頁縮圖）與 `01.*, 02.*` 等 gallery 圖檔
- 首頁 (`/`)：品牌 hero + 精選商品（依 `featured` 數字欄位排序）+ 品牌故事 + 購買方式說明
- 商品列表頁 (`/products`)：所有商品 grid，支援 `?category=xxx` query 過濾
- 商品詳細頁 (`/products/{slug}`)：圖片 gallery + metadata + Markdown 故事渲染 + 「前往賣貨便」CTA
- 依賴：加入 `Markdig` NuGet 用於故事渲染

**移除**（相對於已廢除的 `add-portfolio-mvp` 提案）

- Admin 後台整套（不再需要）
- Session 詢問清單機制（賣貨便直接處理）
- LINE 官方帳號導單流程（社群走 IG/Threads，交易走賣貨便）
- SQLite + EF Core + Identity 相依（拆除 scaffold 留下的 5 個 NuGet 與相關 code）
- 商品分類 CRUD 後台（category 直接寫在 `meta.json`）
- 圖片上傳後台（使用者代為丟入資料夾）

## Capabilities

### New Capabilities

- `product-showcase`：純介紹站的公開瀏覽體驗，包含首頁、商品列表、商品詳細頁、檔案式商品資料掃描

### Modified Capabilities

（無 — 此為 pivot 後首次建立的 spec）

## Impact

**移除**
- `Areas/Identity/`（整個資料夾）
- `Data/`（含 `ApplicationDbContext.cs` 與 `Migrations/`）
- `app.db`, `app.db-shm`, `app.db-wal`
- `JenJenKnits.csproj` 中 5 個 NuGet：`Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Identity.UI`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`，以及 `app.db` 的 `<None Update>` 條目
- `appsettings.json` 中 `ConnectionStrings` section
- `Program.cs` 中 `AddDbContext`, `AddDefaultIdentity`, `UseMigrationsEndPoint`, `UseAuthorization` 相關註冊

**新增**
- `wwwroot/products/`：商品資料夾根目錄（`.gitignore` 需允許此路徑；圖檔與 `meta.json` 皆進 git）
- `Models/Product.cs`：POCO 型別
- `Services/IProductCatalog.cs` + `FileSystemProductCatalog.cs`：商品掃描服務
- `Pages/Products/Index.cshtml(.cs)`：商品列表頁
- `Pages/Products/Detail.cshtml(.cs)`：商品詳細頁（route: `@page "/products/{slug}"`）
- `Markdig` NuGet

**修改**
- `Pages/Index.cshtml(.cs)`：硬編的 4 張商品卡改為從 `IProductCatalog` 讀取
- `Pages/Shared/_Layout.cshtml`：導覽列加入 `/products` 連結
- `Program.cs`：大幅簡化（移除 Identity/EF/SQLite 相關註冊，加入 `IProductCatalog` DI）

**範圍外（不在此 change 處理）**
- 部署設定（Cloud Run + Cloudflare + max-instances + auto-disable billing）：待另開 change 或 `docs/deployment.md` 記錄
- 賣貨便庫存同步：明確 non-goal，訪客點 CTA 到賣貨便看真實狀態
