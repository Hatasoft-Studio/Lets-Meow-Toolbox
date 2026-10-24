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

        TxtVersion.Text = $"版本：v{AppInfo.Version}";
        var os = OsVersionService.FriendlyName;
        TxtAbout.Text =
            $"支持系统：{os}（Win10/11 x64）\n" +
            $"许可协议：GPL-3.0（GNU General Public License v3.0）\n" +
            $"技术栈：WPF · .NET 8 · HandyControl（MIT）\n\n" +
            "本工具不含任何加壳/混淆逻辑，高危功能仅以外置插件 DLL 形式提供。";
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
            Ui.Error("无法打开浏览器访问仓库地址。");
        }
    }

    private void OnViewChangelog(object sender, RoutedEventArgs e)
    {
        ChangelogDialog.Show(Window.GetWindow(this));
    }

    private void OnResetAll(object sender, RoutedEventArgs e)
    {
        if (!Ui.Confirm(Window.GetWindow(this),
            "确定要一键初始化全部内容吗？\n\n" +
            "• 全部功能设置将还原到系统默认状态\n" +
            "• 外观选项恢复默认（二次元装饰开启）\n" +
            "• 清空全部配置预设\n\n" +
            "此操作不可撤销，确定继续？", "⚠ 一键初始化"))
            return;

        // 1) 还原全部功能注册表设置
        int restored = BackupService.Instance.RestoreAll();

        // 2) 复位外观设置（保留已同意的协议）
        SettingsService.Instance.ResetToDefault();

        // 3) 清空配置预设
        ConfigService.Instance.Clear();

        LogService.Instance.Warn($"一键初始化完成：还原功能 {restored} 项，外观复位，预设清空。");
        Ui.Success($"已还原 {restored} 项设置，外观与预设已复位");

        // 4) 重建全部页面以刷新开关状态
        (Window.GetWindow(this) as MainWindow)?.ReloadAllViews();
    }

    private void OnExportConfig(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Title = "导出配置预设",
            Filter = "Let's Meow 预设 (*.meowpreset)|*.meowpreset",
            FileName = $"meow-preset-{DateTime.Now:yyyyMMdd-HHmmss}.meowpreset"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;
        ConfigService.Instance.Export(dlg.FileName);
        LogService.Instance.Info($"配置：导出预设成功 → {dlg.FileName}");
        Ui.Success($"预设已导出：{dlg.FileName}");
    }

    private void OnImportConfig(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "导入配置预设",
            Filter = "Let's Meow 预设 (*.meowpreset)|*.meowpreset|所有文件 (*.*)|*.*"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;
        try
        {
            var count = ConfigService.Instance.Import(dlg.FileName);
            Ui.Success($"已导入 {count} 项配置（各功能需重新点按开关生效）");
        }
        catch (Exception ex)
        {
            LogService.Instance.Error($"配置：导入预设失败：{ex.Message}");
            Ui.Error($"导入失败：{ex.Message}");
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
        catch { Ui.Warning("无法打开日志目录"); }
    }
}
