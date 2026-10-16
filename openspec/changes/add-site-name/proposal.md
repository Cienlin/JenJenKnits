# Proposal

## Why

希望在 Google 搜尋「織物製造所」時能找到官網。但「織物製造所」在日文是一般名詞（織布工廠），目前搜尋結果幾乎都是日本的紡織工廠，所以品牌訊號要盡量明確。Google 的「網站名稱」功能會讀首頁的 WebSite 結構化資料，在搜尋結果上方顯示品牌名稱，而不是網址。這項資料只能放在網域根目錄的首頁；2026-10-16 官網改用 `jhcraftstudio.com` 後（`use-custom-domain`）才符合條件。

## What Changes

- 首頁的結構化資料除了既有的品牌（Organization），新增網站（WebSite）資料：名稱「織物製造所」、別名「JH Craft Studio」、網址為首頁
- 兩者放在同一份 JSON-LD 裡並互相關聯：網站的發行者就是這個品牌
- 修正：結構化資料裡巢狀的空欄位（例如沒設定 Instagram 時的社群帳號）不輸出成 `null`

不含：Search Console 操作（2026-10-16 已完成）、照片縮圖、外部連結經營。

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

- `product-showcase`：「結構化資料」需求的首頁情境，從只有品牌資料改為品牌＋網站資料（網站名稱、別名、首頁網址，並標示發行者是該品牌）

## Impact

- 頁面：`JenJenKnits/Pages/Index.cshtml`（首頁的 JSON-LD）
- 共用：`JenJenKnits/Models/PageSeo.cs`（序列化時移除巢狀的空欄位）
- 測試：`JenJenKnits.Tests/SeoTests.cs`
- 上線後要重新輸出 GitHub Pages；Google 不保證一定會採用網站名稱，生效需要幾天到幾週
