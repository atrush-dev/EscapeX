using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("EscapeX")]
[assembly: AssemblyDescription("Трей-утилита для принудительного завершения зависших окон и быстрого сворачивания активного окна")]
[assembly: AssemblyProduct("EscapeX")]
[assembly: AssemblyCompany("Alexander Trush")]
[assembly: AssemblyCopyright("© 2026 Alexander Trush")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// =============================================================================
//  EscapeX — system tray application
//  Hold Escape 10 seconds -> kills foreground process
// =============================================================================

// ── Язык приложения ────────────────────────────────────────────────────────
enum AppLanguage { Ru, En }

// ── Локализация ────────────────────────────────────────────────────────────
static class L10n
{
    private const string REG_KEY  = @"Software\EscapeX";
    private const string REG_LANG = "Language";

    // Страны СНГ/постсовет. пространства → язык Ru по умолчанию
    private static readonly HashSet<string> RuRegions = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        "RU","BY","KZ","UA","MD","AM","AZ","GE","KG","TJ","TM","UZ"
    };

    private static AppLanguage _lang;

    // Текущий язык (read-only снаружи)
    public static AppLanguage Lang { get { return _lang; } }

    // ── Инициализация при старте ──────────────────────────────────────────
    public static void Init(string[] args = null)
    {
        if (args != null)
        {
            foreach (string a in args)
            {
                if (string.Equals(a, "--lang=ru", StringComparison.OrdinalIgnoreCase))
                {
                    _lang = AppLanguage.Ru;
                    Save(_lang);
                    return;
                }
                if (string.Equals(a, "--lang=en", StringComparison.OrdinalIgnoreCase))
                {
                    _lang = AppLanguage.En;
                    Save(_lang);
                    return;
                }
            }
        }

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY))
        {
            if (key != null)
            {
                string saved = key.GetValue(REG_LANG) as string;
                if (!string.IsNullOrEmpty(saved))
                {
                    _lang = string.Equals(saved, "ru", StringComparison.OrdinalIgnoreCase)
                            ? AppLanguage.Ru : AppLanguage.En;
                    return;
                }
            }
        }
        // Первый запуск — автоопределение
        _lang = DetectLanguage();
        Save(_lang);
    }

    private static AppLanguage DetectLanguage()
    {
        try
        {
            string region = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            if (RuRegions.Contains(region)) return AppLanguage.Ru;
        }
        catch { }
        try
        {
            string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (string.Equals(culture, "ru", StringComparison.OrdinalIgnoreCase))
                return AppLanguage.Ru;
        }
        catch { }
        return AppLanguage.En;
    }

    // ── Переключение ──────────────────────────────────────────────────────
    public static void Toggle()
    {
        _lang = (_lang == AppLanguage.Ru) ? AppLanguage.En : AppLanguage.Ru;
        Save(_lang);
    }

    private static void Save(AppLanguage lang)
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REG_KEY))
                key.SetValue(REG_LANG, lang == AppLanguage.Ru ? "ru" : "en",
                             RegistryValueKind.String);
        }
        catch { }
    }

    // ── Строки ────────────────────────────────────────────────────────────
    private static readonly Dictionary<string, string> Ru = new Dictionary<string, string>
    {
        // Иконка в трее
        { "tray.tip",            "EscapeX" },

        // Шапка-подсказка в меню
        { "hint.text",           "Зажми Escape на 10 сек — закроется зависшее окно" },

        // Пункты меню
        { "menu.startup",        "Запускать при старте Windows" },
        { "menu.hook",           "Активна функция Escape-Kill" },
        { "menu.stash",          "Активна функция Escape-Stash (Escape+X)" },
        { "tooltip.hook",        "Escape-Kill\n\nПринудительно закрывает зависшие окна и процессы. Зажми Escape и удерживай 10 секунд, если игра или программа не отвечает — активное окно будет закрыто." },
        { "tooltip.stash",       "Escape-Stash\n\nБыстро сворачивает активное окно в трей. Нажми одновременно Escape и X (в любом порядке) — окно свернётся почти мгновенно. Полезно, если игра или программа не сворачивается обычным способом (например, старые игры на весь экран)." },
        { "menu.showlog",        "Показать лог автозагрузки" },
        { "menu.about",          "О программе" },
        { "menu.exit",           "Выход" },

        // О программе
        { "about.text",          "EscapeX 1.0 (28.08.2026)\n\nЛёгкая утилита в трее для принудительного управления зависшими окнами и играми.\n\nEscape-Kill — зажми Escape и удерживай 10 секунд, чтобы закрыть зависшее окно или процесс.\n\nEscape-Stash — нажми одновременно Escape и X, чтобы мгновенно свернуть активное окно (полезно для старых игр, которые не сворачиваются обычным способом).\n\nАвтор: Alexander Trush" },
        { "about.title",         "О программе" },
        { "about.github",        "Проект на GitHub" },

        // Установка — setup-процесс
        { "setup.ok.text",       "EscapeX успешно добавлен в автозагрузку Windows с повышенными правами.\n\nЧтобы всё заработало полностью (в том числе завершение игр, запущенных от администратора), нужно один раз перезагрузить компьютер или выйти и заново войти в систему.\n\nПосле этого EscapeX будет автоматически появляться в трее и работать в фоне — больше ничего делать не нужно." },
        { "setup.ok.title",      "EscapeX — установка завершена" },

        // Запрос повышения прав при первом старте
        { "uac.text",            "EscapeX\n\nЧтобы функции Escape-Kill и Escape-Stash работали со всеми программами и играми (включая те, что сами запущены от администратора), EscapeX нужно один раз получить права администратора.\n\nНажмите ОК — появится системный запрос Windows на повышение прав. Разрешите его, а затем один раз перезагрузите компьютер или выйдите и заново войдите в систему — после этого EscapeX будет запускаться сам, в фоне, без всплывающих окон.\n\nЛибо закройте это окно и запустите EscapeX вручную через «Запуск от имени администратора»." },
        { "uac.title",           "EscapeX — нужны права администратора" },

        // Ошибки schtasks
        { "err.title",           "EscapeX — ошибка" },
        { "err.schtasks",        "Ошибка schtasks (код {0}):\n{1}" },

        // Balloon tips
        { "balloon.startup.on",  "Автозагрузка включена." },
        { "balloon.startup.off", "Автозагрузка отключена." },
        { "balloon.stash.on",    "Escape-Stash включён." },
        { "balloon.stash.off",   "Escape-Stash выключен." },

        // Лог
        { "log.empty",           "(лог пока пуст)" },
        { "log.register",        "[Register] schtasks.exe {0}" },
        { "log.unregister",      "[Unregister] schtasks.exe {0}" },
        { "log.exitcode",        "[ExitCode={0}] {1}" },
        { "log.error",           "[ERROR] {0}" },
        { "log.kill.success",    "[Kill] Завершён процесс: {0} (PID {1})" },
        { "log.kill.fail",       "[Kill] Не удалось завершить PID {0}: {1}" },
        { "log.stash.success",   "[Stash] Свёрнуто окно: {0} (HWND {1})" },
        { "log.exception",       "[EXCEPTION] {0}" },
        { "badge.log.on",        "Вкл" },
        { "badge.log.off",       "Выкл" },
    };

    private static readonly Dictionary<string, string> En = new Dictionary<string, string>
    {
        { "tray.tip",            "EscapeX" },

        { "hint.text",           "Hold Escape 10 sec — kills the frozen window" },

        { "menu.startup",        "Run at Windows startup" },
        { "menu.hook",           "Escape-Kill feature enabled" },
        { "menu.stash",          "Escape-Stash feature enabled (Escape+X)" },
        { "tooltip.hook",        "Escape-Kill\n\nForce-closes frozen windows and processes. Hold Escape for 10 seconds if a game or app stops responding — the active window will be closed." },
        { "tooltip.stash",       "Escape-Stash\n\nQuickly minimizes the active window. Press Escape and X together (in any order) — the window minimizes almost instantly. Useful when a game or app won't minimize the normal way (e.g. older fullscreen games)." },
        { "menu.showlog",        "Show startup log" },
        { "menu.about",          "About" },
        { "menu.exit",           "Exit" },

        { "about.text",          "EscapeX 1.0 (Aug 28, 2026)\n\nA lightweight tray utility for force-managing frozen windows and games.\n\nEscape-Kill — hold Escape for 10 seconds to close a frozen window or process.\n\nEscape-Stash — press Escape and X together to instantly minimize the active window (handy for older games that won't minimize normally).\n\nAuthor: Alexander Trush" },
        { "about.title",         "About EscapeX" },
        { "about.github",        "Project on GitHub" },

        { "setup.ok.text",       "EscapeX was successfully added to Windows startup with elevated privileges.\n\nFor full functionality (including terminating games that run as Administrator), please restart your PC or sign out and back in once.\n\nAfter that, EscapeX will appear in the tray automatically — nothing else to do." },
        { "setup.ok.title",      "EscapeX — setup complete" },

        { "uac.text",            "EscapeX\n\nFor Escape-Kill and Escape-Stash to work with all apps and games — including ones already running as Administrator — EscapeX needs Administrator rights once.\n\nClick OK — Windows will show its own elevation prompt. Approve it, then restart your PC once (or sign out and back in) — after that, EscapeX will launch automatically in the background, no more prompts.\n\nOr close this window and run EscapeX manually via \"Run as administrator.\"" },
        { "uac.title",           "EscapeX — Administrator rights required" },

        { "err.title",           "EscapeX — error" },
        { "err.schtasks",        "schtasks error (code {0}):\n{1}" },

        { "balloon.startup.on",  "Startup enabled." },
        { "balloon.startup.off", "Startup disabled." },
        { "balloon.stash.on",    "Escape-Stash enabled." },
        { "balloon.stash.off",   "Escape-Stash disabled." },

        { "log.empty",           "(log is empty)" },
        { "log.register",        "[Register] schtasks.exe {0}" },
        { "log.unregister",      "[Unregister] schtasks.exe {0}" },
        { "log.exitcode",        "[ExitCode={0}] {1}" },
        { "log.error",           "[ERROR] {0}" },
        { "log.kill.success",    "[Kill] Terminated process: {0} (PID {1})" },
        { "log.kill.fail",       "[Kill] Failed to terminate PID {0}: {1}" },
        { "log.stash.success",   "[Stash] Minimized window: {0} (HWND {1})" },
        { "log.exception",       "[EXCEPTION] {0}" },
        { "badge.log.on",        "On" },
        { "badge.log.off",       "Off" },
    };

    public static string T(string key)
    {
        Dictionary<string, string> d = (_lang == AppLanguage.Ru) ? Ru : En;
        string val;
        return d.TryGetValue(key, out val) ? val : key;
    }

    // Получить строку для конкретного языка (без учёта текущего)
    public static string GetRaw(string key, AppLanguage lang)
    {
        Dictionary<string, string> d = (lang == AppLanguage.Ru) ? Ru : En;
        string val;
        return d.TryGetValue(key, out val) ? val : key;
    }
}

