# Tasks

## 1. 日期與作者（D3、D4）

- [x] 1.1 `Product` 加台灣時間的更新日期（UTC+8 取日期），sitemap 的 `lastmod` 改用它；`SiteFaq` 加 `UpdatedOn`（2026-10-20，註解寫明改題目時一起改）；`PageSeo` 加 `Modified`。既有 sitemap 測試通過，新增測試：UTC 晚上 8 點以後的更新時間在 sitemap 是台灣的隔天；`dotnet test` 全部通過
- [x] 1.2 `_Layout.cshtml` 加 `<meta name="author" content="織物製造所">`；`PageSeo.Modified` 有值時，版權頁多一列「更新」（`<time datetime>`）。新增測試：首頁、商品頁有 meta author 與更新日期列，作品列表沒有更新日期列；`dotnet test` 全部通過

## 2. 結構化資料（D1、D2、D3）

- [x] 2.1 首頁：Organization 改 OnlineStore；常見問題併進 `{首頁}#webpage` 頁面節點（有題目時 `@type` 是 `["WebPage", "FAQPage"]`，沒題目時是 `WebPage`、沒有 mainEntity），帶 url、name、isPartOf、about、author、dateModified；首頁的 `Modified` = 精選商品與 `SiteFaq.UpdatedOn` 中最新的日期。`SeoTests` 的 `GraphNode` 支援 `@type` 陣列；更新既有首頁與常見問題測試，新增：dateModified 等於最新日期且和頁尾一致、author 指向品牌；`dotnet test` 全部通過
- [x] 2.2 商品頁：JSON-LD 改成 `@graph`，ItemPage（`#webpage`，mainEntity 指向 `#product`、author、dateModified）＋ Product（`#product`，原本欄位不變）；`Modified` = 商品更新日期。更新既有商品頁測試，新增：ItemPage 的 dateModified 和頁尾一致、mainEntity 指向 Product；`dotnet test` 全部通過

## 3. 畫面與上線

- [x] 3.1 390px 手機看首頁與商品頁的頁尾：更新日期這一列和其他列對齊、沒有水平捲軸；桌機 1280px 看一次
- [x] 3.2 和 `add-indexnow` 一起部署（部署前先 `--dry-run` 給使用者看會通知哪些網址）；Schema Markup Validator 檢查首頁與泡芙小花提網商品頁 0 錯誤，OnlineStore、WebPage／FAQPage、ItemPage、Product 都讀得到；請使用者用 ultralab 重掃首頁，結果記在 `docs/status.md`（2026-10-20：部署 `2266872`，線上 Validator 首頁與商品頁 0 錯誤；ultralab 重掃留給使用者，記在 `docs/status.md` 待辦）
- [x] 3.3 `docs/status.md`：「已完成」加這個 change；改常見問題的流程補上「一起改 `SiteFaq.UpdatedOn`」
