## Why

商品資訊（價格、材質、尺寸、故事）大多還沒填，目前要補只能手改 `meta.json`、`story.md` 再 git push，容易打錯格式也看不到結果。加一個只有管理者能進的簡易後台，讓補資訊變成填表單；同時把網站從「讀檔案」推進到「有資料庫、有登入」的全端架構，作為可以放上履歷的作品。

本 change 只做**第一階段**：文字資料進資料庫並可在後台編輯。照片上傳與部署到主機留給之後的 change。

## What Changes

- **商品文字資料改存 SQLite**（EF Core）：名稱、分類、價格、材質、尺寸、簡介、賣貨便連結、精選排序、配色模擬器、故事 Markdown、發布狀態
- **照片維持資料夾規則**：`wwwroot/products/{slug}/` 的 `cover.*`、`01.*…`、`colors.*` 不變
- **一次性匯入**：資料庫為空時，從現有商品資料夾的 `meta.json` + `story.md` 匯入；匯入後刪除這兩種檔案，避免兩份資料不同步
- **後台 `/admin`**：
  - 管理員登入／登出（單一帳號，Cookie 驗證，密碼只存雜湊值，登入嘗試有次數限制）
  - 商品列表（含草稿），顯示發布狀態與缺漏欄位
  - 新增商品（設定網址代稱 slug）、編輯文字資料、故事 Markdown 預覽
  - 草稿／發布切換
  - 後台不出現在前台導覽，且告訴搜尋引擎不要收錄
- **前台**：改從資料庫讀商品；草稿不出現在任何前台頁面
- **BREAKING**：`meta.json`、`story.md` 不再使用；資料夾名稱加底線當草稿的規則改為後台的「草稿」狀態
- **新增測試專案**：匯入、前台可見性、登入保護的自動化測試

**範圍外（之後的 change）**：照片上傳／排序／刪除、刪除商品、部署到主機、多帳號、Demo 模式

## Capabilities

### New Capabilities

- `product-admin`：管理者登入後台，查看、新增、編輯商品文字資料並切換發布狀態

### Modified Capabilities

- `product-showcase`：商品文字資料來源由檔案改為資料庫（照片規則不變）；草稿改由發布狀態決定；故事改由資料庫中的 Markdown 渲染

> `product-showcase` 仍在 `add-showcase-mvp` 內（尚未歸檔）。歸檔順序：`add-showcase-mvp` → `redesign-editorial` → 本 change。

## Impact

**新增**
- NuGet：`Microsoft.EntityFrameworkCore.Sqlite`、`Microsoft.EntityFrameworkCore.Design`（遷移工具）、`Microsoft.Extensions.Identity.Core`（只用密碼雜湊）
- `Data/`：DbContext、商品實體、遷移、匯入程式
- `Pages/Admin/`：登入、商品列表、新增、編輯
- `App_Data/jenjenknits.db`：資料庫檔（進 git，見 design D2）
- `JenJenKnits.Tests/`：xUnit 測試專案

**修改**
- `Services/`：以資料庫實作 `IProductCatalog`，移除 `FileSystemProductCatalog`
- `Program.cs`：註冊 DbContext、驗證、授權、登入次數限制；啟動時套用遷移與匯入
- `wwwroot/products/*/`：刪除 `meta.json`、`story.md`（照片保留）

**不受影響**
- 前台頁面與樣式、路由、`tools/export-static.mjs`（GitHub Pages 展示網站照常輸出）

> 本 change 推翻 `add-showcase-mvp` D6「不需要資料庫與登入」的決定，理由見 design D1。
