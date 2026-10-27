using System.Reflection;

public static class prisons
{
    private static readonly FieldInfo levels = typeof(LevelDataManager).GetField("m_T17Levels", BindingFlags.Instance | BindingFlags.NonPublic);

    public static PrisonData[] all()
    {
        var ldm = LevelDataManager.GetInstance();
        var found = ldm == null || levels == null ? null : levels.GetValue(ldm) as PrisonData[];
        return found ?? new PrisonData[0];
    }
}
