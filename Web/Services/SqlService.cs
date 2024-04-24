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
        var command = conn.CreateCommand();
        command.CommandText = @"PRAGMA schema_version;";

        var userVersion = "";
        using (var reader = command.ExecuteReader())
        {
            reader.Read();
            userVersion = reader.GetString(0);
;           if (string.IsNullOrWhiteSpace(userVersion))
            {
                throw new InvalidOperationException("Unable to find database version");
            }
        }

        Console.WriteLine($"Database Version: {userVersion}");
    }
}