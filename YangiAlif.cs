// ============================================================
//  YANGI_ALIF  —  yangi alifbo yordamchisi
//  Muallif: Xasanov Jasurbek
//  (c) Mualliflik huquqi himoyalangan.
//
//  Yozayotgan paytda harflarni avtomatik almashtiradi:
//      o' → ö     g' → ğ     sh → ş     ch → ç
//
//  Yozilgan matn HECH QAYERGA saqlanmaydi va yuborilmaydi.
// ============================================================

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32;

// ============================================================
//  Fokusni o'g'irlamaydigan bildirishnoma oynasi
// ============================================================
public class Toast : Form
{
    const int WS_EX_NOACTIVATE = 0x08000000;
    const int WS_EX_TOOLWINDOW  = 0x00000080;
    const int WS_EX_TOPMOST     = 0x00000008;

    protected override bool ShowWithoutActivation { get { return true; } }
    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
            return cp;
        }
    }

    string caption;
    Color accent;
    System.Windows.Forms.Timer life;
    int step = 0;
    float uiScale = 1f;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    public Toast(string text, Color color)
    {
        caption = text;
        accent = color;

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.FromArgb(24, 26, 32);
        Opacity = 0;

        // Ekran masshtabiga (125%, 150%) moslashamiz — aks holda kichkina chiqadi
        float scale = 1f;
        try { using (Graphics g = CreateGraphics()) scale = g.DpiX / 96f; } catch { }
        uiScale = scale;
        Size = new Size((int)(230 * scale), (int)(68 * scale));
        Font = new Font("Segoe UI", 9F);

        // Foydalanuvchi ishlayotgan ekranda ko'rsatamiz (ikki monitor uchun)
        Screen scr;
        try { scr = Screen.FromHandle(GetForegroundWindow()); }
        catch { scr = Screen.PrimaryScreen; }
        if (scr == null) scr = Screen.PrimaryScreen;

        Rectangle wa = scr.WorkingArea;
        Location = new Point(wa.Right - Width - (int)(24 * scale),
                             wa.Bottom - Height - (int)(24 * scale));

        life = new System.Windows.Forms.Timer();
        life.Interval = 40;
        life.Tick += Animate;
        life.Start();

        Paint += Draw;
    }

    void Animate(object s, EventArgs e)
    {
        step++;
        if (step <= 5) Opacity = step * 0.19;          // paydo bo'lish
        else if (step >= 26) Opacity -= 0.16;          // so'nish
        if (Opacity <= 0.01 && step > 26) { life.Stop(); Close(); }
    }

    void Draw(object s, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        float k = uiScale;
        using (Brush b = new SolidBrush(accent))
            g.FillRectangle(b, 0, 0, 5 * k, Height);

        using (Brush dot = new SolidBrush(accent))
            g.FillEllipse(dot, 22 * k, Height / 2f - 9 * k, 18 * k, 18 * k);

        // 11pt ≈ 15px, 8pt ≈ 11px — piksel birligida masshtablaymiz
        using (Font f1 = new Font("Segoe UI", 15F * k, FontStyle.Bold, GraphicsUnit.Pixel))
        using (Font f2 = new Font("Segoe UI", 11F * k, FontStyle.Regular, GraphicsUnit.Pixel))
        using (Brush w = new SolidBrush(Color.White))
        using (Brush w2 = new SolidBrush(Color.FromArgb(150, 255, 255, 255)))
        {
            g.DrawString(caption, f1, w, 54 * k, 16 * k);
            g.DrawString("Yangi Alif", f2, w2, 55 * k, 38 * k);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (life != null) life.Dispose();
        base.OnFormClosed(e);
    }
}

// ============================================================
public static class Program
{
    // ---------- Mahsulot ----------
    public const string AppName   = "Yangi Alif";
    public const string AppVer    = "1.0";
    public const string AppAuthor = "Xasanov Jasurbek";
    public const string Tagline   = "Yangi alifbo yordamchisi";

    // ---------- Qo'llab-quvvatlash ----------
    public const string SupportEmail    = "jasurbekxasanov214@gmail.com";
    public const string SupportTelegram = "@khasanov_jasur";

    public static string WorkDir     = "";
    public static bool   SilentStart = false;
    public static bool   Portable    = false;   // skript rejimi: o'rnatish oynasi chiqmasin

    const string MutexName = "YangiAlif_SingleInstance_v1";
    const string RegPath = @"Software\YangiAlif";
    const string RunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string RunName = "YangiAlif";

    // ---------- Brend ranglari (logotipdan olingan) ----------
    public static readonly Color Brand      = Color.FromArgb(72, 120, 48);    // asosiy yashil
    public static readonly Color BrandDark  = Color.FromArgb(44, 82, 32);
    public static readonly Color BrandLight = Color.FromArgb(122, 170, 80);
    public static readonly Color OkGreen    = Color.FromArgb(88, 150, 56);
    public static readonly Color OffGray    = Color.FromArgb(150, 155, 148);

    // ---------- Mavzuga bog'liq ranglar (yorug' / qorong'i) ----------
    // Windows mavzusiga qarab avtomatik tanlanadi.
    public static bool  DarkMode    = false;
    public static Color Surface     = Color.White;                       // oyna foni
    public static Color BrandPale   = Color.FromArgb(242, 247, 238);     // sarlavha foni
    public static Color CardBorder  = Color.FromArgb(205, 224, 196);
    public static Color Ink         = Color.FromArgb(38, 46, 34);        // asosiy matn
    public static Color Muted       = Color.FromArgb(108, 118, 104);     // ikkilamchi matn
    public static Color HeadTitle   = Color.FromArgb(44, 82, 32);        // sarlavha matni
    public static Color AccentText  = Color.FromArgb(72, 120, 48);       // qoidalardagi harflar
    public static Color PrivBg      = Color.FromArgb(240, 253, 244);
    public static Color PrivBorder  = Color.FromArgb(187, 247, 208);
    public static Color PrivTitle   = Color.FromArgb(21, 128, 61);
    public static Color PrivText    = Color.FromArgb(22, 101, 52);

