using Microsoft.Win32;

namespace LetsMeowToolbox.Plugins.HighRisk.Actions;

/// <summary>
/// 高危：关闭 Windows Defender 的实时防护与病毒扫描。
/// 双通道：①写“Windows Defender 防病毒策略”注册表；②尽力调用 Set-MpPreference 即时关闭。
/// 状态以 Get-MpComputerStatus 的实时防护真实状态为准。
/// 重要：Windows 11 若“篡改防护”开启，策略与命令都会被系统忽略，需用户先手动关闭篡改防护。
/// </summary>
internal sealed class DefenderAction : PluginAbstractions.IHighRiskAction
{
    private const string PolicyRoot = @"SOFTWARE\Policies\Microsoft\Windows Defender";
    private const string RtKey = PolicyRoot + @"\Real-Time Protection";

    public string Id => "defender.disable";
    public string Title => "关闭 Windows Defender";
    public string Description =>
        "停用 Windows 安全中心（Defender）的实时防护与病毒扫描。\n" +
        "适合安装了第三方杀毒、或在内网/离线环境统一管理的场景。关闭后系统将缺少微软原生防护。";
    public string Warning =>
        "⚠ 高危操作：关闭后将不再有 Windows 自带病毒防护，系统安全风险显著上升！\n" +
        "• Windows 11 必须先在“Windows 安全中心 → 病毒和威胁防护 → 管理设置”里手动关闭“篡改防护”，否则本操作不会生效。\n" +
        "• 建议仅在已安装可靠第三方安全软件，或明确知晓后果时启用。\n" +
        "• 本操作不会删除任何文件，仅修改系统策略，可通过“还原”恢复。";
    public string RegistryNote => "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\DisableAntiSpyware + Set-MpPreference";
    public bool RequiresAdmin => true;

    public bool IsAvailable() => true;

    /// <summary>读取实时防护真实状态：true=已关闭（生效），false=仍开启，null=无法判断。</summary>
    private static bool? RealTimeProtectionOff()
    {
        var r = HighRiskUtil.RunCapture("powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"try { Write-Output (Get-MpComputerStatus).RealTimeProtectionEnabled } catch { Write-Output 'UNKNOWN' }\"",
            60_000);
        var s = r.output.Trim().ToLowerInvariant();
        if (s.Contains("false")) return true;
        if (s.Contains("true")) return false;
        return null;
    }

    public bool IsApplied()
    {
        var off = RealTimeProtectionOff();
        if (off.HasValue) return off.Value;
        // 取不到实时状态时退回看策略键
        return HighRiskUtil.GetDword(PolicyRoot, "DisableAntiSpyware") == 1;
    }

    public bool Apply()
    {
        // 1) 写策略键（重启/篡改防护关闭后生效，且能持久保持）
        bool ok = true;
        ok &= HighRiskUtil.SetPolicyDword(PolicyRoot, "DisableAntiSpyware", 1);
        ok &= HighRiskUtil.SetPolicyDword(PolicyRoot, "DisableAntiVirus", 1);
        ok &= HighRiskUtil.SetPolicyDword(RtKey, "DisableRealtimeMonitoring", 1);
        ok &= HighRiskUtil.SetPolicyDword(RtKey, "DisableBehaviorMonitoring", 1);
        ok &= HighRiskUtil.SetPolicyDword(RtKey, "DisableIOAVProtection", 1);
        ok &= HighRiskUtil.SetPolicyDword(RtKey, "DisableOnAccessProtection", 1);

        // 2) 尽力即时关闭（Win10、或 Win11 已关闭篡改防护时立即生效）
        HighRiskUtil.RunAndWait("powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"try { Set-MpPreference -DisableRealtimeMonitoring $true -DisableBehaviorMonitoring $true -DisableIOAVProtection $true } catch {}\"",
            60_000);

        // 3) 校验真实状态；仍开启则说明被篡改防护拦截
        var off = RealTimeProtectionOff();
        if (off == false)
        {
            throw new InvalidOperationException(
                "策略已写入，但实时防护仍在运行——这是 Windows 11“篡改防护”阻止所致。\n\n" +
                "请先手动操作：Windows 安全中心 → 病毒和威胁防护 → 管理设置 → 关闭“篡改防护”，\n" +
                "然后回到本页再次点击“启用（高危）”。篡改防护由系统保护，任何程序都无法绕过。");
        }
        return ok;
    }

    public bool Restore()
    {
        // 恢复策略（删除策略键）
        bool ok = true;
        ok &= HighRiskUtil.DeleteValue(PolicyRoot, "DisableAntiSpyware");
        ok &= HighRiskUtil.DeleteValue(PolicyRoot, "DisableAntiVirus");
        ok &= HighRiskUtil.DeleteValue(RtKey, "DisableRealtimeMonitoring");
        ok &= HighRiskUtil.DeleteValue(RtKey, "DisableBehaviorMonitoring");
        ok &= HighRiskUtil.DeleteValue(RtKey, "DisableIOAVProtection");
        ok &= HighRiskUtil.DeleteValue(RtKey, "DisableOnAccessProtection");

        // 尽力重新打开实时防护
        HighRiskUtil.RunAndWait("powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"try { Set-MpPreference -DisableRealtimeMonitoring $false -DisableBehaviorMonitoring $false -DisableIOAVProtection $false } catch {}\"",
            60_000);
        return ok;
    }
}
