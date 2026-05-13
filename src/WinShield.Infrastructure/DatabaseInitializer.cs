namespace WinShield.Infrastructure;

public sealed class DatabaseInitializer
{
    private readonly SqliteConnectionFactory _factory;

    public DatabaseInitializer(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = """
        create table if not exists ScanSessions (
          Id text primary key,
          Type text not null,
          Status text not null,
          StartedAt text not null,
          CompletedAt text null,
          FilesScanned integer not null,
          Json text not null
        );
        create table if not exists QuarantineRecords (
          Id text primary key,
          OriginalPath text not null,
          QuarantinePath text not null,
          QuarantinedAt text not null,
          Sha256 text not null,
          RuleId text null,
          Severity text not null,
          Restored integer not null,
          RestoredAt text null,
          Json text not null
        );
        create table if not exists CleanupHistory (
          Id text primary key,
          CompletedAt text not null,
          BytesRemoved integer not null,
          ItemsRemoved integer not null
        );
        create table if not exists Settings (
          Key text primary key,
          Value text not null
        );
        """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
