# 專案進度（最後更新：2026-10-20）

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
| SEO 網站名稱：首頁 WebSite 結構化資料（「織物製造所」／JH Craft Studio），Schema Markup Validator 0 錯誤 | `openspec/changes/archive/2026-10-16-add-site-name` |
| IndexNow：`deploy-pages.sh` 部署後自動通知 Bing 有變動的網址（只送 sitemap 裡內容真的變了的；`--dry-run` 預覽；金鑰 `tools/indexnow-key.txt`，公開的） | `openspec/changes/add-indexnow`（2026-10-20 上線，第一次回 202） |
| 結構化資料：品牌改 OnlineStore、首頁 WebPage＋FAQPage、商品頁 ItemPage；作者 meta 與 author；頁尾版權頁顯示更新日期（dateModified，台灣時間） | `openspec/changes/add-page-dates-author`（2026-10-20 上線，Validator 0 錯誤） |
| AEO：首頁「03 常見問題」6 題＋FAQPage、品牌別名 JH Craft Studio、首頁標題與描述（2026-10-20 上線，`f842c28`；Schema Markup Validator 0 錯誤） | `openspec/changes/archive/2026-10-20-add-faq` |

- 目前規格：`openspec/specs/product-showcase`、`openspec/specs/product-admin`
- 35 個自動化測試（`dotnet test`）＋ IndexNow 腳本 11 個（`node --test tools/indexnow.test.mjs`）
- 只用 `main` 分支；`gh-pages` 由 `tools/export-static.mjs` 產生，不要手改
- 官網：https://jhcraftstudio.com （GitHub Pages 靜態輸出；改內容後要重新輸出才會更新，步驟見 README）

## 進行中

**IndexNow**（`openspec/changes/add-indexnow`）與**頁面作者與更新日期**（`openspec/changes/add-page-dates-author`）：2026-10-20 都已上線、任務全部完成，等歸檔（`/opsx:archive`）

**第三階段：部署＋CI/CD**
- 官網先用 GitHub Pages＋自己的網域（2026-10-16 已完成）；後台仍只在本機使用
- 之後上主機用 Azure App Service（Linux B1），目前延後；什麼時候該搬（想用手機改資料、要加伺服器端功能、找工作時）、費用、注意事項都記在 `openspec/changes/add-deployment/design.md`

## 之後

- GitHub Actions：push 後自動測試、輸出並部署 GitHub Pages，取代手動 `tools/deploy-pages.sh`（另開 change）
- **品牌故事**（另開 change，等貞貞的素材；也解決 ultralab 的「關於頁面連結」「專業度 E-E-A-T」）：品牌故事與專業度（鉤織年資、幫親友做過的件數，只寫真的；沒有擺過市集、教過課，不提）、頁尾加版權聲明；已決定（2026-10-20）：只寫品牌名、不放創作者名字與新照片、JH 只說是兩位創作者名字的字首、聯絡先只用 IG（細節見 `docs/brand/about-questions.md`）。聯絡方式 2026-10-20 已先用 IG（頁尾「聯絡」列＋ContactPoint）。注意：2026-10-05 編織誌改版時移除了首頁的「關於」區塊，規格寫著「首頁不顯示品牌故事」，要一起改；放首頁章節還是獨立頁面到時再決定
- 商品頁加麵包屑的結構化資料（BreadcrumbList），畫面上已經有麵包屑
- 後台第二階段：照片上傳、排序、刪除商品（建議部署完成後再做）；照片的 alt 改成描述性文字（現在是「泡芙小花提網 照片 2」）
- 照片產生較小的版本（手機載入速度）

## 不用寫程式的待辦

