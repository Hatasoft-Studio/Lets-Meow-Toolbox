using System.Globalization;
using System.Windows.Data;

namespace LetsMeowToolbox.App.Views;

/// <summary>把布尔“是否已应用”转为“已启用 / 未启用”文本。</summary>
public sealed class AppliedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "已启用" : "未启用";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
