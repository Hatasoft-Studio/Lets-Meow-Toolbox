using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

/// <summary>语言包详情/加载提醒窗口：安装前预览或加载前确认。</summary>
public partial class LanguagePackDetailsDialog : Window
{
    /// <summary>是否官方语言包（绑定到盾牌图标）。</summary>
    public bool IsOfficial { get; }

    public LanguagePackDetailsDialog(LanguagePack pack, bool installMode, DateTime? installTime, long? fileSize)
    {
        InitializeComponent();
        DataContext = this;
        IsOfficial = pack.IsOfficial;

        Title = Loc.T(installMode ? "安装语言包" : "语言包信息");
        TxtPackName.Text = pack.DisplayName;
        TxtAuthor.Text = pack.Author;
        TxtOfficialTag.Text = Loc.T(pack.IsOfficial ? "官方语言包" : "第三方语言包");
        if (!pack.IsOfficial)
        {
            TagOfficial.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xF3, 0xE0));
            TxtOfficialTag.Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0x51, 0x00));
        }

        BtnCancel.Content = Loc.T("取消");
        BtnAction.Content = Loc.T(installMode ? "安装" : "加载");

        AddRow(Loc.T("包名"), pack.DisplayName);
        AddRow(Loc.T("版本号"), "v" + pack.Version);
        AddRow(Loc.T("适用软件版本"),
            string.IsNullOrWhiteSpace(pack.AppVersion) || pack.AppVersion.Trim() == "*"
                ? Loc.T("不限")
                : pack.AppVersion);
        AddRow(Loc.T("安装时间"),
            installTime.HasValue ? installTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "—");
        AddRow(Loc.T("语言类型"), pack.TypeText);
        AddRow(Loc.T("语言编码"), pack.Id);
        AddRow(Loc.T("文件大小"), fileSize.HasValue ? LanguagePackInstaller.FormatSize(fileSize.Value) : "—");
        AddRow(Loc.T("语言包简介"), string.IsNullOrWhiteSpace(pack.Description) ? "—" : pack.Description);
        AddRow(Loc.T("作者网站"), string.IsNullOrWhiteSpace(pack.AuthorWebsite) ? "—" : pack.AuthorWebsite);
    }

    private void AddRow(string label, string value)
    {
        var grid = new Grid { Margin = new Thickness(0, 6, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(96) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var lb = new TextBlock
        {
            Text = label,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Foreground = FindResource("TextSecondary") as Brush ?? Brushes.Gray
        };
        var val = new TextBlock { Text = value, FontSize = 13, TextWrapping = TextWrapping.Wrap };
        Grid.SetColumn(val, 1);

        grid.Children.Add(lb);
        grid.Children.Add(val);
        ExtraDetailsPanel.Children.Add(grid);
    }

    private void OnAction(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
