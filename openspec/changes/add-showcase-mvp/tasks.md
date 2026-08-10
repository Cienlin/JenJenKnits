## 1. Scaffold Cleanup（拆除 Identity + EF + SQLite）

- [ ] 1.1 刪除 `Areas/Identity/` 整個資料夾
- [ ] 1.2 刪除 `Data/` 整個資料夾（含 `ApplicationDbContext.cs` 與 `Migrations/`）
- [ ] 1.3 刪除專案根目錄的 `app.db`, `app.db-shm`, `app.db-wal`
- [ ] 1.4 從 `JenJenKnits.csproj` 移除 5 個 NuGet 與 `app.db` 的 `<None Update>` 條目
- [ ] 1.5 簡化 `Program.cs`（移除 DbContext / Identity / MigrationsEndPoint / UseAuthorization 註冊，剩 ~15 行）
- [ ] 1.6 從 `appsettings.json` 移除 `ConnectionStrings` section
- [ ] 1.7 檢查 `_Layout.cshtml` 是否有 `_LoginPartial` 引用並移除（若有）
- [ ] 1.8 執行 `dotnet build`，確認清乾淨且無編譯錯誤
- [ ] 1.9 執行 `dotnet run`,確認 landing page 仍可正常瀏覽

## 2. Dependencies

- [ ] 2.1 `dotnet add package Markdig`
- [ ] 2.2 執行 `dotnet build` 確認 Markdig 可解析

## 3. Product Data Layer

- [ ] 3.1 建立 `Models/Product.cs` POCO：`Slug`, `Name`, `Category`, `Price`, `Material`, `Dimensions`, `ShortDescription`, `BuyUrl`, `Featured (int?)`, `CoverImageRelativePath`, `GalleryImageRelativePaths (List<string>)`, `StoryHtml (string?)`
- [ ] 3.2 建立 `Services/IProductCatalog.cs`：`GetAll()`, `GetFeatured()`, `GetByCategory(string)`, `GetBySlug(string)`, `GetAllCategories()`
- [ ] 3.3 建立 `Services/FileSystemProductCatalog.cs`：掃 `wwwroot/products/{slug}/`，讀 `meta.json` + `story.md` + 圖檔清單
- [ ] 3.4 錯誤處理：`meta.json` 缺失或 parse 失敗 → 跳過 + `ILogger.LogWarning`
- [ ] 3.5 錯誤處理：`cover.*` 缺失或 `buyUrl` 空 → 跳過 + warning
- [ ] 3.6 Story 渲染：使用 Markdig 將 `story.md` 轉為 HTML 存於 `Product.StoryHtml`
- [ ] 3.7 於 `Program.cs` 註冊 DI：dev 用 `AddScoped`（每 request 重掃），prod 用 `AddSingleton`（startup 掃一次）
- [ ] 3.8 建立單元測試（可選）：假 folder 結構驗證 scan 邏輯

## 4. Home Page（Index）

- [ ] 4.1 修改 `Pages/Index.cshtml.cs`：注入 `IProductCatalog`，`OnGet` 取得 featured products（`featured` 非 null，依 featured 值升冪排序）
- [ ] 4.2 修改 `Pages/Index.cshtml`：硬編的 4 張商品卡改為 `@foreach` featured，讀取 `name / shortDescription / category / coverImage`
- [ ] 4.3 若 featured 少於 3 件，fallback 為所有商品最新 6 件（依資料夾修改時間或字母序）
- [ ] 4.4 每張商品卡 wrap `<a asp-page="/Products/Detail" asp-route-slug="@p.Slug">` 連到詳細頁

## 5. Product Listing Page（/products）

- [ ] 5.1 建立 `Pages/Products/Index.cshtml(.cs)`：注入 `IProductCatalog`，顯示所有商品 grid（沿用 home page 商品卡樣式）
- [ ] 5.2 支援 `?category={value}` query 過濾
- [ ] 5.3 頁上方顯示分類 pills（從所有商品 category 去重取出），active pill 高亮
- [ ] 5.4 過濾無結果時顯示「此分類目前無商品」提示 + 回全部商品連結
- [ ] 5.5 更新 `_Layout.cshtml` 導覽列：加入 `作品` 連結指向 `/Products`（保留現有 anchor 導覽）

## 6. Product Detail Page（/products/{slug}）

- [ ] 6.1 建立 `Pages/Products/Detail.cshtml(.cs)`，route: `@page "/products/{slug}"`
- [ ] 6.2 `OnGet(string slug)`：找不到 slug 或商品 → `return NotFound()`
- [ ] 6.3 版面：手機圖上文下、桌機圖左文右
- [ ] 6.4 圖片區：cover 為主圖，`01, 02, ...` 為 gallery（點縮圖切換主圖或 lightbox，MVP 可先只列 gallery）
- [ ] 6.5 資訊區：name, category, price（含 NT$ 符號）, material, dimensions
- [ ] 6.6 故事區：`@Html.Raw(Model.Product.StoryHtml)`（Markdig 已渲染）
- [ ] 6.7 CTA：大型「前往賣貨便」按鈕，`target="_blank"` + `rel="noopener noreferrer"`
- [ ] 6.8 未找到頁面樣式：套用友善 404 頁（可用預設或自訂）

## 7. Dev Sample Data

- [ ] 7.1 建立 `wwwroot/products/scarf-earth-tone/`：完整範例（`meta.json` + `story.md` + `cover.jpg` + 2 張 gallery）
- [ ] 7.2 至少再建 2-3 個範例商品，涵蓋不同 category（例如「純羊毛」、「有機棉」、「限量」）
- [ ] 7.3 使用免費商用授權圖片（Unsplash / Pexels）或貞貞實際作品照
- [ ] 7.4 至少 2 件標 `featured`（不同 featured 值），驗證排序正確

## 8. Styling & Responsive

- [ ] 8.1 商品列表頁：手機單欄、平板 2 欄、桌機 3-4 欄（沿用 home 的 product-grid 樣式）
- [ ] 8.2 商品詳細頁：確認手機/桌機兩種 layout 皆可用
- [ ] 8.3 分類 pills：手機可橫向捲動、桌機一列排開
- [ ] 8.4 於 375px 寬度 Chrome DevTools 驗證所有頁面無水平捲軸
- [ ] 8.5 按鈕最小 44×44px、內文行高 ≥ 1.5

## 9. Verification & Handoff

- [ ] 9.1 手動測試 `product-showcase` spec 所有 scenarios（首頁 / 列表 / 過濾 / 詳細 / 賣貨便 CTA / 404 / meta 缺失容錯）
- [ ] 9.2 於乾淨環境（新 clone）執行 `dotnet run` 可正常起（無 db 初始化步驟）
- [ ] 9.3 撰寫 `README.md`：專案簡介 / 如何跑起來 / **如何上架新商品**（照著範例資料夾 copy）
- [ ] 9.4 更新 `.gitignore`：確保 `bin/`, `obj/` 排除；移除 `*.db` 相關（若有）；可加 `wwwroot/products/**/_*` 讓下劃線前綴為草稿
