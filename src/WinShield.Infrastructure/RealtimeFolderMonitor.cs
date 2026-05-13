using WinShield.Domain;

namespace WinShield.Infrastructure;

public sealed class RealtimeFolderMonitor : IRealtimeMonitor, IDisposable
{
    private readonly IThreatScanner _scanner;
    private readonly List<FileSystemWatcher> _watchers = [];

    public RealtimeFolderMonitor(IThreatScanner scanner)
    {
        _scanner = scanner;
    }

    public bool IsRunning => _watchers.Count > 0;
    public event EventHandler<ThreatFinding>? ThreatDetected;

    public Task StartAsync(IReadOnlyList<string> folders, CancellationToken cancellationToken = default)
    {
        StopAsync(cancellationToken).GetAwaiter().GetResult();
        foreach (var folder in folders.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var watcher = new FileSystemWatcher(folder)
            {
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
            watcher.Created += OnCreated;
            watcher.Changed += OnCreated;
            _watchers.Add(watcher);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        foreach (var watcher in _watchers)
        {
            watcher.Dispose();
        }

        _watchers.Clear();
        return Task.CompletedTask;
    }

    private async void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(e.FullPath)) return;
        await WaitForWriteCompleteAsync(e.FullPath);
        var result = await _scanner.ScanAsync(ScanType.Quick, [new ScanTarget(e.FullPath, false)]);
        foreach (var finding in result.Findings)
        {
            ThreatDetected?.Invoke(this, finding);
        }
    }

    private static async Task WaitForWriteCompleteAsync(string path)
    {
        for (var i = 0; i < 10; i++)
        {
            try
            {
                await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return;
            }
            catch
            {
                await Task.Delay(250);
            }
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }
}
