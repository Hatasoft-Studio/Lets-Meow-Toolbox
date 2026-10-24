using System.Diagnostics;
using System.IO;

namespace LetsMeowToolbox.App.Utils;

/// <summary>
/// 基于 RestartManager 的占用检测。对外描述统一使用“查找占用进程 / 关闭程序释放占用”，
/// 不使用任何“强脱/暴力/破解”类敏感表述。
/// </summary>
public sealed class OccupiedProcessInfo
{
    public required int ProcessId { get; init; }
    public required string Name { get; init; }
    public string? AppName { get; init; }
    public string? ServiceName { get; init; }
    public bool IsService { get; init; }
    public bool Restartable { get; init; }
}

public static class RestartManager
{
    /// <summary>查找锁定指定文件/目录的进程。</summary>
    public static IReadOnlyList<OccupiedProcessInfo> FindLockingProcesses(string path)
    {
        var list = new List<OccupiedProcessInfo>();
        if (string.IsNullOrWhiteSpace(path)) return list;

        uint session = 0;
        string key = Guid.NewGuid().ToString();
        int ret = NativeMethods.RmStartSession(out session, 0, key);
        if (ret != NativeMethods.ERROR_SUCCESS) return list;

        try
        {
            ret = NativeMethods.RmRegisterResources(session, 1, new[] { path }, 0, null, 0, null);
            if (ret != NativeMethods.ERROR_SUCCESS && ret != NativeMethods.ERROR_MORE_DATA) return list;

            uint needed = 0;
            uint count = 0;
            uint rebootReasons = 0;
            ret = NativeMethods.RmGetList(session, out needed, ref count, Array.Empty<NativeMethods.RM_PROCESS_INFO>(), ref rebootReasons);
            if (ret == NativeMethods.ERROR_MORE_DATA || ret == NativeMethods.ERROR_SUCCESS)
            {
                uint n = Math.Max(needed, count);
                if (n == 0) return list;
                var infos = new NativeMethods.RM_PROCESS_INFO[n];
                count = n;
                rebootReasons = 0;
                ret = NativeMethods.RmGetList(session, out needed, ref count, infos, ref rebootReasons);
                if (ret == NativeMethods.ERROR_SUCCESS)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var info = infos[i];
                        if (info.Process.dwProcessId == 0) continue;
                        string name;
                        try { name = Process.GetProcessById(info.Process.dwProcessId).ProcessName; }
                        catch { name = string.Empty; }
                        list.Add(new OccupiedProcessInfo
                        {
                            ProcessId = info.Process.dwProcessId,
                            Name = string.IsNullOrEmpty(name) ? "(未知进程)" : name,
                            AppName = string.IsNullOrWhiteSpace(info.strAppName) ? null : info.strAppName,
                            ServiceName = string.IsNullOrWhiteSpace(info.strServiceShortName) ? null : info.strServiceShortName,
                            IsService = info.ApplicationType == NativeMethods.RM_APP_TYPE.RmService,
                            Restartable = info.bRestartable
                        });
                    }
                }
            }
        }
        finally
        {
            NativeMethods.RmEndSession(session);
        }
        return list;
    }

    /// <summary>尝试以独占方式打开文件，用于确认其是否仍被占用。返回 null 表示可独占（未占用）。</summary>
    public static string? TryExclusiveOpen(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return "被其他进程以写权限占用（或需要管理员权限）";
        }
        catch (IOException)
        {
            return "文件正在被占用，无法独占访问";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
