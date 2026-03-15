## Context

This is a greenfield WPF application (.NET 10.0) created with `dotnet new wpf`. The application automates the manual PMDG livery installation process for Microsoft Flight Simulator 2020 across multiple PMDG aircraft types (738, 77F, 77W, etc.). Currently, users must perform 8 manual steps involving ZIP extraction, JSON parsing, file renaming, file copying across deep Windows paths, and running an external layout generator. The target users are flight sim enthusiasts who may not be comfortable with manual file operations.

The application operates entirely on the local file system -- there is no network, database, or cloud component. It must handle two different MSFS installation paths (Microsoft Store and Steam) and work with PMDG's file structure conventions. The PMDG addon folder naming convention is `pmdg-aircraft-<type>-liveries` for the liveries package and `pmdg-aircraft-<type>` for the base aircraft package.

## Goals / Non-Goals

**Goals:**
- Provide a UI flow: select Store type (Steam/MS Store) -> select Aircraft package -> view installed liveries -> install/uninstall
- Support any PMDG aircraft type by scanning the Community folder for `pmdg-aircraft-*-liveries` packages
- Automate the complete livery installation workflow (extract, parse, rename, copy, regenerate layout)
- Handle errors gracefully with clear user feedback (missing folders, corrupt ZIPs, etc.)

**Non-Goals:**
- Downloading liveries from the internet (user provides ZIP files)
- Multi-language / localization support
- Auto-update mechanism for the application itself
- Supporting MSFS 2024 or other simulators
- Supporting non-PMDG aircraft addons

## Decisions

### 1. MVVM Architecture with CommunityToolkit.Mvvm

**Decision**: Use CommunityToolkit.Mvvm for the MVVM pattern.

**Rationale**: CommunityToolkit.Mvvm is the standard Microsoft-recommended MVVM library for .NET. It provides source generators for `ObservableProperty`, `RelayCommand`, and messaging, reducing boilerplate significantly. Alternatives like Prism or ReactiveUI are heavier and unnecessary for this scope.

### 2. Service-Oriented Business Logic

**Decision**: Separate all file system operations into service classes behind interfaces.

**Rationale**: This keeps ViewModels thin and testable. Key services:
- `IPathService` -- resolves MSFS and PMDG paths based on store type and aircraft selection
- `ILiveryInstallationService` -- handles ZIP extraction, JSON parsing, INI renaming, file copying
- `ILayoutService` -- regenerates layout.json
- `IAircraftDiscoveryService` -- scans for available PMDG aircraft packages in Community folder
- `ILiveryDiscoveryService` -- scans installed liveries for a given aircraft package

**Alternatives considered**: Putting logic directly in ViewModels would be simpler initially but creates untestable, tightly-coupled code.

### 3. Built-in Libraries for ZIP and JSON

**Decision**: Use `System.IO.Compression` for ZIP handling and `System.Text.Json` for JSON parsing.

**Rationale**: Both are included in .NET 10.0 with no additional dependencies. The ZIP files are standard archives and the JSON files are simple structures -- no need for third-party libraries.

### 4. Store Selection (User-Driven, Not Auto-Detect)

**Decision**: The user explicitly selects their store type (Steam or Microsoft Store) via a dropdown/radio at the top of the UI. The app validates the selected path exists.

**Rationale**: The user knows which store they bought MSFS from. Explicit selection is simpler and more reliable than filesystem probing. If the path for the selected store doesn't exist, show an error. This avoids false positives from leftover folders.

### 5. Aircraft Package Discovery

**Decision**: Scan the Community folder for directories matching the pattern `pmdg-aircraft-*-liveries` to populate the aircraft selector.

**Rationale**: This makes the app automatically support any PMDG aircraft type without hardcoding. The naming convention is consistent across PMDG products. The base aircraft package name is derived by stripping the `-liveries` suffix.

### 6. Layout.json Regeneration In-App

**Decision**: Implement layout.json generation logic within the application rather than shelling out to MSFSLayoutGenerator.exe.

**Rationale**: The layout.json format is straightforward -- it lists relative file paths with their sizes. Implementing this in-app removes the dependency on an external tool that the user may not have. The algorithm is: recursively enumerate all files under the addon package folder, compute relative paths and file sizes, and write the JSON structure.

### 7. Project Structure

**Decision**: Organize code into folders by concern:

```
/Models          -- Data models (Livery, AircraftPackage, StoreType)
/ViewModels      -- MVVM ViewModels (MainViewModel)
/Views           -- XAML views (MainWindow, user controls)
/Services        -- Business logic services (interfaces + implementations)
/Converters      -- WPF value converters
```

**Rationale**: Standard WPF project layout. A single MainViewModel with the Store/Aircraft/Livery selection state keeps things simple for the initial version. No need for a multi-project solution at this scale.

### 8. Single-Window UI Layout

**Decision**: Use a single window with a top toolbar area (Store selector + Aircraft selector) and a main content area (livery list + install button).

**Rationale**: Keeps the UI simple and avoids navigation complexity. The workflow is linear: pick store -> pick aircraft -> manage liveries.

## Risks / Trade-offs

- **[Risk] PMDG changes their folder naming convention** -> Mitigation: The pattern `pmdg-aircraft-*-liveries` is configurable in one place (the discovery service). Easy to update.
- **[Risk] File permission errors on MSFS directories** -> Mitigation: Check write access before operations; show actionable error messages suggesting "Run as Administrator" if needed.
- **[Risk] Corrupt or non-standard livery ZIP files** -> Mitigation: Validate ZIP contents before extraction (check for expected files like aircraft.cfg, livery.json). Report which files are missing.
- **[Risk] Layout.json format changes in future MSFS updates** -> Mitigation: The format has been stable. If it changes, the regeneration logic is isolated in a single service and easy to update.
- **[Trade-off] .NET 10.0 preview** -> The project targets .NET 10.0 which may still be in preview. This is acceptable for a personal tool but means SDK updates may introduce breaking changes.
