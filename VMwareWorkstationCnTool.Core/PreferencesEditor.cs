using System.Text;

namespace VMwareWorkstationCnTool.Core;

public static class PreferencesEditor
{
    private const string LocaleLine = "pref.locale = \"zh_CN\"";

    public static void EnsureChineseLocale(string preferencesPath)
    {
        var directory = Path.GetDirectoryName(preferencesPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var lines = File.Exists(preferencesPath)
            ? File.ReadAllLines(preferencesPath, Encoding.UTF8).ToList()
            : new List<string>();

        var localeIndex = lines.FindIndex(line => line.TrimStart().StartsWith("pref.locale", StringComparison.OrdinalIgnoreCase));
        if (localeIndex >= 0)
        {
            lines[localeIndex] = LocaleLine;
        }
        else
        {
            lines.Add(LocaleLine);
        }

        File.WriteAllText(preferencesPath, string.Join(Environment.NewLine, lines) + Environment.NewLine, new UTF8Encoding(false));
    }
}
