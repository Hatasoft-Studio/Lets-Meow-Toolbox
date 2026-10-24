using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Advanced;

/// <summary>🟡 高级：修改 Windows 更新暂停期限 / 禁用 Windows 更新（启用弹风险提示）。</summary>
public static class WindowsUpdateModule
{
    private const string WUKey = @"HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings";

    private const string FeatureIdPause = "adv.winupdate.pause";
    private const string FeatureIdDisable = "adv.winupdate.disable";

    public static IReadOnlyList<FeatureViewModel> CreateAll() => new[]
    {
        new FeatureViewModel
        {
            Id = FeatureIdPause,
            Title = "暂停 Windows 更新（超长延期）",
            Risk = "🟡",
            RequiresAdmin = true,
            Description =
                "把 Windows 更新暂停到 2099 年，相当于无限期暂停自动更新。\n" +
                "适合不希望系统在关键工作时自动重启更新的场景。安全补丁将不会自动安装。",
            RegistryNote = $@"{WUKey}\PauseUpdatesExpiryTime",
            State = () => PauseApplied(),
            Apply = on =>
            {
                if (on)
                {
                    var now = DateTime.UtcNow;
                    BackupService.Instance.Record(FeatureIdPause, "更新暂停", WUKey, "PauseUpdatesExpiryTime");
                    BackupService.Instance.Record(FeatureIdPause, "更新暂停", WUKey, "PauseFeatureUpdatesStartTime");
                    BackupService.Instance.Record(FeatureIdPause, "更新暂停", WUKey, "PauseFeatureUpdatesEndTime");
                    BackupService.Instance.Record(FeatureIdPause, "更新暂停", WUKey, "PauseQualityUpdatesStartTime");
                    BackupService.Instance.Record(FeatureIdPause, "更新暂停", WUKey, "PauseQualityUpdatesEndTime");
                    bool ok = true;
                    ok &= RegistryUtil.SetString(WUKey, "PauseUpdatesExpiryTime", "2099-12-31T00:00:00Z");
                    ok &= RegistryUtil.SetString(WUKey, "PauseFeatureUpdatesStartTime", now.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    ok &= RegistryUtil.SetString(WUKey, "PauseFeatureUpdatesEndTime", "2099-12-31T00:00:00Z");
                    ok &= RegistryUtil.SetString(WUKey, "PauseQualityUpdatesStartTime", now.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    ok &= RegistryUtil.SetString(WUKey, "PauseQualityUpdatesEndTime", "2099-12-31T00:00:00Z");
                    return (ok, ok ? "已暂停更新至 2099 年" : "写入失败，请以管理员身份运行");
                }
                else
                {
                    bool ok = true;
                    ok &= RegistryUtil.DeleteValue(WUKey, "PauseUpdatesExpiryTime");
                    ok &= RegistryUtil.DeleteValue(WUKey, "PauseFeatureUpdatesStartTime");
                    ok &= RegistryUtil.DeleteValue(WUKey, "PauseFeatureUpdatesEndTime");
                    ok &= RegistryUtil.DeleteValue(WUKey, "PauseQualityUpdatesStartTime");
                    ok &= RegistryUtil.DeleteValue(WUKey, "PauseQualityUpdatesEndTime");
                    BackupService.Instance.Clear(FeatureIdPause);
                    return (ok, ok ? "已恢复自动更新" : "还原失败");
                }
            }
        },
        new FeatureViewModel
        {
            Id = FeatureIdDisable,
            Title = "禁用 Windows 更新服务",
            Risk = "🟡",
            RequiresAdmin = true,
            Description =
                "把 Windows 更新服务（wuauserv）设为禁用，彻底停止自动更新与后台下载。\n" +
                "这是比较强硬的设置，系统将不再接收任何更新。",
            RegistryNote = "services.msc → Windows Update (wuauserv)",
            State = () => false,
            Apply = on =>
            {
                if (on)
                {
                    var cfg = CmdRunner.Run("sc.exe", "config wuauserv start= disabled");
                    var stop = CmdRunner.Run("sc.exe", "stop wuauserv");
                    bool ok = cfg.Success || stop.Success;
                    return (ok, ok ? "已禁用 Windows 更新服务（重启后彻底生效）" : "操作失败，请以管理员身份运行");
                }
                else
                {
                    var cfg = CmdRunner.Run("sc.exe", "config wuauserv start= delayed-auto");
                    var start = CmdRunner.Run("sc.exe", "start wuauserv");
                    bool ok = cfg.Success || start.Success;
                    return (ok, ok ? "已恢复 Windows 更新服务" : "操作失败");
                }
            }
        }
    };

    private static bool PauseApplied()
    {
        var expiry = RegistryUtil.GetString(WUKey, "PauseUpdatesExpiryTime");
        if (string.IsNullOrWhiteSpace(expiry)) return false;
        return DateTime.TryParse(expiry, out var dt) && dt.Year >= 2090;
    }
}
