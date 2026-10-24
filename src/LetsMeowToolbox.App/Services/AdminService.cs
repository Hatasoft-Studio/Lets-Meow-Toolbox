using System.Diagnostics;
using System.Windows;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Services;

/// <summary>
/// 管理员权限检测与提权重启。
/// </summary>
public static class AdminService
{
    public static bool IsAdmin => ProcessUtil.IsAdmin();

    /// <summary>以管理员身份重新启动当前程序，并结束当前普通权限实例。</summary>
    public static void RelaunchAsAdmin(string? extraArg = null)
    {
        var exe = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exe)) return;
        var psi = new ProcessStartInfo(exe)
        {
            Verb = "runas",
            UseShellExecute = true,
            Arguments = extraArg ?? string.Empty
        };
        try
        {
            Process.Start(psi);
            Application.Current.Shutdown();
        }
        catch
        {
            // 用户取消 UAC 或启动失败：保持当前实例继续运行
        }
    }
}
