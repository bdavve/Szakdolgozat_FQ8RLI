using System;
using System.Linq;

namespace WarehouseRobotSimulation
{
    internal enum Direction
    {
        North,
        East,
        South,
        West
    }

    internal class Robot
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Direction Facing { get; private set; }

        public Robot(int startX, int startY, Direction startDirection)
        {
            X = startX;
            Y = startY;
            Facing = startDirection;
        }

        public void ExecuteCommands(string commands)
        {
            foreach (char command in commands)
            {
                switch (char.ToUpperInvariant(command))
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
                        throw new ArgumentException($"Érvénytelen parancs: '{command}'. Csak R, L, A engedélyezett.");
                }
            }
        }

        private void TurnRight()
        {
            Facing = Facing switch
            {
                Direction.North => Direction.East,
                Direction.East => Direction.South,
                Direction.South => Direction.West,
                Direction.West => Direction.North,
                _ => throw new InvalidOperationException("Ismeretlen irány.")
            };
        }

        private void TurnLeft()
        {
            Facing = Facing switch
            {
                Direction.North => Direction.West,
                Direction.West => Direction.South,
                Direction.South => Direction.East,
                Direction.East => Direction.North,
                _ => throw new InvalidOperationException("Ismeretlen irány.")
            };
        }

        private void Advance()
        {
            switch (Facing)
            {
                case Direction.North:
                    Y++;
                    break;
                case Direction.East:
                    X++;
                    break;
                case Direction.South:
                    Y--;
                    break;
                case Direction.West:
                    X--;
                    break;
                default:
                    throw new InvalidOperationException("Ismeretlen irány.");
            }
        }

        public override string ToString()
        {
            return $"Pozíció: ({X}, {Y}), Irány: {DirectionToHungarian(Facing)}";
        }

        private static string DirectionToHungarian(Direction direction)
        {
            return direction switch
            {
                Direction.North => "Észak",
                Direction.East => "Kelet",
                Direction.South => "Dél",
                Direction.West => "Nyugat",
                _ => "Ismeretlen"
            };
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Raktári robot szimuláció");
            Console.WriteLine("Engedélyezett parancsok: R = jobbra, L = balra, A = előre");
            Console.WriteLine();

            int startX = ReadInt("Add meg a kezdő X koordinátát: ");
            int startY = ReadInt("Add meg a kezdő Y koordinátát: ");
            Direction startDirection = ReadDirection("Add meg a kezdő irányt (észak/kelet/dél/nyugat): ");

            Robot robot = new Robot(startX, startY, startDirection);

            Console.Write("Add meg a parancssorozatot: ");
            string? commands = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(commands))
            {
                Console.WriteLine("Nem adtál meg parancssorozatot.");
                return;
            }

            if (!commands.All(c => "RLA".Contains(char.ToUpperInvariant(c))))
            {
                Console.WriteLine("Hiba: a parancssorozat csak R, L és A karaktereket tartalmazhat.");
                return;
            }

            try
            {
                robot.ExecuteCommands(commands);
                Console.WriteLine();
                Console.WriteLine("A robot végállapota:");
                Console.WriteLine(robot);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt: {ex.Message}");
            }
        }

        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value))
                {
                    return value;
                }

                Console.WriteLine("Érvénytelen szám. Próbáld újra.");
            }
        }

        private static Direction ReadDirection(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim().ToLowerInvariant();

                switch (input)
                {
                    case "észak":
                    case "eszak":
                    case "north":
                    case "n":
                        return Direction.North;

                    case "kelet":
                    case "east":
                    case "e":
                        return Direction.East;

                    case "dél":
                    case "del":
                    case "south":
                    case "s":
                        return Direction.South;

                    case "nyugat":
                    case "west":
                    case "w":
                        return Direction.West;

                    default:
                        Console.WriteLine("Érvénytelen irány. Használható értékek: észak, kelet, dél, nyugat.");
                        break;
                }
            }
        }
    }
}
