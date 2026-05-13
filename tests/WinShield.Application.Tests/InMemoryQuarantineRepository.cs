using WinShield.Domain;

namespace WinShield.Application.Tests;

internal sealed class InMemoryQuarantineRepository : IQuarantineRepository
{
    private readonly Dictionary<Guid, QuarantineRecord> _records = [];

    public Task AddAsync(QuarantineRecord record, CancellationToken cancellationToken = default)
    {
        _records[record.Id] = record;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(QuarantineRecord record, CancellationToken cancellationToken = default)
    {
        _records[record.Id] = record;
        return Task.CompletedTask;
    }

    public Task<QuarantineRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_records.GetValueOrDefault(id));

    public Task<IReadOnlyList<QuarantineRecord>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<QuarantineRecord>>(_records.Values.ToList());
}
