using System.IO;
using System.Windows;
using System.Windows.Threading;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.App.Views;

namespace LetsMeowToolbox.App;

public partial class App : Application
{
    private string _logFile = string.Empty;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 全局日志单例初始化（先于一切，供后续所有操作记录）
        LogService.Instance.Info("======== Let's Meow Toolbox 启动 ========");
        _logFile = LogService.Instance.LogFilePath;

        // 全局异常捕获：任何启动/运行时错误都弹窗显示并落日志，而不是“无法打开”静默失败
        DispatcherUnhandledException += OnDispatcherUnhandled;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex) WriteCrash(ex);
        };

        try
        {
            // 先安装主题画刷，再建主窗口
            Services.ThemeService.InstallDefault();
            // 主题统一由“设置变更”驱动，任何入口切换都会实际换肤
            SettingsService.Instance.Changed += () =>
                Services.ThemeService.Apply(SettingsService.Instance.ShowAnime);
            Services.ThemeService.Apply(SettingsService.Instance.ShowAnime);

            // 首次启动：必须同意 GPL-3.0 协议才能继续；不同意则直接退出，不创建主窗口
            if (!SettingsService.Instance.LicenseAgreed)
            {
                if (!LicenseDialog.ShowForAgree())
                {
                    LogService.Instance.Warn("用户未同意 GPL-3.0 协议，程序退出。");
                    Shutdown(0);
                    return;
                }
                SettingsService.Instance.LicenseAgreed = true;
                LogService.Instance.Info("用户已同意 GPL-3.0 协议（首次）。");
            }

            var main = new MainWindow();
            main.Show();
            LogService.Instance.Info("主窗口已创建并显示。");
        }
        catch (Exception ex)
        {
            WriteCrash(ex);
            MessageBox.Show(
                "Let's Meow Toolbox 启动时发生错误：\n\n" + ex.Message +
                "\n\n详细信息已写入：\n" + _logFile,
                "启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LogService.Instance.Info("======== 程序退出 ========");
        LogService.Instance.Dispose();
        base.OnExit(e);
    }

    private void OnDispatcherUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        WriteCrash(e.Exception);
        MessageBox.Show(
            "运行过程中发生错误：\n\n" + e.Exception.Message +
            "\n\n详细信息已写入：\n" + _logFile,
            "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    private void WriteCrash(Exception ex)
    {
        LogService.Instance.Error("异常/崩溃：\n" + ex);
    }
}
