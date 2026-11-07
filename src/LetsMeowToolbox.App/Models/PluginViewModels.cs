using System.Collections.ObjectModel;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.PluginAbstractions;

namespace LetsMeowToolbox.App.Models;

/// <summary>高危插件展示模型。</summary>
public sealed class PluginViewModel : ObservableObject
{
    public IHighRiskPlugin Plugin { get; init; } = null!;
    public string Name => Loc.T(Plugin.PluginName);
    public string Description => Loc.T(Plugin.Description);
    public string Credit => Loc.T(Plugin.Credit);
    public ObservableCollection<PluginActionViewModel> Actions { get; } = new();
}

/// <summary>插件内单个高危操作展示模型。</summary>
public sealed class PluginActionViewModel : ObservableObject
{
    public IHighRiskAction Action { get; init; } = null!;
    public string Title => Loc.T(Action.Title);
    public string Description => Loc.T(Action.Description);
    public string Warning => Loc.T(Action.Warning);
    public string RegistryNote => Action.RegistryNote;
    public bool RequiresAdmin => Action.RequiresAdmin;
    public string Risk => "🔴";

    private bool _applied;
    public bool Applied
    {
        get => _applied;
        set => Set(ref _applied, value);
    }
}
