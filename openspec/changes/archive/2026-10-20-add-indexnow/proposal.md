# Proposal

## Why

ChatGPT 搜尋、Copilot 很大程度依賴 Bing 的索引。2026-10-20 對照 Bing 網站管理員指南，網站大致都符合，最主要的缺口是沒有 IndexNow：指南在「探索網址」「網址變動時盡快通知」「內容保持最新」「刪除網址」四處都要求用 IndexNow 主動通知 Bing。現在官網改了內容（例如 10-20 上線的常見問題），只能等 Bing 自己來抓，或手動到 Bing Webmaster Tools 一頁一頁要求建立索引。

## What Changes

- 產生一組 IndexNow 金鑰，靜態輸出時在網站根目錄放金鑰檔（`/{金鑰}.txt`），讓 Bing 確認通知來自網站擁有者
- `tools/deploy-pages.sh` 部署時，和目前線上的版本比對，找出新增、更新、刪除的網址；等 GitHub Pages 實際換成新版後，把這些網址送給 IndexNow
- 只送 sitemap 裡的正式網址（分類篩選頁等非正式網址不送）；比對時忽略每次部署都會變、但不是內容的部分（頁尾的 commit 版本、CSS／JS 檔名裡的版本指紋），沒有內容變動就不送
- 送出失敗（網路、金鑰驗證、等太久）只顯示警告與要手動提交的網址，不影響部署本身
- 可以只看會送哪些網址、不真的送出（預覽模式），方便檢查

不含：Google（不支援 IndexNow，仍靠 sitemap 與 Search Console）；首頁與作品列表在 sitemap 補 `lastmod`；GitHub Actions 自動部署（之後另開 change 時沿用這次的腳本）。

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

（無）。這次只改靜態輸出與部署工具，網站程式的行為不變；和 `use-custom-domain` 一樣設 `skip_specs: true`。

## Impact

- 新增：`tools/indexnow.mjs`（比對、等上線、送出）、金鑰檔（存在 repo 裡；金鑰本來就會公開在網站上，不是密碼）
- 修改：`tools/export-static.mjs`（輸出金鑰檔）、`tools/deploy-pages.sh`（推上去之前取得線上版本、推上去之後呼叫 IndexNow）
- 文件：README「更新展示網站」段落說明部署後會自動通知 Bing、怎麼預覽與手動補送
- 外部服務：`https://api.indexnow.org/indexnow`（Bing、Yandex、Naver 等共用）；第一次送出後在 Bing Webmaster Tools 的 IndexNow 頁面確認有收到
