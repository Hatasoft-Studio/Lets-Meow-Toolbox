using Microsoft.Win32;

namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// 开机延迟删除（PendingFileRenameOperations）：调用 MoveFileEx + MOVEFILE_DELAY_UNTIL_REBOOT，
/// 由系统在下次重启时统一执行删除，属于官方支持机制，不涉及任何驱动/句柄剥离手段。
/// </summary>
public static class PendingFileOps
{
    private const string SessionManagerKey = @"SYSTEM\CurrentControlSet\Control\Session Manager";

    /// <summary>安排某个文件/目录在下次重启时被删除。</summary>
    public static bool ScheduleDeleteOnReboot(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return NativeMethods.MoveFileEx(path, null, NativeMethods.MOVEFILE_DELAY_UNTIL_REBOOT);
    }

    /// <summary>读取当前待重启删除的条目（仅用于展示，不修改）。</summary>
    public static IReadOnlyList<string> GetPendingDeletes()
    {
        var list = new List<string>();
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(SessionManagerKey);
            var val = key?.GetValue("PendingFileRenameOperations") as string[];
            if (val == null) return list;
            // 配对：源、目标(空表示删除)。这里只关心“目标为空 = 待删除”。
            for (int i = 0; i + 1 < val.Length; i += 2)
            {
                if (string.IsNullOrEmpty(val[i + 1]))
                    list.Add(val[i]);
            }
        }
        catch { /* 读取失败不阻断 */ }
        return list;
    }
}
