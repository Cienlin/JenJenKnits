// 後台的互動。都是加分功能：沒有 JavaScript 時表單照樣能填、能存。

// ---------- 編輯頁 ----------

document.querySelectorAll("[data-edit-form]").forEach((form) => {
  setUpStory(form);
  setUpUnsavedWarning(form);
});

function setUpStory(form) {
  const editor = form.querySelector("[data-story-editor]");
  if (!editor) return;
  const textarea = editor.querySelector("[data-story]");
  const output = editor.querySelector("[data-preview-output]");
  const tabs = editor.querySelector("[data-story-tabs]");
  const toolbar = editor.querySelector("[data-story-toolbar]");
  tabs.hidden = false;
  toolbar.hidden = false;
  editor.dataset.view = "edit";

  // 自動預覽：停止輸入 0.5 秒後，把整份表單（含 antiforgery token）送到 ?handler=preview，
  // 伺服器用前台相同的渲染器回傳 HTML。以最後送出的請求為準，舊的回應晚到就丟掉
  let latest = 0;
  let timer;
  const preview = async () => {
    clearTimeout(timer);
    const id = ++latest;
    const url = new URL(window.location.href);
    url.searchParams.set("handler", "preview");
    let html;
    try {
      const response = await fetch(url, { method: "POST", body: new FormData(form) });
      html = response.ok ? await response.text() : '<p class="warn">預覽失敗，請重新整理頁面再試。</p>';
    } catch {
      html = '<p class="warn">預覽失敗，請確認網站還在執行。</p>';
    }
    if (id === latest) output.innerHTML = html;
  };
  textarea.addEventListener("input", () => {
    clearTimeout(timer);
    timer = setTimeout(preview, 500);
  });
  preview();

  // 窄螢幕：「編輯／預覽」切換（寬螢幕兩欄並排，切換按鈕由 CSS 隱藏）
  tabs.querySelectorAll("[data-story-tab]").forEach((button) => {
    button.addEventListener("click", () => {
      editor.dataset.view = button.dataset.storyTab;
      tabs.querySelectorAll("[data-story-tab]").forEach((b) => b.setAttribute("aria-pressed", String(b === button)));
      if (button.dataset.storyTab === "preview") preview();
    });
  });

  toolbar.querySelectorAll("[data-md]").forEach((button) => {
    button.addEventListener("click", () => insertMarkdown(textarea, button.dataset.md));
  });
}

// 格式按鈕：粗體包住選取的文字；小標題、清單加在選取範圍每一行的開頭；連結插入 [文字](https://)
function insertMarkdown(textarea, kind) {
  const { selectionStart: start, selectionEnd: end, value } = textarea;
  const selected = value.slice(start, end);
  textarea.focus();

  if (kind === "bold") {
    const text = selected || "粗體";
    replaceText(textarea, start, end, `**${text}**`);
    textarea.setSelectionRange(start + 2, start + 2 + text.length);
  } else if (kind === "link") {
    const text = selected || "連結文字";
    replaceText(textarea, start, end, `[${text}](https://)`);
    const cursor = start + `[${text}](https://`.length; // 游標停在網址要接著打的位置
    textarea.setSelectionRange(cursor, cursor);
  } else {
    const prefix = kind === "h2" ? "## " : "- ";
    const lineStart = value.lastIndexOf("\n", start - 1) + 1;
    // 選取範圍剛好停在下一行開頭時，不算那一行
    const lastLine = end > start && value[end - 1] === "\n" ? end - 1 : end;
    const lineEnd = value.indexOf("\n", lastLine) === -1 ? value.length : value.indexOf("\n", lastLine);
    const lines = value.slice(lineStart, lineEnd).split("\n").map((line) => prefix + line).join("\n");
    replaceText(textarea, lineStart, lineEnd, lines);
    textarea.setSelectionRange(lineStart + lines.length, lineStart + lines.length);
  }
}

// 用 insertText 插入，瀏覽器的復原（Ctrl+Z）才撤銷得了（它會自己送出 input 事件）；
// 這個 API 已標為過時但各瀏覽器都還支援，不支援時改用 setRangeText（復原紀錄會少這一步）
function replaceText(textarea, start, end, text) {
  textarea.setSelectionRange(start, end);
  if (!document.execCommand("insertText", false, text)) {
    textarea.setRangeText(text, start, end, "end");
    textarea.dispatchEvent(new Event("input", { bubbles: true }));
  }
}

// 未儲存提醒：改過任何欄位就顯示「有未儲存的修改」，沒存就離開時由瀏覽器詢問；儲存、放棄修改不詢問
function setUpUnsavedWarning(form) {
  const notes = document.querySelectorAll("[data-dirty-note]");
  let dirty = false;
  const warn = (event) => {
    event.preventDefault();
    event.returnValue = "";
  };
  const markDirty = () => {
    if (dirty) return;
    dirty = true;
    notes.forEach((note) => (note.hidden = false));
    window.addEventListener("beforeunload", warn);
  };
  const stopWarning = () => window.removeEventListener("beforeunload", warn);

  form.addEventListener("input", markDirty);
  form.addEventListener("change", markDirty);
  form.addEventListener("submit", stopWarning);
  document.querySelectorAll("[data-discard]").forEach((link) => link.addEventListener("click", stopWarning));
}

// ---------- 新增頁：輸入代稱時顯示網址 ----------

document.querySelectorAll("[data-slug-input]").forEach((input) => {
  const target = document.querySelector("[data-slug-preview]");
  const update = () => (target.textContent = input.value.trim() || "代稱");
  input.addEventListener("input", update);
  update();
});
