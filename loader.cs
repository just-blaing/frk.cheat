using UnityEngine;

public class loader
{
    public bool finished;

    private readonly string[] names = { "loading features", "reading config", "building interface", "almost there" };
    private const string label = "frk.cheat";
    private const float stepdelay = 0.45f;
    private const float holdtime = 0.5f;
    private const float toastlife = 5f;

    private int step;
    private float started = -1f;
    private float nextat;
    private float stagechange;
    private float finishedat;
    private float toastat = -1f;
    private float progress;

    private GUIStyle big;
    private GUIStyle sub;
    private GUIStyle pct;
    private GUIStyle toast;

    public bool done
    {
        get { return step >= names.Length; }
    }

    public void run(bool ongui)
    {
        if (done)
        {
            return;
        }

        float now = Time.unscaledTime;

        if (started < 0f)
        {
            started = now;
            stagechange = now;
            nextat = now + 0.6f;
        }

        if (now < nextat || (step == 2) != ongui)
        {
            return;
        }

        execute(step);

        step++;
        nextat = now + stepdelay;
        stagechange = now;

        if (done)
        {
            finishedat = now;
        }
    }

    private void execute(int index)
    {
        switch (index)
        {
            case 1:
                config.start();
                break;

            case 2:
                theme.skin();
                break;
        }
    }

    private void styles()
    {
        if (big != null)
        {
            return;
        }

        big = new GUIStyle { fontSize = 54, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Overflow };
        big.normal.textColor = Color.white;

        sub = new GUIStyle { fontSize = 14, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Overflow };
        sub.normal.textColor = Color.white;

        pct = new GUIStyle { fontSize = 12, alignment = TextAnchor.MiddleRight };
        pct.normal.textColor = Color.white;

        toast = new GUIStyle { fontSize = 13, alignment = TextAnchor.MiddleCenter, richText = true };
        toast.normal.textColor = Color.white;
    }

    public void draw()
    {
        if (finished && Time.unscaledTime - toastat > toastlife)
        {
            return;
        }

        if (!done)
        {
            run(true);
        }

        if (Event.current.type != EventType.Repaint || started < 0f)
        {
            return;
        }

        theme.prepare();
        styles();

        float now = Time.unscaledTime;

        if (finished)
        {
            drawtoast(now);
            return;
        }

        float t = now - started;
        float dt = Time.unscaledDeltaTime;
        float target = done ? 1f : (step + Mathf.Clamp01((now - stagechange) / stepdelay) * 0.85f) / names.Length;
        progress = Mathf.Lerp(progress, target, 1f - Mathf.Exp(-dt * 7f));

        float exitp = done ? Mathf.Clamp01((now - finishedat - holdtime) / 0.4f) : 0f;

        if (exitp >= 1f)
        {
            finished = true;
            toastat = now;
            return;
        }

        float alpha = Mathf.Clamp01(t / 0.35f) * (1f - controls.inout(exitp));
        var oldcolor = GUI.color;

        GUI.color = new Color(0f, 0f, 0f, 0.78f * alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

        float x = 56f;
        float y = Screen.height - 210f;
        float drift = controls.inout(exitp) * -30f;

        for (int i = 0; i < label.Length; i++)
        {
            float px = i == 0 ? 0f : big.CalcSize(new GUIContent(label.Substring(0, i))).x;
            float la = Mathf.Clamp01((t - 0.2f - i * 0.05f) / 0.3f);
            float off = (1f - controls.outcubic(la)) * -24f;

            GUI.color = new Color(1f, 1f, 1f, alpha * la);
            controls.text(new Rect(x + px + off + drift, y, 70f, 64f), label[i].ToString(), big, label[i] == '.' ? theme.accent : Color.white);
        }

        string stage = done ? "ready" : names[step] + new string('.', (int)(now * 3f) % 4);
        float sa = Mathf.Clamp01((now - stagechange) * 5f) * Mathf.Clamp01((t - 0.5f) * 4f);

        GUI.color = new Color(1f, 1f, 1f, alpha * sa);
        controls.text(new Rect(x + 2f + drift, y + 68f, 320f, 22f), stage, sub, theme.dim);

        float barh = 3f;
        float by = Screen.height - barh;
        float fw = Screen.width * progress;

        GUI.color = new Color(1f, 1f, 1f, 0.08f * alpha);
        GUI.DrawTexture(new Rect(0f, by, Screen.width, barh), theme.white);

        GUI.color = new Color(theme.accent.r, theme.accent.g, theme.accent.b, alpha);
        GUI.DrawTexture(new Rect(0f, by, fw, barh), theme.white);

        GUI.color = new Color(theme.accentsoft.r, theme.accentsoft.g, theme.accentsoft.b, alpha);
        GUI.DrawTexture(new Rect(fw - 160f, by - 3f, 160f, 8f), theme.glow);

        GUI.color = new Color(1f, 1f, 1f, alpha);
        controls.text(new Rect(Screen.width - 130f, by - 30f, 106f, 20f), Mathf.RoundToInt(progress * 100f) + "%", pct, theme.dim);

        GUI.color = oldcolor;
    }

    private void drawtoast(float now)
    {
        float age = now - toastat;
        float a = Mathf.Clamp01(Mathf.Min(age * 3f, (toastlife - age) * 2f));
        float slide = (1f - controls.outcubic(age * 3f)) * 40f;
        var r = new Rect(Screen.width - 314f + slide, 24f, 290f, 36f);
        var old = GUI.color;

        GUI.color = new Color(1f, 1f, 1f, a);
        controls.shade(r);
        controls.fill(r, theme.panel);
        controls.line(new Rect(r.x, r.y + 4f, 3f, r.height - 8f), theme.accent);
        controls.text(r, "press <b>INSERT</b> to open the menu", toast, theme.text);

        GUI.color = old;
    }
}
