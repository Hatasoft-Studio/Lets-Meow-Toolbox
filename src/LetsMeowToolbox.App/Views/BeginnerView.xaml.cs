using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Modules.Beginner;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Views;

public partial class BeginnerView : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<FeatureViewModel> Features { get; } = new();

    private bool _showRegistryNote;
    public bool ShowRegistryNote
    {
        get => _showRegistryNote;
        set { _showRegistryNote = value; Raise(); }
    }

    public BeginnerView()
    {
        InitializeComponent();
        DataContext = this;

        ShowRegistryNote = SettingsService.Instance.ShowRegistryNote;
        SettingsService.Instance.Changed += () => ShowRegistryNote = SettingsService.Instance.ShowRegistryNote;

        BuildFeatures();
    }

    private void BuildFeatures()
    {
        Features.Clear();
        FeatureHost.Add(Features, RightClickMenuModule.Create());
        foreach (var f in TaskbarExplorerModule.CreateAll()) FeatureHost.Add(Features, f);
        foreach (var f in PrivacyAdsModule.CreateAll()) FeatureHost.Add(Features, f);
        foreach (var f in OptimizeModule.CreateAll()) FeatureHost.Add(Features, f);
    }

    private void OnRestoreAll(object sender, RoutedEventArgs e)
    {
        if (!Ui.Confirm(Window.GetWindow(this),
                "将还原新手区全部设置到系统默认状态。\n已启用过的开关都会恢复，确定继续吗？", "一键还原"))
            return;
        var count = BackupService.Instance.RestoreAll();
        FeatureHost.RefreshStates(Features);
        CmdRunner.RestartExplorer();
        LogService.Instance.Info($"新手区：一键还原 {count} 项设置。");
        Ui.Success($"已还原 {count} 项设置");
    }
}
