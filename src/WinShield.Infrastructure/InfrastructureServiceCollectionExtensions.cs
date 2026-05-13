using Microsoft.Extensions.DependencyInjection;
using WinShield.Domain;

namespace WinShield.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddWinShieldInfrastructure(this IServiceCollection services, string appDataDirectory)
    {
        Directory.CreateDirectory(appDataDirectory);
        var dbPath = Path.Combine(appDataDirectory, "winshield.db");
        var quarantinePath = Path.Combine(appDataDirectory, "quarantine");

        services.AddSingleton(_ => new SqliteConnectionFactory(dbPath));
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<IScanRepository, SqliteScanRepository>();
        services.AddSingleton<IQuarantineRepository, SqliteQuarantineRepository>();
        services.AddSingleton<ICleanupRepository, SqliteCleanupRepository>();
        services.AddSingleton<DemoDataSeeder>();
        services.AddSingleton<IQuarantineService>(sp =>
            new QuarantineService(quarantinePath, sp.GetRequiredService<IQuarantineRepository>(), sp.GetRequiredService<IFileHasher>()));
        services.AddSingleton<IRealtimeMonitor, RealtimeFolderMonitor>();
        services.AddSingleton<IDefenderService, UnsupportedDefenderService>();
        services.AddSingleton<IStartupProvider, MockStartupProvider>();
        return services;
    }
}
