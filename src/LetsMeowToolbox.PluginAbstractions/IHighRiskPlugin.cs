namespace LetsMeowToolbox.PluginAbstractions;

/// <summary>
/// 高危插件对外契约。所有 🔴 高危功能只允许以外置 DLL 插件的形式出现，
/// 主程序本体（LetsMeowToolbox.exe）不包含任何高危代码，仅通过本接口动态加载并调用。
/// </summary>
public interface IHighRiskPlugin
{
    /// <summary>插件唯一标识（建议使用反向域名风格）。</summary>
    string Id { get; }

    /// <summary>插件显示名称。</summary>
    string PluginName { get; }

    /// <summary>插件简介（人话说明）。</summary>
    string Description { get; }

    /// <summary>版权信息（作者 / 项目主页，写入“特别鸣谢”）。</summary>
    string Credit { get; }

    /// <summary>该插件提供的全部高危操作。</summary>
    IReadOnlyList<IHighRiskAction> Actions { get; }
}
