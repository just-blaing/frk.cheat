using System.Reflection;
using UnityEngine;

public static class playeractions
{
    private static Character speedowner;
    private static float basespeed = -1f;

    public static void setspeed(Player p, float v)
    {
        if (p == null || p.m_CharacterMovement == null)
        {
            return;
        }

        if (speedowner != p)
        {
            speedowner = p;
            basespeed = p.m_CharacterMovement.m_fMaxSpeed;
        }

        p.m_CharacterMovement.m_fMaxSpeed = v;
    }

    public static void resetspeed()
    {
        if (speedowner != null && speedowner.m_CharacterMovement != null && basespeed > 0f)
        {
            speedowner.m_CharacterMovement.m_fMaxSpeed = basespeed;
        }

        speedowner = null;
        basespeed = -1f;
    }

    public static void setstrength(float v)
    {
        var p = local.player();

        if (p == null)
        {
            return;
        }

        var s = p.m_CharacterStats;
        s.IncreaseStrengthRPC(v - s.Strength);
    }

    public static void setintellect(float v)
    {
        var p = local.player();

        if (p == null)
        {
            return;
        }

        var s = p.m_CharacterStats;
        s.IncreaseIntellectRPC(v - s.Intellect);
    }

    public static void setcardio(float v)
    {
        var p = local.player();

        if (p == null)
        {
            return;
        }

        var s = p.m_CharacterStats;
        s.IncreaseCardioRPC(v - s.Cardio);
    }

    public static void setreputation(float v)
    {
        var me = local.player();

        if (me == null)
        {
            return;
        }

        int count = Player.GetAllCharactersListSize();

        for (int i = 0; i < count; i++)
        {
            var c = Player.GetCharacterAtIndex(i);

            if (c == null || c == me || c.m_CharacterOpinions == null)
            {
                continue;
            }

            c.m_CharacterOpinions.SetOpinionOf(me, Mathf.RoundToInt(v));
        }
    }

    private static Vector3 backpos;
    private static bool away;
    private static float returnat;
    private static bool visited;

    public static void teleport(Vector3 pos)
    {
        var me = local.player();

        if (me == null)
        {
            return;
        }

        me.m_CachedCurrentPosition = pos;
        me.m_Transform.position = pos;
    }

    public static void restore()
    {
        if (!away)
        {
            return;
        }

        away = false;
        teleport(backpos);
    }

    private static bool hasroutine;
    private static Routines lastbase;
    private static RoutineSubTypes lastsub;
    private static readonly FieldInfo jobfield = typeof(Character).GetField("m_JobRoom", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo cellfield = typeof(Character).GetField("m_MyCell", BindingFlags.Instance | BindingFlags.NonPublic);

    private static RoomBlob routineroom(Player me, Routines type)
    {
        var rm = RoomManager.GetInstance();

        if (rm == null)
        {
            return null;
        }

        switch (type)
        {
            case Routines.RollCall:
                return rm.GetFirstRoomByLocation(RoomBlob.eLocation.RollCall);
            case Routines.MealTime:
                return rm.GetFirstRoomByLocation(RoomBlob.eLocation.MealHall);
            case Routines.Exercise:
                return rm.GetFirstRoomByLocation(RoomBlob.eLocation.Gym);
            case Routines.ShowerTime:
                return rm.GetFirstRoomByLocation(RoomBlob.eLocation.Shower);
            case Routines.ShowTime:
                return rm.GetFirstRoomByLocation(RoomBlob.eLocation.ShowTime);
            case Routines.JobTime:
                var job = jobfield != null ? jobfield.GetValue(me) as RoomBlob : null;
                return job != null ? job : rm.GetFirstRoomByLocation(RoomBlob.eLocation.JobOffice);
            case Routines.LightsOut:
            case Routines.Lockdown:
                return cellfield != null ? cellfield.GetValue(me) as RoomBlob : null;
            default:
                return null;
        }
    }

    private const float staytime = 3f;
    private const float checkinterval = 0.1f;
    private static float nextcheck;

    public static void routine()
    {
        if (Time.unscaledTime < nextcheck)
        {
            return;
        }

        nextcheck = Time.unscaledTime + checkinterval;
        var rm = RoutineManager.GetInstance();
        var me = local.player();

        if (rm == null || me == null)
        {
            return;
        }

        var current = rm.GetCurrentRoutine();

        if (current == null)
        {
            return;
        }

        if (!hasroutine || current.m_BaseRoutineType != lastbase || current.m_SubRoutineType != lastsub)
        {
            hasroutine = true;
            lastbase = current.m_BaseRoutineType;
            lastsub = current.m_SubRoutineType;
            visited = false;
        }

        if (away)
        {
            if (Time.unscaledTime >= returnat)
            {
                restore();
            }

            return;
        }

        if (visited)
        {
            return;
        }

        var room = routineroom(me, lastbase);

        if (room == null)
        {
            return;
        }

        visited = true;
        backpos = me.m_Transform.position;
        away = true;
        returnat = Time.unscaledTime + staytime;
        teleport(room.transform.position);
    }
}
