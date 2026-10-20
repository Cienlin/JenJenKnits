// 執行：node --test tools/indexnow.test.mjs
import assert from "node:assert/strict";
import test from "node:test";
import { describeResponse, findChanges, normalize, urlToFile, waitUntilLive } from "./indexnow.mjs";

const page = (body, { commit = "aaaaaaa", css = "djzafdnge5", hero = "xmy28pkx7d" } = {}) => `<html><head>
<link rel="stylesheet" href="/css/site.${css}.css" /><script src="/js/site.shkjs99pt4.js"></script></head>
<body><img src="/images/hero/puff-flower-lake.${hero}.jpg" alt="" /><main>${body}</main>
<p b-9eztl2mxxs class="mono version">v1.0.0 (${commit})</p></body></html>`;

const sitemap = (...paths) =>
  `<urlset>${paths.map((p) => `<url><loc>https://example.com${p}</loc></url>`).join("")}</urlset>`;

// 用物件模擬新舊兩版的檔案；old 為 null 代表沒有上一版
const reader = (versions) => async (version, file) => versions[version]?.[file] ?? null;

test("網址對應到輸出的檔案", () => {
  assert.equal(urlToFile("https://example.com/"), "index.html");
  assert.equal(urlToFile("https://example.com/products/puff-flower-net/"), "products/puff-flower-net/index.html");
});

test("只差頁尾版本或 CSS、JS 指紋不算更新", () => {
  assert.equal(normalize(page("同樣的內容")), normalize(page("同樣的內容", { commit: "bbbbbbb", css: "mgunjwkmh7" })));
});

test("圖片指紋不同算更新（圖片換了）", () => {
  assert.notEqual(normalize(page("同樣的內容")), normalize(page("同樣的內容", { hero: "zzzzzzzzzz" })));
});

test("內容不同算更新，沒變的不列", async () => {
  const changes = await findChanges(reader({
    old: { "sitemap.xml": sitemap("/", "/products/"), "index.html": page("舊首頁"), "products/index.html": page("作品") },
    new: { "sitemap.xml": sitemap("/", "/products/"), "index.html": page("新首頁", { commit: "bbbbbbb" }), "products/index.html": page("作品", { commit: "bbbbbbb" }) },
  }));
  assert.deepEqual(changes, [{ url: "https://example.com/", kind: "updated" }]);
});

test("新增與刪除依 sitemap 判斷", async () => {
  const changes = await findChanges(reader({
    old: { "sitemap.xml": sitemap("/", "/products/net-tote/"), "index.html": page("首頁") },
    new: { "sitemap.xml": sitemap("/", "/products/floral-coaster/"), "index.html": page("首頁"), "products/floral-coaster/index.html": page("杯墊") },
  }));
  assert.deepEqual(changes, [
    { url: "https://example.com/products/floral-coaster/", kind: "added" },
    { url: "https://example.com/products/net-tote/", kind: "deleted" },
  ]);
});

test("不在 sitemap 的頁面（分類篩選）不列", async () => {
  const changes = await findChanges(reader({
    old: { "sitemap.xml": sitemap("/"), "index.html": page("首頁"), "products/category/杯墊/index.html": page("舊") },
    new: { "sitemap.xml": sitemap("/"), "index.html": page("首頁"), "products/category/杯墊/index.html": page("新") },
  }));
  assert.deepEqual(changes, []);
});

test("沒有上一版時全部列為新增", async () => {
  const changes = await findChanges(reader({
    new: { "sitemap.xml": sitemap("/", "/products/"), "index.html": page("首頁"), "products/index.html": page("作品") },
  }));
  assert.deepEqual(changes.map((c) => c.kind), ["added", "added"]);
});

test("200、202 算成功，其他回應碼要手動補送", () => {
  assert.equal(describeResponse(200).accepted, true);
  assert.equal(describeResponse(202).accepted, true);
  assert.match(describeResponse(202).message, /驗證中/);
  for (const status of [400, 403, 422, 429, 500]) assert.equal(describeResponse(status).accepted, false);
  assert.match(describeResponse(403).message, /金鑰/);
  assert.equal(describeResponse(500).message, "非預期的回應");
});

// 模擬線上網站：依序回傳每一輪抓到的內容
const fakeSite = (...responses) => {
  const requested = [];
  const fetchPage = async (url) => {
    requested.push(url);
    const { status = 200, body = "" } = responses[Math.min(requested.length, responses.length) - 1];
    return { status, ok: status === 200, text: async () => body };
  };
  return { fetchPage, requested };
};

test("線上換成新版（只差頁尾版本也算）才回傳 true，並避開快取", async () => {
  const site = fakeSite({ body: page("舊首頁") }, { body: page("新首頁", { commit: "ccccccc" }) });
  const live = await waitUntilLive(
    [{ url: "https://example.com/", kind: "updated" }],
    async () => page("新首頁"),
    { fetchPage: site.fetchPage, interval: 0, timeout: 1_000 });
  assert.equal(live, true);
  assert.equal(site.requested.length, 2);
  assert.match(site.requested[0], /^https:\/\/example\.com\/\?indexnow=\d+$/);
});

test("只有刪除時等到 404", async () => {
  const site = fakeSite({ status: 200, body: page("托特包") }, { status: 404 });
  const live = await waitUntilLive([{ url: "https://example.com/products/net-tote/", kind: "deleted" }], async () => null,
    { fetchPage: site.fetchPage, interval: 0, timeout: 1_000 });
  assert.equal(live, true);
});

test("超過時間還是舊版就回傳 false", async () => {
  const site = fakeSite({ body: page("舊首頁") });
  const live = await waitUntilLive([{ url: "https://example.com/", kind: "updated" }], async () => page("新首頁"),
    { fetchPage: site.fetchPage, interval: 5, timeout: 30 });
  assert.equal(live, false);
});
