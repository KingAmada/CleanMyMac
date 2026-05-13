using WinShield.Domain;

namespace WinShield.Infrastructure;

public sealed class MockStartupProvider : IStartupProvider
{
    public Task<IReadOnlyList<StartupEntry>> GetStartupEntriesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StartupEntry> entries =
        [
            new("mock-onedrive", "OneDrive", "Microsoft Corporation", @"C:\Program Files\Microsoft OneDrive\OneDrive.exe", StartupEntrySource.Mock, true, StartupRisk.Low, "Common cloud sync startup item."),
            new("mock-temp-updater", "Updater", null, @"C:\Users\User\AppData\Local\Temp\updater.exe", StartupEntrySource.Mock, true, StartupRisk.High, "Executable path points to a temp folder; review before disabling."),
            new("mock-audio", "Audio Console", "Realtek", @"C:\Program Files\Realtek\Audio\RtkNGUI64.exe", StartupEntrySource.Mock, true, StartupRisk.Low, "Hardware support utility.")
        ];
        return Task.FromResult(entries);
    }
}
