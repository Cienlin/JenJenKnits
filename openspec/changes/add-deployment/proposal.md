> **狀態：探索中（草稿）**。主機尚未決定，決定後再補 specs、完整 design 與 tasks。討論紀錄見同資料夾的 `design.md`。

## Why

網站目前只在本機執行：要用後台得先 `dotnet run`，兩台電腦靠 git 同步 SQLite 資料庫檔（容易衝突、容易忘記 push），訪客看到的是手動輸出到 GitHub Pages 的靜態版本。部署到主機後，後台有固定網址（手機也能用），資料只有主機上一份，push 程式碼即自動測試與部署；這也是本專案放上履歷最重要的一步。

## What Changes（預計）

- 部署到主機，前台與後台皆可透過自己的網域存取（預計 `jhcraftstudio.tw` 或 `jhcraft.tw`）
- GitHub Actions：push 到 `main` → build → 測試 → 部署
- 資料的正本改為主機上的 SQLite 與照片；git 中的 `App_Data/jenjenknits.db` 不再是正本（處理方式待決定）
- 管理員帳號改由主機的環境設定提供
- `Site:BaseUrl` 改為新網域；HTTPS 由主機處理；`robots.txt` 在網域根目錄開始生效
- 資料庫與照片的定期備份
- GitHub Pages：主機上線後的角色待決定（關閉、或保留為備份）

## Capabilities

待主機決定後確認（預計新增部署與備份相關的需求，或標記為不影響產品行為的基礎建設變更）。

## Impact

待定。
