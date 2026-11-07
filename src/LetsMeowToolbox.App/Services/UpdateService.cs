using System.Net.Http;
using System.Text.Json;

namespace LetsMeowToolbox.App.Services;

/// <summary>更新接口返回的版本信息。</summary>
public sealed class UpdateInfo
{
    public string LatestVersion { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string[] UnsupportedVersions { get; set; } = Array.Empty<string>();
    public string Changelog { get; set; } = string.Empty;
}

/// <summary>更新检查结果。</summary>
public sealed class UpdateCheckResult
{
    public bool Success { get; init; }
    public string Error { get; init; } = string.Empty;
    public UpdateInfo? Info { get; init; }
    /// <summary>是否存在比当前更新的版本。</summary>
    public bool HasUpdate { get; init; }
    /// <summary>当前版本是否已停止支持（必须更新后才能继续使用）。</summary>
    public bool Unsupported { get; init; }
}

/// <summary>
/// 软件更新检查：向官方更新接口请求最新版本号、下载地址与已停止支持的版本列表。
/// 接口地址固定为 https://api.lets-meow-toolbox.hatasoft365.top/
/// </summary>
public static class UpdateService
{
    public const string Endpoint = "https://api.lets-meow-toolbox.hatasoft365.top/";
    public const string DefaultDownloadUrl = "https://lets-meow-toolbox.hatasoft365.top";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>请求更新接口并与当前版本比较。</summary>
    public static async Task<UpdateCheckResult> CheckAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
            request.Headers.TryAddWithoutValidation("User-Agent", $"LetsMeowToolbox/{AppInfo.Version}");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            using var response = await Http.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = Loc.Tf("服务器返回 HTTP {0}", (int)response.StatusCode);
                LogService.Instance.Warn($"更新检查：{error}");
                return new UpdateCheckResult { Success = false, Error = error };
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var info = JsonSerializer.Deserialize<UpdateInfo>(json, JsonOptions);
            if (info == null || string.IsNullOrWhiteSpace(info.LatestVersion))
            {
                var error = Loc.T("更新接口返回的数据无效。");
                LogService.Instance.Warn($"更新检查：{error}");
                return new UpdateCheckResult { Success = false, Error = error };
            }

            info.LatestVersion = info.LatestVersion.Trim();
            info.UnsupportedVersions ??= Array.Empty<string>();
            if (string.IsNullOrWhiteSpace(info.DownloadUrl)) info.DownloadUrl = DefaultDownloadUrl;

            var hasUpdate = LanguagePackInstaller.CompareVersion(info.LatestVersion, AppInfo.Version) > 0;
            var unsupported = info.UnsupportedVersions.Any(
                v => LanguagePackInstaller.CompareVersion(v, AppInfo.Version) == 0);

            LogService.Instance.Info(
                $"更新检查：当前 v{AppInfo.Version}，最新 v{info.LatestVersion}，" +
                $"{(hasUpdate ? "有新版本" : "已是最新")}，{(unsupported ? "当前版本已停止支持" : "版本受支持")}。");

            return new UpdateCheckResult
            {
                Success = true,
                Info = info,
                HasUpdate = hasUpdate,
                Unsupported = unsupported
            };
        }
        catch (Exception ex)
        {
            LogService.Instance.Warn($"更新检查失败：{ex.Message}");
            return new UpdateCheckResult { Success = false, Error = ex.Message };
        }
    }
}
