using UnityEngine;

public static class chat
{
    public static string text = "frk.cheat";
    private static float next;

    public static void tick()
    {
        if (Time.time < next)
        {
            return;
        }

        var me = local.player();
        var cf = ChatFeedManager.GetInstance();

        if (me == null || me.m_Gamer == null || cf == null || string.IsNullOrEmpty(text))
        {
            return;
        }

        next = Time.time + 0.5f;
        cf.SendChatMessage_RPC(me.m_Gamer, text, ChatFeedManager.MessageTag.PlayerMsg, false);
    }

    public static void draw()
    {
        GUILayout.Label("spam text:");
        text = GUILayout.TextField(text ?? "", 100);
    }
}
