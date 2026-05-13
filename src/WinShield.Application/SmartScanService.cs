using WinShield.Domain;

namespace WinShield.Application;

public sealed record SmartScanSummary(
    ScanSession SecurityScan,
    CleanupScanResult CleanupScan,
    IReadOnlyList<PrivacyTrace> PrivacyTraces,
    IReadOnlyList<StartupEntry> StartupEntries,
    SecurityStatusSnapshot Status);

public sealed class SmartScanService
{
    private readonly IThreatScanner _threatScanner;
    private readonly ICleanupScanner _cleanupScanner;
    private readonly IStartupProvider _startupProvider;
    private readonly IScanRepository _scanRepository;

    public SmartScanService(IThreatScanner threatScanner, ICleanupScanner cleanupScanner, IStartupProvider startupProvider, IScanRepository scanRepository)
    {
        _threatScanner = threatScanner;
        _cleanupScanner = cleanupScanner;
        _startupProvider = startupProvider;
        _scanRepository = scanRepository;
    }

    public async Task<SmartScanSummary> RunAsync(IReadOnlyList<string> roots, CancellationToken cancellationToken = default)
    {
        var targets = roots.Select(r => new ScanTarget(r, true)).ToList();
        var security = await _threatScanner.ScanAsync(ScanType.Smart, targets, cancellationToken);
        await _scanRepository.SaveScanSessionAsync(security, cancellationToken);
        var cleanup = await _cleanupScanner.ScanJunkAsync(roots, cancellationToken);
        var startup = await _startupProvider.GetStartupEntriesAsync(cancellationToken);
        var privacy = roots.Where(Directory.Exists)
            .SelectMany(BuildPrivacyTraces)
            .Take(200)
            .ToList();

        var statusText = security.Findings.Any(f => f.Severity >= ThreatSeverity.High)
            ? "Threats Found"
            : security.Findings.Count > 0 || startup.Any(s => s.Risk is StartupRisk.High)
                ? "Needs Attention"
                : "Protected";

        var status = new SecurityStatusSnapshot(DateTimeOffset.UtcNow, statusText, DateTimeOffset.UtcNow, null,
            security.Findings.Count, 0, false, "Not checked");

        return new SmartScanSummary(security, cleanup, privacy, startup, status);
    }

    private static IEnumerable<PrivacyTrace> BuildPrivacyTraces(string root)
    {
        string[] names = ["Recent", "History", "Cookies", "Cache"];
        foreach (var path in SafeEnumerateFiles(root).Where(p => names.Any(n => p.Contains(n, StringComparison.OrdinalIgnoreCase))))
        {
            FileInfo info;
            try { info = new FileInfo(path); }
            catch { continue; }

            yield return new PrivacyTrace(Guid.NewGuid(), path, "Privacy trace", info.Length, info.LastWriteTimeUtc, false,
                "Trace-like file detected. Review before removing.");
        }
    }

    private static IEnumerable<string> SafeEnumerateFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            IEnumerable<string> files;
            IEnumerable<string> dirs;
            try
            {
                files = Directory.EnumerateFiles(current);
                dirs = Directory.EnumerateDirectories(current);
            }
            catch
            {
                continue;
            }

            foreach (var file in files) yield return file;
            foreach (var dir in dirs) pending.Push(dir);
        }
    }
}
