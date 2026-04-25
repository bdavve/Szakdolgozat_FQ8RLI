using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace SportStatsApp
{
    class Program
    {
        // A megadott kapcsolati karakterlánc
        private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SportStats;Trusted_Connection=True;";

        static void Main(string[] args)
        {
            Console.WriteLine("--- Sportstatisztika Rendszer ---");
            Console.Write("Adja meg a szűrni kívánt csapat nevét (vagy hagyja üresen az összeshez): ");
            string filterTeam = Console.ReadLine();

            try
            {
                var matches = LoadMatchesFromDb(filterTeam);
                var standings = ProcessStandings(matches);
                DisplayTable(standings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a feldolgozás során: {ex.Message}");
            }

            Console.WriteLine("\nNyomjon egy gombot a kilépéshez...");
            Console.ReadKey();
        }

        static List<Match> LoadMatchesFromDb(string filter)
        {
            var matches = new List<Match>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                // Dinamikus szűrés: ha üres a filter, minden meccset lekérünk
                string query = "SELECT HomeTeam, AwayTeam, Result FROM Matches";
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query += " WHERE HomeTeam = @Filter OR AwayTeam = @Filter";
                }

                using (var command = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        command.Parameters.AddWithValue("@Filter", filter);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            matches.Add(new Match
                            {
                                HomeTeam = reader["HomeTeam"].ToString(),
                                AwayTeam = reader["AwayTeam"].ToString(),
                                Result = reader["Result"].ToString().ToLower()
                            });
                        }
                    }
                }
            }
            return matches;
        }

        static List<TeamStats> ProcessStandings(List<Match> matches)
        {
            var statsMap = new Dictionary<string, TeamStats>();

            foreach (var match in matches)
            {
                UpdateStats(statsMap, match.HomeTeam, match.Result, isHome: true);
                UpdateStats(statsMap, match.AwayTeam, match.Result, isHome: false);
            }

            // Rendezés: 1. Pontszám (csökkenő), 2. Csapatnév (növekvő)
            return statsMap.Values
                .OrderByDescending(t => t.Points)
                .ThenBy(t => t.TeamName)
                .ToList();
        }

        static void UpdateStats(Dictionary<string, TeamStats> map, string teamName, string result, bool isHome)
        {
            if (!map.ContainsKey(teamName))
                map[teamName] = new TeamStats { TeamName = teamName };

            var s = map[teamName];
            s.Played++;

            if (result == "draw")
            {
                s.Draws++;
            }
            else if ((isHome && result == "win") || (!isHome && result == "loss"))
            {
                s.Wins++;
            }
            else
            {
                s.Losses++;
            }
        }

        static void DisplayTable(List<TeamStats> table)
        {
            Console.WriteLine("\n" + new string('-', 55));
            Console.WriteLine(string.Format("{0,-20} | {1,2} | {2,2} | {3,2} | {4,2} | {5,3}", "Csapat", "M", "Gy", "D", "V", "P"));
            Console.WriteLine(new string('-', 55));

            foreach (var team in table)
            {
                Console.WriteLine(string.Format("{0,-20} | {1,2} | {2,2} | {3,2} | {4,2} | {5,3}",
                    team.TeamName, team.Played, team.Wins, team.Draws, team.Losses, team.Points));
            }
            Console.WriteLine(new string('-', 55));
        }
    }

    public class Match
    {
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string Result { get; set; }
    }

    public class TeamStats
    {
        public string TeamName { get; set; }
        public int Played { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int Points => (Wins * 3) + (Draws * 1);
    }
}