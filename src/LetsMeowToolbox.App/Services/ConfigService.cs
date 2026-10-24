using System.IO;
using System.Text.Json;

namespace LetsMeowToolbox.App.Services;

/// <summary>
/// 配置导入导出（预设分享）。记录每个功能的“启用/关闭”状态到本地 JSON，
/// 可导出为 .meowpreset 文件与他人分享，也可导入他人预设。
/// 不包含任何敏感数据，仅保存功能开关与设置项。
/// </summary>
public sealed class ConfigService
{
    private static readonly Lazy<ConfigService> _instance = new(() => new ConfigService());
    public static ConfigService Instance => _instance.Value;

    private readonly object _lock = new();
    private readonly string _file;
    private Dictionary<string, bool> _states = new();

    public event Action? StatesChanged;

    private ConfigService()
    {
        _file = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LetsMeowToolbox", "config.json");
        Load();
    }

    public IReadOnlyDictionary<string, bool> States
    {
        get { lock (_lock) return new Dictionary<string, bool>(_states); }
    }

    /// <summary>设置某个功能开关状态。</summary>
    public void Set(string featureId, bool on)
    {
        lock (_lock)
        {
            _states[featureId] = on;
            Save();
        }
        StatesChanged?.Invoke();
    }

    public bool IsOn(string featureId)
    {
        lock (_lock) return _states.TryGetValue(featureId, out var v) && v;
    }

    /// <summary>导出预设到文件。返回文件路径。</summary>
    public string Export(string targetPath)
    {
        Dictionary<string, bool> snapshot;
        lock (_lock) snapshot = new Dictionary<string, bool>(_states);
        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(targetPath, json);
        return targetPath;
    }

    /// <summary>从文件导入预设。返回导入的功能条目数。</summary>
    public int Import(string sourcePath)
    {
        var json = File.ReadAllText(sourcePath);
        var data = JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? new();
        lock (_lock)
        {
            foreach (var (k, v) in data) _states[k] = v;
            Save();
        }
        StatesChanged?.Invoke();
        LogService.Instance.Info($"配置：导入预设成功，共 {data.Count} 项。");
        return data.Count;
    }

    /// <summary>清空全部配置预设状态（一键初始化使用）。</summary>
    public void Clear()
    {
        lock (_lock)
        {
            _states.Clear();
            Save();
        }
        StatesChanged?.Invoke();
        LogService.Instance.Info("配置：已清空全部预设状态（一键初始化）。");
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_file))
                _states = JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(_file)) ?? new();
        }
        catch { _states = new(); }
    }

    private void Save()
    {
        try { File.WriteAllText(_file, JsonSerializer.Serialize(_states)); } catch { /* ignore */ }
    }
}
