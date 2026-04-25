using System;

namespace WarehouseAutomation
{
    // Az irányok definiálása óramutató járásával megegyező sorrendben.
    // Ez fontos a matematikai alapú forgáshoz (modulo aritmetika).
    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public class Robot
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Direction Facing { get; private set; }

        // Konstruktor a kezdőállapot inicializálásához
        public Robot(int startX, int startY, Direction startFacing)
        {
            X = startX;
            Y = startY;
            Facing = startFacing;
        }

        // Parancssorozat feldolgozása
        public void ExecuteCommands(string commands)
        {
            if (string.IsNullOrWhiteSpace(commands)) return;

            foreach (char command in commands.ToUpper())
            {
                switch (command)
                {
                    case 'R':
                        TurnRight();
                        break;
                    case 'L':
                        TurnLeft();
                        break;
                    case 'A':
                        Advance();
                        break;
                    default:
                        Console.WriteLine($"[Figyelmeztetés] Ismeretlen parancs figyelmen kívül hagyva: {command}");
                        break;
                }
            }
        }

        // Jobbra fordulás: (Jelenlegi irány + 1) osztva 4-gyel maradékosan
        private void TurnRight()
        {
            Facing = (Direction)(((int)Facing + 1) % 4);
        }

        // Balra fordulás: (Jelenlegi irány + 3) osztva 4-gyel maradékosan
        // A +3 matematikailag egyenértékű a -1-gyel a modulo 4 rendszerben, de elkerüli a negatív számokat.
        private void TurnLeft()
        {
            Facing = (Direction)(((int)Facing + 3) % 4);
        }

        // Lépés előre az aktuális iránynak megfelelően
        private void Advance()
        {
            switch (Facing)
            {
                case Direction.North: Y++; break;
                case Direction.East: X++; break;
                case Direction.South: Y--; break;
                case Direction.West: X--; break;
            }
        }

        // A robot állapotának szöveges reprezentációja
        public override string ToString()
        {
            string irany = Facing switch
            {
                Direction.North => "Észak",
                Direction.East => "Kelet",
                Direction.South => "Dél",
                Direction.West => "Nyugat",
                _ => "Ismeretlen"
            };
            return $"Pozíció: (X: {X}, Y: {Y}), Irány: {irany}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Raktárautomatizálási Robot Szimulátor ===");

            // Robot inicializálása a (0, 0) pozíción, Észak felé nézve
            Robot robot = new Robot(0, 0, Direction.North);
            Console.WriteLine($"Kezdőállapot -> {robot}");
            Console.WriteLine("---------------------------------------------");

            while (true)
            {
                Console.WriteLine("\nAdjon meg egy parancssorozatot (R = Jobbra, L = Balra, A = Előre)!");
                Console.WriteLine("Vagy írja be: 'exit' a kilépéshez.");
                Console.Write("> ");

                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.Trim().ToLower() == "exit") break;

                // Parancsok futtatása és az új állapot kiírása
                robot.ExecuteCommands(input);
                Console.WriteLine($"Új állapot   -> {robot}");
            }

            Console.WriteLine("Kilépés...");
        }
    }
}