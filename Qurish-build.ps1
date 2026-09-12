# ============================================================
#  KHASANOV_JASUR DASTURI — qurish (build) skripti
#
#  Bu skript:
#    1) YangiAlif.ico  — ko'p o'lchamli professional ikonka yaratadi
#    2) YangiAlif.exe  — ikonka va mahsulot ma'lumotlari bilan
#                     kompilyatsiya qiladi (Windows ichidagi csc bilan)
#
#  Ishlatish:  PowerShell'da shu skriptni ishga tushiring.
# ============================================================
$ErrorActionPreference = 'Stop'
$dir = $PSScriptRoot
Add-Type -AssemblyName System.Drawing

# Versiyani manba koddan o'zi o'qiydi — qo'lda ikki joyda yozib yurmaslik uchun
$csText = Get-Content (Join-Path $dir 'YangiAlif.cs') -Raw
$AppVersion = if ($csText -match 'AppVer\s*=\s*"([^"]+)"') { $Matches[1] } else { '0.0' }

# Ishlab turgan nusxa faylni band qilib turmasligi uchun to'xtatamiz
Get-Process YangiAlif -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 700

# ---------- 0) SHAXSIY LOGOTIP BORMI? ----------
# Agar shu papkada "Logo.png" fayli bo'lsa, dastur HAMMA joyda
# (ikonka, tray, oynalar, o'rnatuvchi, ish stoli yorlig'i) o'shani ishlatadi.
$logoFile = Join-Path $dir 'Logo.png'
$hasLogo = Test-Path $logoFile
if ($hasLogo) {
    Write-Host "0) Shaxsiy logotip topildi: Logo.png — hamma joyda ishlatiladi" -ForegroundColor Magenta
    $logoImg = [System.Drawing.Image]::FromFile($logoFile)
} else {
    Write-Host "0) Logo.png topilmadi — ichki (standart) logotip ishlatiladi" -ForegroundColor DarkGray
}

