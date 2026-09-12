// Cloudflare Pages Function — GET /api/stats
//
// Faqat parol bilan ochiladi. Parol hech qachon kodga yozilmaydi —
// Cloudflare Pages loyihasi sozlamalarida "ADMIN_PASSWORD" nomli
// muhit o'zgaruvchisi (Secret) sifatida siz o'rnatasiz (README'ga
// qarang). Tekshiruv har doim SERVER tomonida bo'ladi, brauzerda emas.
//
// XAVFSIZLIK:
//  - Parolni "behad urinib ko'rish" (brute-force) hujumidan himoya —
//    bitta IP manzil 15 daqiqada 8 martadan ortiq noto'g'ri parol
//    kiritsa, vaqtincha bloklanadi.
//  - Parol taqqoslash VAQT-HUJUMIGA (timing attack) chidamli usulda —
//    oddiy "===" belgisi emas, doim bir xil vaqt sarflaydigan
//    solishtirish orqali amalga oshiriladi.

const RATE_LIMIT_WINDOW_SEC = 15 * 60;
const RATE_LIMIT_MAX = 8;

export async function onRequestGet(context) {
  const { request, env } = context;

  if (!env.ADMIN_PASSWORD) {
    return json({ ok: false, error: 'ADMIN_PASSWORD sozlanmagan' }, 500);
  }
  if (!env.DOWNLOADS) {
    return json({ ok: false, error: 'kv_not_bound' }, 500);
  }

  const ip = request.headers.get('CF-Connecting-IP') || 'unknown';
  const rlKey = 'ratelimit:stats:' + (await sha256(ip));

  const attempts = parseInt((await env.DOWNLOADS.get(rlKey)) || '0', 10);
  if (attempts >= RATE_LIMIT_MAX) {
    return json({ ok: false, error: 'too_many_attempts' }, 429);
  }

  const auth = request.headers.get('Authorization') || '';
  const given = auth.replace(/^Bearer\s+/i, '');
  const valid = !!given && constantTimeEqual(given, env.ADMIN_PASSWORD);

  if (!valid) {
    // Noto'g'ri urinishni hisoblaymiz — muvaffaqiyatli kirishlarni EMAS,
    // shunda haqiqiy admin oddiy foydalanishda hech qachon bloklanmaydi.
    await env.DOWNLOADS.put(rlKey, String(attempts + 1), { expirationTtl: RATE_LIMIT_WINDOW_SEC });
    return json({ ok: false, error: 'unauthorized' }, 401);
  }
  // Muvaffaqiyatli kirishdan keyin hisoblagichni tozalaymiz.
  await env.DOWNLOADS.delete(rlKey);

  const totalExe = parseInt((await env.DOWNLOADS.get('total:exe')) || '0', 10);
  const totalZip = parseInt((await env.DOWNLOADS.get('total:zip')) || '0', 10);

  // Oxirgi 14 kunlik kundalik statistika (admin panelidagi ustunli grafik uchun)
  const days = [];
  for (let i = 13; i >= 0; i--) {
    const d = new Date(Date.now() - i * 86400000).toISOString().slice(0, 10);
    const exe = parseInt((await env.DOWNLOADS.get('day:' + d + ':exe')) || '0', 10);
    const zip = parseInt((await env.DOWNLOADS.get('day:' + d + ':zip')) || '0', 10);
    days.push({ date: d, exe: exe, zip: zip, total: exe + zip });
  }

  return json({
    ok: true,
    total: totalExe + totalZip,
    totalExe: totalExe,
    totalZip: totalZip,
    days: days
  });
}

// a va b uzunligi har xil bo'lsa ham, HAR DOIM to'liq tsiklni aylanadi —
// shunda javob vaqtidan parolning to'g'ri qismi haqida taxmin qilib
// bo'lmaydi ("timing attack"ning oldi olinadi).
function constantTimeEqual(a, b) {
  const len = Math.max(a.length, b.length, 1);
  let diff = a.length === b.length ? 0 : 1;
  for (let i = 0; i < len; i++) {
    const ca = i < a.length ? a.charCodeAt(i) : 0;
    const cb = i < b.length ? b.charCodeAt(i) : 0;
    diff |= ca ^ cb;
  }
  return diff === 0;
}

async function sha256(text) {
  const data = new TextEncoder().encode(text);
  const digest = await crypto.subtle.digest('SHA-256', data);
  return Array.from(new Uint8Array(digest)).map(function (b) {
    return b.toString(16).padStart(2, '0');
  }).join('');
}

function json(obj, status) {
  return new Response(JSON.stringify(obj), {
    status: status || 200,
    headers: { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' }
  });
}
