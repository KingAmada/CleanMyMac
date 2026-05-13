using System.Security.Cryptography;
using WinShield.Domain;

namespace WinShield.Security.Engine;

public sealed class FileHasher : IFileHasher
{
    public async Task<string> Sha256Async(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
