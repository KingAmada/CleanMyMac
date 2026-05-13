using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using WinShield.Domain;

namespace WinShield.Platform.Windows;

public static class WindowsServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return services;
        }

        services.AddSingleton<IDefenderService, WindowsDefenderService>();
        services.AddSingleton<IAmsiService, WindowsAmsiService>();
        services.AddSingleton<IStartupProvider, WindowsStartupProvider>();
        return services;
    }
}
