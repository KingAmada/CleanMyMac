using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinShield.Application;
using WinShield.Domain;

namespace WinShield.App.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly SmartScanService _smartScanService;
    private readonly ICleanupScanner _cleanupScanner;
    private readonly IThreatScanner _threatScanner;
    private readonly IStartupProvider _startupProvider;
    private readonly IQuarantineService _quarantineService;
    private readonly IDefenderService _defenderService;
    private readonly ReportService _reportService;

    [ObservableProperty] private string selectedModule = "Smart Scan";
    [ObservableProperty] private string protectionStatus = "Protected";
    [ObservableProperty] private int healthScore = 94;
    [ObservableProperty] private string scanProgress = "Ready";
    [ObservableProperty] private long reclaimableBytes;
    [ObservableProperty] private int threatCount;
    [ObservableProperty] private int startupCount;
    [ObservableProperty] private int privacyTraceCount;
    [ObservableProperty] private bool isBusy;

    public ObservableCollection<string> Modules { get; } =
    [
        "Smart Scan", "Cleanup", "Protection", "Privacy", "Performance", "Applications", "Files", "Reports", "Settings"
    ];

    public ObservableCollection<CleanupItem> CleanupItems { get; } = [];
    public ObservableCollection<ThreatFinding> ThreatFindings { get; } = [];
    public ObservableCollection<StartupEntry> StartupEntries { get; } = [];
    public ObservableCollection<PrivacyTrace> PrivacyTraces { get; } = [];
    public ObservableCollection<QuarantineRecord> QuarantineItems { get; } = [];
    public ObservableCollection<ScanSession> ScanHistory { get; } = [];

    public MainWindowViewModel(
        SmartScanService smartScanService,
        ICleanupScanner cleanupScanner,
        IThreatScanner threatScanner,
        IStartupProvider startupProvider,
        IQuarantineService quarantineService,
        IDefenderService defenderService,
        IScanRepository scanRepository,
        ReportService reportService)
    {
        _smartScanService = smartScanService;
        _cleanupScanner = cleanupScanner;
        _threatScanner = threatScanner;
        _startupProvider = startupProvider;
        _quarantineService = quarantineService;
        _defenderService = defenderService;
        _reportService = reportService;
        _ = LoadInitialAsync(scanRepository);
    }

    [RelayCommand]
    private void SelectModule(string module) => SelectedModule = module;

    [RelayCommand]
    private async Task RunSmartScanAsync()
    {
        await RunBusyAsync(async () =>
        {
            ScanProgress = "Scanning junk, startup entries, privacy traces, and suspicious files...";
            var roots = DefaultRoots();
            var summary = await _smartScanService.RunAsync(roots);
            Replace(CleanupItems, summary.CleanupScan.Items.Take(100));
            Replace(ThreatFindings, summary.SecurityScan.Findings);
            Replace(StartupEntries, summary.StartupEntries);
            Replace(PrivacyTraces, summary.PrivacyTraces);
            ProtectionStatus = summary.Status.ProtectionStatus;
            ThreatCount = summary.SecurityScan.Findings.Count;
            StartupCount = summary.StartupEntries.Count;
            PrivacyTraceCount = summary.PrivacyTraces.Count;
            ReclaimableBytes = summary.CleanupScan.TotalBytes;
            HealthScore = CalculateHealth(summary);
            ScanProgress = $"Completed. {ThreatCount} findings, {FormatBytes(ReclaimableBytes)} reclaimable.";
        });
    }

    [RelayCommand]
    private async Task RunCleanupScanAsync()
    {
        await RunBusyAsync(async () =>
        {
            ScanProgress = "Scanning safe cleanup locations...";
            var result = await _cleanupScanner.ScanJunkAsync(DefaultRoots());
            Replace(CleanupItems, result.Items.Take(200));
            ReclaimableBytes = result.TotalBytes;
            ScanProgress = $"Cleanup scan complete: {FormatBytes(result.TotalBytes)} found.";
        });
    }

    [RelayCommand]
    private async Task RunSecurityScanAsync()
    {
        await RunBusyAsync(async () =>
        {
            ScanProgress = "Running local malware and suspiciousness scan...";
            var session = await _threatScanner.ScanAsync(ScanType.Quick, DefaultRoots().Select(r => new ScanTarget(r, true)).ToList());
            Replace(ThreatFindings, session.Findings);
            ThreatCount = session.Findings.Count;
            ProtectionStatus = ThreatCount == 0 ? "Protected" : "Needs Attention";
            ScanProgress = $"Security scan complete: {session.FilesScanned} files scanned.";
        });
    }

    [RelayCommand]
    private async Task RunDefenderQuickScanAsync()
    {
        await RunBusyAsync(async () =>
        {
            ScanProgress = "Requesting Microsoft Defender quick scan...";
            var result = await _defenderService.RunScanAsync(new DefenderScanRequest(ScanType.Quick));
            ScanProgress = $"{result.Status}: {Trim(result.Output)}";
        });
    }

    [RelayCommand]
    private async Task RefreshStartupAsync()
    {
        var entries = await _startupProvider.GetStartupEntriesAsync();
        Replace(StartupEntries, entries);
        StartupCount = entries.Count;
    }

    [RelayCommand]
    private async Task RefreshQuarantineAsync()
    {
        Replace(QuarantineItems, await _quarantineService.ListAsync());
    }

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"winshield-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
        await _reportService.ExportJsonAsync(path);
        ScanProgress = $"Report exported to {path}";
    }

    private async Task LoadInitialAsync(IScanRepository scanRepository)
    {
        Replace(ScanHistory, await scanRepository.GetRecentScanSessionsAsync(20));
        await RefreshStartupAsync();
        await RefreshQuarantineAsync();
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await action(); }
        finally { IsBusy = false; }
    }

    private static IReadOnlyList<string> DefaultRoots()
    {
        var roots = new List<string>();
        var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var temp = Path.GetTempPath();
        if (Directory.Exists(user)) roots.Add(user);
        if (Directory.Exists(temp)) roots.Add(temp);
        return roots.Distinct().ToList();
    }

    private static int CalculateHealth(SmartScanSummary summary)
    {
        var score = 100;
        score -= Math.Min(45, summary.SecurityScan.Findings.Count * 12);
        score -= Math.Min(20, summary.StartupEntries.Count(s => s.Risk is StartupRisk.High) * 10);
        score -= Math.Min(15, summary.PrivacyTraces.Count / 10);
        return Math.Clamp(score, 10, 100);
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values) target.Add(value);
    }

    public static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }

    private static string Trim(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "No output";
        var compact = value.ReplaceLineEndings(" ").Trim();
        return compact[..Math.Min(220, compact.Length)];
    }
}
