namespace SportStatsApp;

/// <summary>
/// Egy csapat összesített bajnoki statisztikáit tároló modell.
/// </summary>
public sealed class TeamStats
{
    public string TeamName { get; }
    public int Played   { get; private set; }  // M  – lejátszott meccsek
    public int Wins     { get; private set; }  // Gy – győzelmek
    public int Draws    { get; private set; }  // D  – döntetlenek
    public int Losses   { get; private set; }  // V  – vereségek
    public int Points   => Wins * 3 + Draws;   // Pont

    public TeamStats(string teamName) =>
        TeamName = teamName ?? throw new ArgumentNullException(nameof(teamName));

    /// <summary>
    /// Egy meccs eredményének rögzítése a csapat szemszögéből.
    /// </summary>
    /// <param name="isHome">A csapat hazai volt-e?</param>
    /// <param name="result">Az eredmény a *hazai* csapat szemszögéből: win / loss / draw.</param>
    public void RecordMatch(bool isHome, string result)
    {
        Played++;

        switch (result.ToLowerInvariant())
        {
            case "win":
                if (isHome) Wins++; else Losses++;
                break;
            case "loss":
                if (isHome) Losses++; else Wins++;
                break;
            case "draw":
                Draws++;
                break;
            default:
                throw new ArgumentException($"Ismeretlen eredmény: '{result}'", nameof(result));
        }
    }
}
