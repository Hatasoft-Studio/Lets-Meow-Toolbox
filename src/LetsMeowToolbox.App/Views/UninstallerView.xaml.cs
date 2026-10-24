using System.Windows.Controls;

namespace LetsMeowToolbox.App.Views;

/// <summary>独立页面：卸载程序（Win32 软件批量卸载 / 残留扫描 / 清理报告）。</summary>
public partial class UninstallerView : UserControl
{
    public UninstallerView()
    {
        InitializeComponent();
    }

    private void OnRequestUnlock(string path)
    {
        MainWindow.Instance?.GoToUnlock(path);
    }
}
