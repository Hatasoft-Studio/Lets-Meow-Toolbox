using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

public partial class HighRiskView : UserControl
{
    private readonly PluginLoader _loader = new();
    public ObservableCollection<PluginViewModel> Plugins { get; } = new();

    public HighRiskView()
    {
        InitializeComponent();
        PluginList.ItemsSource = Plugins;
        Refresh();
    }

    public void Refresh()
    {
        Plugins.Clear();
        _loader.Refresh();
        foreach (var plugin in _loader.Plugins)
        {
            var vm = new PluginViewModel { Plugin = plugin };
            foreach (var action in plugin.Actions)
            {
                var avm = new PluginActionViewModel { Action = action };
                try { avm.Applied = action.IsApplied(); } catch { avm.Applied = false; }
                vm.Actions.Add(avm);
            }
            Plugins.Add(vm);
        }
        EmptyCard.Visibility = Plugins.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TxtPluginsDir.Text = Loc.Tf("插件目录：{0}", _loader.PluginsDirectory);
    }

    private void OnApplyAction(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is not PluginActionViewModel vm) return;

        if (!AdminService.IsAdmin)
        {
            if (Ui.Confirm(Window.GetWindow(this),
                    Loc.T("高危操作需要管理员权限，将以管理员身份重启程序后继续。"), Loc.T("权限提示")))
                AdminService.RelaunchAsAdmin();
            return;
        }

        var text = Loc.Tf("【高危】{0}", vm.Title) + $"\n\n{vm.Warning}\n\n" + Loc.T("确定要启用吗？");
        if (!Ui.Confirm(Window.GetWindow(this), text, Loc.T("⚠ 高危操作确认"))) return;

        bool ok;
        string msg = Loc.T("执行完成");
        try { ok = vm.Action.Apply(); }
        catch (Exception ex) { ok = false; msg = ex.Message; }

        vm.Applied = vm.Action.IsApplied();
        LogService.Instance.Warn($"高危操作：{vm.Title} → 启用结果={(ok ? "成功" : "失败")}：{msg}");
        if (ok) Ui.Success(Loc.Tf("{0}：已启用", vm.Title));
        else Ui.Error(Loc.Tf("{0} 执行失败：{1}", vm.Title, msg));
    }

    private void OnRestoreAction(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is not PluginActionViewModel vm) return;
        if (!AdminService.IsAdmin)
        {
            if (Ui.Confirm(Window.GetWindow(this), Loc.T("还原需要管理员权限，将以管理员身份重启程序后继续。"), Loc.T("权限提示")))
                AdminService.RelaunchAsAdmin();
            return;
        }
        if (!Ui.Confirm(Window.GetWindow(this), Loc.Tf("确认还原“{0}”吗？", vm.Title), Loc.T("还原确认"))) return;

        bool ok;
        string msg = Loc.T("执行完成");
        try { ok = vm.Action.Restore(); }
        catch (Exception ex) { ok = false; msg = ex.Message; }

        vm.Applied = vm.Action.IsApplied();
        LogService.Instance.Warn($"高危操作：{vm.Title} → 还原结果={(ok ? "成功" : "失败")}：{msg}");
        if (ok) Ui.Success(Loc.Tf("{0}：已还原", vm.Title));
        else Ui.Error(Loc.Tf("{0} 还原失败：{1}", vm.Title, msg));
    }
}
