using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Fonts.Inter;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WinShield.Application;
using WinShield.Infrastructure;
using WinShield.Platform.Windows;
using WinShield.App.ViewModels;
using WinShield.App.Views;

namespace WinShield.App;

public sealed class App : Avalonia.Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
        RequestedThemeVariant = ThemeVariant.Dark;
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinShield");
        var rulesPath = FindRulesDirectory();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(appData, "logs", "winshield-.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var services = new ServiceCollection()
            .AddLogging(builder => builder.AddSerilog())
            .AddWinShieldCore(rulesPath)
            .AddWinShieldInfrastructure(appData)
            .AddWindowsPlatformServices();

        services.AddSingleton<MainWindowViewModel>();
        Services = services.BuildServiceProvider();
        await Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();
        await Services.GetRequiredService<DemoDataSeeder>().SeedDevelopmentDataAsync();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = Services.GetRequiredService<MainWindowViewModel>() };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string FindRulesDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "rules");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "rules");
    }
}
