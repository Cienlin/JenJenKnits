## 1. Domain Model & Data Layer

- [ ] 1.1 於 `Models/` 建立 `Product`、`Category`、`ProductImage`、`AboutContent` entity classes（含 data annotations 與 navigation properties）
- [ ] 1.2 於 `Data/ApplicationDbContext.cs` 加入對應 `DbSet<T>` 屬性
- [ ] 1.3 於 `OnModelCreating` 設定 unique constraints（`Product.Slug`、`Category.Slug`）與 decimal 精度（`Product.Price` 為 `decimal(10,2)`）
- [ ] 1.4 執行 `dotnet ef migrations add AddPortfolioSchema` 建立 migration
- [ ] 1.5 執行 `dotnet ef database update` 套用到本地 `app.db`
- [ ] 1.6 於 `appsettings.json` 加入 `LineOA:Url`（先放 placeholder）與 `Seed:Admin:Email` 設定
- [ ] 1.7 以 `dotnet user-secrets` 設定 `Seed:Admin:Password`（不進 repo）
- [ ] 1.8 於 `Program.cs` 加入 `SeedAdminAsync()` 啟動邏輯（建立 Admin role + 首位 admin 帳號）

## 2. Public Site Foundation

- [ ] 2.1 於 `Program.cs` 註冊 Session middleware（`AddSession` + `UseSession`），設定 30 天滑動到期
- [ ] 2.2 建立 `Services/IInquiryListService.cs` 與實作類別（封裝 session 讀寫 `List<int>` product IDs）
- [ ] 2.3 於 DI 註冊 `IInquiryListService`
- [ ] 2.4 更新 `Pages/Shared/_Layout.cshtml` 導覽列（首頁 / 商品 / 關於我 / 詢問清單，詢問清單顯示數量 badge）
- [ ] 2.5 更新 `Pages/Index.cshtml` 首頁（顯示品牌介紹 + 至多 6 件 `IsFeatured=true` 商品，若無則 fallback 最新 6 件）

## 3. Public Site - Product Catalog

- [ ] 3.1 建立 `Pages/Products/Index.cshtml` 商品目錄頁（分頁 12 件、依 `CreatedAt` 新到舊排序）
- [ ] 3.2 於商品目錄頁加入分類篩選（左側 sidebar 或頂部 pills，QueryString `?category={slug}`）
- [ ] 3.3 於商品目錄頁加入搜尋框（QueryString `?q={keyword}`，SQL LIKE 於 Name/Description）
- [ ] 3.4 加入「搜尋無結果」友善提示 + 回全部商品連結
- [ ] 3.5 建立 `Pages/Products/Detail.cshtml`，route 使用 slug（`@page "/products/{slug}"`）
- [ ] 3.6 商品詳細頁顯示：名稱、價格、分類、材質、尺寸、故事描述、圖片 gallery
- [ ] 3.7 於商品詳細頁加入「加入詢問清單」按鈕（POST handler，重複加入僅計一次）
- [ ] 3.8 未上架 / 不存在商品回傳 HTTP 404 + 友善錯誤頁

## 4. Public Site - Inquiry & LINE Handoff

- [ ] 4.1 建立 `Pages/Inquiry.cshtml` 顯示 session 中所有商品（縮圖、名稱、價格、總數）
- [ ] 4.2 加入單一商品移除功能（POST handler）
- [ ] 4.3 加入全部清空功能（POST handler）
- [ ] 4.4 空清單時顯示「尚無商品」訊息 + 回商品目錄連結
- [ ] 4.5 加入「以 LINE 詢問」按鈕：產生詢問文字、複製到 clipboard（JS）、開新分頁至 `{LineOA:Url}`
- [ ] 4.6 清單為空時將 LINE 按鈕設為 disabled

## 5. Public Site - About Page

- [ ] 5.1 建立 `Pages/About.cshtml` 讀取 `AboutContent` 唯一 row 並顯示
- [ ] 5.2 無資料時顯示「內容準備中」佔位文字

## 6. Admin Area - Foundation

