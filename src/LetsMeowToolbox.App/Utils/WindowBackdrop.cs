using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// Win11 Fluent 窗口背景：优先启用 Mica（DWMWA_SYSTEMBACKDROP_TYPE=38，值 2=Mica），
/// 失败或非 Win11 时回退到纯色窗口背景，保证任何系统都能正常显示。
/// </summary>
public static class WindowBackdrop
{
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMSBT_MAINWINDOW = 2; // Mica

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    public static void EnableMica(Window window, bool dark)
    {
        // 仅 Win11 支持 Mica
        if (!Services.OsVersionService.IsWin11)
        {
            ApplyFallbackColor(window, dark);
            return;
        }
        try
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;

            int backdrop = DWMSBT_MAINWINDOW;
            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
            int darkMode = dark ? 1 : 0;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
        }
        catch
        {
            ApplyFallbackColor(window, dark);
        }
    }

    private static void ApplyFallbackColor(Window window, bool dark)
    {
        try
        {
            window.Background = new SolidColorBrush(
                dark ? Color.FromRgb(0x20, 0x20, 0x20) : Color.FromRgb(0xF6, 0xF6, 0xF6));
        }
        catch { /* ignore */ }
    }
}
