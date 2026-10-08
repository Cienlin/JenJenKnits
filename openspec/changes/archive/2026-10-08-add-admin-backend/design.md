## Context

動機見 proposal.md。現況：
- 前台頁面只透過 `IProductCatalog`（唯讀：`GetAll`、`GetFeatured`、`GetByCategory`、`GetBySlug`、`GetAllCategories`）取得商品；實作 `FileSystemProductCatalog` 掃描 `wwwroot/products/{slug}/`，讀 `meta.json`、`story.md`，並依檔名找照片
- 沒有資料庫、沒有登入（`add-showcase-mvp` D6 刻意拆除）
- 維護者只有使用者本人（工程師），在**兩台電腦**之間以 git 同步；貞貞只提供商品資訊
- GitHub Pages 展示網站由 `tools/export-static.mjs` 從本機執行中的網站輸出

## Goals / Non-Goals

**Goals:**
- 補商品資訊從「手改檔案」變成「後台填表單」，且兩台電腦都拿得到同一份資料
- 前台頁面與樣式不需修改
- 架構與安全做法經得起面試時的追問（為什麼這樣選）

**Non-Goals:**
- 照片上傳／排序／刪除、刪除商品（下一個 change）
- 部署到主機、正式環境資料庫（第三階段）
- ASP.NET Core Identity、多帳號、角色權限、Demo 模式

## Decisions

### D1. 加回資料庫與登入（推翻 add-showcase-mvp D6）

D6 拆除資料庫與 Identity 的理由是「純介紹站用不到，dead code 是負債」。現在有了具體需求（後台編輯）才加回來，而且只加需要的部分：SQLite + EF Core、Cookie 驗證，不加 Identity 的資料表與頁面。

### D2. SQLite 資料庫檔放 `App_Data/` 並進 git

兩台電腦靠 git 同步，資料庫若不進 git，另一台就拿不到補好的資料。

- 位置：`JenJenKnits/App_Data/jenjenknits.db`，不放 `wwwroot/`（否則可被直接下載）
- 使用 SQLite 預設的 rollback journal，不開 WAL，資料庫只有一個檔案
- `.gitignore` 目前忽略 `*.db`，需加上這個檔案的例外
- 使用規則：改資料前先 `git pull`，改完立刻 commit + push；不在兩台電腦同時改

**替代方案**
- 資料庫不進 git：兩台資料會分歧，最直接的失敗模式
- 後台改完寫回 `meta.json` / `story.md`：git 友善，但沒有資料庫，失去這個 change 的學習與履歷價值
- 雲端資料庫（Neon、Azure SQL 等）：需要帳號與網路，屬於部署階段的決定，現在先不引入

### D3. 照片維持資料夾規則

照片上傳是後台最複雜的部分（格式／大小檢查、縮圖、排序、刪除、存放位置），而目前並不缺照片。資料庫只存文字欄位；照片仍以 slug 找 `wwwroot/products/{slug}/` 的 `cover.*`、`01.*…`、`colors.*`，沿用現有的檔名辨識邏輯。

### D4. 一次性匯入，之後只有一份資料

啟動時若資料庫沒有商品，就從各資料夾的 `meta.json` + `story.md` 匯入（皆設為已發布；底線開頭資料夾略過），成功後刪除這兩種檔案。

**理由**：檔案與資料庫並存會出現「改了哪一份才算數」的問題。只在資料庫為空時匯入，重複啟動不會覆蓋後台的修改。故事原文（含 HTML 註解）照存，渲染時沿用「只有註解視為空白」的規則。

### D5. `IProductCatalog` 介面不變，換成資料庫實作

新增 `DbProductCatalog : IProductCatalog`：讀資料庫中已發布的商品，組合資料夾中的照片，排序規則不變（精選依 `featured` 升冪在前，其餘依 slug）。一律註冊為 Scoped（每個請求讀一次，資料量小）。

後台頁面直接使用 DbContext 讀寫（Razor Pages 的一般做法），不另外包一層 repository；前台仍只認 `IProductCatalog`。

