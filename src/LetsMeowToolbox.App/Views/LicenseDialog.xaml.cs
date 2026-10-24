using System.IO;
using System.Reflection;
using System.Windows;

namespace LetsMeowToolbox.App.Views;

/// <summary>
/// 首次启动协议弹窗：展示 GPL-3.0 全文，用户同意后才允许继续使用；不同意则退出。
/// 也可在“关于”页以只读方式查看（仅“关闭”按钮）。
/// </summary>
public partial class LicenseDialog : Window
{
    private bool _agreeMode;

    public LicenseDialog()
    {
        InitializeComponent();
        TxtLicense.Text = LoadLicenseText();
    }

    /// <summary>首次启动协议确认。返回 true 表示用户同意。</summary>
    public static bool ShowForAgree()
    {
        var d = new LicenseDialog { _agreeMode = true };
        d.ShowDialog();
        return d.DialogResult == true;
    }

    /// <summary>“关于”页只读查看协议。</summary>
    public static void ShowInfo(Window? owner)
    {
        var d = new LicenseDialog();
        d._agreeMode = false;
        d.TxtTitle.Text = "开源许可协议";
        d.TxtHint.Text = "Let's Meow! Toolbox 基于 GNU General Public License v3.0（GPL-3.0）开源。";
        d.BtnAgree.Visibility = Visibility.Collapsed;
        d.BtnDecline.Visibility = Visibility.Collapsed;
        d.BtnClose.Visibility = Visibility.Visible;
        if (owner != null) d.Owner = owner;
        d.ShowDialog();
    }

    /// <summary>从嵌入资源读取 GPL-3.0 全文（按名称模糊匹配，避免受资源命名规则影响）。</summary>
    private static string LoadLicenseText()
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.Contains("gpl-3.0", StringComparison.OrdinalIgnoreCase));
            if (name == null) return "（许可协议文本未找到：gpl-3.0.txt）";
            using var s = asm.GetManifestResourceStream(name);
            if (s == null) return "（许可协议文本读取失败）";
            using var reader = new StreamReader(s);
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            return $"（许可协议文本读取失败：{ex.Message}）";
        }
    }

    private void OnAgree(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnDecline(object sender, RoutedEventArgs e) => DialogResult = false;

    private void OnClose(object sender, RoutedEventArgs e) => DialogResult = true;

    /// <summary>协议确认模式下，通过窗口 X / Alt+F4 关闭视为“不同意”。</summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_agreeMode && DialogResult == null)
            DialogResult = false;
        base.OnClosing(e);
    }
}
