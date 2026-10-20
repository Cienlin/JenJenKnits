// IndexNow：部署 GitHub Pages 時，通知搜尋引擎（Bing、Yandex 等；Google 不支援）哪些網址新增、更新、刪除。
//
// 用法（deploy-pages.sh 會呼叫，通常不用自己跑）：
//   node tools/indexnow.mjs <輸出資料夾> <上一版的 commit> [--dry-run]
// 輸出資料夾是 deploy-pages.sh 建的 git repo（site-export/）；上一版是目前 gh-pages 的 commit，第一次部署時給空字串。
// --dry-run：只列出會通知的網址，不送出。
//
// 金鑰在 tools/indexnow-key.txt，export-static.mjs 會把它放在網站根目錄 /{金鑰}.txt。
// 金鑰本來就公開在網站上，只用來證明通知是網站擁有者送的，不是密碼；要換就改這個檔再部署。
import { execFileSync } from "node:child_process";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

export const labels = { added: "新增", updated: "更新", deleted: "刪除" };

/** sitemap.xml 裡的網址（只有正式網址，分類篩選頁、404 頁不在裡面） */
export const sitemapUrls = (xml) => [...xml.matchAll(/<loc>([^<]+)<\/loc>/g)].map((m) => m[1]);

/** 頁面網址 → 輸出資料夾裡的檔案：https://example.com/products/x/ → products/x/index.html */
export const urlToFile = (url) => {
  const dir = decodeURIComponent(new URL(url).pathname).replace(/^\/|\/$/g, "");
  return dir ? `${dir}/index.html` : "index.html";
};

/**
 * 去掉每次部署都會變、但不是內容的部分：
 * - 頁尾的版本與 commit（_Layout.cshtml 的 <p class="mono version">；b-… 是 CSS 隔離屬性）
 * - CSS、JS 檔名裡的內容指紋（MapStaticAssets：site.djzafdnge5.css）。圖片的指紋保留，圖片換了要通知
 * 這兩種寫法改了要一起改這裡；改漏的結果是每一頁都被當成有變動（多送，不會漏送），--dry-run 看得出來。
 */