// =============================================================================
//  Program entry point
// =============================================================================

static class Program
{
    private const string ARG_SETUP   = "--setup";
    private const string ARG_UNSETUP = "--unsetup";

    internal static readonly string LogPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "escapex_log.txt");

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Язык определяем до любых диалогов (с учётом аргументов командной строки)
        L10n.Init(args);

        Scheduler.LogEnabled = Scheduler.LoadLogEnabled();
        if (args != null)
        {
            foreach (string a in args)
            {
                if (string.Equals(a, "--log", StringComparison.OrdinalIgnoreCase))
                    Scheduler.LogEnabled = true;
            }
        }

        bool isSetup   = false;
        bool isUnsetup = false;
        if (args != null)
        {
            foreach (string a in args)
            {
                if (string.Equals(a, ARG_SETUP, StringComparison.OrdinalIgnoreCase)) isSetup = true;
                if (string.Equals(a, ARG_UNSETUP, StringComparison.OrdinalIgnoreCase)) isUnsetup = true;
            }
        }

        if (isSetup)
        {
            Scheduler.Register(null);
            return;
        }
        if (isUnsetup)
        {
            Scheduler.Unregister(null);
            return;
        }

        if (!IsAdmin() && !Scheduler.TaskExists())
        {
            DialogResult dr = MessageBox.Show(
                L10n.T("uac.text"),
                L10n.T("uac.title"),
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (dr == DialogResult.OK)
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName        = Application.ExecutablePath;
                    psi.Arguments       = ARG_SETUP + " --lang=" + (L10n.Lang == AppLanguage.Ru ? "ru" : "en");
                    psi.UseShellExecute = true;
                    psi.Verb            = "runas";
                    Process.Start(psi);
                }
                catch { }
                return;
            }
        }

        Application.Run(new TrayApp());
    }

    internal static bool IsAdmin()
    {
        WindowsIdentity  id = WindowsIdentity.GetCurrent();
        WindowsPrincipal wp = new WindowsPrincipal(id);
        return wp.IsInRole(WindowsBuiltInRole.Administrator);
    }
}

// =============================================================================
//  Scheduler — Task Scheduler wrapper
// =============================================================================

static class Scheduler
{
    private const string TASK_NAME     = "EscapeX";
    private const string OLD_TASK_NAME = "EscapeKill";

