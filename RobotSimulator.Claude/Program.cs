namespace WarehouseRobot;

/// <summary>
/// A robot négy lehetséges nézési iránya.
/// Az értékek az óramutató járásával megegyezően követik egymást,
/// így a forgatás egyszerű aritmetikával megoldható.
/// </summary>
public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

/// <summary>
/// Raktári robot, amely egy végtelen rácson mozog
/// és R/L/A parancsokat hajt végre.
/// </summary>
public sealed class Robot
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Direction Facing { get; private set; }

    public Robot(int x, int y, Direction facing)
    {
        X = x;
        Y = y;
        Facing = facing;
    }

    /// <summary>Jobbra fordulás (90° az óramutató járása szerint).</summary>
    public void TurnRight() =>
        Facing = (Direction)(((int)Facing + 1) % 4);

    /// <summary>Balra fordulás (90° az óramutató járása ellen).</summary>
    public void TurnLeft() =>
        Facing = (Direction)(((int)Facing + 3) % 4);

    /// <summary>Egy mező előrelépés a jelenlegi nézési irányba.</summary>
    public void Advance()
    {
        switch (Facing)
        {
            case Direction.North: Y++; break;
            case Direction.East: X++; break;
            case Direction.South: Y--; break;
            case Direction.West: X--; break;
        }
    }

    /// <summary>
    /// Parancssorozat végrehajtása.
    /// Érvényes karakterek: R, L, A (kis- és nagybetű egyaránt).
    /// Ismeretlen karakter esetén kivételt dob.
    /// </summary>
    public void Execute(string commands)
    {
        foreach (char c in commands.ToUpperInvariant())
        {
            switch (c)
            {
                case 'R': TurnRight(); break;
                case 'L': TurnLeft(); break;
                case 'A': Advance(); break;
                default:
                    throw new ArgumentException(
                        $"Ismeretlen parancs: '{c}'. Használj R, L vagy A karaktereket.");
            }
        }
    }

    /// <summary>Az irány magyar nevét adja vissza.</summary>
    private static string DirectionName(Direction d) => d switch
    {
        Direction.North => "Észak",
        Direction.East => "Kelet",
        Direction.South => "Dél",
        Direction.West => "Nyugat",
        _ => d.ToString()
    };

    public override string ToString() =>
        $"Pozíció: ({X}, {Y})  |  Irány: {DirectionName(Facing)}";
}

/// <summary>Konzolos belépési pont és felhasználói interakció.</summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║     Raktári Robot Szimulátor  v1.0      ║");
        Console.WriteLine("╠══════════════════════════════════════════╣");
        Console.WriteLine("║  Parancsok:  R = jobbra  L = balra      ║");
        Console.WriteLine("║             A = előre                   ║");
        Console.WriteLine("║  Kilépés:   üres sor vagy 'q'           ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();

        // --- Kezdőpozíció bekérése ---
        int startX = ReadInt("Kezdő X koordináta", 0);
        int startY = ReadInt("Kezdő Y koordináta", 0);
        Direction startDir = ReadDirection("Kezdő irány (N/E/S/W)", Direction.North);

        var robot = new Robot(startX, startY, startDir);

        Console.WriteLine();
        Console.WriteLine($"  Robot elhelyezve → {robot}");
        Console.WriteLine(new string('─', 44));

        // --- Parancs-ciklus ---
        while (true)
        {
            Console.WriteLine();
            Console.Write("Parancssorozat> ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\nRobot leállítva. Végső állapot:");
                Console.WriteLine($"  {robot}");
                break;
            }

            try
            {
                robot.Execute(input);
                Console.WriteLine($"  ✔ Végrehajtva → {robot}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"  ✘ Hiba: {ex.Message}");
            }
        }
    }

    // --- Segédfüggvények a bemenet bekéréséhez ---

    private static int ReadInt(string prompt, int defaultValue)
    {
        Console.Write($"{prompt} [{defaultValue}]: ");
        string? line = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(line)) return defaultValue;
        if (int.TryParse(line, out int value)) return value;
        Console.WriteLine($"  (Érvénytelen szám, alapértelmezés: {defaultValue})");
        return defaultValue;
    }

    private static Direction ReadDirection(string prompt, Direction defaultValue)
    {
        Console.Write($"{prompt} [{DirectionChar(defaultValue)}]: ");
        string? line = Console.ReadLine()?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(line)) return defaultValue;

        return line switch
        {
            "N" => Direction.North,
            "E" => Direction.East,
            "S" => Direction.South,
            "W" => Direction.West,
            _ => LogAndDefault(defaultValue)
        };

        static Direction LogAndDefault(Direction d)
        {
            Console.WriteLine($"  (Érvénytelen irány, alapértelmezés: {DirectionChar(d)})");
            return d;
        }
    }

    private static char DirectionChar(Direction d) => d switch
    {
        Direction.North => 'N',
        Direction.East => 'E',
        Direction.South => 'S',
        Direction.West => 'W',
        _ => '?'
    };
}