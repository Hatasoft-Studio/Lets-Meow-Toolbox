using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace LetsMeowToolbox.App.Services;

/// <summary>
/// 单个语言包（.meowlang）。源语言固定为简体中文，文件内容为 JSON：
/// id / name / author / version / source 元数据，以及 strings（简体中文原文 → 目标语言译文）。
/// </summary>
public sealed class LanguagePack
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    /// <summary>作者官网（与作者一起用于判定是否官方语言包）。</summary>
    public string AuthorWebsite { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Source { get; set; } = "zh-Hans";
    /// <summary>语言类型（如“英语”“繁体中文”）。</summary>
    public string LanguageType { get; set; } = string.Empty;
    /// <summary>适用的主程序版本范围，如 ">=0.2.0"、"*"。</summary>
    public string AppVersion { get; set; } = "*";
    /// <summary>语言包简介。</summary>
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Strings { get; set; } = new();

    /// <summary>语言包文件路径（内嵌的简体中文为空）。</summary>
    [JsonIgnore]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>是否为程序内嵌语言（简体中文）。</summary>
    [JsonIgnore]
    public bool BuiltIn { get; set; }

    /// <summary>界面显示文本。</summary>
    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Id : Name;

    /// <summary>是否为官方语言包（作者与官网都必须匹配）。</summary>
    [JsonIgnore]
    public bool IsOfficial => LanguagePackRules.IsOfficial(Author, AuthorWebsite);

    /// <summary>语言类型显示文本。</summary>
    [JsonIgnore]
    public string TypeText => string.IsNullOrWhiteSpace(LanguageType) ? DisplayName : LanguageType;
}

/// <summary>
/// 多语言服务：
/// • 简体中文（源语言）直接内嵌在程序本体，无需语言包；
/// • 启动时扫描程序目录下的 /lang 文件夹，加载所有 *.meowlang 语言包；
/// • 运行时把界面上的简体中文原文替换为目标语言译文。
/// </summary>
public sealed class LocalizationService
{
    private static readonly Lazy<LocalizationService> _instance = new(() => new LocalizationService());
    public static LocalizationService Instance => _instance.Value;

    private const string BuiltinId = "zh-Hans";

    private readonly List<LanguagePack> _languages = new();
    private readonly object _lock = new();
    private LanguagePack? _current;
    private bool _loadedHandlerRegistered;

    // 记录控件被本地化前的原文，切换语言时可反复翻译，不会叠加。
    private static readonly ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, string>> Originals = new();

    /// <summary>语言发生切换（供主窗口重建界面）。</summary>
    public event Action? LanguageChanged;

    private LocalizationService() { }

    /// <summary>语言包目录：程序目录下的 lang 文件夹。</summary>
    public static string LanguageDirectory => Path.Combine(AppContext.BaseDirectory, "lang");

    /// <summary>当前已加载的全部语言（含内嵌简体中文）。</summary>
    public IReadOnlyList<LanguagePack> Languages
    {
        get { lock (_lock) return _languages.ToList(); }
    }

    /// <summary>是否检测到外置语言包（决定设置页是否显示语言选择）。</summary>
    public bool HasExternalPacks
    {
        get { lock (_lock) return _languages.Any(l => !l.BuiltIn); }
    }

    public string CurrentId => _current?.Id ?? BuiltinId;

    /// <summary>当前语言包。</summary>
    public LanguagePack? Current => _current;

    /// <summary>启动时调用：加载内嵌简体中文 + 扫描 lang 文件夹 + 恢复用户上次选择。</summary>
    public void Initialize()
    {
        lock (_lock)
        {
            if (_languages.Count > 0) return;
            _languages.Add(LoadBuiltin());
            ScanExternalPacks();
        }

        var wanted = SettingsService.Instance.Language;
        _current = _languages.FirstOrDefault(l => l.Id.Equals(wanted, StringComparison.OrdinalIgnoreCase))
                   ?? _languages[0];
        if (!_current.Id.Equals(wanted, StringComparison.OrdinalIgnoreCase))
            SettingsService.Instance.Language = _current.Id;

        RegisterLoadedHandler();
        LogService.Instance.Info(
            $"多语言：当前语言 = {_current.Id}（{_current.Name}），已加载语言包 {_languages.Count(l => !l.BuiltIn)} 个。");
    }