    public static bool TaskExists()
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe",
                "/query /tn \"" + TASK_NAME + "\"");
            psi.UseShellExecute        = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError  = true;
            psi.CreateNoWindow         = true;
            Process p = Process.Start(psi);
            p.WaitForExit();
            return p.ExitCode == 0;
        }
        catch { return false; }
    }

    public static bool Register(NotifyIcon notifyIcon)
    {
        // Тихо удаляем старую задачу EscapeKill
        try
        {
            ProcessStartInfo del = new ProcessStartInfo("schtasks.exe",
                "/delete /f /tn \"" + OLD_TASK_NAME + "\"");
            del.UseShellExecute        = false;
            del.RedirectStandardOutput = true;
            del.RedirectStandardError  = true;
            del.CreateNoWindow         = true;
            Process.Start(del).WaitForExit();
        }
        catch { }

        string exePath = Application.ExecutablePath;
        string args =
            "/create /f" +
            " /tn \"" + TASK_NAME + "\"" +
            " /tr \"\\\"" + exePath + "\\\"\"" +
            " /sc onlogon" +
            " /rl highest";

        Log(string.Format(L10n.T("log.register"), args));
        return RunSchtasks(args, notifyIcon, L10n.T("balloon.startup.on"));
    }

    public static bool Unregister(NotifyIcon notifyIcon)
    {
        if (!TaskExists())
        {
            // Задачи и так нет в Планировщике — удалять нечего, 
            // это не ошибка, цель уже достигнута.
            if (notifyIcon != null)
                notifyIcon.ShowBalloonTip(2000, L10n.T("tray.tip"),
                    L10n.T("balloon.startup.off"), ToolTipIcon.Info);
            return true;
        }

        string args = "/delete /f /tn \"" + TASK_NAME + "\"";
        Log(string.Format(L10n.T("log.unregister"), args));
        return RunSchtasks(args, notifyIcon, L10n.T("balloon.startup.off"));
    }

    private static bool RunSchtasks(string args, NotifyIcon notifyIcon, string successMsg)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe", args);
            psi.UseShellExecute        = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError  = true;
            psi.CreateNoWindow         = true;
            psi.StandardOutputEncoding = System.Text.Encoding.GetEncoding(866);
            psi.StandardErrorEncoding  = System.Text.Encoding.GetEncoding(866);

            Process p = Process.Start(psi);
            StringBuilder sb = new StringBuilder();
            sb.Append(p.StandardOutput.ReadToEnd());
            sb.Append(p.StandardError.ReadToEnd());
            p.WaitForExit();

            string output = sb.ToString().Trim();
            if (L10n.Lang == AppLanguage.En && p.ExitCode == 0)
            {
                string enOutput = args.Contains("/delete")
                    ? "SUCCESS: The scheduled task \"EscapeX\" was successfully deleted."
                    : "SUCCESS: The scheduled task \"EscapeX\" was successfully created.";
                Log(string.Format(L10n.T("log.exitcode"), p.ExitCode, enOutput));
            }
            else
            {
                Log(string.Format(L10n.T("log.exitcode"), p.ExitCode, output));
            }

            if (p.ExitCode != 0)
            {
                string errMsg = string.Format(L10n.T("err.schtasks"), p.ExitCode, output);
                Log(string.Format(L10n.T("log.error"), errMsg));

                if (notifyIcon != null)
                    notifyIcon.ShowBalloonTip(6000, L10n.T("err.title"), errMsg, ToolTipIcon.Error);
                else
                    MessageBox.Show(errMsg, L10n.T("err.title"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (notifyIcon != null && !string.IsNullOrEmpty(successMsg))
                notifyIcon.ShowBalloonTip(2000, L10n.T("tray.tip"), successMsg, ToolTipIcon.Info);

            return true;
        }
        catch (Exception ex)
        {
            Log(string.Format(L10n.T("log.exception"), ex.Message));
            if (notifyIcon != null)
                notifyIcon.ShowBalloonTip(6000, L10n.T("err.title"), ex.Message, ToolTipIcon.Error);
            return false;
        }
    }

    private const string REG_KEY = @"Software\EscapeX";
    private const string REG_LOG = "LogEnabled";

    public static bool LogEnabled = false;

    public static bool LoadLogEnabled()
    {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY);
        if (key == null) return false;
        using (key)
        {
            object val = key.GetValue(REG_LOG, 0);
            return (int)val != 0;
        }
    }

    public static void SaveLogEnabled(bool value)
    {
        LogEnabled = value;
        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REG_KEY))
                key.SetValue(REG_LOG, value ? 1 : 0, RegistryValueKind.DWord);
        }
        catch { }
    }

    public static void Log(string text)
    {
        if (!LogEnabled) return;
        try
        {
            File.AppendAllText(Program.LogPath,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + text + "\r\n");
        }
        catch { }
    }
}

// =============================================================================
//  Dark green menu theme
// =============================================================================

sealed class DarkColorTable : ProfessionalColorTable
{
    private static readonly Color BgMenu        = Color.FromArgb(18,  18,  18);
    private static readonly Color BgHover       = Color.FromArgb(42,  42,  42);
    private static readonly Color NeonGreen      = Color.FromArgb(57,  255, 20);
    private static readonly Color BorderGray     = Color.FromArgb(70,  70,  70);
    private static readonly Color SeparatorColor = Color.FromArgb(55,  55,  55);

    public override Color ToolStripDropDownBackground  { get { return BgMenu; } }
    public override Color ImageMarginGradientBegin     { get { return BgMenu; } }
    public override Color ImageMarginGradientMiddle    { get { return BgMenu; } }
    public override Color ImageMarginGradientEnd       { get { return BgMenu; } }
    public override Color MenuBorder                   { get { return BorderGray; } }
    public override Color MenuItemBorder               { get { return BorderGray; } }
    public override Color MenuItemSelected             { get { return BgHover; } }
    public override Color MenuItemSelectedGradientBegin{ get { return BgHover; } }
    public override Color MenuItemSelectedGradientEnd  { get { return BgHover; } }
    public override Color SeparatorDark                { get { return SeparatorColor; } }
    public override Color SeparatorLight               { get { return SeparatorColor; } }
    public override Color MenuItemPressedGradientBegin { get { return BgHover; } }
    public override Color MenuItemPressedGradientEnd   { get { return BgHover; } }
    public override Color MenuItemPressedGradientMiddle{ get { return BgHover; } }
    public override Color MenuStripGradientBegin       { get { return BgMenu; } }
    public override Color MenuStripGradientEnd         { get { return BgMenu; } }
    public override Color CheckBackground              { get { return BgMenu; } }
    public override Color CheckPressedBackground       { get { return BgHover; } }
    public override Color CheckSelectedBackground      { get { return BgHover; } }

    internal static Color GetNeon()       { return NeonGreen; }
    internal static Color GetBgHover()    { return BgHover; }
    internal static Color GetBgMenu()     { return BgMenu; }
    internal static Color GetSep()        { return SeparatorColor; }
    internal static Color GetBorderGray() { return BorderGray; }
}

