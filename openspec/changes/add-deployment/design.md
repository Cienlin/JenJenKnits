> **狀態：探索中（草稿）**。2026-10-08 在公司電腦以 OpenSpec 探索模式討論的紀錄。2026-10-16：**先用 GitHub Pages＋自己的網域 `jhcraftstudio.com` 當官網，Azure 延後**（見「調整」一節）；之後上主機時用 **Azure App Service（Linux B1）**。其餘待決定事項見文末。

## 已確認的條件（使用者回答）

| 問題 | 回答 |
|---|---|
| 每月主機預算 | 300～500 元台幣也可以 |
| 網域 | 要買自己的網域 |
| 冷啟動（休眠後第一次打開要等 10～30 秒） | 前台不行，後台可以 |
| 後台使用者 | 只有使用者本人（工程師）；貞貞只提供商品資訊 |

## 核心問題

程式每次部署都會被換掉，資料（SQLite、照片）必須保留。選主機幾乎就是在決定資料放哪裡。

## 候選方案（價格為 2026-10 查詢，部署前請再確認）

### 路線一：一台不休眠的主機（前台＋後台）

```
訪客／你 ──▶ 自己的網域 ──▶ 主機（不休眠）── 磁碟：SQLite＋照片
push ──▶ GitHub Actions：測試 → 部署
```

| 主機 | 每月約 | 特色 |
|---|---|---|
| Azure App Service Linux B1 | US$13.14（≈ 420 元） | 最貼近 .NET、不需 Dockerfile、`/home` 磁碟會保留；台灣 .NET 職缺常用 Azure |
| Fly.io（shared-cpu 512MB＋1GB volume） | ≈ US$3～4（≈ 100～130 元） | 便宜、Docker 部署（學容器）、可選東京機房；需寫 Dockerfile、掛 volume。volume 快照自 2026-01 起超過 10GB 收費 |
| Render Starter＋persistent disk | US$7 起＋磁碟 US$0.25/GB，部分方案另收工作區費 | 免費方案不能掛磁碟、15 分鐘休眠，不考慮免費方案 |

### 路線二：前台靜態＋後台另外放

```
訪客 ──▶ 網域 ──▶ 靜態網站（Cloudflare Pages／GitHub Pages，免費、CDN）
                        ▲ ③ 重新輸出並部署
你 ──▶ admin.網域 ──▶ 後台主機（沒人用就休眠）── ① 改資料 ② 按「發布」觸發 GitHub Actions
```

- 前台永遠快且免費；後台可休眠（符合「後台可以等」）
- 沿用 `tools/export-static.mjs`：改成抓後台主機再輸出
- 代價：改資料後 1～2 分鐘前台才更新；兩個部署目標，較複雜
- 履歷：JAMstack 架構

### 比較

| | 路線一 Azure | 路線一 Fly.io | 路線二 |
|---|---|---|---|
| 每月費用 | ≈ 420 元 | ≈ 100～130 元 | ≈ 0～50 元 |
| 複雜度 | 最低 | 中 | 最高 |
| 改資料到上線 | 立即 | 立即 | 1～2 分鐘 |
| 履歷故事 | .NET＋Azure＋CI/CD | Docker＋CI/CD | JAMstack＋CI/CD |

## 調整：先用 GitHub Pages＋自己的網域，Azure 延後（2026-10-16）

客源還少、IG 還在起步，先不付主機費。官網沿用 GitHub Pages（靜態輸出），綁上自己的網域；後台繼續只在本機使用。

- **訪客那邊不受影響**：首頁、作品、商品頁、配色模擬器都是靜態頁面；GitHub Pages 免費、有 HTTPS、可綁網域；每月 100GB 流量對目前規模足夠
- **先綁網域的理由**：IG 個人檔案、貼文、名片上的網址從第一天就是最終網址；之後搬到 Azure 只要改 DNS，網址與搜尋排名都保留。網站在子路徑 `/JenJenKnits/` 時 `robots.txt` 不會生效，綁網域放在根目錄後才有效
- **代價**：後台只能在本機用、手機不能改；改資料要「改 → commit db → 輸出 → 推 gh-pages」，兩台電腦仍靠 git 同步資料庫；沒有伺服器端功能（目前也不需要，下單在賣貨便）
- **GitHub 的使用規定**：Pages 不允許用來經營網路商店或主要促成交易的網站。本站是品牌展示、購買導到賣貨便，屬灰色地帶；若在意，同一份靜態檔案可改放 Cloudflare Pages（免費、允許商用）
- **可以順便做（建議，尚未決定）**：GitHub Actions 在 push 後自動測試、輸出並部署 GitHub Pages，取代手動 `tools/deploy-pages.sh`，免費就有 CI/CD
- 費用：只有網域，約 490 元／年；直接上 Azure 約 6,000 元／年

