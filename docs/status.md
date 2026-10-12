# 專案進度（最後更新：2026-10-12）

給下一次開工（不論哪台電腦、哪個 Claude）快速接手用。開工、收工請用 `/start-work`、`/end-work`。

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

## 已討論過的決定（不在 OpenSpec 裡的）

- **商品頁不做頁籤**（2026-10-07）：維持「01 故事、02 可選顏色」的章節長頁。頁面長約 5.4 屏，主要是故事配圖與色卡照片造成；若之後嫌長，優先考慮「縮短內容」：故事配圖改小、色卡照片預設收起
- **前端 skill 評估**（2026-10-07）：最推薦 impeccable（audit / polish；animate 是其中的指令）；frontend-design 已用過；design-taste-frontend、brandkit、canvas-design 目前不需要。不要同時裝多個設計 skill，規則可能衝突，也很吃 token
- **impeccable animate 檢查**（2026-10-07）：已依其規則調整動態，原則記在 `openspec/changes/archive/2026-10-08-redesign-editorial/design.md` D11
- **Stitch AI**：適合之後設計新頁面時快速產生多版視覺，挑好後交給 Claude 實作進 Razor Pages；風格已定，短期不需要
- **SEO**：不需要另外裝 SEO skill；剩下的是「照片產生較小的版本」，與照片管理一起做
- **主機**：台灣的「虛擬主機」（PChome 等）多為 PHP／WordPress 共用主機、年約、無法 CI/CD，不適合；網域與主機分開買

## 品牌與社群素材

見 `docs/brand/README.md`：IG 帳號、頭貼 5 種版本、自介文字、泡芙小花提網貼文。

## 開工前提醒

- 公司電腦：repo 在 `D:\jenjenknits`（從這裡開 Claude Code 與 `JenJenKnits.sln`）；樣稿、舊版 MVC、Stitch 匯出在 `D:\jenjenknits-workspace`
- 先 `git pull`；後台改資料後要 commit 並 push `JenJenKnits/App_Data/jenjenknits.db`（部署前的過渡做法）
- 每台電腦要各自用 `dotnet run -- hash-password` + `dotnet user-secrets` 設定管理員帳號
- 網站執行中時 `dotnet build` / `dotnet test` 會因為 exe 被鎖住失敗，先關掉網站
- 偏好：繁體中文回答；UI 要用手機實際操作檢查（390px）；前台維持編織誌風格
