# Tasks

## 1. 金鑰檔（D1）

- [x] 1.1 產生 32 個十六進位字元的金鑰，存在 `tools/indexnow-key.txt`；`export-static.mjs` 在輸出資料夾根目錄寫 `{金鑰}.txt`（內容是金鑰）。網站執行中時跑 `node tools/export-static.mjs`，確認檔案存在、內容正確，原本的「missing links: 0」檢查仍通過

## 2. 找出有變動的網址（D2、D5）

- [x] 2.1 新增 `tools/indexnow.mjs` 的比對：讀新舊 `sitemap.xml`、網址對應到檔案、去掉頁尾版本與 CSS／JS 檔名指紋後比較，分成新增／更新／刪除；沒有上一版時列出全部。用 `node --test tools/indexnow.test.mjs` 跑測試，涵蓋：只差頁尾版本或 CSS／JS 指紋不算更新、圖片指紋不同算更新、內容不同算更新、新增與刪除、不在 sitemap 的頁面（分類篩選）不列、沒有上一版時全部列出
- [x] 2.2 `deploy-pages.sh`：建好 `site-export/` 的 git 之後 `git fetch --depth 1` 目前的 `gh-pages` 當上一版（沒有就略過）；加 `--dry-run`，只列出會通知的網址與種類，不推、不送。執行 `bash tools/deploy-pages.sh --dry-run`，預期列出首頁與作品列表為「更新」（10-20 的封面照片 alt 與「飲料提網」還沒部署），且 `git ls-remote origin gh-pages` 前後不變
- [x] 2.3 README「更新展示網站」段落補上：部署後會自動通知 IndexNow、`--dry-run` 怎麼用、金鑰檔是公開的；照 README 的指令實際跑一次確認寫的是對的

## 3. 等上線並送出（D3、D4）

- [x] 3.1 `indexnow.mjs` 加上「等 GitHub Pages 換成新版」（每 10 秒、最多 5 分鐘，帶不重複的查詢字串避開快取；只有刪除時等 404）與送出（`https://api.indexnow.org/indexnow`，`host`、`key`、`keyLocation`、`urlList`）；依回應碼與錯誤印出 D4 的訊息，失敗時列出要手動提交的網址，程式一律以成功結束；網址有子路徑時略過並說明。`deploy-pages.sh` 推上去之後呼叫，並加保護不讓它影響部署結果。`indexnow.test.mjs` 補上回應碼對應訊息的測試，`node --test tools/indexnow.test.mjs` 通過

## 4. 上線與確認

- [x] 4.1 真正部署：線上 `https://jhcraftstudio.com/{金鑰}.txt` 回 200 且內容是金鑰；IndexNow 回 200 或 202，送出的網址和 2.2 預覽的一樣（2026-10-20：和 add-page-dates-author 一起部署 `2266872`，金鑰檔 200 且內容正確，4 個網址回 HTTP 202）
- [x] 4.2 不改內容再部署一次，確認顯示「沒有內容變動」，沒有送出（2026-10-20 確認）
- [x] 4.3 `docs/status.md`：「已完成」加上這個 change、「之後」移除 IndexNow；待辦加「幾天後到 Bing Webmaster Tools 的 IndexNow 頁面確認收到網址」