**什麼時候搬到 Azure**（符合其中一項）：想用手機改資料或改得很頻繁；要加伺服器端功能（聯絡表單、庫存、訂單）；開始找工作、想在履歷上寫「部署到 Azure」。

## 之後的主機：Azure App Service Linux B1（2026-10-16 決定）

上主機時用一台主機（路線一）把 CI/CD 跑順；前台本來就能輸出成靜態網站，之後要演進到路線二也容易。路線一中選 Azure，理由：

- **第一次正式上線，要自己處理的事最少**：不需要 Dockerfile；登入用的 Data Protection 金鑰會自動存在持久的 `/home`；有 Always On（不休眠）與免費的自訂網域憑證
- **資料比較安全**：`/home` 底層是有備援的 Azure 儲存體；Fly.io 的 volume 只有一份，硬體故障只能靠每日快照還原
- **履歷**：.NET＋Azure＋CI/CD；台灣 .NET 職缺常用 Azure
- 費用約 430～470 元／月（見下方費用表），在 300～500 元預算內

代價與取捨：

- 比 Fly.io 每月多約 300 元（一年約 3,500 元）
- 這次不學 Docker。Docker 是「打包方式」，CI/CD 是「自動測試與部署的流程」，兩者無關，選 Azure 一樣有完整的 CI/CD（push → 測試 → 打包成 zip → 部署）
- 決定不是永久的：網站的資料就是一個 SQLite 檔加上照片，之後要搬到 Fly.io 或其他主機，只要搬檔案、改 CI 設定

**主要風險**：`/home` 是網路磁碟，SQLite 的檔案鎖在上面比較慢。這個網站寫入很少、單一主機、資料庫已是 `journal_mode=DELETE`（不是 WAL），風險低，但正式上線前要先實測（見待決定 1）。

**為什麼是 Linux 不是 Windows**：「.NET 要放 Windows」是 .NET Framework 時代的規則；本專案是 ASP.NET Core（`net9.0`），跨平台。套件（EF Core SQLite、Markdig、Identity.Core）都跨平台；Linux 檔名分大小寫，但 `Services/ProductFolders.cs` 比對照片檔名時不分大小寫、網址用實際檔名產生；路徑都用 `Path.Combine`。同樣 B1，Windows 約 US$55／月，超出預算。CI 在 Linux 上跑測試，相容問題會在部署前擋下。

### Azure 的費用（2026-10-16 以 Azure Retail Prices API 查詢）

| 項目 | 費用 |
|---|---|
| App Service B1 **Linux** | Japan East 約 US$13.9／月、East Asia（香港）約 US$14.6／月（約 430～470 元）；依時數計費 |
| 免費帳號 | 要綁信用卡；送 US$200 額度、30 天有效；之後要升級為隨用隨付才能繼續 |
| 不另收費 | 受控憑證（HTTPS）、自訂網域綁定、自動備份（Basic 以上，每小時、保留 30 天、包含 `/home`）、每月前 100GB 流量、Application Insights 每月 5GB 內 |

注意：一定要選 Linux（Windows B1 約 US$55／月）；「停止」網站仍會計費，要刪除 App Service 方案才停止；第一天就在「成本管理」設預算提醒；自訂備份（可下載）需另開儲存體、會另外收費；自動備份與主機同機房，官方不建議當唯一的備份；帳單可能另加 5% 營業稅。

### 憑證與部署權限

- **HTTPS 憑證**：`*.azurewebsites.net` 預設就有 HTTPS；自訂網域用 App Service 受控憑證（B1 可用、免費、自動續約，不支援萬用憑證）。程式已有 `UseHttpsRedirection` 與 HSTS，Azure 上再開「僅限 HTTPS」
- **GitHub Actions 部署到 Azure 的權限**：用 OIDC 聯合認證（Azure 信任這個 repo，不存任何密碼），不用 publish profile

## 網域

- 網域是租用、每年續約，不是買斷；要開自動續約
- `.com` 約 300～450 元／年，`.tw` 約 600～900 元／年；技術與 SEO 上無實質差別，`.tw` 在地感較強
- 2026-10-08 以 RDAP 查詢：`jhcraft.com` 已被註冊；`jhcraftstudio.com`、`jhcraft.tw`、`jhcraftstudio.tw`、`zhiwu.tw` 當時未被註冊
- **2026-10-16 已購買 `jhcraftstudio.com`**（Namecheap）：與 IG `jh.craft.studio` 對應，`.com` 最多人認得
  - 第一年 US$6.99（優惠碼 `NEWCOM679` US$6.79＋ICANN 費 US$0.20）；續約 US$14.98＋US$0.20／年（約 490 元）
  - 登記在使用者個人名下（未勾「代表公司」）；免費 Domain Privacy；DNS 用 Namecheap BasicDNS；未加購 PremiumDNS、主機、SSL、信箱
  - 自動續約：網域與 Domain Privacy 開、PremiumDNS 關
  - 要做：15 天內點 ICANN 驗證信；帳號開兩步驟驗證；品牌信箱需要時用 Namecheap 免費的信件轉寄
