using WinShield.Domain;

namespace WinShield.Infrastructure;

public sealed class DemoDataSeeder
{
    private readonly IScanRepository _scanRepository;
    private readonly ICleanupRepository _cleanupRepository;

    public DemoDataSeeder(IScanRepository scanRepository, ICleanupRepository cleanupRepository)
    {
        _scanRepository = scanRepository;
        _cleanupRepository = cleanupRepository;
    }

    public async Task SeedDevelopmentDataAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _scanRepository.GetRecentScanSessionsAsync(1, cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        var finding = new ThreatFinding(
            Guid.NewGuid(),
            @"C:\Users\User\AppData\Local\Temp\updater.exe",
            ThreatCategory.SuspiciousPersistence,
            ThreatSeverity.Medium,
            "Startup executable in temporary location",
            "Demo finding: startup target points to a temp folder.",
            62,
            "startup-temp-executable",
            "Review the startup entry before disabling or quarantining.",
            DateTimeOffset.UtcNow.AddDays(-1));

        var session = new ScanSession(
            Guid.NewGuid(),
            ScanType.Quick,
            ScanStatus.Completed,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(-1).AddMinutes(3),
            [new ScanTarget(@"C:\Users\User\Downloads")],
            1423,
            [finding]);

        await _scanRepository.SaveScanSessionAsync(session, cancellationToken);
        await _cleanupRepository.AddHistoryAsync(new CleanupHistoryRecord(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-2), 812_646_400, 184), cancellationToken);
    }
}
