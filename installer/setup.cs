using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

public static class setup
{
    private const string pluginname = "frkcheat.dll";
    private const string loadername = "bepinex.zip";

    private static string plugindir(string game)
    {
        return Path.Combine(game, "BepInEx", "plugins");
    }

    private static void check(string game)
    {
        if (!File.Exists(Path.Combine(game, finder.exename)))
        {
            throw new FileNotFoundException(finder.exename + " not found in this folder");
        }
    }

    private static bool hasloader(string game)
    {
        return File.Exists(Path.Combine(game, "BepInEx", "core", "BepInEx.dll")) && File.Exists(Path.Combine(game, "winhttp.dll"));
    }

    private static byte[] resource(string name)
    {
        using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
        using (var m = new MemoryStream())
        {
            s.CopyTo(m);
            return m.ToArray();
        }
    }

    private static void unpack(byte[] data, string game)
    {
        string root = Path.GetFullPath(game);

        using (var zip = new ZipArchive(new MemoryStream(data)))
        {
            foreach (var entry in zip.Entries)
            {
                if (entry.Name == "")
                {
                    continue;
                }

                string target = Path.GetFullPath(Path.Combine(root, entry.FullName.Replace('/', '\\')));

                if (!target.StartsWith(root))
                {
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(target));
                entry.ExtractToFile(target, true);
            }
        }
    }

    public static void install(string game, Action<string> say)
    {
        check(game);

        if (!hasloader(game))
        {
            say("installing bepinex");
            unpack(resource(loadername), game);
        }

        say("copying plugin");
        Directory.CreateDirectory(plugindir(game));
        File.WriteAllBytes(Path.Combine(plugindir(game), pluginname), resource(pluginname));
    }

    public static void uninstall(string game)
    {
        check(game);
        string file = Path.Combine(plugindir(game), pluginname);

        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }
}