# ---------- 1) IKONKA YARATISH ----------
function New-AppBitmap([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.TextRenderingHint = 'AntiAliasGridFit'

    # Shaxsiy logotip HAMMA o'lchamda ishlatiladi (16px dan 256px gacha).
    if ($script:hasLogo) {
        $g.InterpolationMode = 'HighQualityBicubic'
        $g.DrawImage($script:logoImg, (New-Object System.Drawing.Rectangle(0, 0, $size, $size)))
        $g.Dispose()
        return $bmp
    }

    # Brend ranglari: och ko'k -> to'q ko'k
    $rect = New-Object System.Drawing.Rectangle(0, 0, $size, $size)
    $c1 = [System.Drawing.Color]::FromArgb(122, 170, 80)
    $c2 = [System.Drawing.Color]::FromArgb(44, 82, 32)
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $c1, $c2, 55)

    # Yumaloq kvadrat (dasturdagi logotip bilan bir xil)
    $radius = [Math]::Max(2, [int]($size / 4)); $d2 = $radius * 2
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc(0, 0, $d2, $d2, 180, 90)
    $path.AddArc($size - $d2, 0, $d2, $d2, 270, 90)
    $path.AddArc($size - $d2, $size - $d2, $d2, $d2, 0, 90)
    $path.AddArc(0, $size - $d2, $d2, $d2, 90, 90)
    $path.CloseFigure()
    $g.FillPath($brush, $path)

    # "A" harfi
    $font = New-Object System.Drawing.Font('Segoe UI', ($size * 0.60), [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = 'Center'; $sf.LineAlignment = 'Center'
    $white = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $box = New-Object System.Drawing.RectangleF(0, ($size*0.04), $size, $size)
    $g.DrawString('A', $font, $white, $box, $sf)

    # Urg'u nuqtasi (yangi alifbo belgisi) - kichik o'lchamlarda chizmaymiz
    if ($size -ge 32) {
        $dd = [Math]::Max(2, [int]($size / 9))
        $acc = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(250, 204, 21))
        $g.FillEllipse($acc, ($size - $dd - [int]($size/8)), [int]($size/7), $dd, $dd)
        $acc.Dispose()
    }

    $g.Dispose(); $brush.Dispose(); $font.Dispose(); $white.Dispose(); $path.Dispose()
    return $bmp
}

Write-Host "1) Ikonka yaratilmoqda..." -ForegroundColor Cyan
$sizes = @(16, 32, 48, 64, 128, 256)
$pngs = @()
foreach ($s in $sizes) {
    $bmp = New-AppBitmap $s
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngs += ,@($s, $ms.ToArray())
    $bmp.Dispose(); $ms.Dispose()
}

# ICO fayl formatini qo'lda yozamiz (PNG ichki formatda - Vista+ qo'llab-quvvatlaydi)
$icoPath = Join-Path $dir 'YangiAlif.ico'
$fs = [System.IO.File]::Create($icoPath)
$bw = New-Object System.IO.BinaryWriter($fs)
$bw.Write([UInt16]0); $bw.Write([UInt16]1); $bw.Write([UInt16]$pngs.Count)   # ICONDIR
$offset = 6 + (16 * $pngs.Count)
foreach ($p in $pngs) {
    $sz = $p[0]; $data = $p[1]
    $bw.Write([Byte]$(if ($sz -ge 256) { 0 } else { $sz }))   # width
    $bw.Write([Byte]$(if ($sz -ge 256) { 0 } else { $sz }))   # height
    $bw.Write([Byte]0); $bw.Write([Byte]0)                    # palitra, zaxira
    $bw.Write([UInt16]1); $bw.Write([UInt16]32)               # planes, bit chuqurligi
    $bw.Write([UInt32]$data.Length); $bw.Write([UInt32]$offset)
    $offset += $data.Length
}
foreach ($p in $pngs) { $bw.Write($p[1]) }
$bw.Flush(); $bw.Close(); $fs.Close()
Write-Host "   YangiAlif.ico tayyor ($($pngs.Count) o'lcham)" -ForegroundColor Green

# ---------- 2) EXE KOMPILYATSIYA ----------
Write-Host "2) YangiAlif.exe kompilyatsiya qilinmoqda..." -ForegroundColor Cyan
$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) { $csc = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" }

$exe = Join-Path $dir 'YangiAlif.exe'
$manifest = Join-Path $dir 'YangiAlif.manifest'
$args = @(
    '-nologo', '-optimize+', '-target:winexe',
    "-out:$exe",
    "-win32icon:$icoPath",
    "-win32manifest:$manifest",
    '-r:System.dll', '-r:System.Windows.Forms.dll', '-r:System.Drawing.dll'
)
# Shaxsiy logotipni dastur ichiga joylashtiramiz
if ($hasLogo) { $args += "-resource:$logoFile,Logo" }
$args += (Join-Path $dir 'YangiAlif.cs')
$args += (Join-Path $dir 'AssemblyInfo.cs')
$out = & $csc $args 2>&1 | Out-String
if ($LASTEXITCODE -ne 0) {
    Write-Host "   XATO:" -ForegroundColor Red
    Write-Host $out
    exit 1
}
$info = Get-Item $exe
Write-Host "   YangiAlif.exe tayyor — $([math]::Round($info.Length/1KB,1)) KB" -ForegroundColor Green

# ---------- 3) SAYT UCHUN FAYLLARNI YANGILASH ----------
# site/index.html shu fayllarni ko'rsatadi — har qurishda avtomatik yangilanadi,
# shunda versiya chiqarganda saytni qo'lda yangilashni unutib qo'ymaymiz.
$siteDl = Join-Path $dir 'site\downloads'
if (Test-Path (Join-Path $dir 'site\index.html')) {
    Write-Host "3) Sayt fayllari yangilanmoqda..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $siteDl -Force | Out-Null
    Copy-Item $exe (Join-Path $siteDl 'YangiAlif.exe') -Force

    # --- Sayt rasmlari ---
    # Logo.png asli 372x372 (~113 KB), saytda esa 34px da ko'rsatiladi.
    # Shuncha katta faylni yuklash — ayniqsa mobil internetda — ortiqcha,
    # shuning uchun web uchun kichik nusxa tayyorlaymiz.
    $siteAssets = Join-Path $dir 'site\assets'
    New-Item -ItemType Directory -Path $siteAssets -Force | Out-Null
    if ($hasLogo) {
        $small = New-Object System.Drawing.Bitmap(96, 96)
        $gs = [System.Drawing.Graphics]::FromImage($small)
        $gs.InterpolationMode = 'HighQualityBicubic'
        $gs.DrawImage($logoImg, (New-Object System.Drawing.Rectangle(0, 0, 96, 96)))
        $gs.Dispose()
        $small.Save((Join-Path $siteAssets 'logo.png'), [System.Drawing.Imaging.ImageFormat]::Png)
        $small.Dispose()

        # Ijtimoiy tarmoqlarda ulashilganda ko'rinadigan rasm (Telegram,
        # Facebook va h.k. 1200x630 o'lchamni kutadi — kvadrat logotip
        # u yerda kichkina va bo'sh ko'rinadi).
        $og = New-Object System.Drawing.Bitmap(1200, 630)
        $g2 = [System.Drawing.Graphics]::FromImage($og)
        $g2.SmoothingMode = 'AntiAlias'
        $g2.TextRenderingHint = 'AntiAliasGridFit'
        $ogRect = New-Object System.Drawing.Rectangle(0, 0, 1200, 630)
        $bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
            $ogRect,
            [System.Drawing.Color]::FromArgb(44, 82, 32),
            [System.Drawing.Color]::FromArgb(22, 33, 15), 145)
        $g2.FillRectangle($bg, $ogRect)
        $g2.InterpolationMode = 'HighQualityBicubic'
        $g2.DrawImage($logoImg, (New-Object System.Drawing.Rectangle(90, 96, 150, 150)))

        $fTitle = New-Object System.Drawing.Font('Segoe UI', 62, [System.Drawing.FontStyle]::Bold)
        $fSub   = New-Object System.Drawing.Font('Segoe UI', 30)
        $white  = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $mint   = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(207, 227, 194))
        $lime   = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(154, 205, 106))
        $g2.DrawString('Yangi Alif', $fTitle, $white, 268, 96)
        $g2.DrawString('Yangi lotin alifbosi yordamchisi', $fSub, $mint, 276, 192)

        # Qoidalar qatori: shriftni o'lchab, rasm kengligiga SIG'GUNCHA
        # kichraytiramiz — aks holda matn o'ng chetdan chiqib ketadi
        # (qo'lda tanlangan o'lcham boshqa Windows'da boshqacha chiqishi mumkin).
        $ruleText = "o'  →  ö      g'  →  ğ      sh  →  ş      ch  →  ç"
        $ruleMargin = 92
        $ruleMaxW = 1200 - ($ruleMargin * 2)
        $ruleSize = 46
        do {
            if ($fRule) { $fRule.Dispose() }
            $fRule = New-Object System.Drawing.Font('Segoe UI', $ruleSize, [System.Drawing.FontStyle]::Bold)
            $ruleW = $g2.MeasureString($ruleText, $fRule).Width
            $ruleSize -= 2
        } while ($ruleW -gt $ruleMaxW -and $ruleSize -gt 14)
        $g2.DrawString($ruleText, $fRule, $lime, $ruleMargin, 352)

        $g2.DrawString('Windows uchun  ·  Bepul  ·  Internetga ulanmaydi',
                       $fSub, $mint, 96, 468)
        $og.Save((Join-Path $siteAssets 'og.png'), [System.Drawing.Imaging.ImageFormat]::Png)
        $g2.Dispose(); $og.Dispose()
        $bg.Dispose(); $fTitle.Dispose(); $fSub.Dispose(); $fRule.Dispose()
        $white.Dispose(); $mint.Dispose(); $lime.Dispose()
        Write-Host "   Sayt rasmlari: logo.png (96px) + og.png (1200x630)" -ForegroundColor Green
    }

    $stage = Join-Path ([System.IO.Path]::GetTempPath()) 'YangiAlif-portable-build'
    New-Item -ItemType Directory -Path $stage -Force | Out-Null
    Copy-Item (Join-Path $dir 'YangiAlif.cs')  $stage -Force
    Copy-Item (Join-Path $dir 'YangiAlif.ps1') $stage -Force
    Copy-Item (Join-Path $dir 'YangiAlif.vbs') $stage -Force
    Set-Content -Path (Join-Path $stage 'OQI-BOSHLA.txt') -Encoding UTF8 -Value @"
