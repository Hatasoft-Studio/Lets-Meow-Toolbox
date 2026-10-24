using System.Windows;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

/// <summary>
/// 自研 Fluent 弹窗：只用自有画刷，双主题/二次元模式下均正常显示（修复 HandyControl MessageBox 黑屏问题）。
/// </summary>
public partial class FluentDialog : Window
{
    private MessageBoxResult _result = MessageBoxResult.None;
    // 主按钮（最右侧）在“是/否”场景下代表 Yes，在“确定”场景下代表 OK
    private MessageBoxResult _primaryResult = MessageBoxResult.OK;

    public FluentDialog()
    {
        InitializeComponent();
    }

    /// <summary>弹出 Fluent 弹窗，返回用户选择结果。</summary>
    public static MessageBoxResult Show(
        System.Windows.Window? owner,
        string text,
        string title,
        MessageBoxButton buttons,
        MessageBoxImage image,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        var d = new FluentDialog();
        d.TxtTitle.Text = string.IsNullOrWhiteSpace(title) ? "Let's Meow Toolbox" : title;
        d.TxtMessage.Text = text;
        d.TxtIcon.Text = image switch
        {
            MessageBoxImage.Question => "❓",
            MessageBoxImage.Warning => "⚠️",
            MessageBoxImage.Error => "⛔",
            _ => "💬"
        };
        d.TxtTitle.Foreground = image switch
        {
            MessageBoxImage.Error => (System.Windows.Media.Brush)new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE7, 0x48, 0x56)),
            MessageBoxImage.Warning => (System.Windows.Media.Brush)new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x99, 0x00)),
            _ => d.FindResource("TextPrimary") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.Black
        };
        d._result = defaultResult;

        switch (buttons)
        {
            case MessageBoxButton.OK:
                d.BtnOk.Visibility = Visibility.Visible;
                d.BtnOk.Content = "确定";
                d._primaryResult = MessageBoxResult.OK;
                break;
            case MessageBoxButton.OKCancel:
                d.BtnOk.Visibility = Visibility.Visible;
                d.BtnOk.Content = "确定";
                d.BtnCancel.Visibility = Visibility.Visible;
                d._primaryResult = MessageBoxResult.OK;
                break;
            case MessageBoxButton.YesNo:
                d.BtnOk.Visibility = Visibility.Visible;
                d.BtnOk.Content = "是";
                d.BtnNo.Visibility = Visibility.Visible;
                d._primaryResult = MessageBoxResult.Yes;
                break;
            case MessageBoxButton.YesNoCancel:
                d.BtnOk.Visibility = Visibility.Visible;
                d.BtnOk.Content = "是";
                d.BtnNo.Visibility = Visibility.Visible;
                d.BtnCancel.Visibility = Visibility.Visible;
                d._primaryResult = MessageBoxResult.Yes;
                break;
        }

        if (owner != null)
            d.Owner = owner;

        d.ShowDialog();
        return d._result;
    }

    private void OnOk(object sender, RoutedEventArgs e) { _result = _primaryResult; Close(); }
    private void OnCancel(object sender, RoutedEventArgs e) { _result = MessageBoxResult.Cancel; Close(); }
    private void OnNo(object sender, RoutedEventArgs e) { _result = MessageBoxResult.No; Close(); }
}
