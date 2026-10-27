using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class autotask
{
    private static FieldInfo statusfield = typeof(BaseObjective).GetField("m_ObjectiveStatus", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    private static float next;

    public static void finish()
    {
        var p = local.player();
        var om = ObjectiveManager.GetInstance();

        if (p == null || om == null || statusfield == null)
        {
            return;
        }

        List<ObjectiveTree> trees;
        om.GetActiveTrees(p, out trees);

        if (trees == null)
        {
            return;
        }

        foreach (var tree in new List<ObjectiveTree>(trees))
        {
            if (tree == null || tree.MainBranch == null)
            {
                continue;
            }

            for (int i = 0; i < 500; i++)
            {
                var goal = tree.MainBranch.GetCurrentGoal();

                if (goal == null || goal.m_Objective == null)
                {
                    break;
                }

                statusfield.SetValue(goal.m_Objective, ObjectiveStatus.Done);

                if (tree.EvaluateCurrentGoal())
                {
                    break;
                }
            }
        }
    }

    public static void tick()
    {
        if (Time.time < next)
        {
            return;
        }

        next = Time.time + 1f;
        finish();
    }
}
