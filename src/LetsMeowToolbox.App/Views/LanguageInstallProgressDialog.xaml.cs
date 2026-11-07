using System.Windows;
using LetsMeowToolbox.App.Services;

namespace LetsMeowToolbox.App.Views;

/// <summary>安装进度窗口：复制语言包到 ./lang 并刷新目录。</summary>
public partial class LanguageInstallProgressDialog : Window
{
    private readonly LanguagePack _pack;
    private readonly string _sourcePath;
    private bool _running = true;

    public bool Success { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    public LanguageInstallProgressDialog(LanguagePack pack, string sourcePath)
    {
        InitializeComponent();
        _pack = pack;
        _sourcePath = sourcePath;

        Title = Loc.T("安装语言包");
        TxtTitle.Text = Loc.T("正在安装语言包");
        SetProgress(0, Loc.T("准备安装…"));

        Closing += (_, e) => { if (_running) e.Cancel = true; };
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            SetProgress(15, Loc.T("校验语言包…"));
            await Task.Delay(180);

            SetProgress(35, Loc.T("准备安装…"));
            await Task.Delay(180);

            SetProgress(55, Loc.T("复制语言包文件…"));
            LanguagePackInstaller.Install(_pack, _sourcePath);
            await Task.Delay(120);

            SetProgress(80, Loc.T("刷新语言包目录…"));
            LocalizationService.Instance.Refresh();
            await Task.Delay(220);

            SetProgress(100, Loc.T("安装完成"));
            Success = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            LogService.Instance.Error($"多语言：语言包安装异常 {_pack.Id}\n{ex}");
        }
        finally
        {
            _running = false;
            Close();
        }
    }

    private void SetProgress(double percent, string status)
    {
        Bar.Value = percent;
        TxtPercent.Text = $"{percent:F0}%";
        TxtStatus.Text = status;
    }
}
