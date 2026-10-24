using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Services;

/// <summary>
/// 主题（自管）：固定浅色 Fluent 主题；可叠加“二次元化”粉色强调色与壁纸。
/// 不依赖 HandyControl 的 Theme.SetSkin，画刷键值由自有字典自保证。
/// </summary>
public static class ThemeService
{
    private static readonly ResourceDictionary Light = new()
    { Source = new Uri("Themes/Brushes.Light.xaml", UriKind.Relative) };

    private static readonly ResourceDictionary Anime = new()
    { Source = new Uri("Themes/Brushes.Anime.xaml", UriKind.Relative) };

    private static ImageBrush? _wallpaper;

    public static bool IsAnime { get; private set; }

    /// <summary>在 Application 资源中安装默认画刷（启动时调用）。</summary>
    public static void InstallDefault()
    {
        var app = Application.Current;
        var merged = app.Resources.MergedDictionaries;
        if (!merged.Contains(Light))
            merged.Insert(0, Light);
    }

    /// <summary>应用主题：始终浅色，可叠加二次元粉色/壁纸，并刷新所有窗口背景。</summary>
    public static void Apply(bool anime)
    {
        IsAnime = anime;
        var app = Application.Current;
        var merged = app.Resources.MergedDictionaries;

        if (!merged.Contains(Light))
            merged.Insert(0, Light);

        // 二次元化：叠加粉色强调（后插入，覆盖 Accent 键）
        var hasAnime = merged.Contains(Anime);
        if (anime && !hasAnime)
            merged.Insert(1, Anime);
        else if (!anime && hasAnime)
            merged.Remove(Anime);

        foreach (System.Windows.Window w in app.Windows)
            ApplyToWindow(w);
    }

    /// <summary>对单个窗口应用主题背景：主窗口二次元模式下用壁纸，其余用浅色/Mica。</summary>
    public static void ApplyToWindow(System.Windows.Window window)
    {
        try
        {
            if (IsAnime && window is MainWindow)
            {
                window.Background = Wallpaper;
                return;
            }

            WindowBackdrop.EnableMica(window, dark: false);
        }
        catch { /* 主题背景失败不阻断运行 */ }
    }

    private static ImageBrush Wallpaper
    {
        get
        {
            if (_wallpaper == null)
            {
                // 壁纸为 exe 同目录下 Assets/wallpaper.png 的松文件（不嵌入，单文件发布同样可用）
                var path = System.IO.Path.Combine(
                    AppContext.BaseDirectory, "Assets", "wallpaper.png");
                if (!System.IO.File.Exists(path))
                    path = System.IO.Path.Combine(AppContext.BaseDirectory, "wallpaper.png");
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.CacheOption =BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                _wallpaper = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill };
            }
            return _wallpaper;
        }
    }
}
