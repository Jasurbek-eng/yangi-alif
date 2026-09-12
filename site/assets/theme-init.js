// Sahifa hali chizilmasdan turib, foydalanuvchi oldin TANLAGAN mavzuni
// (agar bo'lsa) o'rnatadi — shunda "avval yorug', birdan qorong'iga
// sakrash" (FOUC) ko'rinmaydi. Tanlov bo'lmasa, hech narsa qilmaydi —
// CSS'dagi prefers-color-scheme o'zi tizim moyilligini qo'llaydi.
(function () {
  try {
    var saved = localStorage.getItem('yangialif-theme');
    if (saved === 'dark' || saved === 'light') {
      document.documentElement.setAttribute('data-theme', saved);
    }
  } catch (e) { }
})();
