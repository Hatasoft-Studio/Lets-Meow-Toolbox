using System.Diagnostics;
using System.Windows;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

/// <summary>
/// 更新提示窗口：展示当前版本、最新版本、官方下载地址与更新内容。
/// 普通模式可选择「稍后再说」；强制模式（当前版本已停止支持）只能前往官网，关闭后程序将退出。
/// </summary>
public partial class UpdateDialog : Window
{
    private readonly string _downloadUrl;
    private bool _openSite;

    private UpdateDialog(UpdateCheckResult result, bool required)
    {
        InitializeComponent();

        var info = result.Info!;
        _downloadUrl = string.IsNullOrWhiteSpace(info.DownloadUrl)
            ? UpdateService.DefaultDownloadUrl
            : info.DownloadUrl;

        Title = required ? Loc.T("需要更新") : Loc.T("发现新版本");
        TxtTitle.Text = required
            ? Loc.T("此版本已停止支持")
            : Loc.Tf("发现新版本 v{0}", info.LatestVersion);
        TxtSubtitle.Text = required
            ? Loc.Tf("当前版本 v{0} 已停止支持，请更新到最新版本 v{1} 后继续使用。",
                AppInfo.Version, info.LatestVersion)
            : Loc.Tf("当前版本 v{0}，最新版本 v{1}。", AppInfo.Version, info.LatestVersion);

        TxtCurrent.Text = "v" + AppInfo.Version;
        TxtLatest.Text = "v" + info.LatestVersion;
        TxtUrl.Text = _downloadUrl;

        var changelog = (info.Changelog ?? string.Empty).Trim();
        TxtChangelog.Text = changelog.Length > 0
            ? changelog
            : Loc.T("暂无更新说明，请前往官网查看。");

        BtnGo.Content = Loc.T("前往官网获取新版本");
        BtnLater.Content = required ? Loc.T("退出程序") : Loc.T("稍后再说");
    }

    /// <summary>提示有新版本；返回用户是否选择前往官网。</summary>
    public static bool ShowAvailable(Window? owner, UpdateCheckResult result)
    {
        var dialog = new UpdateDialog(result, false);
        if (owner != null) dialog.Owner = owner;
        dialog.ShowDialog();
        return dialog._openSite;
    }

    /// <summary>强制更新提示；返回用户是否选择前往官网（无论结果如何，调用方都应退出程序）。</summary>
    public static bool ShowRequired(Window? owner, UpdateCheckResult result)
    {
        var dialog = new UpdateDialog(result, true);
        if (owner != null) dialog.Owner = owner;
        dialog.ShowDialog();
        return dialog._openSite;
    }

    private void OnGo(object sender, RoutedEventArgs e)
    {
        _openSite = true;
        try
        {
            Process.Start(new ProcessStartInfo(_downloadUrl) { UseShellExecute = true });
            LogService.Instance.Info($"更新：打开官方下载页面 {_downloadUrl}");
        }
        catch (Exception ex)
        {
            LogService.Instance.Warn($"更新：打开下载页面失败：{ex.Message}");
            Ui.Warning(Loc.T("无法打开浏览器访问该地址。"));
        }
        Close();
    }

    private void OnLater(object sender, RoutedEventArgs e) => Close();
}
