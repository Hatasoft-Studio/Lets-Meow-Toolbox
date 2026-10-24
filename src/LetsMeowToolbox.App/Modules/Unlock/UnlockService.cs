using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Unlock;

/// <summary>解除占用模块：查找占用进程、关闭程序释放占用、开机延迟删除。</summary>
public static class UnlockService
{
    /// <summary>查找占用指定路径的进程。</summary>
    public static IReadOnlyList<UnlockProcessViewModel> FindOccupiers(string path)
    {
        var result = new List<UnlockProcessViewModel>();
        foreach (var info in RestartManager.FindLockingProcesses(path))
        {
            result.Add(new UnlockProcessViewModel
            {
                ProcessId = info.ProcessId,
                ProcessName = info.Name,
                AppName = info.AppName ?? info.Name,
                IsService = info.IsService,
                Restartable = info.Restartable
            });
        }
        return result;
    }

    /// <summary>深度扫描（可选）：额外尝试独占打开，确认锁定状态。</summary>
    public static string? DeepProbe(string path)
        => RestartManager.TryExclusiveOpen(path);

    /// <summary>关闭进程释放占用（带核心进程黑名单保护）。</summary>
    public static (bool ok, string message) CloseProcess(int pid, string processName, bool force)
    {
        if (ProcessUtil.IsProtectedProcess(processName, pid))
            return (false, "该进程属于系统核心进程，出于安全考虑已禁止结束");
        bool ok = ProcessUtil.CloseProcess(pid, force);
        return (ok, ok ? "已关闭该程序" : "关闭失败，进程可能已退出或无权限");
    }

    /// <summary>安排开机延迟删除。</summary>
    public static bool ScheduleRebootDelete(string path)
        => PendingFileOps.ScheduleDeleteOnReboot(path);

    /// <summary>当前待重启删除的条目。</summary>
    public static IReadOnlyList<string> PendingDeletes()
        => PendingFileOps.GetPendingDeletes();
}
