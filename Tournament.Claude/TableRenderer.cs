using System.Text;

namespace SportStatsApp;

/// <summary>
/// Formázott konzol-tabella kiírása.
/// </summary>
public static class TableRenderer
{
    /// <summary>
    /// A bajnoki tabella kirajzolása a konzolra.
    /// </summary>
    public static void Render(IReadOnlyList<TeamStats> standings)
    {
        if (standings.Count == 0)
        {
            Console.WriteLine("  Nincs megjeleníthető adat.");
            return;
        }

        // Oszlopszélességek kiszámítása
        int nameWidth = Math.Max("Csapat".Length,
            standings.Max(t => t.TeamName.Length));

        const int numWidth = 4; // M, Gy, D, V, Pont oszlopok minimális szélessége

        // Fejléc
        string header = FormatRow("#", nameWidth, "Csapat", numWidth, "M", "Gy", "D", "V", "Pont");
        string separator = new('─', header.Length);

        Console.WriteLine();
        Console.WriteLine(separator);
        Console.WriteLine(header);
        Console.WriteLine(separator);

        // Sorok
        for (int i = 0; i < standings.Count; i++)
        {
            var t = standings[i];
            Console.WriteLine(FormatRow(
                (i + 1).ToString(),
                nameWidth,
                t.TeamName,
                numWidth,
                t.Played.ToString(),
                t.Wins.ToString(),
                t.Draws.ToString(),
                t.Losses.ToString(),
                t.Points.ToString()
            ));
        }

        Console.WriteLine(separator);
        Console.WriteLine($"  Összesen {standings.Count} csapat");
        Console.WriteLine();
    }

    private static string FormatRow(
        string rank, int nameWidth, string name, int nw,
        string m, string gy, string d, string v, string pont)
    {
        return $" {rank,3}  {name.PadRight(nameWidth)}  " +
               $"{m.PadLeft(nw)}  {gy.PadLeft(nw)}  {d.PadLeft(nw)}  {v.PadLeft(nw)}  {pont.PadLeft(nw)}";
    }
}