using System.IO.Compression;
using VMwareWorkstationCnTool.Core;

namespace VMwareWorkstationCnTool.Tests;

public sealed class EmbeddedLanguagePackageTests
{
    [Fact]
    public void 程序集中内置了旧版ZhCn语言包()
    {
        using var stream = EmbeddedLanguagePackage.Open();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var entryNames = archive.Entries
            .Select(entry => entry.FullName.Replace('\\', '/'))
            .ToArray();

        Assert.Contains("zh_CN/vmware.vmsg", entryNames);
        Assert.Contains("zh_CN/vmui-zh_CN.dll", entryNames);
        Assert.Contains("zh_CN/vmappsdk-zh_CN.dll", entryNames);
    }
}