- 跟貞貞收集泡芙小花提網的尺寸、各商品的賣貨便連結，到後台補上
- 問貞貞品牌故事、經歷、材質與要不要露出名字：訊息（可直接貼 LINE）與寫法注意事項在 `docs/brand/about-questions.md`，回覆也記在那裡，給之後的「品牌故事」change 用
- 用 ultralab.tw 重新掃首頁，看 AEO 分數（上次 46），特別是 2026-10-20 修的「Content type schema」「Author information」「Publication date」三項；結果記在這裡。內容類型若仍未通過，不為了分數加 Article 等不準確的類型（見「已討論過的決定」）
- 幾天後到 Bing Webmaster Tools 左邊選單的「IndexNow」頁面，確認收到 2026-10-20 送出的 4 個網址（第一次回 202 是金鑰驗證中）
- 開賣（第一件商品填上賣貨便連結）時，改常見問題第 4 題「賣場開賣前，可以私訊 Instagram 訂購」
- 商品或購買方式改變時（例如網格托特包重新上架、開賣），改首頁描述（`_Layout.cshtml`，描述寫到泡芙小花提網、花花杯墊、一週完成、7-11 取貨）；首頁標題只有品牌與特色，不用改；作品列表的標題與描述會自動列出目前的分類
- 約 2026-11-10（上線 2～3 週後）：看 Search Console「成效 → 查詢」大家實際搜什麼字，再決定標題要不要調整（商品叫法已在 2026-10-20 統一為「飲料提網」，見「已討論過的決定」）
- （暫緩）網格托特包的價格、材質、尺寸、賣貨便連結：2026-10-16 先從前台下架，目前不是主力
- 一週後（約 2026-10-23）在 Google 搜尋 `site:jhcraftstudio.com`，或看 Search Console 的「網頁」報表，確認已收錄（2026-10-16 已用「網域」類型登錄 Search Console、提交 sitemap，並對首頁與兩件商品頁要求建立索引）。順便看搜尋結果是否顯示網站名稱「織物製造所」（2026-10-16 上線，生效需幾天到幾週，Google 不保證採用）。Bing 也一起看：搜尋 `site:jhcraftstudio.com` 或在 Bing Webmaster Tools 重做 URL Inspection
- Namecheap：15 天內（2026-10-31 前）點 ICANN 驗證信（2026-10-16 購買）、帳號開兩步驟驗證、確認網域自動續約已開

## 已討論過的決定（不在 OpenSpec 裡的）

- **商品頁不做頁籤**（2026-10-07）：維持「01 故事、02 可選顏色」的章節長頁。頁面長約 5.4 屏，主要是故事配圖與色卡照片造成；若之後嫌長，優先考慮「縮短內容」：故事配圖改小、色卡照片預設收起
- **前端 skill 評估**（2026-10-07）：最推薦 impeccable（audit / polish；animate 是其中的指令）；frontend-design 已用過；design-taste-frontend、brandkit、canvas-design 目前不需要。不要同時裝多個設計 skill，規則可能衝突，也很吃 token
- **impeccable animate 檢查**（2026-10-07）：已依其規則調整動態，原則記在 `openspec/changes/archive/2026-10-08-redesign-editorial/design.md` D11
- **Stitch AI**：適合之後設計新頁面時快速產生多版視覺，挑好後交給 Claude 實作進 Razor Pages；風格已定，短期不需要
- **SEO**：不需要另外裝 SEO skill；剩下的是「照片產生較小的版本」，與照片管理一起做
- **搜尋「織物製造所」**（2026-10-16）：這在日文是一般名詞（織布工廠），目前搜尋結果幾乎都是日本的紡織工廠。台灣用戶用中文搜尋時日本網站權重較低，但也觀察「織物製造所 鉤織」、「jh craft studio」。做法：Search Console（已完成）、網站名稱結構化資料（`add-site-name`）、外部連結（IG 已放；賣貨便賣場介紹欄、其他社群也放官網網址），貼文與自介把「織物製造所」和「手鉤織」寫在一起。網站名稱只影響搜尋結果怎麼顯示，不直接影響排名
- **Search Console**（2026-10-16）：用「網域」類型登錄，只能用 DNS 驗證，「新增多種驗證方法」的提示可以不理；網址審查顯示「Google 可為網址建立索引」代表技術上沒問題，但不代表已收錄；已要求建立索引的網址不用重複提交
- **Bing Webmaster Tools**（2026-10-16，AEO 第一步）：ChatGPT 搜尋、Copilot 很大程度依賴 Bing 的索引，所以也要登錄。用「從 Search Console 匯入」加入網站（自動驗證，不用加 DNS 記錄；不要移除 Bing 對 Google 帳號的存取權，Bing 會定期用它重新驗證）。首頁與兩件商品頁 URL Inspection 顯示「Not discovered」是新網站的正常狀態，已 Request indexing，不用重複送出；用 Bingbot 身分請求各頁都是 200
- **AEO 下一步**（2026-10-16 討論）：加「常見問題」區塊（製作天數、保養、尺寸、購買與運費；內容要先跟貞貞確認）＋品牌資料補 alternateName「JH Craft Studio」，另開 change，medium effort。FAQPage 結構化資料 Google 只對政府／醫療網站顯示搜尋結果樣式，主要價值是頁面上看得到的問答文字。`llms.txt` 目前沒有主要 AI 搜尋服務確認會使用，暫不做
- **常見問題的做法**（2026-10-19，`add-faq`）：放首頁「03 常見問題」（在購買方式後面）、全部展開不收合；題目清單寫在程式裡，頁面與 FAQPage 共用同一份，只放確認過的答案；答案是純文字不放連結。細節見該 change 的 `design.md`
- **外部 SEO／AEO 檢查**（2026-10-20）：用 Google 搜尋的 AI 診斷與 ultralab.tw 掃描首頁（報告 `https://ultralab.tw/probe/report/s5z9v6x3z9`，只掃首頁：SEO 92、AEO 46、AAO 56，總分 65）。報告頁是前端產生的，原始資料在 `https://ultralab.tw/api/probe-collect-email?report=<id>`。處理方式：
  - 要做：首頁標題與描述太短（併入 `add-faq` D7）、FAQPage／問句標題／直接回答（`add-faq`）、FAQ 加洗衣機與退換貨、品牌故事與聯絡方式（另開 change）
  - 已經有、AI 沒看到：每件商品直達自己賣場的按鈕（開新分頁，只是還沒填賣貨便連結）、配色模擬器、商品頁可左右滑的照片
  - 不做：把目錄數字改成文字（連結文字本來就是「作品」「購買方式」，數字只是裝飾，風格已定）、AAO 類（OpenAPI、表單、MCP、RSS、Webhook，是給 API／軟體服務的）、HowTo、作者與發布日期（給文章用）、`rights`／`ai-content-declaration` meta（不是正式標準）、首頁封面照的空 alt（旁邊標題就是商品名稱，刻意留空）
  - `llms.txt` 維持暫不做；這個工具算了兩次所以扣分多，成本很低，想讓作品集上的分數好看時可以再加
  - 工具建議的長度（標題 30～60、描述 120～160 字元）是照英文算的，中文控制在標題約 25 字、描述 70～80 字；「多點一次流失 20%」這類數字沒有出處，參考就好
