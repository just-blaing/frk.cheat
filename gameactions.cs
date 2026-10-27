using UnityEngine;

public static class gameactions
{
    public static void win()
    {
        var e = EscapePrisonFunctionality.GetInstance();

        if (e != null)
        {
            e.TriggerEscape();
        }
    }
}
