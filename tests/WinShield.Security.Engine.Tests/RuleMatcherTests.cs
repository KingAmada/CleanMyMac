using WinShield.Domain;
using WinShield.Security.Engine;
using Xunit;

namespace WinShield.Security.Engine.Tests;

public sealed class RuleMatcherTests
{
    [Fact]
    public void Matches_Text_Pattern_And_Maps_Severity()
    {
        var rule = new DetectionRule("test", "Test rule", ThreatCategory.Malware, 55, "desc", "review",
            Extensions: [".ps1"], TextPatterns: ["WINSHIELD_TEST_DOWNLOAD_MARKER"]);
        var context = new FileInspectionContext("/tmp/sample.ps1", "sample.ps1", ".ps1", 10, DateTimeOffset.UtcNow, null, 4.2, "WINSHIELD_TEST_DOWNLOAD_MARKER", false, false);

        var findings = new RuleMatcher().Match(context, [rule]);

        Assert.Single(findings);
        Assert.Equal(ThreatSeverity.Medium, findings[0].Severity);
        Assert.Equal("test", findings[0].RuleId);
    }

    [Theory]
    [InlineData(10, ThreatSeverity.Info)]
    [InlineData(25, ThreatSeverity.Low)]
    [InlineData(50, ThreatSeverity.Medium)]
    [InlineData(75, ThreatSeverity.High)]
    [InlineData(95, ThreatSeverity.Critical)]
    public void Score_To_Severity_Is_Deterministic(int score, ThreatSeverity expected)
    {
        Assert.Equal(expected, RuleMatcher.ToSeverity(score));
    }
}