export const normalize = (html) => html
  .replace(/(<p\b[^>]*class="mono version"[^>]*>)[^<]*(<\/p>)/, "$1$2")
  .replace(/\.[a-z0-9]{10}\.(css|js)"/g, '.$1"');

/**
 * 比對上一版（old）和這次輸出（new），列出要通知的網址。
 * read(version, file) 讀某一版的檔案，沒有時回傳 null；沒有上一版時，新 sitemap 的網址全部算新增。
 */
export const findChanges = async (read) => {
  const now = sitemapUrls((await read("new", "sitemap.xml")) ?? "");
  const oldSitemap = await read("old", "sitemap.xml");
  if (oldSitemap === null) return now.map((url) => ({ url, kind: "added" }));

  const before = sitemapUrls(oldSitemap);
  const changes = [];
  for (const url of now) {
    if (!before.includes(url)) {
      changes.push({ url, kind: "added" });
      continue;
    }
    const [previous, current] = [await read("old", urlToFile(url)), await read("new", urlToFile(url))];
    if (previous === null || current === null || normalize(previous) !== normalize(current)) {
      changes.push({ url, kind: "updated" });
    }
  }
  for (const url of before.filter((url) => !now.includes(url))) changes.push({ url, kind: "deleted" });
  return changes;
};

const responses = {
  200: "成功",
  202: "已收到，金鑰還在驗證中（第一次送出時正常）",
  400: "格式錯誤",
  403: "金鑰無效：搜尋引擎找不到網站上的金鑰檔，或內容不符",
  422: "網址不屬於這個網域，或金鑰格式不符",
  429: "送太頻繁，被當成濫發",
};

/** IndexNow 的回應碼 → 這次通知算不算成功、怎麼跟使用者說 */
export const describeResponse = (status) => ({
  accepted: status === 200 || status === 202,
  message: responses[status] ?? "非預期的回應",
});

const pause = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

/**
 * 等 GitHub Pages 換成新版：挑一個新增或更新的網址，抓線上頁面直到和這次輸出一樣；只有刪除時，等它回 404。
 * 網址加上不重複的查詢字串，避開 CDN 快取（max-age=600）。換成新版回傳 true，超過時間回傳 false。
 */
export const waitUntilLive = async (changes, readNew, { fetchPage = fetch, interval = 10_000, timeout = 300_000 } = {}) => {
  const probe = changes.find((change) => change.kind !== "deleted") ?? changes[0];
  const expected = probe.kind === "deleted" ? null : normalize(await readNew(urlToFile(probe.url)));
  const deadline = Date.now() + timeout;
  for (;;) {
    try {
      const response = await fetchPage(`${probe.url}?indexnow=${Date.now()}`, { signal: AbortSignal.timeout(15_000) });
      if (expected === null ? response.status === 404 : response.ok && normalize(await response.text()) === expected) return true;
    } catch {
      // 網路暫時不通就下一輪再試
    }
    if (Date.now() + interval > deadline) return false;
    await pause(interval);
  }
};

const main = async () => {
  const [out, previous = "", ...flags] = process.argv.slice(2);
  if (!out) throw new Error("用法：node tools/indexnow.mjs <輸出資料夾> <上一版的 commit> [--dry-run]");
  const dryRun = flags.includes("--dry-run");

  const read = async (version, file) => {
    try {
      if (version === "new") return await fs.readFile(path.join(out, file), "utf8");
      if (!previous) return null;
      return execFileSync("git", ["-C", out, "show", `${previous}:${file}`], { encoding: "utf8", stdio: ["ignore", "pipe", "ignore"] });
    } catch {
      return null;
    }
  };

  // 金鑰檔必須在網域根目錄；網址有子路徑（例如 https://cienlin.github.io/JenJenKnits）時不通知
  const sitemapUrl = (await read("new", "robots.txt"))?.match(/^Sitemap: (\S+)$/m)?.[1];
  if (!sitemapUrl) throw new Error("robots.txt 找不到 Sitemap 網址");
  const site = new URL(sitemapUrl);
  if (site.pathname !== "/sitemap.xml") {
    console.log(`IndexNow：網址有子路徑（${site.origin}${site.pathname.replace(/\/sitemap\.xml$/, "")}），金鑰檔不在網域根目錄，不通知。`);
    return;
  }

  const changes = await findChanges(read);
  if (changes.length === 0) {
    console.log("IndexNow：沒有內容變動，不通知。");
    return;
  }
  console.log(`IndexNow：${changes.length} 個網址有變動${previous ? "" : "（沒有上一版，全部通知）"}`);
  for (const { url, kind } of changes) console.log(`  ${labels[kind]}  ${url}`);
  if (dryRun) {
    console.log("（預覽：不送出）");
    return;
  }

  // 新版上線前送出，搜尋引擎會抓到舊內容；第一次部署時金鑰檔也還沒上線
  console.log("IndexNow：等 GitHub Pages 換成新版（最多 5 分鐘）…");
  if (!(await waitUntilLive(changes, (file) => read("new", file)))) {
    console.log("IndexNow：5 分鐘內沒有看到新版，這次不通知。");
    console.log(manualHint);
    return;
  }

  const key = (await fs.readFile(new URL("indexnow-key.txt", import.meta.url), "utf8")).trim();
  const response = await fetch("https://api.indexnow.org/indexnow", {
    method: "POST",
    headers: { "Content-Type": "application/json; charset=utf-8" },
    body: JSON.stringify({ host: site.hostname, key, keyLocation: `${site.origin}/${key}.txt`, urlList: changes.map((c) => c.url) }),
    signal: AbortSignal.timeout(30_000),
  });
  const { accepted, message } = describeResponse(response.status);
  console.log(`IndexNow：HTTP ${response.status}，${message}`);
  if (!accepted) console.log(manualHint);
};

const manualHint = "需要的話，請到 Bing Webmaster Tools 的「URL Submission」手動提交上面的網址。";

// 被測試 import 時不執行；任何錯誤都只印訊息，不讓部署失敗
if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  main().catch((error) => {
    console.log(`IndexNow：發生錯誤，這次不通知（部署不受影響）：${error.message}`);
    console.log(manualHint);
  });
}
