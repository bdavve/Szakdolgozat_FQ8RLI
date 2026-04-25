using Microsoft.Data.SqlClient;

namespace SportStatsApp;

/// <summary>
/// A Matches tábla adatait olvassa ki SQL Serverből.
/// </summary>
public static class MatchRepository
{
    /// <summary>
    /// Meccsek lekérdezése – opcionálisan egy csapatnévre szűrve.
    /// </summary>
    public static List<MatchRecord> GetMatches(string connectionString, string? teamFilter)
    {
        var matches = new List<MatchRecord>();

        // Alap lekérdezés
        var sql = "SELECT MatchID, HomeTeam, AwayTeam, Result FROM Matches";

        // Ha van szűrő, akkor csak az adott csapat meccseit kérjük
        if (!string.IsNullOrWhiteSpace(teamFilter))
        {
            sql += " WHERE HomeTeam = @Team OR AwayTeam = @Team";
        }

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = new SqlCommand(sql, connection);

        if (!string.IsNullOrWhiteSpace(teamFilter))
        {
            command.Parameters.AddWithValue("@Team", teamFilter);
        }

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            matches.Add(new MatchRecord(
                MatchId:  reader.GetInt32(0),
                HomeTeam: reader.GetString(1),
                AwayTeam: reader.GetString(2),
                Result:   reader.GetString(3)
            ));
        }

        return matches;
    }
}

/// <summary>
/// Egy meccs nyers adatait reprezentáló rekord.
/// </summary>
public readonly record struct MatchRecord(
    int    MatchId,
    string HomeTeam,
    string AwayTeam,
    string Result
);