- [ ] 6.1 於 `Areas/Admin/` 建立 Razor Pages 區域（或直接用 `/admin/*` 路徑）
- [ ] 6.2 建立 `Pages/Admin/_Layout.cshtml` 後台專屬 layout（含側邊選單：儀表板 / 商品 / 分類 / 關於我）
- [ ] 6.3 建立 `Pages/Admin/Index.cshtml` 儀表板首頁，含 `[Authorize(Roles = "Admin")]`
- [ ] 6.4 建立 403 友善錯誤頁（`Pages/AccessDenied.cshtml`）
- [ ] 6.5 於 `Program.cs` 設定 Identity `AccessDeniedPath = "/AccessDenied"`

## 7. Admin - Category CRUD

- [ ] 7.1 建立 `Pages/Admin/Categories/Index.cshtml` 分類列表
- [ ] 7.2 建立 `Pages/Admin/Categories/Create.cshtml`（名稱 + slug，slug 空白時從 name 自動產生）
- [ ] 7.3 建立 `Pages/Admin/Categories/Edit.cshtml`
- [ ] 7.4 加入刪除功能（若分類仍有商品則拒絕並提示「請先移動商品」）

## 8. Admin - Product CRUD

- [ ] 8.1 建立 `Pages/Admin/Products/Index.cshtml` 商品列表（含分類篩選、`IsPublished` 快速切換）
- [ ] 8.2 建立 `Pages/Admin/Products/Create.cshtml` 表單（名稱、分類、價格、材質、尺寸、故事、slug、IsPublished、IsFeatured）
- [ ] 8.3 實作 slug 自動建議邏輯（英數保留 → 空白轉 dash → 全中文時用 `product-{shortGuid}`）
- [ ] 8.4 加入 slug 唯一性 server-side 驗證
- [ ] 8.5 建立 `Pages/Admin/Products/Edit.cshtml`
- [ ] 8.6 加入刪除功能（含確認 dialog，級聯刪除 `ProductImage` 記錄與檔案）
- [ ] 8.7 加入必填欄位 client + server validation（名稱、價格）

## 9. Admin - Image Upload & Management

- [ ] 9.1 於商品編輯頁加入多檔上傳表單（`accept="image/*"` + 顯示已上傳圖片預覽）
- [ ] 9.2 實作上傳 handler：驗證副檔名（jpg/jpeg/png/webp）、大小 ≤ 5MB、儲存至 `wwwroot/uploads/{yyyy}/{mm}/{guid}.{ext}`
- [ ] 9.3 建立 `ProductImage` 資料庫記錄（含相對路徑、原始檔名、`DisplayOrder`）
- [ ] 9.4 於圖片 gallery 加入單張刪除功能（同步刪 DB 記錄與檔案）
- [ ] 9.5 加入圖片排序功能（拖曳或上下移動按鈕，更新 `DisplayOrder`）

## 10. Admin - About Content

- [ ] 10.1 建立 `Pages/Admin/About.cshtml` 單頁表單編輯 `AboutContent`
- [ ] 10.2 實作 upsert 邏輯（若無 row 則新增、有則更新）

## 11. Styling & Responsive Design

- [ ] 11.1 更新首頁 hero + 商品縮圖為手工商品風格（大留白、柔和字型）
- [ ] 11.2 商品目錄頁：手機單欄、平板 2 欄、桌機 3-4 欄
- [ ] 11.3 商品詳細頁：手機圖上文下、桌機圖左文右
- [ ] 11.4 導覽列：手機 hamburger menu、桌機水平列
- [ ] 11.5 全站按鈕最小 44×44px、字型行高 ≥ 1.5
- [ ] 11.6 於 375px 寬度 Chrome DevTools 驗證所有前台頁面無水平捲軸

## 12. Verification & Handoff

- [ ] 12.1 手動測試 `product-showcase` spec 全部 scenarios（首頁、目錄、搜尋、詳細、詢問、LINE、關於我）
- [ ] 12.2 手動測試 `admin-content-management` spec 全部 scenarios（登入、CRUD、圖片、about）
- [ ] 12.3 於乾淨環境（刪 `app.db` 重跑）驗證 migration + seed 流程可重現
- [ ] 12.4 撰寫 `README.md`：專案介紹、如何跑起來、admin 預設帳密設定方式
- [ ] 12.5 建立 dev 測試資料（3 件圍巾 + 2 件肩背袋 + 2 分類 + 1 份 about 內容）供貞貞試用