    // Windows'da qorong'i mavzu yoqilganmi?
    static bool SystemUsesDarkTheme()
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (k == null) return false;
                object v = k.GetValue("AppsUseLightTheme");
                if (v == null) return false;
                return Convert.ToInt32(v) == 0;   // 0 = qorong'i
            }
        }
        catch { return false; }
    }

    static void ApplyTheme()
    {
        DarkMode = SystemUsesDarkTheme();
        if (!DarkMode) return;   // yorug' rejim — yuqoridagi qiymatlar qoladi

        Surface    = Color.FromArgb(32, 34, 31);
        BrandPale  = Color.FromArgb(40, 46, 38);
        CardBorder = Color.FromArgb(64, 74, 60);
        Ink        = Color.FromArgb(232, 236, 228);
        Muted      = Color.FromArgb(158, 168, 152);
        HeadTitle  = Color.FromArgb(168, 208, 130);
        AccentText = Color.FromArgb(150, 196, 108);
        PrivBg     = Color.FromArgb(34, 46, 36);
        PrivBorder = Color.FromArgb(62, 92, 60);
        PrivTitle  = Color.FromArgb(134, 214, 152);
        PrivText   = Color.FromArgb(178, 222, 186);
    }

    // Oyna sarlavhasini ham qorong'i qilamiz (Windows 10 1809+ / 11)
    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    static void ApplyDarkTitleBar(Form f)
    {
        if (!DarkMode) return;
        try
        {
            int on = 1;
            // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (yangi), 19 = eski quruvlar
            if (DwmSetWindowAttribute(f.Handle, 20, ref on, 4) != 0)
                DwmSetWindowAttribute(f.Handle, 19, ref on, 4);
        }
        catch { }
    }

    // ---------- WinAPI ----------
    const int WH_KEYBOARD_LL = 13;
    const int WH_MOUSE_LL    = 14;
    const int WM_KEYDOWN     = 0x0100;
    const int WM_KEYUP       = 0x0101;
    const int WM_SYSKEYDOWN  = 0x0104;
    const int WM_SYSKEYUP    = 0x0105;
    const int WM_LBUTTONDOWN = 0x0201;
    const int WM_RBUTTONDOWN = 0x0204;
    const int WM_MBUTTONDOWN = 0x0207;
    const int LLKHF_INJECTED = 0x10;

    const int PREV_TIMEOUT_MS = 3000;
    const int HOOK_WATCH_MS   = 20000;
    const int HOOK_IDLE_LIMIT = 90000;

    const int VK_BACK    = 0x08;
    const int VK_SHIFT   = 0x10;
    const int VK_CONTROL = 0x11;
    const int VK_MENU    = 0x12;
    const int VK_CAPITAL = 0x14;
    const int VK_LSHIFT  = 0xA0, VK_RSHIFT = 0xA1;
    const int VK_LCTRL   = 0xA2, VK_RCTRL  = 0xA3;
    const int VK_LWIN    = 0x5B, VK_RWIN   = 0x5C;
    const int VK_OEM_7   = 0xDE;
    const int VK_O = 0x4F, VK_G = 0x47, VK_S = 0x53, VK_C = 0x43, VK_H = 0x48;

    const uint KEYEVENTF_KEYUP   = 0x0002;
    const uint KEYEVENTF_UNICODE = 0x0004;
    const uint INPUT_KEYBOARD    = 1;

    delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)] static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll", SetLastError = true)] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)] static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")] static extern short GetKeyState(int nVirtKey);
    [DllImport("user32.dll", SetLastError = true)] static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    [DllImport("user32.dll", SetLastError = true)] static extern bool DestroyIcon(IntPtr hIcon);

    // --- Klaviatura tilini aniqlash uchun ---
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
    [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint idThread);
    [DllImport("user32.dll")] static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

    // --- Parol maydonini aniqlash uchun ---
    [StructLayout(LayoutKind.Sequential)]
    struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    struct GUITHREADINFO
    {
        public int cbSize; public int flags;
        public IntPtr hwndActive, hwndFocus, hwndCapture, hwndMenuOwner, hwndMoveSize, hwndCaret;
        public RECT rcCaret;
    }
    [DllImport("user32.dll")] static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    const int  GWL_STYLE   = -16;
    const int  ES_PASSWORD = 0x0020;
    const uint MAPVK_VK_TO_CHAR = 2;

    [StructLayout(LayoutKind.Sequential)]
    struct KBDLLHOOKSTRUCT { public uint vkCode; public uint scanCode; public uint flags; public uint time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT { public uint type; public InputUnion U; }
    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    struct HARDWAREINPUT { public uint uMsg; public ushort wParamL; public ushort wParamH; }

    // ---------- Sozlamalar ----------
    static bool ruleOG       = true;
    static bool ruleShCh     = true;
    static bool startEnabled = false;
    static bool showWelcome  = true;
    static bool showToast    = true;
    static string excludedApps = "";   // bu dasturlarda ishlamasin (vergul bilan)

    // ---------- Holat ----------
    static bool   enabled      = false;
    static char   prev         = '\0';
    static bool   justReplaced = false;
    static string lastOrig     = "";
    static int    lastKeyTick  = 0;
    static bool   csArmed = false, csDirty = false;

    static IntPtr hookId = IntPtr.Zero, mouseHookId = IntPtr.Zero;
    static HookProc keyProc = HookCallback, mouseProc = MouseCallback;
    static int lastHookTick = 0;

    static NotifyIcon tray;
    static Icon onIcon, offIcon;
    static IntPtr onHandle = IntPtr.Zero, offHandle = IntPtr.Zero;
    static Image customLogo;             // foydalanuvchi qo'ygan Logo.png
    static Mutex mutex;

    static System.Windows.Forms.Timer injectTimer, watchTimer;
    static Queue<INPUT[]> injectQueue = new Queue<INPUT[]>();
    static Toast liveToast;

    // ============================================================
    [STAThread]
    public static void Main()
    {
        bool doUninstall = false, portable = false, silentInstall = false;
        foreach (string a in Environment.GetCommandLineArgs())
        {
            string s = a.TrimStart('-', '/').ToLowerInvariant();
            if (s == "silent")    SilentStart = true;
            if (s == "uninstall") doUninstall = true;
            if (s == "portable")  portable = true;
            if (s == "install")   silentInstall = true;   // jim o'rnatish
        }

        if (string.IsNullOrEmpty(WorkDir))
        {
            try { WorkDir = Path.GetDirectoryName(Application.ExecutablePath); }
            catch { WorkDir = Environment.CurrentDirectory; }
        }

        try { SetProcessDPIAware(); } catch { }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ApplyTheme();      // Windows yorug'/qorong'i mavzusiga moslashamiz

        // --- O'chirish rejimi ---
        if (doUninstall) { RunUninstall(); return; }

        // --- Jim o'rnatish (ko'p kompyuterga tarqatish uchun) ---
        if (silentInstall && !IsInstalled())
        {
            try { DoInstall(true, true); } catch { Environment.Exit(1); }
            Environment.Exit(0);
        }

        // --- Hali o'rnatilmagan bo'lsa: o'rnatish oynasini ko'rsatamiz ---
        if (!portable && !Portable && !IsInstalled())
        {
            ShowInstaller();
            return;
        }

        bool createdNew;
        mutex = new Mutex(true, MutexName, out createdNew);
        if (!createdNew)
        {
            if (!SilentStart)
                MessageBox.Show(AppName + " allaqachon ishlab turibdi.\n\n" +
                                "Uni soat yonidagi belgidan boshqaring.",
                                AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.ThreadException += delegate (object s, ThreadExceptionEventArgs e) { ShowError(e.Exception); };
        AppDomain.CurrentDomain.UnhandledException += delegate (object s, UnhandledExceptionEventArgs e)
        { LogError(e.ExceptionObject as Exception); };

        LoadSettings();
        enabled = startEnabled;

        LoadLogo();
        onIcon  = MakeIcon(true);
        offIcon = MakeIcon(false);

        InstallHooks();

        injectTimer = new System.Windows.Forms.Timer();
        injectTimer.Interval = 1;
        injectTimer.Tick += FlushInject;

        watchTimer = new System.Windows.Forms.Timer();
        watchTimer.Interval = HOOK_WATCH_MS;
        watchTimer.Tick += WatchHooks;
        watchTimer.Start();

        BuildTray();

        if (!SilentStart && showWelcome) ShowWelcome();
        else ShowToast(enabled);

        Application.Run();

        RemoveHooks();
        if (tray != null) { tray.Visible = false; tray.Dispose(); }
        FreeIcons();
    }

    static void ShowError(Exception ex)
    {
        LogError(ex);
        try
        {
            MessageBox.Show(
                "Kutilmagan xatolik yuz berdi:\n\n" + (ex == null ? "?" : ex.Message) +
                "\n\nTafsilotlar quyidagi faylga yozildi:\n" + ErrorLogPath +
                "\n\nShu faylni " + SupportEmail + " manziliga yuborsangiz,\n" +
                "muammoni tezroq hal qilamiz.",
                AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch { }
    }

    // Xatolar jurnali — foydalanuvchi yordam so'raganda sababni aniqlash uchun.
    // Bu yerga FAQAT texnik xato ma'lumoti yoziladi, yozgan matningiz EMAS.
    public static string ErrorLogPath
    {
        get
        {
            try { return Path.Combine(Path.GetTempPath(), "YangiAlif-xatolar.txt"); }
            catch { return "YangiAlif-xatolar.txt"; }
        }
    }

    static void LogError(Exception ex)
    {
        if (ex == null) return;
        try
        {
            string txt =
                "==================================================\r\n" +
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + AppName + " " + AppVer + "\r\n" +
                "Windows: " + Environment.OSVersion.VersionString +
                "   (" + (IntPtr.Size == 8 ? "64-bit" : "32-bit") + ")\r\n" +
                ".NET: " + Environment.Version + "\r\n" +
                "--------------------------------------------------\r\n" +
                ex.ToString() + "\r\n\r\n";

            // Jurnal cheksiz o'smasin
            try
            {
                FileInfo fi = new FileInfo(ErrorLogPath);
                if (fi.Exists && fi.Length > 256 * 1024) fi.Delete();
            }
            catch { }

            File.AppendAllText(ErrorLogPath, txt);
        }
        catch { }
    }

    // ============================================================
    //  O'RNATISH  (dastur o'zini o'rnatadi — alohida Setup fayl kerak emas)
    // ============================================================
    const string UninstKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\YangiAlif";

    public static string InstallDir
    {
        get
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YangiAlif");
        }
    }

    static bool IsInstalled()
    {
        try
        {
            string me = Path.GetDirectoryName(Application.ExecutablePath);
            return string.Equals(me.TrimEnd('\\'), InstallDir.TrimEnd('\\'),
                                 StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    // O'rnatish oynasi — sodda, bitta katta tugma
    static void ShowInstaller()
    {
        Form f = BrandWindow("O'rnatish", 520, 474);
        f.FormBorderStyle = FormBorderStyle.FixedDialog;

        Panel c = new Panel();
        c.Dock = DockStyle.Fill;
        c.Padding = new Padding(26, 16, 26, 16);
        f.Controls.Add(c); c.BringToFront();

        c.Controls.Add(Lbl("Xush kelibsiz!", 0, 2, 460, 28, 14F, FontStyle.Bold, Ink));
        c.Controls.Add(Lbl("Bu dastur yozayotganingizda harflarni avtomatik almashtiradi:",
                           0, 32, 470, 22, 9.5F, FontStyle.Regular, Muted));
        c.Controls.Add(RulesPanel(0, 58, 466));

        CheckBox cbAuto = new CheckBox();
        cbAuto.Text = "Kompyuter yonganda o'zi ishga tushsin";
        cbAuto.Checked = true;
        cbAuto.SetBounds(2, 166, 440, 24);
        cbAuto.ForeColor = Ink;
        c.Controls.Add(cbAuto);

        CheckBox cbDesk = new CheckBox();
        cbDesk.Text = "Ish stolida yorliq yaratilsin";
        cbDesk.Checked = true;
        cbDesk.SetBounds(2, 192, 440, 24);
        cbDesk.ForeColor = Ink;
        c.Controls.Add(cbDesk);

        Label status = Lbl("", 2, 224, 460, 22, 9.5F, FontStyle.Regular, Brand);
        c.Controls.Add(status);

        Button install = PrimaryButton("O'RNATISH", 0, 252, 466);
        install.Height = 46;
        install.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        install.Click += delegate
        {
            install.Enabled = false;
            status.Text = "O'rnatilmoqda, kuting...";
            Application.DoEvents();
            try
            {
                DoInstall(cbAuto.Checked, cbDesk.Checked);
                status.ForeColor = PrivTitle;
                status.Text = "Tayyor! Dastur ishga tushmoqda...";
                Application.DoEvents();
                System.Threading.Thread.Sleep(700);
                f.Close();
            }
            catch (Exception ex)
            {
                install.Enabled = true;
                status.ForeColor = Color.FromArgb(190, 40, 40);
                status.Text = "Xatolik: " + ex.Message;
            }
        };
        c.Controls.Add(install);

        c.Controls.Add(Lbl("Dastur faqat sizning foydalanuvchi papkangizga o'rnatiladi.\n" +
                           "Administrator huquqi talab qilinmaydi. Istalgan vaqtda o'chirish mumkin.",
                           0, 306, 470, 40, 8.5F, FontStyle.Regular, Muted));
        c.Controls.Add(Lbl("© " + AppAuthor + ". Mualliflik huquqi himoyalangan.",
                           0, 344, 470, 20, 8F, FontStyle.Regular, Muted));

        Application.Run(f);
    }

    static void DoInstall(bool autoStart, bool desktopShortcut)
    {
        // Ishlab turgan nusxalarni to'xtatamiz
        try
        {
            foreach (System.Diagnostics.Process p in
                     System.Diagnostics.Process.GetProcessesByName("YangiAlif"))
            {
                if (p.Id == System.Diagnostics.Process.GetCurrentProcess().Id) continue;
                try { p.Kill(); p.WaitForExit(4000); } catch { }
            }
        }
        catch { }

        Directory.CreateDirectory(InstallDir);
        string target = Path.Combine(InstallDir, "YangiAlif.exe");
        File.Copy(Application.ExecutablePath, target, true);

        // Yorliqlar
        Shortcut(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.Programs), AppName + ".lnk"), target);
        if (desktopShortcut)
            Shortcut(Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory), AppName + ".lnk"), target);

        // Windows "Ilovalar" ro'yxati
        try
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(UninstKey))
            {
                k.SetValue("DisplayName", AppName);
                k.SetValue("DisplayVersion", AppVer);
                k.SetValue("Publisher", AppAuthor);
                k.SetValue("DisplayIcon", target + ",0");
                k.SetValue("InstallLocation", InstallDir);
                k.SetValue("UninstallString", "\"" + target + "\" /uninstall");
                k.SetValue("NoModify", 1, RegistryValueKind.DWord);
                k.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                try { k.SetValue("EstimatedSize", (int)(new FileInfo(target).Length / 1024), RegistryValueKind.DWord); }
                catch { }
            }
        }
        catch { }

        // Avtostart
        try
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RunPath))
            {
                if (autoStart) k.SetValue(RunName, "\"" + target + "\" /silent");
                else if (k.GetValue(RunName) != null) k.DeleteValue(RunName, false);
            }
        }
        catch { }

        // O'rnatilgan nusxani ishga tushiramiz
        try { System.Diagnostics.Process.Start(target); } catch { }
    }

    static void Shortcut(string lnk, string target)
    {
        try
        {
            Type t = Type.GetTypeFromProgID("WScript.Shell");
            object shell = Activator.CreateInstance(t);
            object sc = t.InvokeMember("CreateShortcut",
                System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { lnk });
            Type ts = sc.GetType();
            ts.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { target });
            ts.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { Path.GetDirectoryName(target) });
            ts.InvokeMember("IconLocation", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { target + ",0" });
            ts.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, sc, new object[] { AppName + " — " + Tagline });
            ts.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, sc, null);
        }
        catch { }
    }

    static void RunUninstall()
    {
        if (MessageBox.Show(AppName + " kompyuteringizdan o'chirilsinmi?",
                            AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            foreach (System.Diagnostics.Process p in
                     System.Diagnostics.Process.GetProcessesByName("YangiAlif"))
            {
                if (p.Id == System.Diagnostics.Process.GetCurrentProcess().Id) continue;
                try { p.Kill(); p.WaitForExit(3000); } catch { }
            }
        }
        catch { }

        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(RunPath, true))
                if (k != null && k.GetValue(RunName) != null) k.DeleteValue(RunName, false);
        }
        catch { }
        try { File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AppName + ".lnk")); } catch { }
        try { File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), AppName + ".lnk")); } catch { }
        try { Registry.CurrentUser.DeleteSubKeyTree(UninstKey, false); } catch { }
        try { Registry.CurrentUser.DeleteSubKeyTree(RegPath, false); } catch { }

        // O'zini o'chira olmaydi — kichik skript orqali tozalaymiz
        try
        {
            string bat = Path.Combine(Path.GetTempPath(), "yangialif_cleanup.bat");
            File.WriteAllText(bat,
                "@echo off\r\nping 127.0.0.1 -n 3 >nul\r\n" +
                "rd /s /q \"" + InstallDir + "\" >nul 2>&1\r\n" +
                "del /f /q \"%~f0\" >nul 2>&1\r\n");
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(bat);
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            System.Diagnostics.Process.Start(psi);
        }
        catch { }

        MessageBox.Show(AppName + " o'chirildi.\n\nFoydalanganingiz uchun rahmat!",
                        AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ============================================================
    //  TRAY
    // ============================================================
    static void BuildTray()
    {
        tray = new NotifyIcon();
        tray.Visible = true;

        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Font = new Font("Segoe UI", 9.5F);
        menu.Items.Add("Yoqish / O'chirish     Ctrl+Shift", null, delegate { Toggle(); });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Sozlamalar...", null, delegate { ShowSettings(); });
        menu.Items.Add("Qo'llanma", null, delegate { ShowWelcome(); });
        menu.Items.Add("Dastur haqida", null, delegate { ShowAbout(); });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Chiqish", null, delegate { ExitApp(); });
        tray.ContextMenuStrip = menu;
        tray.DoubleClick += delegate { Toggle(); };

        UpdateTray();
    }

    static void UpdateTray()
    {
        if (tray == null) return;
        tray.Icon = enabled ? onIcon : offIcon;
        tray.Text = enabled ? AppName + " — YOQILGAN"
                            : AppName + " — o'chirilgan (Ctrl+Shift)";
    }

    static void ExitApp()
    {
        if (tray != null) tray.Visible = false;
        Application.Exit();
    }

    static void Toggle()
    {
        enabled = !enabled;
        prev = '\0';
        justReplaced = false;
        UpdateTray();
        ShowToast(enabled);
    }

    static void ShowToast(bool on)
    {
        if (!showToast) return;
        try
        {
            if (liveToast != null && !liveToast.IsDisposed) liveToast.Close();
            liveToast = new Toast(on ? "YOQILDI" : "O'CHDI", on ? OkGreen : OffGray);
            liveToast.Show();
        }
        catch { }
    }

    // ============================================================
    //  HOOK'LAR
    // ============================================================
    static bool hookWarned = false;

    static void InstallHooks()
    {
        IntPtr h = GetModuleHandle(null);
        hookId      = SetWindowsHookEx(WH_KEYBOARD_LL, keyProc,   h, 0);
        mouseHookId = SetWindowsHookEx(WH_MOUSE_LL,    mouseProc, h, 0);
        lastHookTick = Environment.TickCount;

        // Hook o'rnatilmasa dastur jimgina ishlamay qoladi — foydalanuvchi
        // sababini bilmay qiynaladi. Shuning uchun ochiq aytamiz.
        if (hookId == IntPtr.Zero && !hookWarned)
        {
            hookWarned = true;
            MessageBox.Show(
                "Dastur klaviaturaga ulana olmadi.\n\n" +
                "Sabab: antivirus yoki boshqa xavfsizlik dasturi to'sqinlik qilayotgan bo'lishi mumkin.\n\n" +
                "Yechim: " + AppName + " ni antivirus ruxsat ro'yxatiga qo'shing " +
                "yoki dasturni qayta ishga tushiring.",
                AppName + " — ogohlantirish", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    static void RemoveHooks()
    {
        if (hookId != IntPtr.Zero)      { UnhookWindowsHookEx(hookId);      hookId = IntPtr.Zero; }
        if (mouseHookId != IntPtr.Zero) { UnhookWindowsHookEx(mouseHookId); mouseHookId = IntPtr.Zero; }
    }

    // Windows sekin hook'ni jimgina o'chirib qo'yishi mumkin.
    // Uzoq vaqt hodisa bo'lmasa (kompyuter bo'sh) - xavfsiz qayta o'rnatamiz.
    static void WatchHooks(object s, EventArgs e)
    {
        if (Environment.TickCount - lastHookTick > HOOK_IDLE_LIMIT)
        {
            RemoveHooks();
            InstallHooks();
        }
    }

    static IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            lastHookTick = Environment.TickCount;
            int m = wParam.ToInt32();
            if (m == WM_LBUTTONDOWN || m == WM_RBUTTONDOWN || m == WM_MBUTTONDOWN)
            { prev = '\0'; justReplaced = false; }
        }
        return CallNextHookEx(mouseHookId, nCode, wParam, lParam);
    }

    static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            lastHookTick = Environment.TickCount;
            int msg = wParam.ToInt32();
            bool keyDown = (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN);
            bool keyUp   = (msg == WM_KEYUP   || msg == WM_SYSKEYUP);

            KBDLLHOOKSTRUCT k = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            bool injected = (k.flags & LLKHF_INJECTED) != 0;
            int vk = (int)k.vkCode;

            if (!injected)
            {
                bool isShift = (vk == VK_SHIFT   || vk == VK_LSHIFT || vk == VK_RSHIFT);
                bool isCtrl  = (vk == VK_CONTROL || vk == VK_LCTRL  || vk == VK_RCTRL);

                // Ctrl+Shift "toza bosish". Bosilayotgan tugmaning o'zi hook ichida
                // hali "bosilgan" ko'rinmasligi mumkin -> IKKINCHI tugmani tekshiramiz.
                if (keyDown)
                {
                    if (isShift || isCtrl)
                    {
                        bool other = isShift ? Down(VK_CONTROL) : Down(VK_SHIFT);
                        if (other) { csArmed = true; csDirty = false; }
                    }
                    else if (Down(VK_CONTROL) || Down(VK_SHIFT))
                    { csDirty = true; csArmed = false; }
                }
                if (keyUp && (isShift || isCtrl))
                {
                    if (csArmed && !csDirty) { csArmed = false; Toggle(); }
                    if (!Down(VK_CONTROL) && !Down(VK_SHIFT)) { csArmed = false; csDirty = false; }
                }

                if (enabled && keyDown && ProcessKey(vk)) return (IntPtr)1;
            }
        }
        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }

    static bool ProcessKey(int vk)
    {
        if (Down(VK_CONTROL) || Down(VK_MENU) || Down(VK_LWIN) || Down(VK_RWIN))
        { prev = '\0'; return false; }

        if (vk == VK_BACK)
        {
            if (justReplaced)
            {
                justReplaced = false;
                ReplaceOnScreen(1, lastOrig);
                prev = '\0';
                return true;
            }
            prev = '\0';
            return false;
        }

        justReplaced = false;
        if (prev != '\0' && Environment.TickCount - lastKeyTick > PREV_TIMEOUT_MS) prev = '\0';

        // Istisno qilingan dastur (o'yin, terminal...) — tegmaymiz
        if (IsExcludedApp()) { prev = '\0'; return false; }

        // Tugma shu klaviatura tilida qaysi harfni beradi?
        // Rus/kirill tilida "s" tugmasi "ы" beradi -> almashtirmaymiz.
        char ch = char.ToLowerInvariant(CharFromVk(vk));
        bool upper = IsUpper();

        // --- Apostrof:  o' -> ö,  g' -> ğ ---
        if (ch == '\'' && !Down(VK_SHIFT) && ruleOG)
        {
            char o = '\0'; string s = null;
            if      (prev == 'o') { o = 'ö'; s = "o'"; }
            else if (prev == 'O') { o = 'Ö'; s = "O'"; }
            else if (prev == 'g') { o = 'ğ'; s = "g'"; }
            else if (prev == 'G') { o = 'Ğ'; s = "G'"; }
            if (o != '\0')
            {
                if (IsPasswordField()) { prev = '\0'; return false; }   // parolga tegmaymiz
                ReplaceOnScreen(1, o.ToString());
                prev = '\0'; justReplaced = true; lastOrig = s;
                return true;
            }
            prev = '\0'; return false;
        }

        // --- h:  sh -> ş,  ch -> ç ---
        if (ch == 'h' && ruleShCh)
        {
            char o = '\0'; string s = null;
            if      (prev == 's') { o = 'ş'; s = "sh"; }
            else if (prev == 'S') { o = 'Ş'; s = "Sh"; }
            else if (prev == 'c') { o = 'ç'; s = "ch"; }
            else if (prev == 'C') { o = 'Ç'; s = "Ch"; }
            if (o != '\0')
            {
                if (IsPasswordField()) { prev = '\0'; return false; }   // parolga tegmaymiz
                ReplaceOnScreen(1, o.ToString());
                prev = '\0'; justReplaced = true; lastOrig = s;
                return true;
            }
            prev = '\0'; return false;
        }

        // --- Kutiladigan birinchi harflar ---
        if (ch == 'o' || ch == 'g' || ch == 's' || ch == 'c')
        {
            prev = upper ? char.ToUpperInvariant(ch) : ch;
            lastKeyTick = Environment.TickCount;
            return false;
        }

        prev = '\0';
        return false;
    }

    // Hook ichida SendInput CHAQIRILMAYDI - navbatga qo'yiladi,
    // taymer hook tugagach yuboradi. Shundagina barqaror ishlaydi.
    static void ReplaceOnScreen(int backspaces, string text)
    {
        INPUT[] inp = new INPUT[backspaces * 2 + text.Length * 2];
        int i = 0;
        for (int b = 0; b < backspaces; b++)
        {
            inp[i++] = Kb((ushort)VK_BACK, 0, 0);
            inp[i++] = Kb((ushort)VK_BACK, 0, KEYEVENTF_KEYUP);
        }
        foreach (char c in text)
        {
            inp[i++] = Kb(0, (ushort)c, KEYEVENTF_UNICODE);
            inp[i++] = Kb(0, (ushort)c, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP);
        }
        injectQueue.Enqueue(inp);
        injectTimer.Start();
    }

    static void FlushInject(object s, EventArgs e)
    {
        injectTimer.Stop();
        while (injectQueue.Count > 0)
        {
            INPUT[] a = injectQueue.Dequeue();
            SendInput((uint)a.Length, a, Marshal.SizeOf(typeof(INPUT)));
        }
    }

    static INPUT Kb(ushort vk, ushort scan, uint flags)
    {
        INPUT i = new INPUT();
        i.type = INPUT_KEYBOARD;
        i.U.ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = flags, time = 0, dwExtraInfo = IntPtr.Zero };
        return i;
    }

    // ============================================================
    //  HIMOYA MEXANIZMLARI
    // ============================================================

    // Faol oynaning klaviatura tilida shu tugma qaysi harfni beradi?
    // Bu MUHIM: foydalanuvchi rus/kirill tiliga o'tsa, "s" tugmasi "ы" beradi —
    // unda almashtirish ISHLAMASLIGI kerak, aks holda matn buziladi.
    static char CharFromVk(int vk)
    {
        try
        {
            uint pid;
            uint tid = GetWindowThreadProcessId(GetForegroundWindow(), out pid);
            IntPtr layout = GetKeyboardLayout(tid);
            uint r = MapVirtualKeyEx((uint)vk, MAPVK_VK_TO_CHAR, layout);
            return (char)(r & 0x7FFF);
        }
        catch { return '\0'; }
    }

    // Parol maydonimi? Agar shunday bo'lsa — HECH NARSA almashtirmaymiz.
    // Aks holda paroli "shaxs" bo'lgan odam hisobiga kira olmay qoladi.
    static bool IsPasswordField()
    {
        try
        {
            IntPtr fg = GetForegroundWindow();
            if (fg == IntPtr.Zero) return false;
            uint pid;
            uint tid = GetWindowThreadProcessId(fg, out pid);

            GUITHREADINFO gti = new GUITHREADINFO();
            gti.cbSize = Marshal.SizeOf(typeof(GUITHREADINFO));
            if (!GetGUIThreadInfo(tid, ref gti) || gti.hwndFocus == IntPtr.Zero) return false;

            System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
            GetClassName(gti.hwndFocus, sb, sb.Capacity);
            string cls = sb.ToString();

            // MUHIM: zamonaviy dasturlarda klass nomi shunchaki "Edit" emas —
            // masalan WinForms "WindowsForms10.EDIT.app.0.xxxx" beradi.
            // Shuning uchun nom ICHIDA "edit" bor-yo'qligini tekshiramiz.
            if (cls.IndexOf("edit", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int style = GetWindowLong(gti.hwndFocus, GWL_STYLE);
                if ((style & ES_PASSWORD) != 0) return true;
            }
        }
        catch { }
        return false;
    }

    // Foydalanuvchi istisno qilgan dasturmi? (o'yinlar, terminal va h.k.)
    static IntPtr cachedHwnd = IntPtr.Zero;
    static bool   cachedExcluded = false;

    static bool IsExcludedApp()
    {
        if (string.IsNullOrEmpty(excludedApps)) return false;
        try
        {
            IntPtr fg = GetForegroundWindow();
            if (fg == cachedHwnd) return cachedExcluded;   // tez: oyna o'zgarmagan
            cachedHwnd = fg;
            cachedExcluded = false;
            if (fg == IntPtr.Zero) return false;

            uint pid;
            GetWindowThreadProcessId(fg, out pid);
            string name = System.Diagnostics.Process.GetProcessById((int)pid).ProcessName;

            foreach (string raw in excludedApps.Split(',', ';'))
            {
                string s = raw.Trim();
                if (s.Length == 0) continue;
                if (s.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    s = s.Substring(0, s.Length - 4);
                if (name.Equals(s, StringComparison.OrdinalIgnoreCase)) { cachedExcluded = true; break; }
            }
            return cachedExcluded;
        }
        catch { return false; }
    }

    static bool Down(int vk) { return (GetAsyncKeyState(vk) & 0x8000) != 0; }
    static bool IsUpper()
    {
        bool sh = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
        bool cp = (GetKeyState(VK_CAPITAL) & 0x0001) != 0;
        return sh ^ cp;
    }

    // ============================================================
    //  SOZLAMALAR
    // ============================================================
    static void LoadSettings()
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(RegPath))
            {
                if (k == null) return;
                ruleOG       = RB(k, "RuleOG", true);
                ruleShCh     = RB(k, "RuleShCh", true);
                startEnabled = RB(k, "StartEnabled", false);
                showWelcome  = RB(k, "ShowWelcome", true);
                showToast    = RB(k, "ShowToast", true);
                object ex = k.GetValue("ExcludedApps");
                if (ex != null) excludedApps = ex.ToString();
            }
        }
        catch { }
    }

    static bool RB(RegistryKey k, string n, bool d)
    {
        object v = k.GetValue(n);
        if (v == null) return d;
        try { return Convert.ToInt32(v) != 0; } catch { return d; }
    }

    static void SaveSettings()
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RegPath))
            {
                k.SetValue("RuleOG",       ruleOG       ? 1 : 0, RegistryValueKind.DWord);
                k.SetValue("RuleShCh",     ruleShCh     ? 1 : 0, RegistryValueKind.DWord);
                k.SetValue("StartEnabled", startEnabled ? 1 : 0, RegistryValueKind.DWord);
                k.SetValue("ShowWelcome",  showWelcome  ? 1 : 0, RegistryValueKind.DWord);
                k.SetValue("ShowToast",    showToast    ? 1 : 0, RegistryValueKind.DWord);
                k.SetValue("ExcludedApps", excludedApps == null ? "" : excludedApps);
            }
        }
        catch { }
    }

    static string AutoStartCommand()
    {
        string exe = "";
        try { exe = Application.ExecutablePath; } catch { }
        if (!string.IsNullOrEmpty(exe) &&
            Path.GetFileName(exe).Equals("YangiAlif.exe", StringComparison.OrdinalIgnoreCase))
            return "\"" + exe + "\" /silent";
        return "wscript.exe \"" + Path.Combine(WorkDir, "YangiAlif.vbs") + "\" silent";
    }

    static bool IsAutoStart()
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(RunPath))
                return k != null && k.GetValue(RunName) != null;
        }
        catch { return false; }
    }

    static void SetAutoStart(bool on)
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RunPath))
            {
                if (on) k.SetValue(RunName, AutoStartCommand());
                else if (k.GetValue(RunName) != null) k.DeleteValue(RunName, false);
            }
        }
        catch { }
    }

    // ============================================================
    //  UMUMIY OYNA USLUBI
    // ============================================================
    static Form BrandWindow(string title, int w, int h)
    {
        Form f = new Form();
        f.Text = AppName + " — " + title;
        f.FormBorderStyle = FormBorderStyle.FixedDialog;
        f.MaximizeBox = false; f.MinimizeBox = false;
        f.StartPosition = FormStartPosition.CenterScreen;
        f.AutoScaleMode = AutoScaleMode.Font;
        f.Font = new Font("Segoe UI", 9.5F);
        f.ClientSize = new Size(w, h);
        f.BackColor = Surface;
        f.ForeColor = Ink;
        if (onIcon != null) f.Icon = onIcon;
        f.HandleCreated += delegate { ApplyDarkTitleBar(f); };

        Panel head = new Panel();
        head.Dock = DockStyle.Top;
        head.Height = 104;
        head.Paint += delegate (object s, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Och yashil fon — logotip aynan shunday fonda yaratilgan
            using (Brush b = new SolidBrush(BrandPale))
                g.FillRectangle(b, 0, 0, head.Width, head.Height);
            using (Pen p = new Pen(CardBorder))
                g.DrawLine(p, 0, head.Height - 1, head.Width, head.Height - 1);

            // Logotip (ichida "YANGI ALIF" yozuvi bor — takrorlamaymiz)
            DrawLogo(g, new Rectangle(18, 8, 88, 88), true, false);

            using (Font f1 = new Font("Segoe UI", 11.5F, FontStyle.Bold))
            using (Font f2 = new Font("Segoe UI", 8.5F))
            using (Brush ib = new SolidBrush(HeadTitle))
            using (Brush mb = new SolidBrush(Muted))
            {
                g.DrawString(title, f1, ib, 116, 34);
                g.DrawString(Tagline + "  •  v" + AppVer, f2, mb, 118, 56);
            }
        };
        f.Controls.Add(head);
        return f;
    }

    static Label Lbl(string t, int x, int y, int w, int h, float size, FontStyle st, Color c)
    {
        Label l = new Label();
        l.Text = t; l.SetBounds(x, y, w, h);
        l.Font = new Font("Segoe UI", size, st);
        l.ForeColor = c;
        l.BackColor = Color.Transparent;
        return l;
    }

    static Button PrimaryButton(string text, int x, int y, int w)
    {
        Button b = new Button();
        b.Text = text;
        b.SetBounds(x, y, w, 34);
        b.FlatStyle = FlatStyle.Flat;
        b.BackColor = Brand;
        b.ForeColor = Color.White;
        b.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        b.FlatAppearance.BorderSize = 0;
        b.Cursor = Cursors.Hand;
        return b;
    }

    // Qoidalarni chiroyli ko'rsatadigan panel
    static Panel RulesPanel(int x, int y, int w)
    {
        Panel p = new Panel();
        p.SetBounds(x, y, w, 96);
        p.BackColor = BrandPale;
        p.Paint += delegate (object s, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (Pen pen = new Pen(CardBorder))
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);

            string[,] rules = { { "o'", "ö" }, { "g'", "ğ" }, { "sh", "ş" }, { "ch", "ç" } };
            using (Font fb = new Font("Segoe UI", 13F, FontStyle.Bold))
            using (Font fa = new Font("Segoe UI", 11F))
            using (Brush ink = new SolidBrush(Ink))
            using (Brush br = new SolidBrush(AccentText))
            using (Brush mu = new SolidBrush(Muted))
            {
                int cw = p.Width / 4;
                for (int i = 0; i < 4; i++)
                {
                    int cx = i * cw;
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    e.Graphics.DrawString(rules[i, 0], fa, mu, new RectangleF(cx, 20, cw, 24), sf);
                    e.Graphics.DrawString("↓", fa, mu, new RectangleF(cx, 40, cw, 20), sf);
                    e.Graphics.DrawString(rules[i, 1], fb, br, new RectangleF(cx, 58, cw, 30), sf);
                }
            }
        };
        return p;
    }

    // ============================================================
    //  QO'LLANMA OYNASI
    // ============================================================
    static Form welcomeForm;

    static void ShowWelcome()
    {
        if (welcomeForm != null && !welcomeForm.IsDisposed)
        { welcomeForm.Activate(); return; }

        Form f = BrandWindow("Qo'llanma", 520, 508);
        welcomeForm = f;

        Panel c = new Panel();
        c.Dock = DockStyle.Fill;
        c.Padding = new Padding(26, 16, 26, 16);
        f.Controls.Add(c);
        c.BringToFront();

        c.Controls.Add(Lbl("Xush kelibsiz!", 0, 4, 460, 28, 14F, FontStyle.Bold, Ink));
        c.Controls.Add(Lbl("Bu dastur siz yozayotgan paytda harflarni avtomatik almashtiradi.",
                           0, 34, 470, 22, 9.5F, FontStyle.Regular, Muted));

        c.Controls.Add(RulesPanel(0, 62, 466));

        c.Controls.Add(Lbl("Bosh harflar ham ishlaydi:   O' → Ö     Sh → Ş     Ch → Ç",
                           0, 166, 466, 22, 9.5F, FontStyle.Regular, Muted));

        // Qadamlar
        c.Controls.Add(Lbl("1", 4, 200, 24, 24, 11F, FontStyle.Bold, Brand));
        c.Controls.Add(Lbl("Yozishdan oldin  Ctrl + Shift  ni bosing — dastur YOQILADI.",
                           30, 200, 440, 22, 10F, FontStyle.Regular, Ink));

        c.Controls.Add(Lbl("2", 4, 228, 24, 24, 11F, FontStyle.Bold, Brand));
        c.Controls.Add(Lbl("Yozing:   shahar → şahar,    o'zbek → özbek",
                           30, 228, 440, 22, 10F, FontStyle.Regular, Ink));

        c.Controls.Add(Lbl("3", 4, 256, 24, 24, 11F, FontStyle.Bold, Brand));
        c.Controls.Add(Lbl("Kerak bo'lsa DARHOL Backspace bosing — asl holi qaytadi (ş → sh).",
                           30, 256, 450, 22, 10F, FontStyle.Regular, Ink));

        c.Controls.Add(Lbl("4", 4, 284, 24, 24, 11F, FontStyle.Bold, Brand));
        c.Controls.Add(Lbl("Sozlamalar uchun soat yonidagi belgiga o'ng tugma bosing.",
                           30, 284, 440, 22, 10F, FontStyle.Regular, Ink));

        CheckBox cb = new CheckBox();
        cb.Text = "Keyingi safar bu oyna ko'rsatilmasin";
        cb.SetBounds(2, 322, 320, 24);
        cb.ForeColor = Muted;
        c.Controls.Add(cb);

        Button ok = PrimaryButton("Boshlash", 336, 318, 130);
        ok.Click += delegate
        {
            if (cb.Checked) { showWelcome = false; SaveSettings(); }
            f.Close();
        };
        c.Controls.Add(ok);
        f.AcceptButton = ok;

        c.Controls.Add(Lbl("© " + AppAuthor + ". Mualliflik huquqi himoyalangan.   " +
                           "Maxfiylik: yozganlaringiz saqlanmaydi.",
                           0, 360, 470, 20, 8F, FontStyle.Regular, Muted));

        f.Show(); f.Activate();
    }

    // ============================================================
    //  SOZLAMALAR OYNASI
    // ============================================================
    static Form settingsForm;

    static void ShowSettings()
    {
        if (settingsForm != null && !settingsForm.IsDisposed)
        { settingsForm.Activate(); return; }

        Form f = BrandWindow("Sozlamalar", 460, 552);
        settingsForm = f;

        Panel c = new Panel();
        c.Dock = DockStyle.Fill;
        c.Padding = new Padding(24, 14, 24, 14);
        f.Controls.Add(c);
        c.BringToFront();

        int y = 6;
        c.Controls.Add(Lbl("ALMASHTIRISH QOIDALARI", 0, y, 400, 20, 8.5F, FontStyle.Bold, AccentText)); y += 26;

        CheckBox cbOG = Chk("o'  →  ö        va        g'  →  ğ", ruleOG, y); c.Controls.Add(cbOG); y += 28;
        CheckBox cbSh = Chk("sh  →  ş        va        ch  →  ç", ruleShCh, y); c.Controls.Add(cbSh); y += 38;

        c.Controls.Add(Lbl("ISHGA TUSHISH", 0, y, 400, 20, 8.5F, FontStyle.Bold, AccentText)); y += 26;

        CheckBox cbAuto = Chk("Kompyuter yonganda o'zi ishga tushsin", IsAutoStart(), y); c.Controls.Add(cbAuto); y += 28;
        CheckBox cbOn   = Chk("Ochilishi bilan darhol YOQILGAN bo'lsin", startEnabled, y); c.Controls.Add(cbOn); y += 38;

        c.Controls.Add(Lbl("KO'RINISH", 0, y, 400, 20, 8.5F, FontStyle.Bold, AccentText)); y += 26;

        CheckBox cbToast = Chk("Yoqilganda ekranda bildirishnoma chiqsin", showToast, y); c.Controls.Add(cbToast); y += 28;
        CheckBox cbWel   = Chk("Ochilganda qo'llanma oynasi chiqsin", showWelcome, y); c.Controls.Add(cbWel); y += 36;

        c.Controls.Add(Lbl("BU DASTURLARDA ISHLAMASIN", 0, y, 400, 20, 8.5F, FontStyle.Bold, AccentText)); y += 24;
        TextBox tbEx = new TextBox();
        tbEx.Text = excludedApps;
        tbEx.SetBounds(4, y, 404, 24);
        tbEx.Font = new Font("Segoe UI", 9F); tbEx.BackColor = Surface; tbEx.ForeColor = Ink; tbEx.BorderStyle = BorderStyle.FixedSingle;
        c.Controls.Add(tbEx); y += 26;
        c.Controls.Add(Lbl("Masalan:  cmd, powershell, dota2   (vergul bilan ajrating)",
                           4, y, 404, 18, 8F, FontStyle.Regular, Muted)); y += 26;

        c.Controls.Add(Lbl("Yoqib-o'chirish tugmasi:   Ctrl + Shift", 0, y, 400, 22, 9.5F, FontStyle.Bold, Ink));

        Button ok = PrimaryButton("Saqlash", 296, 396, 112);
        ok.Click += delegate
        {
            ruleOG = cbOG.Checked; ruleShCh = cbSh.Checked;
            startEnabled = cbOn.Checked; showWelcome = cbWel.Checked;
            showToast = cbToast.Checked;
            excludedApps = tbEx.Text.Trim();
            cachedHwnd = IntPtr.Zero;          // keshni yangilaymiz
            SaveSettings();
            SetAutoStart(cbAuto.Checked);
            UpdateTray();
            f.Close();
        };
        Button no = new Button();
        no.Text = "Bekor"; no.SetBounds(196, 396, 90, 34);
        no.FlatStyle = FlatStyle.Flat; no.BackColor = Surface;
        no.FlatAppearance.BorderColor = CardBorder;
        no.ForeColor = Muted; no.Cursor = Cursors.Hand;
        no.Click += delegate { f.Close(); };
        c.Controls.Add(ok); c.Controls.Add(no);
        f.AcceptButton = ok; f.CancelButton = no;

        f.Show(); f.Activate();
    }

    static CheckBox Chk(string t, bool v, int y)
    {
        CheckBox c = new CheckBox();
        c.Text = t; c.Checked = v;
        c.SetBounds(4, y, 400, 24);
        c.ForeColor = Ink;
        c.Font = new Font("Segoe UI", 9.5F);
        return c;
    }

    // ============================================================
    //  DASTUR HAQIDA
    // ============================================================
    static void ShowAbout()
    {
        Form f = BrandWindow("Dastur haqida", 440, 490);

        Panel c = new Panel();
        c.Dock = DockStyle.Fill;
        c.Padding = new Padding(26, 18, 26, 14);
        f.Controls.Add(c);
        c.BringToFront();

        c.Controls.Add(Lbl(AppName + "  " + AppVer, 0, 4, 380, 26, 13F, FontStyle.Bold, Ink));
        c.Controls.Add(Lbl(Tagline, 0, 30, 380, 20, 9.5F, FontStyle.Regular, Muted));
        c.Controls.Add(Lbl("Muallif:  " + AppAuthor, 0, 60, 380, 22, 10F, FontStyle.Regular, Ink));
        c.Controls.Add(Lbl("© Mualliflik huquqi himoyalangan.", 0, 82, 380, 20, 9F, FontStyle.Regular, Muted));

        Panel priv = new Panel();
        priv.SetBounds(0, 114, 386, 116);
        priv.BackColor = PrivBg;
        priv.Paint += delegate (object s, PaintEventArgs e)
        {
            using (Pen p = new Pen(PrivBorder))
                e.Graphics.DrawRectangle(p, 0, 0, priv.Width - 1, priv.Height - 1);
        };
        priv.Controls.Add(Lbl("MAXFIYLIK KAFOLATI", 16, 12, 340, 20, 9F, FontStyle.Bold, PrivTitle));
        priv.Controls.Add(Lbl("•  Yozganlaringiz saqlanmaydi\n" +
                              "•  Faylga yozilmaydi\n" +
                              "•  Internetga yuborilmaydi\n" +
                              "•  Dastur internetga umuman ulanmaydi",
                              16, 34, 350, 74, 9.5F, FontStyle.Regular, PrivText));
        c.Controls.Add(priv);

        // --- Qo'llab-quvvatlash aloqasi ---
        c.Controls.Add(Lbl("YORDAM VA TAKLIFLAR", 0, 242, 380, 20, 9F, FontStyle.Bold, Brand));
        c.Controls.Add(Lbl("Muammo yoki taklifingiz bo'lsa, bemalol yozing:",
                           0, 262, 386, 20, 9F, FontStyle.Regular, Muted));

        LinkLabel mail = SupportLink("✉   " + SupportEmail, 0, 286);
        mail.LinkClicked += delegate { OpenUrl("mailto:" + SupportEmail); };
        c.Controls.Add(mail);

        LinkLabel tg = SupportLink("✈   " + SupportTelegram + "   (Telegram)", 0, 310);
        tg.LinkClicked += delegate { OpenUrl("https://t.me/" + SupportTelegram.TrimStart('@')); };
        c.Controls.Add(tg);

        Button ok = PrimaryButton("Yopish", 274, 342, 112);
        ok.Click += delegate { f.Close(); };
        c.Controls.Add(ok);
        f.AcceptButton = ok;

        f.Show(); f.Activate();
    }

    static LinkLabel SupportLink(string text, int x, int y)
    {
        LinkLabel l = new LinkLabel();
        l.Text = text;
        l.SetBounds(x, y, 330, 22);
        l.Font = new Font("Segoe UI", 9.5F);
        l.LinkColor = Brand;
        l.ActiveLinkColor = BrandDark;
        l.VisitedLinkColor = Brand;
        l.LinkBehavior = LinkBehavior.HoverUnderline;
        l.BackColor = Color.Transparent;
        return l;
    }

    static void OpenUrl(string url)
    {
        try { System.Diagnostics.Process.Start(url); }
        catch
        {
            MessageBox.Show("Havolani ocha olmadim. Manzilni qo'lda nusxalang:\n\n" + url,
                            AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // ============================================================
    //  LOGOTIP
    //  Agar dastur yonida "Logo.png" bo'lsa - o'sha ishlatiladi.
    //  Bu sizga o'z logotipingizni qo'yish imkonini beradi.
    // ============================================================
    static void LoadLogo()
    {
        // 1) Dastur ichiga joylashtirilgan logotip (qurish paytida qo'shiladi)
        try
        {
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly()
                                  .GetManifestResourceStream("Logo"))
                if (s != null) { customLogo = SafeImage(s); return; }
        }
        catch { }

        // 2) Yoki dastur yonidagi Logo.png fayli
        try
        {
            string p = Path.Combine(WorkDir, "Logo.png");
            if (File.Exists(p))
                using (FileStream fs = new FileStream(p, FileMode.Open, FileAccess.Read))
                    customLogo = SafeImage(fs);
        }
        catch { customLogo = null; }
    }

    // MUHIM: Image.FromStream qaytargan rasm o'sha oqim (stream) OCHIQ
    // turishini talab qiladi. Oqim yopilsa, keyinroq GDI+ xatosi chiqadi.
    // Shuning uchun rasmning mustaqil nusxasini olamiz.
    static Image SafeImage(Stream s)
    {
        using (Image tmp = Image.FromStream(s))
        {
            Bitmap copy = new Bitmap(tmp.Width, tmp.Height,
                                     System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(copy))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(tmp, new Rectangle(0, 0, copy.Width, copy.Height));
            }
            return copy;
        }
    }

    // Logotipni chizadi: yumaloq kvadrat + "A" + urg'u nuqtasi
    public static void DrawLogo(Graphics g, Rectangle r, bool on, bool small)
    {
        // Logotip HAMMA joyda ishlatiladi. Tray'da holatni ko'rsatish uchun:
        // yoqilganda — rangli, o'chirilganda — oqargan (kulrang) logotip.
        if (customLogo != null)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if (on)
            {
                g.DrawImage(customLogo, r);
            }
            else
            {
                // Kulrang + yarim shaffof: o'chirilganligi darhol bilinadi
                float[][] m = new float[][] {
                    new float[] {0.30f, 0.30f, 0.30f, 0, 0},
                    new float[] {0.59f, 0.59f, 0.59f, 0, 0},
                    new float[] {0.11f, 0.11f, 0.11f, 0, 0},
                    new float[] {0,     0,     0,     0.55f, 0},
                    new float[] {0.12f, 0.12f, 0.12f, 0, 1}
                };
                using (System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes())
                {
                    ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(m));
                    g.DrawImage(customLogo, r, 0, 0, customLogo.Width, customLogo.Height,
                                GraphicsUnit.Pixel, ia);
                }
            }
            return;
        }

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        Color c1 = on ? BrandLight : Color.FromArgb(180, 190, 200);
        Color c2 = on ? BrandDark  : Color.FromArgb(120, 132, 148);

        int radius = Math.Max(3, r.Width / 4);
        using (GraphicsPath path = RoundRect(r, radius))
        using (LinearGradientBrush b = new LinearGradientBrush(r, c1, c2, 55f))
            g.FillPath(b, path);

        // "A" harfi
        using (Font f = new Font("Segoe UI", r.Height * 0.60f, FontStyle.Bold, GraphicsUnit.Pixel))
        using (Brush w = new SolidBrush(Color.White))
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            g.DrawString("A", f, w, new RectangleF(r.X, r.Y + r.Height * 0.04f, r.Width, r.Height), sf);
        }

        // Urg'u nuqtasi — yangi alifbo belgisi
        if (!small)
        {
            int d = Math.Max(2, r.Width / 9);
            using (Brush acc = new SolidBrush(on ? Color.FromArgb(250, 204, 21) : Color.FromArgb(200, 208, 218)))
                g.FillEllipse(acc, r.Right - d - r.Width / 8, r.Y + r.Height / 7, d, d);
        }
    }

    static GraphicsPath RoundRect(Rectangle r, int radius)
    {
        GraphicsPath p = new GraphicsPath();
        int d = radius * 2;
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    static Icon MakeIcon(bool on)
    {
        Bitmap bmp = new Bitmap(32, 32);
        using (Graphics g = Graphics.FromImage(bmp))
            DrawLogo(g, new Rectangle(0, 0, 32, 32), on, true);
        IntPtr h = bmp.GetHicon();
        bmp.Dispose();
        if (on) onHandle = h; else offHandle = h;
        return Icon.FromHandle(h);
    }

    static void FreeIcons()
    {
        if (onHandle  != IntPtr.Zero) { DestroyIcon(onHandle);  onHandle  = IntPtr.Zero; }
        if (offHandle != IntPtr.Zero) { DestroyIcon(offHandle); offHandle = IntPtr.Zero; }
    }
}
