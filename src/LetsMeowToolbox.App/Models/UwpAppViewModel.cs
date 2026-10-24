using System.Text.Json;

namespace LetsMeowToolbox.App.Models;

/// <summary>UWP 应用卸载列表项。</summary>
public sealed class UwpAppViewModel : ObservableObject
{
    public string Name { get; init; } = string.Empty;
    /// <summary>开始菜单友好名称（可能为空）。</summary>
    public string FriendlyName { get; init; } = string.Empty;
    public string PackageFullName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    /// <summary>是否在白名单内（可卸载）。</summary>
    public bool Removable { get; init; }
    /// <summary>是否核心保护（禁止卸载）。</summary>
    public bool CoreProtected { get; init; }

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => Set(ref _isChecked, value);
    }

    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        set { Set(ref _status, value); OnPropertyChanged(nameof(HasStatus)); }
    }
    public bool HasStatus => !string.IsNullOrEmpty(_status);
    /// <summary>展示名：优先友好名，其次技术名，最后完整包名。</summary>
    public string DisplayName
        => !string.IsNullOrWhiteSpace(FriendlyName) ? FriendlyName
         : !string.IsNullOrWhiteSpace(Name) ? Name
         : PackageFullName;
    public string Badge => CoreProtected ? "系统核心·禁止" : (Removable ? "可卸载" : "白名单外");
}

/// <summary>PowerShell ConvertTo-Json 输出对应的最小字段。</summary>
internal sealed class AppxJsonEntry
{
    public string? Name { get; set; }
    public string? Friendly { get; set; }
    public string? PackageFullName { get; set; }
    public string? PackageFamilyName { get; set; }
    public string? Version { get; set; }
    public bool? IsFramework { get; set; }
}
