# Design

## Context

- 首頁的 JSON-LD 寫在 `Pages/Index.cshtml`：`PageSeo.JsonLd` 是一個 Organization 字典，由 `_Layout` 用 `PageSeo.Serialize` 輸出成一個 `<script type="application/ld+json">`
- `PageSeo.Serialize` 會先用 `WithoutNulls` 拿掉值是 `null` 的欄位，但只處理字典本身與字典裡的字典，**不處理陣列**。陣列裡如果放的是字典，裡面的 `null`（例如沒設定 Instagram 時的 `sameAs`）會照樣輸出
- 品牌名稱已經出現在 `<title>`（「織物製造所｜手鉤織的日常小物」）、`og:site_name`、首頁 `<h1>` 與 Organization 的 `name`，Google 會拿這些來對照結構化資料
- Google 的網站名稱規定（2026-10 查詢）：WebSite 資料只能放在網域或子網域根目錄的首頁；必填 `name`、`url`，建議 `alternateName`；所有語言都支援；不保證採用，生效需要幾天到幾週
- 測試 `SeoTests` 斷言首頁 JSON-LD 根層的 `@type` 是 Organization；`SiteFactory` 讀 `appsettings.json` 的 `Site:InstagramUrl`（已設定），目前沒有方法在單一測試裡清掉它

## Goals / Non-Goals

**Goals:**

- 首頁同時提供品牌與網站資料，名稱與 `<title>`、`og:site_name`、`<h1>` 一致
- 結構化資料裡任何位置的空欄位都不輸出

**Non-Goals:**

- `inLanguage` 等 Google 網站名稱功能沒有用到的欄位
- 把散落各處的品牌名稱字串集中成一個常數（目前一致，之後改品牌名稱時再做）
- 其他頁面的結構化資料（WebSite 只能放首頁）

## Decisions

### D1：品牌與網站放在同一份 JSON-LD 的 `@graph`，用 `@id` 互相關聯

```
@graph
├─ Organization  @id = {首頁}#organization   name、url、logo、description、sameAs
└─ WebSite       @id = {首頁}#website        name「織物製造所」、alternateName「JH Craft Studio」、url = {首頁}
                 publisher → {首頁}#organization
```

`{首頁}` 是 `Site.PageUrl("/")`，和 canonical 相同。

替代方案：

- 輸出兩個 `<script>`：Google 也能讀，但 `PageSeo.JsonLd` 要改成清單、`_Layout` 要跟著改，影響所有頁面；而且兩份資料之間看不出關係
- 只放 WebSite、拿掉 Organization：會失去標誌與 Instagram 的品牌資料

### D2：`WithoutNulls` 也處理陣列

遇到陣列（字串以外的 `IEnumerable`）時，逐項套用 `WithoutNulls`，讓 `@graph` 裡每個字典的空欄位都被拿掉。這是共用的序列化，之後其他頁面用陣列時也受惠。

替代方案：在 `Index.cshtml` 組資料時用條件判斷，不放 `null` 進去。可行，但每個寫結構化資料的地方都要記得這樣做，容易漏。

### D3：別名用「JH Craft Studio」

使用者決定（2026-10-16）。和 IG 帳號 `jh.craft.studio`、網域 `jhcraftstudio.com` 對應。Google 在主名稱不適合時（例如判斷「織物製造所」太像一般名詞）會改用別名。

### D4：測試可以覆寫網站設定

`SiteFactory` 新增在第一次 `Client()` 之前指定設定值的方法（和既有的 `UseDatabase` 同樣的用法），用來測「未設定 Instagram」。首頁測試改為從 `@graph` 依 `@type` 找出 Organization 與 WebSite 再斷言。

## Risks / Trade-offs

- [Google 不採用網站名稱，仍顯示網址或別名] → 這是 Google 的判斷，無法保證；名稱已在各處保持一致。持續經營外部連結（IG、賣貨便介紹欄），讓 Google 把名稱和網站連起來
- [JSON-LD 格式錯誤，搜尋引擎讀不懂] → 測試會解析 JSON；上線後再用 Schema Markup Validator 檢查一次線上首頁
- [「織物製造所」是日文一般名詞，搜尋這個詞時仍可能排在日本網站後面] → 網站名稱只影響「找到後怎麼顯示」，不直接影響排名；排名要靠收錄與外部連結累積

## Migration Plan

1. 實作、`dotnet test` 通過
2. commit、push，執行 `bash tools/deploy-pages.sh`
3. 用 Schema Markup Validator（validator.schema.org）檢查 `https://jhcraftstudio.com/`：Organization 與 WebSite 都被讀到，沒有錯誤

回復：revert 這次的 commit 再部署一次。
