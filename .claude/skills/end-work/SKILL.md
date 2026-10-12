---
name: end-work
description: 收工流程：確認所有變更（含後台改的資料庫）都 commit 並 push、測試通過、把今天的進度與決定寫進 docs/status.md，需要時更新 GitHub Pages。Use when the user says 收工、下班了、要離開了、要換電腦、確認東西都有上 git.
---

# 收工

使用者要離開這台電腦，之後可能在另一台電腦接著做。目標是：**另一台電腦 `git pull` 之後，程式碼、商品資料和今天討論的結論都完整**。依序做下面的事，最後用繁體中文報告。

## 1. 看有哪些變更

```bash
git status -sb --untracked-files=all
git log --oneline origin/main..HEAD      # 已 commit 但還沒 push 的
```

把變更分類：程式碼、文件、資料庫、照片（`JenJenKnits/wwwroot/products/`）、其他。

## 2. 資料庫（後台改的商品資料）

`JenJenKnits/App_Data/jenjenknits.db` 有變更時：

```bash
node --experimental-sqlite --no-warnings tools/db-diff.mjs
```

- 有真的資料變更 → 列給使用者看，之後跟著 commit（commit 訊息寫出改了哪件商品的哪些欄位，例如 `content: 補網格托特包價格與材質`）。
- 只有檔案變了、資料一樣 → 還原：`git checkout -- JenJenKnits/App_Data/jenjenknits.db`。
- 新放的照片要一起 commit。

## 3. 程式碼有改 → 先跑測試

```bash
dotnet test
```

- 網站在 5178 執行中時 build 會因 exe 被鎖住失敗：先問使用者能不能關掉網站，或請使用者自己關。
- 測試失敗 → 不要 commit 壞掉的程式碼，告訴使用者失敗的測試與原因，讓使用者決定。

## 4. 更新進度文件

把今天做了什麼、做了哪些決定、還有什麼沒做完，更新到 `docs/status.md`（更新「最後更新」日期）。只在對話中討論、但會影響之後工作的結論，也要寫進去；屬於某個 change 的設計討論，寫進該 change 的 `design.md`。先把要寫的重點用幾行列給使用者確認。

沒有新進度（例如只是改資料）就不用改文件。

## 5. Commit 並 push

- 依內容分成幾個有意義的 commit（程式碼／內容資料／文件），訊息結尾加上專案規定的 Co-Authored-By 行
- 不要 commit：`bin/`、`obj/`、`site-export/`、密碼或雜湊值
- `git push`，然後確認 `git status -sb` 顯示與 `origin/main` 一致、`git log origin/main..HEAD` 為空

## 6. 需要的話更新 GitHub Pages

前台看得到的內容有變（商品資料、照片、前台程式碼）時，問使用者要不要更新展示網站。要的話（網站需在 5178 執行中）：

```bash
bash tools/deploy-pages.sh
```

## 7. 報告

用幾行告訴使用者：

1. 推了哪些 commit（一句話說明各自內容）
2. 資料庫改了什麼（沒有就說沒有）
3. GitHub Pages 有沒有更新
4. 下一次開工要接著做的事（就是 `docs/status.md` 裡的下一步）
5. 換到另一台電腦時：先開工（`git pull`）；那台若還沒設管理員帳號要先設
