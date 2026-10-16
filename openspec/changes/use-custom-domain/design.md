# Design

## Context

- 展示網站是把本機執行中的網站用 `tools/export-static.mjs` 爬成靜態檔，再由 `tools/deploy-pages.sh` 在 `site-export/` 重新 `git init` 一個 gh-pages 分支並強制推上去。每次推的都是全新的分支，不會保留上一版的任何檔案
- `export-static.mjs` 的子路徑是第三個參數，預設 `/JenJenKnits`；站內連結都會加上這個子路徑；社群分享照片的絕對網址用 `content="https?://[^"/]+${base}..."` 比對
- 絕對網址（og、canonical、JSON-LD、sitemap、robots.txt）都由 `Site:BaseUrl` 組成（`SiteOptions.Absolute`），目前是 `https://cienlin.github.io/JenJenKnits`
- GitHub Pages 用分支發布時，自訂網域記在發布分支根目錄的 `CNAME` 檔；在 repo 設定填網域，也只是由 GitHub 幫你 commit 這個檔案
- 測試的 `SiteFactory` 沒有覆寫 `Site:BaseUrl`，所以 `SeoTests` 讀的是 `appsettings.json` 的正式值，並在測試裡寫死同一個網址
- 網域在 Namecheap、使用 BasicDNS（免費）

## Goals / Non-Goals

**Goals:**

- `Site:BaseUrl` 是對外網址的唯一來源：改這一個設定，輸出的子路徑與 `CNAME` 就跟著對
- 切換過程中網站不會打不開

**Non-Goals:**

- IPv6（AAAA 紀錄）：GitHub Pages 支援，但不是必要，之後需要再加
- 品牌信箱（Namecheap 的信件轉寄）
- Google Search Console 登錄（另一個待辦，切換完成後用新網域做）

## Decisions

### D1：主要網址用根網域 `jhcraftstudio.com`，`www` 轉過來

使用者決定。根網域較短，放在 IG 個人檔案與名片上較乾淨。DNS 同時設定根網域（A 紀錄）與 `www`（CNAME）時，GitHub Pages 會自動把 `www` 轉到根網域，不需要寫程式。

替代方案：以 `www` 為主。之後若改用 Cloudflare 等服務設定稍有彈性，但目前用不到。

### D2：子路徑與 `CNAME` 從執行中網站的 `robots.txt` 推得

`export-static.mjs` 開始爬之前先抓 `/robots.txt`，從 `Sitemap:` 那一行的絕對網址（由 `Site:BaseUrl` 組成）取出：

- 子路徑：網址的 path 去掉 `/sitemap.xml`（`https://jhcraftstudio.com` → 空字串；`https://cienlin.github.io/JenJenKnits` → `/JenJenKnits`）
- 網域：主機名稱不是 `*.github.io` 時，在輸出根目錄寫入 `CNAME`（內容為主機名稱），和 `.nojekyll` 放在一起
- 社群分享照片的比對改用完整的 `Site:BaseUrl` 開頭，不再用「任何主機＋子路徑」；子路徑為空時，舊的寫法會比對到所有外部網址

移除子路徑參數，用法變成 `node tools/export-static.mjs [輸出資料夾] [網站網址]`。`deploy-pages.sh` 只負責把輸出推上去，不知道網域。

為什麼從 `robots.txt` 抓：它反映網站實際輸出的網址，包含環境變數或 user-secrets 覆寫的值，而且已經是絕對網址、格式固定。

替代方案：

- 在 `deploy-pages.sh` 寫死 `echo jhcraftstudio.com > CNAME`：最簡單，但網域會同時出現在 `appsettings.json` 和腳本裡，改一邊忘了另一邊時，輸出的網址和網域會對不上
- 保留子路徑參數：同樣要和 `Site:BaseUrl` 手動保持一致；不一致時社群分享照片會漏輸出
- 讀 `appsettings.json` 檔案：看不到環境變數與 user-secrets 的覆寫

### D3：測試自己指定 `Site:BaseUrl`

`SiteFactory` 用 `UseSetting("Site:BaseUrl", ...)` 指定一個測試用網址，`SeoTests` 改用同一個常數。網址刻意帶子路徑（例如 `https://example.test/shop`），繼續涵蓋「對外網址有子路徑」的組法。之後換網域、搬主機，測試都不用改。

替代方案：只把 `SeoTests` 裡的常數改成新網域。可行，但下次換網址又要改測試，測試也不再涵蓋子路徑。

## Risks / Trade-offs

- [切換順序錯誤：DNS 還沒生效就推上 `CNAME`，GitHub 會把舊網址轉到還打不開的新網域，網站中斷] → 先設定 DNS、確認生效，才部署含 `CNAME` 的版本（見 Migration Plan）
- [Namecheap 新網域預設有停車頁紀錄（`www` 指向 parkingpage、根網域的 URL Redirect），會和 GitHub 的紀錄衝突] → 設定前先刪掉預設紀錄
- [網域指向 GitHub 但還沒設定好之前，別人可能把這個網域綁到他自己的 GitHub Pages（網域接管）] → 第一步先在 GitHub 帳號驗證網域（TXT 紀錄）
- [憑證發好之前只能用 http；`Site:BaseUrl` 已是 https，這段期間分享預覽可能抓不到] → 時間短（通常數分鐘到一小時），憑證發好後立刻勾 Enforce HTTPS；切換完再用 LINE、Facebook 的分享偵錯工具重新抓取
- [LINE、Facebook、Google 快取舊網址] → GitHub 對舊網址做永久轉址；分享偵錯工具重新抓取；Search Console 之後直接用新網域登錄
- [GitHub Pages 不允許用來經營網路商店] → 本站是品牌展示、購買導到賣貨便；若有疑慮，同一份輸出可改放 Cloudflare Pages（`add-deployment/design.md` 已記錄）
- [網域忘了續約被別人搶走] → 網域與 Domain Privacy 已開自動續約

## Migration Plan

1. **DNS 與驗證**（使用者手動，部署前）
   1. GitHub 帳號 Settings → Pages → Add a domain：輸入 `jhcraftstudio.com`，到 Namecheap 加上 GitHub 給的 TXT 紀錄，回 GitHub 按 Verify
   2. Namecheap → Domain List → Manage → Advanced DNS：刪掉預設的停車頁紀錄；新增根網域 `@` 的 4 筆 A 紀錄（`185.199.108.153`、`185.199.109.153`、`185.199.110.153`、`185.199.111.153`），以及 `www` 的 CNAME 指向 `cienlin.github.io.`
   3. 用 `nslookup jhcraftstudio.com` 確認已回傳 GitHub 的 IP
2. **程式**：完成 D2、D3 與 `Site:BaseUrl` 的修改，`dotnet test` 通過
3. **部署**：執行 `bash tools/deploy-pages.sh`；確認輸出根目錄有 `CNAME`，站內連結沒有 `/JenJenKnits`
4. **GitHub Pages 設定**：repo Settings → Pages，確認 Custom domain 顯示 `jhcraftstudio.com`、DNS 檢查通過；憑證發好後勾 Enforce HTTPS
5. **驗收**：`https://jhcraftstudio.com` 正常；`www` 與舊網址都轉到新網域；`/robots.txt`、`/sitemap.xml` 在根目錄、內容是新網域；商品頁的 og 與 canonical 是新網域；用 390px 寬度實際操作首頁、作品列表、商品頁

**回復**：revert 這次的 commit 再執行 `deploy-pages.sh`，輸出不含 `CNAME`、網址回到 `cienlin.github.io/JenJenKnits`；repo Settings → Pages 移除 Custom domain。DNS 可以保留不動。
