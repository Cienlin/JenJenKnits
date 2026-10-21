# Design

## Context

- 版面以樣稿為準：`docs/design/admin-redesign/`（`list.html`、`edit.html`、`create.html`、`list-phone.html`、`edit-phone.html`），2026-10-21 使用者確認「照這版做」。樣稿是靜態 HTML，樣式全寫在 inline style，實作時要整理成 `admin.css` 的 class
- 後台用 `_AdminLayout.cshtml`，載入前台的 `site.css`（色彩、字體變數）再加 `admin.css`（375 行）。互動只有 `admin.js` 的故事預覽：按鈕送出整份表單到 `?handler=preview`，伺服器用前台相同的 `StoryRenderer` 回傳 HTML
- 「哪些欄位待補」寫在 `Pages/Admin/Index.cshtml.cs` 的 private `MissingFields`，只有列表用
- 照片由 `ProductFolders.FindImages(slug)` 找出封面、gallery、色卡的網址，編輯頁目前只顯示張數
- 儲存後 `RedirectToPage(new { slug })` 回到同一個編輯頁，並用 `TempData["Notice"]` 顯示「已儲存」
- 測試 `AdminProductTests` 斷言了幾段畫面文字（「草稿」、「缺封面」、「價格」、各欄位錯誤訊息），改版時要保留這些文字
- 公司電腦的 Edge 被政策關閉遠端偵錯，手機版實測用無頭 Chrome

## Goals / Non-Goals

**Goals:**

- 一份 HTML，用 CSS 切換桌機與手機版面，不做兩套頁面
- 沒有 JavaScript 時表單照樣能填、能存（自動預覽、格式按鈕、未儲存提醒都是加分）

**Non-Goals:**

- 照片上傳、排序、刪除商品（後台第二階段）
- 照片縮小版（另一個待辦；後台直接用原圖當縮圖）
- 打字時即時重算完成度（只在載入頁面時計算，見 D3）
- 前端 Markdown 套件、前端框架

## Decisions

### D1：樣式沿用前台變數，後台另外定義狀態色

- 色彩與字體用 `site.css` 的變數（`--paper`、`--card`、`--ink`、`--muted`、`--blue`、`--font-serif`、`--font-mono`）
- 表單與內文改用 Noto Sans TC（Google Fonts，只在後台載入）；頁面與段落標題維持 Noto Serif TC
- 後台新增的變數定義在 `admin.css` 的 `.admin` 上：待補橘（底 `#f7e6cf`、字 `#7a420c`、框 `#b5741f`）、已發布藍底 `#e4e9f5`、輸入框框線 `#8f887b`、卡片框線 `#ddd7cb`。對比度在樣稿階段已檢查：文字至少 4.5:1，輸入框框線至少 3:1
- 「改完記得 commit＋push 資料庫」從頁尾移到頁首，顯示資料最後修改時間（版面已經算好的 `lastUpdated`）。部署到主機後這個提醒就可以拿掉

### D2：版面用 CSS 切換，斷點沿用 52rem

- **列表**：維持 `ul.admin-list`，每個 `li` 用 grid。桌機像表格（上方一列欄位名稱，`aria-hidden`，每格內容本身看得懂，例如「NT$ 450」「精選 No. 1」）；窄於 64rem 時用 `grid-template-areas` 排成卡片（縮圖在左，名稱、狀態、分類價格、待補在右）。列表的欄位多，實作時發現 52rem 放不下 9 欄，所以列表的斷點改成 64rem；手機卡片上照片張數只在缺封面時顯示（張數在編輯頁看）
- 後台的 class 不能和 `site.css` 撞名（實作時 `.steps`、`.sec-no` 撞到前台的購買步驟與章節編號，已改成 `admin-` 開頭）
- **整列可點**：商品名稱是唯一的連結，用 `::after` 延伸蓋住整張卡片（stretched link），讀螢幕軟體只會唸到一個連結
- **編輯頁**：整個 `<form>` 是 grid，桌機兩欄（欄位區＋寬約 340px 的側欄），側欄 `position: sticky` 跟著捲動。側欄裡有兩張卡：「發布與儲存」、「資料完成度」。窄於 52rem 時側欄設 `display: contents`，兩張卡直接參與外層排列：完成度用 `order` 移到最上面，發布與儲存變成 `position: sticky; bottom: 0` 的底部列
  - 替代方案：手機另外放一份底部列。但發布開關與儲存按鈕會有兩個，表單送出時要處理重複欄位，不採用
- 底部列要在頁面最後留出等高的空間，不遮住照片區

### D3：完成度與待補欄位共用同一份規則

- 把 `MissingFields` 移到共用的 `ProductCompleteness`（`Services/`），輸入是六個欄位的值，列表傳 `ProductRecord`、編輯頁傳表單目前的值（`ProductInput`），回傳每一項是否已填。列表與編輯頁的判斷不會不一致
- 編輯頁在伺服器端計算一次：一般進入頁面時用資料庫的值；驗證失敗時用使用者剛輸入的值
- 待補欄位的橘框用 CSS 判斷：欄位有 placeholder，`:placeholder-shown` 才顯示橘框，管理員一開始輸入就恢復一般框線，不用 JavaScript。完成度的數字要等儲存後才更新
  - 替代方案：用 JavaScript 即時重算。要在前端複製一份規則（故事要判斷「渲染後是否為空」，例如只有註解），容易和伺服器不一致，不採用
