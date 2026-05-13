using System.Diagnostics;
using System.Runtime.InteropServices;
using WinShield.Domain;

namespace WinShield.Platform.Windows;

public sealed class WindowsDefenderService : IDefenderService
{
    public async Task<DefenderScanResult> RunScanAsync(DefenderScanRequest request, CancellationToken cancellationToken = default)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new DefenderScanResult(false, -1, "Unavailable on this platform", "Microsoft Defender integration requires Windows.");
        }

        var mpCmdRun = DiscoverMpCmdRun();
        if (mpCmdRun is null)
        {
            return new DefenderScanResult(false, -1, "Defender command tool not found", "MpCmdRun.exe could not be located.");
        }

        var arguments = request.Type switch
        {
            ScanType.Full => "-Scan -ScanType 2",
            ScanType.CustomFolder when !string.IsNullOrWhiteSpace(request.Path) => $"-Scan -ScanType 3 -File \"{request.Path}\"",
            _ => "-Scan -ScanType 1"
        };

        var process = new Process
        {
            StartInfo = new ProcessStartInfo(mpCmdRun, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return new DefenderScanResult(true, process.ExitCode, process.ExitCode == 0 ? "Completed" : "Completed with warnings", output + error);
    }

    private static string? DiscoverMpCmdRun()
    {
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var platform = Path.Combine(programData, "Microsoft", "Windows Defender", "Platform");
        if (Directory.Exists(platform))
        {
            var latest = Directory.EnumerateDirectories(platform)
                .OrderByDescending(Path.GetFileName)
                .Select(d => Path.Combine(d, "MpCmdRun.exe"))
                .FirstOrDefault(File.Exists);
            if (latest is not null) return latest;
        }

        var fallback = Path.Combine(programData, "Microsoft", "Windows Defender", "MpCmdRun.exe");
        return File.Exists(fallback) ? fallback : null;
    }
}
