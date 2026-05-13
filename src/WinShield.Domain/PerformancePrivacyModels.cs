namespace WinShield.Domain;

public enum StartupEntrySource { RegistryRun, StartupFolder, ScheduledTask, Mock }

public enum StartupRisk { Low, Medium, High }

public sealed record StartupEntry(
    string Id,
    string Name,
    string? Publisher,
    string Path,
    StartupEntrySource Source,
    bool Enabled,
    StartupRisk Risk,
    string Hint);

public sealed record ResourceSnapshot(
    DateTimeOffset CapturedAt,
    double? CpuPercent,
    long? TotalMemoryBytes,
    long? UsedMemoryBytes,
    long? FreeDiskBytes,
    IReadOnlyList<string> Recommendations);

public sealed record PrivacyTrace(
    Guid Id,
    string Path,
    string Category,
    long SizeBytes,
    DateTimeOffset LastModified,
    bool SelectedByDefault,
    string Explanation);

public sealed record ReportExport(Guid Id, DateTimeOffset CreatedAt, string Format, string Path);

public sealed record SecurityStatusSnapshot(
    DateTimeOffset CapturedAt,
    string ProtectionStatus,
    DateTimeOffset? LastQuickScan,
    DateTimeOffset? LastFullScan,
    int ThreatsFound,
    int QuarantinedItems,
    bool RealTimeMonitorEnabled,
    string DefenderStatus);
