using Microsoft.Data.Sqlite;

namespace Web.Services;

public static class SqlService
{
    public static void UpdateSchema(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is missing or invalid");
        }
        var conn = new SqliteConnection(connectionString);
        conn.Open(); // Open long-running connection to save memory and io calls

        var userVersion = GetUserVersion(conn);
        Console.WriteLine($"Database Version: {userVersion}");
    }

    private static int GetUserVersion(SqliteConnection conn)
    {
        var command = conn.CreateCommand();
        command.CommandText = @"PRAGMA user_version;";

        using var reader = command.ExecuteReader();
        reader.Read();
        var rawVersion = reader.GetString(0);
        return int.Parse(rawVersion);
    }
}