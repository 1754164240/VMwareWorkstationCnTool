namespace VMwareWorkstationCnTool.Core;

public sealed record VMwareReleaseInfo(string TagName, string HtmlUrl, VMwareReleaseAsset WindowsAsset);

public sealed record VMwareReleaseAsset(string Name, string DownloadUrl, long Size);

public sealed record VMwareInstallerDownloadResult(string FilePath, VMwareReleaseInfo Release);

public sealed record DownloadProgress(long BytesReceived, long? TotalBytes)
{
    public int? Percent => TotalBytes is > 0
        ? (int)Math.Clamp(BytesReceived * 100 / TotalBytes.Value, 0, 100)
        : null;
}
