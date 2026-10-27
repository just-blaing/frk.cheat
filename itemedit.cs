using System.Collections.Generic;
using UnityEngine;

public static class itemedit
{
    public static ItemData picked;

    private static Item selected;
    private static ItemContainer selectedin;
    private static Vector2 invscroll;
    private static Vector2 boxscroll;
    private static List<Item> buffer = new List<Item>();
    private static ItemContainer box;
    private static float nextboxscan;

    public static void pick(ItemData d)
    {
        picked = picked == d ? null : d;
    }

    public static void draw()
    {
        var p = local.player();

        GUILayout.Space(8);
        GUILayout.Label("player inventory");

        if (p == null || p.m_ItemContainer == null)
        {
            GUILayout.Label("player not found");
        }
        else
        {
            drawlist(p.m_ItemContainer, ref invscroll, "inventory empty");
        }

        GUILayout.Space(8);

        if (Time.time >= nextboxscan)
        {
            nextboxscan = Time.time + 1f;
            box = itemgive.nearbox();
        }

        GUILayout.Label("nearby box");

        if (box == null)
        {
            GUILayout.Label("walk up to a box (radius 4)");
            return;
        }

        drawlist(box, ref boxscroll, "box empty");
    }

    private static void drawlist(ItemContainer container, ref Vector2 scroll, string empty)
    {
        container.GetItems(ref buffer);

        if (buffer.Count == 0)
        {
            GUILayout.Label(empty);
            return;
        }

        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(130));

        foreach (var it in buffer)
        {
            if (it == null || it.m_ItemData == null)
            {
                continue;
            }

            bool isselected = it == selected;
            string mark = isselected ? "> " : "  ";

            if (GUILayout.Button(mark + itemgive.title(it.m_ItemData) + "  [" + it.Health + "]", GUI.skin.label))
            {
                selected = isselected ? null : it;
                selectedin = isselected ? null : container;
            }
        }

        GUILayout.EndScrollView();

        if (selected != null && selectedin == container && selected.m_ItemData != null)
        {
            drawselected(container);
        }
    }

    private static void drawselected(ItemContainer container)
    {
        var d = selected.m_ItemData;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("selected: " + itemgive.title(d));

        GUILayout.BeginHorizontal();
        GUILayout.Label("durability", GUILayout.Width(70));
        int value = Mathf.RoundToInt(controls.slider(d.m_ItemHealth, 0f, 100f));

        if (value != d.m_ItemHealth)
        {
            d.m_ItemHealth = value;
        }

        GUILayout.Label(d.m_ItemHealth.ToString(), GUILayout.Width(36));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (controls.button("repair"))
        {
            d.ResetHealth();
        }

        if (picked != null && controls.button("replace with " + itemgive.title(picked)))
        {
            replace(container, selected);
            selected = null;
            selectedin = null;
        }
        else if (controls.button("delete"))
        {
            container.RemoveItemRPC(selected, true);
            selected = null;
            selectedin = null;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private static void replace(ItemContainer container, Item old)
    {
        var data = picked;
        container.RemoveItemRPC(old, true);

        if (container == local.player().m_ItemContainer)
        {
            itemgive.giveinventory(data);
        }
        else
        {
            itemgive.intobox(data, container);
        }
    }
}
