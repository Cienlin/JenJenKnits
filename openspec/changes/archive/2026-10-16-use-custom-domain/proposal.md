# Proposal

## Why

已買好網域 `jhcraftstudio.com`（2026-10-16），決定在客源與 IG 穩定前先用 GitHub Pages 當官網、Azure 延後（見 `add-deployment/design.md`「調整」一節）。IG 個人檔案、貼文、名片上要放的網址應該從第一天就是最終網址；之後搬主機只要改 DNS，網址與搜尋排名都能保留。另外網站目前在子路徑 `/JenJenKnits/`，搜尋引擎不會讀子路徑下的 `robots.txt`，移到網域根目錄後才會生效。

## What Changes

- 網站對外網址（`Site:BaseUrl`）改為 `https://jhcraftstudio.com`：社群分享預覽、canonical、JSON-LD、sitemap、robots.txt 的絕對網址都跟著改
- 靜態輸出改為輸出到網域根目錄（不再加 `/JenJenKnits` 子路徑），並在輸出中產生 GitHub Pages 用的 `CNAME` 檔；子路徑與網域都從 `Site:BaseUrl` 推得，不再另外指定
- 修正：`tools/deploy-pages.sh` 每次重建 gh-pages 分支，會洗掉 GitHub 記錄自訂網域的 `CNAME` 檔
- 測試改為自己指定 `Site:BaseUrl`，不再依賴正式設定檔裡的網址
- `www.jhcraftstudio.com` 與舊網址 `cienlin.github.io/JenJenKnits` 都會自動轉到 `jhcraftstudio.com`（GitHub 提供，不需寫程式）
- 使用者手動步驟：Namecheap DNS 設定、GitHub 網域驗證、GitHub Pages 自訂網域與 Enforce HTTPS
- README、`docs/status.md` 的網址與步驟更新

不含：GitHub Actions 自動部署（另開 change）、Azure 部署（`add-deployment`，延後）。

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

（無）。`product-showcase` 的社群分享、canonical、sitemap、robots.txt 需求本來就寫「以網站對外網址組成的絕對網址」，這次只改設定值與輸出工具，需求不變，因此 `.openspec.yaml` 設 `skip_specs: true`。

## Impact

- 設定：`JenJenKnits/appsettings.json`（`Site:BaseUrl`）
- 工具：`tools/export-static.mjs`（子路徑與 `CNAME` 由 `Site:BaseUrl` 推得）、`tools/deploy-pages.sh`（完成訊息）
- 測試：`JenJenKnits.Tests/SiteFactory.cs`、`JenJenKnits.Tests/SeoTests.cs`
- 文件：`README.md`、`docs/status.md`
- 外部：Namecheap DNS、GitHub 帳號的網域驗證、repo 的 Pages 設定
- 已分享出去的舊連結會被 GitHub 轉到新網址；LINE、Facebook 等可能快取舊的分享預覽，需用分享偵錯工具重新抓取
