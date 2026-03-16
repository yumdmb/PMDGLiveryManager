# PMDG Livery Manager

A WPF desktop application (.NET 10, Windows) that automates the installation of third-party liveries for PMDG aircraft addons in Microsoft Flight Simulator 2020.

Without this tool, installing a livery requires 8 manual steps: extracting a ZIP, opening a JSON file to copy an ATC ID, renaming an INI file, copying it into a deeply-nested PMDG work folder, and regenerating a `layout.json` package index. This application collapses that into a single button click.

---

## Features

- Auto-detects the MSFS Community folder for Steam, Microsoft Store, and custom installations
- Discovers all installed PMDG aircraft livery packages
- One-click livery install from a ZIP file
- One-click livery uninstall with confirmation
- Automatic `layout.json` and `manifest.json` regeneration after every change
- Long-path aware (handles Community folder paths exceeding 260 characters)
- Persists store type, custom path, and last-selected aircraft between sessions

---

## Requirements

- Windows 10/11
- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (or use the self-contained publish)
- Microsoft Flight Simulator 2020
- At least one PMDG aircraft addon installed

---

## Build & Run

```bash
dotnet restore
dotnet build LiveryManager.sln

# Run (Windows only — WPF requires a display)
dotnet run --project LiveryManager.csproj

# Self-contained single-file publish
dotnet publish LiveryManager.csproj -c Release -r win-x64 \
  --self-contained true -p:PublishSingleFile=true
# Output: bin\Release\net10.0-windows\win-x64\publish\
```

---

## Architecture

The application follows a strict **MVVM** pattern with manual dependency injection. There are no IoC containers — all six services are instantiated in `App.OnStartup` and injected into the single `MainViewModel` via constructor.

### Layers

```
┌─────────────────────────────────────────────────────────────┐
│                          View Layer                         │
│                                                             │
│   MainWindow.xaml  ──  MainWindow.xaml.cs (empty)          │
│       │ DataContext                                         │
└───────┼─────────────────────────────────────────────────────┘
        │
┌───────▼─────────────────────────────────────────────────────┐
│                       ViewModel Layer                       │
│                                                             │
│   MainViewModel                                             │
│     • ObservableProperties (IsLoading, Liveries, …)        │
│     • RelayCommands (InstallLivery, UninstallLivery)        │
│     • Change handlers (store type, aircraft selection)      │
│       │                                                     │
└───────┼─────────────────────────────────────────────────────┘
        │  interface references (IPathService, etc.)
┌───────▼─────────────────────────────────────────────────────┐
│                       Service Layer                         │
│                                                             │
│  IPathService          IConfigService                       │
│  PathService           ConfigService                        │
│      │                     │                                │
│  IAircraftDiscoveryService  ILiveryDiscoveryService         │
│  AircraftDiscoveryService   LiveryDiscoveryService          │
│                                                             │
│  ILiveryInstallationService  ILayoutService                 │
│  LiveryInstallationService   LayoutService                  │
│       │                                                     │
└───────┼─────────────────────────────────────────────────────┘
        │ plain data
┌───────▼─────────────────────────────────────────────────────┐
│                        Model Layer                          │
│                                                             │
│   StoreType (enum)   AircraftPackage   Livery   AppConfig   │
└─────────────────────────────────────────────────────────────┘
```

### Component Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│  App.OnStartup  (manual DI wiring)                               │
│                                                                  │
│   PathService ──────────────────────────────────────────────┐   │
│   AircraftDiscoveryService ──────────────────────────────┐  │   │
│   LiveryDiscoveryService ─────────────────────────────┐  │  │   │
│   LiveryInstallationService ───────────────────────┐  │  │  │   │
│   LayoutService ────────────────────────────────┐  │  │  │  │   │
│   ConfigService ─────────────────────────────┐  │  │  │  │  │   │
│                                              ▼  ▼  ▼  ▼  ▼  ▼   │
│                                         MainViewModel            │
│                                              │                   │
│                                         MainWindow               │
└──────────────────────────────────────────────────────────────────┘
```

### Service Responsibilities

| Service | Responsibility |
|---|---|
| `PathService` | Resolves the MSFS Community folder and PMDG work folder paths for all three store types |
| `AircraftDiscoveryService` | Scans the Community folder for `pmdg-aircraft-*-liveries` packages |
| `LiveryDiscoveryService` | Enumerates livery subfolders under `SimObjects\Airplanes`, parses `livery.json` for the ATC ID |
| `LiveryInstallationService` | Extracts a ZIP, parses `livery.json`, renames `options.ini` → `{atcId}.ini`, copies it to the PMDG work folder |
| `LayoutService` | Regenerates `layout.json` and updates `manifest.json` total size after any install/uninstall. Uses a safe-move strategy to `C:\_LM_TEMP\` to sidestep long-path limits |
| `ConfigService` | Loads and saves `%APPDATA%\LiveryManager\config.json` (store type, custom path, last aircraft) |

### Install Livery Flow

```
User clicks "Install Livery..."
        │
        ▼
