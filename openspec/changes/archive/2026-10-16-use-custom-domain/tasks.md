# Tasks

## 1. DNS 與網域驗證（使用者手動，可和第 2～4 組同時進行）

- [x] 1.1 GitHub 帳號 Settings → Pages → Add a domain 輸入 `jhcraftstudio.com`，在 Namecheap Advanced DNS 加上 GitHub 給的 TXT 紀錄；回 GitHub 按 Verify，顯示已驗證
- [x] 1.2 Namecheap Advanced DNS 刪掉預設的停車頁紀錄（`www` 的 parkingpage CNAME、根網域的 URL Redirect）；新增 `@` 的 4 筆 A 紀錄（`185.199.108.153`、`185.199.109.153`、`185.199.110.153`、`185.199.111.153`）與 `www` 的 CNAME 指向 `cienlin.github.io.`；畫面上只剩這 5 筆加上驗證用的 TXT
- [x] 1.3 `nslookup jhcraftstudio.com` 回傳上面 4 個 IP、`nslookup www.jhcraftstudio.com` 指向 `cienlin.github.io`

## 2. 測試不依賴正式網址（D3）

- [x] 2.1 `SiteFactory` 用 `UseSetting("Site:BaseUrl", ...)` 指定帶子路徑的測試網址（例如 `https://example.test/shop`），`SeoTests` 改用同一個常數；`dotnet test` 全部通過，`SeoTests` 不再出現 `cienlin.github.io`

## 3. 靜態輸出從 `Site:BaseUrl` 推得子路徑與 `CNAME`（D2）

- [x] 3.1 `export-static.mjs` 先抓 `/robots.txt`，從 `Sitemap:` 網址取得子路徑；移除子路徑參數；社群分享照片改用完整 `Site:BaseUrl` 比對；更新檔案開頭的用法說明。驗證：`Site:BaseUrl` 仍是 `https://cienlin.github.io/JenJenKnits` 時輸出到暫存資料夾，頁數、照片數與 `missing links: 0` 和修改前相同，站內連結仍有 `/JenJenKnits`，且沒有 `CNAME`
- [x] 3.2 主機名稱不是 `*.github.io` 時在輸出根目錄寫入 `CNAME`。驗證：用環境變數 `Site__BaseUrl=https://jhcraftstudio.com` 啟動網站後輸出，`CNAME` 內容為 `jhcraftstudio.com`、站內連結沒有 `/JenJenKnits`、og:image 照片都有輸出、`missing links: 0`

## 4. 切換對外網址

- [x] 4.1 `appsettings.json` 的 `Site:BaseUrl` 改為 `https://jhcraftstudio.com`；`dotnet test` 全部通過；本機 `/robots.txt` 的 Sitemap 與商品頁的 canonical 都是新網域
- [x] 4.2 `tools/deploy-pages.sh` 的完成訊息改顯示新網址；README（展示網站網址、「更新展示網站」一節）與 `docs/status.md`（展示網站網址）改為 `https://jhcraftstudio.com`；`grep -rn "cienlin.github.io"` 只剩封存的 change、`add-deployment` 的紀錄與 DNS 的 CNAME 目標

## 5. 部署與 GitHub Pages 設定（需第 1 組完成）

- [x] 5.1 commit 並 push 第 2～4 組的修改；執行 `bash tools/deploy-pages.sh`；gh-pages 分支根目錄有 `CNAME`、`index.html` 的站內連結沒有 `/JenJenKnits`
- [x] 5.2 （使用者）repo Settings → Pages：Custom domain 顯示 `jhcraftstudio.com`、DNS check 通過；憑證發好後勾 Enforce HTTPS，`http://jhcraftstudio.com` 會轉到 https

## 6. 上線驗收

- [x] 6.1 `https://jhcraftstudio.com` 首頁、作品列表、分類、商品頁正常；`https://www.jhcraftstudio.com` 與 `https://cienlin.github.io/JenJenKnits/` 都轉到新網域；`/robots.txt`、`/sitemap.xml`、`/404.html` 在根目錄且內容為新網域
- [x] 6.2 用 390px 寬度實際操作首頁、作品列表、商品頁（照片切換、配色模擬器），截圖確認版面與連結正常
- [x] 6.3 （使用者）分享預覽：在 LINE 貼 `http://jhcraftstudio.com/`，卡片顯示首頁標題、介紹與預設分享照片（LINE Page Poker 已於 2025-06 停止服務；新網址沒有舊快取，Facebook 偵錯工具略過，商品頁的 og 已在線上確認）；IG 個人檔案的連結欄位改為 `https://jhcraftstudio.com`；`docs/status.md` 的待辦同步更新
