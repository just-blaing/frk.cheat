using System;
using System.Collections.Generic;
using UnityEngine;

public static class zonelist
{
    private static Vector2 scroll;
    private static float nextscan;
    private static Dictionary<RoomBlob.eLocation, List<RoomBlob>> groups = new Dictionary<RoomBlob.eLocation, List<RoomBlob>>();

    private static bool skip(RoomBlob.eLocation loc)
    {
        return loc == RoomBlob.eLocation.NowhereSpecial || loc == RoomBlob.eLocation.Corridor || loc == RoomBlob.eLocation.InmateCell;
    }

    private static void scan()
    {
        if (Time.time < nextscan)
        {
            return;
        }

        nextscan = Time.time + 2f;
        groups.Clear();
        var rm = RoomManager.GetInstance();

        if (rm == null)
        {
            return;
        }

        foreach (RoomBlob.eLocation loc in Enum.GetValues(typeof(RoomBlob.eLocation)))
        {
            if (skip(loc))
            {
                continue;
            }

            var list = rm.GetAllRoomsByLocation(loc);

            if (list != null && list.Count > 0)
            {
                groups[loc] = list;
            }
        }
    }

    public static void draw()
    {
        scan();
        GUILayout.Label("zones: " + groups.Count);
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(300));

        foreach (var pair in groups)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(pair.Key.ToString() + " (" + pair.Value.Count + ")");

            if (controls.button("tp", GUILayout.Width(60)))
            {
                playeractions.teleport(pair.Value[0].transform.position);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}
