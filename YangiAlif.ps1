# ============================================================
#  YANGI ALIF — skript rejimi (zaxira yo'l)
#  Muallif: Xasanov Jasurbek
#
#  Bu yo'l Smart App Control YOQILGAN kompyuterlarda ham ishlaydi,
#  chunki dastur imzolangan powershell.exe ichida, xotirada ishlaydi
#  va imzosiz .exe fayl umuman yaratilmaydi.
#
#  -Silent : qo'llanma oynasini ko'rsatmaydi (avtostart uchun)
# ============================================================
param([switch]$Silent)

$Host.UI.RawUI.WindowTitle = 'Yangi Alif'

# Konsol oynasini yashiramiz
Add-Type -Name Win -Namespace ConsoleUtil -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("kernel32.dll")]
public static extern System.IntPtr GetConsoleWindow();
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);
'@
$h = [ConsoleUtil.Win]::GetConsoleWindow()
if ($h -ne [System.IntPtr]::Zero) { [ConsoleUtil.Win]::ShowWindow($h, 0) | Out-Null }

$cs = Join-Path $PSScriptRoot 'YangiAlif.cs'
if (-not (Test-Path $cs)) {
    Add-Type -AssemblyName System.Windows.Forms
    [System.Windows.Forms.MessageBox]::Show("YangiAlif.cs topilmadi.", "Yangi Alif") | Out-Null
    return
}

Add-Type -Path $cs -ReferencedAssemblies System.Windows.Forms, System.Drawing, mscorlib

[Program]::WorkDir     = $PSScriptRoot
[Program]::Portable    = $true          # o'rnatish oynasi kerak emas
[Program]::SilentStart = [bool]$Silent
[Program]::Main()
