namespace VMwareWorkstationCnTool.Core;

public sealed record VMwareInstallationInfo(bool IsFound, string InstallPath, string Version, string Message)
{
    public static VMwareInstallationInfo Found(string installPath, string version)
    {
        return new VMwareInstallationInfo(true, NormalizePath(installPath), string.IsNullOrWhiteSpace(version) ? "未知版本" : version, "已找到 VMware Workstation。");
    }

    public static VMwareInstallationInfo NotFound(string message)
    {
        return new VMwareInstallationInfo(false, string.Empty, "未知版本", message);
    }

    private static string NormalizePath(string path)
    {
        return path.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
