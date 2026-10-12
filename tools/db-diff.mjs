// 比對「工作目錄的資料庫」和「git 上最新版本（HEAD）」，列出商品資料改了什麼。
// 資料庫是二進位檔，git diff 看不出內容；收工前用它確認要 commit 的是真的資料變更。
//
// 用法（repo 根目錄）：node --experimental-sqlite tools/db-diff.mjs
// 結束代碼：0 = 有資料變更或沒變更；只有檔案變了但資料一樣時會提示可以還原。
import { execFileSync } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { DatabaseSync } from "node:sqlite";

const dbPath = "JenJenKnits/App_Data/jenjenknits.db";
const status = execFileSync("git", ["status", "--porcelain", "--", dbPath], { encoding: "utf8" }).trim();
if (!status) {
  console.log("資料庫沒有變更。");
  process.exit(0);
}

const headCopy = path.join(os.tmpdir(), `jjk-head-${process.pid}.db`);
fs.writeFileSync(headCopy, execFileSync("git", ["show", `HEAD:${dbPath}`]));

// 讀完就關閉連線：Windows 上沒關的話，暫存檔刪不掉
const read = (file) => {
  const db = new DatabaseSync(file, { readOnly: true });
  try {
    return db.prepare("SELECT * FROM Products ORDER BY Slug").all();
  } finally {
    db.close();
  }
};
const before = read(headCopy);
const after = read(dbPath);
fs.rmSync(headCopy, { force: true });

const labels = {
  Name: "名稱", Category: "分類", Price: "價格", Material: "材質", Dimensions: "尺寸",
  ShortDescription: "簡介", BuyUrl: "賣貨便連結", Featured: "精選排序", ColorSimulator: "配色模擬器",
  StoryMarkdown: "故事", IsPublished: "發布",
};
const show = (v) => (v === null || v === undefined || v === "" ? "（空）" : String(v).replace(/\s+/g, " ").slice(0, 80));

let changes = 0;
for (const row of after) {
  const old = before.find((b) => b.Slug === row.Slug);
  if (!old) {
    console.log(`＋ 新商品 ${row.Slug}「${row.Name}」（${row.IsPublished ? "已發布" : "草稿"}）`);
    changes++;
    continue;
  }
  for (const [key, label] of Object.entries(labels)) {
    if (String(old[key]) !== String(row[key])) {
      console.log(`～ ${row.Slug} ${label}：${show(old[key])} → ${show(row[key])}`);
      changes++;
    }
  }
}
for (const removed of before.filter((b) => !after.some((a) => a.Slug === b.Slug))) {
  console.log(`－ 刪除商品 ${removed.Slug}「${removed.Name}」`);
  changes++;
}

if (changes === 0) {
  console.log("資料庫檔案有變動，但商品資料完全一樣（可能只是啟動網站造成）。");
  console.log(`可以還原：git checkout -- ${dbPath}`);
}
