using System.Collections.Generic;
using UnityEngine;

public static class controls
{
    private static readonly Dictionary<int, float> tweens = new Dictionary<int, float>();

    public static float outcubic(float t)
    {
        t = Mathf.Clamp01(t);
        float u = 1f - t;
        return 1f - u * u * u;
    }

    public static float outback(float t)
    {
        t = Mathf.Clamp01(t);
        const float c = 1.70158f;
        float u = t - 1f;
        return 1f + (c + 1f) * u * u * u + c * u * u;
    }

    public static float inout(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    public static void reset()
    {
        tweens.Clear();
    }

    public static float tween(int id, bool target, float speed)
    {
        float v;
        tweens.TryGetValue(id, out v);

        if (Event.current.type == EventType.Repaint)
        {
            v = Mathf.MoveTowards(v, target ? 1f : 0f, Time.unscaledDeltaTime * speed);
            tweens[id] = v;
        }

        return v;
    }

    public static Color alpha(Color c, float a)
    {
        return new Color(c.r, c.g, c.b, c.a * a);
    }

    public static void fill(Rect r, Color c)
    {
        var old = GUI.color;
        GUI.color = new Color(c.r, c.g, c.b, c.a * old.a);
        GUI.DrawTexture(r, theme.white);
        GUI.color = old;
    }

    public static void line(Rect r, Color c)
    {
        var old = GUI.color;
        GUI.color = new Color(c.r, c.g, c.b, c.a * old.a);
        GUI.DrawTexture(r, theme.white);
        GUI.color = old;
    }

    public static void shade(Rect r)
    {
        var old = GUI.color;

        for (int i = 0; i < 8; i++)
        {
            float grow = 2f + i * 3f;
            GUI.color = new Color(0f, 0f, 0f, 0.07f * old.a);
            GUI.DrawTexture(new Rect(r.x - grow, r.y - grow + 6f, r.width + grow * 2f, r.height + grow * 2f), theme.white);
        }

        GUI.color = old;
    }

    public static void box(Rect r, float anim)
    {
        var old = GUI.color;
        GUI.DrawTexture(r, theme.boxoff);

        GUI.color = new Color(old.r, old.g, old.b, old.a * anim);
        GUI.DrawTexture(r, theme.boxon);

        float s = 8f * anim;
        GUI.DrawTexture(new Rect(r.center.x - s * 0.5f, r.center.y - s * 0.5f, s, s), theme.tick);
        GUI.color = old;
    }

    public static void text(Rect r, string s, GUIStyle style, Color c)
    {
        var old = GUI.contentColor;
        GUI.contentColor = c;
        GUI.Label(r, s, style);
        GUI.contentColor = old;
    }

    public static bool button(Rect r, string label)
    {
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        bool inside = r.Contains(e.mousePosition);
        bool clicked = false;

        switch (e.GetTypeForControl(id))
        {
            case EventType.MouseDown:
                if (inside && e.button == 0)
                {
                    GUIUtility.hotControl = id;
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    clicked = inside;
                    e.Use();
                }

                break;

            case EventType.Repaint:
                float h = tween(id, inside && (GUIUtility.hotControl == 0 || GUIUtility.hotControl == id), 10f);
                bool pressed = GUIUtility.hotControl == id && inside;
                var tint = pressed ? theme.accent : Color.Lerp(theme.surface, theme.surfacehover, h);

                fill(r, tint);
                line(new Rect(r.x, r.y + 5f, 2f, r.height - 10f), alpha(theme.accent, h));
                text(r, label, theme.buttontext, pressed ? theme.bg : Color.Lerp(theme.text, Color.white, h));
                break;
        }

        return clicked;
    }

    public static bool button(string label, params GUILayoutOption[] options)
    {
        var r = GUILayoutUtility.GetRect(new GUIContent(label), GUI.skin.button, options);
        return button(r, label);
    }

    public static bool tab(Rect r, string label, bool selected)
    {
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        bool inside = r.Contains(e.mousePosition);
        bool clicked = false;

        switch (e.GetTypeForControl(id))
        {
            case EventType.MouseDown:
                if (inside && e.button == 0)
                {
                    GUIUtility.hotControl = id;
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    clicked = inside;
                    e.Use();
                }

                break;

            case EventType.Repaint:
                float h = tween(id, inside && (GUIUtility.hotControl == 0 || GUIUtility.hotControl == id), 10f);
                text(r, label, selected ? theme.tabselected : theme.tabbutton, selected ? Color.white : Color.Lerp(theme.dim, Color.white, h));
                break;
        }

        return clicked;
    }

    public static float slider(Rect r, float v, float min, float max)
    {
        int id = GUIUtility.GetControlID(FocusType.Passive);
        var e = Event.current;
        var hit = new Rect(r.x - 4f, r.y - 4f, r.width + 8f, r.height + 8f);
        bool inside = hit.Contains(e.mousePosition);
        float travel = Mathf.Max(r.width - 20f, 1f);

        switch (e.GetTypeForControl(id))
        {
            case EventType.MouseDown:
                if (inside && e.button == 0)
                {
                    GUIUtility.hotControl = id;
                    v = Mathf.Lerp(min, max, Mathf.Clamp01((e.mousePosition.x - r.x - 10f) / travel));
                    GUI.changed = true;
                    e.Use();
                }

                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == id)
                {
                    v = Mathf.Lerp(min, max, Mathf.Clamp01((e.mousePosition.x - r.x - 10f) / travel));
                    GUI.changed = true;
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

            case EventType.Repaint:
                bool active = GUIUtility.hotControl == id;
                float h = tween(id, active || (inside && GUIUtility.hotControl == 0), 12f);
                float t = Mathf.Approximately(max, min) ? 0f : Mathf.Clamp01((v - min) / (max - min));
                float knobx = r.x + 10f + t * travel;
                float cy = r.center.y;
                var track = new Rect(r.x, cy - 2f, r.width, 4f);
                var old = GUI.color;

                GUI.color = new Color(theme.trackcolor.r, theme.trackcolor.g, theme.trackcolor.b, old.a);
                GUI.DrawTexture(track, theme.white);

                GUI.color = new Color(theme.accent.r, theme.accent.g, theme.accent.b, old.a * Mathf.Lerp(0.85f, 1f, h));
                GUI.DrawTexture(new Rect(track.x, track.y, Mathf.Max(knobx - track.x, 4f), track.height), theme.white);

                float w = 8f + 2f * h;
                float kh = 16f + 4f * h;
                GUI.color = new Color(1f, 1f, 1f, old.a);
                GUI.DrawTexture(new Rect(knobx - w * 0.5f, cy - kh * 0.5f, w, kh), theme.knob);
                GUI.color = old;
                break;
        }

        return v;
    }

    public static float slider(float v, float min, float max)
    {
        var r = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
        return slider(r, v, min, max);
    }
}
