using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class menuvideos
{
    private const string videoname = "Animation";
    private const string buttonsuffix = "Button";
    private static float nextscan;
    private static readonly List<Behaviour> hidden = new List<Behaviour>();

    private static IEnumerable<MonoBehaviour> comps(string type)
    {
        return UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().Where(c => c != null && c.GetType().Name == type);
    }

    private static bool scandue()
    {
        if (Time.unscaledTime < nextscan)
        {
            return false;
        }

        nextscan = Time.unscaledTime + 1f;
        return true;
    }

    private static bool isbuttonvideo(MonoBehaviour video)
    {
        var parent = video.transform.parent;
        return video.name == videoname && parent != null && parent.name.EndsWith(buttonsuffix);
    }

    private static void hidevideos()
    {
        if (!scandue())
        {
            return;
        }

        foreach (var video in comps("T17RawImage").Where(isbuttonvideo))
        {
            if (video.enabled)
            {
                video.enabled = false;
                hidden.Add(video);
            }
        }
    }

    private static void showvideos()
    {
        foreach (var video in hidden)
        {
            if (video != null)
            {
                video.enabled = true;
            }
        }

        hidden.Clear();
    }

    private static void dumpnode(Transform node, int depth, StringBuilder sb)
    {
        sb.Append(new string(' ', depth * 2)).Append(node.name).Append(" active=").Append(node.gameObject.activeSelf);

        foreach (var c in node.GetComponents<Component>())
        {
            sb.Append(" [").Append(c.GetType().Name).Append(']');
        }

        sb.AppendLine();

        for (int i = 0; i < node.childCount; i++)
        {
            dumpnode(node.GetChild(i), depth + 1, sb);
        }
    }

    private static void dumpbuttons()
    {
        var sb = new StringBuilder();

        foreach (var button in comps("T17Button").Where(b => b.gameObject.activeInHierarchy))
        {
            dumpnode(button.transform, 0, sb);
            sb.AppendLine();
        }

        GUIUtility.systemCopyBuffer = sb.ToString();
        config.status = "menu hierarchy copied";
    }

    public static void init()
    {
        features.all.Add(new feature
        {
            name = "hide videos on menu buttons",
            tab = "Visual",
            toggle = true,
            tick = hidevideos,
            off = showvideos
        });

        features.all.Add(new feature
        {
            name = "copy menu hierarchy",
            tab = "Visual",
            run = dumpbuttons
        });
    }
}
