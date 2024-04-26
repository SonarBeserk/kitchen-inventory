using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;

namespace Web.Services;

public static class SqlService
{
    private const string SqlMigrationsPath = "Sql/";

    /// <summary>
    /// Runs migrations to update the database schema
    /// </summary>
    /// <param name="connectionString">The connection string for the datqbase</param>
    /// <exception cref="InvalidOperationException">Error thrown if the database connection string is invalid</exception>
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

        var migrations = FindMigrationsToApply(userVersion);
        Console.WriteLine($"Migrations to apply: {migrations.Count}");
    }

    private static int GetUserVersion(SqliteConnection conn)
    {
        var command = conn.CreateCommand();
        command.CommandText = @"PRAGMA user_version;";

        using var reader = command.ExecuteReader();
        reader.Read();
        var rawVersion = reader.GetString(0);
        reader.Close();
        return int.Parse(rawVersion);
    }

    private static List<string> FindMigrationsToApply(int appliedVersion)
    {
        var migrationFiles = new List<string>();
        var sqlFolderFiles = Directory.GetFiles(SqlMigrationsPath);
        foreach (var filePath in sqlFolderFiles)
        {
            if (!filePath.EndsWith(".sql"))
            {
                continue;
            }

            var fileName = filePath.Replace(SqlMigrationsPath, "");
            var versionString = fileName.Replace(".sql", "");
            if (!int.TryParse(versionString, out var version)) continue;
            if (version > 0 && version < appliedVersion)
            {
                continue;
            }
            
            migrationFiles.Add(filePath);
        }

        return migrationFiles;
    }
}