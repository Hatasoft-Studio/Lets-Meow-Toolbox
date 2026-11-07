using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

public partial class SettingsView : UserControl
{
    public bool ShowAnime
    {
        get => SettingsService.Instance.ShowAnime;
        set => SettingsService.Instance.ShowAnime = value;
    }

    public bool ShowRegistryNote
    {
        get => SettingsService.Instance.ShowRegistryNote;
        set => SettingsService.Instance.ShowRegistryNote = value;
    }

    public string RepoName => AppInfo.RepoName;
    public string RepoUrl => AppInfo.RepoUrl;

    public SettingsView()
    {
        InitializeComponent();
        DataContext = this;

        SetupLanguageSelector();
        SetupUpdateCard();

        TxtVersion.Text = Loc.Tf("版本：v{0}", AppInfo.Version);
        var os = OsVersionService.FriendlyName;
        TxtAbout.Text =
            Loc.Tf("支持系统：{0}（Win10/11 x64）", os) + "\n" +
            Loc.T("许可协议：GPL-3.0（GNU General Public License v3.0）") + "\n" +
            Loc.T("技术栈：WPF · .NET 8 · HandyControl（MIT）") + "\n\n" +
            Loc.T("本工具不含任何加壳/混淆逻辑，高危功能仅以外置插件 DLL 形式提供。");
    }

    /// <summary>语言卡片：当前语言 + 选择语言/安装语言包按钮。</summary>
    private void SetupLanguageSelector()
    {
        BtnSelectLanguage.Content = Loc.T("选择语言");
        BtnInstallLanguage.Content = Loc.T("安装语言包");
        TxtLanguageHint.Text = Loc.Tf("语言包目录：{0}。可安装第三方语言包，或选择已安装的语言。",
            LocalizationService.LanguageDirectory);
        UpdateCurrentLanguage();
    }

    private void UpdateCurrentLanguage()
    {
        var cur = LocalizationService.Instance.Current;
        TxtCurrentLanguage.Text = Loc.Tf("当前语言：{0}", cur?.DisplayName ?? "简体中文");
    }

    /// <summary>更新卡片：检查更新按钮文案。</summary>
    private void SetupUpdateCard()
    {
        BtnCheckUpdate.Content = Loc.T("检查更新");
        TxtUpdateStatus.Text = string.Empty;
    }

    /// <summary>检查更新：请求官方接口，比较版本号；若当前版本已停止支持则强制退出。</summary>
    private async void OnCheckUpdate(object sender, RoutedEventArgs e)
    {
        var owner = Window.GetWindow(this);
        var label = BtnCheckUpdate.Content;
        BtnCheckUpdate.IsEnabled = false;
        BtnCheckUpdate.Content = Loc.T("正在检查更新…");
        TxtUpdateStatus.Text = string.Empty;

        try
        {
            var result = await UpdateService.CheckAsync();

            if (!result.Success)
            {
                TxtUpdateStatus.Text = Loc.Tf("检查更新失败：{0}", result.Error);
                Ui.Warning(TxtUpdateStatus.Text);
                return;
            }

            if (result.Unsupported)
            {
                TxtUpdateStatus.Text = Loc.T("当前版本已停止支持，必须更新后才能继续使用。");
                UpdateDialog.ShowRequired(owner, result);
                LogService.Instance.Warn($"更新：当前版本 v{AppInfo.Version} 已停止支持，程序退出。");
                Application.Current.Shutdown(0);
                return;
            }

            if (result.HasUpdate)
            {
                TxtUpdateStatus.Text = Loc.Tf("发现新版本 v{0}", result.Info!.LatestVersion);
                UpdateDialog.ShowAvailable(owner, result);
            }
            else
            {
                TxtUpdateStatus.Text = Loc.Tf("当前已是最新版本（v{0}）", AppInfo.Version);
                Ui.Success(TxtUpdateStatus.Text);
            }
        }
        finally
        {
            BtnCheckUpdate.Content = label;
            BtnCheckUpdate.IsEnabled = true;
        }
    }