- **Bing 網站管理員指南對照**（2026-10-20，https://www.bing.com/webmasters/help/webmaster-guidelines-30fba23a ；網頁是前端產生的，全文在 `https://www.bing.com/webmasters/api/help/htmlcontent?ArticleId=30fba23a`）：伺服器產生的 HTML、robots、sitemap 只列正式網址、下架商品回 404、canonical 與 301、沒有 noindex／nosnippet／noarchive、結構化資料與畫面一致、GitHub Pages 有 ETag，都已符合；禁止事項都沒有。要做的：IndexNow（見「之後」）、商品叫法統一（見待辦）、首頁封面照片連結補 alt（2026-10-20 已改）。
- **商品叫法統一為「飲料提網」**（2026-10-20，Bing 指南第 16 點）：原本首頁標題寫「水壺提網」、分類是「飲料提網」、常見問題寫「水壺或杯子」。選「飲料提網」是因為分類本來就是它，也涵蓋常見問題寫的寶特瓶、咖啡杯、保溫杯；首頁描述另外寫出「裝得下水壺、保溫杯和咖啡杯」，搜尋「水壺」還是對得上。作品列表標題改為「作品：手鉤織飲料提網、杯墊」（第 13 點：標題太短），由目前的分類自動產生。照片檔名與 alt 描述性（第 12 點）跟照片管理一起做
- **ultralab 的內容類型、作者、日期三項**（2026-10-20，`add-page-dates-author`）：只標準確的資訊。品牌用 OnlineStore（沒有實體店面，不用 Store／LocalBusiness），頁面用 WebPage／FAQPage／ItemPage；不加 Article、ProfessionalService（不符合事實，Bing 禁止誤導性結構化資料）。作者是品牌。只有更新日期（dateModified），沒有發布日期（資料庫沒有建立時間）；日期一定顯示在頁尾，和結構化資料一致。首頁版型文字改了不會更新日期（沒有可靠來源）
- **首頁標題改成品牌標題**（2026-10-20，使用者覺得列商品類型的標題「太侷限、不像標題」）：標題「織物製造所 JH Craft Studio｜顏色可客製的手作編織小物」（33 字），商品交給各商品頁與作品列表的標題；描述開頭用使用者寫的句子，後面接常見問題確認過的事實（121 字，符合 ultralab 建議的長度）。英文名維持「JH Craft Studio」（和 IG、結構化資料一致，不用 J&H Craft）；材質原本只寫「棉線」所以先寫棉線；同日確認線材是 Excel 樂透棉 LOTTO EX088、製造商標示 100% 棉，已改成「純棉」（見 `docs/brand/about-questions.md`）。取捨：首頁對「鉤織 飲料提網」這類搜尋稍弱，由商品頁承接。2026-10-20 部署 `808f0be`，IndexNow 回 200（金鑰已通過驗證）
- **ultralab 其他四項**（2026-10-20）：「品牌名稱一致性」是改成 OnlineStore 造成的（工具只認字面上的 Organization），改成兩個類型都列；聯絡方式先用 IG（頁尾＋ContactPoint）；「關於頁面連結」「專業度」等品牌故事；「關鍵詞強調（粗體）」不做：對排名影響很小，常見問題答案是純文字、和結構化資料共用（add-faq D4），雜誌風格也不適合到處粗體
- **主機**：台灣的「虛擬主機」（PChome 等）多為 PHP／WordPress 共用主機、年約、無法 CI/CD，不適合；網域與主機分開買
- **官網先用 GitHub Pages，Azure 延後**（2026-10-16）：客源與 IG 還在起步，先不付主機費（Azure 約 6,000 元／年，GitHub Pages 只有網域費約 490 元／年）。上 Azure 時選 Linux：本專案是 ASP.NET Core，跨平台；Windows B1 貴約 4 倍。細節見 `openspec/changes/add-deployment/design.md`
- **網域**（2026-10-16）：`jhcraftstudio.com` 在 Namecheap 購買，登記在個人名下（沒勾「代表公司」），有免費 Domain Privacy，DNS 用 BasicDNS。Namecheap 推銷的主機、SSL、PremiumDNS、信箱都不需要；要品牌信箱時用免費的信件轉寄
- **分享預覽測試**：LINE Page Poker 已在 2025-06 停止服務；改在 LINE 把網址傳給自己看卡片，Facebook（也涵蓋 IG、Threads）用分享偵錯工具
- **整理 repo**（2026-10-16）：網站沒用到的檔案從 `wwwroot` 移出：舊品牌「貞心卉織 / J&S Crafted」logo（`images/logo/`）、`images/hero/net-tote-trio.jpg`、`images/about/puff-flower-glacier.jpg`、肩背包照片（`products/_watermarked/`）、棕色包草稿（`products/_wip-brown-bag/`）。原檔在筆電的 `D:\jenjenknits-workspace\repo-unused\`（保留原本相對路徑），git 歷史也查得到（commit `ffef3d9` 以前）。舊的 LINE 原始照片在同一台的 `D:\jenjenknits-workspace\photo\`
- **手機實測**：公司電腦的 Edge 被公司政策關閉遠端偵錯（`RemoteDebuggingAllowed=0`），自動化點擊與截圖改用無頭 Chrome

## 品牌與社群素材

見 `docs/brand/README.md`：IG 帳號、頭貼 5 種版本、自介文字、泡芙小花提網貼文。

## 開工前提醒

- 公司電腦：repo 在 `D:\jenjenknits`（從這裡開 Claude Code 與 `JenJenKnits.sln`）；樣稿、舊版 MVC、Stitch 匯出在 `D:\jenjenknits-workspace`
- 先 `git pull`；後台改資料後要 commit 並 push `JenJenKnits/App_Data/jenjenknits.db`（部署前的過渡做法）
- 每台電腦要各自用 `dotnet run -- hash-password` + `dotnet user-secrets` 設定管理員帳號
- 網站執行中時 `dotnet build` / `dotnet test` 會因為 exe 被鎖住失敗，先關掉網站
- Namecheap DNS 的兩筆 TXT 不能刪：`_github-pages-challenge-cienlin`（GitHub 網域驗證）、`@` 的 `google-site-verification=...`（Search Console 驗證）
- 改常見問題：改 `JenJenKnits/Models/SiteFaq.cs` 的題目，並把 `UpdatedOn` 改成當天（首頁的更新日期看它），`dotnet test` 後部署
- `bash tools/deploy-pages.sh` 會直接更新正式官網 `https://jhcraftstudio.com`。自訂網域靠輸出裡的 `CNAME` 檔（由 `Site:BaseUrl` 產生）維持，不要在 GitHub repo Settings → Pages 手動改 Custom domain
- 偏好：繁體中文回答；UI 要用手機實際操作檢查（390px）；前台維持編織誌風格
