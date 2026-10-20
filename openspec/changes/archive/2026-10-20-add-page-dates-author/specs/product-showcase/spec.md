# Spec Delta

## ADDED Requirements

### Requirement: 頁面作者與更新日期

系統 SHALL 標明前台頁面的作者是品牌「織物製造所」，並在有更新日期的頁面上，於頁尾版權頁顯示該頁的更新日期。日期一律以台灣時間計算，格式為 `YYYY-MM-DD`，與結構化資料的更新日期一致。

#### Scenario: 作者

- **WHEN** 訪客或搜尋引擎讀取任一前台頁面
- **THEN** 頁面標示作者為「織物製造所」

#### Scenario: 商品頁的更新日期

- **WHEN** 訪客瀏覽 `/products/{slug}`
- **THEN** 頁尾版權頁顯示該商品資料最後更新的日期

#### Scenario: 首頁的更新日期

- **WHEN** 訪客瀏覽 `/`
- **THEN** 頁尾版權頁顯示首頁精選商品與常見問題之中最新的更新日期

#### Scenario: 沒有更新日期的頁面

- **WHEN** 訪客瀏覽沒有更新日期的頁面（例如作品列表、找不到頁面）
- **THEN** 頁尾版權頁不顯示更新日期這一列

### Requirement: 頁尾聯絡方式

系統 SHALL 在已設定 Instagram 時，於每個前台頁面的頁尾版權頁顯示聯絡方式。

#### Scenario: 已設定 Instagram

- **WHEN** 訪客瀏覽任一前台頁面，且網站有設定 Instagram 帳號
- **THEN** 頁尾版權頁顯示「聯絡」一列，內容為 Instagram 帳號名稱並說明以私訊聯絡，點選後在新分頁開啟 Instagram
- **AND** 這個連結在手機上的點擊高度至少 44px

#### Scenario: 未設定 Instagram

- **WHEN** 網站沒有設定 Instagram 帳號
- **THEN** 頁尾版權頁不顯示「聯絡」這一列

## MODIFIED Requirements

### Requirement: 結構化資料

系統 SHALL 以 JSON-LD 提供搜尋引擎可讀的結構化資料。

#### Scenario: 商品頁

- **WHEN** 搜尋引擎讀取商品詳細頁
- **THEN** 頁面包含 Product 資料：名稱、描述、所有商品照片的絕對網址、品牌、分類；有價格時另含以新台幣計價的 Offer 與該頁網址
- **AND** 頁面包含商品頁（ItemPage）資料：該頁網址、主要內容為這件商品、作者為品牌、更新日期與頁尾顯示的相同

#### Scenario: 首頁

- **WHEN** 搜尋引擎讀取首頁
- **THEN** 頁面包含品牌資料，類型同時標為組織（Organization）與網路商店（OnlineStore）：名稱「織物製造所」、別名「JH Craft Studio」、網址、標誌；已設定 Instagram 時列為品牌的社群帳號，並列為以繁體中文服務的客服聯絡窗口
- **AND** 頁面包含網站（WebSite）資料：名稱「織物製造所」、別名「JH Craft Studio」、網址為首頁的絕對網址，並標示網站的發行者是同一個品牌
- **AND** 頁面包含首頁的頁面（WebPage）資料：首頁網址、屬於上述網站、作者為品牌、更新日期與頁尾顯示的相同

#### Scenario: 未設定 Instagram

- **WHEN** 搜尋引擎讀取首頁，且網站沒有設定 Instagram 帳號
- **THEN** 品牌資料不包含社群帳號與聯絡窗口欄位（不輸出空值）

#### Scenario: 首頁常見問題

- **WHEN** 搜尋引擎讀取首頁，且首頁顯示常見問題章節
- **THEN** 首頁的頁面資料同時是常見問題（FAQPage），列出與頁面上相同的每一題題目與答案，文字與順序都和頁面上看得到的一致

#### Scenario: 沒有常見問題

- **WHEN** 搜尋引擎讀取首頁，且網站沒有任何常見問題
- **THEN** 頁面不包含常見問題（FAQPage）資料

### Requirement: Sitemap 與 robots.txt

系統 SHALL 提供 `/sitemap.xml` 與 `/robots.txt` 協助搜尋引擎收錄。

#### Scenario: Sitemap 內容

- **WHEN** 請求 `/sitemap.xml`
- **THEN** 回應列出首頁、`/products` 與所有前台可見商品的絕對網址，商品附最後修改日期（台灣時間，和商品頁頁尾顯示的相同）；草稿與缺封面的商品不列入

#### Scenario: robots.txt 內容

- **WHEN** 請求 `/robots.txt`
- **THEN** 回應允許收錄前台、禁止 `/admin`，並標示 sitemap 的絕對網址
