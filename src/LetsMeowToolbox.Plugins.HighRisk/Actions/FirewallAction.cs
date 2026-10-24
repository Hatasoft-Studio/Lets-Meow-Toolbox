using Microsoft.Win32;

namespace LetsMeowToolbox.Plugins.HighRisk.Actions;

/// <summary>
/// 高危：关闭 Windows 防火墙（三个配置文件全部关闭）。
/// 先用 INetFwPolicy2 COM 立即生效（netsh 兜底），再写入 WindowsFirewall 策略键保证重启后仍生效。
/// 状态以 COM 实时读取为准，而不是仅看我们写入的注册表值。
/// </summary>
internal sealed class FirewallAction : PluginAbstractions.IHighRiskAction
{
    // 实时状态（SharedAccess 服务）
    private const string LiveBase = @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy";
    // 组策略持久化（重启后仍保持）
    private const string PolicyBase = @"SOFTWARE\Policies\Microsoft\WindowsFirewall";
    private static readonly string[] Profiles = { "StandardProfile", "DomainProfile", "PublicProfile" };

    public string Id => "firewall.disable";
    public string Title => "关闭 Windows 防火墙";
    public string Description =>
        "立即关闭专用、域、公用三个网络配置文件的 Windows 防火墙，并通过策略保持该状态。\n" +
        "适合已部署硬件/软件防火墙，或需要在局域网内完全开放端口的场景。";
    public string Warning =>
        "⚠ 高危操作：关闭后本机不再拦截入站网络连接，公网/局域网直接暴露！\n" +
        "• 请确认你已有其他层级的防火墙（如路由器 ACL、硬件防火墙）。\n" +
        "• 仅限有明确需要的场景，可通过“还原”一键恢复。";
    public string RegistryNote => "INetFwPolicy2 + HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\*Profile\\EnableFirewall";
    public bool RequiresAdmin => true;

    public bool IsAvailable() => true;

    public bool IsApplied()
    {
        var live = HighRiskUtil.IsFirewallEnabled();
        if (live.HasValue) return live.Value == false;
        // COM 不可用时退回看策略/实时注册表
        return HighRiskUtil.GetDword($@"{LiveBase}\StandardProfile", "EnableFirewall") == 0;
    }

    public bool Apply()
    {
        // 1) 立即生效：COM 优先，netsh 兜底
        var com = HighRiskUtil.SetFirewallEnabled(false);
        if (com != true)
        {
            var r = HighRiskUtil.RunAndWait("netsh.exe", "advfirewall set allprofiles state off");
            if (r != 0 && r != 1)
                throw new InvalidOperationException("防火墙关闭指令执行失败（netsh 退出码 " + r + "）。");
        }

        // 2) 持久化：写策略键与实时键
        bool ok = true;
        foreach (var p in Profiles)
        {
            ok &= HighRiskUtil.SetPolicyDword($@"{PolicyBase}\{p}", "EnableFirewall", 0);
            ok &= HighRiskUtil.SetPolicyDword($@"{LiveBase}\{p}", "EnableFirewall", 0);
        }

        // 3) 校验真实状态
        var enabled = HighRiskUtil.IsFirewallEnabled();
        if (enabled == true)
            throw new InvalidOperationException("已下发关闭指令，但防火墙仍处于开启状态（可能被安全软件或组策略托管）。");
        return ok;
    }

    public bool Restore()
    {
        var com = HighRiskUtil.SetFirewallEnabled(true);
        if (com != true)
        {
            var r = HighRiskUtil.RunAndWait("netsh.exe", "advfirewall set allprofiles state on");
            if (r != 0 && r != 1)
                throw new InvalidOperationException("防火墙开启指令执行失败（netsh 退出码 " + r + "）。");
        }

        bool ok = true;
        foreach (var p in Profiles)
        {
            HighRiskUtil.DeleteValue($@"{PolicyBase}\{p}", "EnableFirewall");
            ok &= HighRiskUtil.SetPolicyDword($@"{LiveBase}\{p}", "EnableFirewall", 1);
        }
        return ok;
    }
}
