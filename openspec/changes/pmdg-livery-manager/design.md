## Context

This is a greenfield WPF application (.NET 10.0) created with `dotnet new wpf`. The application automates the manual PMDG livery installation process for Microsoft Flight Simulator 2020 across multiple PMDG aircraft types (738, 77F, 77W, etc.). Currently, users must perform 8 manual steps involving ZIP extraction, JSON parsing, file renaming, file copying across deep Windows paths, and running an external layout generator. The target users are flight sim enthusiasts who may not be comfortable with manual file operations.

The application operates entirely on the local file system -- there is no network, database, or cloud component. It must handle two different MSFS installation paths (Microsoft Store and Steam) plus a custom user-specified path, and work with PMDG's file structure conventions. The PMDG addon folder naming convention is `pmdg-aircraft-<type>-liveries` for the liveries package and `pmdg-aircraft-<type>` for the base aircraft package.

A known issue is that MSFS Community folder paths often exceed the Windows 260-character limit (e.g., `C:\Users\USER\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\Packages\Community\pmdg-aircraft-77f-liveries\SimObjects\Airplanes\KOREAN AIR (HL7203)\texture\...`).

## Goals / Non-Goals

**Goals:**
- Provide a UI flow: select Store type (Steam/MS Store/Custom) -> select Aircraft package -> view installed liveries -> install/uninstall
- Support any PMDG aircraft type by scanning the Community folder for `pmdg-aircraft-*-liveries` packages
- Automate the complete livery installation workflow (extract, parse, rename, copy, regenerate layout)
- Handle errors gracefully with clear user feedback (missing folders, corrupt ZIPs, etc.)
- Persist user settings across sessions (store type, community path, last aircraft)
- Handle long file paths (>260 chars) that are common with MSFS folder structures

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
- `IConfigService` -- loads/saves user settings to a JSON config file

**Alternatives considered**: Putting logic directly in ViewModels would be simpler initially but creates untestable, tightly-coupled code.

### 3. Built-in Libraries for ZIP and JSON

**Decision**: Use `System.IO.Compression` for ZIP handling and `System.Text.Json` for JSON parsing.

**Rationale**: Both are included in .NET 10.0 with no additional dependencies. The ZIP files are standard archives and the JSON files are simple structures -- no need for third-party libraries.

### 4. Store Selection with Custom Path Option

**Decision**: The user selects their store type (Steam, Microsoft Store, or Custom) via a dropdown at the top of the UI. Steam and MS Store resolve to known paths. Custom opens a folder browser dialog for the user to select their Community folder manually.

**Rationale**: Most users know which store they bought MSFS from, so Steam/MS Store covers the majority. The Custom option handles non-standard installations, moved folders, or secondary installs. This is the same approach used by other community tools (e.g., doguer27's livery manager). If the resolved path doesn't exist, show an error.

### 5. Aircraft Package Discovery

**Decision**: Scan the Community folder for directories matching the pattern `pmdg-aircraft-*-liveries` to populate the aircraft selector.

**Rationale**: This makes the app automatically support any PMDG aircraft type without hardcoding. The naming convention is consistent across PMDG products. The base aircraft package name is derived by stripping the `-liveries` suffix.

### 6. Layout.json Regeneration In-App with Long Path Mitigation

**Decision**: Implement layout.json generation logic within the application rather than shelling out to MSFSLayoutGenerator.exe. To avoid long path issues during file enumeration, temporarily move the package folder to a short temp path at the drive root (e.g., `C:\_LM_TEMP\`), generate layout.json there, then move the folder back.

**Rationale**: The layout.json format is straightforward. Implementing in-app removes the external tool dependency. The "safe move" strategy is borrowed from doguer27's livery manager and avoids the Windows 260-char path limit that commonly occurs with MSFS Community folder structures. The move is fast since it's on the same drive (just a rename at the filesystem level).

**Fallback**: If the move fails (e.g., files locked), fall back to generating in-place with `\\?\` long path prefix.

### 7. Config Persistence

**Decision**: Save user settings to a JSON file at `%APPDATA%\LiveryManager\config.json`. Settings include: store type, community path, last selected aircraft. Load on startup, save on change.

**Rationale**: Users shouldn't have to re-select their store type and community folder every time they launch the app. JSON is human-readable and uses the built-in `System.Text.Json`. `%APPDATA%` is the standard Windows location for per-user application settings.

**Alternatives considered**: Windows Registry would work but is harder to debug and reset. A settings file in the app directory wouldn't survive reinstalls and may require admin permissions.

### 8. Long Path Handling

**Decision**: Enable long path support in the application manifest and use `\\?\` prefixed paths where necessary. Additionally, use the "safe move to short path" strategy for layout.json generation (see Decision 6).

**Rationale**: MSFS Community folder paths routinely exceed 260 characters. .NET 10.0 supports long paths natively on Windows 10+ when enabled, but some APIs (like `System.IO.Compression.ZipFile`) may still have issues. The safe-move approach provides a reliable fallback.

### 9. Project Structure

**Decision**: Organize code into folders by concern:

```
/Models          -- Data models (Livery, AircraftPackage, StoreType, AppConfig)
/ViewModels      -- MVVM ViewModels (MainViewModel)
/Views           -- XAML views (MainWindow, user controls)
/Services        -- Business logic services (interfaces + implementations)
/Converters      -- WPF value converters
```

**Rationale**: Standard WPF project layout. A single MainViewModel with the Store/Aircraft/Livery selection state keeps things simple for the initial version. No need for a multi-project solution at this scale.

### 10. Single-Window UI Layout

**Decision**: Use a single window with a top toolbar area (Store selector + Aircraft selector) and a main content area (livery list + install button).

**Rationale**: Keeps the UI simple and avoids navigation complexity. The workflow is linear: pick store -> pick aircraft -> manage liveries.

## Risks / Trade-offs

- **[Risk] PMDG changes their folder naming convention** -> Mitigation: The pattern `pmdg-aircraft-*-liveries` is configurable in one place (the discovery service). Easy to update.
- **[Risk] File permission errors on MSFS directories** -> Mitigation: Check write access before operations; show actionable error messages suggesting "Run as Administrator" if needed.
- **[Risk] Corrupt or non-standard livery ZIP files** -> Mitigation: Validate ZIP contents before extraction (check for expected files like aircraft.cfg, livery.json). Report which files are missing.
- **[Risk] Layout.json format changes in future MSFS updates** -> Mitigation: The format has been stable. If it changes, the regeneration logic is isolated in a single service and easy to update.
- **[Risk] Long path issues (>260 chars)** -> Mitigation: App manifest enables long paths; safe-move strategy for layout generation; `\\?\` prefix fallback for file operations.
- **[Risk] Safe-move fails if files are locked by MSFS** -> Mitigation: Only perform layout regeneration when MSFS is not actively loading the package. Show a warning if the move fails and fall back to in-place generation.
- **[Trade-off] .NET 10.0 preview** -> The project targets .NET 10.0 which may still be in preview. This is acceptable for a personal tool but means SDK updates may introduce breaking changes.
