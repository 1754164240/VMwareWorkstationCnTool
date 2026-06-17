using System.IO.Compression;
using System.Text;
using VMwareWorkstationCnTool.Core;

namespace VMwareWorkstationCnTool.Tests;

public sealed class LanguagePackageInstallerTests
{
    [Fact]
    public void 会把内置语言包释放到新版Messages目录()
    {
        using var directory = TestDirectory.Create();
        using var package = CreateLanguagePackage();
        var installPath = Path.Combine(directory.Path, "VMware Workstation");
        Directory.CreateDirectory(Path.Combine(installPath, "messages"));

        var result = LanguagePackageInstaller.Install(package, installPath);

        Assert.True(result.Success);
        Assert.True(File.Exists(Path.Combine(installPath, "messages", "zh_CN", "vmware.vmsg")));
        Assert.Equal("示例内容", File.ReadAllText(Path.Combine(installPath, "messages", "zh_CN", "vmware.vmsg"), Encoding.UTF8));
    }

    [Fact]
    public void 会安装程序集中内置的真实语言包()
    {
        using var directory = TestDirectory.Create();
        using var package = EmbeddedLanguagePackage.Open();
        var installPath = Path.Combine(directory.Path, "VMware Workstation");
        Directory.CreateDirectory(Path.Combine(installPath, "messages"));

        var result = LanguagePackageInstaller.Install(package, installPath);

        Assert.True(result.Success);
        Assert.True(File.Exists(Path.Combine(installPath, "messages", "zh_CN", "vmware.vmsg")));
        Assert.True(File.Exists(Path.Combine(installPath, "messages", "zh_CN", "vmui-zh_CN.dll")));
        Assert.True(File.Exists(Path.Combine(installPath, "messages", "zh_CN", "vmappsdk-zh_CN.dll")));
    }

    [Fact]
    public void 覆盖前会备份已有语言目录()
    {
        using var directory = TestDirectory.Create();
        using var package = CreateLanguagePackage();
        var installPath = Path.Combine(directory.Path, "VMware Workstation");
        var oldZhCn = Path.Combine(installPath, "messages", "zh_CN");
        Directory.CreateDirectory(oldZhCn);
        File.WriteAllText(Path.Combine(oldZhCn, "old.txt"), "旧内容", new UTF8Encoding(false));

        var result = LanguagePackageInstaller.Install(package, installPath);

        Assert.True(result.Success);
        Assert.True(Directory.Exists(result.BackupPath));
        Assert.True(File.Exists(Path.Combine(result.BackupPath!, "old.txt")));
        Assert.Equal("示例内容", File.ReadAllText(Path.Combine(installPath, "messages", "zh_CN", "vmware.vmsg"), Encoding.UTF8));
    }

    private static MemoryStream CreateLanguagePackage()
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("zh_CN/vmware.vmsg");
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write("示例内容");
        }

        stream.Position = 0;
        return stream;
    }
}
