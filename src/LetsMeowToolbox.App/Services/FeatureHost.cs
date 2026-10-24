using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using LetsMeowToolbox.App.Models;

namespace LetsMeowToolbox.App.Services;

/// <summary>
/// 功能卡片托管：负责初始化状态、在用户切换开关时执行 Apply、并在一键还原后刷新状态。
/// </summary>
public static class FeatureHost
{
    private static readonly Dictionary<FeatureViewModel, PropertyChangedEventHandler> _handlers = new();
    private static bool _suppress;

    /// <summary>高级区开启“风险二次确认”（Risk 不为 🟢 的开关在切换前需确认）。</summary>
    public static bool ConfirmOnRisk { get; set; }

    /// <summary>添加一个功能卡片到集合（自动跳过 Win11-only 且当前非 Win11 的功能）。</summary>
    public static void Add(ObservableCollection<FeatureViewModel> coll, FeatureViewModel f)
    {
        if (f.Win11Only && !OsVersionService.IsWin11) return;
        f.IsOn = f.State?.Invoke() ?? false;
        PropertyChangedEventHandler h = (s, e) =>
        {
            if (IsSuppressing) return;
            if (e.PropertyName == nameof(FeatureViewModel.IsOn) && s is FeatureViewModel f)
                Apply(f);
        };
        f.PropertyChanged += h;
        _handlers[f] = h;
        coll.Add(f);
    }

    /// <summary>执行某个功能的启用/关闭（结果用 Growl 提示）。</summary>
    public static void Apply(FeatureViewModel f, Window? owner = null)
    {
        if (f.Apply == null) return;

        if (ConfirmOnRisk && f.Risk != "🟢")
        {
            if (!Ui.Confirm(owner,
                    $"{f.Title}\n\n{f.Description}\n\n确定要执行该操作吗？", "⚠ 风险确认"))
            {
                _suppress = true;
                f.IsOn = !f.IsOn;
                _suppress = false;
                return;
            }
        }

        var (ok, msg) = f.Apply(f.IsOn);
        LogService.Instance.Info($"功能操作：{f.Title} → {(f.IsOn ? "开启" : "关闭")}，结果={(ok ? "成功" : "失败")}：{msg}");
        if (ok) Ui.Success(msg); else Ui.Error(msg);
    }

    /// <summary>一键还原后刷新全部卡片状态（不触发 Apply）。</summary>
    public static void RefreshStates(IEnumerable<FeatureViewModel> coll)
    {
        foreach (var f in coll)
        {
            if (_handlers.TryGetValue(f, out var h)) f.PropertyChanged -= h;
            f.IsOn = f.State?.Invoke() ?? false;
            if (h != null) f.PropertyChanged += h;
        }
    }

    /// <summary>供 IsOn 变更处理器调用，避免在风险确认回退时再次触发 Apply。</summary>
    public static bool IsSuppressing => _suppress;
}
