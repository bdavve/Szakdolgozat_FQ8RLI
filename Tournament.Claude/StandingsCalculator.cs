namespace SportStatsApp;

/// <summary>
/// A bajnoki tabella kiszámítása a nyers meccseredményekből.
/// </summary>
public static class StandingsCalculator
{
    /// <summary>
    /// Meccsek feldolgozása és rendezett tabella előállítása.
    /// Rendezés: pontszám csökkenő → csapatnév ABC növekvő.
    /// </summary>
    public static List<TeamStats> Calculate(IEnumerable<MatchRecord> matches)
    {
        var teams = new Dictionary<string, TeamStats>(StringComparer.OrdinalIgnoreCase);

        foreach (var match in matches)
        {
            var home = GetOrCreate(teams, match.HomeTeam);
            var away = GetOrCreate(teams, match.AwayTeam);

            home.RecordMatch(isHome: true,  match.Result);
            away.RecordMatch(isHome: false, match.Result);
        }

        return teams.Values
            .OrderByDescending(t => t.Points)
            .ThenBy(t => t.TeamName, StringComparer.CurrentCulture)
            .ToList();
    }

    private static TeamStats GetOrCreate(Dictionary<string, TeamStats> dict, string name)
    {
        if (!dict.TryGetValue(name, out var stats))
        {
            stats = new TeamStats(name);
            dict[name] = stats;
        }
        return stats;
    }
}
