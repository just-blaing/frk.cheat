using UnityEngine;

public static class walls
{
    public static void remove()
    {
        int count = 0;

        foreach (var t in UnityEngine.Object.FindObjectsOfType<DamagableTile>())
        {
            if (t == null || Mathf.Approximately(t.Health, 0f))
            {
                continue;
            }

            t.SetHealth(0f);
            var col = t.GetCollider();

            if (col != null)
            {
                col.enabled = false;
            }

            foreach (var r in t.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled = false;
            }

            count++;
        }

        config.status = "walls removed: " + count;
    }
}
