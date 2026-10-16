# Tasks

## 1. 序列化移除巢狀空欄位（D2）

- [ ] 1.1 `PageSeo.WithoutNulls` 遇到陣列時逐項處理；新增測試：`Serialize` 一個含 `@graph` 陣列、陣列裡字典有 `null` 值的資料，輸出不含 `null`；`dotnet test` 全部通過

## 2. 首頁的品牌＋網站資料（D1、D3、D4）

- [ ] 2.1 `SiteFactory` 新增在第一次 `Client()` 之前覆寫設定值的方法；既有測試全部通過
- [ ] 2.2 `Index.cshtml` 改為 `@graph`：Organization（`@id` 為首頁 `#organization`，欄位不變）與 WebSite（`@id` 為首頁 `#website`、name「織物製造所」、alternateName「JH Craft Studio」、url 為首頁、publisher 指向 Organization 的 `@id`）
- [ ] 2.3 `SeoTests`：首頁從 `@graph` 依 `@type` 找出 Organization 與 WebSite，斷言 WebSite 的 name、alternateName、url（等於 canonical）與 publisher；新增「未設定 Instagram 時品牌資料沒有 `sameAs`」的測試；`dotnet test` 全部通過

## 3. 上線

- [ ] 3.1 commit、push，執行 `bash tools/deploy-pages.sh`；線上首頁原始碼的 JSON-LD 有 `@graph`、Organization 與 WebSite
- [ ] 3.2 用 Schema Markup Validator（validator.schema.org）檢查 `https://jhcraftstudio.com/`，Organization 與 WebSite 都被讀到、沒有錯誤；`docs/status.md` 記下「網站名稱已上線，生效需幾天到幾週」
