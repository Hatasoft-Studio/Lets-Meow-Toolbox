using System.IO;
using System.Reflection;
using LetsMeowToolbox.PluginAbstractions;

namespace LetsMeowToolbox.App.Services;

/// <summary>
/// 插件加载系统：扫描程序目录下 plugins\*.dll，动态加载实现了 IHighRiskPlugin 的插件。
/// 主程序不内嵌任何高危逻辑；未放置插件时，高危区为空并给出引导提示。
/// </summary>
public sealed class PluginLoader
{
    private readonly string _pluginsDir;
    private readonly List<IHighRiskPlugin> _plugins = new();

    public PluginLoader()
    {
        _pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
    }

    /// <summary>已加载的高危插件（无则空）。</summary>
    public IReadOnlyList<IHighRiskPlugin> Plugins => _plugins;

    /// <summary>重新扫描 plugins 目录并加载插件。</summary>
    public void Refresh()
    {
        _plugins.Clear();
        if (!Directory.Exists(_pluginsDir)) return;

        foreach (var dll in Directory.EnumerateFiles(_pluginsDir, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var asm = Assembly.LoadFrom(dll);
                foreach (var type in asm.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface) continue;
                    if (typeof(IHighRiskPlugin).IsAssignableFrom(type))
                    {
                        if (Activator.CreateInstance(type) is IHighRiskPlugin plugin)
                            _plugins.Add(plugin);
                    }
                }
            }
            catch
            {
                // 单个插件加载失败不影响其他插件与主程序
            }
        }
    }

    /// <summary>插件目录路径（供 UI 展示与引导）。</summary>
    public string PluginsDirectory => _pluginsDir;
}
