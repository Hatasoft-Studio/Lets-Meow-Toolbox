using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Beginner;

/// <summary>🟢 功能5：电源计划、索引服务、自动播放、文件预览优化。</summary>
public static class OptimizeModule
{
    private const string ExplorerAdv = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string AutoplayKey = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";

    // 电源计划 GUID（微软官方定义）
    private const string Balanced = "381b4222-f694-41f0-9685-ff5bb260df2e";
    private const string HighPerf = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";

    public static IReadOnlyList<FeatureViewModel> CreateAll() => new[]
    {
        new FeatureViewModel
        {
            Id = "optimize.power.highperf",
            Title = "启用高性能电源计划",
            Risk = "🟢",
            Description = "把电源计划切到“高性能”，减少系统降频带来的卡顿。笔记本建议结合电量使用。",
            RegistryNote = "powercfg /setactive (高性能)",
            State = () => false,
            Apply = on =>
            {
                if (on)
                {
                    var r = CmdRunner.Run("powercfg.exe", $"/setactive {HighPerf}");
                    return (r.Success, r.Success ? "已启用高性能电源计划" : "切换失败，可能设备不支持该计划");
                }
                else
                {
                    var r = CmdRunner.Run("powercfg.exe", $"/setactive {Balanced}");
                    return (r.Success, r.Success ? "已恢复平衡电源计划" : "切换失败");
                }
            }
        },
        new FeatureViewModel
        {
            Id = "optimize.index.off",
            Title = "关闭索引服务（Windows 搜索）",
            Risk = "🟡",
            RequiresAdmin = true,
            Description =
                "关闭后台文件索引服务（WSearch）。可省下磁盘 IO 与 CPU，但“开始菜单搜索、文件搜索”会变慢。\n" +
                "适合磁盘较慢、不常用搜索的机器。",
            RegistryNote = "services.msc → WSearch (Windows Search)",
            State = () => false,
            Apply = on =>
            {
                if (on)
                {
                    var stop = CmdRunner.Run("sc.exe", "config WSearch start= disabled");
                    var stopNow = CmdRunner.Run("sc.exe", "stop WSearch");
                    bool ok = stop.Success || stopNow.Success;
                    return (ok, ok ? "已关闭索引服务（重启后生效）" : "操作失败，请以管理员身份运行");
                }
                else
                {
                    var cfg = CmdRunner.Run("sc.exe", "config WSearch start= delayed-auto");
                    var start = CmdRunner.Run("sc.exe", "start WSearch");
                    bool ok = cfg.Success || start.Success;
                    return (ok, ok ? "已恢复索引服务" : "操作失败");
                }
            }
        },
        new FeatureViewModel
        {
            Id = "optimize.autoplay.off",
            Title = "关闭自动播放（U盘/光盘）",
            Risk = "🟢",
            Description = "插入 U 盘、光盘时不再自动弹出播放或执行，防止自动运行的潜在风险。",
            RegistryNote = $@"{AutoplayKey}\NoDriveTypeAutoRun",
            State = () => RegistryUtil.GetDword(AutoplayKey, "NoDriveTypeAutoRun") == 0xff,
            Apply = on =>
            {
                if (on)
                {
                    BackupService.Instance.Record("optimize.autoplay.off", "自动播放", AutoplayKey, "NoDriveTypeAutoRun");
                    bool ok = RegistryUtil.SetDword(AutoplayKey, "NoDriveTypeAutoRun", 0xff);
                    return (ok, ok ? "已关闭自动播放" : "写入失败");
                }
                else
                {
                    bool ok = RegistryUtil.DeleteValue(AutoplayKey, "NoDriveTypeAutoRun");
                    BackupService.Instance.Clear("optimize.autoplay.off");
                    return (ok, ok ? "已恢复自动播放" : "还原失败");
                }
            }
        },
        new FeatureViewModel
        {
            Id = "optimize.preview.icons",
            Title = "用图标代替缩略图（文件预览优化）",
            Risk = "🟢",
            Description =
                "资源管理器不再为每个文件生成缩略图，浏览超大文件夹时更快、更省磁盘 IO。\n" +
                "代价是文件夹里不再显示图片/视频的预览图。",
            RegistryNote = $@"{ExplorerAdv}\IconsOnly",
            State = () => RegistryUtil.GetDword(ExplorerAdv, "IconsOnly") == 1,
            Apply = on =>
            {
                if (on)
                {
                    BackupService.Instance.Record("optimize.preview.icons", "图标模式", ExplorerAdv, "IconsOnly");
                    bool ok = RegistryUtil.SetDword(ExplorerAdv, "IconsOnly", 1);
                    if (ok) CmdRunner.RestartExplorer();
                    return (ok, ok ? "已启用图标模式（浏览更快）" : "写入失败");
                }
                else
                {
                    bool ok = RegistryUtil.SetDword(ExplorerAdv, "IconsOnly", 0);
                    if (ok) CmdRunner.RestartExplorer();
                    BackupService.Instance.Clear("optimize.preview.icons");
                    return (ok, ok ? "已恢复缩略图预览" : "还原失败");
                }
            }
        }
    };
}
