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
        TxtPluginsDir.Text = $"插件目录：{_loader.PluginsDirectory}";
    }

    private void OnApplyAction(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is not PluginActionViewModel vm) return;

        if (!AdminService.IsAdmin)
        {
            if (Ui.Confirm(Window.GetWindow(this),
                    "高危操作需要管理员权限，将以管理员身份重启程序后继续。", "权限提示"))
                AdminService.RelaunchAsAdmin();
            return;
        }

        var text = $"【高危】{vm.Title}\n\n{vm.Warning}\n\n确定要启用吗？";
        if (!Ui.Confirm(Window.GetWindow(this), text, "⚠ 高危操作确认")) return;

        bool ok;
        string msg = "执行完成";
        try { ok = vm.Action.Apply(); }
        catch (Exception ex) { ok = false; msg = ex.Message; }

        vm.Applied = vm.Action.IsApplied();
        LogService.Instance.Warn($"高危操作：{vm.Title} → 启用结果={(ok ? "成功" : "失败")}：{msg}");
        if (ok) Ui.Success($"{vm.Title}：已启用");
        else Ui.Error($"{vm.Title} 执行失败：{msg}");
    }

    private void OnRestoreAction(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is not PluginActionViewModel vm) return;
        if (!AdminService.IsAdmin)
        {
            if (Ui.Confirm(Window.GetWindow(this), "还原需要管理员权限，将以管理员身份重启程序后继续。", "权限提示"))
                AdminService.RelaunchAsAdmin();
            return;
        }
        if (!Ui.Confirm(Window.GetWindow(this), $"确认还原“{vm.Title}”吗？", "还原确认")) return;

        bool ok;
        string msg = "执行完成";
        try { ok = vm.Action.Restore(); }
        catch (Exception ex) { ok = false; msg = ex.Message; }

        vm.Applied = vm.Action.IsApplied();
        LogService.Instance.Warn($"高危操作：{vm.Title} → 还原结果={(ok ? "成功" : "失败")}：{msg}");
        if (ok) Ui.Success($"{vm.Title}：已还原");
        else Ui.Error($"{vm.Title} 还原失败：{msg}");
    }
}
