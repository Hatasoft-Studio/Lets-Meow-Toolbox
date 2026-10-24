using System.Text.Json;
using LetsMeowToolbox.App.Models;
using LetsMeowToolbox.App.Utils;

namespace LetsMeowToolbox.App.Modules.Beginner;

/// <summary>🟢 功能4：安全预装 UWP 卸载（仅白名单可删，系统核心 UWP 禁止）。</summary>
public static class UwpCleanModule
{
    /// <summary>
    /// 枚举全部 UWP 应用（排除框架/资源包），并尽量取“开始菜单友好名称”，
    /// 标注是否可卸载 / 是否核心保护；可卸载项排在最前。
    /// </summary>
    public static IReadOnlyList<UwpAppViewModel> List()
    {
        var result = new List<UwpAppViewModel>();
        var r = PowerShellRunner.Run(
            "$ErrorActionPreference='SilentlyContinue'; " +
            "$start=@{}; " +
            "Get-StartApps | ForEach-Object { $fam=($_.AppID -split '!')[0]; if(-not $start.ContainsKey($fam)){ $start[$fam]=$_.Name } }; " +
            "Get-AppxPackage | Where-Object { -not $_.IsFramework } | ForEach-Object { " +
            "$fn=$_.PackageFamilyName; $fr=''; if($start.ContainsKey($fn)){ $fr=$start[$fn] }; " +
            "[PSCustomObject]@{ Name=$_.Name; PackageFullName=$_.PackageFullName; PackageFamilyName=$fn; Version=$_.Version; Friendly=$fr; IsFramework=$_.IsFramework } " +
            "} | ConvertTo-Json -Compress");

        if (!r.Success && string.IsNullOrWhiteSpace(r.Output)) return result;
        try
        {
            var json = r.Output.Trim();
            if (string.IsNullOrWhiteSpace(json)) return result;
            List<AppxJsonEntry> items;
            if (json.StartsWith("["))
                items = JsonSerializer.Deserialize<List<AppxJsonEntry>>(json) ?? new();
            else
                items = new List<AppxJsonEntry> { JsonSerializer.Deserialize<AppxJsonEntry>(json) ?? new() };

            foreach (var it in items)
            {
                if (string.IsNullOrEmpty(it.PackageFullName)) continue;
                if (it.IsFramework == true) continue; // 再保险：框架/运行库不列出
                bool core = UwpWhitelist.IsCoreProtected(it.PackageFullName);
                bool removable = !core && UwpWhitelist.IsRemovable(it.PackageFullName);
                result.Add(new UwpAppViewModel
                {
                    Name = it.Name ?? it.PackageFullName,
                    FriendlyName = it.Friendly ?? string.Empty,
                    PackageFullName = it.PackageFullName,
                    Version = it.Version ?? string.Empty,
                    Removable = removable,
                    CoreProtected = core,
                    IsChecked = removable
                });
            }
        }
        catch { /* 解析失败返回空 */ }

        return result
            .OrderByDescending(a => a.Removable)
            .ThenByDescending(a => a.CoreProtected)
            .ThenBy(a => a.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>卸载单个包。返回 (成功, 消息)。</summary>
    public static (bool, string) Uninstall(string packageFullName)
    {
        var script = $"$ErrorActionPreference='Stop'; Remove-AppxPackage -Package '{packageFullName}'";
        var r = PowerShellRunner.Run(script);
        if (r.Success) return (true, "卸载成功");
        return (false, $"卸载失败：{r.Output.Trim().Split('\n').LastOrDefault() ?? "未知错误"}");
    }
}
