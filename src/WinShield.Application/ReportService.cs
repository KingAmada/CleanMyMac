using System.Text.Json;
using WinShield.Domain;

namespace WinShield.Application;

public sealed class ReportService
{
    private readonly IScanRepository _scanRepository;
    private readonly ICleanupRepository _cleanupRepository;
    private readonly IQuarantineRepository _quarantineRepository;

    public ReportService(IScanRepository scanRepository, ICleanupRepository cleanupRepository, IQuarantineRepository quarantineRepository)
    {
        _scanRepository = scanRepository;
        _cleanupRepository = cleanupRepository;
        _quarantineRepository = quarantineRepository;
    }

    public async Task<string> ExportJsonAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            ExportedAt = DateTimeOffset.UtcNow,
            Scans = await _scanRepository.GetRecentScanSessionsAsync(100, cancellationToken),
            Cleanup = await _cleanupRepository.GetHistoryAsync(100, cancellationToken),
            Quarantine = await _quarantineRepository.ListAsync(cancellationToken)
        };
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
        return outputPath;
    }
}
