using System.Reflection;

namespace VMwareWorkstationCnTool.Core;

public static class EmbeddedLanguagePackage
{
    private const string ResourceSuffix = ".Resources.zh_CN.zip";

    public static Stream Open()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(ResourceSuffix, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new FileNotFoundException("程序中没有找到内置的 zh_CN 语言包。");
        }

        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException("无法读取内置的 zh_CN 语言包。");
    }
}
