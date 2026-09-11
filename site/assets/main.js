(function () {
  // ============================================================
  //  Haqiqiy dasturdagi almashtirish qoidalari — shu yerda ham
  //  qo'llaniladi (jonli namuna VA "sinab ko'ring" maydoni uchun),
  //  shunda saytdagi ko'rsatkichlar ilova qanday ishlashini aynan
  //  aks ettiradi, taxminiy emas.
  // ============================================================
  var RULES = [
    ["o'", "ö"], ["O'", "Ö"], ["g'", "ğ"], ["G'", "Ğ"],
    ["sh", "ş"], ["Sh", "Ş"], ["SH", "Ş"],
    ["ch", "ç"], ["Ch", "Ç"], ["CH", "Ç"]
  ];

  // Berilgan matnni to'liq skanerlab, har bir "oxirigacha to'g'ri
  // keladigan" qoidani darhol qo'llaydi — chapdan o'ngga, xuddi
  // odam yozayotgandagidek.
  function transformText(src) {
    var buf = '';
    for (var i = 0; i < src.length; i++) {
      buf += src[i];
      for (var r = 0; r < RULES.length; r++) {
        var from = RULES[r][0];
        if (buf.length >= from.length && buf.slice(-from.length) === from) {
          buf = buf.slice(0, -from.length) + RULES[r][1];
          break;
        }
      }
    }
    return buf;
  }

  // transformText'ning har bir oraliq holatini ham qaytaradigan varianti —
  // animatsiya uchun (har harf terilganda ekranda qanday ko'rinishini
  // bosqichma-bosqich saqlaydi).
  function buildFrames(src) {
    var frames = [], buf = '';
    for (var i = 0; i < src.length; i++) {
      buf += src[i];
      for (var r = 0; r < RULES.length; r++) {
        var from = RULES[r][0];
        if (buf.length >= from.length && buf.slice(-from.length) === from) {
          buf = buf.slice(0, -from.length) + RULES[r][1];
          break;
        }
      }
      frames.push(buf);
    }
    return frames;
  }

  // ---------- Yuklab olish tugmalari ostidagi SHA-256 va hajm ----------
  // downloads/checksums.json'dan o'qiydi (Qurish-build.ps1 har build'da
  // yangilaydi). Fayl topilmasa (hali qurilmagan bo'lsa) jimgina o'tkazib
  // yuboradi.
  function fmtSize(bytes) {
    if (!bytes) return null;
    return (bytes / 1024).toFixed(1).replace(/\.0$/, '') + ' KB';
  }
  // MUHIM: cache:'no-store' — bu fayl HAR build'da o'zgaradi (versiya,
  // checksum), shuning uchun brauzer keshidan emas, doim yangisini olamiz.
  fetch('downloads/checksums.json', { cache: 'no-store' }).then(function (r) { return r.ok ? r.json() : null; })
    .then(function (data) {
      if (!data) return;
      ['exe', 'zip'].forEach(function (key) {
        var info = data[key];
        if (!info) return;
        var hashEl = document.querySelector('[data-hash-for="' + key + '"]');
        if (hashEl) hashEl.textContent = info.sha256;
        // querySelectorAll: bir xil data-size-for="exe" bir nechta joyda
        // (hero'da va yuklab olish kartasida) ishlatilishi mumkin.
        var sizeText = fmtSize(info.bytes);
        if (sizeText) {
          document.querySelectorAll('[data-size-for="' + key + '"]').forEach(function (el) {
            el.textContent = '~' + sizeText;
          });
        }
      });
      var verEl = document.getElementById('appVersion');
      if (verEl && data.version) verEl.textContent = 'v' + data.version;
    })
    .catch(function () { /* checksums.json hali yo'q — sukut bo'yicha matn qoladi */ });

  document.querySelectorAll('.copy-btn').forEach(function (btn) {
    btn.addEventListener('click', function () {
      var key = btn.getAttribute('data-copy-target');
      var srcEl = key
        ? document.querySelector('[data-hash-for="' + key + '"]')
        : document.getElementById(btn.getAttribute('data-copy-el'));
      if (!srcEl) return;
      navigator.clipboard.writeText(srcEl.textContent.trim()).then(function () {
        btn.classList.add('copied'); btn.textContent = '✓';
        setTimeout(function () { btn.classList.remove('copied'); btn.textContent = '⧉'; }, 1500);
      }).catch(function () { });
    });
  });

  // ---------- Mobil menyu ----------
  var menuBtn = document.getElementById('menuBtn');
  var mobileLinks = document.getElementById('mobileLinks');
  if (menuBtn && mobileLinks) {
    menuBtn.addEventListener('click', function () {
      var open = mobileLinks.classList.toggle('open');
      menuBtn.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
    mobileLinks.querySelectorAll('a').forEach(function (a) {
      a.addEventListener('click', function () {
        mobileLinks.classList.remove('open');
        menuBtn.setAttribute('aria-expanded', 'false');
      });
    });
  }

  // ---------- "Yuqoriga" tugmasi ----------
  var topBtn = document.getElementById('topBtn');
  if (topBtn) {
    window.addEventListener('scroll', function () {
      topBtn.classList.toggle('show', window.scrollY > 500);
    }, { passive: true });
  }

  // ---------- Bo'limlar sekin paydo bo'lishi ----------
  if ('IntersectionObserver' in window) {
    var toReveal = document.querySelectorAll('main > section');
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (e) {
        if (e.isIntersecting) { e.target.classList.add('in'); io.unobserve(e.target); }
      });
    }, { threshold: 0.12 });
    toReveal.forEach(function (el) { el.classList.add('reveal'); io.observe(el); });
  }

  // ---------- Hero'dagi jonli yozish namunasi (avtomatik, tegilmaydi) ----------
  var demoEl = document.getElementById('demoText');
  if (demoEl) {
    var PHRASE = "O'zbekiston go'zal, shahri chiroyli";
    var frames = buildFrames(PHRASE);
    var i = 0, dir = 1, pause = 0;
    function tick() {
      if (pause > 0) { pause--; }
      else {
        i += dir;
        if (i >= frames.length) { i = frames.length; dir = -1; pause = 22; }
        else if (i <= 0) { i = 0; dir = 1; pause = 6; }
      }
      demoEl.textContent = frames[Math.max(0, Math.min(i, frames.length - 1))] || '';
      setTimeout(tick, dir > 0 ? 70 : 18);
    }
    tick();
  }

  // ---------- "O'zingiz sinab ko'ring" — jonli, interaktiv maydon ----------
  // Foydalanuvchi yozgan (yoki joylagan) matn shu yerning o'zida,
  // hech qayerga yuborilmasdan, dasturdagi bilan bir xil qoida bo'yicha
  // "Yangi Alif"ga aylantiriladi.
  var pgInput = document.getElementById('pgInput');
  var pgOutput = document.getElementById('pgOutput');
  if (pgInput && pgOutput) {
    var PLACEHOLDER_RESULT = 'Natija shu yerda darhol chiqadi…';
    function renderPlayground() {
      var val = pgInput.value;
      pgOutput.textContent = val.trim() ? transformText(val) : '';
      pgOutput.classList.toggle('empty', !val.trim());
      if (!val.trim()) pgOutput.textContent = PLACEHOLDER_RESULT;
    }
    pgInput.value = "Bugun ob-havo juda yaxshi, o'qishga bordim.\nSharqona chorbog'da do'stlarim bilan choy ichdik.";
    renderPlayground();
    pgInput.addEventListener('input', renderPlayground);

    var pgCopy = document.getElementById('pgCopy');
    if (pgCopy) {
      pgCopy.addEventListener('click', function () {
        var text = pgOutput.textContent === PLACEHOLDER_RESULT ? '' : pgOutput.textContent;
        if (!text) return;
        navigator.clipboard.writeText(text).then(function () {
          pgCopy.classList.add('copied'); pgCopy.textContent = '✓';
          setTimeout(function () { pgCopy.classList.remove('copied'); pgCopy.textContent = '⧉'; }, 1500);
        }).catch(function () { });
      });
    }
    var pgClear = document.getElementById('pgClear');
    if (pgClear) {
      pgClear.addEventListener('click', function () {
        pgInput.value = '';
        renderPlayground();
        pgInput.focus();
      });
    }
  }
})();
