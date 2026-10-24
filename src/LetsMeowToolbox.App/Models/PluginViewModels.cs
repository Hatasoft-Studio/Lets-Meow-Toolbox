using System.Collections.ObjectModel;
using LetsMeowToolbox.PluginAbstractions;

namespace LetsMeowToolbox.App.Models;

/// <summary>高危插件展示模型。</summary>
public sealed class PluginViewModel : ObservableObject
{
    public IHighRiskPlugin Plugin { get; init; } = null!;
    public string Name => Plugin.PluginName;
    public string Description => Plugin.Description;
    public string Credit => Plugin.Credit;
    public ObservableCollection<PluginActionViewModel> Actions { get; } = new();
}

/// <summary>插件内单个高危操作展示模型。</summary>
public sealed class PluginActionViewModel : ObservableObject
{
    public IHighRiskAction Action { get; init; } = null!;
    public string Title => Action.Title;
    public string Description => Action.Description;
    public string Warning => Action.Warning;
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
