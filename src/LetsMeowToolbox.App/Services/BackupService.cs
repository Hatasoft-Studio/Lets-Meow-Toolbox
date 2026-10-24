using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Services;

/// <summary>单条注册表改动前的状态快照，用于还原。</summary>
public sealed class RegBackupItem
{
    public string Key { get; set; } = string.Empty;       // HKLM\...\...
    public string ValueName { get; set; } = string.Empty; // 空串表示“(默认)”
    public int View { get; set; }                          // RegistryView 值
    public int Kind { get; set; }                          // RegistryValueKind 值
    public bool Existed { get; set; }
    public object? OriginalValue { get; set; }             // JSON 可序列化（string/int/long/byte[]）
}

/// <summary>某个功能的全部改动快照。</summary>
public sealed class BackupEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<RegBackupItem> Items { get; set; } = new();
}

/// <summary>
/// 单条功能注册表备份 / 单独还原、一键还原。所有修改类操作必须先 Record 再写入。
/// </summary>
public sealed class BackupService
{
    private static readonly Lazy<BackupService> _instance = new(() => new BackupService());
    public static BackupService Instance => _instance.Value;

    private readonly object _lock = new();
    private readonly string _file;
    private Dictionary<string, BackupEntry> _entries = new();

    private BackupService()
    {
        _file = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LetsMeowToolbox", "backups.json");
        Load();
    }

    public IReadOnlyCollection<BackupEntry> Entries
    {
        get { lock (_lock) return _entries.Values.OrderByDescending(e => e.CreatedAt).ToList(); }
    }

    /// <summary>记录某个值改动前的状态（必须在写入之前调用）。</summary>
    public void Record(string id, string name, string fullKey, string valueName,
        RegistryView view = RegistryView.Default)
    {
        lock (_lock)
        {
            if (!_entries.TryGetValue(id, out var entry))
            {
                entry = new BackupEntry { Id = id, Name = name };
                _entries[id] = entry;
            }
            var item = new RegBackupItem
            {
                Key = fullKey,
                ValueName = valueName,
                View = (int)view
            };
            try
            {
                var (hive, sub) = SplitKey(fullKey);
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                using var k = baseKey.OpenSubKey(sub);
                var v = k?.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                item.Existed = k != null && k.GetValueNames().Contains(valueName);
                if (item.Existed && v != null && k != null)
                {
                    item.Kind = (int)k.GetValueKind(valueName);
                    item.OriginalValue = v;
                }
            }
            catch { item.Existed = false; }
            entry.Items.Add(item);
            Save();
        }
    }

    /// <summary>清除某个功能的备份（用户主动放弃还原时）。</summary>
    public void Clear(string id)
    {
        lock (_lock)
        {
            _entries.Remove(id);
            Save();
        }
    }

    /// <summary>单独还原某个功能。</summary>
    public bool RestoreFeature(string id)
    {
        BackupEntry entry;
        lock (_lock)
        {
            if (!_entries.TryGetValue(id, out entry!)) return false;
        }
        bool allOk = true;
        foreach (var item in entry.Items)
        {
            var view = (RegistryView)item.View;
            var kind = (RegistryValueKind)item.Kind;
            var (hive, sub) = SplitKey(item.Key);
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                if (item.Existed)
                {
                    using var k = baseKey.CreateSubKey(sub, true);
                    if (item.OriginalValue != null)
                        k?.SetValue(item.ValueName, item.OriginalValue, kind);
                }
                else
                {
                    using var k = baseKey.OpenSubKey(sub, true);
                    k?.DeleteValue(item.ValueName, throwOnMissingValue: false);
                }
            }
            catch { allOk = false; }
        }
        return allOk;
    }

    /// <summary>一键还原全部功能。返回还原的功能数量。</summary>
    public int RestoreAll()
    {
        lock (_lock)
        {
            var ids = _entries.Keys.ToList();
            foreach (var id in ids) RestoreFeature(id);
            _entries.Clear();
            Save();
            return ids.Count;
        }
    }

    public bool Has(string id)
    {
        lock (_lock) return _entries.ContainsKey(id);
    }

    public BackupEntry? Get(string id)
    {
        lock (_lock) return _entries.TryGetValue(id, out var e) ? e : null;
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_file))
            {
                var json = File.ReadAllText(_file);
                _entries = JsonSerializer.Deserialize<Dictionary<string, BackupEntry>>(json) ?? new();
            }
        }
        catch { _entries = new(); }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_file, json);
        }
        catch { /* 备份文件写入失败不阻断主流程 */ }
    }

    private static (RegistryHive, string) SplitKey(string fullKey)
    {
        if (fullKey.StartsWith(@"HKLM\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.LocalMachine, fullKey[5..]);
        if (fullKey.StartsWith(@"HKCU\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.CurrentUser, fullKey[5..]);
        throw new ArgumentException($"不支持的根键：{fullKey}");
    }
}
