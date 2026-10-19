# Proposal

## Why

越來越多人用 ChatGPT 搜尋、Copilot、Google 的 AI 摘要找東西（AEO）。這些服務會引用頁面上「一問一答」的文字，但官網目前沒有買家最常問的資訊：客製要等多久、怎麼保養、提網適合多大的水壺、運費和取貨方式。Search Console 與 Bing Webmaster Tools 都已登錄（2026-10-16），接下來要讓頁面上有值得被引用的內容。另外品牌資料目前只有網站資料有別名「JH Craft Studio」，品牌本身沒有，搜尋英文名稱時較難對上同一個品牌。

## What Changes

- 首頁在「02 購買方式」後面新增「03 常見問題」章節，題目和答案直接顯示在頁面上（不用點開），首頁目錄多一項
- 首頁結構化資料新增常見問題（FAQPage），內容與頁面上看得到的問答完全一致
- 品牌（Organization）資料補別名「JH Craft Studio」，和網站資料一致
- 只放跟貞貞確認過的答案；還沒確認的題目不顯示，也不輸出結構化資料。全部都沒確認時，首頁不出現這個章節，目錄也不列

不含：後台編輯常見問題（之後與後台第二階段一起考慮）、商品頁各自的常見問題、`llms.txt`。

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

- `product-showcase`：
  - 新增「首頁常見問題」需求
  - 「首頁品牌展示與精選商品」的首頁順序加上常見問題
  - 「結構化資料」的首頁情境：品牌資料加別名，並新增常見問題資料

## Impact

- 頁面：`JenJenKnits/Pages/Index.cshtml`（新章節、目錄、JSON-LD）
- 新增：常見問題的資料（題目與答案，頁面和 JSON-LD 共用同一份）
- 樣式：`JenJenKnits/wwwroot/css/site.css`（沿用編織誌的章節與線條樣式）
- 測試：`JenJenKnits.Tests/SeoTests.cs`
- 不用寫程式的事：把草稿答案傳給貞貞確認
- 上線後要重新輸出 GitHub Pages，再用 Schema Markup Validator 檢查