    /// <summary>切换语言并保存到设置。返回是否切换成功。</summary>
    public bool SetLanguage(string id)
    {
        LanguagePack? target;
        lock (_lock)
            target = _languages.FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (target == null) return false;
        if (_current?.Id == target.Id) return true;

        _current = target;
        SettingsService.Instance.Language = target.Id;
        LogService.Instance.Info($"多语言：切换到 {target.Id}（{target.Name}）。");
        LanguageChanged?.Invoke();
        return true;
    }

    /// <summary>按语言编码查找已加载的语言包。</summary>
    public LanguagePack? Find(string id)
    {
        lock (_lock)
            return _languages.FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>重新扫描 lang 目录（安装/删除语言包后调用）。</summary>
    public void Refresh()
    {
        lock (_lock)
        {
            _languages.RemoveAll(l => !l.BuiltIn);
            ScanExternalPacks();
            if (_current == null || _languages.All(l => !l.Id.Equals(_current.Id, StringComparison.OrdinalIgnoreCase)))
            {
                _current = _languages[0];
                SettingsService.Instance.Language = _current.Id;
            }
        }
        LogService.Instance.Info($"多语言：刷新语言包目录，当前共 {Languages.Count} 个语言。");
    }

    /// <summary>翻译单条简体中文原文；找不到译文时原样返回。</summary>
    public string Translate(string? source)
    {
        if (string.IsNullOrEmpty(source)) return source ?? string.Empty;
        var pack = _current;
        if (pack == null || pack.BuiltIn) return source;

        var dict = pack.Strings;
        if (dict.TryGetValue(source, out var direct) && !string.IsNullOrEmpty(direct)) return direct;

        // 多行文本：逐行翻译后拼接，便于语言包按行维护。
        if (source.IndexOf('\n') >= 0)
        {
            var lines = source.Replace("\r\n", "\n").Split('\n');
            var translated = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length == 0) { translated[i] = string.Empty; continue; }
                // 不含中文的行（品牌名、数字、纯英文）原样保留
                if (!ContainsCjk(line)) { translated[i] = line; continue; }
                if (!dict.TryGetValue(line, out var t) || string.IsNullOrEmpty(t)) return source;
                translated[i] = t;
            }
            return string.Join("\n", translated);
        }
        return source;
    }

    private static bool ContainsCjk(string s)
    {
        foreach (var c in s)
            if (c >= '\u4e00' && c <= '\u9fff') return true;
        return false;
    }

    /// <summary>带 {0} 占位符的翻译（先翻译格式串，再格式化）。</summary>
    public string TranslateFormat(string source, params object?[] args)
    {
        var fmt = Translate(source);
        try { return string.Format(fmt, args); }
        catch { return source; }
    }

    /// <summary>把某个界面元素上的静态简体中文字面量替换为当前语言。</summary>
    public void LocalizeElement(DependencyObject d)
    {
        if (_current == null) return;
        try
        {
            if (d is TextBlock tb)
            {
                LocalizeProperty(tb, TextBlock.TextProperty, tb.Text, v => tb.Text = v);
                foreach (var run in tb.Inlines.OfType<Run>())
                    LocalizeProperty(run, Run.TextProperty, run.Text, v => run.Text = v);
            }
            if (d is ContentControl cc && cc.Content is string content)
                LocalizeProperty(cc, ContentControl.ContentProperty, content, v => cc.Content = v);
            if (d is HeaderedContentControl hcc && hcc.Header is string header)
                LocalizeProperty(hcc, HeaderedContentControl.HeaderProperty, header, v => hcc.Header = v);
            if (d is Window w)
                LocalizeProperty(w, Window.TitleProperty, w.Title, v => w.Title = v);
            if (d is FrameworkElement fe && fe.ToolTip is string tip)
                LocalizeProperty(fe, FrameworkElement.ToolTipProperty, tip, v => fe.ToolTip = v);
        }
        catch { /* 单个元素翻译失败不影响其它元素 */ }
    }

    /// <summary>等界面完成布局后再翻译整棵可视树（避免模板/列表项尚未实例化）。</summary>
    public void ApplyToDeferred(DependencyObject root)
    {
        if (_current == null || _current.BuiltIn) return;
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            new Action(() => ApplyTo(root)),
            System.Windows.Threading.DispatcherPriority.Loaded);
    }

    /// <summary>遍历可视树，刷新全部静态文本（语言切换后调用）。</summary>
    public void ApplyTo(DependencyObject root)
    {
        if (_current == null) return;
        LocalizeElement(root);
        int count = 0;
        try { count = VisualTreeHelper.GetChildrenCount(root); } catch { }
        for (int i = 0; i < count; i++)
        {
            DependencyObject child;
            try { child = VisualTreeHelper.GetChild(root, i); } catch { continue; }
            ApplyTo(child);
        }
    }

    private void LocalizeProperty(DependencyObject d, DependencyProperty prop, string? current, Action<string> setter)
    {
        if (current == null) return;
        if (BindingOperations.GetBindingExpressionBase(d, prop) != null) return;

        var map = Originals.GetOrCreateValue(d);
        if (!map.TryGetValue(prop, out var original))
        {
            original = current;
            map[prop] = original;
        }

        var translated = Translate(original);
        if (!string.Equals(current, translated)) setter(translated);
    }

    private static LanguagePack LoadBuiltin()
    {
        var builtin = new LanguagePack
        {
            Id = BuiltinId,
            Name = "简体中文",
            Author = "Hatasoft-Studio",
            Version = "1.0.0",
            BuiltIn = true
        };

        try
        {
            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.Contains("zh-Hans", StringComparison.OrdinalIgnoreCase)
                                     && n.EndsWith(".meowlang", StringComparison.OrdinalIgnoreCase));
            if (name == null) return builtin;
            using var s = asm.GetManifestResourceStream(name);
            if (s == null) return builtin;
            using var reader = new StreamReader(s);
            var parsed = JsonSerializer.Deserialize<LanguagePack>(reader.ReadToEnd(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed == null) return builtin;
            parsed.BuiltIn = true;
            parsed.Id = BuiltinId;
            return parsed;
        }
        catch { return builtin; }
    }

    private void ScanExternalPacks()
    {
        try
        {
            var dir = LanguageDirectory;
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.EnumerateFiles(dir, "*.meowlang", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var pack = JsonSerializer.Deserialize<LanguagePack>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (pack == null || string.IsNullOrWhiteSpace(pack.Id) || string.IsNullOrWhiteSpace(pack.Name)) continue;
                    if (pack.Id.Equals(BuiltinId, StringComparison.OrdinalIgnoreCase)) continue;
                    if (_languages.Any(l => l.Id.Equals(pack.Id, StringComparison.OrdinalIgnoreCase))) continue;
                    pack.FilePath = file;
                    pack.BuiltIn = false;
                    _languages.Add(pack);
                }
                catch (Exception ex)
                {
                    LogService.Instance.Warn($"多语言：语言包加载失败 {Path.GetFileName(file)}：{ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            LogService.Instance.Warn($"多语言：扫描 lang 文件夹失败：{ex.Message}");
        }
    }

    /// <summary>任何界面元素加载完成后自动翻译其静态文本。</summary>
    private void RegisterLoadedHandler()
    {
        if (_loadedHandlerRegistered) return;
        _loadedHandlerRegistered = true;
        // Loaded 事件的类处理器只对 Window 级元素生效，子元素需由各页面/弹窗主动调用翻译。
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
            new RoutedEventHandler((s, _) =>
            {
                if (s is DependencyObject d) ApplyToDeferred(d);
            }));
    }
}

/// <summary>翻译调用简写。</summary>
public static class Loc
{
    public static string T(string? source) => LocalizationService.Instance.Translate(source);
    public static string Tf(string source, params object?[] args)
        => LocalizationService.Instance.TranslateFormat(source, args);
}