    private void OnOpenLanguageManager(object sender, RoutedEventArgs e)
    {
        LanguageManagerDialog.Show(Window.GetWindow(this));
        UpdateCurrentLanguage();
    }

    private void OnInstallLanguagePack(object sender, RoutedEventArgs e)
    {
        LanguageManagerDialog.InstallInteractive(Window.GetWindow(this));
        UpdateCurrentLanguage();
    }

    private void OnViewLicense(object sender, RoutedEventArgs e)
    {
        LicenseDialog.ShowInfo(Window.GetWindow(this));
    }

    private void OnOpenRepo(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(AppInfo.RepoUrl) { UseShellExecute = true });
            LogService.Instance.Info("关于：打开 GitHub 仓库页面。");
        }
        catch (Exception ex)
        {
            LogService.Instance.Error($"关于：打开 GitHub 仓库失败：{ex.Message}");
            Ui.Error(Loc.T("无法打开浏览器访问仓库地址。"));
        }
    }

    private void OnViewChangelog(object sender, RoutedEventArgs e)
    {
        ChangelogDialog.Show(Window.GetWindow(this));
    }

    private void OnResetAll(object sender, RoutedEventArgs e)
    {
        if (!Ui.Confirm(Window.GetWindow(this),
            Loc.T("确定要一键初始化全部内容吗？\n\n" +
                  "• 全部功能设置将还原到系统默认状态\n" +
                  "• 外观选项恢复默认（二次元装饰开启）\n" +
                  "• 清空全部配置预设\n\n" +
                  "此操作不可撤销，确定继续？"), Loc.T("⚠ 一键初始化")))
            return;

        // 1) 还原全部功能注册表设置
        int restored = BackupService.Instance.RestoreAll();

        // 2) 复位外观设置（保留已同意的协议）
        SettingsService.Instance.ResetToDefault();

        // 3) 清空配置预设
        ConfigService.Instance.Clear();

        LogService.Instance.Warn($"一键初始化完成：还原功能 {restored} 项，外观复位，预设清空。");
        Ui.Success(Loc.Tf("已还原 {0} 项设置，外观与预设已复位", restored));

        // 4) 重建全部页面以刷新开关状态
        (Window.GetWindow(this) as MainWindow)?.ReloadAllViews();
    }

    private void OnExportConfig(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Title = Loc.T("导出配置预设"),
            Filter = "Let's Meow 预设 (*.meowpreset)|*.meowpreset",
            FileName = $"meow-preset-{DateTime.Now:yyyyMMdd-HHmmss}.meowpreset"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;
        ConfigService.Instance.Export(dlg.FileName);
        LogService.Instance.Info($"配置：导出预设成功 → {dlg.FileName}");
        Ui.Success(Loc.Tf("预设已导出：{0}", dlg.FileName));
    }

    private void OnImportConfig(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = Loc.T("导入配置预设"),
            Filter = "Let's Meow 预设 (*.meowpreset)|*.meowpreset|所有文件 (*.*)|*.*"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;
        try
        {
            var count = ConfigService.Instance.Import(dlg.FileName);
            Ui.Success(Loc.Tf("已导入 {0} 项配置（各功能需重新点按开关生效）", count));
        }
        catch (Exception ex)
        {
            LogService.Instance.Error($"配置：导入预设失败：{ex.Message}");
            Ui.Error(Loc.Tf("导入失败：{0}", ex.Message));
        }
    }

    private void OnOpenLogs(object sender, RoutedEventArgs e)
    {
        var dir = LogService.Instance.LogDirectory;
        try
        {
            if (Directory.Exists(dir))
                Process.Start(new ProcessStartInfo("explorer.exe", dir) { UseShellExecute = true });
        }
        catch { Ui.Warning(Loc.T("无法打开日志目录")); }
    }
}