**替代方案**：讀寫都包成服務介面。只有一組後台頁面時，多一層只是轉手。

### D6. 登入：Cookie 驗證 + 單一管理員帳號

- 設定：`Admin:UserName`、`Admin:PasswordHash`；本機用 user-secrets，不寫進 `appsettings.json`
- 雜湊：`PasswordHasher<T>`（`Microsoft.Extensions.Identity.Core`，只取雜湊演算法），提供 `dotnet run -- hash-password` 產生雜湊值
- Cookie：HttpOnly、SameSite=Strict、HTTPS 時 Secure；閒置 8 小時過期
- 授權：`/Admin` 資料夾全部需要登入，登入頁除外
- 登入次數限制：內建 Rate Limiter，同一 IP 每分鐘最多 5 次登入請求
- 防 CSRF：Razor Pages 表單預設的 antiforgery token
- 不收錄：後台版面加 `<meta name="robots" content="noindex">`，`/admin` 回應加 `X-Robots-Tag: noindex`

**替代方案**：ASP.NET Core Identity。功能完整但會帶入使用者資料表、註冊、忘記密碼等頁面，對單一管理員是多餘的攻擊面；面試時說明「為什麼不用」也是設計判斷的展現。

### D7. 後台介面

- 獨立的 `_AdminLayout`，沿用前台的色彩與字體變數，但以清楚好填為主，不做編織誌的裝飾
- 以電腦操作為主；手機上不出現水平捲軸、按鈕至少 44px
- 列表用表格：名稱、分類、狀態、封面、缺漏欄位
- 編輯頁：表單欄位 + 故事 Markdown 文字框與「預覽」按鈕

### D8. 故事預覽與前台共用渲染

把目前 `FileSystemProductCatalog` 內的 Markdig 設定抽成共用的故事渲染器，前台與預覽都用它。預覽是編輯頁的 POST handler，只回傳渲染後的 HTML 片段，不寫入資料庫。

### D9. 資料庫遷移

EF Core migrations 進 git；應用程式啟動時執行 `Database.Migrate()`，接著執行 D4 的匯入檢查。

### D10. 測試

新增 `JenJenKnits.Tests`（xUnit），每個測試使用暫存的 SQLite 檔：
- 匯入：正常資料夾、格式錯誤、缺 `name`、底線資料夾、資料庫已有資料時不匯入
- 前台可見性：草稿與缺封面的商品不出現、詳細頁 404
- 登入（`WebApplicationFactory`）：未登入導向登入頁、錯誤密碼、正確密碼、登出
- 編輯驗證：價格、精選排序、賣貨便連結格式

## Risks / Trade-offs

- [資料庫是二進位檔，兩台電腦都改會衝突] → 遵守 D2 的「先 pull、改完就 push」；第三階段改用主機上的資料庫後此風險消失
- [忘記 commit 資料庫檔，另一台看不到修改] → README 寫明流程；後台頁尾顯示資料庫最後修改時間作為提醒
- [密碼雜湊誤進 git] → 只用 user-secrets／環境變數；`appsettings.json` 只放空白欄位名稱
- [匯入後刪除檔案，萬一匯入有誤] → 檔案仍在 git 歷史中可還原；匯入結果由測試覆蓋
- [第三階段部署後，資料的正本在哪裡] → 見 Open Questions，不影響本階段

## Migration Plan

1. 從 `redesign-editorial` 開新分支實作（改版尚未合併到 `main`）
2. 首次啟動自動匯入；確認後台與前台內容一致後，commit 資料庫檔與刪除的 `meta.json` / `story.md`
3. 另一台電腦 `git pull` 後即可使用；記得在那台設定 user-secrets 的管理員帳號
4. 回滾：revert 本 change 的 commit，檔案與 `FileSystemProductCatalog` 會一起回來

## Open Questions

- 第三階段部署到主機後，資料的正本改為主機上的資料庫，git 裡的 SQLite 檔是否保留為備份或測試資料
