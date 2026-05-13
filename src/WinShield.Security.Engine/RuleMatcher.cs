using System.Text.RegularExpressions;
using WinShield.Domain;

namespace WinShield.Security.Engine;

public sealed class RuleMatcher : IRuleMatcher
{
    public IReadOnlyList<ThreatFinding> Match(FileInspectionContext context, IReadOnlyList<DetectionRule> rules)
    {
        var findings = new List<ThreatFinding>();
        foreach (var rule in rules)
        {
            if (!Matches(rule, context))
            {
                continue;
            }

            var score = rule.Weight + HeuristicScore(context);
            findings.Add(new ThreatFinding(
                Guid.NewGuid(),
                context.Path,
                rule.Category,
                ToSeverity(score),
                rule.Name,
                BuildEvidence(rule, context, score),
                score,
                rule.Id,
                rule.Recommendation,
                DateTimeOffset.UtcNow));
        }

        return findings;
    }

    public static ThreatSeverity ToSeverity(int score) => score switch
    {
        >= 90 => ThreatSeverity.Critical,
        >= 70 => ThreatSeverity.High,
        >= 45 => ThreatSeverity.Medium,
        >= 20 => ThreatSeverity.Low,
        _ => ThreatSeverity.Info
    };

    private static bool Matches(DetectionRule rule, FileInspectionContext context)
    {
        if (rule.RequiresStartupContext && !context.IsStartupContext)
        {
            return false;
        }

        if (rule.Extensions is { Count: > 0 } &&
            !rule.Extensions.Any(e => string.Equals(e.TrimStart('.'), context.Extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (rule.Sha256Hashes is { Count: > 0 } && context.Sha256 is not null &&
            rule.Sha256Hashes.Any(h => string.Equals(h, context.Sha256, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var matchedAny = false;
        if (!string.IsNullOrWhiteSpace(rule.PathRegex))
        {
            matchedAny |= Regex.IsMatch(context.Path, rule.PathRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        if (!string.IsNullOrWhiteSpace(rule.FileNameRegex))
        {
            matchedAny |= Regex.IsMatch(context.FileName, rule.FileNameRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        if (rule.TextPatterns is { Count: > 0 } && context.TextPreview is not null)
        {
            matchedAny |= rule.TextPatterns.Any(p => context.TextPreview.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        return matchedAny;
    }

    private static int HeuristicScore(FileInspectionContext context)
    {
        var score = 0;
        if (!context.HasTrustedPublisher && IsExecutableLike(context.Extension)) score += 10;
        if (context.Entropy is > 7.4 && IsExecutableLike(context.Extension)) score += 15;
        if (context.Path.Contains("temp", StringComparison.OrdinalIgnoreCase) && IsExecutableLike(context.Extension)) score += 15;
        if (context.IsStartupContext) score += 10;
        return score;
    }

    private static bool IsExecutableLike(string extension) =>
        extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".vbs", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".js", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".bat", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase);

    private static string BuildEvidence(DetectionRule rule, FileInspectionContext context, int score) =>
        $"Rule '{rule.Id}' matched {Path.GetFileName(context.Path)} with score {score}. Size={context.Size}, Extension={context.Extension}, StartupContext={context.IsStartupContext}.";
}
