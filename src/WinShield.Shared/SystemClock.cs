using WinShield.Domain;

namespace WinShield.Shared;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
