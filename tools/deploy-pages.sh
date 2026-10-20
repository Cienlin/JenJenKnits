#!/usr/bin/env bash
# 更新 GitHub Pages 展示網站：輸出靜態網站並強制推到 gh-pages 分支，再用 IndexNow 通知搜尋引擎哪些網址變了。
# 用法（repo 根目錄，網站需在 http://localhost:5178 執行中）：bash tools/deploy-pages.sh
#   --dry-run：只輸出並列出會通知 IndexNow 的網址，不推、不送
set -euo pipefail

dry_run=""
[[ "${1:-}" == "--dry-run" ]] && dry_run="--dry-run"

root="$(git rev-parse --show-toplevel)"
out="$root/site-export"
commit="$(git -C "$root" rev-parse --short HEAD)"
remote="$(git -C "$root" remote get-url origin)"

if ! curl -sf -o /dev/null http://localhost:5178/; then
  echo "網站沒有在 http://localhost:5178 執行。先在 JenJenKnits 資料夾執行：dotnet run --launch-profile http" >&2
  exit 1
fi

rm -rf "$out"
node "$root/tools/export-static.mjs" "$out"

cd "$out"
git init -q -b gh-pages
git config core.autocrlf false
git config user.name "$(git -C "$root" config user.name)"
git config user.email "$(git -C "$root" config user.email)"
# 目前線上的版本，IndexNow 用它比對哪些網址變了；第一次部署時沒有
previous=""
git fetch -q --depth 1 "$remote" gh-pages 2>/dev/null && previous="$(git rev-parse FETCH_HEAD)"
git add -A
git commit -q -m "deploy: static export of $commit"

if [[ -n "$dry_run" ]]; then
  node "$root/tools/indexnow.mjs" "$out" "$previous" --dry-run
  echo "預覽完成，沒有推到 gh-pages。"
  exit 0
fi

git push -q -f "$remote" gh-pages
site="https://$(cat "$out/CNAME" 2>/dev/null || echo cienlin.github.io/JenJenKnits)"  # 沒有自訂網域時是 GitHub Pages 的預設網址
echo "已推到 gh-pages（$commit），約 1 分鐘後在 $site 生效。"
# 通知失敗不影響部署（indexnow.mjs 自己會印出原因與手動提交的方法）
node "$root/tools/indexnow.mjs" "$out" "$previous" || echo "IndexNow 通知沒有完成，部署不受影響。"
