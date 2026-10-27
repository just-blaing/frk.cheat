using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

public static class finder
{
    public const string exename = "TheEscapists2.exe";
    public const string defaultdir = @"C:\Program Files\Steam\steamapps\The Escapists 2";
    private const string foldername = "The Escapists 2";

    public static string find()
    {
        foreach (var lib in libraries())
        {
            string dir = Path.Combine(lib, "steamapps", "common", foldername);

            if (File.Exists(Path.Combine(dir, exename)))
            {
                return dir;
            }
        }

        return defaultdir;
    }

    private static List<string> libraries()
    {
        var found = new List<string>();
        string steam = steampath();

        if (steam == "")
        {
            return found;
        }

        found.Add(steam);
        string vdf = Path.Combine(steam, "steamapps", "libraryfolders.vdf");

        if (!File.Exists(vdf))
        {
            return found;
        }

        foreach (Match m in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s+\"([^\"]+)\""))
        {
            found.Add(m.Groups[1].Value.Replace("\\\\", "\\"));
        }

        return found;
    }

    private static string steampath()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
        {
            var value = key == null ? null : key.GetValue("SteamPath") as string;
            return value == null ? "" : value.Replace('/', '\\');
        }
    }
}