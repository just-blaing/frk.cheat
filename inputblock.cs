using System.Reflection;
using HarmonyLib;

public static class inputblock
{
    public static bool active;

    public static void apply()
    {
        var harmony = new Harmony("frk.cheat.inputblock");
        var update = typeof(T17EventSystem).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        harmony.Patch(update, new HarmonyMethod(typeof(inputblock).GetMethod("skip")));
    }

    public static bool skip()
    {
        return !active;
    }
}
