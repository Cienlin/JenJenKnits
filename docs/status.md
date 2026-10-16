# 專案進度（最後更新：2026-10-16）

給下一次開工（不論哪台電腦、哪個 Claude）快速接手用。開工、收工請用 `/start-work`、`/end-work`。

## 已完成

| 項目 | 紀錄 |
|---|---|
| 展示網站（首頁、作品列表、商品頁、配色模擬器） | `openspec/changes/archive/2026-10-08-add-showcase-mvp` |
| 前台改版「編織誌」、動態原則（D11） | `openspec/changes/archive/2026-10-08-redesign-editorial` |
| 後台第一階段：SQLite、登入、編輯商品文字資料 | `openspec/changes/archive/2026-10-08-add-admin-backend` |
| SEO：社群分享預覽、canonical、JSON-LD、sitemap、robots | `openspec/changes/archive/2026-10-08-add-seo-metadata` |
| 品牌改名「織物製造所」、IG 串接（@jh.craft.studio） | commit `b09a10a`、`9ce2f7c` |
| 官網改用自己的網域 `jhcraftstudio.com`（GitHub Pages、HTTPS、舊網址與 www 自動轉址） | `openspec/changes/archive/2026-10-16-use-custom-domain` |
| 後台列表顯示「待補：」，提醒還沒填的欄位 | commit `ebbb462` |

- 目前規格：`openspec/specs/product-showcase`、`openspec/specs/product-admin`
- 23 個自動化測試（`dotnet test`）
- 只用 `main` 分支；`gh-pages` 由 `tools/export-static.mjs` 產生，不要手改
- 官網：https://jhcraftstudio.com （GitHub Pages 靜態輸出；改內容後要重新輸出才會更新，步驟見 README）

## 進行中

**第三階段：部署＋CI/CD**
- 官網先用 GitHub Pages＋自己的網域（2026-10-16 已完成）；後台仍只在本機使用
- 之後上主機用 Azure App Service（Linux B1），目前延後；什麼時候該搬（想用手機改資料、要加伺服器端功能、找工作時）、費用、注意事項都記在 `openspec/changes/add-deployment/design.md`

## 之後

- GitHub Actions：push 後自動測試、輸出並部署 GitHub Pages，取代手動 `tools/deploy-pages.sh`（另開 change）
- 後台第二階段：照片上傳、排序、刪除商品（建議部署完成後再做）
- 照片產生較小的版本（手機載入速度）

## 不用寫程式的待辦

- 跟貞貞收集泡芙小花提網的尺寸、各商品的賣貨便連結，到後台補上
- （暫緩）網格托特包的價格、材質、尺寸、賣貨便連結：2026-10-16 先從前台下架，目前不是主力
- 登錄 Google Search Console（用 `jhcraftstudio.com`）並提交 sitemap
- Namecheap：15 天內點 ICANN 驗證信（2026-10-16 購買）、帳號開兩步驟驗證、確認網域自動續約已開

## 已討論過的決定（不在 OpenSpec 裡的）

- **商品頁不做頁籤**（2026-10-07）：維持「01 故事、02 可選顏色」的章節長頁。頁面長約 5.4 屏，主要是故事配圖與色卡照片造成；若之後嫌長，優先考慮「縮短內容」：故事配圖改小、色卡照片預設收起
- **前端 skill 評估**（2026-10-07）：最推薦 impeccable（audit / polish；animate 是其中的指令）；frontend-design 已用過；design-taste-frontend、brandkit、canvas-design 目前不需要。不要同時裝多個設計 skill，規則可能衝突，也很吃 token
- **impeccable animate 檢查**（2026-10-07）：已依其規則調整動態，原則記在 `openspec/changes/archive/2026-10-08-redesign-editorial/design.md` D11
- **Stitch AI**：適合之後設計新頁面時快速產生多版視覺，挑好後交給 Claude 實作進 Razor Pages；風格已定，短期不需要
- **SEO**：不需要另外裝 SEO skill；剩下的是「照片產生較小的版本」，與照片管理一起做
- **主機**：台灣的「虛擬主機」（PChome 等）多為 PHP／WordPress 共用主機、年約、無法 CI/CD，不適合；網域與主機分開買
- **官網先用 GitHub Pages，Azure 延後**（2026-10-16）：客源與 IG 還在起步，先不付主機費（Azure 約 6,000 元／年，GitHub Pages 只有網域費約 490 元／年）。上 Azure 時選 Linux：本專案是 ASP.NET Core，跨平台；Windows B1 貴約 4 倍。細節見 `openspec/changes/add-deployment/design.md`
- **網域**（2026-10-16）：`jhcraftstudio.com` 在 Namecheap 購買，登記在個人名下（沒勾「代表公司」），有免費 Domain Privacy，DNS 用 BasicDNS。Namecheap 推銷的主機、SSL、PremiumDNS、信箱都不需要；要品牌信箱時用免費的信件轉寄
- **分享預覽測試**：LINE Page Poker 已在 2025-06 停止服務；改在 LINE 把網址傳給自己看卡片，Facebook（也涵蓋 IG、Threads）用分享偵錯工具
- **手機實測**：公司電腦的 Edge 被公司政策關閉遠端偵錯（`RemoteDebuggingAllowed=0`），自動化點擊與截圖改用無頭 Chrome

## 品牌與社群素材

見 `docs/brand/README.md`：IG 帳號、頭貼 5 種版本、自介文字、泡芙小花提網貼文。

## 開工前提醒

- 公司電腦：repo 在 `D:\jenjenknits`（從這裡開 Claude Code 與 `JenJenKnits.sln`）；樣稿、舊版 MVC、Stitch 匯出在 `D:\jenjenknits-workspace`
- 先 `git pull`；後台改資料後要 commit 並 push `JenJenKnits/App_Data/jenjenknits.db`（部署前的過渡做法）
- 每台電腦要各自用 `dotnet run -- hash-password` + `dotnet user-secrets` 設定管理員帳號
- 網站執行中時 `dotnet build` / `dotnet test` 會因為 exe 被鎖住失敗，先關掉網站
- `bash tools/deploy-pages.sh` 會直接更新正式官網 `https://jhcraftstudio.com`。自訂網域靠輸出裡的 `CNAME` 檔（由 `Site:BaseUrl` 產生）維持，不要在 GitHub repo Settings → Pages 手動改 Custom domain
- 偏好：繁體中文回答；UI 要用手機實際操作檢查（390px）；前台維持編織誌風格
