using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

/// <summary>鸣谢项目条目。</summary>
public sealed class CreditItem
{
    public required string Name { get; init; }
    public required string License { get; init; }
    public required string Usage { get; init; }
    public required string Repo { get; init; }
}

public partial class CreditsView : UserControl
{
    public ObservableCollection<CreditItem> Projects { get; } = new()
    {
        new CreditItem
        {
            Name = "HandyControl",
            License = "MIT",
            Usage = "本工具箱的整套现代化卡片 UI 与控件（Card、Dialog、Growl、主题切换）基于 HandyControl 构建。",
            Repo = "https://github.com/HandyOrg/HandyControl"
        },
        new CreditItem
        {
            Name = "Open-Shell（经典开始菜单 / 右键菜单思路）",
            License = "MIT",
            Usage = "本工具“切换经典 Win10 右键菜单”功能参考了 Open-Shell 社区对 Win11 右键菜单回归的注册表思路。",
            Repo = "https://github.com/Open-Shell/Open-Shell-Menu"
        },
        new CreditItem
        {
            Name = "Bulk Crap Uninstaller（BCU）",
            License = "GPL-3.0",
            Usage = "本工具“软件卸载 / 残留扫描”模块的设计思路参考 BCU 的卸载注册表读取与残留检测方案，为独立实现。",
            Repo = "https://github.com/Klocman/Bulk-Crap-Uninstaller"
        },
        new CreditItem
        {
            Name = "Win11Debloat",
            License = "MIT",
            Usage = "本工具预装 UWP 卸载白名单与“关闭广告/遥测/通知推荐”的注册表键位口径参考了 Win11Debloat 的公开清单。",
            Repo = "https://github.com/Raphire/Win11Debloat"
        },
        new CreditItem
        {
            Name = "Sophia Script",
            License = "CC-BY-NC-SA-4.0",
            Usage = "本工具部分系统设置（暂停更新、服务管理）的注册表 / sc 命令口径参考了 Sophia Script 的公开脚本。",
            Repo = "https://github.com/farag2/Sophia-Script"
        },
        new CreditItem
        {
            Name = "EZ-Mirror",
            License = "—",
            Usage = "EZ-Mirror为此项目提供了Github仓库加速下载服务，提升用户体验。",
            Repo = "https://mirror.hatasoft365.top/"
        }
    };

    public CreditsView()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>打开项目主页 / GitHub 仓库（鸣谢条目）。</summary>
    private void OnVisit(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is not string url || string.IsNullOrWhiteSpace(url)) return;
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            LogService.Instance.Info($"特别鸣谢：打开 {url}");
        }
        catch (Exception ex)
        {
            LogService.Instance.Error($"特别鸣谢：打开 {url} 失败：{ex.Message}");
            Ui.Error("无法打开浏览器访问该地址。");
        }
    }

    private void OnOpenRepo(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(AppInfo.RepoUrl) { UseShellExecute = true });
            LogService.Instance.Info("特别鸣谢：打开 GitHub 仓库页面。");
        }
        catch (Exception ex)
        {
            LogService.Instance.Error($"特别鸣谢：打开 GitHub 仓库失败：{ex.Message}");
            Ui.Error("无法打开浏览器访问仓库地址。");
        }
    }
}
