using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class extras
{
    private static void addall<T>(List<T> list) where T : struct
    {
        foreach (T v in Enum.GetValues(typeof(T)))
        {
            if (v.ToString() != "NULL")
            {
                list.Add(v);
            }
        }
    }

    public static void unlockcosmetics()
    {
        var um = UnlockManager.GetInstance();

        if (um == null)
        {
            return;
        }

        var set = new CustomisationSet();
        addall(set.bodyTypes);
        addall(set.skinColours);
        addall(set.hairs);
        addall(set.hats);
        addall(set.upperFaces);
        addall(set.lowerFaces);
        um.UnlockCustomisationSet(set, false, true);
    }

    private static readonly FieldInfo levelsfield = typeof(LevelDataManager).GetField("m_T17Levels", BindingFlags.Instance | BindingFlags.NonPublic);

    private static PrisonData[] prisons()
    {
        var ldm = LevelDataManager.GetInstance();

        if (ldm == null || levelsfield == null)
        {
            return new PrisonData[0];
        }

        return levelsfield.GetValue(ldm) as PrisonData[] ?? new PrisonData[0];
    }

    public static void unlockprisons()
    {
        var pm = ProgressManager.GetInstance();

        foreach (var prison in prisons())
        {
            if (prison == null)
            {
                continue;
            }

            prison.m_bIsDLC = false;

            if (pm != null && prison.m_UnlockMilestone != null)
            {
                pm.SetMilestoneAchieved(prison.m_UnlockMilestone.id, true);
            }
        }

        if (pm != null)
        {
            pm.ProcessPendingMilestones();
        }
    }

    private static void ensurekeyaward()
    {
        if (KeyAwardManager.GetInstance() == null)
        {
            new GameObject("keyawardmanager").AddComponent<KeyAwardManager>();
        }
    }

    public static void escapeall(Gamer gamer)
    {
        if (gamer == null)
        {
            return;
        }

        var methods = (EscapeMethod[])Enum.GetValues(typeof(EscapeMethod));

        foreach (var prison in prisons())
        {
            if (prison == null || prison.m_LevelInfo == null)
            {
                continue;
            }

            foreach (var method in methods)
            {
                if (method != EscapeMethod.Unknown)
                {
                    KeyAwardManager.OnPrisonEscaped(gamer, prison.m_LevelInfo, method);
                }
            }
        }
    }

    private static readonly PropertyInfo modeprop = typeof(GlobalStart).GetProperty("m_GlobalStartMode", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void showfrontend()
    {
        var gs = GlobalStart.GetInstance();

        if (gs == null || modeprop == null)
        {
            return;
        }

        modeprop.SetValue(gs, GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND, null);
    }

    public static void settime(float h, float m)
    {
        var rm = RoutineManager.GetInstance();

        if (rm != null)
        {
            rm.SetTime((int)h, (int)m, false);
        }
    }

    private static void setdays(float days)
    {
        var rm = RoutineManager.GetInstance();

        if (rm == null)
        {
            return;
        }

        var field = typeof(RoutineManager).GetField("m_ElapsedInGameDays", BindingFlags.Instance | BindingFlags.NonPublic);

        if (field == null)
        {
            return;
        }

        field.SetValue(rm, (int)days);
        rm.SetTime(rm.TimeHourPart, 0, false);
    }

    private static void setstat(int id, float value)
    {
        var gamer = Gamer.GetPrimaryGamer();
        var ss = StatSystem.GetInstance();

        if (gamer == null || ss == null)
        {
            return;
        }

        ss.SetStat(id, value, gamer);
    }

    public static void maxprogress()
    {
        ensurekeyaward();
        var gamer = Gamer.GetPrimaryGamer();
        escapeall(gamer);
        var pm = ProgressManager.GetInstance();
        var ss = StatSystem.GetInstance();

        if (pm == null || ss == null || gamer == null)
        {
            return;
        }

        foreach (var m in pm.m_Milestones)
        {
            if (m == null || m.criteria == null)
            {
                continue;
            }

            foreach (var c in m.criteria)
            {
                if (c == null)
                {
                    continue;
                }

                var rule = c.statRule;
                int id = (int)rule.m_StatID;

                if (rule.m_Compare == StatsTracking.StatCompare.HAS_ID)
                {
                    ss.AddIDStat(id, (int)rule.m_RefValue, gamer);
                }
                else if (rule.m_Compare == StatsTracking.StatCompare.EQUAL)
                {
                    ss.SetStat(id, rule.m_RefValue, gamer);
                }
                else
                {
                    ss.SetStat(id, Mathf.Max(ss.GetStatValue(id), rule.m_RefValue), gamer);
                }
            }

            pm.SetMilestoneAchieved(m.id, true);
        }

        pm.ProcessPendingMilestones();
    }

    private static float nextzoom;

    public static void minimapzoom(float v)
    {
        foreach (var m in UnityEngine.Object.FindObjectsOfType<MiniMap>())
        {
            if (m == null)
            {
                continue;
            }

            m.m_MapZoom = v;
            m.m_ScaleOfIcons = new Vector3(2f, 2f, 1f) * (v / 4f);
            var pool = m.transform.parent.GetComponentInChildren<IconPool>();

            if (pool == null)
            {
                continue;
            }

            foreach (var icon in pool.m_ActiveObjects)
            {
                if (icon != null)
                {
                    icon.transform.localScale = m.m_ScaleOfIcons;
                }
            }
        }
    }

    public static float zoom()
    {
        var f = features.all.Find(x => x.name == "minimap zoom");
        return f != null ? f.val : 4f;
    }

    public static void init()
    {
        features.all.Add(new feature
        {
            name = "unlock all cosmetics",
            tab = "Menu",
            run = unlockcosmetics
        });

        features.all.Add(new feature
        {
            name = "100% progress",
            tab = "Menu",
            run = maxprogress
        });

        features.all.Add(new feature
        {
            name = "unlock all DLC and prisons",
            tab = "Menu",
            run = unlockprisons
        });

        var favours = new feature
        {
            name = "favours done",
            tab = "Player",
            slider = true,
            min = 0f,
            max = 999f,
            val = 0f
        };

        favours.run = () => setstat((int)STAT_IDS.FavoursCompleted, favours.val);
        features.all.Add(favours);

        var knockouts = new feature
        {
            name = "knockouts",
            tab = "Player",
            slider = true,
            min = 0f,
            max = 999f,
            val = 0f
        };

        knockouts.run = () => setstat((int)STAT_IDS.InmatesKOd, knockouts.val);
        features.all.Add(knockouts);

        var hour = new feature
        {
            name = "hour",
            tab = "World",
            slider = true,
            min = 0f,
            max = 23f,
            val = 12f
        };

        var minute = new feature
        {
            name = "minute",
            tab = "World",
            slider = true,
            min = 0f,
            max = 59f,
            val = 0f
        };

        hour.run = () => settime(hour.val, minute.val);
        minute.run = () => settime(hour.val, minute.val);
        features.all.Add(hour);
        features.all.Add(minute);

        var daysfeature = new feature
        {
            name = "days in prison",
            tab = "World",
            slider = true,
            min = 0f,
            max = 365f,
            val = 0f
        };

        daysfeature.run = () => setdays(daysfeature.val);
        features.all.Add(daysfeature);

        features.all.Add(new feature
        {
            name = "minimap zoom",
            tab = "Visual",
            toggle = true,
            slider = true,
            min = 1f,
            max = 10f,
            val = 4f,
            tick = () =>
            {
                if (Time.time >= nextzoom)
                {
                    nextzoom = Time.time + 0.2f;
                    minimapzoom(extras.zoom());
                }
            },
            off = () => minimapzoom(4f)
        });
    }
}
