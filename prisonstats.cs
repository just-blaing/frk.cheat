using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class prisonstats
{
    private static Vector2 scroll;
    private static EscapeMethod[] cachedmethods;
    private static string cachedcount = "";
    private static float nextcount;
    private static readonly Dictionary<string, int> picks = new Dictionary<string, int>();
    private static readonly FieldInfo levelsfield = typeof(LevelDataManager).GetField("m_T17Levels", BindingFlags.Instance | BindingFlags.NonPublic);

    private static PrisonData[] prisons()
    {
        var ldm = LevelDataManager.GetInstance();

        if (ldm == null || levelsfield == null)
        {
            return new PrisonData[0];
        }

        return levelsfield.GetValue(ldm) as PrisonData[] ?? new PrisonData[0];
    }

    private static EscapeMethod[] methods()
    {
        var all = (EscapeMethod[])Enum.GetValues(typeof(EscapeMethod));
        var list = new List<EscapeMethod>();

        foreach (var m in all)
        {
            if (m != EscapeMethod.Unknown)
            {
                list.Add(m);
            }
        }

        return list.ToArray();
    }

    private static string escapedcount()
    {
        try
        {
            var ss = StatSystem.GetInstance();

            if (ss == null)
            {
                return "stats not ready";
            }

            return ss.GetStatValue((int)STAT_IDS.EscapedPrisonIDs).ToString();
        }
        catch (Exception)
        {
            return "stats not ready";
        }
    }

    private static void mark(PrisonData prison, EscapeMethod method)
    {
        var gamer = Gamer.GetPrimaryGamer();

        if (gamer == null)
        {
            config.status = "player not found";
            return;
        }

        if (KeyAwardManager.GetInstance() == null)
        {
            new GameObject("keyawardmanager").AddComponent<KeyAwardManager>();
        }

        KeyAwardManager.OnPrisonEscaped(gamer, prison.m_LevelInfo, method);
        config.status = "escape marked: " + prison.m_LevelInfo.m_PrisonEnum + ", " + method;
    }

    public static void draw()
    {
        if (Time.unscaledTime >= nextcount)
        {
            nextcount = Time.unscaledTime + 1f;
            cachedcount = escapedcount();
        }

        GUILayout.Label("escapes by prison (stats): " + cachedcount);

        if (cachedmethods == null)
        {
            cachedmethods = methods();
        }

        var list = cachedmethods;
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(200));

        foreach (var prison in prisons())
        {
            if (prison == null || prison.m_LevelInfo == null || list.Length == 0)
            {
                continue;
            }

            string key = prison.m_LevelInfo.m_PrisonEnum.ToString();
            int index;

            if (!picks.TryGetValue(key, out index))
            {
                index = 0;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(key, GUILayout.Width(120));

            if (controls.button(list[index].ToString(), GUILayout.Width(150)))
            {
                index = (index + 1) % list.Length;
                picks[key] = index;
            }

            if (controls.button("mark", GUILayout.Width(90)))
            {
                try
                {
                    mark(prison, list[index]);
                }
                catch (Exception e)
                {
                    config.status = "mark failed: " + e.GetType().Name;
                }
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}
