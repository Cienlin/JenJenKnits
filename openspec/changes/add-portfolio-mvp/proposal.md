## Why

貞貞（家人）想以手工毛線圍巾/肩背袋為副業，目前尚無銷售管道與品牌識別。既有電商平台（Pinkoi、蝦皮）雖有流量但需付平台費且缺乏品牌門面，且經驗證她的目標客層更適合「先建立信任 → LINE 私訊詢問」的低壓銷售模式。此 MVP 建立輕量作品集網站，提供品牌門面與 LINE 導單通路，讓她專注創作而非平台操作。

## What Changes

- 建立公開作品集網站，含商品目錄（分類篩選、搜尋）、商品詳細頁（多圖、材質/尺寸/價格/故事）
- 建立「詢問清單」流程（session-based，非購物車），使用者選定商品後導向 LINE 官方帳號完成詢問下單
- 建立「關於我 / 製作理念」頁面，內容可由後台編輯
- 建立管理員後台（沿用現有 ASP.NET Core Identity），支援：商品 CRUD、分類 CRUD、多圖上傳（本機 `wwwroot/uploads/`）、關於我內容編輯
- 行動裝置優先（RWD），因手工商品買家主要透過手機瀏覽
- **明確排除**（第一版不做）：購物車結帳流程、金流串接、物流串接、電子發票、消費者會員系統、多店家/多租戶

## Capabilities

### New Capabilities

- `product-showcase`：公開瀏覽體驗，包含首頁、商品目錄（含分類與搜尋）、商品詳細頁、詢問清單、LINE 導單流程、關於我頁
- `admin-content-management`：管理員登入受保護區，包含商品 CRUD、分類 CRUD、圖片上傳與管理、關於我內容編輯

### Modified Capabilities

（無，此為首次建立的 spec）

## Impact

- **新增 Domain Models**（於 `Models/` 或 `Domain/`）：`Product`、`Category`、`ProductImage`、`AboutContent`
- **擴充 `Data/ApplicationDbContext.cs`**：加入對應 DbSet，並沿用現有 Identity schema
- **新增 EF Core Migration**：建立商品/分類/圖片/關於內容 tables
- **新增 Razor Pages**：
  - 前台：`/Index`、`/Products/Index`、`/Products/Detail/{slug}`、`/Inquiry`、`/About`
  - 後台：`/Admin/Products/*`、`/Admin/Categories/*`、`/Admin/About`
- **新增 `wwwroot/uploads/`**：本機圖片儲存路徑（.gitignore 已排除 `*.db`，需追加此路徑）
- **設定**：於 `appsettings.json` 新增 `LineOA:Url` 設定，儲存 LINE 官方帳號連結
- **既有 Identity**：新增管理員 role，並設定 seed 建立初始 admin 帳號（避免登入死鎖）
- **無外部相依變更**：本 MVP 不新增 NuGet package（沿用模板既有的 Identity + EF Core + SQLite）
