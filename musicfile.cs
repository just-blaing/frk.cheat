using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public static class musicfile
{
    private static Vector2 gamescroll;
    private static List<Events> tracks;

    private static List<Events> builtin()
    {
        if (tracks != null)
        {
            return tracks;
        }

        tracks = new List<Events>();

        foreach (Events e in System.Enum.GetValues(typeof(Events)))
        {
            var name = e.ToString();

            if (name.StartsWith("Play_Music_Prison") && name.Contains("Routine"))
            {
                tracks.Add(e);
            }
        }

        return tracks;
    }

    private static void pausebuiltin()
    {
        foreach (var e in builtin())
        {
            AudioController.PauseRoutineMusic(e);
        }
    }

    public static void draw()
    {
        GUILayout.Space(8f);
        GUILayout.Label("game music");

        if (controls.button("stop music"))
        {
            pausebuiltin();
        }

        gamescroll = GUILayout.BeginScrollView(gamescroll, GUILayout.Height(180f));

        foreach (var e in builtin())
        {
            if (controls.button(e.ToString()))
            {
                pausebuiltin();
                AudioController.PlayRoutineMusic(e);
            }
        }

        GUILayout.EndScrollView();
    }
}
