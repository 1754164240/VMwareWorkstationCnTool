using System.Text;
using VMwareWorkstationCnTool.Core;

namespace VMwareWorkstationCnTool.Tests;

public sealed class LocalizationServiceTests
{
    [Fact]
    public void 一键汉化会安装语言包并写入偏好设置()
    {
        using var directory = TestDirectory.Create();
        var installPath = Path.Combine(directory.Path, "VMware Workstation");
        var preferencesPath = Path.Combine(directory.Path, "VMware", "preferences.ini");
        Directory.CreateDirectory(Path.Combine(installPath, "messages"));

        var service = new LocalizationService(EmbeddedLanguagePackage.Open, preferencesPath);

        var result = service.Localize(installPath);

        Assert.True(result.Success);
        Assert.True(File.Exists(Path.Combine(installPath, "messages", "zh_CN", "vmware.vmsg")));
        Assert.Contains("pref.locale = \"zh_CN\"", File.ReadAllText(preferencesPath, Encoding.UTF8));
    }
}
