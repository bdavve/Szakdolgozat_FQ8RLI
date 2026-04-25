using System;
using Microsoft.Data.SqlClient;

class Test
{
    static void Main()
    {
        string userInput = Console.ReadLine();
        string connStr = "Server=localhost;Database=Test;User Id=sa;Password=P@ssw0rd;";

        using var conn = new SqlConnection(connStr);
        conn.Open();

        // Use a parameterized query to avoid SQL injection
        string query = "SELECT * FROM Users WHERE Name = @name";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@name", userInput ?? string.Empty);

        using var reader = cmd.ExecuteReader();
        // ... process reader as needed
    }
}