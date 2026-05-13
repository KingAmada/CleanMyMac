using WinShield.Cleaning.Engine;
using WinShield.Security.Engine;
using Xunit;

namespace WinShield.Cleaning.Engine.Tests;

public sealed class CleanupScannerTests
{
    [Fact]
    public async Task Duplicate_Finder_Groups_By_Size_Then_Hash()
    {
        var root = Directory.CreateTempSubdirectory("winshield-dupes").FullName;
        await File.WriteAllTextAsync(Path.Combine(root, "a.txt"), "same");
        await File.WriteAllTextAsync(Path.Combine(root, "b.txt"), "same");
        await File.WriteAllTextAsync(Path.Combine(root, "c.txt"), "different");

        var scanner = new CleanupScanner(new FileHasher());
        var groups = await scanner.FindDuplicatesAsync(root);

        Assert.Single(groups);
        Assert.Equal(2, groups[0].Paths.Count);
    }

    [Fact]
    public async Task Junk_Scan_Aggregates_Size()
    {
        var root = Directory.CreateTempSubdirectory("winshield-junk").FullName;
        await File.WriteAllTextAsync(Path.Combine(root, "trace.log"), "12345");

        var scanner = new CleanupScanner(new FileHasher());
        var result = await scanner.ScanJunkAsync([root]);

        Assert.True(result.TotalBytes >= 5);
        Assert.Contains(result.Items, i => i.Category.ToString() == "LogFile");
    }
}
