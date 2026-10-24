using LetsMeowToolbox.App.Models;

namespace LetsMeowToolbox.App.Models;

/// <summary>单个功能卡片的数据模型（用于新手区 / 高级区的开关型功能）。</summary>
public sealed class FeatureViewModel : ObservableObject
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    /// <summary>人话说明。</summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>底层注册表 / API 路径（可选，可关闭显示）。</summary>
    public string RegistryNote { get; init; } = string.Empty;
    /// <summary>风险标识：🟢 / 🟡 / 🔴。</summary>
    public string Risk { get; init; } = "🟢";
    /// <summary>是否仅 Win11 可用（否则在 Win10 下自动隐藏）。</summary>
    public bool Win11Only { get; init; }
    /// <summary>是否需要管理员权限。</summary>
    public bool RequiresAdmin { get; init; }
    /// <summary>该功能当前“建议开启”的初始状态（仅用于展示，不自动执行）。</summary>
    public bool SuggestedOn { get; init; }

    /// <summary>启用/关闭的委托：返回结果消息。由视图在用户切换时调用。</summary>
    public Func<bool, (bool ok, string message)>? Apply { get; init; }
    /// <summary>查询当前状态（用于切换后刷新），可为空。</summary>
    public Func<bool>? State { get; init; }

    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set => Set(ref _isOn, value);
    }
}
