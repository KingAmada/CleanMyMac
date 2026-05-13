using System.Text;
using WinShield.Domain;

namespace WinShield.Security.Engine;

public sealed class LocalThreatScanner : IThreatScanner
{
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ps1", ".bat", ".cmd", ".vbs", ".js", ".hta", ".json", ".xml", ".txt", ".reg"
    };

    private readonly IDetectionRuleProvider _ruleProvider;
    private readonly IRuleMatcher _ruleMatcher;
    private readonly IFileHasher _hasher;
    private readonly IAmsiService _amsi;

    public LocalThreatScanner(IDetectionRuleProvider ruleProvider, IRuleMatcher ruleMatcher, IFileHasher hasher, IAmsiService amsi)
    {
        _ruleProvider = ruleProvider;
        _ruleMatcher = ruleMatcher;
        _hasher = hasher;
        _amsi = amsi;
    }

    public async Task<ScanSession> ScanAsync(ScanType type, IReadOnlyList<ScanTarget> targets, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var started = DateTimeOffset.UtcNow;
        var rules = await _ruleProvider.LoadRulesAsync(cancellationToken);
        var findings = new List<ThreatFinding>();
        var filesScanned = 0;

        try
        {
            foreach (var target in targets)
            {
                foreach (var file in EnumerateFiles(target))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    filesScanned++;
                    var context = await BuildContextAsync(file, type is ScanType.SuspiciousStartup, cancellationToken);
                    findings.AddRange(_ruleMatcher.Match(context, rules));

                    if (context.TextPreview is not null)
                    {
                        var amsi = await _amsi.ScanContentAsync(Path.GetFileName(file), context.TextPreview, cancellationToken);
                        if (amsi is { Supported: true, Detected: true })
                        {
                            findings.Add(new ThreatFinding(Guid.NewGuid(), file, ThreatCategory.UnknownSuspicious, amsi.Severity,
                                "AMSI provider reported suspicious content", amsi.ProviderResult, 75, "amsi-provider",
                                "Review the script content and quarantine if unexpected.", DateTimeOffset.UtcNow));
                        }
                    }
                }
            }

            return new ScanSession(id, type, ScanStatus.Completed, started, DateTimeOffset.UtcNow, targets, filesScanned, findings);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new ScanSession(id, type, ScanStatus.Failed, started, DateTimeOffset.UtcNow, targets, filesScanned, findings, ex.Message);
        }
    }

    private static IEnumerable<string> EnumerateFiles(ScanTarget target)
    {
        if (File.Exists(target.Path))
        {
            yield return target.Path;
            yield break;
        }

        if (!Directory.Exists(target.Path))
        {
            yield break;
        }

        foreach (var file in target.Recursive ? SafeEnumerateFiles(target.Path) : Directory.EnumerateFiles(target.Path))
        {
            yield return file;
        }
    }

    private static IEnumerable<string> SafeEnumerateFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            IEnumerable<string> files;
            IEnumerable<string> dirs;
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

    private async Task<FileInspectionContext> BuildContextAsync(string file, bool startupContext, CancellationToken cancellationToken)
    {
        var info = new FileInfo(file);
        var hash = info.Length <= 512L * 1024L * 1024L ? await _hasher.Sha256Async(file, cancellationToken) : null;
        var extension = info.Extension;
        var preview = TextExtensions.Contains(extension) ? await ReadPreviewAsync(file, cancellationToken) : null;
        var entropy = info.Length is > 0 and <= 50L * 1024L * 1024L ? CalculateEntropy(file) : null;

        return new FileInspectionContext(file, info.Name, extension, info.Length, info.LastWriteTimeUtc, hash, entropy, preview, startupContext, false);
    }

    private static async Task<string?> ReadPreviewAsync(string file, CancellationToken cancellationToken)
    {
        var buffer = new char[64 * 1024];
        using var reader = new StreamReader(file, Encoding.UTF8, true);
        var read = await reader.ReadBlockAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
        return new string(buffer, 0, read);
    }

    private static double CalculateEntropy(string file)
    {
        Span<long> counts = stackalloc long[256];
        var total = 0L;
        using var stream = File.OpenRead(file);
        var buffer = new byte[8192];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            total += read;
            for (var i = 0; i < read; i++) counts[buffer[i]]++;
        }

        var entropy = 0d;
        for (var i = 0; i < counts.Length; i++)
        {
            if (counts[i] == 0) continue;
            var p = (double)counts[i] / total;
            entropy -= p * Math.Log2(p);
        }

        return entropy;
    }
}
