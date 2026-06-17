using System.Text;
using VMwareWorkstationCnTool.Core;

namespace VMwareWorkstationCnTool.Tests;

public sealed class PreferencesEditorTests
{
    [Fact]
    public void 写入语言配置时会追加到文件末尾()
    {
        using var directory = TestDirectory.Create();
        var preferencesPath = Path.Combine(directory.Path, "preferences.ini");
        File.WriteAllText(preferencesPath, "pref.foo = \"bar\"\r\n", new UTF8Encoding(false));

        PreferencesEditor.EnsureChineseLocale(preferencesPath);

        var text = File.ReadAllText(preferencesPath, Encoding.UTF8);
        Assert.Equal("pref.foo = \"bar\"\r\npref.locale = \"zh_CN\"\r\n", text);
    }

    [Fact]
    public void 重复写入语言配置时不会产生多余行()
    {
        using var directory = TestDirectory.Create();
        var preferencesPath = Path.Combine(directory.Path, "preferences.ini");
        File.WriteAllText(preferencesPath, "pref.locale = \"en_US\"\r\npref.foo = \"bar\"\r\n", new UTF8Encoding(false));

        PreferencesEditor.EnsureChineseLocale(preferencesPath);

        var lines = File.ReadAllLines(preferencesPath, Encoding.UTF8);
        Assert.Contains("pref.locale = \"zh_CN\"", lines);
        Assert.DoesNotContain("pref.locale = \"en_US\"", lines);
        Assert.Single(lines, line => line.StartsWith("pref.locale", StringComparison.OrdinalIgnoreCase));
    }
}
