using System.IO;
using System.Text.Json;

namespace LetsMeowToolbox.App.Services;

/// <summary>应用自身 UI 偏好设置（二次元装饰、底层路径显示等）。固定浅色 Fluent 主题。</summary>
public sealed class SettingsService
{
    private static readonly Lazy<SettingsService> _instance = new(() => new SettingsService());
    public static SettingsService Instance => _instance.Value;

    private readonly object _lock = new();
    private readonly string _file;
    private bool _showAnime = true;
    private bool _showRegistryNote;
    private bool _licenseAgreed;

    public event Action? Changed;

    private SettingsService()
    {
        _file = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LetsMeowToolbox", "settings.json");
        Load();
    }

    public bool ShowAnime
    {
        get => _showAnime;
        set
        {
            lock (_lock)
            {
                if (_showAnime == value) return;
                _showAnime = value;
                Save();
            }
            LogService.Instance.Info($"设置：二次元装饰 = {(_showAnime ? "开" : "关")}");
            Changed?.Invoke();
        }
    }

    public bool ShowRegistryNote
    {
        get => _showRegistryNote;
        set
        {
            lock (_lock)
            {
                if (_showRegistryNote == value) return;
                _showRegistryNote = value;
                Save();
            }
            LogService.Instance.Info($"设置：显示底层注册表/API 路径 = {(_showRegistryNote ? "开" : "关")}");
            Changed?.Invoke();
        }
    }

    /// <summary>是否已同意 GPL-3.0 协议（首次启动需同意）。</summary>
    public bool LicenseAgreed
    {
        get => _licenseAgreed;
        set
        {
            lock (_lock)
                if (_licenseAgreed != value)
                {
                    _licenseAgreed = value;
                    Save();
                }
        }
    }

    /// <summary>
    /// 一键初始化：外观设置复位为默认（二次元装饰=开、显示底层路径=关）。
    /// 保留 LicenseAgreed（已同意的协议不重复询问）。
    /// </summary>
    public void ResetToDefault()
    {
        lock (_lock)
        {
            _showAnime = true;
            _showRegistryNote = false;
            Save();
        }
        LogService.Instance.Info("设置：外观设置已复位为默认值（一键初始化）。");
        Changed?.Invoke();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_file)) return;
            var j = JsonSerializer.Deserialize<SettingsDto>(File.ReadAllText(_file));
            if (j == null) return;
            _showAnime = j.ShowAnime;
            _showRegistryNote = j.ShowRegistryNote;
            _licenseAgreed = j.LicenseAgreed;
        }
        catch { /* ignore */ }
    }

    private void Save()
    {
        try
        {
            var j = new SettingsDto
            {
                ShowAnime = _showAnime,
                ShowRegistryNote = _showRegistryNote,
                LicenseAgreed = _licenseAgreed
            };
            File.WriteAllText(_file, JsonSerializer.Serialize(j));
        }
        catch { /* ignore */ }
    }

    private sealed class SettingsDto
    {
        public bool ShowAnime { get; set; } = true;
        public bool ShowRegistryNote { get; set; }
        public bool LicenseAgreed { get; set; }
    }
}
