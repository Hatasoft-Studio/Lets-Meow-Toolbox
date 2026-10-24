using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LetsMeowToolbox.App.Views;

/// <summary>把“是否系统核心保护”转为徽标背景色。</summary>
public sealed class CoreBadgeConverter : IValueConverter
{
    private static readonly Brush Core = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static readonly Brush Safe = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Core : Safe;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
