using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LetsMeowToolbox.App.Services;
using LetsMeowToolbox.App.Utils;
using LetsMeowToolbox.App.Views;

namespace LetsMeowToolbox.App;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    /// <summary>当前主窗口实例（供 FluentToast 等静态入口定位提示条挂载层）。</summary>
    internal static MainWindow? Instance;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool _showAnime;
    public bool ShowAnime
    {
        get => _showAnime;
        set
        {
            _showAnime = value;
            SettingsService.Instance.ShowAnime = value;
            Raise();
        }
    }

    private readonly List<(string title, string greeting)> _pages = new()
    {
        ("新手区", "喵～这里是新手区，都是安全的常规设置，放心大胆点开关吧，错了也能一键还原哦！"),
        ("高级区", "注意喵！这里是高级区，操作前会先让你确认风险，慢慢来不要急～"),
        ("高危插件区", "呜哇，这里是很危险的地方！只有外置插件才会出现，操作前一定要看清提示喵。"),
        ("卸载程序", "在这里集中管理已安装的 Windows 程序，卸载前记得保存好数据喵～"),
        ("解除占用", "文件被占用？来这里找到并结束占用进程，或安排开机删除喵～"),
        ("设置", "在这里调整外观、日志与小工具的行为喵～"),
        ("特别鸣谢", "感谢所有让这个小工具变得更好的开源项目喵～")
    };

    /// <summary>Win11 太阳谷风格导航图标（Segoe MDL2 Assets 字形，顺序对应 _pages）。</summary>
    private static readonly string[] NavIcons =
    {
        "\uE70F", // Home         → 新手区
        "\uE734", // FavoriteStar → 高级区（进阶）
        "\uE7BA", // Warning      → 高危插件区
        "\uE74D", // Delete       → 卸载程序
        "\uE8C8", // Process      → 解除占用
        "\uE713", // Settings     → 设置
        "\uE946"  // Info         → 特别鸣谢
    };

    private object[] _views;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Instance = this;

        _views = new object[]
        {
            new BeginnerView(),
            new AdvancedView(),
            new HighRiskView(),
            new UninstallerView(),
            new UnlockView(),
            new SettingsView(),
            new CreditsView()
        };

        _showAnime = SettingsService.Instance.ShowAnime;

        // 设置页也可能改二次元开关，此处同步刷新顶栏开关状态
        SettingsService.Instance.Changed += () =>
        {
            if (_showAnime != SettingsService.Instance.ShowAnime)
            {
                _showAnime = SettingsService.Instance.ShowAnime;
                Raise(nameof(ShowAnime));
            }
        };

        // 安装窗口背景（Mica 或纯色兜底）——窗口句柄就绪后调用
        SourceInitialized += (_, _) => ThemeService.ApplyToWindow(this);

        BuildNav();
        InitCursor();
        SelectPage(0);
        PlayWindowEntrance();
        RefreshAdmin();
        TxtOs.Text = $"系统：{OsVersionService.FriendlyName} · {(OsVersionService.Is64Bit ? "x64" : "")}";
    }

    private void BuildNav()
    {
        for (int i = 0; i < _pages.Count; i++)
        {
            int idx = i;
            // 太阳谷图标 + 文字（图标随选中/悬停自动变色）
            var icon = new System.Windows.Controls.TextBlock
            {
                Text = NavIcons[i],
                FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                FontSize = 16,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            var label = new System.Windows.Controls.TextBlock
            {
                Text = _pages[i].title,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            var sp = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            sp.Children.Add(icon);
            sp.Children.Add(label);

            var rb = new RadioButton
            {
                Style = (Style)FindResource("FluentNav"),
                Content = sp,
                GroupName = "nav",
                Tag = idx,
                IsChecked = i == 0
            };
            rb.Checked += (_, _) => SelectPage(idx);
            NavPanel.Children.Add(rb);
        }
    }

    /// <summary>跳到“解除占用”页并设置目标路径（卸载页联动）。</summary>
    public void GoToUnlock(string path)
    {
        const int unlockIndex = 4; // 对应 _pages 中的“解除占用”
        SelectPage(unlockIndex);
        if (_views[unlockIndex] is UnlockView uv)
            uv.SetPath(path);
    }

    /// <summary>重建全部页面（设置页“一键初始化”后调用，刷新所有功能状态）。</summary>
    public void ReloadAllViews()
    {
        var current = _views.ToList().FindIndex(v => ReferenceEquals(v, MainContent?.Content));
        _views = new object[]
        {
            new BeginnerView(),
            new AdvancedView(),
            new HighRiskView(),
            new UninstallerView(),
            new UnlockView(),
            new SettingsView(),
            new CreditsView()
        };
        int idx = current >= 0 && current < _views.Length ? current : 0;
        MainContent.Content = _views[idx];
        TxtTitle.Text = _pages[idx].title;
        TxtGreeting.Text = _pages[idx].greeting;
        // 顶部二次元开关与设置同步
        _showAnime = SettingsService.Instance.ShowAnime;
        Raise(nameof(ShowAnime));
    }

    private void SelectPage(int index)
    {
        if (MainContent == null) return;
        TxtTitle.Text = _pages[index].title;
        TxtGreeting.Text = _pages[index].greeting;
        PlayLoadBar();

        var next = _views[index];
        if (ReferenceEquals(MainContent.Content, next)) return;

        // 翻页动画：旧页淡出 → 换内容 → 新页从右侧滑入 + 淡入（类似翻页）
        var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(0, TimeSpan.FromMilliseconds(110));
        fadeOut.Completed += (_, _) =>
        {
            MainContent.Content = next;
            MainContent.Opacity = 0;

            // 新页初始从右偏移，随后滑入归位并淡入
            if (MainContent.RenderTransform is not TranslateTransform t)
            {
                t = new TranslateTransform();
                MainContent.RenderTransform = t;
            }
            t.X = 44;

            var slideIn = new System.Windows.Media.Animation.DoubleAnimation(0, TimeSpan.FromMilliseconds(260))
            {
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(1, TimeSpan.FromMilliseconds(260))
            {
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = EasingMode.EaseOut }
            };
            t.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
            MainContent.BeginAnimation(System.Windows.UIElement.OpacityProperty, fadeIn);
        };
        MainContent.BeginAnimation(System.Windows.UIElement.OpacityProperty, fadeOut);
    }

    /// <summary>粉色顶部加载进度条：页面渲染时播放，先快后慢并淡出。</summary>
    private void PlayLoadBar()
    {
        if (LoadBarContainer == null) return;
        LoadBarContainer.BeginAnimation(OpacityProperty, null);
        LoadBarScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        LoadBarContainer.Opacity = 1;
        LoadBarScale.ScaleX = 0;

        var s1 = new DoubleAnimation(0.65, TimeSpan.FromMilliseconds(220))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        s1.Completed += (_, _) =>
        {
            var s2 = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            s2.Completed += (_, _) =>
                LoadBarContainer.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(0, TimeSpan.FromMilliseconds(220)));
            LoadBarScale.BeginAnimation(ScaleTransform.ScaleXProperty, s2);
        };
        LoadBarScale.BeginAnimation(ScaleTransform.ScaleXProperty, s1);
    }

    /// <summary>窗口初次出现的柔和入场动画。</summary>
    private void PlayWindowEntrance()
    {
        try
        {
            Opacity = 0;
            BeginAnimation(OpacityProperty,
                new DoubleAnimation(1, TimeSpan.FromMilliseconds(360))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        }
        catch { /* 入场动画失败不影响功能 */ }
    }

    private void RefreshAdmin()
    {
        bool admin = AdminService.IsAdmin;
        TxtAdminStatus.Text = admin
            ? "管理员权限：已提升"
            : "当前为普通权限。涉及系统设置的功能需要管理员权限。";
        BtnElevate.Visibility = admin ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnElevate(object sender, RoutedEventArgs e)
    {
        if (AdminService.IsAdmin) return;
        if (Ui.Confirm(this, "将以管理员身份重新运行本程序。当前未完成的设置会丢失，是否继续？", "管理员权限"))
            AdminService.RelaunchAsAdmin();
    }

    #region iPadOS 风格光标（粉色小点 + 吸附交互控件）

    private bool _hasMouse;
    private Point _mouse;
    private Rect? _snap;

    // 光标当前渲染值（用于平滑插值）
    private double _dotX, _dotY;
    private double _hX, _hY, _hW = 10, _hH = 10, _hCr = 10;
    private double _dotOp, _haloOp;

    private void InitCursor()
    {
        RootGrid.PreviewMouseMove += (_, e) =>
        {
            _hasMouse = true;
            _mouse = e.GetPosition(RootGrid);
            _snap = FindSnapTarget(_mouse);
        };
        RootGrid.MouseEnter += (_, _) => { _hasMouse = true; TryHideNative(); };
        RootGrid.MouseLeave += (_, _) => { _hasMouse = false; RestoreNative(); };
        Activated += (_, _) => TryHideNative();
        Deactivated += (_, _) => RestoreNative();
        System.Windows.Media.CompositionTarget.Rendering += OnCursorFrame;
    }

    private void TryHideNative()
    {
        try { if (IsActive && _hasMouse) Mouse.OverrideCursor = Cursors.None; } catch { /* ignore */ }
    }

    private void RestoreNative()
    {
        try { Mouse.OverrideCursor = null; } catch { /* ignore */ }
    }

    /// <summary>命中测试：从鼠标位置向上找最近的可交互控件，返回其在 RootGrid 中的包围盒。</summary>
    private Rect? FindSnapTarget(Point p)
    {
        try
        {
            var hit = VisualTreeHelper.HitTest(RootGrid, p);
            DependencyObject? o = hit?.VisualHit;
            while (o != null && o != RootGrid)
            {
                if (o is ButtonBase || o is TextBox || o is ComboBox || o is ListBoxItem || o is TabItem)
                {
                    if (o is FrameworkElement fe && fe.IsEnabled && fe.IsVisible)
                    {
                        var rect = fe.TransformToAncestor(RootGrid)
                            .TransformBounds(new Rect(0, 0, fe.ActualWidth, fe.ActualHeight));
                        if (rect.Width > 0 && rect.Height > 0 && rect.Width <= 1000 && rect.Height <= 90)
                        {
                            rect.Inflate(7, 6);
                            return rect;
                        }
                    }
                    return null;
                }
                o = VisualTreeHelper.GetParent(o);
            }
        }
        catch { /* ignore */ }
        return null;
    }

    private void OnCursorFrame(object? sender, EventArgs e)
    {
        try
        {
            bool snapped = _snap.HasValue;
            const double kFree = 0.3;  // 自由跟随
            const double kSnap = 0.42; // 吸附时更跟手
            double k = snapped ? kSnap : kFree;
            if (!_hasMouse)
            {
                _dotOp += (0 - _dotOp) * kFree;
                _haloOp += (0 - _haloOp) * kFree;
                CursorDot.Opacity = _dotOp;
                CursorHalo.Opacity = _haloOp;
                return;
            }

            double targetDx = _mouse.X - 4.5, targetDy = _mouse.Y - 4.5;
            double targetDotOp = 1, targetHaloOp = 0;
            double tx = _hX, ty = _hY, tw = _hW, th = _hH, tcr = 10;

            if (_snap is Rect rc)
            {
                tx = rc.X; ty = rc.Y; tw = rc.Width; th = rc.Height; tcr = 10;
                targetDx = rc.X + rc.Width / 2 - 4.5;
                targetDy = rc.Y + rc.Height / 2 - 4.5;
                targetHaloOp = 1; targetDotOp = 0;
            }

            _dotX += (targetDx - _dotX) * k;
            _dotY += (targetDy - _dotY) * k;
            DotTranslate.X = _dotX; DotTranslate.Y = _dotY;

            _hX += (tx - _hX) * k; _hY += (ty - _hY) * k;
            _hW += (tw - _hW) * k; _hH += (th - _hH) * k;
            _hCr += (tcr - _hCr) * k;
            HaloTranslate.X = _hX; HaloTranslate.Y = _hY;
            CursorHalo.Width = _hW; CursorHalo.Height = _hH;
            CursorHalo.CornerRadius = new CornerRadius(_hCr);

            _dotOp += (targetDotOp - _dotOp) * k;
            _haloOp += (targetHaloOp - _haloOp) * k;
            CursorDot.Opacity = _dotOp;
            CursorHalo.Opacity = _haloOp;
        }
        catch { /* 光标动画失败不影响操作 */ }
    }

    protected override void OnClosed(EventArgs e)
    {
        RestoreNative();
        System.Windows.Media.CompositionTarget.Rendering -= OnCursorFrame;
        base.OnClosed(e);
    }

    #endregion
}
