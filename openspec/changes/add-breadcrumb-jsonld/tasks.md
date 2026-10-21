# Tasks

## 1. 商品頁的麵包屑資料（D1～D5）

- [ ] 1.1 `Detail.cshtml` 的 `@graph` 新增 BreadcrumbList（`@id` 為該頁 `#breadcrumb`）：第 1 項 name「作品」、item 為 `Site.PageUrl("/products")`；第 2 項 name 為商品名稱、item 為該頁網址；ItemPage 加 `breadcrumb` 指向它的 `@id`；更新檔頭註解
- [ ] 1.2 `SeoTests` 新增測試：用有分類的商品，從 `@graph` 找出 BreadcrumbList，斷言只有兩項、`position` 為 1、2、`name` 為「作品」與商品名稱、`item` 為作品列表與該頁的 canonical，且 ItemPage 的 `breadcrumb.@id` 指向它；`dotnet test` 全部通過
- [ ] 1.3 本機啟動網站，看商品頁原始碼的 JSON-LD 有 BreadcrumbList，畫面沒有變化

## 2. 上線

- [ ] 2.1 commit、push，執行 `bash tools/deploy-pages.sh`；線上兩件商品頁原始碼的 JSON-LD 有 BreadcrumbList，網址是 `https://jhcraftstudio.com/...`；IndexNow 送出兩件商品頁
- [ ] 2.2 用 Schema Markup Validator 與 Google 複合式搜尋結果測試（search.google.com/test/rich-results）檢查一件線上商品頁，「導覽標記」（Breadcrumbs）被讀到、沒有錯誤；結果與已完成項目記到 `docs/status.md`
