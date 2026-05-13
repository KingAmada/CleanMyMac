using WinShield.Domain;
using WinShield.Security.Engine;
using Xunit;

namespace WinShield.Domain.Tests;

public sealed class SeverityTests
{
    [Fact]
    public void Critical_Severity_Orders_Above_Low()
    {
        Assert.True(ThreatSeverity.Critical > ThreatSeverity.Low);
        Assert.Equal(ThreatSeverity.High, RuleMatcher.ToSeverity(70));
    }
}