YANGI ALIF -- portable (skript) versiya
========================================

BU NIMA?
Smart App Control (SAC) yoqilgan kompyuterlar uchun. Bu usulda
imzosiz .exe fayl UMUMAN yaratilmaydi -- dastur to'g'ridan-to'g'ri
imzolangan powershell.exe ichida, xotirada ishga tushadi.

QANDAY ISHLATISH?
1. Bu 3 ta faylni (YangiAlif.cs, YangiAlif.ps1, YangiAlif.vbs)
   BIR papkaga joylashtiring.
2. YangiAlif.vbs faylini ikki marta bosing.

Savol-taklif: jasurbekxasanov214@gmail.com | Telegram: @khasanov_jasur
"@
    $zipPath = Join-Path $siteDl 'YangiAlif-portable.zip'
    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zipPath -Force

    # Yaxlitlikni tekshirish uchun SHA-256 (sayt shu faylni o'qib ko'rsatadi)
    $exeHash = (Get-FileHash (Join-Path $siteDl 'YangiAlif.exe') -Algorithm SHA256).Hash.ToLower()
    $zipHash = (Get-FileHash $zipPath -Algorithm SHA256).Hash.ToLower()
    $checksums = @"
YangiAlif.exe (v$AppVersion)
  SHA-256: $exeHash