sealed class DarkGreenRenderer : ToolStripProfessionalRenderer
{
    private static readonly Color TextNormal   = Color.FromArgb(46, 204, 113);
    private static readonly Color TextHover    = DarkColorTable.GetNeon();
    private static readonly Color TextDisabled = Color.FromArgb(90, 130, 100);

    public DarkGreenRenderer() : base(new DarkColorTable()) { }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        using (SolidBrush bgBrush = new SolidBrush(DarkColorTable.GetBgMenu()))
            e.Graphics.FillRectangle(bgBrush, new Rectangle(Point.Empty, e.Item.Size));

        if (!(e.Item.Selected || e.Item.Pressed)) return;
        if (!e.Item.Enabled) return;

        const int marginX    = 4;
        const int marginY    = 1;
        const int pillRadius = 5;

        Rectangle pill = new Rectangle(
            marginX, marginY,
            e.Item.Width  - marginX * 2,
            e.Item.Height - marginY * 2);

        if (pill.Width <= 0 || pill.Height <= 0) return;

        using (GraphicsPath path = RoundedRectPathSmall(pill, pillRadius))
        using (SolidBrush br = new SolidBrush(DarkColorTable.GetBgHover()))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(br, path);
            e.Graphics.SmoothingMode = SmoothingMode.Default;
        }

        if (e.Item.Tag as string == "exit")
            DrawLangBadge(e.Graphics, e.Item);
        else if (e.Item.Tag as string == "log")
            DrawLogBadge(e.Graphics, e.Item);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        if (!e.Item.Enabled)
        {
            e.TextColor = TextDisabled;
            base.OnRenderItemText(e);
            return;
        }

        ToolStripMenuItem mi = e.Item as ToolStripMenuItem;
        bool isChecked = (mi != null && mi.Checked);
        bool isHovered = (e.Item.Selected || e.Item.Pressed);

        e.TextColor = (isChecked || isHovered) ? TextHover : TextNormal;
        base.OnRenderItemText(e);

        if (!(e.Item.Selected || e.Item.Pressed))
        {
            if (e.Item.Tag as string == "exit")
                DrawLangBadge(e.Graphics, e.Item);
            else if (e.Item.Tag as string == "log")
                DrawLogBadge(e.Graphics, e.Item);
        }
    }

    private static void DrawLangBadge(Graphics g, ToolStripItem item)
    {
        Rectangle rc = new Rectangle(item.Width - 36, (item.Height - 16) / 2, 26, 16);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (GraphicsPath bg = RoundedRectPathSmall(rc, 3))
        using (SolidBrush br = new SolidBrush(Color.FromArgb(30, 30, 30)))
            g.FillPath(br, bg);

        using (GraphicsPath gp = RoundedRectPathSmall(rc, 3))
        using (Pen p = new Pen(Color.FromArgb(90, 90, 90), 1f))
            g.DrawPath(p, gp);

        string txt = L10n.Lang == AppLanguage.Ru ? "EN" : "RU";
        using (Font f = new Font("Segoe UI", 7.5f, FontStyle.Bold))
        {
            SizeF ts = g.MeasureString(txt, f);
            float tx = rc.X + (rc.Width  - ts.Width)  / 2f;
            float ty = rc.Y + (rc.Height - ts.Height) / 2f;
            using (SolidBrush tb = new SolidBrush(Color.FromArgb(224, 224, 224)))
                g.DrawString(txt, f, tb, tx, ty);
        }
        g.SmoothingMode = SmoothingMode.Default;
    }

    private static void DrawLogBadge(Graphics g, ToolStripItem item)
    {
        Rectangle rc = new Rectangle(item.Width - 44, (item.Height - 16) / 2, 34, 16);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        bool enabled = Scheduler.LogEnabled;
        Color bgCol     = Color.FromArgb(30, 30, 30);
        Color borderCol = Color.FromArgb(90, 90, 90);
        Color textCol   = enabled ? Color.FromArgb(57, 255, 20) : Color.FromArgb(224, 224, 224);

        using (GraphicsPath bg = RoundedRectPathSmall(rc, 3))
        using (SolidBrush br = new SolidBrush(bgCol))
            g.FillPath(br, bg);

        using (GraphicsPath gp = RoundedRectPathSmall(rc, 3))
        using (Pen p = new Pen(borderCol, 1f))
            g.DrawPath(p, gp);

        string txt = enabled ? L10n.T("badge.log.on") : L10n.T("badge.log.off");
        using (Font f = new Font("Segoe UI", 7.5f, FontStyle.Bold))
        {
            SizeF ts = g.MeasureString(txt, f);
            float tx = rc.X + (rc.Width  - ts.Width)  / 2f;
            float ty = rc.Y + (rc.Height - ts.Height) / 2f;
            using (SolidBrush tb = new SolidBrush(textCol))
                g.DrawString(txt, f, tb, tx, ty);
        }
        g.SmoothingMode = SmoothingMode.Default;
    }

    internal static GraphicsPath RoundedRectPathSmall(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        GraphicsPath path = new GraphicsPath();
        path.AddArc(bounds.X,         bounds.Y,          d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y,          d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d,   0, 90);
        path.AddArc(bounds.X,         bounds.Bottom - d, d, d,  90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using (SolidBrush br = new SolidBrush(DarkColorTable.GetBgMenu()))
            e.Graphics.FillRectangle(br, e.AffectedBounds);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        Rectangle rc = e.AffectedBounds;
        rc.Width -= 1; rc.Height -= 1;
        using (GraphicsPath path = RoundedRectPathForBorder(rc, 8))
        using (Pen pen = new Pen(DarkColorTable.GetBorderGray(), 1f))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawPath(pen, path);
            e.Graphics.SmoothingMode = SmoothingMode.Default;
        }
    }

    private static GraphicsPath RoundedRectPathForBorder(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        GraphicsPath path = new GraphicsPath();
        path.AddArc(bounds.X,           bounds.Y,            d, d, 180, 90);
        path.AddArc(bounds.Right - d,   bounds.Y,            d, d, 270, 90);
        path.AddArc(bounds.Right - d,   bounds.Bottom - d,   d, d,   0, 90);
        path.AddArc(bounds.X,           bounds.Bottom - d,   d, d,  90, 90);
        path.CloseFigure();
        return path;
    }


    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        int y = e.Item.Height / 2;
        using (Pen pen = new Pen(DarkColorTable.GetSep(), 1f))
            e.Graphics.DrawLine(pen, 4, y, e.Item.Width - 4, y);
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        Rectangle imgRect = e.ImageRectangle;
        if (imgRect.IsEmpty)
            imgRect = new Rectangle(2, (e.Item.Height - 16) / 2, 16, 16);

        using (SolidBrush bg = new SolidBrush(DarkColorTable.GetBgHover()))
            e.Graphics.FillRectangle(bg, imgRect);

        Rectangle frame = imgRect;
        frame.Width -= 1; frame.Height -= 1;
        using (Pen pen = new Pen(DarkColorTable.GetBorderGray(), 1f))
            e.Graphics.DrawRectangle(pen, frame);

        float x0 = imgRect.Left + imgRect.Width * 0.18f;
        float y0 = imgRect.Top  + imgRect.Height * 0.52f;
        float x1 = imgRect.Left + imgRect.Width * 0.42f;
        float y1 = imgRect.Top  + imgRect.Height * 0.72f;
        float x2 = imgRect.Left + imgRect.Width * 0.82f;
        float y2 = imgRect.Top  + imgRect.Height * 0.28f;

        using (Pen pen = new Pen(DarkColorTable.GetNeon(), 2f))
        {
            pen.StartCap = LineCap.Round;
            pen.EndCap   = LineCap.Round;
            pen.LineJoin = LineJoin.Round;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawLines(pen, new PointF[] {
                new PointF(x0, y0), new PointF(x1, y1), new PointF(x2, y2) });
            e.Graphics.SmoothingMode = SmoothingMode.Default;
        }
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        using (SolidBrush br = new SolidBrush(DarkColorTable.GetBgMenu()))
            e.Graphics.FillRectangle(br, e.AffectedBounds);
    }
}


