using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class prisonlist
{
    private static Vector2 scroll;
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

    private static void load(PrisonData prison)
    {
        var gs = GlobalStart.GetInstance();

        if (gs == null || prison.m_LevelInfo == null)
        {
            return;
        }

        gs.StartLevel(GlobalStart.GLOBALSTART_GAME_MODES.SINGLE, prison.m_LevelInfo.m_AssociatedFile, 0);
    }

    public static void draw()
    {
        GUILayout.Label("prison select");
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(300));

        foreach (var prison in prisons())
        {
            if (prison == null || prison.m_LevelInfo == null)
            {
                continue;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(prison.m_LevelInfo.m_PrisonEnum.ToString());

            if (controls.button("load", GUILayout.Width(110)))
            {
                load(prison);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}
