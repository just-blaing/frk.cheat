using System;
using System.IO;
using BepInEx;
using UnityEngine;

[BepInPlugin("frk.cheat", "frk.cheat", "1.0")]
public class plugin : BaseUnityPlugin
{
    private const string startupurl = "https://t.me/frk_cheat";

    private static readonly Array inputs = Enum.GetValues(typeof(Player.PlayerInputs));

    private menu ui = new menu();
    private loader boot = new loader();
    private bool blocked;
    private Player lastme;

    private void Awake()
    {
        features.init();
        inputblock.apply();
    }

    private void Start()
    {
        string marker = Path.Combine(config.dir, "opened.txt");

        if (File.Exists(marker))
        {
            return;
        }

        Directory.CreateDirectory(config.dir);
        File.WriteAllText(marker, "");
        Application.OpenURL(startupurl);
    }

    private static void setplayer(Player me, bool enabled)
    {
        foreach (Player.PlayerInputs input in inputs)
        {
            me.SetInputEnabled(input, enabled);
        }
    }

    private void Update()
    {
        if (!boot.done)
        {
            boot.run(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Insert) && boot.finished)
        {
            ui.open = !ui.open;
        }

        ui.tick();

        bool shouldblock = ui.open || ui.typing;
        inputblock.active = shouldblock;
        var me = local.player();

        if (shouldblock != blocked || (shouldblock && me != lastme))
        {
            if (me != null)
            {
                setplayer(me, !shouldblock);
            }

            blocked = shouldblock;
            lastme = me;
        }

        foreach (var f in features.all)
        {
            if (f.on && f.tick != null)
            {
                f.tick();
            }
        }
    }

    private void OnGUI()
    {
        foreach (var f in features.all)
        {
            if (f.on && f.draw != null)
            {
                f.draw();
            }
        }

        ui.draw();
        boot.draw();
    }
}
