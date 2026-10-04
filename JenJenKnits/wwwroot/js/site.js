const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

// 首頁：滑過或用手指劃過 22 色點，標題換成那個線色並顯示色號
document.querySelectorAll("[data-yarn]").forEach((yarn) => {
  const row = yarn.querySelector(".yarn-row");
  const dots = [...row.children];
  const label = yarn.querySelector("[data-yarn-label]");
  const title = document.querySelector("[data-yarn-title]");

  const isLight = (hex) => {
    const [r, g, b] = [1, 3, 5].map((i) => parseInt(hex.slice(i, i + 2), 16) / 255);
    return 0.2126 * r + 0.7152 * g + 0.0722 * b > 0.72;
  };

  const pick = (clientX) => {
    const rect = row.getBoundingClientRect();
    const index = Math.min(dots.length - 1, Math.max(0, Math.floor(((clientX - rect.left) / rect.width) * dots.length)));
    const dot = dots[index];

    dots.forEach((other) => other.classList.toggle("is-active", other === dot));
    title.style.color = dot.dataset.hex;
    title.classList.toggle("is-light", isLight(dot.dataset.hex));
    label.textContent = `${dot.dataset.number} 號`;
    label.style.left = `${dot.offsetLeft + dot.offsetWidth / 2}px`;
    label.hidden = false;
  };

  row.addEventListener("pointerdown", (event) => pick(event.clientX));
  row.addEventListener("pointermove", (event) => {
    if (event.pointerType === "mouse" || event.buttons > 0) {
      pick(event.clientX);
    }
  });
});

// 商品頁：大圖可左右滑動；點縮圖捲到該張；目前那張的縮圖會標示出來
document.querySelectorAll("[data-gallery]").forEach((gallery) => {
  const track = gallery.querySelector("[data-gallery-track]");
  const slides = [...track.children];
  const thumbs = [...gallery.querySelectorAll("[data-gallery-thumb]")];
  const transitionName = slides[0]?.style.viewTransitionName;

  const setActive = (index) => {
    thumbs.forEach((thumb, i) => thumb.setAttribute("aria-current", String(i === index)));
    // 轉場名稱跟著目前那張照片走，回上一頁時是從看到的那張縮回卡片
    if (transitionName) {
      slides.forEach((slide, i) => (slide.style.viewTransitionName = i === index ? transitionName : ""));
    }
  };

  thumbs.forEach((thumb, index) => {
    thumb.addEventListener("click", () => {
      track.scrollTo({ left: slides[index].offsetLeft, behavior: reduceMotion.matches ? "auto" : "smooth" });
      setActive(index);
    });
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

// 配色模擬器：選主色 / 配色，小花即時換色並顯示色號
document.querySelectorAll("[data-simulator]").forEach((simulator) => {
  const sync = () => {
    simulator.querySelectorAll("input[data-part]:checked").forEach((input) => {
      simulator.style.setProperty(`--${input.dataset.part}`, input.dataset.hex);
      simulator.querySelector(`[data-number="${input.dataset.part}"]`).textContent = input.value;
    });
  };

  simulator.addEventListener("change", sync);
  // 按上一頁回來時瀏覽器可能保留上次的選擇，重新對齊一次
  window.addEventListener("pageshow", sync);
});
