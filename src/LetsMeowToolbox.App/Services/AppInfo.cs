namespace LetsMeowToolbox.App.Services;

/// <summary>
/// 应用版本与更新日志。
/// 版本规则（自此版起执行）：主版本号.次版本号.修订号
///   • 主版本号：重大架构调整 / 破坏性变更时 +1（0.x 阶段指整体重构）
///   • 次版本号：新增功能时 +1
///   • 修订号：修复缺陷 / 优化时 +1
/// 每次发布时需在本类同步更新 Version 与 Changelog。
/// </summary>
public static class AppInfo
{
    /// <summary>当前版本号（显示用）。</summary>
    public const string Version = "0.1.1";

    /// <summary>GitHub 仓库。</summary>
    public const string RepoName = "Hatasoft-Studio/Lets-Meow-Toolbox";
    public const string RepoUrl = "https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/";

    /// <summary>更新日志（按版本倒序，最新在上）。</summary>
    public static readonly IReadOnlyList<(string Version, string Date, string[] Changes)> Changelog = new[]
    {
        ("v0.1.1", "2026-10-24", new[]
        {
            "【修复】日志系统：日志改为全局记录，记录用户全部设置操作、执行结果与时间（INFO/WARN/ERROR），不再为空文件。",
            "【新增】设置页新增「一键初始化全部内容」：还原全部功能设置到默认状态、复位外观设置、清空配置预设。",
            "【新增】版本号体系：自此版起为 v0.1.1，关于页显示当前版本；新增「查看更新日志」按钮（本窗口）。",
            "【新增】特别鸣谢：每个开源项目新增「访问」按钮，一键打开该项目主页/GitHub 仓库；新增 EZ-Mirror 条目；修复字体模糊（加大字号、加深颜色）。",
            "【新增】界面重组：「卸载程序」与「解除占用」从新手区/高级区拆出，各自独立为一个导航分类与独立页面。",
            "【新增】关于页新增 GitHub 仓库信息与「前往 GitHub 仓库」一键按钮。",
            "【修复】二次元模式下开关与按钮的颜色、文字可读性优化。"
        })
    };
}
