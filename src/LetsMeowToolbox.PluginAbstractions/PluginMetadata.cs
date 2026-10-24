namespace LetsMeowToolbox.PluginAbstractions;

/// <summary>
/// 插件操作执行结果。返回给主程序用于记录日志与展示。
/// </summary>
public sealed record HighRiskActionResult(bool Success, string Message);

/// <summary>
/// 标记高危插件程序集的特性，便于主程序识别与展示。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class HighRiskPluginAttribute : Attribute
{
    public string Name { get; }
    public string Version { get; }

    public HighRiskPluginAttribute(string name, string version)
    {
        Name = name;
        Version = version;
    }
}
