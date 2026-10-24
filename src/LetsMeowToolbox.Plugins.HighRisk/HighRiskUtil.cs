using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace LetsMeowToolbox.Plugins.HighRisk;

/// <summary>
/// 高危插件内部的注册表 / 进程 / 防火墙辅助（仅插件使用，主程序本体不包含这些逻辑）。
/// 所有 HKLM 读写均显式指定 64 位视图，避免被 WOW64 重定向到无效节点。
/// </summary>
internal static class HighRiskUtil
{
    // INetFwPolicy2 配置文件掩码
    private const int FwProfileDomain = 1;
    private const int FwProfilePrivate = 2;
    private const int FwProfilePublic = 4;
    private const int FwProfileAll = 0x7FFFFFFF;

    private static RegistryKey OpenBase() =>
        RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

    /// <summary>以 DWORD 写 HKLM 64 位策略值。</summary>
    public static bool SetPolicyDword(string subKey, string name, int value)
    {
        try
        {
            using var baseKey = OpenBase();
            using var key = baseKey.CreateSubKey(subKey, true);
            key.SetValue(name, value, RegistryValueKind.DWord);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>删除 HKLM 下的某个值（还原时使用）。不存在则视为成功。</summary>
    public static bool DeleteValue(string subKey, string name)
    {
        try
        {
            using var baseKey = OpenBase();
            using var key = baseKey.OpenSubKey(subKey, writable: true);
            if (key == null) return true;
            key.DeleteValue(name, throwOnMissingValue: false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>读取 DWORD，不存在返回 null。</summary>
    public static int? GetDword(string subKey, string name)
    {
        try
        {
            using var baseKey = OpenBase();
            using var key = baseKey.OpenSubKey(subKey);
            var v = key?.GetValue(name);
            return v is int i ? i : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>启动可执行文件并等待退出，返回退出码；超时则结束并返回 -1。</summary>
    public static int RunAndWait(string file, string args, int timeoutMs = 120_000)
    {
        try
        {
            var psi = new ProcessStartInfo(file, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return -1;
            if (p.WaitForExit(timeoutMs))
            {
                return p.ExitCode;
            }
            try { p.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>启动可执行文件并捕获输出（退出码 + 标准输出/错误），用于读取真实系统状态。</summary>
    public static (int code, string output) RunCapture(string file, string args, int timeoutMs = 90_000)
    {
        try
        {
            var psi = new ProcessStartInfo(file, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            using var p = Process.Start(psi);
            if (p == null) return (-1, string.Empty);
            var sb = new StringBuilder();
            sb.Append(p.StandardOutput.ReadToEnd());
            sb.Append(p.StandardError.ReadToEnd());
            if (!p.WaitForExit(timeoutMs))
            {
                try { p.Kill(entireProcessTree: true); } catch { /* ignore */ }
                return (-1, sb.ToString());
            }
            return (p.ExitCode, sb.ToString());
        }
        catch (Exception ex)
        {
            return (-1, ex.Message);
        }
    }

    /// <summary>
    /// 通过 INetFwPolicy2 立即设置三个配置文件防火墙状态。
    /// 返回 true 表示调用成功；null 表示该系统不支持 COM 接口（需走 netsh 兜底）。
    /// </summary>
    public static bool? SetFirewallEnabled(bool enabled)
    {
        try
        {
            var t = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            if (t == null) return null;
            object? fw = Activator.CreateInstance(t);
            t.InvokeMember("FirewallEnabled", BindingFlags.SetProperty, null, fw,
                new object[] { FwProfileAll, enabled });
            return true;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 读取真实防火墙状态：任一配置文件仍开启则返回 true；全部关闭返回 false；
    /// 无法获取返回 null。
    /// </summary>
    public static bool? IsFirewallEnabled()
    {
        try
        {
            var t = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            if (t == null) return null;
            object? fw = Activator.CreateInstance(t);
            foreach (int profile in new[] { FwProfileDomain, FwProfilePrivate, FwProfilePublic })
            {
                var raw = t.InvokeMember("FirewallEnabled",
                    BindingFlags.GetProperty, null, fw, new object[] { profile });
                if (raw is bool enabled && enabled) return true;
            }
            return false;
        }
        catch
        {
            return null;
        }
    }
}
