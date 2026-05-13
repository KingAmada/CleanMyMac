using Microsoft.Extensions.DependencyInjection;
using WinShield.Cleaning.Engine;
using WinShield.Domain;
using WinShield.Security.Engine;
using WinShield.Shared;

namespace WinShield.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWinShieldCore(this IServiceCollection services, string rulesPath)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFileHasher, FileHasher>();
        services.AddSingleton<IDetectionRuleProvider>(_ => new JsonDetectionRuleProvider(rulesPath));
        services.AddSingleton<IRuleMatcher, RuleMatcher>();
        services.AddSingleton<IAmsiService, UnsupportedAmsiService>();
        services.AddSingleton<IThreatScanner, LocalThreatScanner>();
        services.AddSingleton<ICleanupScanner, CleanupScanner>();
        services.AddSingleton<SmartScanService>();
        services.AddSingleton<ReportService>();
        return services;
    }
}
