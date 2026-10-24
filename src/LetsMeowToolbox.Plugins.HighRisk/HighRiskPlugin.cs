using System.Reflection;
using LetsMeowToolbox.PluginAbstractions;

// 高危插件程序集元数据，供主程序识别与“特别鸣谢”展示。
[assembly: HighRiskPlugin("LetsMeowToolbox 高危插件（Defender / 防火墙 / Edge）", "1.0.0")]

namespace LetsMeowToolbox.Plugins.HighRisk;

/// <summary>
/// 🔴 高危插件入口。仅在本程序集（外置 DLL）内实现高危逻辑，
/// 主程序本体不包含、不内嵌任何高危代码，只在本插件被放置到 plugins 目录时动态加载。
/// </summary>
public sealed class HighRiskPlugin : IHighRiskPlugin
{
    public string Id => "io.hatasoft.meowtoolbox.highrisk";
    public string PluginName => "高危系统组件管理插件";
    public string Description =>
        "集中管理高风险系统设置（杀毒、防火墙、系统浏览器组件）。" +
        "所有操作必须由你在界面手动确认，且需要管理员权限，任何情况下都不会自动执行。";
    public string Credit =>
        "实现思路参考开源项目：Win11Debloat (https://github.com/Raphire/Win11Debloat) 与 " +
        "Sophia Script (https://github.com/farag2/Sophia-Script) 的注册表策略方案；" +
        "本插件为独立实现，仅借鉴其公开的注册表键位口径。";

    public IReadOnlyList<IHighRiskAction> Actions { get; } = new IHighRiskAction[]
    {
        new Actions.DefenderAction(),
        new Actions.FirewallAction(),
        new Actions.EdgeAction()
    };
}
