# 織物製造所

手鉤織品牌的展示網站：首頁、作品列表、商品頁（照片、故事、配色模擬器），以及只有管理者能進的商品後台。

- 技術：ASP.NET Core 9 Razor Pages、EF Core + SQLite、Cookie 驗證、xUnit
- 展示網站（靜態輸出）：https://cienlin.github.io/JenJenKnits/
- 規格：`openspec/specs/`（網站目前的行為）；設計與決策紀錄：`openspec/changes/archive/`（每個 change 的 proposal / design / specs / tasks）

## 在本機執行

需要 .NET 9 SDK。

```bash
cd JenJenKnits
dotnet run --launch-profile http
```

打開 http://localhost:5178 。第一次啟動會自動建立／升級資料庫（`App_Data/jenjenknits.db`）。

```bash
dotnet test        # 在 repo 根目錄執行全部測試
```

## 商品資料放在哪裡

| 內容 | 位置 | 怎麼改 |
|---|---|---|
| 文字資料（名稱、價格、故事…） | `JenJenKnits/App_Data/jenjenknits.db` | 後台 `/admin` |
| 照片 | `JenJenKnits/wwwroot/products/{slug}/` | 直接放檔案 |

照片檔名規則：`cover.jpg` 封面（必要，沒有封面的商品不會出現在前台）、`01.jpg`、`02.jpg`… gallery、`colors.jpg` 色卡；副檔名可用 jpg / jpeg / png / webp。

## 後台

網址：http://localhost:5178/admin （前台沒有連結到這裡，也不會被搜尋引擎收錄）

### 第一次設定管理員帳號（每台電腦各做一次）

帳號密碼不放在 git 裡，存在本機的 user-secrets：

```bash
cd JenJenKnits
dotnet run -- hash-password                                  # 輸入密碼（至少 12 字），印出雜湊值
dotnet user-secrets set "Admin:UserName" "你的帳號"
dotnet user-secrets set "Admin:PasswordHash" "上一步印出的雜湊值"
```

沒設定時前台照常運作，後台登入一律失敗。

### 上架新商品

1. 後台「新增商品」：輸入名稱與網址代稱（slug，例如 `mustard-tote`），會先存成草稿
2. 照片放進 `wwwroot/products/{slug}/`
3. 回到後台補完資料，勾選「發布到前台」並儲存

### 兩台電腦之間同步（重要）

資料庫是一個檔案，跟著 git 走：

- **改資料前先 `git pull`**
- **改完立刻 commit 並 push** `JenJenKnits/App_Data/jenjenknits.db`（和新放的照片）
- 不要在兩台電腦上同時改資料：資料庫檔沒辦法自動合併，衝突時只能二選一

後台頁尾會顯示「資料最後更新」時間，可以用來確認是不是最新的資料。

## 更新展示網站（GitHub Pages）

GitHub Pages 只能放靜態檔案，所以是把本機執行中的網站輸出成 HTML 再推上去（後台不會被輸出）：

```bash
cd JenJenKnits && dotnet run --launch-profile http     # 另開一個終端機讓它一直跑
node tools/export-static.mjs                            # 在 repo 根目錄執行，輸出到 site-export/（需要 Node.js）
cd site-export
git init -b gh-pages && git add -A && git commit -m "deploy"
git push -f https://github.com/Cienlin/JenJenKnits.git gh-pages
```
