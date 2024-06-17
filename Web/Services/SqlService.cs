using Microsoft.Data.Sqlite;

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

        if (migrations.Count <= 0)
        {
            return;
        }

        Console.WriteLine($"Applying {migrations.Count} migration files");
        ApplyMigrations(conn, userVersion, migrations);
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

    private static void SetUserVersion(SqliteConnection conn, int version)
    {
        if (version < 1)
        {
            throw new InvalidOperationException("Value must be greater than zero");
        }

        var command = conn.CreateCommand();
        command.CommandText = @"PRAGMA user_version = " + version + ";";

        var resp = command.ExecuteNonQuery();
        if (resp != 0)
        {
            throw new InvalidOperationException("Unable to update user version");
        }
    }

    private static Dictionary<int, string> FindMigrationsToApply(int appliedVersion)
    {
        var migrationFiles = new Dictionary<int, string>();
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
            if (version <= appliedVersion)
            {
                continue;
            }

            migrationFiles[version] = filePath;
        }

        return migrationFiles;
    }

    private static void ApplyMigrations(SqliteConnection conn, int appliedVersion, Dictionary<int, string> filePaths)
    {
        // There are migrations left to apply
        while (filePaths.Count > 0)
        {
            appliedVersion++;

            if (!filePaths.TryGetValue(appliedVersion, out var migrationPath))
            {
                throw new InvalidOperationException($"Migration could not be found for migration {appliedVersion}");
            }

            var query = File.ReadAllText(migrationPath);
            var command = conn.CreateCommand();
            command.CommandText = query;

            var resp = command.ExecuteNonQuery();
            if (resp != 0)
            {
                Console.WriteLine($"Error applying migration {migrationPath}: {resp}");
            }

            // Update user version so we can tell the migration completed
            SetUserVersion(conn, appliedVersion);

            // Remove the migration from the dictionary to signal it no longer needs to run
            filePaths.Remove(appliedVersion);
        }
    }
}
