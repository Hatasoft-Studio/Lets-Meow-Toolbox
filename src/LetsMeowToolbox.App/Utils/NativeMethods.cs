using System.Runtime.InteropServices;

namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// 微软原生 Win32 API 的 P/Invoke 封装。全部结构定义以官方文档为准。
/// </summary>
internal static class NativeMethods
{
    // ---------- 重启删除（MoveFileEx / PendingFileRenameOperations） ----------
    public const uint MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;
    public const uint MOVEFILE_REPLACE_EXISTING = 0x00000001;
    public const uint MOVEFILE_WRITE_THROUGH = 0x00000008;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool MoveFileEx(string? lpExistingFileName, string? lpNewFileName, uint dwFlags);

    // ---------- RestartManager（解除占用首选方案，微软官方 API） ----------
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RM_UNIQUE_PROCESS
    {
        public int dwProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    public enum RM_APP_TYPE : uint
    {
        RmUnknownApp = 0,
        RmMainWindow = 1,
        RmOtherWindow = 2,
        RmService = 3,
        RmExplorer = 4,
        RmConsole = 5,
        RmCritical = 1000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RM_PROCESS_INFO
    {
        public RM_UNIQUE_PROCESS Process;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string strAppName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string strServiceShortName;
        public RM_APP_TYPE ApplicationType;
        public uint AppStatus;
        public uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bRestartable;
    }

    // RmGetList / RmRegisterResources 等返回值
    public const int ERROR_SUCCESS = 0;
    public const int ERROR_MORE_DATA = 234;
    public const int ERROR_SESSION_CREDENTIALS_CONFLICT = 1219;

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    public static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    public static extern int RmEndSession(uint pSessionHandle);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    public static extern int RmRegisterResources(
        uint pSessionHandle,
        uint nFiles,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgsFilenames,
        uint nApplications,
        [MarshalAs(UnmanagedType.LPArray)] RM_UNIQUE_PROCESS[]? rgApplications,
        uint nServices,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[]? rgsServiceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    public static extern int RmGetList(
        uint dwSessionHandle,
        out uint pnProcInfoNeeded,
        ref uint pnProcInfo,
        [In, Out, MarshalAs(UnmanagedType.LPArray)] RM_PROCESS_INFO[] rgAffectedApps,
        ref uint lpdwRebootReasons);

    // ---------- 是否以管理员身份运行 ----------
    [DllImport("shell32.dll")]
    public static extern bool IsUserAnAdmin();
}
