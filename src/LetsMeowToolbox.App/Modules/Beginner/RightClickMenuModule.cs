using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Beginner;

/// <summary>🟢 功能1：Win11 切换经典 Win10 右键菜单。</summary>
public static class RightClickMenuModule
{
    private const string ClsidKey =
        @"HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32";

    public const string FeatureId = "newbie.rightclickmenu";

    public static FeatureViewModel Create()
    {
        return new FeatureViewModel
        {
            Id = FeatureId,
            Title = "切换经典 Win10 右键菜单",
            Risk = "🟢",
            RequiresAdmin = false,
            Win11Only = true, // 仅 Win11 需要此选项
            Description =
                "Win11 新右键菜单把“复制/粘贴/重命名”都折叠进了二级菜单，用起来慢半拍。\n" +
                "开启后恢复 Win10 那种一列到底的完整菜单，操作更顺手。",
            RegistryNote = "HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32",
            State = () => RegistryUtil.KeyExists(ClsidKey),
            Apply = Apply
        };
    }

    private static (bool, string) Apply(bool on)
    {
        if (on)
        {
            BackupService.Instance.Record(FeatureId, "经典右键菜单", ClsidKey, "");
            bool ok = RegistryUtil.SetString(ClsidKey, "", "");
            if (ok) CmdRunner.RestartExplorer();
            return (ok, ok ? "已启用经典右键菜单（资源管理器已重启）" : "写入注册表失败，请检查权限");
        }
        else
        {
            bool ok = RegistryUtil.DeleteSubKey(ClsidKey);
            if (ok) CmdRunner.RestartExplorer();
            BackupService.Instance.Clear(FeatureId);
            return (ok, ok ? "已恢复 Win11 默认右键菜单" : "还原失败，请检查权限");
        }
    }
}
