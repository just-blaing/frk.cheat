using System.Globalization;
using System.Text;
using UnityEngine;

public class menu
{
    public bool open;
    public bool typing;
    private bool dirty;
    private bool wasopen;
    private Rect window = new Rect(60, 60, 760, 580);
    private readonly string[] tabs = { "Player", "Items", "World", "Fun", "Visual", "Menu", "Zones", "Editor", "Configs" };
    private int tab;
    private int lasttab = -1;
    private Vector2 scroll;
    private string newname = "";

    private float openness;
    private float shown;
    private float contentfade;
    private float indicatorx;
    private int rowindex;
    private Vector2 pendingmove;

    private string laststatus = "";
    private float statusat;

    private const float headerheight = 44f;
    private const float tabheight = 32f;
    private const float footerheight = 30f;
    private const float rowheight = 34f;
    private const float pad = 22f;

    private float tabwidth
    {
        get { return (window.width - 32f) / tabs.Length; }
    }

    public void tick()
    {
        float dt = Time.unscaledDeltaTime;
        openness = Mathf.MoveTowards(openness, open ? 1f : 0f, dt * 4.5f);
        shown = controls.outcubic(openness);

        if (open && !wasopen)
        {
            contentfade = 0f;
        }

        wasopen = open;

        if (openness <= 0f)
        {
            return;
        }

        contentfade = Mathf.MoveTowards(contentfade, 1f, dt * 1.6f);
        indicatorx = Mathf.Lerp(indicatorx, tab * tabwidth, 1f - Mathf.Exp(-dt * 16f));

        foreach (var f in features.all)
        {
            if (f.toggle)
            {
                f.anim = Mathf.MoveTowards(f.anim, f.on ? 1f : 0f, dt * 8f);
            }
        }
    }

    public void draw()
    {
        if (openness <= 0f)
        {
            typing = false;
            return;
        }

        var previousskin = GUI.skin;
        var previouscolor = GUI.color;
        var previousmatrix = GUI.matrix;

        GUI.skin = theme.skin();

        if (openness < 1f)
        {
            GUI.matrix = previousmatrix * Matrix4x4.TRS(new Vector3(0f, (shown - 1f) * 18f, 0f), Quaternion.identity, Vector3.one);
        }

        GUI.color = new Color(1f, 1f, 1f, shown);
        controls.shade(window);
        window = GUI.Window(1337, window, drawwindow, "");

        window.x = Mathf.Clamp(window.x + pendingmove.x, 80f - window.width, Screen.width - 80f);
        window.y = Mathf.Clamp(window.y + pendingmove.y, 0f, Screen.height - headerheight);
        pendingmove = Vector2.zero;

        GUI.matrix = previousmatrix;
        GUI.color = previouscolor;
        GUI.skin = previousskin;
    }

    private void drawwindow(int id)
    {
        if (dirty && Event.current.rawType == EventType.MouseUp)
        {
            dirty = false;
            config.save();
        }

        string focusname = GUI.GetNameOfFocusedControl();
        typing = focusname == "cfgname" || focusname.StartsWith("val:");

        drawdrag();

        if (tab != lasttab)
        {
            lasttab = tab;
            contentfade = 0f;
            scroll = Vector2.zero;
            controls.reset();
        }

        rowindex = 0;

        drawheader();
        drawtabs();

        float top = headerheight + tabheight + 14f;
        float slide = (1f - controls.outcubic(contentfade * 2.2f)) * 14f;
        var content = new Rect(pad, top + slide, window.width - pad * 2f, window.height - top - footerheight - 6f);

        GUILayout.BeginArea(content);
        GUI.color = new Color(1f, 1f, 1f, shown * Mathf.Clamp01(contentfade * 4f));
        drawcontent();
        GUILayout.EndArea();

        GUI.color = new Color(1f, 1f, 1f, shown);
        drawfooter();
    }

