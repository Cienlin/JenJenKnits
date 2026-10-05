// 把執行中的網站輸出成靜態檔案，給 GitHub Pages 用。
//
// 用法（先在 JenJenKnits 資料夾 `dotnet run --launch-profile http`）：
//   node tools/export-static.mjs [輸出資料夾] [網站網址] [子路徑]
//   預設：./site-export  http://localhost:5178  /JenJenKnits
//
// 做的事：從首頁開始爬所有站內頁面，存成 資料夾/index.html；下載頁面用到的 CSS、JS、圖片；
// 所有站內連結加上 GitHub Pages 的子路徑；分類篩選 /products?category=X 改成 /products/category/X/；
// /not-found 存成 404.html。最後檢查每個站內連結都對得到檔案。
import fs from "node:fs/promises";
import path from "node:path";

const [out = "site-export", origin = "http://localhost:5178", base = "/JenJenKnits"] = process.argv.slice(2);
const isAsset = (p) => /\.(css|js|png|jpe?g|webp|gif|svg|ico|woff2?)$/i.test(p);

// 站內網址 → 輸出後的網址（含子路徑）
const rewrite = (url) => {
  const [p, hash = ""] = url.split("#");
  const [pathname, query = ""] = p.split("?");
  const category = new URLSearchParams(query).get("category");
  const target = category ? `/products/category/${encodeURIComponent(category)}/` : isAsset(pathname) ? p : pathname;
  return base + target + (hash ? `#${hash}` : "");
};

// 頁面網址 → 輸出檔案路徑
const pageFile = (url) => {
  const [pathname, query = ""] = url.split("?");
  const category = new URLSearchParams(query).get("category");
  const dir = category ? `products/category/${category}` : pathname.replace(/^\/|\/$/g, "");
  return path.join(out, dir, "index.html");
};

await fs.rm(out, { recursive: true, force: true });
const queue = ["/"], seen = new Set(queue), assets = new Set();

const save = async (file, data) => {
  await fs.mkdir(path.dirname(file), { recursive: true });
  await fs.writeFile(file, data);
};

const exportPage = async (url, file) => {
  const res = await fetch(origin + url);
  if (!res.ok) throw new Error(`${url} → HTTP ${res.status}`);
  let html = await res.text();
  for (const [, link] of html.matchAll(/(?:href|src)="(\/[^"]*)"/g)) {
    const clean = link.replace(/&amp;/g, "&").split("#")[0];
    if (isAsset(clean.split("?")[0])) assets.add(clean);
    else if (clean && !seen.has(clean)) { seen.add(clean); queue.push(clean); }
  }
  html = html.replace(/(href|src)="(\/[^"]*)"/g, (_, attr, link) => `${attr}="${rewrite(link.replace(/&amp;/g, "&"))}"`);
  await save(file, html);
};

while (queue.length) {
  const url = queue.shift();
  await exportPage(url, pageFile(url));
}
await exportPage("/not-found", path.join(out, "404.html"));

for (const url of assets) {
  const res = await fetch(origin + url);
  if (!res.ok) throw new Error(`${url} → HTTP ${res.status}`);
  await save(path.join(out, url.split("?")[0]), Buffer.from(await res.arrayBuffer()));
}
await save(path.join(out, ".nojekyll"), "");

// 檢查：每個站內連結都要對得到輸出的檔案
const missing = [];
const files = await fs.readdir(out, { recursive: true });
for (const f of files.filter((f) => f.endsWith(".html"))) {
  const html = await fs.readFile(path.join(out, f), "utf8");
  for (const [, link] of html.matchAll(new RegExp(`(?:href|src)="${base}(/[^"#?]*)`, "g"))) {
    const target = path.join(out, decodeURIComponent(link), isAsset(link) ? "" : "index.html");
    await fs.access(target).catch(() => missing.push(`${f} → ${link}`));
  }
}
console.log(`pages: ${seen.size} + 404, assets: ${assets.size}, missing links: ${missing.length}`);
if (missing.length) {
  console.log(missing.join("\n"));
  process.exit(1);
}
