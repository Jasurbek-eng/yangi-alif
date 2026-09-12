# ============================================================
#  Dastur oynalarining suratini oladi (sayt va README uchun).
#
#  Oynalar EKRANGA CHIQARILMAYDI — to'g'ridan-to'g'ri rasmga
#  chiziladi (DrawToBitmap), shuning uchun skript fon rejimida
#  ham ishlaydi va sichqoncha/klaviaturaga tegmaydi.
#
#  Ishlatish:  powershell -STA -ExecutionPolicy Bypass -File Ekran-rasm.ps1
#  Natija:     site/assets/oyna-qollanma.png, oyna-sozlamalar.png
# ============================================================
$ErrorActionPreference = 'Stop'
$dir = $PSScriptRoot

# MUHIM: -STA shart. Busiz WinForms oynalari to'g'ri chizilmaydi.
if ([System.Threading.Thread]::CurrentThread.GetApartmentState() -ne 'STA') {
    Write-Host "XATO: skriptni -STA bilan ishga tushiring:" -ForegroundColor Red
    Write-Host "  powershell -STA -ExecutionPolicy Bypass -File Ekran-rasm.ps1" -ForegroundColor Yellow
    exit 1
}

Add-Type -Path (Join-Path $dir 'YangiAlif.cs') `
         -ReferencedAssemblies System.Windows.Forms, System.Drawing, mscorlib
[System.Windows.Forms.Application]::EnableVisualStyles()

$t = [Program]
$flags = [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static
$assets = Join-Path $dir 'site\assets'
New-Item -ItemType Directory -Path $assets -Force | Out-Null

[Program]::WorkDir = $dir
$t.GetMethod('LoadLogo', $flags).Invoke($null, $null)

function Save-Window($method, $field, $file) {
    $t.GetMethod($method, $flags).Invoke($null, $null)
    $form = $t.GetField($field, $flags).GetValue($null)
    if (-not $form) { Write-Host "   $file : oyna yaratilmadi" -ForegroundColor Red; return }
    $form.Refresh(); Start-Sleep -Milliseconds 400; $form.Refresh()
    $bmp = New-Object System.Drawing.Bitmap($form.Width, $form.Height)
    $form.DrawToBitmap($bmp, (New-Object System.Drawing.Rectangle(0, 0, $form.Width, $form.Height)))
    $path = Join-Path $assets $file
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $w = $bmp.Width; $h = $bmp.Height      # o'lchamni Dispose'dan OLDIN olamiz
    $bmp.Dispose(); $form.Close()
    $kb = [math]::Round((Get-Item $path).Length / 1KB, 1)
    Write-Host "   $file  (${w}x${h}, $kb KB)" -ForegroundColor Green
}

Write-Host "Oyna suratlari olinmoqda..." -ForegroundColor Cyan
Save-Window 'ShowWelcome'  'welcomeForm'  'oyna-qollanma.png'
Save-Window 'ShowSettings' 'settingsForm' 'oyna-sozlamalar.png'
Write-Host "TAYYOR." -ForegroundColor Green
