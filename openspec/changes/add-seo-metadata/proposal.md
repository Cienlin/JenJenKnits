## Why

IG 帳號開始經營後，網站連結會常被貼到 IG、Threads、LINE。目前頁面沒有社群分享資訊，貼出去只有網址、沒有照片與名稱；也沒有 sitemap、canonical 與商品結構化資料，搜尋引擎較難完整、正確地收錄。

## What Changes

- 每頁輸出社群分享資訊（Open Graph / Twitter Card）：標題、描述、照片、網址；商品頁用商品封面
- 每頁輸出 canonical 網址（絕對網址；作品列表的分類篩選指回 `/products`）
- 商品頁輸出 Product 結構化資料（JSON-LD）；首頁輸出品牌（Organization）結構化資料，含 Instagram
- 新增 `/sitemap.xml`（首頁、作品列表、所有已發布商品，含最後修改時間）與 `/robots.txt`（禁止 `/admin`，指向 sitemap）
- 作品列表頁使用專屬的 description
- 新設定 `Site:BaseUrl`：網站對外的完整網址（目前是 GitHub Pages），所有絕對網址由它組成；換主機時只改這個設定
- GitHub Pages 靜態輸出一併輸出 `sitemap.xml`、`robots.txt` 與分享用照片

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

- `product-showcase`：新增搜尋引擎與社群分享相關的需求

## Impact

- `Models/SiteOptions.cs`、`appsettings.json`：`BaseUrl`
- `Models/Product.cs`、`Services/DbProductCatalog.cs`：商品帶出最後修改時間（sitemap 用）
- `Pages/Shared/_Layout.cshtml`：meta / canonical / JSON-LD 輸出
- `Pages/Index`、`Pages/Products/Index`、`Pages/Products/Detail`：各頁的分享資訊與結構化資料
- `Program.cs`：`/sitemap.xml`、`/robots.txt`
- `tools/export-static.mjs`：輸出上述檔案與分享照片
- `JenJenKnits.Tests`：對應測試
