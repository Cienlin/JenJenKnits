## Context

動機見 proposal.md。目前對外的網站是 GitHub Pages 的靜態輸出（`https://cienlin.github.io/JenJenKnits`，子路徑 `/JenJenKnits`），之後會換到正式主機。靜態輸出腳本只改寫以 `/` 開頭的 `href` / `src`。

## Decisions

### D1. `Site:BaseUrl` 決定所有絕對網址

社群分享、canonical、JSON-LD、sitemap 都需要絕對網址。新增 `Site:BaseUrl`（不含結尾斜線），目前設為 GitHub Pages 網址。本機執行時輸出的絕對網址也指向 GitHub Pages，這是刻意的：canonical 永遠指向對外網站。換主機時只改這個設定。

絕對網址不以 `/` 開頭，靜態輸出腳本不會改寫它們，所以不會被重複加上子路徑。

### D2. 頁面以 ViewData 提供分享資訊，`_Layout` 統一輸出

各頁設定 `Title`、`Description`、`OgImage`、`OgType`、`JsonLd`；`_Layout` 負責組成絕對網址並輸出 meta、canonical 與 JSON-LD。預設分享照片使用首頁主照片。JSON-LD 以 `System.Text.Json` 序列化，避免手寫字串的跳脫問題。

### D3. 分類篩選的 canonical 指回 `/products`

篩選結果是 `/products` 的子集，指回主列表比把每個分類當成獨立頁面單純，也與靜態輸出的分類網址（`/products/category/{分類}/`）不一致的問題無關。

### D4. Offer 只在有價格時輸出，不填庫存狀態

網站不同步賣貨便庫存（`add-showcase-mvp` D4），所以不宣告 availability，避免提供錯誤資訊。

### D5. robots.txt 在 GitHub Pages 上的限制

搜尋引擎只讀網域根目錄的 `robots.txt`；GitHub Pages 專案網站在子路徑下，輸出的 `robots.txt` 不會被讀取。後台本來就不會被靜態輸出，影響不大；sitemap 改由 Google Search Console 手動提交。換到正式主機後 robots.txt 即生效。

### D6. 靜態輸出

輸出腳本額外抓取 `/sitemap.xml`、`/robots.txt`，並下載頁面中以 `BaseUrl` 開頭、指向本站圖片的絕對網址（分享照片可能沒有被 `<img>` 引用）。

### D7. 頁面網址結尾一律加斜線

canonical、sitemap 與 JSON-LD 的頁面網址都寫成 `/products/`、`/products/{slug}/`。GitHub Pages 上每頁是 `資料夾/index.html`，不加斜線會被 301 轉址，canonical 不該指向會轉址的網址；ASP.NET 版兩種寫法都能開，所以統一用加斜線的形式。

## Risks / Trade-offs

- [本機看到的分享網址指向 GitHub Pages] → 預期行為（D1）
- [社群平台快取舊的預覽] → 用 Facebook 分享偵錯工具重新抓取
