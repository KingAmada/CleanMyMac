using WinShield.Domain;

namespace WinShield.Infrastructure;

public sealed class UnsupportedDefenderService : IDefenderService
{
    public Task<DefenderScanResult> RunScanAsync(DefenderScanRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(new DefenderScanResult(false, -1, "Unavailable on this platform", "Microsoft Defender command integration requires Windows."));
}
