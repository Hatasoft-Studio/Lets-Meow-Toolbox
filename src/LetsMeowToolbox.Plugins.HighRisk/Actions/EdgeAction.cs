using Microsoft.Win32;

namespace LetsMeowToolbox.Plugins.HighRisk.Actions;

/// <summary>
/// 高危：移除 Microsoft Edge 浏览器（Edge Chromium 系统组件）。
/// 通过调用 Edge 自带 setup.exe 以 --force-uninstall 卸载，而非直接删除文件。
/// 注意：新版 Windows 11 将 Edge 作为受保护组件，官方卸载器可能拒绝卸载；本工具如实反馈结果。
/// </summary>
internal sealed class EdgeAction : PluginAbstractions.IHighRiskAction
{
    private static readonly string[] EdgeAppDirs =
    {
        @"C:\Program Files (x86)\Microsoft\Edge\Application",
        @"C:\Program Files\Microsoft\Edge\Application"
    };

    private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge";
    private const string AppPathsKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe";

    public string Id => "edge.remove";
    public string Title => "移除 Edge / WebView2 系统组件";
    public string Description =>
        "调用 Microsoft Edge 官方卸载器，以强制模式移除浏览器（不含 WebView2 运行时，以免影响其它程序）。\n" +
        "适合不希望使用 Edge、改用其他浏览器的场景。新版 Windows 11 可能将其作为受保护组件拒绝卸载。";
    public string Warning =>
        "⚠ 高危操作：将尝试移除系统级 Edge 浏览器！\n" +
        "• 新版 Windows 11 可能通过官方卸载器拒绝移除，属系统限制。\n" +
        "• 操作只调用官方卸载器，不直接删除系统文件；如需恢复请重新安装 Edge。\n" +
        "• 若操作后 Edge 仍存在，说明系统当前版本不允许卸载，请改用“设置→应用”手动尝试。";
    public string RegistryNote => "Edge Application\\<版本>\\Installer\\setup.exe --uninstall --force-uninstall";
    public bool RequiresAdmin => true;

    /// <summary>msedge.exe 是否仍存在（真实判定浏览器是否已移除）。</summary>
    private static bool EdgePresent()
    {
        foreach (var dir in EdgeAppDirs)
        {
            try
            {
                if (File.Exists(Path.Combine(dir, "msedge.exe"))) return true;
            }
            catch { /* ignore */ }
        }
        // 再看 App Paths 注册项
        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).OpenSubKey(AppPathsKey);
                if (key?.GetValue(null) is string p && !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                    return true;
            }
            catch { /* ignore */ }
        }
        return false;
    }

    /// <summary>定位官方卸载器 setup.exe（位于 Application\&lt;版本&gt;\Installer\setup.exe）。</summary>
    private static string? ResolveSetup()
    {
        // 1) 标准安装目录下找版本文件夹中的 Installer\setup.exe
        foreach (var dir in EdgeAppDirs)
        {
            try
            {
                if (!Directory.Exists(dir)) continue;
                var found = Directory.GetDirectories(dir)
                    .Select(d => Path.Combine(d, "Installer", "setup.exe"))
                    .Where(File.Exists)
                    .OrderByDescending(p => p, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (found != null) return found;
            }
            catch { /* ignore */ }
        }

        // 2) 注册表卸载项兜底
        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).OpenSubKey(UninstallKey);
                if (key == null) continue;
                var loc = key.GetValue("InstallLocation") as string;
                var ver = key.GetValue("DisplayVersion") as string;
                if (string.IsNullOrWhiteSpace(loc)) continue;
                string candidate = string.IsNullOrWhiteSpace(ver)
                    ? Path.Combine(loc, "Installer", "setup.exe")
                    : Path.Combine(loc, ver, "Installer", "setup.exe");
                if (File.Exists(candidate)) return candidate;
            }
            catch { /* ignore */ }
        }
        return null;
    }

    public bool IsAvailable() => true;

    public bool IsApplied() => !EdgePresent();

    public bool Apply()
    {
        if (!EdgePresent()) return true; // 已不存在

        var setup = ResolveSetup();
        if (setup == null)
            throw new InvalidOperationException("未找到 Edge 官方卸载器（setup.exe），无法安全移除。");

        var code = HighRiskUtil.RunAndWait(setup,
            "--uninstall --system-level --verbose-logging --force-uninstall", 300_000);

        // 稍等文件释放后再校验
        Thread.Sleep(1500);
        if (EdgePresent())
        {
            throw new InvalidOperationException(
                "官方卸载器已执行（退出码 " + code + "），但 Edge 仍存在——当前 Windows 版本将其作为受保护组件，拒绝卸载。\n" +
                "这是系统限制，本工具不会强制删除文件以免破坏系统。可尝试在“设置→应用”中卸载，或保留 Edge。");
        }
        return true;
    }

    /// <summary>Edge 移除不可原地“还原”，需重新安装；返回 true 表示无需自动还原。</summary>
    public bool Restore() => true;
}
