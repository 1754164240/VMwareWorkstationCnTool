using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace VMwareWorkstationCnTool.Core;

public sealed class VMwareReleaseService
{
    public const string ReleasesUrl = "https://github.com/201853910/VMwareWorkstation/releases/";
    private const string LatestReleaseApiUrl = "https://api.github.com/repos/201853910/VMwareWorkstation/releases/latest";
    private const string RepositoryBaseUrl = "https://github.com";
    private const string LatestReleasePageUrl = "https://github.com/201853910/VMwareWorkstation/releases/latest";
    private const string ExpandedAssetsUrlPrefix = "https://github.com/201853910/VMwareWorkstation/releases/expanded_assets/";
    private readonly HttpClient httpClient;

    public VMwareReleaseService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        if (this.httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            this.httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VMwareWorkstationCnTool", "1.0.0"));
        }
    }

    public static VMwareReleaseInfo ParseLatestRelease(string json)
    {
        var release = JsonSerializer.Deserialize<GitHubReleaseResponse>(json)
            ?? throw new InvalidOperationException("无法解析 VMware Workstation 最新版本信息。");

        var windowsAsset = release.Assets
            .Where(asset => asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(asset => asset.Size)
            .FirstOrDefault();

        if (windowsAsset is null)
        {
            throw new InvalidOperationException("最新 Release 中没有找到 Windows exe 安装包。");
        }

        return new VMwareReleaseInfo(
            release.TagName,
            release.HtmlUrl,
            new VMwareReleaseAsset(windowsAsset.Name, windowsAsset.BrowserDownloadUrl, windowsAsset.Size));
    }

    public static VMwareReleaseInfo ParseLatestReleasePage(string html)
    {
        var tagName = ParseLatestTagFromReleasePage(html, LatestReleasePageUrl);
        return ParseExpandedAssetsPage(tagName, html);
    }

    public static string ParseLatestTagFromReleasePage(string html, string finalUrl)
    {
        var expandedAssetsMatch = Regex.Match(
            html,
            "releases/expanded_assets/(?<tag>[^\"?&<>]+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        if (expandedAssetsMatch.Success)
        {
            return Uri.UnescapeDataString(expandedAssetsMatch.Groups["tag"].Value);
        }

        var uri = new Uri(finalUrl);
        var tagPrefix = "/201853910/VMwareWorkstation/releases/tag/";
        var tagIndex = uri.AbsolutePath.IndexOf(tagPrefix, StringComparison.OrdinalIgnoreCase);
        if (tagIndex >= 0)
        {
            return Uri.UnescapeDataString(uri.AbsolutePath[(tagIndex + tagPrefix.Length)..]);
        }

        throw new InvalidOperationException("无法从下载页识别最新版标签。");
    }

    public static VMwareReleaseInfo ParseExpandedAssetsPage(string tagName, string html)
    {
        var matches = Regex.Matches(
            html,
            "href=\"(?<href>/201853910/VMwareWorkstation/releases/download/(?<tag>[^/]+)/(?<name>[^\"]+\\.exe))\"",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var match = matches
            .Cast<Match>()
            .FirstOrDefault(item => item.Success);

        if (match is null)
        {
            throw new InvalidOperationException("下载页中没有找到 Windows exe 安装包。");
        }

        var assetName = Uri.UnescapeDataString(match.Groups["name"].Value);
        var downloadUrl = RepositoryBaseUrl + match.Groups["href"].Value;

        return new VMwareReleaseInfo(
            tagName,
            $"{ReleasesUrl}tag/{tagName}",
            new VMwareReleaseAsset(assetName, downloadUrl, 0));
    }

    public async Task<VMwareReleaseInfo> GetLatestReleaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(LatestReleaseApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseLatestRelease(json);
        }
        catch (HttpRequestException)
        {
            return await GetLatestReleaseFromPageAsync(cancellationToken);
        }
    }

    public async Task<VMwareInstallerDownloadResult> DownloadLatestWindowsInstallerAsync(string targetDirectory, CancellationToken cancellationToken)
    {
        var release = await GetLatestReleaseAsync(cancellationToken);
        Directory.CreateDirectory(targetDirectory);

        var filePath = Path.Combine(targetDirectory, release.WindowsAsset.Name);
        using var response = await httpClient.GetAsync(release.WindowsAsset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destination = File.Create(filePath);
        await source.CopyToAsync(destination, cancellationToken);

        return new VMwareInstallerDownloadResult(filePath, release);
    }

    public static string GetDefaultDownloadDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloads = Path.Combine(userProfile, "Downloads");
        return Directory.Exists(downloads) ? downloads : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    }

    private async Task<VMwareReleaseInfo> GetLatestReleaseFromPageAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(LatestReleasePageUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        var tagName = ParseLatestTagFromReleasePage(html, response.RequestMessage?.RequestUri?.AbsoluteUri ?? LatestReleasePageUrl);

        using var assetsResponse = await httpClient.GetAsync(ExpandedAssetsUrlPrefix + Uri.EscapeDataString(tagName), cancellationToken);
        assetsResponse.EnsureSuccessStatusCode();

        var assetsHtml = await assetsResponse.Content.ReadAsStringAsync(cancellationToken);
        return ParseExpandedAssetsPage(tagName, assetsHtml);
    }

    private sealed class GitHubReleaseResponse
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; init; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; init; } = string.Empty;

        [JsonPropertyName("assets")]
        public IReadOnlyList<GitHubAssetResponse> Assets { get; init; } = Array.Empty<GitHubAssetResponse>();
    }

    private sealed class GitHubAssetResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; init; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; init; }
    }
}