// =============================================================================
//  TrayApp — main application context
// =============================================================================

sealed class TrayApp : ApplicationContext
{
    // ── Win32 ──────────────────────────────────────────────────────────────
    private const int  WH_KEYBOARD_LL = 13;
    private const uint WM_KEYDOWN     = 0x0100;
    private const uint WM_SYSKEYDOWN  = 0x0104;
    private const uint WM_KEYUP       = 0x0101;
    private const uint WM_SYSKEYUP    = 0x0105;
    private const uint VK_ESCAPE      = 0x1B;
    private const uint KF_REPEAT      = 0x4000;

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint   vkCode;
        public uint   scanCode;
        public uint   flags;
        public uint   time;
        public IntPtr dwExtraInfo;
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
                                                   IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                                                 IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    private const int SW_FORCEMINIMIZE = 11;
    private const uint VK_X = 0x58;

    // ── Константы ──────────────────────────────────────────────────────────
    private const string REG_KEY      = @"Software\EscapeX";
    private const string REG_HOOK     = "HookEnabled";
    private const int    HOLD_SECONDS = 10;
    private const int    TIMER_PERIOD = 250;
    private const string REG_STASH    = "StashEnabled";
    private const int    X_COMBO_DELAY_MS = 150;

    // ── UI-поля ───────────────────────────────────────────────────────────
    private readonly NotifyIcon        _trayIcon;
    private readonly ToolStripMenuItem _miStartup;
    private readonly ToolStripMenuItem _miHook;
    private readonly ToolStripMenuItem _miStash;
    private readonly ToolStripMenuItem _miShowLog;
    private readonly ToolStripMenuItem _miAbout;
    private readonly ToolStripMenuItem _miExit;

    private ToolTip                     _menuToolTip;
    private System.Windows.Forms.Timer _tipTimer;
    private ToolStripMenuItem           _tipHoveredItem;
    private string                      _activeTipText = "";

    // Шапка (только hint — без бейджа)
    private readonly Label                _hintLabel;
    private readonly Panel                _hintPanel;
    private readonly ToolStripControlHost _hintHost;

    // ── Хук ───────────────────────────────────────────────────────────────
    private IntPtr               _hookHandle = IntPtr.Zero;
    private LowLevelKeyboardProc _hookProc;

    // ── Состояние удержания ────────────────────────────────────────────────
    private readonly object _lock       = new object();
    private bool     _pressing          = false;
    private DateTime _pressStart        = DateTime.MinValue;
    private IntPtr   _pressHwnd         = IntPtr.Zero;
    private bool     _escDownForCombo   = false;
    private bool     _xDown             = false;
    private bool     _xComboFired       = false;

    private System.Threading.Timer _checkTimer;
    private System.Threading.Timer _xComboTimer;

    // ── Конструктор ────────────────────────────────────────────────────────
    public TrayApp()
    {
        // ── Шапка: только Label с подсказкой (без бейджа) ─────────────────
        _hintLabel           = new Label();
        _hintLabel.Text      = L10n.T("hint.text");
        _hintLabel.ForeColor = Color.FromArgb(90, 130, 100);
        _hintLabel.BackColor = Color.FromArgb(18, 18, 18);
        _hintLabel.AutoSize  = false;
        _hintLabel.Dock      = DockStyle.Fill;
        _hintLabel.TextAlign = ContentAlignment.MiddleCenter;
        _hintLabel.Padding   = new Padding(0);
        _hintLabel.Font      = new Font("Segoe UI", 8f);

        _hintPanel           = new Panel();
        _hintPanel.BackColor = Color.FromArgb(18, 18, 18);
        _hintPanel.Height    = 20;
        _hintPanel.Padding   = new Padding(0);
        _hintPanel.Controls.Add(_hintLabel);

        _hintHost          = new ToolStripControlHost(_hintPanel);
        _hintHost.Padding  = new Padding(0);
        _hintHost.Margin   = new Padding(0, 1, 0, 1);
        _hintHost.AutoSize = false;

        // ── Пункты меню ───────────────────────────────────────────────────
        _miStartup = new ToolStripMenuItem(L10n.T("menu.startup"));
        _miStartup.CheckOnClick = true;
        _miStartup.Checked      = Scheduler.TaskExists();
        _miStartup.CheckedChanged += OnStartupChanged;

        _miHook = new ToolStripMenuItem(L10n.T("menu.hook"));
        _miHook.CheckOnClick = true;
        _miHook.Checked      = LoadHookEnabled();
        _miHook.CheckedChanged += OnHookEnabledChanged;

        _miStash = new ToolStripMenuItem(L10n.T("menu.stash"));
        _miStash.CheckOnClick    = true;
        _miStash.Checked         = LoadStashEnabled();
        _miStash.CheckedChanged += OnStashEnabledChanged;

        _miShowLog = new ToolStripMenuItem(L10n.T("menu.showlog"));
        _miShowLog.Tag = "log";

        _miAbout = new ToolStripMenuItem(L10n.T("menu.about"));
        _miAbout.Click += delegate { ShowAbout(); };

        _miExit = new ToolStripMenuItem(L10n.T("menu.exit"));
        _miExit.Tag = "exit";

        // ── Контекстное меню ──────────────────────────────────────────────
        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Renderer        = new DarkGreenRenderer();
        menu.BackColor       = Color.FromArgb(18, 18, 18);
        menu.ForeColor       = Color.FromArgb(57, 255, 20);
        menu.ShowImageMargin = true;
        menu.Padding         = new Padding(0, 3, 0, 3);
        menu.VisibleChanged += delegate {
            if (menu.Visible)
                ApplyRoundedRegion(menu, 8);
        };

        _menuToolTip = CreateDarkToolTip();
        _tipTimer    = new System.Windows.Forms.Timer();
        _tipTimer.Interval = 1200;
        _tipTimer.Tick += delegate
        {
            _tipTimer.Stop();
            if (_tipHoveredItem != null && menu.Visible)
            {
                Point pt = new Point(_tipHoveredItem.Bounds.Right + 4, _tipHoveredItem.Bounds.Top);
                _menuToolTip.Show(_activeTipText, menu, pt, 12000);
            }
        };

        _miHook.MouseEnter  += delegate { ArmItemTooltip(_miHook, L10n.T("tooltip.hook")); };
        _miHook.MouseLeave  += delegate { HideItemTooltip(menu); };
        _miStash.MouseEnter += delegate { ArmItemTooltip(_miStash, L10n.T("tooltip.stash")); };
        _miStash.MouseLeave += delegate { HideItemTooltip(menu); };

        bool isBadgeClick    = false;
        bool isLogBadgeClick = false;

        menu.Closing += delegate(object sender, ToolStripDropDownClosingEventArgs e)
        {
            HideItemTooltip(menu);
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                if (isBadgeClick || isLogBadgeClick ||
                    _miStartup.Selected || _miHook.Selected || _miStash.Selected ||
                    _miStartup.Pressed  || _miHook.Pressed  || _miStash.Pressed)
                {
                    e.Cancel = true;
                }
            }
        };

