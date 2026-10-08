## 1. 設定與資料

- [x] 1.1 `SiteOptions.BaseUrl` 與 `appsettings.json` 設為 GitHub Pages 網址；`Product.UpdatedAt` 由 `DbProductCatalog` 帶出；`dotnet build` 成功

## 2. 頁面輸出

- [x] 2.1 `_Layout` 輸出 Open Graph、Twitter Card、canonical（絕對網址、不含查詢字串）與 JSON-LD；首頁、作品列表、商品頁設定各自的標題、描述、照片、類型；作品列表使用專屬描述
- [x] 2.2 商品頁 Product JSON-LD（照片、品牌、分類、有價格才有 Offer）；首頁 Organization JSON-LD（含 Instagram）
- [x] 2.3 測試：商品頁的 og:image 為封面絕對網址、canonical 不含查詢字串、分類篩選 canonical 指向 `/products`、JSON-LD 可被解析且無價格時沒有 Offer

## 3. Sitemap 與 robots.txt

- [x] 3.1 `/sitemap.xml` 與 `/robots.txt`；測試：sitemap 不含草稿與缺封面商品、robots.txt 禁止 `/admin` 並指向 sitemap

## 4. 靜態輸出與驗證

- [x] 4.1 `tools/export-static.mjs` 輸出 sitemap、robots 與分享照片；連結檢查 0 缺漏
- [x] 4.2 部署 GitHub Pages，確認線上頁面的 meta 與分享照片網址可存取；`dotnet test` 全部通過
