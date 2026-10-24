using System.Diagnostics;

namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// 直接运行系统命令行工具（sc.exe / powercfg.exe / taskkill 等）并等待退出。
/// </summary>
public static class CmdRunner
{
    public static ShellResult Run(string exe, string args, int timeoutMs = 120_000)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        try
        {
            using var p = Process.Start(psi);
            if (p == null) return new ShellResult(false, -1, "无法启动命令");
            var outB = new System.Text.StringBuilder();
            p.OutputDataReceived += (_, e) => { if (e.Data != null) outB.AppendLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) outB.AppendLine(e.Data); };
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            if (p.WaitForExit(timeoutMs))
            {
                p.WaitForExit();
                p.CancelOutputRead();
                p.CancelErrorRead();
                return new ShellResult(p.ExitCode == 0, p.ExitCode, outB.ToString());
            }
            try { p.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return new ShellResult(false, -1, "命令执行超时");
        }
        catch (Exception ex)
        {
            return new ShellResult(false, -1, ex.Message);
        }
    }

    /// <summary>重新启动资源管理器（用于让资源管理器相关设置生效）。</summary>
    public static void RestartExplorer()
    {
        try
        {
            foreach (var p in Process.GetProcessesByName("explorer"))
            {
                try { p.Kill(entireProcessTree: false); } catch { /* ignore */ }
            }
            Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
        }
        catch { /* ignore */ }
    }
}
