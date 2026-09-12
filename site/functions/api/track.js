// Cloudflare Pages Function — POST /api/track?file=exe|zip
//
// Har bir yuklab olish tugmasi bosilganda chaqiriladi va anonim,
// YIG'INDI hisoblagichni bittaga oshiradi. Bu — SAYTNING o'zi uchun
// (dasturning EMAS): dastur hech qachon internetga ulanmaydi, lekin
// oddiy vebsayt tabiatan tarmoqli bo'ladi va bu yerda faqat "nechta
// marta bosildi" degan yig'indi sonini bilamiz — kim, qayerdan,
// nima yozgani haqida HECH QANDAY ma'lumot saqlanmaydi.
//
// Xom IP manzil saqlanmaydi — faqat bitta kunlik takroriy bosishni
// (masalan tasodifiy ikki marta bosish) chetlab o'tish uchun uning
// bir martalik SHA-256 xeshi 26 soatga saqlanadi, keyin o'zi o'chadi.

export async function onRequestPost(context) {
  const { request, env } = context;
  const url = new URL(request.url);
  const file = url.searchParams.get('file');

  if (file !== 'exe' && file !== 'zip') {
    return json({ ok: false, error: 'invalid file' }, 400);
  }

  // Boshqa saytlar shu manzilga to'g'ridan-to'g'ri so'rov yuborib,
  // hisoblagichni sun'iy oshirishi mumkin. Buni to'liq to'xtatib
  // bo'lmaydi (bu — himoyalanmagan ochiq API), lekin brauzer yuborgan
  // Origin sarlavhasi BOSHQA manzilni ko'rsatsa, rad etamiz — bu oddiy
  // tashqi suiiste'molning katta qismini kesib tashlaydi.
  const origin = request.headers.get('Origin');
  if (origin && origin !== new URL(request.url).origin) {
    return json({ ok: false, error: 'bad_origin' }, 403);
  }

  if (!env.DOWNLOADS) {
    // KV bog'lanmagan — Cloudflare Pages sozlamalarida "DOWNLOADS" nomli
    // KV Namespace ulanishi kerak (README'ga qarang). Saytning o'zi
    // buzilmasin deb xato qaytarmaymiz, shunchaki hisoblamaymiz.
    return json({ ok: true, counted: false, reason: 'kv_not_bound' });
  }

  const today = new Date().toISOString().slice(0, 10); // YYYY-MM-DD

  const ip = request.headers.get('CF-Connecting-IP') || '';
  const dedupeSeed = ip + ':' + today + ':' + file;
  const dedupeHash = await sha256(dedupeSeed);
  const dedupeKey = 'seen:' + today + ':' + file + ':' + dedupeHash;

  const already = await env.DOWNLOADS.get(dedupeKey);
  if (already) {
    return json({ ok: true, counted: false, reason: 'duplicate' });
  }
  // 26 soat — bir kunlik oynadan sal ortiq, vaqt mintaqasi chetiga yetish uchun.
  await env.DOWNLOADS.put(dedupeKey, '1', { expirationTtl: 60 * 60 * 26 });

  await incr(env.DOWNLOADS, 'total:' + file);
  await incr(env.DOWNLOADS, 'day:' + today + ':' + file);

  return json({ ok: true, counted: true });
}

// Cloudflare KV'da atom (compare-and-swap) operatsiyasi yo'q — juda
// katta bir vaqtdagi trafikda bitta-ikkita hisob yo'qolishi mumkin.
// Kichik loyiha uchun bu qabul qilinadi: aniq emas, taxminiy hisoblagich.
async function incr(kv, key) {
  const cur = parseInt((await kv.get(key)) || '0', 10);
  await kv.put(key, String(cur + 1));
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