        _miShowLog.MouseDown += delegate(object sender, MouseEventArgs e)
        {
            Rectangle badgeRect = new Rectangle(_miShowLog.Width - 44, (_miShowLog.Height - 16) / 2, 34, 16);
            isLogBadgeClick = badgeRect.Contains(e.Location);
            if (isLogBadgeClick)
            {
                Scheduler.SaveLogEnabled(!Scheduler.LogEnabled);
                menu.Invalidate();
            }
        };

        _miShowLog.Click += delegate(object sender, EventArgs e)
        {
            if (isLogBadgeClick)
            {
                isLogBadgeClick = false;
                return;
            }
            OpenLog();
        };

        _miExit.MouseDown += delegate(object sender, MouseEventArgs e)
        {
            Rectangle badgeRect = new Rectangle(_miExit.Width - 36, (_miExit.Height - 16) / 2, 26, 16);
            isBadgeClick = badgeRect.Contains(e.Location);
            if (isBadgeClick)
            {
                OnLangSwitched(this, EventArgs.Empty);
            }
        };

        _miExit.Click += delegate(object sender, EventArgs e)
        {
            if (isBadgeClick)
            {
                isBadgeClick = false;
                return;
            }
            ExitApp();
        };

        menu.Items.Add(_hintHost);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_miStartup);
        menu.Items.Add(_miHook);
        menu.Items.Add(_miStash);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_miShowLog);
        menu.Items.Add(_miAbout);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_miExit);

        // Вычислить ширину меню один раз
        UpdateHintHostWidth();

        // ── Иконка трея ───────────────────────────────────────────────────
        _trayIcon = new NotifyIcon();
        _trayIcon.Icon             = LoadTrayIcon();
        _trayIcon.Text             = L10n.T("tray.tip");
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.Visible          = true;

        _checkTimer = new System.Threading.Timer(OnTimerTick, null,
                                                  Timeout.Infinite, Timeout.Infinite);
        _xComboTimer = new System.Threading.Timer(OnXComboTick, null,
                                                   Timeout.Infinite, Timeout.Infinite);
        if (_miHook.Checked)
            InstallHook();
    }

    // ── Вычисление ширины hintHost ─────────────────────────────────────────

    private void UpdateHintHostWidth()
    {
        // ── Кандидаты от пунктов меню ─────────────────────────────────────
        const int extraPad = 24;
        const int minWidth = 200;

        Font menuFont = SystemFonts.MenuFont;
        ToolStripMenuItem[] items = new ToolStripMenuItem[]
        {
            _miStartup, _miHook, _miStash, _miShowLog, _miAbout, _miExit
        };

        int maxW = minWidth;
        foreach (ToolStripMenuItem mi in items)
        {
            int w = TextRenderer.MeasureText(mi.Text, menuFont).Width + extraPad;
            if (w > maxW) maxW = w;
        }

        // ── Кандидаты от hint-строки (RU и EN) ────────────────────────────
        int labelHPad = _hintLabel.Padding.Left + _hintLabel.Padding.Right + 8;

        string hintRu = L10n.GetRaw("hint.text", AppLanguage.Ru);
        string hintEn = L10n.GetRaw("hint.text", AppLanguage.En);

        int hintWRu = TextRenderer.MeasureText(hintRu, _hintLabel.Font).Width + labelHPad;
        int hintWEn = TextRenderer.MeasureText(hintEn, _hintLabel.Font).Width + labelHPad;

        if (hintWRu > maxW) maxW = hintWRu;
        if (hintWEn > maxW) maxW = hintWEn;

        _hintHost.Width   = maxW;
        _hintPanel.Width  = maxW;
    }

    // ── Переключение языка ─────────────────────────────────────────────────

    private void OnLangSwitched(object sender, EventArgs e)
    {
        L10n.Toggle();
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        _miStartup.Text  = L10n.T("menu.startup");
        _miHook.Text     = L10n.T("menu.hook");
        _miStash.Text    = L10n.T("menu.stash");
        _miShowLog.Text  = L10n.T("menu.showlog");
        _miAbout.Text    = L10n.T("menu.about");
        _miExit.Text     = L10n.T("menu.exit");
        _hintLabel.Text  = L10n.T("hint.text");
        _trayIcon.Text   = L10n.T("tray.tip");
        UpdateHintHostWidth();
        if (_trayIcon.ContextMenuStrip != null)
        {
            _trayIcon.ContextMenuStrip.Invalidate();
            if (_trayIcon.ContextMenuStrip.Visible)
                ApplyRoundedRegion(_trayIcon.ContextMenuStrip, 8);
        }
    }

    // ── Иконка трея ───────────────────────────────────────────────────────

    private static Icon LoadTrayIcon()
    {
        try
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream("tray_icon.ico"))
            {
                if (s != null)
                    return new Icon(s);
            }
        }
        catch { }
        return SystemIcons.Application;
    }

    // ── О программе ───────────────────────────────────────────────────────

    private static void ShowAbout()
    {
        using (Form form = new Form())
        {
            form.Text            = L10n.T("about.title");
            form.BackColor       = Color.FromArgb(18, 18, 18);
            form.ForeColor       = Color.FromArgb(224, 224, 224);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox     = false;
            form.MinimizeBox     = false;
            form.ShowInTaskbar   = false;
            form.ShowIcon        = false;
            form.StartPosition   = FormStartPosition.CenterScreen;
            form.ClientSize      = new Size(440, 315);

            Label lblText = new Label();
            lblText.Text      = L10n.T("about.text");
            lblText.ForeColor = Color.FromArgb(224, 224, 224);
            lblText.BackColor = Color.Transparent;
            lblText.Font      = new Font("Segoe UI", 9f);
            lblText.Location  = new Point(20, 20);
            lblText.Size      = new Size(400, 210);
            lblText.TextAlign = ContentAlignment.TopLeft;

            LinkLabel lnkGithub = new LinkLabel();
            lnkGithub.Text            = L10n.T("about.github");
            lnkGithub.Font            = new Font("Segoe UI", 9f);
            lnkGithub.LinkColor       = Color.FromArgb(46, 204, 113);
            lnkGithub.ActiveLinkColor = Color.FromArgb(57, 255, 20);
            lnkGithub.VisitedLinkColor= Color.FromArgb(46, 204, 113);
            lnkGithub.LinkBehavior    = LinkBehavior.HoverUnderline;
            lnkGithub.AutoSize        = true;
            lnkGithub.Location        = new Point(20, 240);

            Color linkNormal = Color.FromArgb(46, 204, 113);
            Color linkHover  = Color.FromArgb(57, 255, 20);

            lnkGithub.MouseEnter += delegate { lnkGithub.LinkColor = linkHover; };
            lnkGithub.MouseLeave += delegate { lnkGithub.LinkColor = linkNormal; };

            lnkGithub.LinkClicked += delegate
            {
                try
                {
                    Process.Start(new ProcessStartInfo("https://github.com") { UseShellExecute = true });
                }
                catch { }
            };

            Button btnOk = new Button();
            btnOk.Text      = "OK";
            btnOk.Font      = new Font("Segoe UI", 9f);
            btnOk.Size      = new Size(80, 28);
            btnOk.Location  = new Point(340, 270);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.BackColor = Color.FromArgb(30, 30, 30);
            btnOk.ForeColor = Color.FromArgb(224, 224, 224);
            btnOk.FlatAppearance.BorderColor         = Color.FromArgb(70, 70, 70);
            btnOk.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 45, 45);
            btnOk.FlatAppearance.MouseDownBackColor = Color.FromArgb(25, 25, 25);

            form.AcceptButton = btnOk;
            form.CancelButton = btnOk;

            form.Controls.Add(lblText);
            form.Controls.Add(lnkGithub);
            form.Controls.Add(btnOk);

            form.ShowDialog();
        }
    }

    // ── Автозагрузка ──────────────────────────────────────────────────────

    private void OnStartupChanged(object sender, EventArgs e)
    {
        if (_miStartup.Checked)
        {
            if (!Program.IsAdmin())
            {
                ElevateAndRun("--setup");
                RevertCheckbox(false);
                return;
            }
            bool ok = Scheduler.Register(_trayIcon);
            if (!ok) RevertCheckbox(false);
        }
        else
        {
            if (!Program.IsAdmin())
            {
                ElevateAndRun("--unsetup");
                RevertCheckbox(true);
                return;
            }
            bool ok = Scheduler.Unregister(_trayIcon);
            if (!ok) RevertCheckbox(true);
        }
    }

    private void ElevateAndRun(string arg)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName        = Application.ExecutablePath;
            psi.Arguments       = arg + " --lang=" + (L10n.Lang == AppLanguage.Ru ? "ru" : "en") + (Scheduler.LogEnabled ? " --log" : "");
            psi.UseShellExecute = true;
            psi.Verb            = "runas";
            Process.Start(psi);
        }
        catch { }
    }

    private void RevertCheckbox(bool value)
    {
        _miStartup.CheckedChanged -= OnStartupChanged;
        _miStartup.Checked         = value;
        _miStartup.CheckedChanged += OnStartupChanged;
    }

    // ── Лог ───────────────────────────────────────────────────────────────

    private static void OpenLog()
    {
        try
        {
            if (!File.Exists(Program.LogPath))
                File.WriteAllText(Program.LogPath, L10n.T("log.empty") + "\r\n");
            Process.Start("notepad.exe", Program.LogPath);
        }
        catch { }
    }

    // ── Хук ───────────────────────────────────────────────────────────────

    private void InstallHook()
    {
        if (_hookHandle != IntPtr.Zero) return;
        _hookProc = HookCallback;
        Process       curProc = Process.GetCurrentProcess();
        ProcessModule mod     = curProc.MainModule;
        _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc,
                                        GetModuleHandle(mod.ModuleName), 0);
        curProc.Dispose();
    }

    private void UninstallHook()
    {
        if (_hookHandle == IntPtr.Zero) return;
        UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
        ResetPress();
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            uint msg = (uint)wParam.ToInt64();

            if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
            {
                KBDLLHOOKSTRUCT kb = (KBDLLHOOKSTRUCT)
                    Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                bool isRepeat = (kb.flags & KF_REPEAT) != 0;

                if (kb.vkCode == VK_X && !isRepeat)
                {
                    _xDown = true;
                    TryArmXCombo();
                }

                if (kb.vkCode == VK_ESCAPE)
                {
                    if (!isRepeat)
                    {
                        _escDownForCombo = true;
                        TryArmXCombo();

                        IntPtr hwnd = GetForegroundWindow();
                        lock (_lock)
                        {
                            if (!_pressing)
                            {
                                _pressing   = true;
                                _pressStart = DateTime.UtcNow;
                                _pressHwnd  = hwnd;
                                _checkTimer.Change(TIMER_PERIOD, TIMER_PERIOD);
                            }
                        }
                    }
                }
            }
            else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
            {
                KBDLLHOOKSTRUCT kb = (KBDLLHOOKSTRUCT)
                    Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                if (kb.vkCode == VK_X)
                {
                    _xDown       = false;
                    _xComboFired = false;
                    _xComboTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                if (kb.vkCode == VK_ESCAPE)
                {
                    _escDownForCombo = false;
                    _xComboFired     = false;
                    _xComboTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    ResetPress();
                }
            }
        }

        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    // ── Таймер ────────────────────────────────────────────────────────────

    private void OnTimerTick(object state)
    {
        IntPtr   hwnd;
        DateTime start;

        lock (_lock)
        {
            if (!_pressing) return;
            hwnd  = _pressHwnd;
            start = _pressStart;
        }

        if ((DateTime.UtcNow - start).TotalSeconds < HOLD_SECONDS)
            return;

        ResetPress();
        TryKillWindow(hwnd);
    }

    private void TryArmXCombo()
    {
        if (!_miStash.Checked) return;
        if (_xComboFired) return;
        if (!(_escDownForCombo && _xDown)) return;

        _xComboTimer.Change(X_COMBO_DELAY_MS, Timeout.Infinite);
    }

    private void OnXComboTick(object state)
    {
        if (_xComboFired) return;
        if (!(_escDownForCombo && _xDown)) return; // отпустили раньше срока — не срабатываем

        _xComboFired = true;
        IntPtr hwnd = GetForegroundWindow();
        ForceMinimizeWindow(hwnd);
    }

    private void ResetPress()
    {
        lock (_lock)
        {
            _pressing   = false;
            _pressHwnd  = IntPtr.Zero;
            _pressStart = DateTime.MinValue;
        }
        _checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private static readonly HashSet<string> ProtectedProcesses = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        "explorer", "svchost", "csrss",
        "wininit", "services", "System", "smss",
        "StartMenuExperienceHost", "ShellExperienceHost"
    };

    private static void TryKillWindow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return;

        uint pid;
        GetWindowThreadProcessId(hwnd, out pid);
        if (pid == 0) return;

        int selfPid = Process.GetCurrentProcess().Id;
        if ((int)pid == selfPid) return;

        try
        {
            Process proc = Process.GetProcessById((int)pid);
            if (ProtectedProcesses.Contains(proc.ProcessName))
                return;

            Scheduler.Log(string.Format(L10n.T("log.kill.success"), proc.ProcessName, pid));
            proc.Kill();
        }
        catch (Exception ex)
        {
            Scheduler.Log(string.Format(L10n.T("log.kill.fail"), pid, ex.Message));
        }
    }

    private static void ForceMinimizeWindow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return;

        int selfPid = Process.GetCurrentProcess().Id;
        uint pid;
        GetWindowThreadProcessId(hwnd, out pid);
        if (pid == 0 || (int)pid == selfPid) return;

        string procName = "?";
        try
        {
            using (Process proc = Process.GetProcessById((int)pid))
            {
                procName = proc.ProcessName;
                if (ProtectedProcesses.Contains(procName))
                    return;
            }
        }
        catch { }

        ShowWindowAsync(hwnd, SW_FORCEMINIMIZE);
        Scheduler.Log(string.Format(L10n.T("log.stash.success"), procName, hwnd));
    }

    // ── Реестр ────────────────────────────────────────────────────────────

    private static bool LoadHookEnabled()
    {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY);
        if (key == null) return true;
        using (key)
        {
            object val = key.GetValue(REG_HOOK, 1);
            return (int)val != 0;
        }
    }

    private static void SaveHookEnabled(bool value)
    {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REG_KEY))
            key.SetValue(REG_HOOK, value ? 1 : 0, RegistryValueKind.DWord);
    }

    private void OnHookEnabledChanged(object sender, EventArgs e)
    {
        bool enabled = _miHook.Checked;
        SaveHookEnabled(enabled);
        if (enabled) InstallHook();
        else UninstallHook();
    }

    private static bool LoadStashEnabled()
    {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY);
        if (key == null) return true;
        using (key)
        {
            object val = key.GetValue(REG_STASH, 1);
            return (int)val != 0;
        }
    }

    private static void SaveStashEnabled(bool value)
    {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REG_KEY))
            key.SetValue(REG_STASH, value ? 1 : 0, RegistryValueKind.DWord);
    }

    private void OnStashEnabledChanged(object sender, EventArgs e)
    {
        SaveStashEnabled(_miStash.Checked);
    }

    // ── Выход ─────────────────────────────────────────────────────────────

    private void ExitApp()
    {
        UninstallHook();
        if (_tipTimer != null)    _tipTimer.Dispose();
        if (_menuToolTip != null) _menuToolTip.Dispose();
        _checkTimer.Dispose();
        _xComboTimer.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UninstallHook();
            if (_tipTimer != null)    _tipTimer.Dispose();
            if (_menuToolTip != null) _menuToolTip.Dispose();
            if (_checkTimer != null) _checkTimer.Dispose();
            if (_xComboTimer != null) _xComboTimer.Dispose();
            if (_trayIcon != null)   _trayIcon.Dispose();
        }
        base.Dispose(disposing);
    }

    // ── Скруглённые углы меню ─────────────────────────────────────────────

    private static void ApplyRoundedRegion(ToolStripDropDown strip, int radius)
    {
        if (strip.Width <= 0 || strip.Height <= 0) return;
        using (GraphicsPath path = RoundedRectPath(
                   new Rectangle(0, 0, strip.Width, strip.Height), radius))
        {
            strip.Region = new Region(path);
        }
    }

    private static GraphicsPath RoundedRectPath(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        GraphicsPath path = new GraphicsPath();
        path.AddArc(bounds.X,           bounds.Y,            d, d, 180, 90);
        path.AddArc(bounds.Right - d,   bounds.Y,            d, d, 270, 90);
        path.AddArc(bounds.Right - d,   bounds.Bottom - d,   d, d,   0, 90);
        path.AddArc(bounds.X,           bounds.Bottom - d,   d, d,  90, 90);
        path.CloseFigure();
        return path;
    }

    // ── Тёмный тултип для пунктов меню ──────────────────────────────────

    private void ArmItemTooltip(ToolStripMenuItem item, string text)
    {
        _tipHoveredItem = item;
        _activeTipText  = text;
        if (_tipTimer != null)
        {
            _tipTimer.Stop();
            _tipTimer.Start();
        }
    }

    private void HideItemTooltip(ContextMenuStrip menu)
    {
        if (_tipTimer != null)
            _tipTimer.Stop();
        _tipHoveredItem = null;
        if (_menuToolTip != null && menu != null && menu.IsHandleCreated)
            _menuToolTip.Hide(menu);
    }

    private ToolTip CreateDarkToolTip()
    {
        ToolTip tip = new ToolTip();
        tip.OwnerDraw      = true;
        tip.AutomaticDelay = 0;
        tip.InitialDelay   = 1200;
        tip.ReshowDelay    = 300;
        tip.AutoPopDelay   = 12000;
        tip.ShowAlways     = true;

        tip.Draw += delegate(object sender, DrawToolTipEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(24, 24, 24)))
                e.Graphics.FillRectangle(bg, e.Bounds);
            using (Pen border = new Pen(DarkColorTable.GetBorderGray(), 1f))
                e.Graphics.DrawRectangle(border, 0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1);

            TextRenderer.DrawText(
                e.Graphics, e.ToolTipText, e.Font,
                new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 6,
                              e.Bounds.Width - 16, e.Bounds.Height - 12),
                Color.FromArgb(220, 220, 220),
                TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top);
        };

        tip.Popup += delegate(object sender, PopupEventArgs e)
        {
            using (Graphics g = e.AssociatedControl != null
                       ? e.AssociatedControl.CreateGraphics()
                       : Graphics.FromHwnd(IntPtr.Zero))
            {
                string text = !string.IsNullOrEmpty(_activeTipText)
                    ? _activeTipText
                    : tip.GetToolTip(e.AssociatedControl);
                Size textSize = TextRenderer.MeasureText(
                    g, text ?? "", SystemFonts.MenuFont, new Size(280, 0),
                    TextFormatFlags.WordBreak);
                e.ToolTipSize = new Size(textSize.Width + 20, textSize.Height + 16);
            }
        };

        return tip;
    }
}
