using Microsoft.Data.Sqlite;

namespace XerSize.Repositories.Common;

public sealed class SqliteLocalStore
{
    private readonly object syncRoot = new();

    public SqliteLocalStore()
    {
        DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "xersize.db3");

        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);

        InitializeDatabase();
    }

    public string DatabasePath { get; }

    public object SyncRoot => syncRoot;

    private string WalPath => $"{DatabasePath}-wal";

    private string ShmPath => $"{DatabasePath}-shm";

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();

        return connection;
    }

    public Task<string> CreateExportCopyAsync()
    {
        lock (syncRoot)
        {
            CheckpointWal();

            var exportDirectory = Path.Combine(FileSystem.CacheDirectory, "Exports");
            Directory.CreateDirectory(exportDirectory);

            var exportPath = Path.Combine(
                exportDirectory,
                $"xersize-backup-{DateTime.Now:yyyyMMdd-HHmmss}.db3");

            File.Copy(DatabasePath, exportPath, overwrite: true);

            return Task.FromResult(exportPath);
        }
    }

    public Task<string> ReplaceDatabaseAsync(string sourceDatabasePath)
    {
        if (string.IsNullOrWhiteSpace(sourceDatabasePath))
            throw new ArgumentException("Import file path cannot be empty.", nameof(sourceDatabasePath));

        if (!File.Exists(sourceDatabasePath))
            throw new FileNotFoundException("Import database file does not exist.", sourceDatabasePath);

        lock (syncRoot)
        {
            var backupDirectory = Path.Combine(FileSystem.AppDataDirectory, "DatabaseBackups");
            Directory.CreateDirectory(backupDirectory);

            var backupPath = Path.Combine(
                backupDirectory,
                $"xersize-before-import-{DateTime.Now:yyyyMMdd-HHmmss}.db3");

            CheckpointWal();

            if (File.Exists(DatabasePath))
                File.Copy(DatabasePath, backupPath, overwrite: true);

            DeleteIfExists(WalPath);
            DeleteIfExists(ShmPath);

            File.Copy(sourceDatabasePath, DatabasePath, overwrite: true);

            DeleteIfExists(WalPath);
            DeleteIfExists(ShmPath);

            InitializeDatabase();

            return Task.FromResult(backupPath);
        }
    }

    private void InitializeDatabase()
    {
        lock (syncRoot)
        {
            using var connection = OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = """
                PRAGMA journal_mode = WAL;
                PRAGMA foreign_keys = ON;
                """;

            command.ExecuteNonQuery();
        }
    }

    private void CheckpointWal()
    {
        using var connection = OpenConnection();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA wal_checkpoint(FULL);";

        command.ExecuteNonQuery();
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}