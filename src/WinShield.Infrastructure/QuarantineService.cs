using WinShield.Domain;

namespace WinShield.Infrastructure;

public sealed class QuarantineService : IQuarantineService
{
    private readonly string _quarantineDirectory;
    private readonly IQuarantineRepository _repository;
    private readonly IFileHasher _hasher;

    public QuarantineService(string quarantineDirectory, IQuarantineRepository repository, IFileHasher hasher)
    {
        _quarantineDirectory = quarantineDirectory;
        _repository = repository;
        _hasher = hasher;
        Directory.CreateDirectory(_quarantineDirectory);
    }

    public async Task<QuarantineRecord> QuarantineAsync(ThreatFinding finding, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(finding.TargetPath))
        {
            throw new FileNotFoundException("The target file no longer exists.", finding.TargetPath);
        }

        var id = Guid.NewGuid();
        var hash = await _hasher.Sha256Async(finding.TargetPath, cancellationToken);
        var quarantinePath = Path.Combine(_quarantineDirectory, $"{id:N}.wshield-quarantine");
        File.Move(finding.TargetPath, quarantinePath, overwrite: false);

        var record = new QuarantineRecord(id, finding.TargetPath, quarantinePath, DateTimeOffset.UtcNow, hash, finding.RuleId, finding.Severity, false, null);
        await _repository.AddAsync(record, cancellationToken);
        return record;
    }

    public async Task RestoreAsync(Guid quarantineId, CancellationToken cancellationToken = default)
    {
        var record = await _repository.GetAsync(quarantineId, cancellationToken) ?? throw new InvalidOperationException("Quarantine item not found.");
        if (!File.Exists(record.QuarantinePath)) throw new FileNotFoundException("Quarantine payload not found.", record.QuarantinePath);
        Directory.CreateDirectory(Path.GetDirectoryName(record.OriginalPath)!);
        File.Move(record.QuarantinePath, record.OriginalPath, overwrite: false);
        await _repository.UpdateAsync(record with { Restored = true, RestoredAt = DateTimeOffset.UtcNow }, cancellationToken);
    }

    public async Task DeletePermanentlyAsync(Guid quarantineId, CancellationToken cancellationToken = default)
    {
        var record = await _repository.GetAsync(quarantineId, cancellationToken) ?? throw new InvalidOperationException("Quarantine item not found.");
        if (File.Exists(record.QuarantinePath)) File.Delete(record.QuarantinePath);
        await _repository.UpdateAsync(record with { Restored = false }, cancellationToken);
    }

    public Task<IReadOnlyList<QuarantineRecord>> ListAsync(CancellationToken cancellationToken = default) => _repository.ListAsync(cancellationToken);
}
