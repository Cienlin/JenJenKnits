// 後台：故事 Markdown 預覽。把整份表單（含 antiforgery token）送到 ?handler=preview，伺服器用前台相同的渲染器回傳 HTML
document.querySelectorAll("[data-edit-form]").forEach((form) => {
  const button = form.querySelector("[data-preview]");
  const output = form.querySelector("[data-preview-output]");

  button?.addEventListener("click", async () => {
    button.disabled = true;
    try {
      const url = new URL(window.location.href);
      url.searchParams.set("handler", "preview");
      const response = await fetch(url, { method: "POST", body: new FormData(form) });
      output.innerHTML = response.ok ? await response.text() : '<p class="warn">預覽失敗，請重新整理頁面再試。</p>';
    } catch {
      output.innerHTML = '<p class="warn">預覽失敗，請確認網站還在執行。</p>';
    } finally {
      button.disabled = false;
    }
  });
});
