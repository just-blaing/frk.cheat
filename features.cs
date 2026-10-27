using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class features
{
    public static List<feature> all = new List<feature>();

    private const BindingFlags anyfield = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static FieldInfo hpflag = typeof(CharacterStats).GetField("m_bInfinitePlayerHealthOn", anyfield);
    private static FieldInfo enflag = typeof(CharacterStats).GetField("m_bInfinitePlayerEnergyOn", anyfield);
    private static FieldInfo recoveryfield = typeof(Character).GetField("m_fAttackRecoveryTime", anyfield);
    private const float cooldowncap = 0.2f;
    private static MethodInfo setalertness = typeof(PrisonAlertnessManager).GetMethod("SetPrisonAlertnessRPC", anyfield);

    private static CharacterStats laststats;
    private static List<DeskInteraction> desks = new List<DeskInteraction>();
    private static List<CCTVCamera> cams = new List<CCTVCamera>();
    private static float nextdesksscan;
    private static float nextdesks;
    private static float nextcamsscan;
    private static float nextcams;
    private static List<ContrabandDetector> scanners = new List<ContrabandDetector>();
    private static float nextscannersscan;
    private static float nextscanners;
    private static readonly FieldInfo scannerrange = typeof(ContrabandDetector).GetField("m_CharactersInRange", anyfield);
    private static float nextswap;
    private static System.Random rnd = new System.Random();

    private static void setflag(FieldInfo f, CharacterStats s, bool v)
    {
        if (f == null)
        {
            return;
        }

        if (f.IsStatic)
        {
            f.SetValue(null, v);
        }
        else if (s != null)
        {
            f.SetValue(s, v);
        }
    }

    private static void setsafety(int level)
    {
        var m = PrisonAlertnessManager.GetInstance();

        if (m == null || setalertness == null)
        {
            return;
        }

        setalertness.Invoke(m, new object[] { (PrisonAlertness)Mathf.Clamp(level, 0, 10) });
    }

    private static void nocooldown()
    {
        var p = local.player();

        if (p != null && recoveryfield != null && recoveryfield.GetValue(p) is float left && left > cooldowncap)
        {
            recoveryfield.SetValue(p, cooldowncap);
        }
    }

    private static void setdesks(float time)
    {
        foreach (var d in desks)
        {
            if (d != null)
            {
                d.m_DeskOpeningTime = time;
            }
        }
    }

    private static void fastdesks()
    {
        if (Time.time >= nextdesksscan)
        {
            nextdesksscan = Time.time + 5f;
            desks.Clear();
            desks.AddRange(UnityEngine.Object.FindObjectsOfType<DeskInteraction>());
        }

        if (Time.time >= nextdesks)
        {
            nextdesks = Time.time + 1f;
            setdesks(0.1f);
        }
    }

    private static void setcams(bool enabled)
    {
        foreach (var c in cams)
        {
            if (c != null)
            {
                c.enabled = enabled;
            }
        }
    }

    private static void camsoff()
    {
        if (Time.time >= nextcamsscan)
        {
            nextcamsscan = Time.time + 5f;
            cams.Clear();
            cams.AddRange(UnityEngine.Object.FindObjectsOfType<CCTVCamera>());
        }

        if (Time.time >= nextcams)
        {
            nextcams = Time.time + 1f;
            setcams(false);
        }
    }

    private static void setscanners(bool enabled)
    {
        foreach (var s in scanners)
        {
            if (s == null)
            {
                continue;
            }

            s.enabled = enabled;

            foreach (var col in s.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = enabled;
            }

            if (!enabled && scannerrange != null && scannerrange.GetValue(s) is IList inrange)
            {
                inrange.Clear();
            }
        }
    }

    private static void scannersoff()
    {
        if (Time.time >= nextscannersscan)
        {
            nextscannersscan = Time.time + 5f;
            scanners.Clear();
            scanners.AddRange(UnityEngine.Object.FindObjectsOfType<ContrabandDetector>());
        }

        if (Time.time >= nextscanners)
        {
            nextscanners = Time.time + 1f;
            setscanners(false);
        }
    }

    private static GUIStyle watermarkstyle;
    private static int framecount;
    private static float frametime;
    private static int fps;

    private static void watermark()
    {
        if (watermarkstyle == null)
        {
            watermarkstyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                padding = new RectOffset(0, 0, 0, 0)
            };
        }

        if (Event.current.type == EventType.Repaint)
        {
            framecount++;
            frametime += Time.unscaledDeltaTime;

            if (frametime >= 0.5f)
            {
                fps = Mathf.RoundToInt(framecount / frametime);
                framecount = 0;
                frametime = 0f;
            }
        }

        GUI.color = new Color(1f, 1f, 1f, 0.7f);
        GUI.Label(new Rect(8f, 6f, 240f, 18f), "frk.cheat v1.0  FPS: " + fps, watermarkstyle);
        GUI.color = Color.white;
    }

    private static void floodtoilets()
    {
        foreach (var t in UnityEngine.Object.FindObjectsOfType<ToiletInteraction>())
        {
            if (t != null)
            {
                t.FloodToilet();
            }
        }
    }

    private static T pick<T>() where T : struct
    {
        var vals = Enum.GetValues(typeof(T));
        T v;

        do
        {
            v = (T)vals.GetValue(rnd.Next(vals.Length));
        }
        while (v.ToString() == "NULL");

        return v;
    }

    private static void swapaccessories()
    {
        if (Time.time < nextswap)
        {
            return;
        }

        nextswap = Time.time + rnd.Next(10, 101) / 1000f;

        var p = local.player();

        if (p == null || p.m_CharacterCustomisation == null)
        {
            return;
        }

        var cc = p.m_CharacterCustomisation;

        cc.SetCustomisationOverrides(new Customisation
        {
            hair = pick<CustomisationData.Hair>(),
            hat = pick<CustomisationData.Hat>(),
            defaultOutfit = cc.m_Outfit
        });

        cc.SetFaceOverrides(pick<CustomisationData.LowerFaceAccessory>(), pick<CustomisationData.UpperFaceAccessory>());
    }

    private static void resetaccessories()
    {
        var p = local.player();

        if (p == null || p.m_CharacterCustomisation == null)
        {
            return;
        }

        p.m_CharacterCustomisation.ClearOverrides();
        p.m_CharacterCustomisation.SetFaceOverrides(CustomisationData.LowerFaceAccessory.NULL, CustomisationData.UpperFaceAccessory.NULL);
    }

    private static void durable()
    {
        var p = local.player();

        if (p == null)
        {
            return;
        }

        var item = p.GetEquippedItem();

        if (item != null && item.m_ItemData != null && item.m_ItemData.m_ItemHealth < 100)
        {
            item.m_ItemData.m_ItemHealth = 100;
        }
    }

    private static feature slider(string name, string tab, float min, float max, float val, Action<float> apply)
    {
        var f = new feature { name = name, tab = tab, slider = true, min = min, max = max, val = val };
        f.run = () => apply(f.val);
        all.Add(f);
        return f;
    }

    public static void init()
    {
        all.Add(new feature
        {
            name = "no clip",
            tab = "Player",
            toggle = true,
            tick = () =>
            {
                var p = local.player();

                if (p != null && !p.m_bCollisionDisabled_Debug)
                {
                    p.m_bCollisionDisabled_Debug = true;
                }
            },
            off = () =>
            {
                var p = local.player();

                if (p != null)
                {
                    p.m_bCollisionDisabled_Debug = false;
                }
            }
        });

        all.Add(new feature
        {
            name = "clear heat",
            tab = "Player",
            run = () =>
            {
                var p = local.player();

                if (p != null && p.m_CharacterStats.Heat > 0f)
                {
                    p.m_CharacterStats.RestoreHeatRPC();
                }
            }
        });

        all.Add(new feature
        {
            name = "infinite health",
            tab = "Player",
            toggle = true,
            tick = () =>
            {
                var p = local.player();
                var s = p != null ? p.m_CharacterStats : null;

                if (s != null)
                {
                    laststats = s;
                }

                setflag(hpflag, s, true);

                if (s != null && s.Health < 100f)
                {
                    s.SetHealth(100f);
                }
            },
            off = () => setflag(hpflag, laststats, false)
        });

        all.Add(new feature
        {
            name = "infinite energy",
            tab = "Player",
            toggle = true,
            tick = () =>
            {
                var p = local.player();
                var s = p != null ? p.m_CharacterStats : null;

                if (s != null)
                {
                    laststats = s;
                }

                setflag(enflag, s, true);

                if (s != null && s.Energy < 100f)
                {
                    s.SetEnergy(100f);
                }
            },
            off = () => setflag(enflag, laststats, false)
        });

        all.Add(new feature
        {
            name = "no attack cooldown",
            tab = "Player",
            toggle = true,
            tick = nocooldown
        });

        all.Add(new feature
        {
            name = "max strength, cardio, intellect",
            tab = "Player",
            run = () =>
            {
                var p = local.player();

                if (p == null)
                {
                    return;
                }

                var s = p.m_CharacterStats;
                s.IncreaseStrengthRPC(100f - s.Strength);
                s.IncreaseCardioRPC(100f - s.Cardio);
                s.IncreaseIntellectRPC(100f - s.Intellect);
            }
        });

        slider("set strength", "Player", 0f, 100f, 100f, playeractions.setstrength);
        slider("set intellect", "Player", 0f, 100f, 100f, playeractions.setintellect);
        slider("set agility", "Player", 0f, 100f, 100f, playeractions.setcardio);
        slider("set reputation", "Player", 0f, 100f, 50f, playeractions.setreputation);

        all.Add(new feature
        {
            name = "follow daily routine",
            tab = "Player",
            toggle = true,
            tick = playeractions.routine,
            off = playeractions.restore
        });

        all.Add(new feature
        {
            name = "auto-complete tasks",
            tab = "Player",
            toggle = true,
            tick = autotask.tick
        });

        var speed = new feature
        {
            name = "player speed",
            tab = "Player",
            toggle = true,
            slider = true,
            min = 1f,
            max = 20f,
            val = 5f
        };

        speed.tick = () =>
        {
            var p = local.player();

            if (p != null)
            {
                playeractions.setspeed(p, speed.val);
            }
        };

        speed.off = playeractions.resetspeed;
        all.Add(speed);

        all.Add(new feature
        {
            name = "win game",
            tab = "Player",
            run = gameactions.win
        });

        all.Add(new feature
        {
            name = "infinite item durability",
            tab = "Items",
            toggle = true,
            tick = durable
        });

        var money = new feature
        {
            name = "set money",
            tab = "Items",
            slider = true,
            min = 0f,
            max = 999f,
            limit = 99999999f,
            val = 100f
        };

        money.run = () =>
        {
            var p = local.player();

            if (p != null)
            {
                p.m_CharacterStats.IncreaseMoney(money.val - p.m_CharacterStats.Money);
            }
        };

        all.Add(money);

        all.Add(new feature
        {
            name = "spam items on floor",
            tab = "Items",
            toggle = true,
            tick = itemgive.spamfloor
        });

        all.Add(new feature
        {
            name = "all keys",
            tab = "World",
            toggle = true,
            tick = doors.tick
        });

        all.Add(new feature
        {
            name = "disable cameras",
            tab = "World",
            toggle = true,
            tick = camsoff,
            off = () => setcams(true)
        });

        all.Add(new feature
        {
            name = "disable contraband scanners",
            tab = "World",
            toggle = true,
            tick = scannersoff,
            off = () => setscanners(true)
        });

        all.Add(new feature
        {
            name = "fast desk opening",
            tab = "World",
            toggle = true,
            tick = fastdesks,
            off = () => setdesks(5f)
        });

        all.Add(new feature
        {
            name = "game speed",
            tab = "World",
            toggle = true,
            slider = true,
            min = 1f,
            max = 10f,
            val = 3f,
            tick = () => Time.timeScale = Mathf.Max(0.1f, features.speed()),
            off = () => Time.timeScale = 1f
        });

        all.Add(new feature
        {
            name = "remove all walls",
            tab = "World",
            run = walls.remove
        });

        all.Add(new feature
        {
            name = "flood all toilets",
            tab = "World",
            run = floodtoilets
        });

        var safety = new feature
        {
            name = "safety level",
            tab = "World",
            slider = true,
            min = 0f,
            max = 10f,
            val = 0f
        };

        safety.run = () => setsafety((int)safety.val);
        all.Add(safety);

        extras.init();

        all.Add(new feature
        {
            name = "chat spam",
            tab = "Fun",
            toggle = true,
            tick = chat.tick
        });

        all.Add(new feature
        {
            name = "swap hair and accessories",
            tab = "Fun",
            toggle = true,
            tick = swapaccessories,
            off = resetaccessories
        });

        all.Add(new feature
        {
            name = "version watermark",
            tab = "Visual",
            toggle = true,
            on = true,
            draw = watermark
        });

        all.Add(new feature
        {
            name = "unlock all achievements",
            tab = "Menu",
            run = achievements.unlockall
        });

        menuvideos.init();
        editortools.init();
    }

    public static float speed()
    {
        var f = all.Find(x => x.name == "game speed");
        return f != null ? f.val : 1f;
    }
}
