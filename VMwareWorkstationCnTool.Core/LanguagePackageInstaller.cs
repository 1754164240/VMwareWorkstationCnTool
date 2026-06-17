using System.IO.Compression;

namespace VMwareWorkstationCnTool.Core;

public static class LanguagePackageInstaller
{
    public static InstallResult Install(Stream languagePackageZip, string installPath)
    {
        if (!Directory.Exists(installPath))
        {
            return InstallResult.Fail("VMware Workstation 安装目录不存在。");
        }

        var messagesPath = Path.Combine(installPath, "messages");
        if (!Directory.Exists(messagesPath))
        {
            return InstallResult.Fail("安装目录中没有 messages 文件夹。");
        }

        var zhCnPath = Path.Combine(messagesPath, "zh_CN");
        string? backupPath = null;
        if (Directory.Exists(zhCnPath))
        {
            backupPath = CreateBackupPath(messagesPath);
            Directory.Move(zhCnPath, backupPath);
        }

        try
        {
            ExtractZhCnDirectory(languagePackageZip, messagesPath);
            return InstallResult.Ok("语言文件已复制到 VMware Workstation。", backupPath);
        }
        catch
        {
            if (Directory.Exists(zhCnPath))
            {
                Directory.Delete(zhCnPath, recursive: true);
            }

            if (backupPath is not null && Directory.Exists(backupPath))
            {
                Directory.Move(backupPath, zhCnPath);
            }

            throw;
        }
    }

    private static void ExtractZhCnDirectory(Stream languagePackageZip, string messagesPath)
    {
        languagePackageZip.Position = 0;
        using var archive = new ZipArchive(languagePackageZip, ZipArchiveMode.Read, leaveOpen: true);

        foreach (var entry in archive.Entries)
        {
            var normalizedEntryName = entry.FullName.Replace('\\', '/');
            if (!normalizedEntryName.StartsWith("zh_CN/", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            var relativePath = normalizedEntryName.Replace('/', Path.DirectorySeparatorChar);
            var destinationPath = Path.GetFullPath(Path.Combine(messagesPath, relativePath));
            var messagesFullPath = Path.GetFullPath(messagesPath) + Path.DirectorySeparatorChar;
            if (!destinationPath.StartsWith(messagesFullPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("语言包中包含非法路径。");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            entry.ExtractToFile(destinationPath, overwrite: true);
        }
    }

    private static string CreateBackupPath(string messagesPath)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupPath = Path.Combine(messagesPath, $"zh_CN.backup.{timestamp}");
        var index = 1;

        while (Directory.Exists(backupPath))
        {
            backupPath = Path.Combine(messagesPath, $"zh_CN.backup.{timestamp}.{index++}");
        }

        return backupPath;
    }
}