    private void drawdrag()
    {
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;

        switch (e.GetTypeForControl(id))
        {
            case EventType.MouseDown:
                if (e.button == 0 && new Rect(0f, 0f, window.width, headerheight).Contains(e.mousePosition))
                {
                    GUIUtility.hotControl = id;
                    e.Use();
                }

                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == id)
                {
                    pendingmove += e.delta;
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();
                }

                break;
        }
    }

    private void drawheader()
    {
        GUI.color = new Color(1f, 1f, 1f, shown);
        GUI.Label(new Rect(pad, 7f, 300f, 30f), "frk<color=#ffffff>.</color>cheat <size=12><color=#7a7a96>v1.0</color></size>", theme.title);
        float right = window.width - pad;
        var cap = new Rect(right - 54f, 11f, 54f, 22f);
        controls.fill(cap, controls.alpha(theme.accent, 0.25f));
        GUI.Label(cap, "INSERT", theme.keycap);
        GUI.Label(new Rect(cap.x - 70f, 11f, 64f, 22f), "to hide", theme.hint);
    }

    private void drawtabs()
    {
        float w = tabwidth;
        float y = headerheight;
        float span = window.width - 32f;

        GUI.color = new Color(1f, 1f, 1f, shown);
        controls.line(new Rect(16f, y + tabheight, span, 1f), new Color(1f, 1f, 1f, 0.1f));

        for (int i = 0; i < tabs.Length; i++)
        {
            var r = new Rect(16f + i * w, y, w, tabheight);

            if (controls.tab(r, tabs[i].ToLower(), i == tab))
            {
                tab = i;
            }
        }

        controls.line(new Rect(16f + indicatorx + 10f, y + tabheight - 2f, w - 20f, 2f), theme.accent);

        float gw = 180f;
        float gx = Mathf.Repeat(Time.unscaledTime * 110f, span + gw) - gw;
        GUI.BeginClip(new Rect(16f, y + tabheight - 1f, span, 3f));
        GUI.color = new Color(theme.accentsoft.r, theme.accentsoft.g, theme.accentsoft.b, 0.7f * shown);
        GUI.DrawTexture(new Rect(gx, 1f, gw, 1f), theme.glow);
        GUI.EndClip();

        GUI.color = new Color(1f, 1f, 1f, shown);
    }

    private void drawfooter()
    {
        GUI.Label(new Rect(pad, window.height - 26f, 300f, 20f), "config: " + config.current, theme.small);

        if (config.status != laststatus)
        {
            laststatus = config.status;
            statusat = Time.unscaledTime;
        }

        if (laststatus == "" || Time.unscaledTime - statusat > 5f)
        {
            return;
        }

        float age = Time.unscaledTime - statusat;
        float alpha = Mathf.Clamp01(Mathf.Min(age * 5f, (5f - age) * 3f));
        float lift = (1f - controls.outcubic(age * 4f)) * 10f;
        float w = Mathf.Min(theme.status.CalcSize(new GUIContent(laststatus)).x + 36f, 360f);
        var r = new Rect(window.width - pad - w, window.height - 36f + lift, w, 26f);

        GUI.color = new Color(1f, 1f, 1f, shown * alpha);
        controls.fill(r, theme.surfacehover);
        controls.line(new Rect(r.x, r.y + 3f, 3f, r.height - 6f), theme.accent);
        GUI.Label(r, laststatus, theme.status);
    }

    private void drawcontent()
    {
        scroll = GUILayout.BeginScrollView(scroll, GUIStyle.none, GUI.skin.verticalScrollbar);

        switch (tabs[tab])
        {
            case "Configs":
                drawconfigs();
                break;

            case "Zones":
                zonelist.draw();
                break;

            default:
                drawfeatures(tabs[tab]);

                if (tabs[tab] == "Items")
                {
                    itemgive.draw();
                    itemedit.draw();
                }
                else if (tabs[tab] == "Fun")
                {
                    chat.draw();
                    musicfile.draw();
                }
                else if (tabs[tab] == "World")
                {
                    prisonlist.draw();
                }
                else if (tabs[tab] == "Menu")
                {
                    prisonstats.draw();
                }

                break;
        }

        GUILayout.EndScrollView();
    }

    private void drawrow(feature f)
    {
        if (!f.toggle && !f.slider && f.run == null)
        {
            return;
        }

        float a = controls.outcubic(contentfade * 2.2f - Mathf.Min(rowindex, 9) * 0.11f);
        rowindex++;

        var r = GUILayoutUtility.GetRect(0f, rowheight, GUILayout.ExpandWidth(true), GUILayout.Height(rowheight));
        GUILayout.Space(2f);
        r.y += (1f - a) * 10f;

        var old = GUI.color;
        GUI.color = new Color(old.r, old.g, old.b, old.a * a);

        float right = r.xMax - 10f;
        var box = Rect.zero;
        var track = Rect.zero;
        var valuerect = Rect.zero;

        if (f.toggle)
        {
            box = new Rect(right - 18f, r.y + (r.height - 18f) * 0.5f, 18f, 18f);
            right = box.x - 14f;
        }

        if (f.slider)
        {
            valuerect = new Rect(right - 58f, r.y + (r.height - 24f) * 0.5f, 58f, 24f);
            track = new Rect(valuerect.x - 178f, r.y + (r.height - 22f) * 0.5f, 170f, 22f);
            right = track.x - 14f;
        }

        var e = Event.current;
        bool card = f.toggle || f.slider;

        if (e.type == EventType.Repaint)
        {
            f.hover = Mathf.MoveTowards(f.hover, r.Contains(e.mousePosition) ? 1f : 0f, Time.unscaledDeltaTime * 10f);

            if (card)
            {
                controls.fill(r, controls.alpha(theme.rowhover, f.hover));
                controls.line(new Rect(r.x + 4f, r.yMax + 1f, r.width - 8f, 1f), new Color(1f, 1f, 1f, 0.05f));
            }
        }

        if (f.run != null && !f.toggle)
        {
            var button = f.slider
                ? new Rect(r.x + 4f, r.y + 4f, Mathf.Max(right - r.x - 4f, 20f), r.height - 8f)
                : new Rect(r.x, r.y + 2f, r.width, r.height - 4f);

            if (controls.button(button, f.name))
            {
                f.press();
            }
        }
        else
        {
            var label = new Rect(r.x + 14f, r.y, Mathf.Max(right - r.x - 14f, 0f), r.height);

            if (e.type == EventType.Repaint)
            {
                var wasgui = GUI.color;
                var pick = Color.Lerp(new Color(0.72f, 0.72f, 0.8f), Color.white, Mathf.Max(f.anim, f.hover));

                if (f.toggle)
                {
                    GUI.color = new Color(theme.accent.r, theme.accent.g, theme.accent.b, wasgui.a * f.anim);
                    GUI.DrawTexture(new Rect(r.x, r.center.y - 9f * f.anim, 2f, 18f * f.anim), theme.white);
                    GUI.color = wasgui;
                }

                controls.text(label, f.name, theme.rowlabel, pick);

                if (f.toggle)
                {
                    controls.box(box, f.anim);
                }
            }
            else if (f.toggle && e.type == EventType.MouseDown && e.button == 0)
            {
                bool hit = f.slider ? label.Contains(e.mousePosition) || box.Contains(e.mousePosition) : r.Contains(e.mousePosition);

                if (hit)
                {
                    f.press();
                    config.save();
                    e.Use();
                }
            }
        }

        if (f.slider)
        {
            float nv = Mathf.Round(controls.slider(track, f.val, f.min, f.max));

            if (nv != f.val)
            {
                f.val = nv;
                dirty = true;
            }

            drawinput(f, valuerect);
        }

        GUI.color = old;
    }

    private void drawinput(feature f, Rect box)
    {
        var e = Event.current;
        string name = "val:" + f.name;
        bool focused = GUI.GetNameOfFocusedControl() == name;

        if (focused && !f.editing)
        {
            f.editing = true;
            f.edit = f.val.ToString();
        }

        GUI.SetNextControlName(name);
        string next = GUI.TextField(box, focused ? f.edit : f.val.ToString(), 9, theme.input);

        if (focused)
        {
            f.edit = digits(next);

            if (e.rawType == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                commit(f);
                GUI.FocusControl("");
            }
            else if (e.rawType == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                f.editing = false;
                GUI.FocusControl("");
            }
            else if (e.rawType == EventType.MouseDown && !box.Contains(e.mousePosition))
            {
                commit(f);
                GUI.FocusControl("");
            }
        }
        else if (f.editing)
        {
            commit(f);
        }
    }

    private void commit(feature f)
    {
        f.editing = false;
        float parsed;

        if (!float.TryParse(f.edit, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
        {
            return;
        }

        float top = f.limit > 0f ? f.limit : f.max;
        float nv = Mathf.Clamp(Mathf.Round(parsed), f.min, top);

        if (nv != f.val)
        {
            f.val = nv;
            config.save();
        }
    }

    private static string digits(string s)
    {
        var sb = new StringBuilder();

        foreach (char c in s)
        {
            if (c >= '0' && c <= '9')
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private void drawfeatures(string tabname)
    {
        bool any = false;

        foreach (var f in features.all)
        {
            if (f.tab != tabname)
            {
                continue;
            }

            any = true;
            drawrow(f);
        }

        if (!any)
        {
            GUILayout.Label("nothing here yet");
        }
    }

    private void drawconfigs()
    {
        foreach (var n in config.names())
        {
            GUILayout.BeginHorizontal();

            if (controls.button(n == config.current ? "> " + n : n))
            {
                config.switchto(n);
            }

            if (n != config.current && controls.button("x", GUILayout.Width(36f)))
            {
                config.remove(n);
                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10f);
        GUI.SetNextControlName("cfgname");
        newname = GUILayout.TextField(newname, 24);

        if (controls.button("create config"))
        {
            config.create(newname);
            newname = "";
            GUIUtility.ExitGUI();
        }

        if (controls.button("add config from clipboard"))
        {
            config.import(GUIUtility.systemCopyBuffer);
            GUIUtility.ExitGUI();
        }

        if (controls.button("copy current config to clipboard"))
        {
            config.export();
        }
    }
}
