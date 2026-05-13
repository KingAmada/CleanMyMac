using System.Text.Json;
using Microsoft.Data.Sqlite;
using WinShield.Domain;

namespace WinShield.Infrastructure;

public sealed class SqliteScanRepository : IScanRepository
{
    private readonly SqliteConnectionFactory _factory;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public SqliteScanRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task SaveScanSessionAsync(ScanSession session, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = """
        insert or replace into ScanSessions (Id, Type, Status, StartedAt, CompletedAt, FilesScanned, Json)
        values ($id, $type, $status, $started, $completed, $files, $json);
        """;
        command.Parameters.AddWithValue("$id", session.Id.ToString());
        command.Parameters.AddWithValue("$type", session.Type.ToString());
        command.Parameters.AddWithValue("$status", session.Status.ToString());
        command.Parameters.AddWithValue("$started", session.StartedAt.ToString("O"));
        command.Parameters.AddWithValue("$completed", (object?)session.CompletedAt?.ToString("O") ?? DBNull.Value);
        command.Parameters.AddWithValue("$files", session.FilesScanned);
        command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(session, _json));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScanSession>> GetRecentScanSessionsAsync(int take, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "select Json from ScanSessions order by StartedAt desc limit $take";
        command.Parameters.AddWithValue("$take", take);
        var sessions = new List<ScanSession>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var session = JsonSerializer.Deserialize<ScanSession>(reader.GetString(0), _json);
            if (session is not null) sessions.Add(session);
        }

        return sessions;
    }
}

public sealed class SqliteQuarantineRepository : IQuarantineRepository
{
    private readonly SqliteConnectionFactory _factory;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public SqliteQuarantineRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task AddAsync(QuarantineRecord record, CancellationToken cancellationToken = default) => await UpsertAsync(record, cancellationToken);

    public async Task UpdateAsync(QuarantineRecord record, CancellationToken cancellationToken = default) => await UpsertAsync(record, cancellationToken);

    public async Task<QuarantineRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "select Json from QuarantineRecords where Id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value is string json ? JsonSerializer.Deserialize<QuarantineRecord>(json, _json) : null;
    }

    public async Task<IReadOnlyList<QuarantineRecord>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "select Json from QuarantineRecords order by QuarantinedAt desc";
        var records = new List<QuarantineRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var record = JsonSerializer.Deserialize<QuarantineRecord>(reader.GetString(0), _json);
            if (record is not null) records.Add(record);
        }

        return records;
    }

    private async Task UpsertAsync(QuarantineRecord record, CancellationToken cancellationToken)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = """
        insert or replace into QuarantineRecords
        (Id, OriginalPath, QuarantinePath, QuarantinedAt, Sha256, RuleId, Severity, Restored, RestoredAt, Json)
        values ($id, $original, $quarantine, $at, $hash, $rule, $severity, $restored, $restoredAt, $json);
        """;
        command.Parameters.AddWithValue("$id", record.Id.ToString());
        command.Parameters.AddWithValue("$original", record.OriginalPath);
        command.Parameters.AddWithValue("$quarantine", record.QuarantinePath);
        command.Parameters.AddWithValue("$at", record.QuarantinedAt.ToString("O"));
        command.Parameters.AddWithValue("$hash", record.Sha256);
        command.Parameters.AddWithValue("$rule", (object?)record.RuleId ?? DBNull.Value);
        command.Parameters.AddWithValue("$severity", record.Severity.ToString());
        command.Parameters.AddWithValue("$restored", record.Restored ? 1 : 0);
        command.Parameters.AddWithValue("$restoredAt", (object?)record.RestoredAt?.ToString("O") ?? DBNull.Value);
        command.Parameters.AddWithValue("$json", JsonSerializer.Serialize(record, _json));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed class SqliteCleanupRepository : ICleanupRepository
{
    private readonly SqliteConnectionFactory _factory;
    public SqliteCleanupRepository(SqliteConnectionFactory factory) => _factory = factory;

    public async Task AddHistoryAsync(CleanupHistoryRecord record, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "insert into CleanupHistory (Id, CompletedAt, BytesRemoved, ItemsRemoved) values ($id, $at, $bytes, $items)";
        command.Parameters.AddWithValue("$id", record.Id.ToString());
        command.Parameters.AddWithValue("$at", record.CompletedAt.ToString("O"));
        command.Parameters.AddWithValue("$bytes", record.BytesRemoved);
        command.Parameters.AddWithValue("$items", record.ItemsRemoved);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CleanupHistoryRecord>> GetHistoryAsync(int take, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = "select Id, CompletedAt, BytesRemoved, ItemsRemoved from CleanupHistory order by CompletedAt desc limit $take";
        command.Parameters.AddWithValue("$take", take);
        var records = new List<CleanupHistoryRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            records.Add(new CleanupHistoryRecord(Guid.Parse(reader.GetString(0)), DateTimeOffset.Parse(reader.GetString(1)), reader.GetInt64(2), reader.GetInt32(3)));
        }

        return records;
    }
}
