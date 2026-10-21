# Proposal

## Why

商品頁畫面上已經有麵包屑（「← 全部作品 / 飲料提網」），但結構化資料裡沒有對應的 BreadcrumbList，搜尋引擎只能自己猜這一頁在網站裡的位置。補上之後，Google 桌機版搜尋結果可以把網址那一行顯示成「jhcraftstudio.com › 作品」這樣的路徑，也讓搜尋引擎和 AI 搜尋更清楚商品頁屬於「作品」底下。畫面上已經有麵包屑，這次只要補資料，不用改畫面。

## What Changes

- 商品頁的 JSON-LD `@graph` 新增麵包屑（BreadcrumbList）：「作品」（`/products`）→ 這件商品（該頁網址），兩層都用 canonical 網址
- 商品頁（ItemPage）資料用 `breadcrumb` 指向這份麵包屑，和既有的 Product、ItemPage 關聯在一起
- 分類這一層不放進麵包屑：分類頁的 canonical 指回 `/products`，而且 GitHub Pages 上的分類網址和伺服器上不同（理由見 `design.md`）

不含：首頁、作品列表的麵包屑（兩頁都在第一層，沒有上一層可以列）；畫面上麵包屑的樣式或文字；分類頁網址的調整。

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

- `product-showcase`：「結構化資料」需求的商品頁情境，新增麵包屑資料（作品列表 → 這件商品），並由商品頁資料指向它

## Impact

- 頁面：`JenJenKnits/Pages/Products/Detail.cshtml`（商品頁的 JSON-LD）
- 測試：`JenJenKnits.Tests/SeoTests.cs`
- 上線後要重新輸出 GitHub Pages（`tools/deploy-pages.sh`，IndexNow 會通知 Bing 兩件商品頁有變動），再用 Schema Markup Validator 與 Google 複合式搜尋結果測試檢查線上商品頁
- Google 目前只在桌機版搜尋結果顯示麵包屑，手機版只顯示網域；生效需要等 Google 重新檢索
