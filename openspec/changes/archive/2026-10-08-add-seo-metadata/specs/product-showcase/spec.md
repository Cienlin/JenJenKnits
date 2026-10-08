## ADDED Requirements

### Requirement: 社群分享預覽

系統 SHALL 在每個前台頁面輸出社群分享資訊，讓連結貼到 IG、Threads、LINE、Facebook 時顯示標題、描述與照片；所有網址皆為以網站對外網址組成的絕對網址。

#### Scenario: 分享商品頁

- **WHEN** 有人分享 `/products/{slug}` 的連結
- **THEN** 頁面提供該商品名稱為標題、商品簡介為描述（未填時用網站預設描述）、商品封面為照片、類型為商品，以及該頁的絕對網址

#### Scenario: 分享首頁或作品列表

- **WHEN** 有人分享首頁或 `/products` 的連結
- **THEN** 頁面提供該頁標題、該頁描述與網站預設分享照片

### Requirement: Canonical 網址

系統 SHALL 在每個前台頁面標示唯一的 canonical 絕對網址，避免同一內容因不同網址或不同主機被視為重複頁面。

#### Scenario: 一般頁面

- **WHEN** 訪客瀏覽任一前台頁面
- **THEN** 頁面的 canonical 為網站對外網址加上該頁路徑，不含查詢字串

#### Scenario: 分類篩選

- **WHEN** 訪客瀏覽 `/products?category={分類}`
- **THEN** 頁面的 canonical 指向 `/products`

### Requirement: 結構化資料

系統 SHALL 以 JSON-LD 提供搜尋引擎可讀的結構化資料。

#### Scenario: 商品頁

- **WHEN** 搜尋引擎讀取商品詳細頁
- **THEN** 頁面包含 Product 資料：名稱、描述、所有商品照片的絕對網址、品牌、分類；有價格時另含以新台幣計價的 Offer 與該頁網址

#### Scenario: 首頁

- **WHEN** 搜尋引擎讀取首頁
- **THEN** 頁面包含品牌（Organization）資料：名稱、網址、標誌；已設定 Instagram 時列為品牌的社群帳號

### Requirement: Sitemap 與 robots.txt

系統 SHALL 提供 `/sitemap.xml` 與 `/robots.txt` 協助搜尋引擎收錄。

#### Scenario: Sitemap 內容

- **WHEN** 請求 `/sitemap.xml`
- **THEN** 回應列出首頁、`/products` 與所有前台可見商品的絕對網址，商品附最後修改日期；草稿與缺封面的商品不列入

#### Scenario: robots.txt 內容

- **WHEN** 請求 `/robots.txt`
- **THEN** 回應允許收錄前台、禁止 `/admin`，並標示 sitemap 的絕對網址
