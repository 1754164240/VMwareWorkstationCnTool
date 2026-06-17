using Microsoft.Win32;

namespace VMwareWorkstationCnTool.Core;

public sealed class VMwareInstallationLocator
{
    private static readonly string[] RegistryKeyPaths =
    {
        @"SOFTWARE\VMware, Inc.\VMware Workstation",
        @"SOFTWARE\WOW6432Node\VMware, Inc.\VMware Workstation"
    };

    private static readonly string[] DefaultInstallPaths =
    {
        @"C:\Program Files (x86)\VMware\VMware Workstation",
        @"C:\Program Files\VMware\VMware Workstation",
        @"D:\Program Files (x86)\VMware\VMware Workstation",
        @"D:\Program Files\VMware\VMware Workstation"
    };

    private readonly IReadOnlyCollection<VMwareRegistryEntry> registryEntries;
    private readonly IReadOnlyCollection<string> fallbackPaths;

    public VMwareInstallationLocator()
        : this(ReadRegistryEntries(), DefaultInstallPaths)
    {
    }

    public VMwareInstallationLocator(IReadOnlyCollection<VMwareRegistryEntry> registryEntries, IReadOnlyCollection<string> fallbackPaths)
    {
        this.registryEntries = registryEntries;
        this.fallbackPaths = fallbackPaths;
    }

    public VMwareInstallationInfo Find()
    {
        foreach (var entry in registryEntries)
        {
            if (!string.IsNullOrWhiteSpace(entry.InstallPath))
            {
                var result = FromCustomPath(entry.InstallPath, entry.ProductVersion);
                if (result.IsFound)
                {
                    return result;
                }
            }
        }

        foreach (var fallbackPath in fallbackPaths)
        {
            var result = FromCustomPath(fallbackPath);
            if (result.IsFound)
            {
                return result;
            }
        }

        return VMwareInstallationInfo.NotFound("未检测到 VMware Workstation 安装目录，请手动选择安装目录。");
    }

    public static VMwareInstallationInfo FromCustomPath(string? path, string? version = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return VMwareInstallationInfo.NotFound("请选择 VMware Workstation 安装目录。");
        }

        var normalizedPath = path.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!Directory.Exists(normalizedPath))
        {
            return VMwareInstallationInfo.NotFound("选择的安装目录不存在。");
        }

        var messagesPath = Path.Combine(normalizedPath, "messages");
        if (!Directory.Exists(messagesPath))
        {
            return VMwareInstallationInfo.NotFound("选择的目录中没有 messages 文件夹，请选择 VMware Workstation 的安装目录。");
        }

        var detectedVersion = string.IsNullOrWhiteSpace(version) ? ReadVersionFromExecutable(normalizedPath) : version;
        return VMwareInstallationInfo.Found(normalizedPath, detectedVersion ?? "未知版本");
    }

    private static IReadOnlyCollection<VMwareRegistryEntry> ReadRegistryEntries()
    {
        var entries = new List<VMwareRegistryEntry>();

        if (!OperatingSystem.IsWindows())
        {
            return entries;
        }

        foreach (var keyPath in RegistryKeyPaths)
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key is null)
            {
                continue;
            }

            entries.Add(new VMwareRegistryEntry(
                key.GetValue("InstallPath") as string,
                key.GetValue("ProductVersion") as string));
        }

        return entries;
    }

    private static string? ReadVersionFromExecutable(string installPath)
    {
        var vmwareExe = Path.Combine(installPath, "vmware.exe");
        if (!File.Exists(vmwareExe))
        {
            return null;
        }

        try
        {
            var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(vmwareExe);
            return string.IsNullOrWhiteSpace(info.ProductVersion) ? info.FileVersion : info.ProductVersion;
        }
        catch
        {
            return null;
        }
    }
}