- DNS 先不要設定：要先改好部署腳本（見下方「綁網域到 GitHub Pages」），否則下次部署會把網域設定洗掉

### 綁網域到 GitHub Pages（步驟）

1. Namecheap DNS：`jhcraftstudio.com` 設 4 筆 A 紀錄（`185.199.108.153`、`185.199.109.153`、`185.199.110.153`、`185.199.111.153`）；`www` 設 CNAME 到 `cienlin.github.io`
2. GitHub repo → Settings → Pages → Custom domain 填網域；憑證發好後勾「Enforce HTTPS」
3. （建議）GitHub 帳號設定驗證網域（TXT 紀錄），避免被別人綁走
4. 程式要改：
   - `tools/deploy-pages.sh` 每次重建 gh-pages 分支，會洗掉 GitHub 記網域用的 `CNAME` 檔，要改成輸出時產生 `CNAME`
   - `tools/export-static.mjs` 的子路徑從 `/JenJenKnits` 改成空的（腳本已支援參數）
   - `Site:BaseUrl` 改成新網域（社群分享預覽、canonical、sitemap、robots.txt 會用到）
5. 舊網址 `cienlin.github.io/JenJenKnits` 會由 GitHub 自動轉到新網域

## 不考慮的選項

- **台灣的虛擬主機**（例如 PChome 虛擬主機）：多為 PHP／WordPress 共用主機，未必支援 .NET 9；年約、包含用不到的信箱等服務所以較貴；只能 FTP 上傳，無法接 GitHub Actions。網域可以在 PChome 等註冊商單獨購買，不必加購主機
- **地端**（自己的伺服器）：要自己處理硬體、停電、網路與更新，對小品牌網站負擔太大

## 程式碼現況：部署時要處理的事（2026-10-16 盤點）

```
       部署時會被換掉                      必須保留
┌─────────────────────────┐      ┌───────────────────────────────┐
│ 程式（.dll）             │      │ SQLite 資料庫                  │
│ wwwroot/css、js          │      │ Data Protection 金鑰（登入用）  │
│ wwwroot/products（照片） │      │ （第二階段）後台上傳的照片       │
└─────────────────────────┘      └───────────────────────────────┘
```

1. **資料庫路徑**：`Program.cs` 已支援用 `ConnectionStrings:Site` 覆寫，主機上用環境變數（`ConnectionStrings__Site`）指到 `/home` 底下、程式資料夾 `/home/site/wwwroot` 以外的位置即可，不用改程式
2. **照片**：目前在 `wwwroot/products/`、進 git，跟著程式一起部署，這一階段不用處理。後台第二階段做上傳時要搬到持久空間；而且 `MapStaticAssets()` 只提供建置時就存在的檔案，執行中上傳的照片需要另外用靜態檔案中介軟體提供
3. **Data Protection 金鑰**：程式沒有指定存放位置。Azure App Service 會自動存到 `/home`，部署後不會被登出（換主機時要記得自己設定）
4. **反向代理**：Azure 在前面處理 HTTPS 再轉給程式。程式要讀轉送標頭（forwarded headers），否則登入次數限制（每個 IP 每分鐘 5 次）會把所有人當成同一個 IP，別人亂試密碼時管理員也會被擋；Cookie 的 Secure 判斷也會錯。上線時確認 Azure 是否已預設開啟，沒有就自己設定
5. **管理員帳號**：本機用 user-secrets；主機上改用應用程式設定（`Admin__UserName`、`Admin__PasswordHash`）

## 待決定（Open Questions）

1. ~~主機~~ → 先用 GitHub Pages；之後上主機用 Azure App Service Linux B1（2026-10-16）。上 Azure 時的待辦：先部署一次實測 SQLite 放在 `/home`：用後台存幾筆資料、重啟主機，確認資料還在、速度可接受
2. ~~網域~~ → `jhcraftstudio.com`（Namecheap，2026-10-16 購買）
3. ~~主要網址~~ → `jhcraftstudio.com`；`www.jhcraftstudio.com` 由 GitHub 自動轉過來（2026-10-16）
4. GitHub Actions 自動輸出並部署 GitHub Pages：要做，但和綁網域分開，另開 change（2026-10-16）
5. 上 Azure 時：區域選 Japan East（較便宜）或 East Asia（香港），比較延遲
6. 上 Azure 時：git 裡的 `App_Data/jenjenknits.db` 怎麼處理（刪除、保留為測試資料、或作為初始資料）
7. 上 Azure 時：備份。B1 已有每小時自動備份（保留 30 天、包含 `/home`），但與主機同機房；要不要另外定期下載一份
8. 上 Azure 時：GitHub Pages 關閉，或保留為備份
9. 照片上傳（後台第二階段）的存放位置：本機（GitHub Pages 階段，照片進 git）；上 Azure 後用 `/home` 或 Azure Blob Storage
