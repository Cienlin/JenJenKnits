## MODIFIED Requirements

### Requirement: 結構化資料

系統 SHALL 以 JSON-LD 提供搜尋引擎可讀的結構化資料。

#### Scenario: 商品頁

- **WHEN** 搜尋引擎讀取商品詳細頁
- **THEN** 頁面包含 Product 資料：名稱、描述、所有商品照片的絕對網址、品牌、分類；有價格時另含以新台幣計價的 Offer 與該頁網址

#### Scenario: 首頁

- **WHEN** 搜尋引擎讀取首頁
- **THEN** 頁面包含品牌（Organization）資料：名稱、網址、標誌；已設定 Instagram 時列為品牌的社群帳號
- **AND** 頁面包含網站（WebSite）資料：名稱「織物製造所」、別名「JH Craft Studio」、網址為首頁的絕對網址，並標示網站的發行者是同一個品牌

#### Scenario: 未設定 Instagram

- **WHEN** 搜尋引擎讀取首頁，且網站沒有設定 Instagram 帳號
- **THEN** 品牌資料不包含社群帳號欄位（不輸出空值）
