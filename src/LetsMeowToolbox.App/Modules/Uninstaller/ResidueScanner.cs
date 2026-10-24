using System.IO;
using Microsoft.Win32;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Uninstaller;

/// <summary>
/// 残留扫描：卸载后扫描文件系统与注册表中可能残留的内容，生成候选列表。
/// 所有候选默认不勾选，必须用户手动勾选后才会清理。
/// </summary>
public static class ResidueScanner
{
    /// <summary>扫描某个已卸载程序的残留。</summary>
    public static IReadOnlyList<ResidueCandidate> Scan(UninstallEntry entry)
    {
        var list = new List<ResidueCandidate>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var name = entry.DisplayName;
        var pub = entry.Publisher;

        void Add(string type, string target, string desc, bool isRegistry = false)
        {
            if (string.IsNullOrWhiteSpace(target)) return;
            if (!seen.Add(target)) return;
            list.Add(new ResidueCandidate { Type = type, Target = target, Description = desc, IsRegistry = isRegistry });
        }

        // 1. 卸载注册表键本身是否还在（卸载失败常见残留）—— 按条目所在根精确定位
        var hivePrefix = entry.Hive == "HKCU" ? "HKCU" : "HKLM";
        if (entry.Hive == "HKCU")
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                using var k = baseKey.OpenSubKey(entry.Key);
                if (k != null && k.GetValue("DisplayName") != null)
                    Add("注册表", $"{hivePrefix}\\{entry.Key}", "卸载后仍残留的卸载信息键", true);
            }
            catch { /* ignore */ }
        }
        else
        {
            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                    using var k = baseKey.OpenSubKey(entry.Key);
                    if (k != null && k.GetValue("DisplayName") != null)
                        Add("注册表", $"{hivePrefix}\\{entry.Key}", "卸载后仍残留的卸载信息键", true);
                }
                catch { /* ignore */ }
            }
        }

        // 2. 安装目录是否仍存在
        if (!string.IsNullOrWhiteSpace(entry.InstallLocation) && Directory.Exists(entry.InstallLocation))
            Add("目录", entry.InstallLocation, "卸载后仍存在的安装目录");

        // 3. 常见数据目录（AppData）
        foreach (var root in new[] {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) })
        {
            foreach (var candidate in new[] { name, pub })
            {
                if (string.IsNullOrWhiteSpace(candidate)) continue;
                var p = Path.Combine(root, candidate);
                if (Directory.Exists(p))
                    Add("目录", p, "程序数据目录残留");
            }
        }

        // 4. 开始菜单 / 桌面快捷方式
        var sm = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs)),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms)),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory))
        };
        foreach (var dir in sm.Where(Directory.Exists))
        {
            foreach (var file in EnumerateShortcuts(dir))
            {
                if (ContainsName(file, name) || ContainsName(file, pub))
                    Add("快捷方式", file, "卸载后残留的快捷方式");
            }
        }

        return list;
    }

    /// <summary>清理用户勾选的残留项。返回清理数量。</summary>
    public static (int cleaned, int failed) Clean(IEnumerable<ResidueCandidate> selected)
    {
        int cleaned = 0, failed = 0;
        foreach (var item in selected)
        {
            try
            {
                if (item.IsRegistry)
                {
                    // Target 形如 HKLM\...\key 或 HKCU\...\key
                    if (DeleteRegistryKey(item.Target)) cleaned++; else failed++;
                }
                else
                {
                    if (item.Type == "快捷方式" && File.Exists(item.Target))
                    {
                        File.Delete(item.Target); cleaned++;
                    }
                    else if (Directory.Exists(item.Target))
                    {
                        Directory.Delete(item.Target, recursive: true); cleaned++;
                    }
                    else if (File.Exists(item.Target))
                    {
                        File.Delete(item.Target); cleaned++;
                    }
                    else { failed++; }
                }
            }
            catch { failed++; }
        }
        return (cleaned, failed);
    }

    private static IEnumerable<string> EnumerateShortcuts(string dir)
    {
        var list = new List<string>();
        try
        {
            foreach (var f in Directory.EnumerateFiles(dir, "*.lnk"))
                list.Add(f);
        }
        catch { /* ignore */ }
        return list;
    }

    private static bool ContainsName(string path, string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var baseName = Path.GetFileNameWithoutExtension(path);
        var nm = name.Trim().TrimEnd(' ', '.');
        if (string.IsNullOrEmpty(nm)) return false;
        return baseName.Contains(nm, StringComparison.OrdinalIgnoreCase) ||
               nm.Contains(baseName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool DeleteRegistryKey(string fullKey)
    {
        try
        {
            fullKey = fullKey.Trim();
            if (fullKey.StartsWith(@"HKLM\", StringComparison.OrdinalIgnoreCase))
            {
                var sub = fullKey[5..];
                var parts = sub.Split('\\');
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var parent = baseKey.OpenSubKey(string.Join('\\', parts.Take(parts.Length - 1)), true);
                parent?.DeleteSubKeyTree(parts.Last(), throwOnMissingSubKey: false);
                return true;
            }
            if (fullKey.StartsWith(@"HKCU\", StringComparison.OrdinalIgnoreCase))
            {
                var sub = fullKey[5..];
                var parts = sub.Split('\\');
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                using var parent = baseKey.OpenSubKey(string.Join('\\', parts.Take(parts.Length - 1)), true);
                parent?.DeleteSubKeyTree(parts.Last(), throwOnMissingSubKey: false);
                return true;
            }
            return false;
        }
        catch { return false; }
    }
}
