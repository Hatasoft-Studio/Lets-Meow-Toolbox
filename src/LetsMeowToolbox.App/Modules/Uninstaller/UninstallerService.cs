using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Uninstaller;

/// <summary>
/// 软件卸载：读取卸载注册表、调用原卸载程序、扫描文件+注册表残留、
/// 残留预览、用户手动勾选清理。所有清理动作都必须用户手动确认。
/// </summary>
public static class UninstallerService
{
    private static readonly string[] UninstallSubKeys =
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    /// <summary>枚举卸载注册表中的全部程序（64 位 + 32 位 + 当前用户）。</summary>
    public static IReadOnlyList<UninstallEntry> Enumerate()
    {
        var map = new Dictionary<string, UninstallEntry>();

        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            foreach (var sub in UninstallSubKeys)
            {
                foreach (var entry in ReadKey(RegistryHive.LocalMachine, view, sub, "HKLM"))
                    map[entry.Key] = entry;
            }
        }
        foreach (var entry in ReadKey(RegistryHive.CurrentUser, RegistryView.Default, UninstallSubKeys[0], "HKCU"))
            map[entry.Key] = entry;

        return map.Values
            .Where(e => !string.IsNullOrWhiteSpace(e.DisplayName))
            .OrderBy(e => e.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static IEnumerable<UninstallEntry> ReadKey(RegistryHive hive, RegistryView view, string sub, string hiveName)
    {
        var list = new List<UninstallEntry>();
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(sub);
            if (key == null) yield break;
            foreach (var name in key.GetSubKeyNames())
            {
                using var k = key.OpenSubKey(name);
                if (k == null) continue;
                var dn = k.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(dn)) continue;
                list.Add(new UninstallEntry
                {
                    Key = $"{sub}\\{name}",
                    Hive = hiveName,
                    DisplayName = dn,
                    Publisher = k.GetValue("Publisher") as string ?? string.Empty,
                    DisplayVersion = k.GetValue("DisplayVersion") as string ?? string.Empty,
                    UninstallString = k.GetValue("UninstallString") as string ?? string.Empty,
                    QuietUninstallString = k.GetValue("QuietUninstallString") as string ?? string.Empty,
                    InstallLocation = k.GetValue("InstallLocation") as string ?? string.Empty,
                    EstimatedSizeKB = k.GetValue("EstimatedSize") is int sz ? (long)sz : 0
                });
            }
        }
        catch { /* 读取失败跳过 */ }
        foreach (var e in list) yield return e;
    }

    /// <summary>调用原卸载程序。优先静默卸载串，其次交互卸载串。</summary>
    public static (bool ok, string message) RunUninstaller(UninstallEntry entry)
    {
        var cmd = !string.IsNullOrWhiteSpace(entry.QuietUninstallString)
            ? entry.QuietUninstallString
            : entry.UninstallString;
        if (string.IsNullOrWhiteSpace(cmd)) return (false, "未找到卸载命令（可能被系统保护或已失效）");

        var (exe, args) = ParseCommand(cmd);
        var result = CmdRunner.Run(exe, args, timeoutMs: 300_000);
        return result.Success
            ? (true, "卸载程序已执行完成")
            : (false, $"卸载程序返回异常（退出码 {result.ExitCode}）：{result.Output.Trim()}");
    }

    private static (string exe, string args) ParseCommand(string cmd)
    {
        cmd = cmd.Trim();
        if (cmd.StartsWith("\""))
        {
            var end = cmd.IndexOf('"', 1);
            if (end > 0)
            {
                var exe = cmd.Substring(1, end - 1);
                var args = cmd.Substring(end + 1).Trim();
                return (exe, args);
            }
        }
        else if (cmd.StartsWith("msiexec", StringComparison.OrdinalIgnoreCase) ||
                 cmd.StartsWith("rundll32", StringComparison.OrdinalIgnoreCase) ||
                 cmd.StartsWith("wmic", StringComparison.OrdinalIgnoreCase))
        {
            var sp = cmd.IndexOf(' ');
            return sp > 0 ? (cmd[..sp], cmd[(sp + 1)..]) : (cmd, string.Empty);
        }
        return (cmd, string.Empty);
    }
}
