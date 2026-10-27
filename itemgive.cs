using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class itemgive
{
    private static List<ItemData> all;
    private static List<ItemData> shown = new List<ItemData>();
    private static string lastquery;
    private static Dictionary<int, string> names = new Dictionary<int, string>();
    private static string search = "";
    private static Vector2 scroll;
    private static float nextspam;
    private static float lastsend;
    private static int pending;
    private static ItemData target;
    private const BindingFlags anyfield = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const int maxpending = 2;

    public static List<ItemData> items()
    {
        if (all != null)
        {
            return all;
        }

        var im = ItemManager.GetInstance();

        if (im == null || LevelScript.GetInstance() == null)
        {
            return new List<ItemData>();
        }

        all = new List<ItemData>();
        var seen = new HashSet<int>();

        foreach (var field in new[] { "m_AllowedItems", "m_KeyItems" })
        {
            var f = typeof(ItemManager).GetField(field, anyfield);
            addall(f != null ? f.GetValue(im) as IEnumerable : null, seen);
        }

        all.Sort((a, b) => string.Compare(title(a), title(b), StringComparison.CurrentCulture));
        lastquery = null;
        return all;
    }

    private static bool giveable(ItemData d)
    {
        var key = d.HasFunctionality(BaseItemFunctionality.Functionality.Key) as KeyFunctionality;

        if (key != null && key.IsHidden)
        {
            return false;
        }

        return ItemManager.GetInstance().IsItemInAllowedList(d);
    }

    private static void addall(IEnumerable source, HashSet<int> seen)
    {
        if (source == null)
        {
            return;
        }

        foreach (var o in source)
        {
            var d = o as ItemData;

            if (d != null && d.m_ItemDataID > 0 && giveable(d) && seen.Add(d.m_ItemDataID))
            {
                all.Add(d);
            }
        }
    }

    public static string title(ItemData d)
    {
        string name;

        if (names.TryGetValue(d.m_ItemDataID, out name))
        {
            return name;
        }

        if (!Localization.Get(d.m_ItemLocalizationTag, out name) || !readable(name))
        {
            name = d.name;
        }

        names[d.m_ItemDataID] = name;
        return name;
    }

    private static bool readable(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Trim().Length == 0)
        {
            return false;
        }

        return !(name.StartsWith("M:") || name.StartsWith("M.") || name.StartsWith("Text.") || name.Contains("Text.Item."));
    }

    public static void give(ItemData data, Action<Item> then, int container)
    {
        var p = local.player();
        var im = ItemManager.GetInstance();

        if (p == null || im == null || data == null)
        {
            return;
        }

        int req = 0;
        pending++;
        lastsend = Time.time;

        im.AssignItemRPC(p.m_NetView.ownerId, data.m_ItemDataID, (item, id) =>
        {
            pending = Mathf.Max(0, pending - 1);

            if (item != null && then != null)
            {
                then(item);
            }
        }, ref req, container);
    }

    private static void toinventory(Item item)
    {
        var p = local.player();

        if (p == null)
        {
            return;
        }

        if (!p.m_ItemContainer.AddItemRPC(item))
        {
            dropnear(item);
        }
    }

    public static void giveinventory(ItemData data)
    {
        var p = local.player();

        if (p == null || p.m_ItemContainer == null)
        {
            return;
        }

        give(data, toinventory, p.m_ItemContainer.NetView.viewID);
    }

    private static void dropnear(Item item)
    {
        var p = local.player();

        if (p == null)
        {
            return;
        }

        var pos = p.m_CachedCurrentPosition + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
        item.DropItemInLevel(p, pos);
    }

    public static ItemContainer nearbox()
    {
        var p = local.player();

        if (p == null)
        {
            return null;
        }

        ItemContainer best = null;
        float bestdist = 4f;

        foreach (var c in UnityEngine.Object.FindObjectsOfType<ItemContainer>())
        {
            if (c == null || c == p.m_ItemContainer)
            {
                continue;
            }

            float d = Vector3.Distance(c.transform.position, p.m_Transform.position);

            if (d < bestdist)
            {
                bestdist = d;
                best = c;
            }
        }

        return best;
    }

    public static void intobox(ItemData d, ItemContainer box)
    {
        if (box == null)
        {
            return;
        }

        give(d, item =>
        {
            if (!box.AddItemRPC(item, true))
            {
                dropnear(item);
            }
        }, box.NetView.viewID);
    }

    private static void boxgive(ItemData d)
    {
        intobox(d, nearbox());
    }

    private static feature spamfeature()
    {
        return features.all.Find(x => x.name == "spam items on floor");
    }

    private static bool spamming(ItemData d)
    {
        var f = spamfeature();
        return f != null && f.on && target == d;
    }

    private static void togglespam(ItemData d)
    {
        var f = spamfeature();

        if (f == null)
        {
            return;
        }

        if (spamming(d))
        {
            target = null;
            f.on = false;
        }
        else
        {
            target = d;
            f.on = true;
        }

        config.save();
    }

    public static void spamfloor()
    {
        if (Time.time < nextspam)
        {
            return;
        }

        if (pending > 0 && Time.time - lastsend > 2f)
        {
            pending = 0;
        }

        if (pending >= maxpending)
        {
            return;
        }

        var list = items();

        if (list.Count == 0)
        {
            return;
        }

        nextspam = Time.time + 0.15f;
        var pick = target != null ? target : list[UnityEngine.Random.Range(0, list.Count)];
        give(pick, dropnear, -1);
    }

    private static void applyfilter()
    {
        string q = (search ?? "").Trim().ToLower();

        if (q == lastquery)
        {
            return;
        }

        lastquery = q;
        shown.Clear();

        foreach (var d in items())
        {
            if (q.Length == 0 || title(d).ToLower().Contains(q) || d.name.ToLower().Contains(q))
            {
                shown.Add(d);
            }
        }
    }

    private const float rowh = 36f;
    private const float viewh = 300f;
    private static int firstrow;
    private static int visiblerows;
    private static int layouttotal;

    public static void draw()
    {
        GUILayout.Label("items: " + items().Count + ", found: " + shown.Count);
        GUILayout.BeginHorizontal();
        search = GUILayout.TextField(search ?? "", 40, GUILayout.ExpandWidth(true));

        if (controls.button("refresh", GUILayout.Width(90f)))
        {
            all = null;
        }

        GUILayout.EndHorizontal();
        applyfilter();

        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(viewh));

        if (Event.current.type == EventType.Layout)
        {
            layouttotal = shown.Count;
            firstrow = Mathf.Clamp((int)(scroll.y / rowh), 0, Mathf.Max(0, layouttotal - 1));
            visiblerows = Mathf.Min(layouttotal - firstrow, (int)(viewh / rowh) + 3);
        }

        GUILayout.Space(firstrow * rowh);

        for (int i = 0; i < visiblerows; i++)
        {
            int index = firstrow + i;

            if (index >= shown.Count)
            {
                GUILayout.Space(rowh);
                continue;
            }

            var d = shown[index];
            GUILayout.BeginHorizontal(theme.clean, GUILayout.Height(rowh));
            GUILayout.Label(title(d), GUILayout.ExpandWidth(true));

            if (controls.button(spamming(d) ? "stop" : "spam", GUILayout.Width(58f)))
            {
                togglespam(d);
            }

            if (controls.button("+inv", GUILayout.Width(58f)))
            {
                giveinventory(d);
            }

            if (controls.button("+floor", GUILayout.Width(66f)))
            {
                give(d, dropnear, -1);
            }

            if (controls.button("+box", GUILayout.Width(56f)))
            {
                boxgive(d);
            }

            if (controls.button(itemedit.picked == d ? "picked" : "pick", GUILayout.Width(66f)))
            {
                itemedit.pick(d);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(Mathf.Max(0, layouttotal - firstrow - visiblerows) * rowh);

        if (layouttotal == 0)
        {
            GUILayout.Label("nothing found");
        }

        GUILayout.EndScrollView();
    }
}
