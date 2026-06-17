using VMwareWorkstationCnTool.Core;

namespace VMwareWorkstationCnTool.Tests;

public sealed class InstallationLocatorTests
{
    [Fact]
    public void 注册表包含安装信息时会返回安装目录和版本()
    {
        var path = @"D:\Program Files (x86)\VMware\VMware Workstation\";
        var locator = new VMwareInstallationLocator(new[]
        {
            new VMwareRegistryEntry(path, "25.0.1.25219725")
        }, Array.Empty<string>());

        var result = locator.Find();

        Assert.True(result.IsFound);
        Assert.Equal(path.TrimEnd('\\'), result.InstallPath);
        Assert.Equal("25.0.1.25219725", result.Version);
    }

    [Fact]
    public void 手动目录包含Messages文件夹时会被接受()
    {
        using var directory = TestDirectory.Create();
        Directory.CreateDirectory(Path.Combine(directory.Path, "messages"));

        var result = VMwareInstallationLocator.FromCustomPath(directory.Path);

        Assert.True(result.IsFound);
        Assert.Equal(directory.Path, result.InstallPath);
        Assert.Equal("未知版本", result.Version);
    }

    [Fact]
    public void 无效手动目录会给出失败原因()
    {
        using var directory = TestDirectory.Create();

        var result = VMwareInstallationLocator.FromCustomPath(directory.Path);

        Assert.False(result.IsFound);
        Assert.Contains("messages", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
