using System.Windows;
using System.Windows.Controls;

namespace LetsMeowToolbox.App.Views;

/// <summary>独立页面：解除占用（查找占用进程 / 结束进程 / 开机延迟删除）。</summary>
public partial class UnlockView : UserControl
{
    public UnlockView()
    {
        InitializeComponent();
    }

    /// <summary>由卸载页联动调用：设置要解除占用的路径并自动扫描。</summary>
    public void SetPath(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
            Unlocker.SetPath(path);
    }
}
