namespace VMwareWorkstationCnTool.Core;

public sealed class LocalizationService
{
    private readonly Func<Stream> openLanguagePackage;
    private readonly string preferencesPath;

    public LocalizationService(Func<Stream> openLanguagePackage, string preferencesPath)
    {
        this.openLanguagePackage = openLanguagePackage;
        this.preferencesPath = preferencesPath;
    }

    public static LocalizationService CreateDefault()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return new LocalizationService(
            EmbeddedLanguagePackage.Open,
            Path.Combine(appData, "VMware", "preferences.ini"));
    }

    public OperationResult Localize(string installPath)
    {
        using var package = openLanguagePackage();
        var installResult = LanguagePackageInstaller.Install(package, installPath);
        if (!installResult.Success)
        {
            return OperationResult.Fail(installResult.Message);
        }

        PreferencesEditor.EnsureChineseLocale(preferencesPath);
        var backupMessage = installResult.BackupPath is null ? string.Empty : $"{Environment.NewLine}已备份原 zh_CN 到：{installResult.BackupPath}";
        return OperationResult.Ok($"汉化完成，请重启 VMware Workstation。{backupMessage}");
    }
}
