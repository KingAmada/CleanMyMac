using WinShield.Security.Engine;
using Xunit;

namespace WinShield.Security.Engine.Tests;

public sealed class FileHasherTests
{
    [Fact]
    public async Task Sha256_Returns_Known_Hash()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "winshield");

        var hash = await new FileHasher().Sha256Async(path);

        Assert.Equal("a837a1f958aa3f51290889e7a8161c620b60bc4909e559552a54ec6925536f33", hash);
    }
}
