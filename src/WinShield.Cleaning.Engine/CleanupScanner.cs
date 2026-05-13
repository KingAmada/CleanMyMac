using WinShield.Domain;
namespace WinShield.Cleaning.Engine;

public sealed class CleanupScanner : ICleanupScanner
{
    private static readonly string[] JunkExtensions = [".tmp", ".temp", ".log", ".dmp", ".old"];
    private readonly IFileHasher _hasher;

    public CleanupScanner(IFileHasher hasher)
    {
        _hasher = hasher;
    }

    public Task<CleanupScanResult> ScanJunkAsync(IReadOnlyList<string> roots, CancellationToken cancellationToken = default)
    {
        var items = new List<CleanupItem>();
        foreach (var root in roots.Where(Directory.Exists))
        {
            foreach (var file in SafeEnumerateFiles(root))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var info = new FileInfo(file);
                var category = Categorize(file, info.Extension);
                if (category is null) continue;

                var safety = category is CleanupCategory.LogFile or CleanupCategory.UserTemp or CleanupCategory.AppTemp
                    ? CleanupSafetyLevel.Safe
                    : CleanupSafetyLevel.Review;

                items.Add(new CleanupItem(Guid.NewGuid(), file, category.Value, info.Length, info.LastWriteTimeUtc, safety,
                    safety is CleanupSafetyLevel.Safe ? RecommendedCleanupAction.Delete : RecommendedCleanupAction.Review,
                    safety is CleanupSafetyLevel.Safe,
                    $"Detected as {category.Value}. WinShield will present this for review before deletion."));
            }
        }

        var total = items.Sum(i => i.SizeBytes);
        var safe = items.Where(i => i.SafetyLevel is CleanupSafetyLevel.Safe).Sum(i => i.SizeBytes);
        return Task.FromResult(new CleanupScanResult(Guid.NewGuid(), DateTimeOffset.UtcNow, items, total, safe));
    }

    public Task<IReadOnlyList<LargeFileItem>> FindLargeFilesAsync(string root, long minimumBytes, CancellationToken cancellationToken = default)
    {
        var files = Directory.Exists(root)
            ? SafeEnumerateFiles(root)
                .Select(p => new FileInfo(p))
                .Where(f => f.Exists && f.Length >= minimumBytes)
                .OrderByDescending(f => f.Length)
                .Take(500)
                .Select(f => new LargeFileItem(f.FullName, f.Length, f.LastWriteTimeUtc, f.Extension))
                .ToList()
            : [];
        return Task.FromResult<IReadOnlyList<LargeFileItem>>(files);
    }

    public async Task<IReadOnlyList<DuplicateFileGroup>> FindDuplicatesAsync(string root, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(root)) return Array.Empty<DuplicateFileGroup>();

        var groupsBySize = SafeEnumerateFiles(root)
            .Select(p => new FileInfo(p))
            .Where(f => f.Exists && f.Length > 0)
            .GroupBy(f => f.Length)
            .Where(g => g.Count() > 1);

        var duplicates = new List<DuplicateFileGroup>();
        foreach (var group in groupsBySize)
        {
            var byHash = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in group)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var hash = await _hasher.Sha256Async(file.FullName, cancellationToken);
                if (!byHash.TryGetValue(hash, out var paths))
                {
                    paths = [];
                    byHash[hash] = paths;
                }

                paths.Add(file.FullName);
            }

            duplicates.AddRange(byHash.Where(kvp => kvp.Value.Count > 1).Select(kvp => new DuplicateFileGroup(kvp.Key, group.Key, kvp.Value)));
        }

        return duplicates;
    }

    private static CleanupCategory? Categorize(string path, string extension)
    {
        var lower = path.ToLowerInvariant();
        if (lower.Contains("thumbnail")) return CleanupCategory.ThumbnailCache;
        if (extension.Equals(".dmp", StringComparison.OrdinalIgnoreCase)) return CleanupCategory.CrashDump;
        if (extension.Equals(".log", StringComparison.OrdinalIgnoreCase)) return CleanupCategory.LogFile;
        if (JunkExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) return CleanupCategory.UserTemp;
        if (lower.Contains("cache") && lower.Contains("browser")) return CleanupCategory.BrowserCache;
        if (lower.Contains("temp")) return CleanupCategory.AppTemp;
        return null;
    }

    private static IEnumerable<string> SafeEnumerateFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            IEnumerable<string> files = [];
            IEnumerable<string> dirs = [];
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
