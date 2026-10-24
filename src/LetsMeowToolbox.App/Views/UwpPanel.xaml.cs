using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Modules.Beginner;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

public partial class UwpPanel : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<UwpAppViewModel> Apps { get; } = new();

    public UwpPanel()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        Apps.Clear();
        var list = UwpCleanModule.List();
        foreach (var app in list) Apps.Add(app);
        var removable = list.Count(a => a.Removable);
        var core = list.Count(a => a.CoreProtected);
        TxtHint.Text = $"共 {list.Count} 个应用：可卸载 {removable}，系统核心保护 {core}（不可操作）";
        LogService.Instance.Info($"UWP 列表刷新：共 {list.Count} 个应用（可卸载 {removable}）。");
    }

    private async void OnUninstallSelected(object sender, RoutedEventArgs e)
    {
        var selected = Apps.Where(a => a.Removable && a.IsChecked).ToList();
        if (selected.Count == 0) { Ui.Warning("请先勾选要卸载的预装应用（仅白名单）"); return; }
        if (!Ui.Confirm(Window.GetWindow(this),
                $"确认卸载 {selected.Count} 个预装应用吗？\n已勾选的应用都在安全白名单内，系统核心应用不会被触碰。"))
            return;

        LogService.Instance.Warn($"UWP 卸载开始：共 {selected.Count} 个应用。");
        foreach (var app in selected)
        {
            TxtHint.Text = $"正在卸载：{app.DisplayName}…";
            var (ok, msg) = UwpCleanModule.Uninstall(app.PackageFullName);
            app.Status = ok ? "已卸载" : msg;
            LogService.Instance.Info($"UWP 卸载：{app.DisplayName} → {(ok ? "成功" : "失败：" + msg)}");
            await System.Threading.Tasks.Task.Delay(300);
        }
        Ui.Info("预装应用卸载处理完毕");
        OnRefresh(this, new RoutedEventArgs());
    }
}
