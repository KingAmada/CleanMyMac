namespace WinShield.Domain;

public interface IClock
{
    DateTimeOffset Now { get; }
}

public interface IFileHasher
{
    Task<string> Sha256Async(string path, CancellationToken cancellationToken = default);
}

public interface IThreatScanner
{
    Task<ScanSession> ScanAsync(ScanType type, IReadOnlyList<ScanTarget> targets, CancellationToken cancellationToken = default);
}

public interface IDetectionRuleProvider
{
    Task<IReadOnlyList<DetectionRule>> LoadRulesAsync(CancellationToken cancellationToken = default);
}

public interface IRuleMatcher
{
    IReadOnlyList<ThreatFinding> Match(FileInspectionContext context, IReadOnlyList<DetectionRule> rules);
}

public interface ICleanupScanner
{
    Task<CleanupScanResult> ScanJunkAsync(IReadOnlyList<string> roots, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LargeFileItem>> FindLargeFilesAsync(string root, long minimumBytes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DuplicateFileGroup>> FindDuplicatesAsync(string root, CancellationToken cancellationToken = default);
}

public interface IQuarantineService
{
    Task<QuarantineRecord> QuarantineAsync(ThreatFinding finding, CancellationToken cancellationToken = default);
    Task RestoreAsync(Guid quarantineId, CancellationToken cancellationToken = default);
    Task DeletePermanentlyAsync(Guid quarantineId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuarantineRecord>> ListAsync(CancellationToken cancellationToken = default);
}

public interface IScanRepository
{
    Task SaveScanSessionAsync(ScanSession session, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScanSession>> GetRecentScanSessionsAsync(int take, CancellationToken cancellationToken = default);
}

public interface IQuarantineRepository
{
    Task AddAsync(QuarantineRecord record, CancellationToken cancellationToken = default);
    Task UpdateAsync(QuarantineRecord record, CancellationToken cancellationToken = default);
    Task<QuarantineRecord?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuarantineRecord>> ListAsync(CancellationToken cancellationToken = default);
}

public interface ICleanupRepository
{
    Task AddHistoryAsync(CleanupHistoryRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CleanupHistoryRecord>> GetHistoryAsync(int take, CancellationToken cancellationToken = default);
}

public interface IDefenderService
{
    Task<DefenderScanResult> RunScanAsync(DefenderScanRequest request, CancellationToken cancellationToken = default);
}

public interface IAmsiService
{
    Task<AmsiScanResult> ScanContentAsync(string contentName, string content, CancellationToken cancellationToken = default);
}

public interface IStartupProvider
{
    Task<IReadOnlyList<StartupEntry>> GetStartupEntriesAsync(CancellationToken cancellationToken = default);
}

public interface IRealtimeMonitor
{
    bool IsRunning { get; }
    event EventHandler<ThreatFinding>? ThreatDetected;
    Task StartAsync(IReadOnlyList<string> folders, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