OpenFileDialog  →  *.zip selected
        │
        ▼
LiveryInstallationService.InstallLiveryAsync
   1. Derive folder name from ZIP filename
   2. Extract ZIP  →  <airplanesPath>\<FolderName>\
   3. Parse livery.json  →  atcId
   4. Rename options.ini  →  {atcId}.ini
   5. Copy {atcId}.ini  →  PMDG work folder
   6. Return Livery object
        │
        ▼
LayoutService.RegenerateLayoutAsync
   1. Move liveries package  →  C:\_LM_TEMP\  (short-path workaround)
   2. Enumerate all files, build layout entries
   3. Write layout.json  (indented, UTF-8 no-BOM, LF endings)
   4. Update manifest.json total_package_size  (zero-padded to 20 chars)
   5. Move package back
        │
        ▼
RefreshLiveriesCore  →  update Liveries list in UI
        │
        ▼
StatusMessage = "Livery 'X' installed successfully."
```

### Uninstall Livery Flow

```
User selects livery  →  clicks "Uninstall"
        │
        ▼
MessageBox confirm dialog
        │
        ▼
LiveryInstallationService.UninstallLiveryAsync
   1. Directory.Delete(liveryFolder, recursive: true)
   2. File.Delete({atcId}.ini from PMDG work folder)
        │
        ▼
LayoutService.RegenerateLayoutAsync  (same as install)
        │
        ▼
RefreshLiveriesCore  →  update Liveries list in UI
```

### Startup Flow

```
App.OnStartup
    │
    ├─ Instantiate 6 services
    ├─ new MainViewModel(services)
    │       │
    │       └─ LoadSavedConfig()
    │               ├─ ConfigService.LoadConfig()  (%APPDATA%\LiveryManager\config.json)
    │               ├─ Restore SelectedStoreType + CustomCommunityPath  (suppressed, no handlers fire)
    │               └─ fire-and-forget DiscoverAircraftAsync(lastAircraft)
    │                       ├─ Validate community path
    │                       ├─ Task.Run → AircraftDiscoveryService.DiscoverAircraftPackages
    │                       ├─ Restore last selected aircraft (or pick first)
    │                       └─ RefreshLiveriesCore  →  LiveryDiscoveryService.GetInstalledLiveries
    │
    └─ new MainWindow { DataContext = viewModel }.Show()
```

---

## Project Structure

```
LiveryManager/
├── App.xaml / App.xaml.cs          # Startup, manual DI wiring
├── app.manifest                    # longPathAware = true
├── AssemblyInfo.cs                 # WPF theme resource declarations
├── MainWindow.xaml                 # Single application window
├── MainWindow.xaml.cs              # Empty code-behind
│
├── Models/
│   ├── StoreType.cs                # Enum: Steam | MicrosoftStore | Custom
│   ├── AircraftPackage.cs          # Discovered PMDG liveries package
│   ├── Livery.cs                   # Single installed livery entry
│   └── AppConfig.cs                # Persisted settings DTO
│
├── Services/
│   ├── IPathService.cs / PathService.cs
│   ├── IAircraftDiscoveryService.cs / AircraftDiscoveryService.cs
│   ├── ILiveryDiscoveryService.cs / LiveryDiscoveryService.cs
│   ├── ILiveryInstallationService.cs / LiveryInstallationService.cs
│   ├── ILayoutService.cs / LayoutService.cs
│   └── IConfigService.cs / ConfigService.cs
│
├── ViewModels/
│   └── MainViewModel.cs            # Single ViewModel for the entire UI
│
├── Converters/
│   └── StoreTypeConverter.cs       # StoreType enum → display string
│
└── Views/                          # Reserved for future UserControls
```

---

## Configuration

User settings are persisted automatically at `%APPDATA%\LiveryManager\config.json`:

```json
{
  "StoreType": "Steam",
  "CustomCommunityPath": null,
  "LastAircraft": "pmdg-aircraft-77f"
}
```

The file is created on first run and updated whenever the store type or selected aircraft changes.

---

## Key Design Decisions

- **No IoC container** — six services are instantiated directly in `App.OnStartup`. Simple, explicit, no framework overhead for an app of this size.
- **Thin ViewModel** — all file-system logic lives in services behind interfaces. The ViewModel only orchestrates services and manages UI state.
- **Long-path safe** — `LayoutService` moves the liveries package to a short root path (`C:\_LM_TEMP\`) before enumerating files to avoid the 260-char `MAX_PATH` limit, with a `\\?\` prefix fallback.
- **Nullable enforced** — `<Nullable>enable</Nullable>` is on; all code is null-safe with pattern matching and null-coalescing rather than null checks.
- **No error dialogs** — all user feedback (success and failure) surfaces through a `StatusMessage` bound to the status bar. No modal dialogs except the uninstall confirmation.
