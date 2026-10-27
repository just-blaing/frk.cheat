using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BepInEx;
using UnityEngine;

public static class config
{
    public static string dir = Path.Combine(Paths.ConfigPath, "frkcheat");
    public static string current = "default";
    public static string status = "";
    private const string prefix = "frkcfg:";

    public static void start()
    {
        Directory.CreateDirectory(dir);
        string marker = Path.Combine(dir, "active.txt");

        if (File.Exists(marker))
        {
            current = File.ReadAllText(marker).Trim();
        }

        if (!File.Exists(file(current)))
        {
            current = "default";
        }

        if (File.Exists(file(current)))
        {
            load();
        }
        else
        {
            save();
        }
    }

    public static string file(string name)
    {
        return Path.Combine(dir, name + ".cfg");
    }

    public static string[] names()
    {
        var list = new List<string>();

        foreach (var p in Directory.GetFiles(dir, "*.cfg"))
        {
            list.Add(Path.GetFileNameWithoutExtension(p));
        }

        list.Sort((a, b) => string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase));
        return list.ToArray();
    }

    public static string dump()
    {
        var lines = new List<string>();

        foreach (var f in features.all)
        {
            lines.Add(f.name + "|" + (f.on ? 1 : 0) + "|" + f.val.ToString(CultureInfo.InvariantCulture));
        }

        return string.Join("\n", lines.ToArray());
    }

    public static void apply(string text)
    {
        var named = new HashSet<string>();

        foreach (var line in text.Split('\n'))
        {
            var parts = line.Trim().Split('|');

            if (parts.Length < 3)
            {
                continue;
            }

            var f = features.all.Find(x => x.name == parts[0]);

            if (f == null)
            {
                continue;
            }

            named.Add(f.name);

            bool was = f.on;
            f.on = f.toggle && parts[1] == "1";

            if (was && !f.on && f.off != null)
            {
                f.off();
            }

            float v;

            if (f.slider && float.TryParse(parts[parts.Length - 1], NumberStyles.Float, CultureInfo.InvariantCulture, out v))
            {
                f.val = Mathf.Clamp(v, f.min, f.max);
            }
        }

        foreach (var f in features.all)
        {
            if (f.toggle && f.on && !named.Contains(f.name))
            {
                f.on = false;

                if (f.off != null)
                {
                    f.off();
                }
            }
        }
    }

    private static bool looks(string text)
    {
        foreach (var line in text.Split('\n'))
        {
            var parts = line.Trim().Split('|');

            if (parts.Length >= 3 && features.all.Exists(x => x.name == parts[0]))
            {
                return true;
            }
        }

        return false;
    }

    private static string unique(string name)
    {
        string result = name;
        int i = 2;

        while (File.Exists(file(result)))
        {
            result = name + " " + i;
            i++;
        }

        return result;
    }

    public static void load()
    {
        try
        {
            apply(File.ReadAllText(file(current)));
        }
        catch (Exception e)
        {
            status = "failed to load config " + current + ": " + e.Message;
        }
    }

    public static void save()
    {
        File.WriteAllText(file(current), dump());
        File.WriteAllText(Path.Combine(dir, "active.txt"), current);
    }

    public static void switchto(string name)
    {
        if (!File.Exists(file(name)))
        {
            status = "config '" + name + "' not found";
            return;
        }

        save();
        current = name;
        load();
        save();
        status = "loaded config '" + name + "'";
    }

    public static void create(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c.ToString(), "");
        }

        name = name.Trim();

        if (name == "")
        {
            status = "enter a config name";
            return;
        }

        current = unique(name);
        save();
        status = "created config '" + current + "' (with current settings)";
    }

    public static void remove(string name)
    {
        if (name == current)
        {
            status = "the active config cannot be deleted";
            return;
        }

        if (File.Exists(file(name)))
        {
            File.Delete(file(name));
            status = "deleted config '" + name + "'";
        }
    }

    public static void export()
    {
        GUIUtility.systemCopyBuffer = prefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(dump()));
        status = "settings '" + current + "' copied to clipboard";
    }

    private static string decode(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        string t = text.Trim();

        if (!t.StartsWith(prefix, StringComparison.Ordinal))
        {
            return t;
        }

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(t.Substring(prefix.Length)));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public static void import(string text)
    {
        string body = decode(text);

        if (string.IsNullOrEmpty(body) || !looks(body))
        {
            status = "no frk.cheat config in the clipboard";
            return;
        }

        current = unique("imported");
        apply(body);
        save();
        status = "imported config '" + current + "'";
    }
}
