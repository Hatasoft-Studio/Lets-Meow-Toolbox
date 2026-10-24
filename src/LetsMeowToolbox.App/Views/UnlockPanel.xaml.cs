using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Modules.Unlock;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

public partial class UnlockPanel : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<UnlockProcessViewModel> Occupiers { get; } = new();

    public UnlockPanel()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>由外部（如卸载联动）设置要解除占用的路径。</summary>
    public void SetPath(string path)
    {
        TxtPath.Text = path;
        OnFind(this, new RoutedEventArgs());
    }

    private void OnBrowse(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Title = "选择要解除占用的文件", CheckFileExists = true };
        if (dlg.ShowDialog(Window.GetWindow(this)) == true)
            TxtPath.Text = dlg.FileName;
    }

    private void OnFind(object sender, RoutedEventArgs e)
    {
        var path = TxtPath.Text.Trim();
        if (string.IsNullOrEmpty(path)) { Ui.Warning("请先输入文件 / 文件夹路径"); return; }

        Occupiers.Clear();
        foreach (var p in UnlockService.FindOccupiers(path))
            Occupiers.Add(p);

        if (ChkDeep.IsChecked == true)
        {
            var probe = UnlockService.DeepProbe(path);
            TxtHint.Text = probe == null
                ? "深度扫描：文件当前可独占访问（未被占用）"
                : $"深度扫描：{probe}";
        }

        if (Occupiers.Count == 0)
        {
            TxtHint.Text = "未找到占用该路径的进程（或需要管理员权限才能识别服务）";
            LogService.Instance.Info($"解除占用：{path} 未发现占用进程。");
            return;
        }
        TxtHint.Text = $"找到 {Occupiers.Count} 个占用进程";
        LogService.Instance.Info($"解除占用：{path} 发现 {Occupiers.Count} 个占用进程。");
    }

    private void OnEndProcess(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).Tag is not UnlockProcessViewModel vm) return;
        if (vm.IsService)
        {
            Ui.Warning($"“{vm.AppName}”是系统服务，请通过 services.msc 或卸载程序处理");
            return;
        }
        if (!Ui.Confirm(Window.GetWindow(this),
                $"确认结束进程“{vm.AppName}”（PID {vm.ProcessId}）吗？\n程序未保存的数据可能丢失。", "结束进程"))
            return;
        var (ok, msg) = UnlockService.CloseProcess(vm.ProcessId, vm.ProcessName, force: true);
        vm.Status = ok ? "已结束" : msg;
        if (ok)
        {
            LogService.Instance.Warn($"解除占用：已结束进程 {vm.AppName}（PID {vm.ProcessId}）。");
            Ui.Success($"已结束进程：{vm.AppName}");
            // 自动刷新占用列表
            OnFind(this, new RoutedEventArgs());
        }
        else
        {
            LogService.Instance.Error($"解除占用：结束进程 {vm.AppName} 失败：{msg}");
            Ui.Error(msg);
        }
    }

    private void OnScheduleRebootDelete(object sender, RoutedEventArgs e)
    {
        var path = TxtPath.Text.Trim();
        if (string.IsNullOrEmpty(path)) { Ui.Warning("请先输入路径"); return; }
        if (!Ui.Confirm(Window.GetWindow(this),
                $"将安排“{path}”在下次开机时删除。\n需要管理员权限，重启前请先退出占用它的程序。", "开机延迟删除"))
            return;
        if (!AdminService.IsAdmin)
        {
            if (!Ui.Confirm(Window.GetWindow(this), "该操作需要管理员权限，将以管理员身份重启程序后继续。", "权限提示"))
                return;
            AdminService.RelaunchAsAdmin();
            return;
        }
        var ok = UnlockService.ScheduleRebootDelete(path);
        if (ok)
        {
            LogService.Instance.Warn($"解除占用：已安排开机删除 {path}。");
            Ui.Success("已安排开机删除（重启后生效）");
        }
        else
        {
            LogService.Instance.Error($"解除占用：安排开机删除失败：{path}");
            Ui.Error("安排失败，请确认路径有效且以管理员身份运行");
        }
    }

    private void OnShowPending(object sender, RoutedEventArgs e)
    {
        var list = UnlockService.PendingDeletes();
        if (list.Count == 0) { Ui.Info("当前没有待重启删除的条目"); return; }
        var text = string.Join("\n", list);
        Ui.Show(Window.GetWindow(this), text, "待重启删除列表",
            MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
    }
}
