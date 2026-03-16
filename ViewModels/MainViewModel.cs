using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveryManager.Models;
using LiveryManager.Services;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;

namespace LiveryManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ── Services ──────────────────────────────────────────────────────────────

    private readonly IPathService _pathService;
    private readonly IAircraftDiscoveryService _aircraftDiscoveryService;
    private readonly ILiveryDiscoveryService _liveryDiscoveryService;
    private readonly ILiveryInstallationService _liveryInstallationService;
    private readonly ILayoutService _layoutService;
    private readonly IConfigService _configService;

    // ── Guard flags ───────────────────────────────────────────────────────────

    /// <summary>Prevents re-entry in OnSelectedStoreTypeChanged when reverting a cancelled Custom selection.</summary>
    private bool _suppressStoreTypeChanged;

    /// <summary>Prevents OnSelectedAircraftChanged from triggering a refresh when the aircraft is set programmatically by DiscoverAircraftAsync.</summary>
    private bool _suppressAircraftChanged;

    /// <summary>The last confirmed store type (used to revert on Custom cancellation).</summary>
    private StoreType? _previousStoreType;

    // ── Observable properties ─────────────────────────────────────────────────

    /// <summary>True while any background operation is in progress.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    [NotifyCanExecuteChangedFor(nameof(InstallLiveryCommand))]
    [NotifyCanExecuteChangedFor(nameof(UninstallLiveryCommand))]
    private bool _isLoading;

    /// <summary>Success or error message shown in the status bar.</summary>
    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>Currently selected MSFS store type (Steam / Microsoft Store / Custom).</summary>
    [ObservableProperty]
    private StoreType? _selectedStoreType;

    /// <summary>User-supplied Community folder path when StoreType is Custom.</summary>
    [ObservableProperty]
    private string? _customCommunityPath;

    /// <summary>Aircraft packages discovered in the Community folder.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    private ObservableCollection<AircraftPackage> _aircraftPackages = [];

    /// <summary>Currently selected aircraft package.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    [NotifyCanExecuteChangedFor(nameof(InstallLiveryCommand))]
    private AircraftPackage? _selectedAircraft;

    /// <summary>Currently selected livery in the list.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UninstallLiveryCommand))]
    private Livery? _selectedLivery;

    /// <summary>Installed liveries for the selected aircraft.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    private ObservableCollection<Livery> _liveries = [];

    /// <summary>True when an aircraft is selected, not loading, and no liveries are installed.</summary>
    public bool IsEmptyState => SelectedAircraft != null && !IsLoading && Liveries.Count == 0;

    // ── Static data for view bindings ─────────────────────────────────────────

    /// <summary>Exposed for XAML binding to populate the store-type ComboBox.</summary>
    public static IReadOnlyList<StoreType> StoreTypes { get; } =
        [StoreType.Steam, StoreType.MicrosoftStore, StoreType.Custom];

    // ── Constructor ───────────────────────────────────────────────────────────

    public MainViewModel(
        IPathService pathService,
        IAircraftDiscoveryService aircraftDiscoveryService,
        ILiveryDiscoveryService liveryDiscoveryService,
        ILiveryInstallationService liveryInstallationService,
        ILayoutService layoutService,
        IConfigService configService)
    {
        _pathService = pathService;
        _aircraftDiscoveryService = aircraftDiscoveryService;
        _liveryDiscoveryService = liveryDiscoveryService;
        _liveryInstallationService = liveryInstallationService;
        _layoutService = layoutService;
        _configService = configService;

        LoadSavedConfig();
    }

    // ── Config persistence ────────────────────────────────────────────────────

    /// <summary>
    /// Load saved config and restore UI state on startup.
    /// Sets backing fields directly to avoid triggering change handlers during init.
    /// </summary>
    private void LoadSavedConfig()
    {
        var config = _configService.LoadConfig();
        if (!config.StoreType.HasValue)
            return;

        _previousStoreType = config.StoreType;

        // CustomCommunityPath has no change handler – set property directly
        CustomCommunityPath = config.CustomCommunityPath;

        // Use suppress flag so OnSelectedStoreTypeChanged does not fire during init
        _suppressStoreTypeChanged = true;
        SelectedStoreType = config.StoreType;
        _suppressStoreTypeChanged = false;

        // Trigger aircraft discovery (async fire-and-forget from ctor)
        _ = DiscoverAircraftAsync(config.LastAircraft);
    }

    private void SaveConfig()
    {
        _configService.SaveConfig(new AppConfig
        {
            StoreType = SelectedStoreType,
            CustomCommunityPath = CustomCommunityPath,
            LastAircraft = SelectedAircraft?.Name
        });
    }

    // ── Property change handlers ──────────────────────────────────────────────

    /// <summary>
    /// React to store type changes. Opens a folder browser when Custom is selected.
    /// Reverts the selection if the user cancels the dialog.
    /// </summary>
    partial void OnSelectedStoreTypeChanged(StoreType? value)
    {
        if (_suppressStoreTypeChanged) return;

        if (value == StoreType.Custom)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select your MSFS Community folder"
            };

            if (dialog.ShowDialog() == true)
            {
                CustomCommunityPath = dialog.FolderName;
                _previousStoreType = StoreType.Custom;
                SaveConfig();
                _ = DiscoverAircraftAsync();
            }
            else
            {
                // User cancelled – revert to previous selection without re-entering this handler
                _suppressStoreTypeChanged = true;
                SelectedStoreType = _previousStoreType;
                _suppressStoreTypeChanged = false;
            }
        }
        else if (value.HasValue)
        {
            CustomCommunityPath = null;
            _previousStoreType = value;
            SaveConfig();
            _ = DiscoverAircraftAsync();
        }
    }

    /// <summary>
    /// React to aircraft selection changes. Refreshes the livery list.
    /// No-op when the change comes from DiscoverAircraftAsync (flag suppressed).
    /// </summary>
    partial void OnSelectedAircraftChanged(AircraftPackage? value)
    {
        if (_suppressAircraftChanged) return;

        if (value != null)
        {
            SaveConfig();
            _ = RefreshLiveriesAsync();
        }
        else
        {
            Liveries = [];
        }
    }

    // ── Aircraft discovery ────────────────────────────────────────────────────

    /// <summary>
    /// Discover PMDG aircraft packages in the Community folder and select the best aircraft.
    /// Calls RefreshLiveriesCore directly to keep IsLoading=true for the entire operation.
    /// </summary>
    private async Task DiscoverAircraftAsync(string? lastAircraft = null)
    {
        if (!SelectedStoreType.HasValue) return;

        IsLoading = true;
        StatusMessage = null;

        try
        {
            var communityPath = _pathService.GetCommunityFolderPath(SelectedStoreType.Value, CustomCommunityPath);
            if (!_pathService.ValidatePath(communityPath, out var error))
            {
                StatusMessage = $"Community folder not found: {error}";
                AircraftPackages = [];
                SetSelectedAircraftSuppressed(null);
                Liveries = [];
                return;
            }

            var packages = await Task.Run(() =>
                _aircraftDiscoveryService.DiscoverAircraftPackages(communityPath));

            AircraftPackages = new ObservableCollection<AircraftPackage>(packages);

            if (packages.Count == 0)
            {
                StatusMessage = "No PMDG livery packages found in the Community folder.";
                SetSelectedAircraftSuppressed(null);
                Liveries = [];
                return;
            }

            // Restore the last used aircraft or default to the first package
            var target = (lastAircraft != null
                ? packages.FirstOrDefault(a => a.Name == lastAircraft)
                : null) ?? packages[0];

            // Set via suppressed setter so OnSelectedAircraftChanged does NOT fire
            SetSelectedAircraftSuppressed(target);
            SaveConfig();

            // Refresh liveries inline (IsLoading stays true for the whole sequence)
            await RefreshLiveriesCore();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error discovering aircraft: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Livery list refresh ───────────────────────────────────────────────────

    /// <summary>
    /// Core refresh logic – does NOT manage IsLoading. Called by both RefreshLiveriesAsync
    /// and the install/uninstall commands so they can keep their own IsLoading context.
    /// </summary>
    private async Task RefreshLiveriesCore()
    {
        if (SelectedAircraft == null || !SelectedStoreType.HasValue) return;

        var communityPath = _pathService.GetCommunityFolderPath(SelectedStoreType.Value, CustomCommunityPath);
        var airplanesPath = _pathService.GetAirplanesFolderPath(communityPath, SelectedAircraft.Name);
        var liveries = await Task.Run(() => _liveryDiscoveryService.GetInstalledLiveries(airplanesPath));
        Liveries = new ObservableCollection<Livery>(liveries);
    }

    /// <summary>
    /// Public refresh – manages its own IsLoading. Triggered by user changing aircraft.
    /// </summary>
    private async Task RefreshLiveriesAsync()
    {
        if (SelectedAircraft == null || !SelectedStoreType.HasValue) return;

        IsLoading = true;
        StatusMessage = null;

        try
        {
            await RefreshLiveriesCore();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading liveries: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task InstallLiveryAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Livery ZIP File",
            Filter = "ZIP Files (*.zip)|*.zip|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true) return;

        await InstallLiveryFromZipAsync(dialog.FileName);
    }

    /// <summary>
    /// Install a livery ZIP that was dropped onto the main window.
    /// </summary>
    public async Task InstallDroppedLiveryAsync(string zipFilePath)
    {
        if (!CanAcceptZipDrop())
        {
            StatusMessage = "Select a store type and aircraft before dropping a livery ZIP.";
            return;
        }

        await InstallLiveryFromZipAsync(zipFilePath);
    }

    /// <summary>
    /// Report a non-fatal drag/drop validation message through the status bar.
    /// </summary>
    public void ReportDropStatus(string message)
    {
        StatusMessage = message;
    }

    /// <summary>
    /// Returns true when the window can accept a dropped ZIP file for installation.
    /// </summary>
    public bool CanAcceptZipDrop() => CanInstall() && SelectedStoreType.HasValue;

    /// <summary>
    /// Shared install pipeline used by both the file picker and drag/drop.
    /// </summary>
    private async Task InstallLiveryFromZipAsync(string zipFilePath)
    {
        if (SelectedAircraft == null || !SelectedStoreType.HasValue) return;

        IsLoading = true;
        StatusMessage = null;

        try
        {
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("The selected ZIP file could not be found.", zipFilePath);
            }

            if (!Path.GetExtension(zipFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only ZIP files can be installed.");
            }

            var communityPath = _pathService.GetCommunityFolderPath(SelectedStoreType.Value, CustomCommunityPath);
            var airplanesPath = _pathService.GetAirplanesFolderPath(communityPath, SelectedAircraft.Name);
            var workFolderPath = _pathService.GetPmdgWorkFolderPath(SelectedStoreType.Value, SelectedAircraft.Name, CustomCommunityPath);

            var livery = await _liveryInstallationService.InstallLiveryAsync(zipFilePath, airplanesPath, workFolderPath);
            await _layoutService.RegenerateLayoutAsync(SelectedAircraft.LiveriesPath);
            await RefreshLiveriesCore();

            StatusMessage = $"Livery '{livery.AtcId ?? livery.FolderName}' installed successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Installation failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanInstall() => SelectedAircraft != null && !IsLoading;

    [RelayCommand(CanExecute = nameof(CanUninstall))]
    private async Task UninstallLiveryAsync()
    {
        if (SelectedLivery == null || SelectedAircraft == null || !SelectedStoreType.HasValue) return;

        var livery = SelectedLivery;

        var confirm = MessageBox.Show(
            $"Uninstall livery '{livery.AtcId ?? livery.FolderName}'?\n\n" +
            $"This will permanently delete the '{livery.FolderName}' folder and its associated INI file.",
            "Confirm Uninstall",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        StatusMessage = null;

        try
        {
            var workFolderPath = _pathService.GetPmdgWorkFolderPath(SelectedStoreType.Value, SelectedAircraft.Name, CustomCommunityPath);
            await _liveryInstallationService.UninstallLiveryAsync(livery, workFolderPath);
            await _layoutService.RegenerateLayoutAsync(SelectedAircraft.LiveriesPath);
            await RefreshLiveriesCore();

            StatusMessage = $"Livery '{livery.AtcId ?? livery.FolderName}' uninstalled successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Uninstall failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanUninstall() => SelectedLivery != null && SelectedAircraft != null && !IsLoading;

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets SelectedAircraft via its backing field to avoid triggering OnSelectedAircraftChanged.
    /// </summary>
    private void SetSelectedAircraftSuppressed(AircraftPackage? value)
    {
        _suppressAircraftChanged = true;
        SelectedAircraft = value;
        _suppressAircraftChanged = false;
    }
}
