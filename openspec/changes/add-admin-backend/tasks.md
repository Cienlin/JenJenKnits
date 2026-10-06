## 1. 資料庫與測試基礎

- [ ] 1.1 從 `redesign-editorial` 建立分支 `add-admin-backend`；加入 NuGet `Microsoft.EntityFrameworkCore.Sqlite`、`Microsoft.EntityFrameworkCore.Design`、`Microsoft.Extensions.Identity.Core`；`dotnet build` 成功
- [ ] 1.2 建立 `Data/`：商品實體（文字欄位、`IsPublished`、`UpdatedAt`，slug 唯一索引）與 DbContext；連線字串指向 `App_Data/jenjenknits.db`（rollback journal）；`.gitignore` 加上此檔的例外；產生初始 migration，`dotnet ef database update` 建出資料庫
- [ ] 1.3 建立 `JenJenKnits.Tests`（xUnit + `Microsoft.AspNetCore.Mvc.Testing`），提供暫存 SQLite 與暫存商品資料夾的測試輔助；`dotnet test` 可執行（先有一個冒煙測試）

## 2. 匯入與前台改讀資料庫

- [ ] 2.1 把 Markdig 設定與照片辨識邏輯從 `FileSystemProductCatalog` 抽成共用元件（故事渲染器、照片資料夾掃描），行為不變；`dotnet build` 成功、前台頁面截圖與改動前一致
- [ ] 2.2 匯入程式：資料庫為空時匯入各資料夾 `meta.json` + `story.md`（已發布、略過底線資料夾與格式錯誤／缺 name 者並記 warning），成功後刪除這兩種檔案；啟動時先 `Database.Migrate()` 再匯入；測試涵蓋正常、格式錯誤、缺 name、底線資料夾、資料庫已有資料
- [ ] 2.3 `DbProductCatalog : IProductCatalog`（只回傳已發布且有封面的商品，排序與現在相同，缺封面記 warning），註冊為 Scoped 並移除 `FileSystemProductCatalog`；測試涵蓋草稿不出現、缺封面不出現、詳細頁 404
- [ ] 2.4 執行匯入：確認兩件商品在首頁、列表、分類、詳細頁與匯入前相同；commit 資料庫檔與刪除的 `meta.json` / `story.md`

## 3. 登入

- [ ] 3.1 `Admin` 設定（UserName、PasswordHash）與 `dotnet run -- hash-password` 指令；`appsettings.json` 只留空白欄位，本機用 user-secrets 設定
- [ ] 3.2 Cookie 驗證、`/Admin` 資料夾授權（登入頁除外）、登入頁與登出、登入次數限制（每 IP 每分鐘 5 次）、`/admin` 回應加 `X-Robots-Tag: noindex`；未設定帳號時登入一律失敗並記 warning
- [ ] 3.3 測試：未登入導向登入頁並帶回原頁、錯誤密碼顯示「帳號或密碼錯誤」、正確密碼進入列表、登出後需重新登入、超過次數被拒絕

## 4. 後台頁面

- [ ] 4.1 `_AdminLayout`（noindex、沿用前台色彩與字體變數、登出鈕、頁尾顯示資料庫最後修改時間）；確認前台任何頁面都沒有 `/admin` 連結
- [ ] 4.2 商品列表：名稱、分類、發布／草稿、是否有封面、缺漏欄位，順序與前台相同；手動確認網格托特包顯示缺價格、材質、尺寸、故事
- [ ] 4.3 新增商品：名稱 + slug（格式與唯一性驗證），建立為草稿後進入編輯頁並提示照片資料夾路徑；測試涵蓋重複 slug 與不合法字元
- [ ] 4.4 編輯商品：所有文字欄位、分類輸入提示（既有分類）、配色模擬器選項、發布切換，儲存前驗證並保留輸入；slug 唯讀；測試涵蓋價格、精選排序、賣貨便連結格式錯誤
- [ ] 4.5 故事 Markdown 預覽：編輯頁 POST handler 以共用渲染器回傳 HTML 片段，不寫入資料庫；手動確認預覽與前台詳細頁一致

## 5. 整合驗證與文件

- [ ] 5.1 端對端走一次：登入 → 補網格托特包的價格與故事 → 儲存 → 前台詳細頁出現新內容 → 改回草稿 → 前台 404 → 再發布；桌機與 390px 手機無水平捲軸、主控台無錯誤；`dotnet test` 全部通過
- [ ] 5.2 執行 `tools/export-static.mjs`，確認 GitHub Pages 展示網站仍可輸出且連結檢查為 0 缺漏
- [ ] 5.3 撰寫 README 的「後台」段落：設定管理員帳號（user-secrets、hash-password）、兩台電腦的資料庫同步規則（先 pull、改完就 push）、更新展示網站的步驟
