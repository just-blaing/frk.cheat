using System;

public class feature
{
    public string name;
    public string tab;
    public bool toggle;
    public bool on;
    public bool slider;
    public float min;
    public float max = 100f;
    public float val;
    public Action run;
    public Action tick;
    public Action draw;
    public Action off;

    public float anim;
    public float hover;
    public float limit;
    public bool editing;
    public string edit = "";

    public void press()
    {
        if (toggle)
        {
            on = !on;
            if (!on && off != null)
            {
                off();
            }
        }
        else if (run != null)
        {
            run();
        }
    }
}