YangiAlif-portable.zip (v$AppVersion)
  SHA-256: $zipHash

Tekshirish (PowerShell'da):
  Get-FileHash YangiAlif.exe -Algorithm SHA256
Yoki (cmd'da):
  certutil -hashfile YangiAlif.exe SHA256
"@
    Set-Content -Path (Join-Path $siteDl 'checksums.txt') -Value $checksums -Encoding UTF8
    $checksumJson = "{`"version`":`"$AppVersion`",`"exe`":{`"sha256`":`"$exeHash`",`"bytes`":$((Get-Item (Join-Path $siteDl 'YangiAlif.exe')).Length)},`"zip`":{`"sha256`":`"$zipHash`",`"bytes`":$((Get-Item $zipPath).Length)}}"
    Set-Content -Path (Join-Path $siteDl 'checksums.json') -Value $checksumJson -Encoding UTF8 -NoNewline

    # Har bir JS faylni o'z MAZMUNIGA bog'liq ?v= belgisi bilan chaqiramiz —
    # AppVersion'ga EMAS. Sabab: sayt fayllari (main.js, theme-init.js)
    # ilova versiyasidan MUSTAQIL ravishda ham yangilanishi mumkin — agar
    # ?v= faqat AppVersion'ga bog'liq bo'lsa, ilova versiyasi o'zgarmagan
    # holda saytga kiritilgan tuzatish avvalgi tashrif buyurgan
    # foydalanuvchi brauzerining ESKI KESHIDA qolib ketadi.
    #
    # MUHIM: Get-Content/Set-Content -Encoding UTF8 ISHLATILMAYDI — Windows
    # PowerShell 5.1'da Get-Content'ning standart o'qish kodировkasi UTF-8
    # emas, shuning uchun o', g', sh, ch belgilari (ö ğ ş ç) va emoji'lar
    # BUZILADI ("mojibake"). .NET File'ning o'zini, aniq UTF-8 (BOM'siz)
    # bilan ishlatamiz — bu ishonchli va hamma joyda bir xil ishlaydi.
    $indexPath = Join-Path $dir 'site\index.html'
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $indexHtml = [System.IO.File]::ReadAllText($indexPath, [System.Text.Encoding]::UTF8)

    function Get-ShortHash([string]$path) {
        $bytes = [System.IO.File]::ReadAllBytes($path)
        $sha = [System.Security.Cryptography.SHA256]::Create()
        try { ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join '' | ForEach-Object { $_.Substring(0, 10) } }
        finally { $sha.Dispose() }
    }
    foreach ($jsName in @('main.js', 'theme-init.js')) {
        $jsPath = Join-Path $dir "site\assets\$jsName"
        if (Test-Path $jsPath) {
            $jsHash = Get-ShortHash $jsPath
            $escaped = [regex]::Escape($jsName)
            # MUHIM: ${jsName}?v= — figurali qavs SHART. "$jsName?v="
            # yozilsa, PowerShell "?" belgisini o'zgaruvchi nomining
            # DAVOMI deb tushunib, butun ifodani bo'sh satrga aylantiradi
            # (haqiqiy, sinovdan o'tkazilgan PowerShell xatti-harakati).
            $indexHtml = $indexHtml -replace "assets/${escaped}(\?v=[^`"]*)?`"", "assets/${jsName}?v=${jsHash}`""
        }
    }
    [System.IO.File]::WriteAllText($indexPath, $indexHtml, $utf8NoBom)

    Write-Host "   Sayt fayllari tayyor (site/downloads/) — SHA-256 hisoblandi" -ForegroundColor Green
}

Write-Host ""
Write-Host "TAYYOR." -ForegroundColor Green
Write-Host "  Tarqatish uchun:  YangiAlif.exe  (bitta fayl - o'zini o'rnatadi)" -ForegroundColor Yellow
