# Yangi Alif

**Yangi alifbo yordamchisi — Windows uchun**

Siz yozayotgan paytda harflarni avtomatik almashtiradi:

| Yozasiz | Chiqadi |
|---------|---------|
| `o'`    | `ö`     |
| `g'`    | `ğ`     |
| `sh`    | `ş`     |
| `ch`    | `ç`     |

Bosh harflar ham ishlaydi: `O' → Ö`, `Sh → Ş`, `Ch → Ç`

---

## Ishlatish

- **Yoqish / o'chirish:** `Ctrl + Shift` (toza bosib qo'yib yuboring)
- **Xatoni qaytarish:** almashuvdan keyin darhol `Backspace` bosing (`ş → sh`)
- **Sozlamalar:** soat yonidagi belgiga o'ng tugma

---

## Fayllar

| Fayl | Vazifasi |
|------|----------|
| `YangiAlif.cs` | Asosiy kod (C#) |
| `AssemblyInfo.cs` | Mahsulot ma'lumotlari |
| `YangiAlif.manifest` | DPI, Windows 10/11, administrator huquqi |
| `Logo.png` | Logotip (dastur ichiga joylashtiriladi) |
| `Qurish-build.ps1` | **Qurish skripti** — ikonka va `.exe` yasaydi |
| `YangiAlif.ps1` / `.vbs` | Zaxira yo'l: Smart App Control yoqilgan kompyuterlar uchun |
| `site/` | Yuklab olish sayti — pastga qarang |

`YangiAlif.exe` va `YangiAlif.ico` — qurish natijalari, git'da saqlanmaydi.

---

## Qurish

PowerShell'da:

```
powershell -ExecutionPolicy Bypass -File Qurish-build.ps1
```

Natija: `YangiAlif.exe` (o'zini o'rnatadigan bitta fayl). Shu bilan birga skript
`site/downloads/` papkasidagi `.exe`, portable `.zip` va ularning SHA-256
checksumlarini ham avtomatik yangilaydi — sayt har doim eng so'nggi build bilan
sinxron turadi.

---

## Sayt (`site/`)

Yuklab olish sahifasi — `site/index.html`. Toza HTML/CSS/JS, hech qanday server
yoki bog'lanish kerak emas — istalgan static hosting'ga (GitHub Pages, Cloudflare
Pages, Netlify, yoki oddiy cPanel) butun `site/` papkasini yuklash kifoya.

Mahalliy ko'rish uchun:

```
powershell -ExecutionPolicy Bypass -File Qurish-build.ps1
python -m http.server 8123 --directory site
```

so'ng brauzerda `http://localhost:8123` oching.

**Domen tanlangach** almashtiring: `site/robots.txt` va `site/sitemap.xml`
ichidagi `yangialif.uz` — hozircha namunaviy manzil, `site/index.html`
ichidagi `og:url`/`og:image` meta teglari ham shunga mos yangilanishi kerak.

Talab: Windows 10/11 (.NET Framework 4 ichida mavjud, alohida hech narsa kerak emas).

### Buyruq satri parametrlari

| Parametr | Ma'nosi |
|----------|---------|
| `/install` | Jim o'rnatish (savolsiz) |
| `/uninstall` | O'chirish |
| `/silent` | Qo'llanma oynasisiz ishga tushish |
| `/portable` | O'rnatmasdan ishlatish |

---

## Muhim texnik eslatmalar

- `.cs` va `.ps1` fayllar **UTF-8 BOM** bilan saqlanishi shart (ö, ğ, ş, ç harflari uchun).
- `.vbs` fayl **BOM'siz** bo'lishi shart — aks holda VBScript "Invalid character" xatosi beradi.
- Almashtirish `SendInput` hook **ichida** chaqirilmaydi — navbat va taymer orqali yuboriladi.
- Smart App Control imzosiz `.exe` ni tasodifiy bloklaydi. Ishonchli yechim — code signing sertifikati.

---

## Maxfiylik

Dastur klaviaturani faqat harf almashtirish uchun kuzatadi.
Yozilgan matn **saqlanmaydi**, faylga yozilmaydi va internetga yuborilmaydi.
Dastur internetga umuman ulanmaydi.

---

## Aloqa

- Email: jasurbekxasanov214@gmail.com
- Telegram: [@khasanov_jasur](https://t.me/khasanov_jasur)

---

© Xasanov Jasurbek. Mualliflik huquqi himoyalangan.
