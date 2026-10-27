using UnityEngine;

public static class theme
{
    public static readonly Color bg = new Color32(11, 11, 18, 255);
    public static readonly Color panel = new Color32(17, 17, 27, 255);
    public static readonly Color surface = new Color32(26, 26, 40, 255);
    public static readonly Color surfacehover = new Color32(40, 40, 64, 255);
    public static readonly Color field = new Color32(13, 13, 21, 255);
    public static readonly Color line = new Color32(48, 48, 72, 255);
    public static readonly Color text = new Color32(226, 226, 240, 255);
    public static readonly Color dim = new Color32(122, 122, 150, 255);
    public static readonly Color accent = new Color32(255, 255, 255, 255);
    public static readonly Color accentsoft = new Color32(255, 255, 255, 255);
    public static readonly Color rowhover = new Color32(24, 24, 38, 255);
    public static readonly Color trackcolor = new Color32(44, 44, 66, 255);

    public static Texture2D white;
    public static Texture2D boxoff;
    public static Texture2D boxon;
    public static Texture2D tick;
    public static Texture2D knob;
    public static Texture2D glow;

    public static GUIStyle title;
    public static GUIStyle hint;
    public static GUIStyle rowlabel;
    public static GUIStyle value;
    public static GUIStyle tabbutton;
    public static GUIStyle tabselected;
    public static GUIStyle status;
    public static GUIStyle clean;
    public static GUIStyle tile;
    public static GUIStyle barstyle;
    public static GUIStyle shadow;
    public static GUIStyle buttontext;
    public static GUIStyle keycap;
    public static GUIStyle small;
    public static GUIStyle input;

    private static GUISkin cached;
    private static bool ready;

