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


Write-Host ""
Write-Host "TAYYOR." -ForegroundColor Green
Write-Host "  Tarqatish uchun:  YangiAlif.exe  (bitta fayl - o'zini o'rnatadi)" -ForegroundColor Yellow
