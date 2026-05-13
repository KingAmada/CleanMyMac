namespace WinShield.Domain;

public enum CleanupCategory
{
    UserTemp,
    WindowsTemp,
    AppTemp,
    ThumbnailCache,
    CrashDump,
    LogFile,
    RecycleBin,
    BrowserCache,
    RecentDocuments,
    Downloads,
    AppLeftover,
    Other
}

public enum CleanupSafetyLevel { Safe, Review, Caution }

public enum RecommendedCleanupAction { Delete, Review, Ignore }

public sealed record CleanupItem(
    Guid Id,
    string Path,
    CleanupCategory Category,
    long SizeBytes,
    DateTimeOffset LastModified,
    CleanupSafetyLevel SafetyLevel,
    RecommendedCleanupAction RecommendedAction,
    bool SelectedByDefault,
    string Explanation);

public sealed record CleanupScanResult(
    Guid Id,
    DateTimeOffset ScannedAt,
    IReadOnlyList<CleanupItem> Items,
    long TotalBytes,
    long SafeBytes);

public sealed record LargeFileItem(string Path, long SizeBytes, DateTimeOffset LastModified, string Extension);

public sealed record DuplicateFileGroup(string Sha256, long SizeBytes, IReadOnlyList<string> Paths);

public sealed record FolderUsageNode(string Path, long SizeBytes, IReadOnlyList<FolderUsageNode> Children);

public sealed record CleanupHistoryRecord(Guid Id, DateTimeOffset CompletedAt, long BytesRemoved, int ItemsRemoved);
