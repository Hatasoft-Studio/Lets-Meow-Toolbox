using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Beginner;

/// <summary>🟢 功能2：任务栏、资源管理器外观微调。</summary>
public static class TaskbarExplorerModule
{
    private const string Adv = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

    public static IReadOnlyList<FeatureViewModel> CreateAll() => new[]
    {
        new FeatureViewModel
        {
            Id = "newbie.taskbar.alignleft",
            Title = "任务栏图标靠左",
            Risk = "🟢",
            Description = "Win11 默认任务栏图标居中，把它改回靠左，接近 Win10 的习惯。",
            RegistryNote = $@"{Adv}\TaskbarAl",
            // TaskbarAl：0 = 靠左；1 = 居中（默认）。键缺失（null）等同居中。
            State = () => RegistryUtil.GetDword(Adv, "TaskbarAl") == 0,
            Apply = ApplyDword(Adv, "TaskbarAl", 0, 1, "任务栏图标已靠左", "已恢复任务栏居中")
        },
        new FeatureViewModel
        {
            Id = "newbie.taskbar.hidetaskview",
            Title = "隐藏“任务视图”按钮",
            Risk = "🟢",
            Win11Only = true,
            Description = "任务栏上那个显示所有窗口缩略图的按钮，用不到就藏起来，更清爽。",
            RegistryNote = $@"{Adv}\ShowTaskViewButton",
            State = () => RegistryUtil.GetDword(Adv, "ShowTaskViewButton") == 0,
            Apply = ApplyDword(Adv, "ShowTaskViewButton", 0, 1, "已隐藏任务视图按钮", "已恢复显示")
        },
        new FeatureViewModel
        {
            Id = "newbie.explorer.openthispc",
            Title = "打开“此电脑”而不是“主页”",
            Risk = "🟢",
            Description = "Win11 资源管理器默认打开“主页”。改成默认打开“此电脑”，文件找起来更直接。",
            RegistryNote = $@"{Adv}\LaunchTo",
            State = () => RegistryUtil.GetDword(Adv, "LaunchTo") == 1,
            Apply = ApplyDword(Adv, "LaunchTo", 1, 0, "已默认打开“此电脑”", "已恢复默认“主页”")
        },
        new FeatureViewModel
        {
            Id = "newbie.explorer.hideoneda",
            Title = "导航窗格隐藏 OneDrive 提示",
            Risk = "🟢",
            Description = "资源管理器左侧不再显示 OneDrive 的同步状态提示，界面更干净。",
            RegistryNote = $@"{Adv}\ShowSyncProviderNotifications",
            State = () => RegistryUtil.GetDword(Adv, "ShowSyncProviderNotifications") == 0,
            Apply = ApplyDword(Adv, "ShowSyncProviderNotifications", 0, 1, "已隐藏 OneDrive 同步提示", "已恢复显示")
        }
    };

    private static Func<bool, (bool, string)> ApplyDword(string key, string name, int on, int off,
        string onMsg, string offMsg)
    {
        return enable =>
        {
            string fid = "newbie." + name.ToLowerInvariant();
            if (enable)
            {
                BackupService.Instance.Record(fid, name, key, name);
                bool ok = RegistryUtil.SetDword(key, name, on);
                return (ok, ok ? onMsg : "写入失败，请检查权限");
            }
            else
            {
                bool ok = RegistryUtil.SetDword(key, name, off);
                BackupService.Instance.Clear(fid);
                return (ok, ok ? offMsg : "还原失败");
            }
        };
    }
}
