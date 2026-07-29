' ============================================================
'  YANGI ALIF — oynasiz ishga tushiruvchi (zaxira yo'l)
'  Smart App Control yoqilgan kompyuterlar uchun.
'
'  Ikki marta bosing — dastur hech qanday oyna ko'rsatmasdan ishga tushadi.
'  "silent" argumenti berilsa, qo'llanma oynasi ham chiqmaydi.
' ============================================================
Dim fso, shell, scriptDir, silent, cmd
Set fso = CreateObject("Scripting.FileSystemObject")
Set shell = CreateObject("WScript.Shell")
scriptDir = fso.GetParentFolderName(WScript.ScriptFullName)

silent = ""
If WScript.Arguments.Count > 0 Then
    If LCase(WScript.Arguments(0)) = "silent" Then silent = " -Silent"
End If

cmd = "powershell.exe -NoProfile -ExecutionPolicy Bypass -Sta -WindowStyle Hidden -File """ & scriptDir & "\YangiAlif.ps1""" & silent

shell.Run cmd, 0, False
