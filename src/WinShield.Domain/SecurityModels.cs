namespace WinShield.Domain;

public enum ThreatSeverity { Info = 0, Low = 1, Medium = 2, High = 3, Critical = 4 }

public enum ThreatCategory
{
    Malware,
    Spyware,
    Adware,
    Pup,
    SuspiciousPersistence,
    BrowserHijacker,
    UnknownSuspicious
}

public enum ScanType
{
    Quick,
    Full,
    CustomFolder,
    SuspiciousStartup,
    BrowserHijacker,
    AmsiContent,
    Smart
}

public enum ScanStatus { Pending, Running, Completed, Failed, Cancelled }

public sealed record ScanTarget(string Path, bool Recursive = true, string? DisplayName = null);

public sealed record ThreatFinding(
    Guid Id,
    string TargetPath,
    ThreatCategory Category,
    ThreatSeverity Severity,
    string Title,
    string Evidence,
    int Score,
    string? RuleId,
    string Recommendation,
    DateTimeOffset DetectedAt);

public sealed record ScanSession(
    Guid Id,
    ScanType Type,
    ScanStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<ScanTarget> Targets,
    int FilesScanned,
    IReadOnlyList<ThreatFinding> Findings,
    string? ErrorMessage = null);

public sealed record DetectionRule(
    string Id,
    string Name,
    ThreatCategory Category,
    int Weight,
    string Description,
    string Recommendation,
    string? PathRegex = null,
    string? FileNameRegex = null,
    IReadOnlyList<string>? Extensions = null,
    IReadOnlyList<string>? TextPatterns = null,
    IReadOnlyList<string>? Sha256Hashes = null,
    bool RequiresStartupContext = false);

public sealed record FileInspectionContext(
    string Path,
    string FileName,
    string Extension,
    long Size,
    DateTimeOffset LastModified,
    string? Sha256,
    double? Entropy,
    string? TextPreview,
    bool IsStartupContext,
    bool HasTrustedPublisher);

public sealed record QuarantineRecord(
    Guid Id,
    string OriginalPath,
    string QuarantinePath,
    DateTimeOffset QuarantinedAt,
    string Sha256,
    string? RuleId,
    ThreatSeverity Severity,
    bool Restored,
    DateTimeOffset? RestoredAt);

public sealed record DefenderScanRequest(ScanType Type, string? Path = null);

public sealed record DefenderScanResult(bool Available, int ExitCode, string Status, string Output);

public sealed record AmsiScanResult(bool Supported, bool Detected, string ProviderResult, ThreatSeverity Severity);
