using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Modules.Advanced;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

public partial class AdvancedView : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<FeatureViewModel> Features { get; } = new();

    private bool _unlocked;
    public bool Unlocked
    {
        get => _unlocked;
        set { _unlocked = value; Raise(); }
    }

    private bool _showRegistryNote;
    public bool ShowRegistryNote
    {
        get => _showRegistryNote;
        set { _showRegistryNote = value; Raise(); }
    }

    public AdvancedView()
    {
        InitializeComponent();
        DataContext = this;

        ShowRegistryNote = SettingsService.Instance.ShowRegistryNote;
        SettingsService.Instance.Changed += () => ShowRegistryNote = SettingsService.Instance.ShowRegistryNote;

        FeatureHost.ConfirmOnRisk = true;
        foreach (var f in WindowsUpdateModule.CreateAll()) FeatureHost.Add(Features, f);
        Unlocked = false;
    }

    private void OnUnlock(object sender, RoutedEventArgs e)
        => Unlocked = true;

    private void OnLock(object sender, RoutedEventArgs e)
        => Unlocked = false;

    private void OnRestoreAll(object sender, RoutedEventArgs e)
    {
        if (!Ui.Confirm(Window.GetWindow(this),
                "将还原高级区全部设置（更新暂停/禁用等）到默认状态，确定继续吗？", "高级区一键还原"))
            return;
        var count = BackupService.Instance.RestoreAll();
        FeatureHost.RefreshStates(Features);
        LogService.Instance.Info($"高级区：一键还原 {count} 项设置。");
        Ui.Success($"已还原 {count} 项高级设置");
    }
}
