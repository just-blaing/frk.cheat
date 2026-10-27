using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public static class editortools
{
    private const BindingFlags anyinst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const int width = 120;
    private const int cellcount = 14400;
    private const int blockedflags = 0x8 | 0x800 | 0x1000 | 0x2000 | 0x4000;
    private static readonly System.Random rnd = new System.Random();
    private static bool zonereqs;
    private static bool zoneoverlap;
    private static bool brushitems;
    private static bool checklistdone;
    private static bool patched;
    private static feature idfeature;

    private static object fieldval(object o, string name)
    {
        if (o == null)
        {
            return null;
        }

        for (var t = o.GetType(); t != null; t = t.BaseType)
        {
            var f = t.GetField(name, anyinst | BindingFlags.Static);

            if (f != null)
            {
                return f.GetValue(f.IsStatic ? null : o);
            }
        }

        return null;
    }

    private static void setfield(object o, string name, object value)
    {
        for (var t = o.GetType(); t != null; t = t.BaseType)
        {
            var f = t.GetField(name, anyinst);

            if (f != null)
            {
                f.SetValue(o, value);
                return;
            }
        }
    }

    public static void zoneafter(LevelEditor_ZoneManager __instance, int iID)
    {
        if (!zonereqs)
        {
            return;
        }

        var zones = fieldval(__instance, "m_Zones") as Array;

        if (zones == null || iID < 0 || iID >= zones.Length)
        {
            return;
        }

        var zone = zones.GetValue(iID);

        if (zone == null)
        {
            return;
        }

        var required = fieldval(zone, "m_Required") as IList;

        if (required != null)
        {
            required.Clear();
        }

        setfield(zone, "m_bValid", true);
    }

    public static void zonearea(ref bool __result, ref byte[] zonePrint)
    {
        if (zoneoverlap && zonePrint != null)
        {
            __result = true;
        }
    }

    public static void brushafter(LevelEditorBrushElement __instance, ref bool __result)
    {
        if (!brushitems)
        {
            return;
        }

        var f = typeof(LevelEditorBrushElement).GetField("m_CurrentError", anyinst);

        if (f != null)
        {
            int cur = Convert.ToInt32(f.GetValue(__instance));
            f.SetValue(__instance, Enum.ToObject(f.FieldType, cur & ~blockedflags));
        }

        setfield(__instance, "m_bValid", true);
        __result = true;
    }

    private static float nextchecklist;

    private static void refreshchecklists()
    {
        foreach (var list in UnityEngine.Object.FindObjectsOfType<LevelEditor_CheckList>())
        {
            if (list != null)
            {
                list.UpdateCheckList();
            }
        }
    }

    private static void checklisttick()
    {
        checklistdone = true;

        if (Time.unscaledTime < nextchecklist)
        {
            return;
        }

        nextchecklist = Time.unscaledTime + 0.1f;
        refreshchecklists();
    }

    private static void checklistoff()
    {
        checklistdone = false;
        nextchecklist = 0f;
        refreshchecklists();
    }

    public static void metafter(ref bool __result)
    {
        if (checklistdone)
        {
            __result = true;
        }
    }

    private static void settext(object text, string value)
    {
        var set = text.GetType().GetMethods(anyinst).FirstOrDefault(x => x.Name == "SetNonLocalizedText" && x.GetParameters().Length >= 1 && x.GetParameters()[0].ParameterType == typeof(string));

        if (set == null)
        {
            return;
        }

        var ps = set.GetParameters();
        var args = new object[ps.Length];
        args[0] = value;

        for (int i = 1; i < ps.Length; i++)
        {
            args[i] = ps[i].DefaultValue ?? (ps[i].ParameterType.IsValueType ? Activator.CreateInstance(ps[i].ParameterType) : null);
        }

        set.Invoke(text, args);
    }

    public static void entryafter(LevelEditor_CheckList_Entry __instance, ref bool __result)
    {
        if (!checklistdone)
        {
            return;
        }

        try
        {
            int id = Convert.ToInt32(fieldval(__instance, "m_LimitationID"));
            var group = BuildingBlockManager.GetLimitationGroup(id);
            var text = fieldval(__instance, "m_CountText");

            if (group != null && text != null)
            {
                settext(text, group.m_Min + "/" + group.m_Min);
            }

            setfield(__instance, "m_bOverLimit", false);
            __result = true;
        }
        catch (Exception)
        {
        }
    }

    private static void patchall()
    {
        if (patched)
        {
            return;
        }

        patched = true;

        try
        {
            var h = new Harmony("frk.cheat.editor");
            h.Patch(typeof(LevelEditor_ZoneManager).GetMethod("ValidateZone", anyinst), null, new HarmonyMethod(typeof(editortools).GetMethod("zoneafter")));
            h.Patch(typeof(LevelEditor_Controller).GetMethod("IsZoneAreaValid", anyinst), null, new HarmonyMethod(typeof(editortools).GetMethod("zonearea")));
            h.Patch(typeof(LevelEditorBrushElement).GetMethod("ValidateElement", anyinst), null, new HarmonyMethod(typeof(editortools).GetMethod("brushafter")));
            h.Patch(typeof(BuildingBlockManager.LimitationGroup).GetMethod("HasMetRequirements", anyinst), null, new HarmonyMethod(typeof(editortools).GetMethod("metafter")));
            h.Patch(typeof(LevelEditor_CheckList_Entry).GetMethod("UpdateState", anyinst), null, new HarmonyMethod(typeof(editortools).GetMethod("entryafter")));
        }
        catch (Exception e)
        {
            config.status = "editor patches failed: " + e.GetType().Name;
        }
    }

    private static bool hastile(object tileproperty)
    {
        var p = (BaseLevelManager.TileProperty)tileproperty;
        return (p & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask;
    }

    private static MethodInfo method(object lm, string name)
    {
        return lm.GetType().GetMethod(name, anyinst);
    }

    private static bool drop(object lm, MethodInfo mtile, MethodInfo mwall, MethodInfo mobj, int x, int y, BaseBuildingBlock block)
    {
        var tile = block as BuildingBlock_Tile;

        if (tile != null)
        {
            mtile.Invoke(lm, new object[] { x, y, tile, rnd.Next(), true, false });
            return true;
        }

        var wall = block as BuildingBlock_Wall;

        if (wall != null)
        {
            mwall.Invoke(lm, new object[] { x, y, wall, rnd.Next(), true, false });
            return true;
        }

        var obj = block as BuildingBlock_Object;

        if (obj != null)
        {
            mobj.Invoke(lm, new object[] { x, y, obj, true });
            return true;
        }

        return false;
    }

    private static void replace(bool onlyselected)
    {
        try
        {
            var ctrl = UnityEngine.Object.FindObjectOfType<LevelEditor_Controller>();

            if (ctrl == null)
            {
                config.status = "open the level editor first";
                return;
            }

            var lm = fieldval(ctrl, "m_LevelManager");
            var layers = fieldval(lm, "m_BuildingLayers") as Array;

            if (layers == null)
            {
                config.status = "level layers not found";
                return;
            }

            int layerindex = Convert.ToInt32(fieldval(lm, "m_CurrentLayer"));
            var layer = layers.GetValue(layerindex);
            var props = fieldval(layer, "m_TileProperties") as Array;
            var floors = fieldval(layer, "m_TileTileIDs") as Array;
            var walls = fieldval(layer, "m_WallTileIDs") as Array;
            var objects = fieldval(layer, "m_ObjectTileIDs") as Array;
            var tiles = UnityEngine.Object.FindObjectsOfType<BuildingBlock_Tile>();
            var wallblocks = UnityEngine.Object.FindObjectsOfType<BuildingBlock_Wall>();
            var objblocks = UnityEngine.Object.FindObjectsOfType<BuildingBlock_Object>();
            var mtile = method(lm, "PlaceTile");
            var mwall = method(lm, "PlaceWall");
            var mobj = method(lm, "PlaceObject");
            var mrm = method(lm, "RemoveObject");
            var update = method(lm, "UpdateTiles");

            if (mtile == null || mwall == null || mobj == null || mrm == null || update == null)
            {
                config.status = "placement methods not found";
                return;
            }

            int[] indices;

            if (onlyselected)
            {
                int sx = Convert.ToInt32(fieldval(ctrl, "m_LastSelected_X_Position"));
                int sy = Convert.ToInt32(fieldval(ctrl, "m_LastSelected_Y_Position"));
                indices = new[] { sy * width + sx };
            }
            else
            {
                indices = Enumerable.Range(0, cellcount).ToArray();
            }

            int changed = 0;

            foreach (int i in indices)
            {
                if (i < 0 || i >= cellcount || props == null || !hastile(props.GetValue(i)))
                {
                    continue;
                }

                int x = i % width;
                int y = i / width;

                if (floors != null && tiles.Length > 0)
                {
                    var block = BuildingBlockManager.GetBlock((BaseLevelManager.TileIDData)floors.GetValue(i));

                    if (block is BuildingBlock_Tile && drop(lm, mtile, mwall, mobj, x, y, tiles[rnd.Next(tiles.Length)]))
                    {
                        changed++;
                    }
                }

                if (walls != null && wallblocks.Length > 0)
                {
                    var block = BuildingBlockManager.GetBlock((BaseLevelManager.TileIDData)walls.GetValue(i));

                    if (block is BuildingBlock_Wall && drop(lm, mtile, mwall, mobj, x, y, wallblocks[rnd.Next(wallblocks.Length)]))
                    {
                        changed++;
                    }
                }

                if (objects != null && objblocks.Length > 0)
                {
                    var old = BuildingBlockManager.GetBlock((BaseLevelManager.TileIDData)objects.GetValue(i)) as BuildingBlock_Object;

                    if (old != null)
                    {
                        mrm.Invoke(lm, new object[] { x, y, old, true });

                        if (drop(lm, mtile, mwall, mobj, x, y, objblocks[rnd.Next(objblocks.Length)]))
                        {
                            changed++;
                        }
                    }
                }
            }

            update.Invoke(lm, null);
            config.status = "blocks replaced: " + changed;
        }
        catch (Exception e)
        {
            config.status = "replace failed: " + e.GetType().Name;
        }
    }

    private static void replacebyid(int id)
    {
        try
        {
            var ctrl = UnityEngine.Object.FindObjectOfType<LevelEditor_Controller>();

            if (ctrl == null)
            {
                config.status = "open the level editor first";
                return;
            }

            var target = BuildingBlockManager.GetBlock(id);

            if (target == null)
            {
                config.status = "no block with id " + id + " in the catalog";
                return;
            }

            var lm = fieldval(ctrl, "m_LevelManager");
            var layers = fieldval(lm, "m_BuildingLayers") as Array;
            int layerindex = Convert.ToInt32(fieldval(lm, "m_CurrentLayer"));
            var layer = layers.GetValue(layerindex);
            var objects = fieldval(layer, "m_ObjectTileIDs") as Array;
            int sx = Convert.ToInt32(fieldval(ctrl, "m_LastSelected_X_Position"));
            int sy = Convert.ToInt32(fieldval(ctrl, "m_LastSelected_Y_Position"));
            int i = sy * width + sx;
            var mtile = method(lm, "PlaceTile");
            var mwall = method(lm, "PlaceWall");
            var mobj = method(lm, "PlaceObject");
            var mrm = method(lm, "RemoveObject");
            var update = method(lm, "UpdateTiles");

            if (target is BuildingBlock_Object && objects != null)
            {
                var old = BuildingBlockManager.GetBlock((BaseLevelManager.TileIDData)objects.GetValue(i)) as BuildingBlock_Object;

                if (old != null)
                {
                    mrm.Invoke(lm, new object[] { sx, sy, old, true });
                }
            }

            if (!drop(lm, mtile, mwall, mobj, sx, sy, target))
            {
                config.status = "this block type cannot be replaced";
                return;
            }

            update.Invoke(lm, null);
            config.status = "block replaced with id " + id;
        }
        catch (Exception e)
        {
            config.status = "replace by id failed: " + e.GetType().Name;
        }
    }

    public static void init()
    {
        patchall();

        features.all.Add(new feature
        {
            name = "meet zone requirements",
            tab = "Editor",
            toggle = true,
            tick = () => { zonereqs = true; },
            off = () => { zonereqs = false; }
        });

        features.all.Add(new feature
        {
            name = "zones on zones",
            tab = "Editor",
            toggle = true,
            tick = () => { zoneoverlap = true; },
            off = () => { zoneoverlap = false; }
        });

        features.all.Add(new feature
        {
            name = "items on items",
            tab = "Editor",
            toggle = true,
            tick = () => { brushitems = true; },
            off = () => { brushitems = false; }
        });

        features.all.Add(new feature
        {
            name = "random replace layer blocks",
            tab = "Editor",
            run = () => replace(false)
        });

        features.all.Add(new feature
        {
            name = "replace selected block randomly",
            tab = "Editor",
            run = () => replace(true)
        });

        features.all.Add(new feature
        {
            name = "run checklist",
            tab = "Editor",
            toggle = true,
            tick = checklisttick,
            off = checklistoff
        });

        idfeature = new feature
        {
            name = "block id to replace",
            tab = "Editor",
            slider = true,
            min = 0,
            max = 950,
            val = 0
        };
        features.all.Add(idfeature);

        features.all.Add(new feature
        {
            name = "replace selected block by id",
            tab = "Editor",
            run = () => replacebyid((int)idfeature.val)
        });
    }
}
