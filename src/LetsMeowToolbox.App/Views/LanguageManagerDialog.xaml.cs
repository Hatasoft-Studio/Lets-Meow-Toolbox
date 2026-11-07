using System.IO;
using System.Windows;
using System.Windows.Controls;
using LetsMeowToolbox.App.Services;
using Microsoft.Win32;

namespace LetsMeowToolbox.App.Views;

/// <summary>语言包管理：列出 ./lang 下所有语言包，支持安装与加载。</summary>
public partial class LanguageManagerDialog : Window
{
    public LanguageManagerDialog()
    {
        InitializeComponent();
        Title = Loc.T("语言管理");
        TxtLanguageDir.Text = Loc.Tf("语言包目录：{0}", LocalizationService.LanguageDirectory);
        BtnInstall.Content = Loc.T("安装语言包…");
        BtnLoad.Content = Loc.T("加载所选语言");
        BtnClose.Content = Loc.T("关闭");
        RefreshList();
    }

    public static void Show(Window? owner)
    {
        var dlg = new LanguageManagerDialog();
        if (owner != null) dlg.Owner = owner;
        dlg.ShowDialog();
    }

    private void RefreshList()
    {
        var currentId = LocalizationService.Instance.CurrentId;
        var list = LocalizationService.Instance.Languages
            .OrderBy(l => l.BuiltIn ? 0 : (l.IsOfficial ? 1 : 2))
            .ThenBy(l => l.DisplayName, StringComparer.CurrentCulture)
            .ToList();
        PackList.ItemsSource = list;
        PackList.SelectedItem = list.FirstOrDefault(l => l.Id.Equals(currentId, StringComparison.OrdinalIgnoreCase))
                                ?? list.FirstOrDefault();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void OnInstall(object sender, RoutedEventArgs e)
    {
        InstallInteractive(this);
        RefreshList();
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        if (PackList.SelectedItem is not LanguagePack pack)
        {
            Ui.Warning(Loc.T("请选择一个语言包。"));
            return;
        }

        if (!LanguagePackInstaller.CheckCompatible(pack, out var reason))
        {
            LogService.Instance.Warn($"多语言：语言包不适用 {pack.Id}：{reason}");
            Ui.Show(this, reason, Loc.T("加载失败"), MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (pack.Id.Equals(LocalizationService.Instance.CurrentId, StringComparison.OrdinalIgnoreCase))
        {
            Ui.Info(Loc.T("当前已使用该语言，无需重新加载。"));
            return;
        }

        DateTime? installedAt = pack.BuiltIn || !File.Exists(pack.FilePath)
            ? null
            : File.GetLastWriteTime(pack.FilePath);
        long? size = File.Exists(pack.FilePath) ? new FileInfo(pack.FilePath).Length : null;

        var details = new LanguagePackDetailsDialog(pack, false, installedAt, size) { Owner = this };
        if (details.ShowDialog() != true) return;

        LocalizationService.Instance.SetLanguage(pack.Id);
        LogService.Instance.Info($"多语言：用户选择加载语言包 {pack.Id}（{pack.DisplayName}），准备重启程序。");
        if (!LanguagePackInstaller.RestartApp(out var err))
        {
            LogService.Instance.Error($"多语言：重启程序失败：{err}");
            Ui.Show(this, Loc.Tf("重启程序失败：{0}", err), Loc.T("加载失败"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    /// <summary>选择 .meowlang 文件 → 详情 → 第三方确认 → 进度 → 安装结果。</summary>
    public static void InstallInteractive(Window? owner)
    {
        var ofd = new OpenFileDialog
        {
            Title = Loc.T("选择语言包"),
            Filter = Loc.T("Meow 语言包 (*.meowlang)|*.meowlang|所有文件 (*.*)|*.*"),
            CheckFileExists = true
        };
        if (ofd.ShowDialog(owner) != true) return;

        var source = ofd.FileName;
        var pack = LanguagePackInstaller.ReadFile(source, out var readError);
        if (pack == null)
        {
            LogService.Instance.Error($"多语言：读取语言包失败 {source}：{readError}");
            Ui.Show(owner, readError, Loc.T("安装失败"), MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (pack.Id.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase))
        {
            Ui.Show(owner, Loc.T("简体中文已内嵌在程序中，无需安装语言包。"), Loc.T("安装失败"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!LanguagePackInstaller.CheckCompatible(pack, out var reason))
        {
            LogService.Instance.Warn($"多语言：语言包不适用 {pack.Id}：{reason}");
            Ui.Show(owner, reason, Loc.T("安装失败"), MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var existing = LocalizationService.Instance.Find(pack.Id);
        if (existing != null)
        {
            if (!SameIdentity(existing, pack))
            {
                var msg = Loc.Tf(
                    "无法安装语言包：已安装的同名语言包与待安装语言包的作者或作者网站不一致。\n\n请手动删除旧语言包后重试。\n\n旧语言包：{0}（{1}）",
                    existing.DisplayName, existing.Author);
                LogService.Instance.Warn($"多语言：语言包身份不一致，拒绝覆盖 {pack.Id}。");
                Ui.Show(owner, msg, Loc.T("安装失败"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var cmp = LanguagePackInstaller.CompareVersion(existing.Version, pack.Version);
            var kind = cmp > 0 ? Loc.T("降级") : cmp < 0 ? Loc.T("更新") : Loc.T("重新安装");
            var ok = Ui.Confirm(owner,
                Loc.Tf("检测到已安装同名语言包（版本 {0}）。\n\n将执行{1}操作，安装版本 {2}。是否继续？",
                    existing.Version, kind, pack.Version),
                Loc.T("语言包版本变更确认"));
            if (!ok) return;
        }

        var details = new LanguagePackDetailsDialog(pack, true, DateTime.Now, new FileInfo(source).Length);
        if (owner != null) details.Owner = owner;
        if (details.ShowDialog() != true) return;

        // 第三方语言包需二次确认
        if (!pack.IsOfficial)
        {
            if (!Ui.Confirm(owner, Loc.T("该语言包由第三方个人制作者/社区提供，是否继续安装？"),
                    Loc.T("第三方语言包确认")))
                return;
        }

        var progress = new LanguageInstallProgressDialog(pack, source);
        if (owner != null) progress.Owner = owner;
        progress.ShowDialog();

        if (progress.Success)
        {
            LocalizationService.Instance.Refresh();
            LogService.Instance.Info($"多语言：语言包安装成功 {pack.Id} v{pack.Version}，来源 {source}");
            Ui.Show(owner, Loc.T("语言包安装成功！"), Loc.T("安装完成"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            LogService.Instance.Error($"多语言：语言包安装失败 {pack.Id}：{progress.ErrorMessage}");
            Ui.Show(owner,
                Loc.Tf("语言包安装失败：{0}\n\n详细信息已写入日志：{1}",
                    progress.ErrorMessage, LogService.Instance.LogFilePath),
                Loc.T("安装失败"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static bool SameIdentity(LanguagePack a, LanguagePack b)
        => string.Equals(a.Author?.Trim(), b.Author?.Trim(), StringComparison.OrdinalIgnoreCase)
           && string.Equals(NormalizeSite(a.AuthorWebsite), NormalizeSite(b.AuthorWebsite), StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSite(string? site) => (site ?? string.Empty).Trim().TrimEnd('/');
}
