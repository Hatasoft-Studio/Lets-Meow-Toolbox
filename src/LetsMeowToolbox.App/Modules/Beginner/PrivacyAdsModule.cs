using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Beginner;

/// <summary>🟢 功能3：关闭系统广告、锁屏广告、遥测、通知推荐。</summary>
public static class PrivacyAdsModule
{
    private const string Cdm = @"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
    private const string DataCollection = @"HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection";
    private const string AdvId = @"HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo";

    public static IReadOnlyList<FeatureViewModel> CreateAll() => new[]
    {
        new FeatureViewModel
        {
            Id = "privacy.telemetry",
            Title = "关闭遥测（数据上报）",
            Risk = "🟢",
            RequiresAdmin = true,
            Description = "停止向微软上报诊断与使用数据。离线/注重隐私时建议开启。",
            RegistryNote = $@"{DataCollection}\AllowTelemetry",
            State = () => RegistryUtil.GetDword(DataCollection, "AllowTelemetry") == 0,
            Apply = SetDwordFeature("privacy.telemetry", DataCollection, "AllowTelemetry", 0,
                onMsg: "已关闭遥测", offMsg: "已恢复遥测")
        },
        new FeatureViewModel
        {
            Id = "privacy.advertisingid",
            Title = "关闭个性化广告 ID",
            Risk = "🟢",
            Description = "关闭系统用于跨应用推送个性化广告的广告 ID，减少广告定向。",
            RegistryNote = $@"{AdvId}\Enabled",
            State = () => RegistryUtil.GetDword(AdvId, "Enabled") == 0,
            Apply = SetDwordFeature("privacy.advertisingid", AdvId, "Enabled", 0,
                onMsg: "已关闭个性化广告 ID", offMsg: "已恢复广告 ID")
        },
        new FeatureViewModel
        {
            Id = "privacy.lockscreenads",
            Title = "关闭锁屏广告与建议",
            Risk = "🟢",
            Description = "关闭锁屏上的“Windows 聚焦”广告位与每日建议内容，锁屏不再推广告。",
            RegistryNote = $@"{Cdm}\RotatingLockScreenEnabled / SubscribedContent-338389Enabled",
            State = () => RegistryUtil.GetDword(Cdm, "RotatingLockScreenEnabled") == 0,
            Apply = on =>
            {
                if (on)
                {
                    BackupService.Instance.Record("privacy.lockscreenads", "锁屏广告", Cdm, "RotatingLockScreenEnabled");
                    BackupService.Instance.Record("privacy.lockscreenads", "锁屏广告", Cdm, "SubscribedContent-338389Enabled");
                    bool a = RegistryUtil.SetDword(Cdm, "RotatingLockScreenEnabled", 0);
                    bool b = RegistryUtil.SetDword(Cdm, "SubscribedContent-338389Enabled", 0);
                    return (a && b, (a && b) ? "已关闭锁屏广告与建议" : "部分写入失败");
                }
                else
                {
                    bool a = RegistryUtil.DeleteValue(Cdm, "RotatingLockScreenEnabled");
                    bool b = RegistryUtil.DeleteValue(Cdm, "SubscribedContent-338389Enabled");
                    BackupService.Instance.Clear("privacy.lockscreenads");
                    return (a && b, (a && b) ? "已恢复锁屏建议" : "还原失败");
                }
            }
        },
        new FeatureViewModel
        {
            Id = "privacy.notifads",
            Title = "关闭通知中心的推荐内容",
            Risk = "🟢",
            Description = "关闭“在通知中心中显示建议”的功能，通知栏不再出现应用推荐。",
            RegistryNote = $@"{Cdm}\SystemPaneSuggestionsEnabled / SubscribedContent-338388Enabled",
            State = () => RegistryUtil.GetDword(Cdm, "SystemPaneSuggestionsEnabled") == 0,
            Apply = on =>
            {
                if (on)
                {
                    BackupService.Instance.Record("privacy.notifads", "通知推荐", Cdm, "SystemPaneSuggestionsEnabled");
                    BackupService.Instance.Record("privacy.notifads", "通知推荐", Cdm, "SubscribedContent-338388Enabled");
                    bool a = RegistryUtil.SetDword(Cdm, "SystemPaneSuggestionsEnabled", 0);
                    bool b = RegistryUtil.SetDword(Cdm, "SubscribedContent-338388Enabled", 0);
                    return (a && b, (a && b) ? "已关闭通知中心推荐" : "部分写入失败");
                }
                else
                {
                    bool a = RegistryUtil.DeleteValue(Cdm, "SystemPaneSuggestionsEnabled");
                    bool b = RegistryUtil.DeleteValue(Cdm, "SubscribedContent-338388Enabled");
                    BackupService.Instance.Clear("privacy.notifads");
                    return (a && b, "已恢复通知推荐");
                }
            }
        },
        new FeatureViewModel
        {
            Id = "privacy.tips",
            Title = "关闭“开始使用”的提示与建议",
            Risk = "🟢",
            Description = "关闭系统设置里的“获取使用提示和技巧”等推广内容。",
            RegistryNote = $@"{Cdm}\SoftLandingEnabled / SubscribedContent-310093Enabled",
            State = () => RegistryUtil.GetDword(Cdm, "SoftLandingEnabled") == 0,
            Apply = on =>
            {
                if (on)
                {
                    BackupService.Instance.Record("privacy.tips", "使用提示", Cdm, "SoftLandingEnabled");
                    BackupService.Instance.Record("privacy.tips", "使用提示", Cdm, "SubscribedContent-310093Enabled");
                    bool a = RegistryUtil.SetDword(Cdm, "SoftLandingEnabled", 0);
                    bool b = RegistryUtil.SetDword(Cdm, "SubscribedContent-310093Enabled", 0);
                    return (a && b, (a && b) ? "已关闭使用提示与建议" : "部分写入失败");
                }
                else
                {
                    bool a = RegistryUtil.DeleteValue(Cdm, "SoftLandingEnabled");
                    bool b = RegistryUtil.DeleteValue(Cdm, "SubscribedContent-310093Enabled");
                    BackupService.Instance.Clear("privacy.tips");
                    return (a && b, "已恢复使用提示");
                }
            }
        }
    };

    private static Func<bool, (bool, string)> SetDwordFeature(string fid, string key, string name,
        int onVal, string onMsg, string offMsg)
    {
        return on =>
        {
            if (on)
            {
                BackupService.Instance.Record(fid, name, key, name);
                bool ok = RegistryUtil.SetDword(key, name, onVal);
                return (ok, ok ? onMsg : "写入失败，请检查权限");
            }
            else
            {
                bool ok = RegistryUtil.DeleteValue(key, name);
                BackupService.Instance.Clear(fid);
                return (ok, ok ? offMsg : "还原失败");
            }
        };
    }
}