- 完成度的待補項目是連到欄位 `id` 的錨點連結；欄位上方要留出 sticky 側欄／頁首不會蓋住的捲動距離（`scroll-margin-top`）

### D4：自動預覽沿用既有端點

- 故事欄位停止輸入 0.5 秒後，把整份表單（含 antiforgery token）送到既有的 `?handler=preview`，結果放進預覽區。渲染一律由伺服器的 `StoryRenderer` 做，和前台一致
- 連續輸入時只保留最後一次請求的結果（後送出的覆蓋先送出的，舊的回應晚到就丟掉）
- 手機寬度下編輯區與預覽用「編輯／預覽」兩個按鈕切換（`aria-pressed`），切到預覽時立刻送一次；桌機寬度兩欄並排，切換按鈕隱藏
- 沒有 JavaScript 時：預覽區顯示「需要 JavaScript 才能預覽」，故事照樣能編輯與儲存
  - 替代方案：前端 Markdown 套件即時渲染。要另外對齊 `StoryRenderer` 的規則（註解不顯示、只允許的標籤），不採用

### D5：格式按鈕用 `insertText` 插入，保留復原

- 粗體：用 `**` 包住選取文字，沒選取時插入 `**粗體**` 並選取「粗體」兩字
- 小標題、清單：在選取範圍涵蓋的每一行開頭加 `## `、`- `
- 連結：插入 `[選取的文字](https://)`，並把游標放在網址的位置
- 用 `document.execCommand("insertText")` 插入，瀏覽器的復原（Ctrl+Z）可以撤銷；不支援時改用 `setRangeText` 並送出 `input` 事件。這個 API 已被標為過時，但 Chrome、Edge、Firefox、Safari 都還支援，而且目前沒有其他能保留復原紀錄的方法

### D6：未儲存提醒

- 表單第一次觸發 `input` 或 `change` 事件時標記為「已修改」：側欄與底部列顯示「有未儲存的修改」，並註冊 `beforeunload`，離開前由瀏覽器詢問
- 送出表單（儲存）時取消提醒；「放棄修改」是連回同一頁的連結，點了先取消提醒再離開（瀏覽器重新載入資料庫裡的內容）
- 故事預覽的請求不算修改

### D7：發布開關仍是表單裡的 checkbox

- 維持 `asp-for="Input.IsPublished"` 的 checkbox，加上 `role="switch"`，用 CSS 畫成開關。模型繫結、驗證、測試的表單欄位都不變
- 開關旁的說明依目前狀態顯示：已發布時「前台看得到這件商品」，草稿時「只在後台看得到」；沒有封面時加上「沒有封面照片，前台不會顯示」

### D8：照片區唯讀縮圖

- 依 `FindImages` 的順序顯示封面（較大、外框標示）、gallery、色卡，每張下面標檔名；`loading="lazy"`，`alt` 用「封面」「照片 01」「色卡」
- 下方保留資料夾與檔名規則的說明。之後第二階段的上傳、排序加在同一區

### D9：列表的資料

- `IndexModel` 另外提供已發布、草稿、有待補欄位的件數；每列多傳封面網址（已有 `Images.CoverPath`）
- 更新日期顯示 `MM-dd`，用和前台 `Product.UpdatedOn` 相同的換算（`UpdatedAt` 是 UTC，加 8 小時為台灣時間）；草稿的縮圖淡一點
- 頁首的「資料最後修改時間」目前用 `ToLocalTime()`（伺服器所在時區），一併改成同樣的台灣時間換算，部署到國外主機時也不會差 8 小時

### D10：新增頁

- 上方三個步驟用 `<ol>`，目前步驟加 `aria-current="step"`
- 網址預覽：`Site:BaseUrl` 去掉 `https://` 後接 `/products/`，再接輸入的代稱（`admin.js` 依輸入更新；空白時顯示「代稱」）

## Risks / Trade-offs

- [`admin.css` 大部分重寫，可能弄壞登入頁或錯誤訊息的樣式] → 登入頁、驗證失敗的編輯頁、新增頁的錯誤狀態都列入實際檢查
- [互動功能沒有自動化測試] → 伺服器端的部分（完成度、照片、總覽、預覽端點）有測試；自動預覽、格式按鈕、未儲存提醒、手機底部列用無頭 Chrome 在 390px 與 1280px 實際操作檢查，列在 tasks
- [`execCommand` 過時，未來瀏覽器可能拿掉] → 已有 `setRangeText` 備案，只差在復原紀錄
- [編輯頁一次載入 10 張原圖（每張約 250KB）] → 只在後台、本機使用，`loading="lazy"`；照片縮小版做好後一起改用
- [`display: contents` 讓側欄在手機上失去 `<aside>` 的地標語意] → 側欄用 `<div>`，兩張卡各自有標題，不依賴地標

## Migration Plan

純前端與畫面改動，沒有資料庫或網址的變動。完成後 commit、push 即可；GitHub Pages 不用重新輸出（後台不會被輸出）。退回：revert 相關 commit。
