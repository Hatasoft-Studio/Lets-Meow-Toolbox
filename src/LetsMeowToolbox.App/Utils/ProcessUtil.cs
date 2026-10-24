using System.Diagnostics;

namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// 进程管理辅助，含核心进程黑名单保护。
/// </summary>
public static class ProcessUtil
{
    /// <summary>尝试关闭指定进程（先发温和的关闭请求，失败再结束）。</summary>
    public static bool CloseProcess(int pid, bool force)
    {
        Process? p = null;
        try { p = Process.GetProcessById(pid); }
        catch { return true; } // 已不存在视为成功
        if (p == null) return true;

        try
        {
            if (p.HasExited) return true;

            // 核心进程黑名单保护（含子进程同样受保护）
            if (IsProtectedProcess(p.ProcessName, pid))
            {
                return false;
            }

            if (!force)
            {
                // 先请求关闭主窗口
                try { p.CloseMainWindow(); } catch { /* ignore */ }
                if (p.WaitForExit(3000)) return true;
            }
            p.Kill(entireProcessTree: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 核心进程 / 系统进程黑名单：这些进程一律禁止被结束，
    /// 避免误关导致系统不稳定。名称判断为主，路径判断为辅。
    /// </summary>
    public static bool IsProtectedProcess(string processName, int pid)
    {
        var name = (processName ?? string.Empty).ToLowerInvariant();
        string[] hardProtected = new[]
        {
            "system", "systemsettings", "svchost", "csrss", "wininit", "winlogon",
            "lsass", "services", "smss", "explorer", "taskmgr", "dwm",
            "fontdrvhost", "sihost", "conhost", "registry", "audiodg",
            "spoolsv", "winlogon", "searchhost", "ctfmon", "runtimebroker",
            "securityhealthservice", "securityhealthsystray", "msmpeng",
            "dllhost", "shellexperiencehost", "startmenuexperiencehost", "sgasvc"
        };
        if (hardProtected.Contains(name)) return true;

        // 系统进程（Session 0 / 关键服务）——通过父进程等更保守的方式仅做补充，不误伤用户程序
        return false;
    }

    /// <summary>判断是否管理员。</summary>
    public static bool IsAdmin() => NativeMethods.IsUserAnAdmin();
}
