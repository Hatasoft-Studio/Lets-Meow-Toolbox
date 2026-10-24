using System.Text;
using System.Windows;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

/// <summary>更新日志弹窗：展示从 v0.1.1 起的全部版本变更。</summary>
public partial class ChangelogDialog : Window
{
    public ChangelogDialog()
    {
        InitializeComponent();
        TxtVersionNote.Text = $"当前版本：v{AppInfo.Version}";
        TxtChangelog.Text = BuildText();
    }

    /// <summary>弹出更新日志窗口。</summary>
    public static void Show(Window? owner)
    {
        var d = new ChangelogDialog();
        if (owner != null) d.Owner = owner;
        d.ShowDialog();
    }

    private static string BuildText()
    {
        var sb = new StringBuilder();
        foreach (var (version, date, changes) in AppInfo.Changelog)
        {
            sb.AppendLine($"══ {version} · {date} ══");
            foreach (var c in changes)
                sb.AppendLine(c);
            sb.AppendLine();
        }
        sb.AppendLine("版本规则：");
        sb.AppendLine("• 主版本号：重大架构调整 / 破坏性变更时 +1（0.x 阶段指整体重构）");
        sb.AppendLine("• 次版本号：新增功能时 +1");
        sb.AppendLine("• 修订号：修复缺陷 / 优化时 +1");
        return sb.ToString().TrimEnd();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
