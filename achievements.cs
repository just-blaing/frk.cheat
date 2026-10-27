using System;
using System.Reflection;
using UnityEngine;

public static class achievements
{
    private static readonly FieldInfo trackingfield = typeof(Platform).GetField("m_StatsTracking", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    public static void unlockall()
    {
        var platform = Platform.GetInstance();

        if (platform == null || trackingfield == null)
        {
            config.status = "platform not found";
            return;
        }

        var tracking = trackingfield.GetValue(platform) as StatsTracking;

        if (tracking == null || tracking.m_Tropies == null)
        {
            config.status = "trophy list not found";
            return;
        }

        int done = 0;

        foreach (var trophy in tracking.m_Tropies)
        {
            if (trophy == null || string.IsNullOrEmpty(trophy.m_APIName))
            {
                continue;
            }

            try
            {
                platform.UnlockAchievement(trophy.m_APIName);
                done++;
            }
            catch (Exception)
            {
            }
        }

        config.status = "achievements unlocked: " + done + " of " + tracking.m_Tropies.Length;
    }
}
