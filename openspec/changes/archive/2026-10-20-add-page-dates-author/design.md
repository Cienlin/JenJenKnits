# Design

## Context

- 首頁 JSON-LD 是一個 `@graph`：Organization（`#organization`）、WebSite（`#website`）、有題目時的 FAQPage（`#faq`）。商品頁是單一個 Product 物件，不是 `@graph`
- 資料庫只有商品的 `UpdatedAt`（UTC），沒有建立時間；常見問題寫在 `SiteFaq.Default`，沒有日期
- sitemap 的商品 `lastmod` 目前直接用 UTC 日期（`UpdatedAt.ToString("yyyy-MM-dd")`）
- 頁尾「版權頁」是 `<dl>`：品牌、內容、線材色卡、販售
- ultralab 的判斷在伺服器端，看不到規則；只能照 schema.org 與 Google 的做法，上線後重掃確認
- Bing 指南第 14 點、禁止事項「Misleading Structured Data」：結構化資料必須和畫面上看得到的內容一致

## Goals / Non-Goals

**Goals:**

- 每個日期都對得上真正的資料變動，不用「部署日期」「今天」這類每次都會變的日期
- 結構化資料的日期在畫面上看得到

**Non-Goals:**

- 首頁版型文字（標題、購買方式說明）改了也更新日期：這類修改很少，沒有可靠的資料來源；要的話之後再考慮
- 不準確的類型或日期：不加 Article、ProfessionalService、datePublished

## Decisions

### D1：品牌類型用 OnlineStore

schema.org 的 OnlineStore（網路商店）是 Organization 的子類型，Google 的組織結構化資料說明也建議用最明確的子類型。網站在賣貨便販售、沒有實體店面，OnlineStore 比 Store、LocalBusiness 準確（後兩者代表有實體地點）。`@id` 不變，WebSite 的發行者照樣指向它。

替代方案：ProfessionalService（專業服務，屬於 LocalBusiness，需要地址，不符合）；Article（文章，首頁不是文章）。

### D2：首頁與商品頁各加一個「頁面」節點

```
首頁 @graph
├─ OnlineStore  #organization
├─ WebSite      #website
└─ WebPage（有常見問題時 @type 是 ["WebPage", "FAQPage"]）  {首頁}#webpage
     url、name（頁面標題）、isPartOf → #website、about → #organization、author → #organization、
     dateModified、mainEntity（有常見問題時：題目與答案）

商品頁 @graph
├─ ItemPage  {商品頁}#webpage   url、name、isPartOf → 首頁#website、author → 首頁#organization、dateModified、mainEntity → #product
└─ Product   {商品頁}#product   （原本的欄位不變）
```

常見問題併進首頁的頁面節點，而不是再放一個獨立的 FAQPage：FAQPage 本身就是 WebPage 的一種，同一個網址只描述一次比較清楚（Yoast 等常見 SEO 外掛也是用 `["WebPage", "FAQPage"]`）。原本的 `#faq` 改成 `#webpage`。

替代方案：保留獨立的 FAQPage 再加一個 WebPage：同一頁兩個頁面節點，搜尋引擎要自己判斷哪個才是這一頁。

### D3：更新日期的來源

- 商品頁：商品的 `UpdatedAt`
- 首頁：精選商品的 `UpdatedAt` 與 `SiteFaq.UpdatedOn` 之中最新的。`SiteFaq.UpdatedOn` 是寫在程式裡的日期，改題目或答案時一起改（註解寫明）
- 日期一律轉成台灣時間（UTC+8，台灣沒有日光節約時間）再取日期，格式 `YYYY-MM-DD`。`Product` 加一個台灣時間的日期屬性，商品頁、首頁、sitemap 都用它，三處不會差一天

替代方案：

- 部署時比對內容決定日期（IndexNow 的比對結果）：最準，但日期要在輸出後改寫 HTML，綁死 GitHub Pages 靜態輸出，搬到 Azure 就失效
- 用建置或部署的日期：每次部署都變，等於假的更新日期

### D4：作者與畫面上的日期

- `_Layout.cshtml` 在所有前台頁面輸出 `<meta name="author" content="織物製造所">`
- `PageSeo` 加 `Modified`（日期，可以沒有）；有值時，版權頁多一列「更新」，內容是 `<time datetime="YYYY-MM-DD">YYYY-MM-DD</time>`；結構化資料的 `dateModified` 用同一個值，畫面與資料一定一致
- 作品列表、找不到頁面沒有日期，不顯示這一列

### D5：品牌類型列兩個、IG 當聯絡方式（2026-10-20 追加）

- `@type` 寫成 `["Organization", "OnlineStore"]`：OnlineStore 本來就是 Organization 的子類型，兩個都列不改變意思，Google、Bing、Validator 都支援多個類型；ultralab 只認字面上的 Organization（D1 上線後「品牌名稱一致性」從通過變成找不到品牌資料）
- 已設定 Instagram 時：品牌資料加 `contactPoint`（`ContactPoint`，`contactType` 為 `customer service`，`url` 是 IG，`availableLanguage` 為 `zh-Hant`）；頁尾版權頁在「販售」後面加「聯絡」一列，「Instagram @帳號（私訊）」連到 IG、開新分頁。帳號名稱由 IG 網址取出（`SiteOptions.InstagramHandle`）
- 連結的點擊範圍用內距放大到約 44px，負外距抵銷，列高不變（390px 實測）
- ultralab 的「聯絡方式」可能只認 Email、電話，不一定算通過；但 IG 是目前真正的聯絡管道，對訪客與搜尋引擎都準確

## Risks / Trade-offs

- [改了常見問題忘記改 `SiteFaq.UpdatedOn`] → 首頁日期會偏舊（不會是錯的「未來」日期）；註解提醒，改題目的流程（status.md）也寫上
- [ultralab 的「內容類型」清單不包含 WebPage、OnlineStore] → 這次只做準確的標記；重掃後若仍未通過，記錄下來，不為了分數加不準確的類型
- [商品頁 JSON-LD 從單一物件改成 @graph] → Google 與 Bing 都支援 @graph；上線後用 Schema Markup Validator 確認 Product 與 Offer 仍讀得到

## Migration Plan

1. 實作、`dotnet test` 通過，390px 手機看頁尾的新列
2. 和 `add-indexnow` 一起部署：IndexNow 會通知所有有變動的頁面（首頁、作品列表、兩件商品頁）
3. Schema Markup Validator 檢查首頁與一件商品頁：0 錯誤；ultralab 重掃首頁，記下三項的結果

回復：revert 這次的 commit 再部署。
