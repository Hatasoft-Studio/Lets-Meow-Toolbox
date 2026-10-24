namespace LetsMeowToolbox.PluginAbstractions;

/// <summary>
/// 单个高危操作。仅描述“做什么”，是否允许执行由用户在界面手动确认后触发，
/// 插件内禁止有任何“自动执行、自动确认、无提示”逻辑。
/// </summary>
public interface IHighRiskAction
{
    /// <summary>操作唯一标识。</summary>
    string Id { get; }

    /// <summary>操作名称。</summary>
    string Title { get; }

    /// <summary>人话说明：做什么、会有什么影响。</summary>
    string Description { get; }

    /// <summary>高危风险提示文案（启用前必须向用户展示并二次确认）。</summary>
    string Warning { get; }

    /// <summary>底层注册表 / API 路径（可选，供“显示底层路径”使用）。</summary>
    string RegistryNote { get; }

    /// <summary>是否需要管理员权限。</summary>
    bool RequiresAdmin { get; }

    /// <summary>
    /// 检测该操作在当前系统是否可用（例如版本不匹配时返回 false，界面隐藏）。
    /// 返回 true 表示可用。
    /// </summary>
    bool IsAvailable();

    /// <summary>
    /// 检测该操作当前是否处于“已启用”状态（用于还原按钮 / 状态显示）。
    /// </summary>
    bool IsApplied();

    /// <summary>执行该高危操作。返回是否成功。</summary>
    bool Apply();

    /// <summary>还原该操作。返回是否成功。</summary>
    bool Restore();
}
