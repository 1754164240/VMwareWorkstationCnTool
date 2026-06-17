namespace VMwareWorkstationCnTool.Core;

public sealed record InstallResult(bool Success, string Message, string? BackupPath)
{
    public static InstallResult Ok(string message, string? backupPath) => new(true, message, backupPath);

    public static InstallResult Fail(string message) => new(false, message, null);
}
