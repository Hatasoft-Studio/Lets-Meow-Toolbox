using System.Diagnostics;

namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// PowerShell 执行辅助（用于 UWP 卸载、服务控制等）。只在需要管理员权限时以提权方式调用。
/// </summary>
public sealed record ShellResult(bool Success, int ExitCode, string Output);

public static class PowerShellRunner
{
    /// <summary>以无窗口方式执行 PowerShell 命令并等待，返回输出。</summary>
    public static ShellResult Run(string script, bool requiresAdmin = false, int timeoutMs = 180_000)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = requiresAdmin,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = !requiresAdmin,
            RedirectStandardError = !requiresAdmin,
            Verb = requiresAdmin ? "runas" : string.Empty,
            Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command " +
                        "\"" + script.Replace("\"", "\\\"") + "\""
        };
        try
        {
            using var p = Process.Start(psi);
            if (p == null) return new ShellResult(false, -1, "无法启动 PowerShell");
            var output = new System.Text.StringBuilder();
            if (!requiresAdmin)
            {
                p.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                p.ErrorDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }
            if (p.WaitForExit(timeoutMs))
            {
                if (!requiresAdmin)
                {
                    p.WaitForExit();
                    // 结束异步读取
                    p.CancelOutputRead();
                    p.CancelErrorRead();
                }
                return new ShellResult(p.ExitCode == 0, p.ExitCode, output.ToString());
            }
            try { p.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return new ShellResult(false, -1, "PowerShell 执行超时");
        }
        catch (Exception ex)
        {
            return new ShellResult(false, -1, ex.Message);
        }
    }
}
