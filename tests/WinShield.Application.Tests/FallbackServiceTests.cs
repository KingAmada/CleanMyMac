using WinShield.Domain;
using WinShield.Infrastructure;
using WinShield.Security.Engine;
using Xunit;

namespace WinShield.Application.Tests;

public sealed class FallbackServiceTests
{
    [Fact]
    public async Task Defender_Fallback_Is_Unavailable()
    {
        var result = await new UnsupportedDefenderService().RunScanAsync(new DefenderScanRequest(ScanType.Quick));
        Assert.False(result.Available);
    }

    [Fact]
    public async Task Amsi_Fallback_Is_Unsupported()
    {
        var result = await new UnsupportedAmsiService().ScanContentAsync("test", "content");
        Assert.False(result.Supported);
    }
}
