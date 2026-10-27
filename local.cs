public static class local
{
    public static Player player()
    {
        foreach (var p in Player.GetAllPlayers())
        {
            if (p != null && p.m_Gamer != null && p.m_Gamer.IsLocal() && !p.GetIsDisabled())
            {
                return p;
            }
        }

        return null;
    }
}
