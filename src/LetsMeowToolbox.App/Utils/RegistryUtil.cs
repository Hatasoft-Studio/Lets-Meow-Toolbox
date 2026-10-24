using Microsoft.Win32;

namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// 注册表读写与备份辅助。所有修改类操作都必须经 BackupService 记录“改动前状态”，
/// 以便单条还原 / 一键还原。
/// </summary>
public static class RegistryUtil
{
    /// <summary>读取 DWORD，不存在返回 null。</summary>
    public static int? GetDword(RegistryHive hive, RegistryView view, string subKey, string name)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(subKey);
            return key?.GetValue(name) is int i ? i : null;
        }
        catch { return null; }
    }

    public static int? GetDword(string fullSubKey, string name, RegistryView view = RegistryView.Default)
    {
        var (hive, sub) = Split(fullSubKey);
        return GetDword(hive, view, sub, name);
    }

    public static string? GetString(string fullSubKey, string name, RegistryView view = RegistryView.Default)
    {
        var (hive, sub) = Split(fullSubKey);
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(sub);
            return key?.GetValue(name) as string;
        }
        catch { return null; }
    }

    /// <summary>写 DWORD。返回是否成功。</summary>
    public static bool SetDword(string fullSubKey, string name, int value, RegistryView view = RegistryView.Default)
    {
        var (hive, sub) = Split(fullSubKey);
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.CreateSubKey(sub, true);
            key.SetValue(name, value, RegistryValueKind.DWord);
            return true;
        }
        catch { return false; }
    }

    /// <summary>写字符串。返回是否成功。</summary>
    public static bool SetString(string fullSubKey, string name, string value, RegistryView view = RegistryView.Default)
    {
        var (hive, sub) = Split(fullSubKey);
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.CreateSubKey(sub, true);
            key.SetValue(name, value, RegistryValueKind.String);
            return true;
        }
        catch { return false; }
    }

    /// <summary>删除某个值（不存在视为成功）。返回是否成功。</summary>
    public static bool DeleteValue(string fullSubKey, string name, RegistryView view = RegistryView.Default)
    {
        var (hive, sub) = Split(fullSubKey);
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(sub, true);
            key?.DeleteValue(name, throwOnMissingValue: false);
            return true;
        }
        catch { return false; }
    }

    /// <summary>删除整个子键（不存在视为成功）。返回是否成功。</summary>
    public static bool DeleteSubKey(string fullSubKey, RegistryView view = RegistryView.Default)
    {
        var (hive, sub) = Split(fullSubKey);
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            var parts = sub.Split('\\');
            var parent = string.Join('\\', parts.Take(parts.Length - 1));
            var leaf = parts.Last();
            using var parentKey = baseKey.OpenSubKey(parent, true);
            parentKey?.DeleteSubKeyTree(leaf, throwOnMissingSubKey: false);
            return true;
        }
        catch { return false; }
    }

    /// <summary>判断某个值是否存在。</summary>
    public static bool ValueExists(string fullSubKey, string name, RegistryView view = RegistryView.Default)
    {
        var (hive, sub) = Split(fullSubKey);
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(sub);
            return key?.GetValue(name) != null;
        }
        catch { return false; }
    }

    /// <summary>判断某个子键是否存在。</summary>
    public static bool KeyExists(string fullSubKey, RegistryView view = RegistryView.Default)
    {
        var (hive, sub) = Split(fullSubKey);
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            return baseKey.OpenSubKey(sub) != null;
        }
        catch { return false; }
    }

    private static (RegistryHive, string) Split(string fullSubKey)
    {
        fullSubKey = fullSubKey.Trim();
        if (fullSubKey.StartsWith(@"HKLM\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.LocalMachine, fullSubKey[5..]);
        if (fullSubKey.StartsWith(@"HKCU\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.CurrentUser, fullSubKey[5..]);
        if (fullSubKey.StartsWith(@"HKCR\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.ClassesRoot, fullSubKey[5..]);
        if (fullSubKey.StartsWith(@"HKU\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.Users, fullSubKey[4..]);
        throw new ArgumentException($"无法识别的注册表根键：{fullSubKey}", nameof(fullSubKey));
    }
}
