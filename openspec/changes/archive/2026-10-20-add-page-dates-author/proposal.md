# Proposal

## Why

ultralab.tw 的 AEO 掃描（2026-10-20，首頁）有三項失敗：沒有內容類型的結構化資料、沒有作者、沒有發布或更新日期。AI 搜尋引用來源時，會參考「誰寫的、什麼時候更新的」。網站目前只有品牌（Organization）、網站（WebSite）、常見問題（FAQPage）與商品（Product）資料，沒有描述「這一頁」本身的資料，也沒有任何日期。

要用準確的資訊補上：網站不是文章（Article），也不是到府服務（ProfessionalService），硬加這些類型屬於 Bing 指南禁止的「誤導性結構化資料」。

## What Changes

- 品牌的類型從 Organization 改成更明確的 OnlineStore（網路商店）
- 首頁與商品頁加上「頁面」資料：首頁是 WebPage（有常見問題時同時是 FAQPage，常見問題併進這個節點），商品頁是 ItemPage，主要內容指向商品。頁面資料標明作者是品牌，並帶更新日期（dateModified）
- 所有前台頁面加 `<meta name="author" content="織物製造所">`
- 頁尾「版權頁」顯示這一頁的更新日期，和結構化資料的日期一致（結構化資料必須對得上畫面上看得到的內容）
- 更新日期的來源：商品頁用商品的更新時間；首頁用精選商品與常見問題裡最新的日期；日期一律用台灣時間
- sitemap 的 `lastmod` 改用同樣的台灣時間日期

2026-10-20 上線後追加（ultralab 重掃的結果）：

- 品牌的類型同時列 Organization 與 OnlineStore：ultralab 只認得字面上的 Organization，改成 OnlineStore 後「品牌名稱一致性」變成找不到品牌資料
- 聯絡方式先只用 Instagram：頁尾版權頁加「聯絡」一列連到 IG；品牌資料加客服聯絡窗口（ContactPoint，IG、繁體中文）

不含：發布日期（datePublished，資料庫沒有建立時間，不寫不準確的日期）、作品列表的頁面資料、作者的個人資料（等「品牌故事」）、LINE 或 Email（等貞貞決定要不要公開）、頁尾版權聲明。

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

- `product-showcase`：
  - 新增「頁面作者與更新日期」需求
  - 「結構化資料」：品牌改為 OnlineStore；首頁與商品頁加上頁面資料（作者、更新日期）；常見問題併入首頁的頁面資料
  - 「Sitemap 與 robots.txt」：lastmod 用台灣時間的日期
  - 新增「頁尾聯絡方式」需求；「結構化資料」的品牌資料加聯絡窗口（追加）

## Impact

- 頁面：`Pages/Index.cshtml`（@graph 重組）、`Pages/Products/Detail.cshtml`（JSON-LD 改成 @graph）、`Pages/Shared/_Layout.cshtml`（meta author、版權頁的更新日期）
- 模型：`PageSeo` 加更新日期；`Product` 加台灣時間的更新日期；`SiteFaq` 加常見問題的更新日期
- `Program.cs`：sitemap 的 lastmod
- 樣式：版權頁多一列，沿用現有樣式
- 測試：`SeoTests`、`HomeFaqTests`
- 上線後重新掃 ultralab，並用 Schema Markup Validator 檢查
