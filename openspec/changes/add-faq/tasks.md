# Tasks

## 1. 跟貞貞確認答案（D6，不用寫程式，可以和 2～4 同時進行）

- [ ] 1.1 把 design.md D6 的訊息傳給貞貞（含第 5、6 題與品牌故事、聯絡方式的問題）；收到回覆後把確認過的答案（與她補充的題目）記在 D6 表格，沒確認的題目標「不放」；品牌故事與聯絡方式的回覆記到 `docs/status.md`，給之後的 change 用

## 2. 品牌別名與首頁標題（D3、D7）

- [x] 2.1 `Index.cshtml` 的 Organization 加 `alternateName`「JH Craft Studio」；`SeoTests` 首頁測試斷言 Organization 的 alternateName；`dotnet test` 全部通過
- [x] 2.2 `_Layout.cshtml` 的預設標題與描述改為 D7 的文字（色數用 `YarnPalette.Colors.Count`）；`SeoTests` 首頁測試斷言 `<title>`、`og:title` 以「織物製造所｜」開頭並包含「水壺提網」，`description`、`og:description` 包含「JH Craft Studio」與色數；`dotnet test` 全部通過

## 3. 常見問題資料與結構化資料（D2、D3、D4）

- [x] 3.1 新增常見問題資料型別（題目、答案）與預設清單（先放空的），啟動時註冊成服務；`SiteFactory` 新增在第一次 `Client()` 之前指定常見問題的方法；既有測試全部通過
- [x] 3.2 首頁 `@graph` 在有題目時加入 FAQPage（`@id` 為首頁 `#faq`，每題是 Question＋acceptedAnswer Answer），沒有題目時不加節點、陣列裡也沒有 `null`；`SeoTests` 新增：兩題（其中一題答案含 `</script>`）時 FAQPage 的題目、答案與順序正確；空清單時 `@graph` 沒有 FAQPage；`dotnet test` 全部通過

## 4. 首頁「03 常見問題」章節（D1、D5）

- [x] 4.1 `Index.cshtml` 在 `#buy` 之後加 `#faq` 章節（`.sec-head`：03／常見問題／FAQ；每題題號 `Q.01`、`<h3>` 題目、答案段落），目錄加「03 常見問題」；沒有題目時章節與目錄項目都不輸出；新增測試：有題目時頁面上依序看得到每題題目與答案、目錄有連到 `#faq` 的項目；空清單時兩者都沒有；`dotnet test` 全部通過
- [x] 4.2 `site.css` 加常見問題樣式：手機上題號、題目、答案由上往下排、上下細線分隔，桌機（52rem 以上）題目在左、答案在右；答案行高 ≥ 1.5。用 390px 手機實際操作：目錄點「常見問題」能跳到章節、每題容易讀、沒有水平捲軸；桌機 1280px 看一次排版

## 5. 放進答案並上線

- [ ] 5.1 把 1.1 確認過的答案依 D6 的順序放進預設清單；`dotnet run` 後首頁看得到章節，頁面文字與 JSON-LD 的答案一字不差
- [ ] 5.2 `docs/status.md`：「之後」移除 AEO 這項、「已完成」加上這次的 change；待辦加「開賣（填上賣貨便連結）時改常見問題第 4 題」「商品類型改變時改首頁標題與描述」「上線 2～3 週後看 Search Console 的查詢字詞」
- [ ] 5.3 commit、push，執行 `bash tools/deploy-pages.sh`；用 Schema Markup Validator（validator.schema.org）檢查 `https://jhcraftstudio.com/`，Organization（含別名）、WebSite、FAQPage 都被讀到、沒有錯誤；再用 ultralab.tw 重新掃一次首頁，記下 AEO 分數的變化
- [ ] 5.4 Search Console 與 Bing Webmaster Tools 對首頁重新要求建立索引；`docs/status.md` 記下日期
