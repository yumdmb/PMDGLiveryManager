## 1. Project Setup & Dependencies

- [x] 1.1 Add CommunityToolkit.Mvvm NuGet package to LiveryManager.csproj
- [x] 1.2 Create project folder structure: Models/, ViewModels/, Views/, Services/, Converters/
- [x] 1.3 Define the StoreType enum (Steam, MicrosoftStore, Custom)
- [x] 1.4 Add app.manifest enabling long path awareness (longPathAware=true)

## 2. Models

- [x] 2.1 Create AircraftPackage model (Name, LiveriesPath, BasePath)
- [x] 2.2 Create Livery model (FolderName, AtcId, FolderPath, IsValid)
- [x] 2.3 Create AppConfig model (StoreType, CustomCommunityPath, LastAircraft)

## 3. Path Detection Service

- [x] 3.1 Create IPathService interface with methods: GetCommunityFolderPath(StoreType, customPath?), GetPmdgWorkFolderPath(StoreType, aircraftName, customPath?), GetAirplanesFolderPath(communityPath, aircraftName)
- [x] 3.2 Implement PathService resolving Microsoft Store paths (%LOCALAPPDATA%\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\...)
- [x] 3.3 Implement PathService resolving Steam paths (%APPDATA%\Microsoft Flight Simulator\...)
- [x] 3.4 Implement PathService Custom store type: use the user-provided community path directly, derive work folder from parent
- [x] 3.5 Implement ValidatePath method that checks folder existence and returns error if missing

## 4. Aircraft Discovery Service

- [x] 4.1 Create IAircraftDiscoveryService interface with DiscoverAircraftPackages(communityPath) method
- [x] 4.2 Implement scanning Community folder for pmdg-aircraft-*-liveries directories
- [x] 4.3 Derive base aircraft name by stripping the -liveries suffix

## 5. Livery Discovery Service

- [x] 5.1 Create ILiveryDiscoveryService interface with GetInstalledLiveries(airplanesPath) method
- [x] 5.2 Implement scanning SimObjects\Airplanes subfolders for installed liveries
- [x] 5.3 Parse livery.json in each subfolder to extract atcId for display

## 6. Livery Installation Service

- [x] 6.1 Create ILiveryInstallationService interface with InstallLivery and UninstallLivery methods
- [x] 6.2 Implement ZIP extraction into the Airplanes folder using System.IO.Compression
- [x] 6.3 Implement livery.json parsing to extract atcId using System.Text.Json
- [x] 6.4 Implement options.ini rename to {atcId}.ini
- [x] 6.5 Implement copy of renamed INI file to PMDG work folder
- [x] 6.6 Implement uninstall: delete livery folder and remove INI from work folder
- [x] 6.7 Add validation for missing livery.json, missing atcId, and missing options.ini with appropriate error/warning messages

## 7. Layout Regeneration Service

- [x] 7.1 Create ILayoutService interface with RegenerateLayout(liveryPackagePath) method
- [x] 7.2 Implement recursive file enumeration of the livery package folder
- [x] 7.3 Compute relative paths using URI-based method (forward slashes), file size, and last-modified date as Windows FILETIME (FileInfo.LastWriteTimeUtc.ToFileTimeUtc())
- [x] 7.4 Exclude layout.json, manifest.json, MSFSLayoutGenerator.exe, and _CVT_* prefixed paths from the content array
- [x] 7.5 Serialize with WriteIndented=true, UnsafeRelaxedJsonEscaping, and write with LF line endings (replace \r\n with \n)
- [x] 7.6 After writing layout.json, update manifest.json total_package_size if the property exists (zero-padded to 20 chars, CRLF line endings)
- [x] 7.7 Write layout.json to the root of the livery package folder
- [x] 7.8 Implement safe-move long path strategy: move package to short temp path (e.g., C:\_LM_TEMP\), generate layout, move back. Fallback to \\?\ prefix if move fails.

## 8. Config Persistence Service

- [x] 8.1 Create IConfigService interface with LoadConfig() and SaveConfig(AppConfig) methods
- [x] 8.2 Implement ConfigService: load from %APPDATA%\LiveryManager\config.json using System.Text.Json
- [x] 8.3 Implement ConfigService: save to %APPDATA%\LiveryManager\config.json, create directory if needed
- [x] 8.4 Handle missing/corrupt config file gracefully (return defaults)

## 9. Main ViewModel

- [ ] 9.1 Create MainViewModel with ObservableProperties: SelectedStoreType, CustomCommunityPath, AircraftPackages, SelectedAircraft, Liveries, IsLoading, StatusMessage
- [ ] 9.2 Load saved config on ViewModel initialization and restore UI state
- [ ] 9.3 Implement store type selection change handler that triggers aircraft discovery (and saves config)
- [ ] 9.4 Implement Custom store type handler: open folder browser dialog, store path, trigger discovery
- [ ] 9.5 Implement aircraft selection change handler that triggers livery list refresh (and saves config)
- [ ] 9.6 Implement InstallLiveryCommand: open file dialog, run installation pipeline, regenerate layout, refresh list
- [ ] 9.7 Implement UninstallLiveryCommand: confirm, run uninstall, regenerate layout, refresh list
- [ ] 9.8 Add error handling and user feedback (success/error messages via StatusMessage)

## 10. Main Window UI

- [ ] 10.1 Design MainWindow.xaml layout: top toolbar (Store selector with Custom option, Aircraft selector), main area (livery list), action buttons (Install, Uninstall)
- [ ] 10.2 Bind Store type selector (ComboBox with Steam/MS Store/Custom) to MainViewModel.SelectedStoreType
- [ ] 10.3 Bind Aircraft selector (ComboBox) to MainViewModel.AircraftPackages and SelectedAircraft
- [ ] 10.4 Bind livery list (ListView/DataGrid) to MainViewModel.Liveries showing folder name and atcId
- [ ] 10.5 Bind Install and Uninstall buttons to respective commands
- [ ] 10.6 Add progress indicator bound to IsLoading
- [ ] 10.7 Add empty state message when no liveries are installed
- [ ] 10.8 Add status/error message display area bound to StatusMessage

## 11. Wiring & Integration

- [ ] 11.1 Register services in App.xaml.cs or use simple DI (constructor injection in MainViewModel)
- [ ] 11.2 Set MainWindow DataContext to MainViewModel
- [ ] 11.3 Build and verify the project compiles without errors
