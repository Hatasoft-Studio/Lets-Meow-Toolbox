using LetsMeowToolbox.App.Models;

namespace LetsMeowToolbox.App.Models;

/// <summary>已安装程序条目（来自卸载注册表）。</summary>
public sealed class UninstallEntry : ObservableObject
{
    public string Key { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Publisher { get; init; } = string.Empty;
    public string DisplayVersion { get; init; } = string.Empty;
    public string UninstallString { get; init; } = string.Empty;
    public string QuietUninstallString { get; init; } = string.Empty;
    public string InstallLocation { get; init; } = string.Empty;
    public long EstimatedSizeKB { get; init; }
    /// <summary>该条目所在的注册表根（HKLM / HKCU），用于残留扫描定位。</summary>
    public string Hive { get; init; } = "HKLM";

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => Set(ref _isChecked, value);
    }

    private bool _hasResidue;
    public bool HasResidue
    {
        get => _hasResidue;
        set => Set(ref _hasResidue, value);
    }

    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        set { Set(ref _status, value); OnPropertyChanged(nameof(HasStatus)); }
    }
    public bool HasStatus => !string.IsNullOrEmpty(_status);

    public string SizeText =>
        EstimatedSizeKB <= 0 ? string.Empty : $"{EstimatedSizeKB / 1024.0:0.#} MB";
}

/// <summary>残留候选（文件 / 注册表），默认不勾选，由用户手动确认清理。</summary>
public sealed class ResidueCandidate : ObservableObject
{
    public string Type { get; init; } = "文件";
    public string Target { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsRegistry { get; init; }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => Set(ref _isChecked, value);
    }
}
