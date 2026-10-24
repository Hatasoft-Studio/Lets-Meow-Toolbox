using System.Windows;
using LetsMeowToolbox.App.Views;

namespace LetsMeowToolbox.App.Services;

/// <summary>UI 交互辅助：自研 Fluent 确认弹窗与提示条（不再使用 HandyControl 的 MessageBox/Growl，避免黑屏）。</summary>
public static class Ui
{
    /// <summary>确认弹窗，返回是否确认。</summary>
    public static bool Confirm(System.Windows.Window? owner, string text, string title = "请确认")
        => FluentDialog.Show(owner, text, title,
            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;

    /// <summary>信息/结果弹窗（兼容原 HandyControl.MessageBox 调用方）。</summary>
    public static MessageBoxResult Show(
        System.Windows.Window? owner, string text, string title = "提示",
        MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information,
        MessageBoxResult defaultResult = MessageBoxResult.None)
        => FluentDialog.Show(owner, text, title, buttons, image, defaultResult);

    public static void Success(string message) => FluentToast.Show(message, ToastKind.Success);
    public static void Info(string message) => FluentToast.Show(message, ToastKind.Info);
    public static void Warning(string message) => FluentToast.Show(message, ToastKind.Warning);
    public static void Error(string message) => FluentToast.Show(message, ToastKind.Error);
}
