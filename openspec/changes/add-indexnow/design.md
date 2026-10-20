# Design

## Context

- 官網是 GitHub Pages 靜態網站：`tools/export-static.mjs` 從執行中的網站（localhost:5178）爬出 HTML、資產、`sitemap.xml`、`robots.txt`、`CNAME`；`tools/deploy-pages.sh` 每次在 `site-export/` 重新 `git init`，強制推到 `gh-pages`。推上去後約 1 分鐘生效
- 每次部署「每一頁的 HTML 都會變」，即使內容沒變：頁尾 `<p class="mono version">` 印出建置時的 commit（`AppVersion.Display`），CSS／JS 網址帶 `?v=` 版本號（`asp-append-version`）
- `sitemap.xml` 只列正式網址：首頁、作品列表、每件上架商品。分類篩選頁 `/products/category/X/` 有輸出但不在 sitemap，canonical 指回作品列表
- GitHub Pages 的 CDN 回應 `Cache-Control: max-age=600`
- IndexNow 協定（indexnow.org/documentation）：金鑰 8～128 字元（英數與 `-`）；金鑰檔放根目錄 `/{金鑰}.txt` 是官方「強烈建議」的做法；`POST /indexnow`，JSON `{host, key, keyLocation, urlList}`，一次最多 10,000 個網址；送給一個引擎會分享給所有參與的引擎。回應：200 成功、202 已收到但金鑰驗證中、400 格式錯誤、403 金鑰無效、422 網址不屬於這個網域、429 太頻繁
- Bing 指南：只通知新增、更新、刪除的網址，避免大批送出

## Goals / Non-Goals

**Goals:**

- 沒有內容變動的部署不送任何網址；只改 CSS 也不送
- Bing 收到通知時，GitHub Pages 已經是新版本
- 任何 IndexNow 的錯誤都不讓部署失敗，並告訴使用者怎麼手動補送

**Non-Goals:**

- 照片檔案內容變了但檔名不變（頁面 HTML 不變）時通知：目前照片換檔都會換頁面，之後做照片管理時再看
- sitemap 的 `lastmod`、Google 的即時通知（Google 不支援 IndexNow）
- 網址有子路徑時（例如 `https://cienlin.github.io/JenJenKnits`）通知：金鑰檔必須在網域根目錄，這種情況直接略過並說明

## Decisions

### D1：金鑰存在 repo，輸出時放在網站根目錄

金鑰是 32 個十六進位字元，產生一次後存在 `tools/indexnow-key.txt`。`export-static.mjs` 讀這個檔，在輸出資料夾根目錄寫 `{金鑰}.txt`（內容就是金鑰），和 `CNAME` 一樣每次輸出都帶上。

金鑰本來就會公開在網站上，作用只是證明「這個網域的擁有者送的通知」，不是密碼，所以可以進 git。別人拿到金鑰也只能通知我們自己網站的網址。要換金鑰時改這個檔再部署一次，舊的金鑰檔就從網站上消失。

替代方案：

- 寫在 `appsettings.json`，由 ASP.NET 網站提供金鑰檔：網站本身目前不對外，金鑰檔只在靜態輸出用得到；之後搬到 Azure（`add-deployment`）時再考慮
- 用 `keyLocation` 放在自訂路徑：官方建議根目錄；放根目錄也最簡單

### D2：和線上版本比對，只送 sitemap 裡有變動的網址

`deploy-pages.sh` 在 `site-export/` 建好 git 之後，先 `git fetch --depth 1` 目前的 `gh-pages`（第一次部署沒有就略過），作為「上一版」。`tools/indexnow.mjs` 用 `git show 上一版:檔案` 讀舊檔，和新輸出比對：

