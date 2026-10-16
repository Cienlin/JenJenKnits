// 把執行中的網站輸出成靜態檔案，給 GitHub Pages 用。
//
// 用法（先在 JenJenKnits 資料夾 `dotnet run --launch-profile http`）：
//   node tools/export-static.mjs [輸出資料夾] [網站網址]
//   預設：./site-export  http://localhost:5178
//
// 對外網址（Site:BaseUrl）從網站的 robots.txt 取得，不另外指定：網址有子路徑時（例如 https://cienlin.github.io/JenJenKnits）
// 站內連結都加上子路徑；網域不是 *.github.io 時另外輸出 CNAME（GitHub Pages 的自訂網域）。
//
// 做的事：從首頁開始爬所有站內頁面（另外輸出 sitemap.xml、robots.txt 與社群分享照片），存成 資料夾/index.html；下載頁面用到的 CSS、JS、圖片；
// 所有站內連結加上子路徑；分類篩選 /products?category=X 改成 /products/category/X/；
// /not-found 存成 404.html。最後檢查每個站內連結都對得到檔案。
import fs from "node:fs/promises";
import path from "node:path";

const [out = "site-export", origin = "http://localhost:5178"] = process.argv.slice(2);
const isAsset = (p) => /\.(css|js|png|jpe?g|webp|gif|svg|ico|woff2?)$/i.test(p);
const escapeRegExp = (s) => s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

// robots.txt 的 Sitemap 是 Site:BaseUrl + /sitemap.xml
const robots = await fetch(`${origin}/robots.txt`);
if (!robots.ok) throw new Error(`/robots.txt → HTTP ${robots.status}`);
const sitemapUrl = (await robots.text()).match(/^Sitemap: (\S+)$/m)?.[1];
if (!sitemapUrl?.endsWith("/sitemap.xml")) throw new Error("/robots.txt 找不到 Sitemap 網址");
const baseUrl = sitemapUrl.slice(0, -"/sitemap.xml".length);
const { hostname, pathname } = new URL(sitemapUrl);
const base = pathname.slice(0, -"/sitemap.xml".length); // 子路徑；網域根目錄時是空字串

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
  // 社群分享照片是絕對網址（Site:BaseUrl），不一定有 <img> 用到，也要一起輸出
  for (const [, link] of html.matchAll(new RegExp(`content="${escapeRegExp(baseUrl)}(/[^"]+)"`, "g"))) {
    if (isAsset(link)) assets.add(link);
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
// 自訂網域記在發布分支根目錄的 CNAME；deploy-pages.sh 每次都重建分支，所以每次輸出都要帶上
if (!hostname.endsWith(".github.io")) await save(path.join(out, "CNAME"), `${hostname}\n`);

// 搜尋引擎用的檔案：內容已是絕對網址，原樣輸出
for (const file of ["sitemap.xml", "robots.txt"]) {
  const res = await fetch(`${origin}/${file}`);
  if (!res.ok) throw new Error(`/${file} → HTTP ${res.status}`);
  await save(path.join(out, file), await res.text());
}

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
console.log(`base: ${baseUrl}, pages: ${seen.size} + 404, assets: ${assets.size}, missing links: ${missing.length}`);
if (missing.length) {
  console.log(missing.join("\n"));
  process.exit(1);
}
