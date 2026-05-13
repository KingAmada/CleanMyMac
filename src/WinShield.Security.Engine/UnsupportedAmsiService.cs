using WinShield.Domain;

namespace WinShield.Security.Engine;

public sealed class UnsupportedAmsiService : IAmsiService
{
    public Task<AmsiScanResult> ScanContentAsync(string contentName, string content, CancellationToken cancellationToken = default) =>
        Task.FromResult(new AmsiScanResult(false, false, "AMSI is unavailable on this platform.", ThreatSeverity.Info));
}
