using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Modules.Uninstaller;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

public partial class UninstallerPanel : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<UninstallEntry> Programs { get; } = new();
    public ObservableCollection<ResidueCandidate> Residues { get; } = new();

    private bool _hasResidue;
    public bool HasResidue
    {
        get => _hasResidue;
        set { _hasResidue = value; Raise(); }
    }

    /// <summary>解除占用联动：请求打开占用扫描面板。</summary>
    public event Action<string>? RequestUnlock;

    public UninstallerPanel()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        Programs.Clear();
        Residues.Clear();
        HasResidue = false;
        foreach (var item in UninstallerService.Enumerate())
            Programs.Add(item);
        TxtHint.Text = $"共读取到 {Programs.Count} 个程序";
        LogService.Instance.Info($"卸载程序：列表刷新，共 {Programs.Count} 个程序。");
    }

    private async void OnUninstallSelected(object sender, RoutedEventArgs e)
    {
        var selected = Programs.Where(p => p.IsChecked).ToList();
        if (selected.Count == 0) { Ui.Warning("请先勾选要卸载的程序"); return; }
        if (selected.Count > 1 && !Ui.Confirm(Window.GetWindow(this),
                $"将依次卸载 {selected.Count} 个程序，是否继续？"))
            return;

        LogService.Instance.Warn($"卸载程序：开始卸载 {selected.Count} 个程序。");
        foreach (var entry in selected)
        {
            TxtHint.Text = $"正在卸载：{entry.DisplayName}…";
            if (!Ui.Confirm(Window.GetWindow(this),
                    $"确认卸载“{entry.DisplayName}”吗？\n\n将调用程序自带的卸载程序。", "卸载确认"))
            {
                entry.Status = "已取消";
                LogService.Instance.Info($"卸载程序：{entry.DisplayName} 已取消。");
                continue;
            }
            // 后台执行，避免阻塞界面；完成后回 UI 线程更新状态
            var (ok, msg) = await System.Threading.Tasks.Task.Run(() => UninstallerService.RunUninstaller(entry));
            entry.Status = ok ? "卸载完成" : $"卸载返回异常：{msg}";
            LogService.Instance.Info($"卸载程序：{entry.DisplayName} → {(ok ? "卸载完成" : "异常：" + msg)}");
            await System.Threading.Tasks.Task.Delay(300);
        }
        Ui.Info("本轮卸载处理完毕，可点击“刷新列表”查看最新状态");
    }

    private async void OnScanResidue(object sender, RoutedEventArgs e)
    {
        var selected = Programs.Where(p => p.IsChecked).ToList();
        if (selected.Count == 0) { Ui.Warning("请先勾选要扫描残留的程序"); return; }

        TxtHint.Text = "正在扫描残留…";
        Residues.Clear();
        var found = await System.Threading.Tasks.Task.Run(() =>
        {
            var list = new List<ResidueCandidate>();
            foreach (var entry in selected)
                list.AddRange(ResidueScanner.Scan(entry));
            return list;
        });
        foreach (var cand in found) Residues.Add(cand);
        HasResidue = Residues.Count > 0;
        foreach (var p in selected) p.HasResidue = true;
        TxtHint.Text = HasResidue ? $"扫描到 {Residues.Count} 处可能残留（默认不勾选）" : "未发现明显残留";
        LogService.Instance.Info($"残留扫描：{selected.Count} 个程序，发现 {Residues.Count} 处可能残留。");
    }

    private void OnCleanResidue(object sender, RoutedEventArgs e)
    {
        var selected = Residues.Where(r => r.IsChecked).ToList();
        if (selected.Count == 0) { Ui.Warning("请先勾选要清理的残留项"); return; }
        if (!Ui.Confirm(Window.GetWindow(this),
                $"将清理 {selected.Count} 处残留（文件/注册表）。\n清理不可撤销，请确认无误后再继续。", "残留清理确认"))
            return;

        var (cleaned, failed) = ResidueScanner.Clean(selected);
        foreach (var c in selected) Residues.Remove(c);
        HasResidue = Residues.Count > 0;
        LogService.Instance.Warn($"残留清理：清理 {cleaned} 项，失败 {failed} 项。");
        Ui.Info(failed > 0 ? $"已清理 {cleaned} 项，{failed} 项失败" : $"已清理 {cleaned} 项残留");
    }

    private void OnUnlock(object sender, RoutedEventArgs e)
    {
        var target = Residues.FirstOrDefault(r => r.IsChecked)?.Target;
        if (string.IsNullOrWhiteSpace(target))
            target = Programs.FirstOrDefault(p => p.IsChecked && !string.IsNullOrWhiteSpace(p.InstallLocation))?.InstallLocation;
        if (string.IsNullOrWhiteSpace(target))
        { Ui.Warning("请先勾选一个残留项（或安装目录），再使用“解除占用”"); return; }
        RequestUnlock?.Invoke(target);
    }

    private void OnExportReport(object sender, RoutedEventArgs e)
    {
        var rows = new List<(string, string)>();
        foreach (var p in Programs)
            rows.Add(($"{p.DisplayName} ({p.DisplayVersion})", string.IsNullOrEmpty(p.Status) ? "未处理" : p.Status));
        if (Residues.Count > 0)
        {
            rows.Add(("—— 残留扫描 ——", ""));
            foreach (var r in Residues)
                rows.Add(($"{r.Type}: {r.Target}", r.IsChecked ? "已勾选清理" : "未勾选"));
        }
        var file = ReportWriter.Export("软件卸载 / 清理报告", rows);
        Ui.Success($"报告已导出：{file}");
    }
}
