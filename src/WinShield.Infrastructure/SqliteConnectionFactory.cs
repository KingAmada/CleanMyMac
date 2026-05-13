using Microsoft.Data.Sqlite;

namespace WinShield.Infrastructure;

public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string databasePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
    }

    public SqliteConnection CreateConnection() => new(_connectionString);
}
