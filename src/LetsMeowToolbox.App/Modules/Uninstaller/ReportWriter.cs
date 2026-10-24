using System.IO;

namespace LetsMeowToolbox.App.Modules.Uninstaller;

/// <summary>清理 / 卸载报告导出（Markdown）。</summary>
public static class ReportWriter
{
    public static string Export(string title, IReadOnlyList<(string item, string result)> rows)
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "LetsMeowToolbox");
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, $"meow-report-{DateTime.Now:yyyyMMdd-HHmmss}.md");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {title}");
        sb.AppendLine();
        sb.AppendLine($"- 生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("- 由 Let's Meow Toolbox（Hatasoft-Studio，GPL-3.0）导出");
        sb.AppendLine();
        sb.AppendLine("| 项目 | 结果 |");
        sb.AppendLine("| --- | --- |");
        foreach (var (item, result) in rows)
        {
            sb.AppendLine($"| {item.Replace("|", "\\|")} | {result.Replace("|", "\\|")} |");
        }
        sb.AppendLine();
        sb.AppendLine("> 说明：本报告由工具自动生成，仅供参考。");
        File.WriteAllText(file, sb.ToString());
        return file;
    }
}