    public static Texture2D shape(int w, int h, float radius, Color fill, Color border, float bw)
    {
        var t = new Texture2D(w, h, TextureFormat.ARGB32, false);
        float hx = w * 0.5f;
        float hy = h * 0.5f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float px = Mathf.Abs(x + 0.5f - hx) - (hx - radius);
                float py = Mathf.Abs(y + 0.5f - hy) - (hy - radius);
                float mx = Mathf.Max(px, 0f);
                float my = Mathf.Max(py, 0f);
                float d = Mathf.Sqrt(mx * mx + my * my) + Mathf.Min(Mathf.Max(px, py), 0f) - radius;
                float outer = Mathf.Clamp01(0.5f - d);
                float inner = Mathf.Clamp01(0.5f - (d + bw));
                var c = bw > 0f ? Color.Lerp(border, fill, inner) : fill;
                c.a *= outer;
                t.SetPixel(x, y, c);
            }
        }

        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return t;
    }

    public static Texture2D bar(int w, int h, float barh, Color fill)
    {
        var t = new Texture2D(w, h, TextureFormat.ARGB32, false);
        float hx = w * 0.5f;
        float hy = h * 0.5f;
        float r = barh * 0.5f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float px = Mathf.Abs(x + 0.5f - hx) - (hx - r);
                float py = Mathf.Abs(y + 0.5f - hy);
                float mx = Mathf.Max(px, 0f);
                float my = Mathf.Max(py, 0f);
                float d = Mathf.Sqrt(mx * mx + my * my) + Mathf.Min(Mathf.Max(px, py), 0f) - r;
                var c = fill;
                c.a *= Mathf.Clamp01(0.5f - d);
                t.SetPixel(x, y, c);
            }
        }

        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return t;
    }

    private static Texture2D softshadow()
    {
        const int size = 64;
        var t = new Texture2D(size, size, TextureFormat.ARGB32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x + 0.5f - size * 0.5f) - 8f, 0f);
                float dy = Mathf.Max(Mathf.Abs(y + 0.5f - size * 0.5f) - 8f, 0f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - d / 24f);
                t.SetPixel(x, y, new Color(0f, 0f, 0f, a * a * 0.7f));
            }
        }

        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return t;
    }

    private static Texture2D glowstrip()
    {
        const int w = 64;
        var t = new Texture2D(w, 2, TextureFormat.ARGB32, false);

        for (int x = 0; x < w; x++)
        {
            float s = Mathf.Sin(Mathf.PI * (x + 0.5f) / w);

            for (int y = 0; y < 2; y++)
            {
                t.SetPixel(x, y, new Color(1f, 1f, 1f, s * s));
            }
        }

        t.filterMode = FilterMode.Bilinear;
        t.wrapMode = TextureWrapMode.Clamp;
        t.Apply();
        return t;
    }

    private static void backgrounds(GUIStyle s, Texture2D normal, Texture2D hover, Texture2D active, Texture2D focused)
    {
        s.normal.background = normal;
        s.hover.background = hover;
        s.active.background = active;
        s.focused.background = focused;
        s.onNormal.background = normal;
        s.onHover.background = hover;
        s.onActive.background = active;
        s.onFocused.background = focused;
    }

    private static void colors(GUIStyle s, Color normal, Color hover, Color active)
    {
        s.normal.textColor = normal;
        s.hover.textColor = hover;
        s.active.textColor = active;
        s.focused.textColor = normal;
        s.onNormal.textColor = normal;
        s.onHover.textColor = hover;
        s.onActive.textColor = active;
        s.onFocused.textColor = normal;
    }

    private static GUIStyle make(int fontsize, TextAnchor anchor)
    {
        return new GUIStyle
        {
            fontSize = fontsize,
            alignment = anchor,
            wordWrap = false,
            clipping = TextClipping.Clip,
            richText = true
        };
    }

    public static void prepare()
    {
        if (ready)
        {
            return;
        }

        ready = true;

        white = shape(4, 4, 0f, Color.white, Color.white, 0f);
        boxoff = shape(18, 18, 3f, field, line, 1f);
        boxon = shape(18, 18, 3f, accent, Color.clear, 0f);
        tick = shape(8, 8, 1.5f, bg, Color.clear, 0f);
        knob = shape(10, 20, 3f, Color.white, Color.clear, 0f);
        glow = glowstrip();

        tile = new GUIStyle { border = new RectOffset(4, 4, 4, 4) };
        tile.normal.background = shape(12, 12, 3f, Color.white, Color.clear, 0f);

        barstyle = new GUIStyle { border = new RectOffset(2, 2, 0, 0) };
        barstyle.normal.background = bar(12, 4, 4f, Color.white);

        shadow = new GUIStyle { border = new RectOffset(24, 24, 24, 24) };
        shadow.normal.background = softshadow();

        buttontext = make(13, TextAnchor.MiddleCenter);
        colors(buttontext, Color.white, Color.white, Color.white);

        keycap = make(10, TextAnchor.MiddleCenter);
        keycap.fontStyle = FontStyle.Bold;
        colors(keycap, text, text, text);

        var inputbg = shape(24, 24, 3f, field, line, 1f);
        var inputfocus = shape(24, 24, 3f, field, accent, 1f);
        input = make(12, TextAnchor.MiddleCenter);
        backgrounds(input, inputbg, inputbg, inputfocus, inputfocus);
        colors(input, Color.white, Color.white, Color.white);
        input.border = new RectOffset(6, 6, 6, 6);
        input.padding = new RectOffset(4, 4, 0, 0);

        small = make(11, TextAnchor.MiddleLeft);
        colors(small, dim, dim, dim);

        title = make(17, TextAnchor.MiddleLeft);
        title.fontStyle = FontStyle.Bold;
        colors(title, Color.white, Color.white, Color.white);

        hint = make(12, TextAnchor.MiddleRight);
        colors(hint, dim, dim, dim);

        status = make(12, TextAnchor.MiddleCenter);
        colors(status, text, text, text);

        rowlabel = make(13, TextAnchor.MiddleLeft);
        colors(rowlabel, Color.white, Color.white, Color.white);

        value = make(12, TextAnchor.MiddleRight);
        colors(value, dim, dim, dim);

        tabbutton = make(13, TextAnchor.MiddleCenter);
        colors(tabbutton, Color.white, Color.white, Color.white);

        tabselected = make(13, TextAnchor.MiddleCenter);
        tabselected.fontStyle = FontStyle.Bold;
        colors(tabselected, Color.white, Color.white, Color.white);

        clean = new GUIStyle();
    }

    public static GUISkin skin()
    {
        if (cached != null)
        {
            return cached;
        }

        prepare();

        var s = UnityEngine.Object.Instantiate(GUI.skin);

        var winbg = shape(40, 40, 4f, bg, new Color(accent.r, accent.g, accent.b, 0.55f), 1f);
        var surf = shape(24, 24, 3f, surface, Color.clear, 0f);
        var surfhover = shape(24, 24, 3f, surfacehover, Color.clear, 0f);
        var surfaccent = shape(24, 24, 3f, accent, Color.clear, 0f);
        var fieldbg = shape(24, 24, 3f, field, line, 1f);
        var fieldfocus = shape(24, 24, 3f, field, accent, 1f);
        var panelbg = shape(24, 24, 3f, panel, line, 1f);
        var thumbbg = shape(8, 8, 4f, new Color(accent.r, accent.g, accent.b, 0.7f), Color.clear, 0f);
        var track = bar(12, 4, 4f, trackcolor);

        var window = make(13, TextAnchor.UpperLeft);
        backgrounds(window, winbg, winbg, winbg, winbg);
        colors(window, text, text, text);
        window.border = new RectOffset(10, 10, 10, 10);
        s.window = window;

        var box = make(13, TextAnchor.UpperLeft);
        backgrounds(box, panelbg, panelbg, panelbg, panelbg);
        colors(box, text, text, text);
        box.border = new RectOffset(6, 6, 6, 6);
        box.padding = new RectOffset(10, 10, 8, 8);
        box.margin = new RectOffset(4, 4, 4, 4);
        s.box = box;

        var label = make(13, TextAnchor.MiddleLeft);
        colors(label, text, text, text);
        label.padding = new RectOffset(4, 4, 3, 3);
        label.margin = new RectOffset(2, 2, 2, 2);
        s.label = label;

        var button = make(13, TextAnchor.MiddleCenter);
        backgrounds(button, surf, surfhover, surfaccent, surf);
        colors(button, text, Color.white, bg);
        button.border = new RectOffset(6, 6, 6, 6);
        button.padding = new RectOffset(12, 12, 6, 6);
        button.margin = new RectOffset(3, 3, 3, 3);
        button.fixedHeight = 30f;
        s.button = button;
        s.toggle = new GUIStyle(button);

        var textfield = make(13, TextAnchor.MiddleLeft);
        backgrounds(textfield, fieldbg, fieldbg, fieldfocus, fieldfocus);
        colors(textfield, text, text, text);
        textfield.border = new RectOffset(6, 6, 6, 6);
        textfield.padding = new RectOffset(10, 10, 6, 6);
        textfield.margin = new RectOffset(3, 3, 3, 3);
        textfield.fixedHeight = 30f;
        textfield.clipping = TextClipping.Clip;
        s.textField = textfield;

        s.scrollView = new GUIStyle();

        s.verticalScrollbar = new GUIStyle { fixedWidth = 5f, margin = new RectOffset(3, 0, 0, 0) };

        var vthumb = new GUIStyle { fixedWidth = 5f, border = new RectOffset(2, 2, 2, 2) };
        backgrounds(vthumb, thumbbg, thumbbg, thumbbg, thumbbg);
        s.verticalScrollbarThumb = vthumb;

        s.verticalScrollbarUpButton = new GUIStyle { fixedWidth = 0f, fixedHeight = 0f };
        s.verticalScrollbarDownButton = new GUIStyle { fixedWidth = 0f, fixedHeight = 0f };
        s.horizontalScrollbar = new GUIStyle { fixedHeight = 0f };
        s.horizontalScrollbarThumb = new GUIStyle { fixedHeight = 0f };
        s.horizontalScrollbarLeftButton = new GUIStyle { fixedWidth = 0f, fixedHeight = 0f };
        s.horizontalScrollbarRightButton = new GUIStyle { fixedWidth = 0f, fixedHeight = 0f };

        var slider = new GUIStyle { fixedHeight = 4f, border = new RectOffset(2, 2, 0, 0), margin = new RectOffset(6, 6, 9, 9) };
        backgrounds(slider, track, track, track, track);
        s.horizontalSlider = slider;

        var thumb = new GUIStyle { fixedWidth = 10f, fixedHeight = 20f };
        backgrounds(thumb, theme.knob, theme.knob, theme.knob, theme.knob);
        s.horizontalSliderThumb = thumb;

        cached = s;
        return cached;
    }
}
