#!/usr/bin/env bash
# 更新 GitHub Pages 展示網站：輸出靜態網站並強制推到 gh-pages 分支。
# 用法（repo 根目錄，網站需在 http://localhost:5178 執行中）：bash tools/deploy-pages.sh
set -euo pipefail

root="$(git rev-parse --show-toplevel)"
out="$root/site-export"
commit="$(git -C "$root" rev-parse --short HEAD)"

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
git add -A
git commit -q -m "deploy: static export of $commit"
git push -q -f "$(git -C "$root" remote get-url origin)" gh-pages
echo "已推到 gh-pages（$commit），約 1 分鐘後在 https://cienlin.github.io/JenJenKnits/ 生效。"
