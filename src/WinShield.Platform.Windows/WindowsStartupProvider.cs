using System.Runtime.InteropServices;
using Microsoft.Win32;
using WinShield.Domain;

namespace WinShield.Platform.Windows;

public sealed class WindowsStartupProvider : IStartupProvider
{
    public Task<IReadOnlyList<StartupEntry>> GetStartupEntriesAsync(CancellationToken cancellationToken = default)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Task.FromResult<IReadOnlyList<StartupEntry>>([]);
        }

        var entries = new List<StartupEntry>();
        ReadRunKey(entries, Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run");
        ReadRunKey(entries, Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run");
        ReadStartupFolder(entries, Environment.GetFolderPath(Environment.SpecialFolder.Startup));
        return Task.FromResult<IReadOnlyList<StartupEntry>>(entries);
    }

    private static void ReadRunKey(List<StartupEntry> entries, RegistryKey hive, string path)
    {
        using var key = hive.OpenSubKey(path, false);
        if (key is null) return;
        foreach (var name in key.GetValueNames())
        {
            var value = key.GetValue(name)?.ToString() ?? string.Empty;
            entries.Add(new StartupEntry($"{hive.Name}:{path}:{name}", name, null, value, StartupEntrySource.RegistryRun, true, RiskForPath(value),
                HintForPath(value)));
        }
    }

    private static void ReadStartupFolder(List<StartupEntry> entries, string folder)
    {
        if (!Directory.Exists(folder)) return;
        foreach (var file in Directory.EnumerateFiles(folder))
        {
            entries.Add(new StartupEntry(file, Path.GetFileNameWithoutExtension(file), null, file, StartupEntrySource.StartupFolder, true, RiskForPath(file), HintForPath(file)));
        }
    }

    private static StartupRisk RiskForPath(string path) =>
        path.Contains(@"\Temp\", StringComparison.OrdinalIgnoreCase) ||
        path.Contains(@"\AppData\Local\Temp\", StringComparison.OrdinalIgnoreCase)
            ? StartupRisk.High
            : StartupRisk.Low;

    private static string HintForPath(string path) =>
        RiskForPath(path) is StartupRisk.High
            ? "Startup target points to a temp location. Review publisher and purpose."
            : "No obvious risky startup indicators detected.";
}
