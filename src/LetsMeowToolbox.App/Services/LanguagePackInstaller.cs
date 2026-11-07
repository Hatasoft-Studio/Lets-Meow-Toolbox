using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace LetsMeowToolbox.App.Services;

/// <summary>官方语言包判定与版本规则。</summary>
public static class LanguagePackRules
{
    public const string OfficialAuthor = "Hatasoft-Studio";
    public const string OfficialSite = "https://hatasoft365.top";
    public const string OfficialSiteAlt = "https://lets-meow-toolbox.hatasoft365.top";

    /// <summary>作者与官网必须同时匹配才算官方语言包。</summary>
    public static bool IsOfficial(string? author, string? website)
    {
        var site = (website ?? string.Empty).Trim().TrimEnd('/');
        return string.Equals((author ?? string.Empty).Trim(), OfficialAuthor, StringComparison.OrdinalIgnoreCase)
               && (string.Equals(site, OfficialSite, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(site, OfficialSiteAlt, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>语言包读取、校验、安装与重启。</summary>
public static class LanguagePackInstaller
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>读取并校验语言包文件；失败时返回 null 并给出原因。</summary>
    public static LanguagePack? ReadFile(string path, out string error)
    {
        error = string.Empty;
        try
        {
            if (!File.Exists(path))
            {
                error = Loc.Tf("语言包文件不存在：{0}", path);
                return null;
            }

            var pack = JsonSerializer.Deserialize<LanguagePack>(File.ReadAllText(path), JsonOptions);
            if (pack == null)
            {
                error = Loc.T("语言包内容为空或格式无效。");
                return null;
            }

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(pack.Id)) missing.Add("id");
            if (string.IsNullOrWhiteSpace(pack.Name)) missing.Add("name");
            if (string.IsNullOrWhiteSpace(pack.Author)) missing.Add("author");
            if (string.IsNullOrWhiteSpace(pack.Version)) missing.Add("version");
            if (pack.Strings.Count == 0) missing.Add("strings");
            if (missing.Count > 0)
            {
                error = Loc.Tf("语言包缺少必填字段：{0}", string.Join(", ", missing));
                return null;
            }

            pack.FilePath = path;
            return pack;
        }
        catch (Exception ex)
        {
            error = Loc.Tf("语言包解析失败：{0}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// 校验语言包声明的适用版本是否匹配当前程序版本。
    /// 支持用 <c>|</c> 分隔多个版本（如 <c>0.2.0 | 0.2.1 | 0.2.2</c>，表示任一版本均适用），
    /// 也支持 <c>&gt;=</c>、<c>&lt;=</c>、<c>&gt;</c>、<c>&lt;</c>、<c>=</c> 与 <c>*</c> 约束。
    /// </summary>
    public static bool CheckCompatible(LanguagePack pack, out string reason)
    {
        reason = string.Empty;
        var range = (pack.AppVersion ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(range) || range == "*") return true;

        if (range.Contains('|'))
        {
            foreach (var part in range.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                if (MatchesRange(part.Trim())) return true;
            }
        }
        else if (MatchesRange(range))
        {
            return true;
        }

        reason = Loc.Tf("该语言包适用于 {0}，当前软件版本为 v{1}。", range, AppInfo.Version);
        return false;
    }

    /// <summary>判断单段版本约束（可用空格/逗号组合多个条件，需全部满足）是否匹配当前版本。</summary>
    private static bool MatchesRange(string range)
    {
        var current = ParseVersion(AppInfo.Version) ?? new Version(0, 0);
        foreach (var token in range.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var t = token.Trim();
            if (string.IsNullOrEmpty(t) || t == "*") continue;

            string op = "==";
            foreach (var candidate in new[] { ">=", "<=", ">", "<", "=", "==" })
            {
                if (t.StartsWith(candidate, StringComparison.Ordinal))
                {
                    op = candidate;
                    t = t[candidate.Length..].Trim();
                    break;
                }
            }

            var v = ParseVersion(t);
            if (v == null) continue; // 无法解析的约束忽略，避免误杀

            var ok = op switch
            {
                ">=" => current >= v,
                "<=" => current <= v,
                ">" => current > v,
                "<" => current < v,
                _ => Normalize(current) == Normalize(v)
            };
            if (!ok) return false;
        }
        return true;
    }

    /// <summary>比较两个版本号，返回负数/0/正数。</summary>
    public static int CompareVersion(string? a, string? b)
    {
        var va = ParseVersion(a) ?? new Version(0, 0);
        var vb = ParseVersion(b) ?? new Version(0, 0);
        return va.CompareTo(vb);
    }

    private static Version? ParseVersion(string? text)
    {
        var s = (text ?? string.Empty).Trim().TrimStart('v', 'V');
        if (s.Length == 0) return null;
        var parts = s.Split('.');
        var nums = new List<int>();
        foreach (var p in parts)
        {
            if (int.TryParse(p, out var n)) nums.Add(n);
            else break;
        }
        while (nums.Count < 2) nums.Add(0);
        var v = new Version(nums[0], nums[1],
            nums.Count > 2 ? nums[2] : 0, nums.Count > 3 ? nums[3] : 0);
        return v;
    }

    private static string Normalize(Version v) => $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";

    /// <summary>语言包安装目标路径（按语言编码命名，保证同名语言包唯一）。</summary>
    public static string InstalledPath(string id)
    {
        var safe = new string(id.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
        return Path.Combine(LocalizationService.LanguageDirectory, safe + ".meowlang");
    }

    /// <summary>复制语言包到 lang 目录，并记录安装时间。</summary>
    public static void Install(LanguagePack pack, string sourcePath)
    {
        var dir = LocalizationService.LanguageDirectory;
        Directory.CreateDirectory(dir);
        var dest = InstalledPath(pack.Id);
        var tmp = dest + ".tmp";

        File.Copy(sourcePath, tmp, true);
        if (File.Exists(dest)) File.Delete(dest);
        File.Move(tmp, dest);
        File.SetLastWriteTime(dest, DateTime.Now);
    }

    /// <summary>重启程序（用于加载新语言）。</summary>
    public static bool RestartApp(out string error)
    {
        error = string.Empty;
        try
        {
            var exe = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exe))
            {
                error = Loc.T("无法定位当前程序路径。");
                return false;
            }
            Process.Start(new ProcessStartInfo(exe)
            {
                UseShellExecute = true,
                WorkingDirectory = AppContext.BaseDirectory
            });
            Application.Current.Shutdown();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>把字节数格式化为易读文本。</summary>
    public static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / 1024.0 / 1024.0:F2} MB";
    }
}
