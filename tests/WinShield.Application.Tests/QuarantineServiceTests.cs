using WinShield.Domain;
using WinShield.Infrastructure;
using WinShield.Security.Engine;
using Xunit;

namespace WinShield.Application.Tests;

public sealed class QuarantineServiceTests
{
    [Fact]
    public async Task Quarantine_Moves_File_And_Stores_Metadata()
    {
        var source = Path.GetTempFileName();
        await File.WriteAllTextAsync(source, "quarantine-me");
        var quarantineDir = Directory.CreateTempSubdirectory("winshield-quarantine").FullName;
        var repository = new InMemoryQuarantineRepository();
        var service = new QuarantineService(quarantineDir, repository, new FileHasher());
        var finding = new ThreatFinding(Guid.NewGuid(), source, ThreatCategory.UnknownSuspicious, ThreatSeverity.Medium,
            "Test", "Evidence", 50, "test-rule", "Review", DateTimeOffset.UtcNow);

        var record = await service.QuarantineAsync(finding);

        Assert.False(File.Exists(source));
        Assert.True(File.Exists(record.QuarantinePath));
        Assert.Equal("test-rule", record.RuleId);
        Assert.Single(await repository.ListAsync());
    }
}
