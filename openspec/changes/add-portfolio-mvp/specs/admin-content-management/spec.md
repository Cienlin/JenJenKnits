## ADDED Requirements

### Requirement: 管理員身分驗證

系統 SHALL 使用既有 ASP.NET Core Identity 驗證管理員身分，僅授權具「Admin」角色之使用者存取管理區。

#### Scenario: 未登入嘗試存取後台

- **WHEN** 未登入使用者造訪 `/admin/*` 任一路徑
- **THEN** 系統導向 `/Identity/Account/Login`，登入成功後導回原目標頁

#### Scenario: 已登入但非 Admin 角色

- **WHEN** 已登入但不具 Admin 角色的使用者造訪 `/admin/*`
- **THEN** 系統回傳 HTTP 403 並顯示「權限不足」頁

#### Scenario: 已登入 Admin 存取後台

- **WHEN** 具 Admin 角色的使用者造訪 `/admin`
- **THEN** 系統顯示管理儀表板首頁，含商品/分類/關於我三個入口

### Requirement: 初始管理員自動建立

系統 SHALL 於應用程式首次啟動時，自動建立一組預設管理員帳號，避免無法登入的死鎖狀態。

#### Scenario: 首次啟動無 Admin 帳號

- **WHEN** 應用程式啟動時偵測資料庫中無任何 Admin 角色使用者
- **THEN** 系統自 `appsettings.json` 讀取 `Seed:Admin:Email` 與 `Seed:Admin:Password` 建立管理員帳號並指派 Admin 角色

#### Scenario: 已存在 Admin 帳號

- **WHEN** 應用程式啟動時已有至少一位 Admin 角色使用者
- **THEN** 系統不重複建立，也不覆寫既有密碼

### Requirement: 商品管理 CRUD

系統 SHALL 於管理區提供商品建立、檢視、編輯、刪除功能。

#### Scenario: 建立新商品

- **WHEN** Admin 於 `/admin/products/create` 填入名稱、分類、價格、材質、尺寸、故事、slug（可自動產生）、是否上架、是否精選
- **THEN** 系統儲存商品並導向該商品編輯頁

#### Scenario: 必填欄位驗證失敗

- **WHEN** Admin 未填名稱或價格便送出表單
- **THEN** 系統阻擋送出並於對應欄位下顯示中文錯誤訊息

#### Scenario: 編輯商品

- **WHEN** Admin 於 `/admin/products/edit/{id}` 修改任一欄位並送出
- **THEN** 系統儲存變更並顯示「已儲存」提示

#### Scenario: 刪除商品

- **WHEN** Admin 點選刪除並確認
- **THEN** 系統刪除該商品及其關聯的所有 `ProductImage` 紀錄與對應圖檔

#### Scenario: 上/下架切換

- **WHEN** Admin 於商品列表切換 `IsPublished` 狀態
- **THEN** 系統立即更新，未上架商品不再出現於前台目錄

### Requirement: 分類管理 CRUD

系統 SHALL 於管理區提供分類建立、編輯、刪除功能。

#### Scenario: 建立分類

- **WHEN** Admin 於 `/admin/categories/create` 填入名稱與 slug
- **THEN** 系統儲存分類

#### Scenario: 刪除含商品的分類

- **WHEN** Admin 嘗試刪除仍有商品的分類
- **THEN** 系統拒絕刪除並提示「請先將分類下商品移至其他分類」

#### Scenario: 刪除空分類

- **WHEN** Admin 刪除無關聯商品的分類
- **THEN** 系統刪除該分類

### Requirement: 商品圖片上傳與管理

系統 SHALL 支援每件商品多張圖片上傳，並允許 Admin 排序與刪除。

#### Scenario: 上傳圖片

- **WHEN** Admin 於商品編輯頁選擇 1 至多張圖檔（JPG/PNG/WebP，每張 ≤ 5MB）並上傳
- **THEN** 系統將圖檔儲存至 `wwwroot/uploads/{yyyy}/{mm}/{guid}.{ext}`，並於資料庫記錄 `ProductImage`（含相對路徑、原始檔名、顯示順序）

#### Scenario: 上傳非圖片檔或超過大小

- **WHEN** Admin 上傳 PDF 或超過 5MB 的檔案
- **THEN** 系統拒絕該檔案並顯示錯誤訊息（其他合法檔案仍應成功上傳）

#### Scenario: 調整圖片順序

- **WHEN** Admin 拖曳（或以按鈕）調整某商品圖片順序
- **THEN** 系統更新 `DisplayOrder` 並反映於前台商品詳細頁

#### Scenario: 刪除單張圖片

- **WHEN** Admin 於商品編輯頁刪除某張圖片
- **THEN** 系統移除該 `ProductImage` 資料與檔案

### Requirement: 關於我內容編輯

系統 SHALL 於管理區提供關於我頁面內容編輯功能。

#### Scenario: 編輯關於我

- **WHEN** Admin 於 `/admin/about` 修改品牌故事、製作理念、聯絡資訊
- **THEN** 系統儲存並更新前台 `/about` 頁面內容
