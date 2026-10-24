using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace LetsMeowToolbox.App.Services;

public enum ToastKind { Info, Success, Warning, Error }

/// <summary>自研 Fluent 顶部提示条：在主窗口内淡入淡出，2.6 秒自动消失（替代 HandyControl Growl，避免黑屏）。</summary>
public static class FluentToast
{
    private static Color ColorOf(ToastKind k) => k switch
    {
        ToastKind.Success => Color.FromRgb(0x0F, 0x7B, 0x0F),
        ToastKind.Warning => Color.FromRgb(0xC8, 0x83, 0x00),
        ToastKind.Error => Color.FromRgb(0xC4, 0x2B, 0x1C),
        _ => Color.FromRgb(0x00, 0x78, 0xD4)
    };

    private static string GlyphOf(ToastKind k) => k switch
    {
        ToastKind.Success => "✓",
        ToastKind.Warning => "⚠",
        ToastKind.Error => "✕",
        _ => "ℹ"
    };

    public static void Show(string message, ToastKind kind = ToastKind.Info)
    {
        var win = Application.Current?.MainWindow as MainWindow;
        if (win == null) return;
        win.Dispatcher.Invoke(() => ShowCore(win, message, kind));
    }

    private static void ShowCore(MainWindow win, string message, ToastKind kind)
    {
        var panel = win.ToastHost;
        if (panel == null) return;

        var bg = new SolidColorBrush(ColorOf(kind));
        var border = new Border
        {
            Background = bg,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 9, 16, 9),
            Margin = new Thickness(0, 0, 0, 8),
            MaxWidth = 420,
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 18, ShadowDepth = 2, Opacity = 0.25, Color = Colors.Black
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var glyph = new TextBlock
        {
            Text = GlyphOf(kind), FontSize = 13, FontWeight = FontWeights.Bold,
            Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("Segoe UI, Microsoft YaHei UI")
        };
        var text = new TextBlock
        {
            Text = message, FontSize = 12.5, Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("Segoe UI, Microsoft YaHei UI")
        };
        grid.Children.Add(glyph);
        grid.Children.Add(text);
        Grid.SetColumn(text, 1);
        border.Child = grid;

        panel.Children.Add(border);

        // 淡入
        border.Opacity = 0;
        border.BeginAnimation(System.Windows.UIElement.OpacityProperty,
            new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });

        // 自动淡出移除
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2600) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            var outAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(220));
            outAnim.Completed += (_, _) => panel.Children.Remove(border);
            border.BeginAnimation(System.Windows.UIElement.OpacityProperty, outAnim);
        };
        timer.Start();
    }
}
