namespace LiveryManager;

using System.Windows;
using LiveryManager.Services;
using LiveryManager.ViewModels;

/// <summary>
/// Application entry point. Wires services and injects MainViewModel into MainWindow.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var pathService = new PathService();
        var aircraftDiscoveryService = new AircraftDiscoveryService();
        var liveryDiscoveryService = new LiveryDiscoveryService();
        var liveryInstallationService = new LiveryInstallationService();
        var liveryPackageInspectionService = new LiveryPackageInspectionService();
        var layoutService = new LayoutService();
        var configService = new ConfigService();

        var viewModel = new MainViewModel(
            pathService,
            aircraftDiscoveryService,
            liveryDiscoveryService,
            liveryInstallationService,
            liveryPackageInspectionService,
            layoutService,
            configService);

        var mainWindow = new MainWindow { DataContext = viewModel };
        mainWindow.Show();
    }
}
