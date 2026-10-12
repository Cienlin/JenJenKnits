---
name: start-work
description: 開工流程：同步另一台電腦的進度、檢查環境、讀進度文件並建議今天要做什麼。Use when the user says 開工、開始工作、繼續上次的進度、我回來了, or starts a new session on this project.
---

# 開工

使用者在兩台電腦（公司、家裡）之間切換開發，另一台電腦上的 Claude 看不到這邊的對話，所有進度都靠 repo 裡的文件同步。開工時依序做下面的事，最後用繁體中文給一份簡短的報告。

## 1. 同步程式碼

在 repo 根目錄：

```bash
git status -sb
git fetch --prune
git log --oneline HEAD..origin/main     # 另一台電腦推上來、這台還沒有的 commit
```

- 工作目錄乾淨 → `git pull --ff-only`，並列出拉下來的 commit（這就是另一台電腦做了什麼）。
- 工作目錄**有未 commit 的變更** → 不要 pull，先告訴使用者是哪些檔案。若是 `JenJenKnits/App_Data/jenjenknits.db`，執行 `node --experimental-sqlite --no-warnings tools/db-diff.mjs` 看是真的資料變更還是只是啟動網站造成的，再請使用者決定 commit 或還原。
- 不在 `main` 分支 → 提醒使用者（專案只用 `main`；`gh-pages` 是產生出來的）。

## 2. 讀進度

- `docs/status.md`：已完成、進行中、待辦、已討論過的決定
- `openspec list`：進行中的 change；有的話讀它的 `proposal.md` 和 `design.md`（探索中的草稿也在這裡）
- 品牌與社群素材在 `docs/brand/`，需要時再讀

## 3. 檢查這台電腦的環境

```bash
dotnet --list-sdks                      # 需要 9.x
node --version                          # 22 以上（輸出腳本、db-diff 用）
cd JenJenKnits && dotnet user-secrets list   # 只確認 Admin:UserName / Admin:PasswordHash 有沒有設，不要把雜湊值印出來
```

- 沒設定管理員帳號 → 提醒使用者執行 `dotnet run -- hash-password`，再用 `dotnet user-secrets set` 設定（README「後台」段落）。
- 檢查 5178 埠是否已有網站在跑（`Get-NetTCPConnection -LocalPort 5178` 或 `curl -s localhost:5178`）：網站執行中時 `dotnet build`／`dotnet test` 會因為 exe 被鎖住而失敗。

## 4. 報告與建議

用幾行告訴使用者：

1. 另一台電腦這段期間推了什麼（沒有就說沒有）
2. 目前階段與進行中的 change
3. 環境有沒有要處理的（帳號、未 commit 的檔案）
4. 依 `docs/status.md` 建議今天的下一步（一到兩個選項），並建議適合的 effort：文案／問答用 low、一般開發用 medium、架構／部署／資安用 high

不要自動開始實作，等使用者決定今天要做什麼。

## 使用者偏好

- 一律用繁體中文回答；使用者說「簡單回答」時就簡短
- UI 相關要用 390px 手機實際操作檢查，不只看有沒有水平捲軸
- 前台風格已定（編織誌），新頁面沿用，不重新設計
- 新功能走 OpenSpec：propose → apply → archive
