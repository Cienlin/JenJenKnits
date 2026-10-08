# 專案進度（最後更新：2026-10-08）

給下一次開工（不論哪台電腦、哪個 Claude）快速接手用。

## 已完成

| 項目 | 紀錄 |
|---|---|
| 展示網站（首頁、作品列表、商品頁、配色模擬器） | `openspec/changes/archive/2026-10-08-add-showcase-mvp` |
| 前台改版「編織誌」、動態原則（D11） | `openspec/changes/archive/2026-10-08-redesign-editorial` |
| 後台第一階段：SQLite、登入、編輯商品文字資料 | `openspec/changes/archive/2026-10-08-add-admin-backend` |
| SEO：社群分享預覽、canonical、JSON-LD、sitemap、robots | `openspec/changes/archive/2026-10-08-add-seo-metadata` |
| 品牌改名「織物製造所」、IG 串接（@jh.craft.studio） | commit `b09a10a`、`9ce2f7c` |

- 目前規格：`openspec/specs/product-showcase`、`openspec/specs/product-admin`
- 23 個自動化測試（`dotnet test`）
- 只用 `main` 分支；`gh-pages` 由 `tools/export-static.mjs` 產生，不要手改
- 展示網站：https://cienlin.github.io/JenJenKnits/ （改內容後要重新輸出才會更新，步驟見 README）

## 進行中

**第三階段：部署到主機＋CI/CD**（探索中，主機尚未決定）
→ 討論紀錄與待決定事項：`openspec/changes/add-deployment/design.md`

## 之後

- 後台第二階段：照片上傳、排序、刪除商品（建議部署完成後再做）
- 照片產生較小的版本（手機載入速度）

## 不用寫程式的待辦

- 跟貞貞收集網格托特包的價格、材質、尺寸、故事、賣貨便連結，到後台補上
- 買網域（考慮 `jhcraftstudio.tw` 或 `jhcraft.tw`）
- 登錄 Google Search Console 並提交 sitemap
- 用 LINE 或 Facebook 分享偵錯工具測試分享預覽
- IG 個人檔案的連結欄位填網站網址

## 開工前提醒

- 先 `git pull`；後台改資料後要 commit 並 push `JenJenKnits/App_Data/jenjenknits.db`（部署前的過渡做法）
- 每台電腦要各自用 `dotnet run -- hash-password` + `dotnet user-secrets` 設定管理員帳號
- 網站執行中時 `dotnet build` / `dotnet test` 會因為 exe 被鎖住失敗，先關掉網站
- 偏好：繁體中文回答；UI 要用手機實際操作檢查（390px）；前台維持編織誌風格
