using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LetsMeowToolbox.App.Services;

/// <summary>Windows 版本识别。用于自动隐藏不兼容选项。</summary>
public static class OsVersionService
{
    private static readonly Lazy<(string name, int build)> _info = new(Detect);

    public static string FriendlyName => _info.Value.name;
    public static int Build => _info.Value.build;
    public static bool IsWin11 => Build >= 22000;
    public static bool IsWin10 => Build >= 10240 && Build < 22000;
    public static bool IsWin10Or11 => Build >= 10240;

    private static (string, int) Detect()
    {
        string name = "Windows";
        int build = 0;
        try
        {
            build = Environment.OSVersion.Version.Build;
        }
        catch { /* ignore */ }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var display = key?.GetValue("DisplayVersion") as string;
            var prod = key?.GetValue("ProductName") as string;
            name = display ?? prod ?? name;
        }
        catch { /* ignore */ }

        if (build >= 22000 && !name.Contains("11"))
            name = "Windows 11";
        else if (build >= 10240 && build < 22000 && !name.Contains("10"))
            name = "Windows 10";
        return (name, build);
    }

    /// <summary>指示是否 x64。</summary>
    public static bool Is64Bit => RuntimeInformation.OSArchitecture == Architecture.X64 ||
                                  RuntimeInformation.OSArchitecture == Architecture.Arm64;
}
