# Design

## Context

- 商品頁的 JSON-LD 寫在 `Pages/Products/Detail.cshtml`：`@graph` 裡有 ItemPage（`{頁面}#webpage`）與 Product（`{頁面}#product`），用 `@id` 互相關聯，由 `_Layout` 用 `PageSeo.Serialize` 輸出
- 畫面上的麵包屑：「← 全部作品」連到 `/products`；商品有分類時再接「/ 飲料提網」，連到 `/products?category=飲料提網`。沒有「首頁」，也沒有商品名稱這一層（商品名稱是下方的 `<h1>`）
- 分類篩選頁的 canonical 指向 `/products`（`add-seo-metadata` 的決定），sitemap 也只列 `/products`
- 靜態輸出（`tools/export-static.mjs`）只改寫 `href`／`src` 屬性裡的站內路徑：分類連結在 GitHub Pages 上變成 `/products/category/飲料提網/`。JSON-LD 裡的絕對網址不會被改寫
- Google 的麵包屑規定（2026-10 查詢 https://developers.google.com/search/docs/appearance/structured-data/breadcrumb ）：至少兩個 ListItem；每項必填 `name`、`position`，`item` 除了最後一項都必填；建議表現「一般使用者走到這頁的路徑」，不用照網址結構；不需要列網域那一層，也不需要列頁面本身；只在桌機版搜尋結果顯示

## Goals / Non-Goals

**Goals:**

- 麵包屑裡每個網址都是會被收錄的正式網址（canonical），在伺服器與 GitHub Pages 上點進去都是同一個內容

**Non-Goals:**

- 分類頁擁有自己的 canonical 或乾淨網址（`/products/category/{分類}`）
- 把麵包屑寫成共用元件給其他頁面用（目前只有商品頁有上一層）

## Decisions

### D1：麵包屑只有兩層：「作品」→ 這件商品

```
@graph
├─ ItemPage        @id = {頁面}#webpage     …、breadcrumb → {頁面}#breadcrumb
├─ Product         @id = {頁面}#product
└─ BreadcrumbList  @id = {頁面}#breadcrumb
   ├─ 1  name「作品」       item = {首頁}products     （Site.PageUrl("/products")）
   └─ 2  name = 商品名稱    item = {頁面}             （和 canonical 相同）
```

不放分類，原因：

1. 分類頁的 canonical 指回 `/products`，麵包屑指向它等於指向一個自稱「我其實是 `/products`」的網址
2. JSON-LD 的網址不會被靜態輸出改寫：伺服器產生的 `/products?category=飲料提網` 到了 GitHub Pages 會被忽略查詢字串，打開的是全部作品，和名稱「飲料提網」對不上

替代方案：

- 分類用查詢字串網址：有上面兩個問題
- 分類用靜態網址 `/products/category/…/`：伺服器上沒有這個路由，本機與測試看到的是 404；要讓兩邊一致得改分類網址（見 Non-Goals）
- 讓分類頁有自己的 canonical 與乾淨網址，再把分類放進麵包屑：要改路由、canonical、sitemap 與靜態輸出；目前每個分類只有一兩件商品，內容太少，不適合單獨收錄。等商品與分類變多時再考慮，到時候麵包屑加一層即可

不放「首頁」：Google 會自己顯示網域，規定也說不需要列這一層；畫面上的麵包屑也沒有首頁。

### D2：第一層名稱用「作品」

和導覽列、作品列表的 `<h1>` 與標題開頭相同，是那一頁的名字。畫面上的「← 全部作品」是連結文字，意思相同；搜尋結果會顯示成「jhcraftstudio.com › 作品」。

### D3：最後一層也寫 `item`

Google 允許最後一項省略 `item`（會用所在頁面的網址），但寫出來比較明確，其他讀結構化資料的服務（Bing、AI 搜尋）不用自己推斷。值和 canonical 相同，所以不會不一致。

### D4：麵包屑放進同一份 `@graph`，ItemPage 用 `breadcrumb` 指向它

延續既有做法（`add-site-name` D1）：一份 JSON-LD、各節點用 `@id` 關聯。`breadcrumb` 是 schema.org WebPage（含 ItemPage）的屬性。

替代方案：把 BreadcrumbList 直接巢狀寫在 ItemPage 的 `breadcrumb` 裡。Google 兩種都讀得懂，但和其他節點寫法不同，測試也要多一層。

### D5：測試

`SeoTests` 新增商品頁測試：從 `@graph` 找出 BreadcrumbList，斷言兩項的 `position`（1、2）、`name`（「作品」、商品名稱）、`item`（`/products` 的絕對網址、該頁的 canonical），以及 ItemPage 的 `breadcrumb.@id` 指向它。用有分類的商品測，順便確認分類沒有被放進去。

## Risks / Trade-offs

- [Google 不顯示麵包屑，或手機版看不到] → Google 不保證採用，且目前只在桌機版顯示；資料本身仍幫助搜尋引擎理解網站結構。上線後用複合式搜尋結果測試確認讀得到
- [搜尋結果路徑少了分類（「› 作品」而不是「› 作品 › 飲料提網」），少一個關鍵字] → 商品名稱與標題已經有商品類型；分類頁有自己的 canonical 之後再加一層（D1）
- [之後改了作品列表的網址或名稱，麵包屑忘了跟著改] → 網址用 `Site.PageUrl("/products")` 產生、測試會比對 canonical，網址改了測試會失敗

## Migration Plan

1. `dotnet test` 通過後 commit、push
2. `bash tools/deploy-pages.sh` 重新輸出並部署 GitHub Pages；IndexNow 會通知 Bing 兩件商品頁有變動
3. 用 Schema Markup Validator 與 Google 複合式搜尋結果測試檢查線上商品頁，確認 BreadcrumbList 0 錯誤

退回：revert 這個 commit 後重新部署。只是多一段 JSON-LD，沒有資料或網址的變動。