- 範圍：新、舊兩份 `sitemap.xml` 的網址。分類篩選頁、404 頁不在 sitemap，不送
- 新增：只在新 sitemap；刪除：只在舊 sitemap（Bing 收到後會看到 404）；更新：兩邊都有，且頁面內容不同
- 比較內容前先去掉每次都會變的部分：頁尾版本 `<p class="mono version">…</p>`，以及網址裡的 `?v=…`
- 網址對應檔案：`https://jhcraftstudio.com/` → `index.html`，`/products/puff-flower-net/` → `products/puff-flower-net/index.html`
- 抓不到上一版（第一次部署）時，送新 sitemap 的全部網址

替代方案：

- 每次部署都送 sitemap 全部網址：最簡單，但沒變的網址也一直送，違反指南「只通知有變動的」，送多了可能被當成濫發（429）
- 用 sitemap 的 `lastmod` 判斷：首頁、作品列表沒有 `lastmod`；商品的更新日期也反映不到版型、常見問題的變動
- 用 HTTP 抓線上網站來比對：比 git 慢，還會遇到 CDN 快取

### D3：等 GitHub Pages 換成新版才送出

推上去之後，從要通知的網址挑一個：有新增或更新時，每 10 秒抓一次線上頁面（加上不重複的查詢字串，避開 CDN 快取），直到去掉版本後和輸出的檔案一樣；只有刪除時，等它回 404。最多等 5 分鐘。金鑰檔和頁面在同一次部署，頁面換成新版就代表金鑰檔也上線了。

替代方案：

- 推上去馬上送：Bing 可能抓到舊內容，這次通知就浪費了；第一次部署時金鑰檔也可能還沒上線，回 403
- 查 GitHub API 的 Pages 建置狀態：要另外登入 `gh`，兩台電腦都要設定

### D4：送出失敗不影響部署

`indexnow.mjs` 自己處理所有錯誤，最後一定以成功結束；`deploy-pages.sh` 呼叫時也加保護。各種情況印出的訊息：

- 200、202：成功（202 是第一次送出、金鑰還在驗證，正常）
- 其他回應碼、網路錯誤、等超過 5 分鐘：顯示原因，並列出這次要通知的網址，請使用者到 Bing Webmaster Tools 的「URL Submission」手動提交
- 沒有變動：顯示「沒有內容變動，不通知 IndexNow」

送到 `https://api.indexnow.org/indexnow`，JSON 帶 `host`、`key`、`keyLocation`（根目錄的金鑰檔網址）、`urlList`。

### D5：預覽模式

`bash tools/deploy-pages.sh --dry-run`：照常輸出並取得線上版本，只列出「會通知哪些網址、各是新增／更新／刪除」，不推、不送。實作後先用預覽確認比對正確，再真正部署。

## Risks / Trade-offs

- [頁尾版本的 HTML 改了，去不掉版本號] → 每一頁都會被當成有變動，結果是多送而不是漏送；預覽模式看得出來（沒改內容卻列出全部網址）。`indexnow.mjs` 的比對規則旁註明要和 `_Layout.cshtml` 頁尾一起改
- [第一次送出回 202，Bing 還沒驗證金鑰] → 正常；之後在 Bing Webmaster Tools 的 IndexNow 頁面確認有收到網址
- [GitHub Pages 超過 5 分鐘才生效] → 不送出，列出網址請使用者手動提交
- [金鑰外流被拿來亂送] → 只能送我們網域的網址，影響有限；換金鑰（D1）即可
- [Google 不會因此更快收錄] → Google 仍靠 sitemap 與 Search Console 手動要求，和現在一樣

## Migration Plan

1. 實作後執行 `bash tools/deploy-pages.sh --dry-run`：目前線上還沒有 10-20 的封面照片 alt 與「飲料提網」叫法，預期列出首頁、作品列表要更新
2. 真正部署：確認線上有金鑰檔、IndexNow 回 200 或 202
3. 幾分鐘到一天後，到 Bing Webmaster Tools 的 IndexNow 頁面確認收到這些網址
4. 再部署一次（沒有改內容），確認顯示「沒有內容變動」

回復：拿掉 `deploy-pages.sh` 裡呼叫 `indexnow.mjs` 的那行即可；要完全停用，再刪掉金鑰檔並重新部署。
