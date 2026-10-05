## Why

`add-showcase-mvp` 的「鉤織花邊」視覺（D12 / D15）上線前試看，覺得太接近一般購物網站、少了品牌個性；首頁也放了太多商品頁層級的細節。2026-10-05 比較了六組樣稿（`D:\jenjenknits\design-mockups\`，不在 repo 內）後，選定 **A「編織誌」**：把網站做成一本印刷的鉤織書，首頁只留「作品」與「購買方式」，商品細節集中到商品頁。

## What Changes

- **全站視覺改為「編織誌」**：米色紙張底、墨色文字、藏青 `#1F3C8E` 單色點綴；字體改為 Noto Serif TC（中文）、Instrument Serif（數字與英文）、IBM Plex Mono（小標）；照片加裁切線與圖號（Fig. 01）
- **頁首改為刊頭**：中央英文 logo、右側導覽（作品／購買方式）、下方一條 22 色線材印刷色條（取代首頁的 22 色點互動）
- **首頁**：
  - 第一屏改為直排「貞心卉織」大標 + 主照片 + 目錄
  - 精選作品改為雜誌跨頁：主照片、編號與分類、名稱、簡介、規格表、最多 4 張 gallery 小圖，奇偶交錯左右排
  - **BREAKING**：移除「關於」區塊（品牌故事），頁首與頁尾導覽一併拿掉「關於」連結
- **商品列表頁**：改為目錄式排版，分類篩選改為文字列
- **商品頁**：
  - 照片輪播加上下一張／上一張與圖號計數；資訊欄在桌機上跟著捲動
  - 故事改為獨立的「01 故事」章節（首字放大），色卡與配色模擬器為「02 可選顏色」章節
  - 新增「下一件作品」連結
  - 配色模擬器改為「先選主色或配色，再點色票」；主色／配色鈕同時顯示色號，手機上預覽固定在畫面上方
- **頁尾改為版權頁**：品牌、內容、線材色卡、販售管道與版本號
- **移除**：首頁 22 色點滑動換色、主照片泡芙小花遮罩、區塊色帶與扇形花邊、作品卡片上下錯落、Huninn 與 Noto Sans TC 字體

不變的部分：商品資料（`wwwroot/products/` 資料夾慣例、`meta.json`、`story.md`）、路由、購買按鈕三段切換、換頁轉場、版本號。

## Capabilities

### New Capabilities

（無）

### Modified Capabilities

- `product-showcase`：首頁不再顯示品牌故事、精選作品改為跨頁呈現；商品頁新增「下一件作品」、故事與可選顏色成為獨立章節、桌機版資訊欄不再包含故事；配色模擬器改為先選部位再選色並在手機上固定預覽

> `product-showcase` 目前仍在 `add-showcase-mvp` 內（尚未歸檔）。歸檔順序：先歸檔 `add-showcase-mvp`，再歸檔本 change。

## Impact

**修改**
- `wwwroot/css/site.css`：整份改寫
- `wwwroot/js/site.js`：移除 22 色點互動；照片輪播加上箭頭與計數；配色模擬器改寫
- `Pages/Shared/_Layout.cshtml`：刊頭、印刷色條、版權頁、字體
- `Pages/Index.cshtml`：第一屏、跨頁、購買方式；移除關於
- `Pages/Products/Index.cshtml`、`Pages/Shared/_ProductCard.cshtml`：目錄式列表與卡片
- `Pages/Products/Detail.cshtml(.cs)`：章節化、下一件作品
- `Pages/Shared/_PuffFlowerSimulator.cshtml`：主色／配色鈕 + 色票
- `Pages/NotFound.cshtml`、`Pages/Error.cshtml`：套用新樣式

**不受影響**
- `Models/`、`Services/`、`Program.cs`、商品資料夾格式
