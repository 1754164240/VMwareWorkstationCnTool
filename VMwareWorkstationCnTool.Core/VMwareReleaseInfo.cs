namespace VMwareWorkstationCnTool.Core;

public sealed record VMwareReleaseInfo(string TagName, string HtmlUrl, VMwareReleaseAsset WindowsAsset);

public sealed record VMwareReleaseAsset(string Name, string DownloadUrl, long Size);

public sealed record VMwareInstallerDownloadResult(string FilePath, VMwareReleaseInfo Release);
