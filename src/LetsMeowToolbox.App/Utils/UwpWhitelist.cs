namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// UWP 卸载白名单与系统核心保护名单。
/// 只允许卸载白名单内的预装应用；系统核心 UWP 一律禁止操作。
/// </summary>
public static class UwpWhitelist
{
    /// <summary>允许用户卸载的预装应用（PackageFamilyName 前缀/全名）。白名单之外的不可卸载。</summary>
    public static readonly string[] RemovableFamilies =
    {
        "Microsoft.BingWeather_",
        "Microsoft.GetHelp_",
        "Microsoft.Getstarted_",
        "Microsoft.Messaging_",
        "Microsoft.Microsoft3DViewer_",
        "Microsoft.MicrosoftOfficeHub_",
        "Microsoft.MicrosoftSolitaireCollection_",
        "Microsoft.MixedReality.Portal_",
        "Microsoft.Office.OneNote_",
        "Microsoft.People_",
        "Microsoft.SkypeApp_",
        "Microsoft.Todos_",
        "Microsoft.WindowsFeedbackHub_",
        "Microsoft.YourPhone_",
        "Microsoft.ZuneMusic_",
        "Microsoft.ZuneVideo_",
        "Clipchamp.Clipchamp_",
        "SpotifyAB.SpotifyMusic_",
        "DolbyLaboratories.DolbyAccess_",
        "Microsoft.Wallet_",
        "Microsoft.XboxGamingOverlay_",
        "Microsoft.XboxGameCallableUI_",
        "Microsoft.MicrosoftNews_",
        "Microsoft.MicrosoftWeather_",
        "Microsoft.MicrosoftTips_",
        "MicrosoftTeams_",            // 个人版 Teams（企业版不带 _ 前缀结尾同样被覆盖）
        "Microsoft.Advertising.Xaml_",
        "Microsoft.MSPaint_",         // 新版画图
        "Microsoft.549981C3F5F10_"    // Cortana
    };

    /// <summary>系统核心 UWP，禁止卸载（名称包含即保护）。</summary>
    public static readonly string[] CoreProtectedFragments =
    {
        "Microsoft.WindowsStore_",        // 应用商店
        "Microsoft.WindowsCalculator_",   // 计算器
        "Microsoft.Windows.Photos_",      // 照片
        "Microsoft.WindowsNotepad_",      // 记事本
        "Microsoft.ScreenSketch_",        // 截图工具
        "Microsoft.Paint_",               // 旧版画图
        "Microsoft.ExplorerControl_",     // 桌面系统
        "Microsoft.StartMenuExperienceHost_",
        "Microsoft.Windows.StartMenuExperienceHost_",
        "Microsoft.Windows.ShellExperienceHost_",
        "Microsoft.Windows.Search_",
        "Microsoft.Windows.Cortana_",
        "Microsoft.WindowsAppRuntime.",
        "Microsoft.DesktopAppInstaller_"  // winget
    };

    /// <summary>判断某个包是否在白名单（可卸载）。</summary>
    public static bool IsRemovable(string packageName)
    {
        if (IsCoreProtected(packageName)) return false;
        return RemovableFamilies.Any(f => packageName.StartsWith(f, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>判断某个包是否为核心保护项。</summary>
    public static bool IsCoreProtected(string packageName)
    {
        return CoreProtectedFragments.Any(f =>
            packageName.StartsWith(f, StringComparison.OrdinalIgnoreCase) ||
            packageName.Contains(f, StringComparison.OrdinalIgnoreCase));
    }
}
