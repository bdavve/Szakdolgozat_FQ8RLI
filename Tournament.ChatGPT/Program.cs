using Microsoft.Data.SqlClient;

const string ConnectionString =
    "Server=(localdb)\\MSSQLLocalDB;Database=SportStats;Trusted_Connection=True;";

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.Write("Adj meg egy csapatnevet a szűréshez, vagy hagyd üresen az összes meccshez: ");
string? teamFilter = Console.ReadLine();

try
{
    List<Match> matches = await LoadMatchesAsync(teamFilter);

    if (matches.Count == 0)
    {
        Console.WriteLine();
        Console.WriteLine("Nincs feldolgozható meccs a megadott feltételek alapján.");
        return;
    }

    List<TeamStats> table = BuildLeagueTable(matches);

    PrintTable(table);
}
catch (SqlException ex)
{
    Console.WriteLine();
    Console.WriteLine("Adatbázis-hiba történt:");
    Console.WriteLine(ex.Message);
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("Váratlan hiba történt:");
    Console.WriteLine(ex.Message);
}

static async Task<List<Match>> LoadMatchesAsync(string? teamFilter)
{
    var matches = new List<Match>();

    bool hasFilter = !string.IsNullOrWhiteSpace(teamFilter);

    string sql = """
        SELECT MatchID, HomeTeam, AwayTeam, Result
        FROM Matches
        WHERE
            @TeamFilter IS NULL
            OR HomeTeam = @TeamFilter
            OR AwayTeam = @TeamFilter
        """;

    await using var connection = new SqlConnection(ConnectionString);
    await connection.OpenAsync();

    await using var command = new SqlCommand(sql, connection);
    command.Parameters.AddWithValue("@TeamFilter", hasFilter ? teamFilter!.Trim() : DBNull.Value);

    await using SqlDataReader reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        matches.Add(new Match
        {
            MatchId = reader.GetInt32(reader.GetOrdinal("MatchID")),
            HomeTeam = reader.GetString(reader.GetOrdinal("HomeTeam")),
            AwayTeam = reader.GetString(reader.GetOrdinal("AwayTeam")),
            Result = reader.GetString(reader.GetOrdinal("Result"))
        });
    }

    return matches;
}

static List<TeamStats> BuildLeagueTable(List<Match> matches)
{
    var statsByTeam = new Dictionary<string, TeamStats>(StringComparer.OrdinalIgnoreCase);

    foreach (Match match in matches)
    {
        TeamStats homeTeam = GetOrCreateTeam(statsByTeam, match.HomeTeam);
        TeamStats awayTeam = GetOrCreateTeam(statsByTeam, match.AwayTeam);

        homeTeam.MatchesPlayed++;
        awayTeam.MatchesPlayed++;

        switch (match.Result.Trim().ToLowerInvariant())
        {
            case "win":
                homeTeam.Wins++;
                awayTeam.Losses++;
                homeTeam.Points += 3;
                break;

            case "loss":
                homeTeam.Losses++;
                awayTeam.Wins++;
                awayTeam.Points += 3;
                break;

            case "draw":
                homeTeam.Draws++;
                awayTeam.Draws++;
                homeTeam.Points += 1;
                awayTeam.Points += 1;
                break;

            default:
                Console.WriteLine($"Figyelmeztetés: ismeretlen eredmény a(z) {match.MatchId}. meccsnél: {match.Result}");
                homeTeam.MatchesPlayed--;
                awayTeam.MatchesPlayed--;
                break;
        }
    }

    return statsByTeam.Values
        .OrderByDescending(team => team.Points)
        .ThenBy(team => team.TeamName, StringComparer.CurrentCultureIgnoreCase)
        .ToList();
}

static TeamStats GetOrCreateTeam(Dictionary<string, TeamStats> statsByTeam, string teamName)
{
    if (!statsByTeam.TryGetValue(teamName, out TeamStats? stats))
    {
        stats = new TeamStats
        {
            TeamName = teamName
        };

        statsByTeam[teamName] = stats;
    }

    return stats;
}

static void PrintTable(List<TeamStats> table)
{
    Console.WriteLine();
    Console.WriteLine("BAJNOKI TABELLA");
    Console.WriteLine(new string('-', 72));

    Console.WriteLine(
        "{0,-4} {1,-30} {2,4} {3,4} {4,4} {5,4} {6,6}",
        "H.",
        "Csapat",
        "M",
        "Gy",
        "D",
        "V",
        "Pont"
    );

    Console.WriteLine(new string('-', 72));

    int position = 1;

    foreach (TeamStats team in table)
    {
        Console.WriteLine(
            "{0,-4} {1,-30} {2,4} {3,4} {4,4} {5,4} {6,6}",
            position,
            team.TeamName,
            team.MatchesPlayed,
            team.Wins,
            team.Draws,
            team.Losses,
            team.Points
        );

        position++;
    }

    Console.WriteLine(new string('-', 72));
}

sealed class Match
{
    public int MatchId { get; set; }

    public required string HomeTeam { get; set; }

    public required string AwayTeam { get; set; }

    /// <summary>
    /// win  = hazai győzelem
    /// loss = vendég győzelem
    /// draw = döntetlen
    /// </summary>
    public required string Result { get; set; }
}

sealed class TeamStats
{
    public required string TeamName { get; set; }

    public int MatchesPlayed { get; set; }

    public int Wins { get; set; }

    public int Draws { get; set; }

    public int Losses { get; set; }

    public int Points { get; set; }
}