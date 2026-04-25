using SportStatsApp;

// ──────────────────────────────────────────────
//  SportStats – Bajnoki tabella generátor
// ──────────────────────────────────────────────

const string ConnectionString =
    "Server=(localdb)\\MSSQLLocalDB;Database=SportStats;Trusted_Connection=True;";

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║     SportStats – Bajnoki tabella         ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

// ── 1. Opcionális csapatnév-szűrő bekérése ──
Console.Write("Csapatnév szűrő (ENTER = összes): ");
string? teamFilter = Console.ReadLine()?.Trim();

if (string.IsNullOrEmpty(teamFilter))
{
    teamFilter = null;
    Console.WriteLine("» Összes meccs feldolgozása...");
}
else
{
    Console.WriteLine($"» Szűrés: \"{teamFilter}\"");
}

Console.WriteLine();

try
{
    // ── 2. Meccsek lekérdezése az adatbázisból ──
    var matches = MatchRepository.GetMatches(ConnectionString, teamFilter);

    if (matches.Count == 0)
    {
        Console.WriteLine("Nem található meccs a megadott feltételekkel.");
        return;
    }

    Console.WriteLine($"Lekérdezve: {matches.Count} meccs.");

    // ── 3. Tabella kiszámítása ──
    var standings = StandingsCalculator.Calculate(matches);

    // ── 4. Eredmény kiírása ──
    TableRenderer.Render(standings);
}
catch (Microsoft.Data.SqlClient.SqlException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Adatbázis hiba: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Váratlan hiba: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}