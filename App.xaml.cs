using System.Windows;
using LiveryManager.Services;
using LiveryManager.ViewModels;

namespace LiveryManager;

/// <summary>
/// Application entry point. Wires services and injects MainViewModel into MainWindow.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Instantiate services (simple manual DI – no container needed at this scale)
        var pathService = new PathService();
        var aircraftDiscoveryService = new AircraftDiscoveryService();
        var liveryDiscoveryService = new LiveryDiscoveryService();
        var liveryInstallationService = new LiveryInstallationService();
        var layoutService = new LayoutService();
        var configService = new ConfigService();

        // Build the ViewModel with injected services
        var viewModel = new MainViewModel(
            pathService,
            aircraftDiscoveryService,
            liveryDiscoveryService,
            liveryInstallationService,
            layoutService,
            configService);

        // Create and show the main window with the ViewModel as DataContext
        var mainWindow = new MainWindow { DataContext = viewModel };
        mainWindow.Show();
    }
}
