using System.Collections;
using System.Reflection;
using UnityEngine;

public static class doors
{
    private const float radius = 4f;
    private static readonly BindingFlags anyfield = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static MethodInfo allow = typeof(DoorManager).GetMethod("SetDoorAsAllowedForCharacter", anyfield);
    private static FieldInfo ignores = typeof(DoorManager).GetField("m_CharacterDoorIgnores", anyfield);
    private static Character setup;
    private static Door[] list = new Door[0];
    private static float next;
    private static float nextscan;

    public static void tick()
    {
        if (Time.time < next)
        {
            return;
        }

        next = Time.time + 0.25f;

        var me = local.player();
        var dm = DoorManager.GetInstance();

        if (me == null || dm == null || allow == null || ignores == null)
        {
            return;
        }

        if (setup != me)
        {
            setup = me;
            dm.SetUpCharacterKeys(me);
        }

        var table = ignores.GetValue(dm) as IDictionary;

        if (table == null || !table.Contains(me))
        {
            return;
        }

        if (Time.time >= nextscan)
        {
            nextscan = Time.time + 2f;
            list = Object.FindObjectsOfType<Door>();
        }

        foreach (var d in list)
        {
            if (d == null || d.Collider == null || Vector3.Distance(d.transform.position, me.transform.position) > radius)
            {
                continue;
            }

            if (me.IsAllowedThroughDoor(d.Local_DoorID))
            {
                continue;
            }

            allow.Invoke(dm, new object[] { me.m_PhysicsSphereCol, d, me, null });
        }
    }
}
