const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
const scrollBehavior = () => (reduceMotion.matches ? "auto" : "smooth");

// 商品頁：大圖可左右滑動；點縮圖、上一張／下一張或鍵盤左右鍵換張；顯示目前第幾張
document.querySelectorAll("[data-gallery]").forEach((gallery) => {
  const track = gallery.querySelector("[data-gallery-track]");
  const slides = [...track.children];
  const thumbs = [...gallery.querySelectorAll("[data-gallery-thumb]")];
  const thumbList = gallery.querySelector(".gallery-thumbs");
  const count = gallery.querySelector("[data-gallery-count]");
  const prev = gallery.querySelector("[data-gallery-prev]");
  const next = gallery.querySelector("[data-gallery-next]");
  const transitionName = slides[0]?.style.viewTransitionName;
  const pad = (n) => String(n).padStart(2, "0");
  let current = 0;

  const setActive = (index) => {
    current = index;
    thumbs.forEach((thumb, i) => thumb.setAttribute("aria-current", String(i === index)));
    count.textContent = `Fig. ${pad(index + 1)} / ${pad(slides.length)}`;
    if (prev) prev.disabled = index === 0;
    if (next) next.disabled = index === slides.length - 1;
    // 轉場名稱跟著目前那張照片走，回上一頁時是從看到的那張縮回卡片
    if (transitionName) {
      slides.forEach((slide, i) => (slide.style.viewTransitionName = i === index ? transitionName : ""));
    }
    // 手機上縮圖列可以橫向捲動，讓目前那張保持在看得到的位置
    const thumb = thumbs[index];
    if (thumb && thumbList.scrollWidth > thumbList.clientWidth) {
      const left = thumb.offsetLeft - (thumbList.clientWidth - thumb.offsetWidth) / 2;
      thumbList.scrollTo({ left, behavior: scrollBehavior() });
    }
  };

  const go = (index) => {
    const target = Math.max(0, Math.min(slides.length - 1, index));
    track.scrollTo({ left: slides[target].offsetLeft, behavior: scrollBehavior() });
    setActive(target);
  };

  thumbs.forEach((thumb, index) => thumb.addEventListener("click", () => go(index)));
  prev?.addEventListener("click", () => go(current - 1));
  next?.addEventListener("click", () => go(current + 1));
  track.addEventListener("keydown", (event) => {
    if (event.key === "ArrowRight" || event.key === "ArrowLeft") {
      event.preventDefault();
      go(current + (event.key === "ArrowRight" ? 1 : -1));
    }
  });

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          setActive(slides.indexOf(entry.target));
        }
      });
    },
    { root: track, threshold: 0.6 }
  );
  slides.forEach((slide) => observer.observe(slide));
});

// 配色模擬器：先點「主色」或「配色」選要換的部位，再點色票；小花和色號即時更新
document.querySelectorAll("[data-simulator]").forEach((simulator) => {
  const parts = [...simulator.querySelectorAll("[data-part]")];
  const chips = [...simulator.querySelectorAll("[data-yarn]")];
  const hexOf = (number) => chips.find((chip) => chip.dataset.yarn === String(number)).dataset.hex;
  const picked = Object.fromEntries(
    parts.map((part) => [part.dataset.part, Number(part.querySelector("[data-number]").textContent)])
  );
  let active = "main";

  const sync = () => {
    parts.forEach((part) => {
      const name = part.dataset.part;
      simulator.style.setProperty(`--${name}`, hexOf(picked[name]));
      part.setAttribute("aria-pressed", String(name === active));
      part.querySelector("[data-swatch]").style.setProperty("--c", hexOf(picked[name]));
      part.querySelector("[data-number]").textContent = String(picked[name]).padStart(2, "0");
    });
    chips.forEach((chip) => {
      const number = Number(chip.dataset.yarn);
      chip.setAttribute("aria-pressed", String(number === picked[active]));
      chip.dataset.role = (number === picked.main ? "主" : "") + (number === picked.accent ? "配" : "");
    });
  };

  parts.forEach((part) =>
    part.addEventListener("click", () => {
      active = part.dataset.part;
      sync();
    })
  );
  chips.forEach((chip) =>
    chip.addEventListener("click", () => {
      picked[active] = Number(chip.dataset.yarn);
      sync();
    })
  );
});
