## 1. Project Setup & Dependencies

- [ ] 1.1 Add CommunityToolkit.Mvvm NuGet package to LiveryManager.csproj
- [ ] 1.2 Create project folder structure: Models/, ViewModels/, Views/, Services/, Converters/
- [ ] 1.3 Define the StoreType enum (Steam, MicrosoftStore)

## 2. Models

- [ ] 2.1 Create AircraftPackage model (Name, LiveriesPath, BasePath)
- [ ] 2.2 Create Livery model (FolderName, AtcId, FolderPath, IsValid)

## 3. Path Detection Service

- [ ] 3.1 Create IPathService interface with methods: GetCommunityFolderPath(StoreType), GetPmdgWorkFolderPath(StoreType, aircraftName), GetAirplanesFolderPath(communityPath, aircraftName)
- [ ] 3.2 Implement PathService resolving Microsoft Store paths (%LOCALAPPDATA%\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\...)
- [ ] 3.3 Implement PathService resolving Steam paths (%APPDATA%\Microsoft Flight Simulator\...)
- [ ] 3.4 Implement ValidatePath method that checks folder existence and returns error if missing

## 4. Aircraft Discovery Service

- [ ] 4.1 Create IAircraftDiscoveryService interface with DiscoverAircraftPackages(communityPath) method
- [ ] 4.2 Implement scanning Community folder for pmdg-aircraft-*-liveries directories
- [ ] 4.3 Derive base aircraft name by stripping the -liveries suffix

## 5. Livery Discovery Service

- [ ] 5.1 Create ILiveryDiscoveryService interface with GetInstalledLiveries(airplanesPath) method
- [ ] 5.2 Implement scanning SimObjects\Airplanes subfolders for installed liveries
- [ ] 5.3 Parse livery.json in each subfolder to extract atcId for display

## 6. Livery Installation Service

- [ ] 6.1 Create ILiveryInstallationService interface with InstallLivery and UninstallLivery methods
- [ ] 6.2 Implement ZIP extraction into the Airplanes folder using System.IO.Compression
- [ ] 6.3 Implement livery.json parsing to extract atcId using System.Text.Json
- [ ] 6.4 Implement options.ini rename to {atcId}.ini
- [ ] 6.5 Implement copy of renamed INI file to PMDG work folder
- [ ] 6.6 Implement uninstall: delete livery folder and remove INI from work folder
- [ ] 6.7 Add validation for missing livery.json, missing atcId, and missing options.ini with appropriate error/warning messages

## 7. Layout Regeneration Service

- [ ] 7.1 Create ILayoutService interface with RegenerateLayout(liveryPackagePath) method
- [ ] 7.2 Implement recursive file enumeration of the livery package folder
- [ ] 7.3 Compute relative paths using URI-based method (forward slashes), file size, and last-modified date as Windows FILETIME (FileInfo.LastWriteTimeUtc.ToFileTimeUtc())
- [ ] 7.4 Exclude layout.json, manifest.json, MSFSLayoutGenerator.exe, and _CVT_* prefixed paths from the content array
- [ ] 7.5 Serialize with WriteIndented=true, UnsafeRelaxedJsonEscaping, and write with LF line endings (replace \r\n with \n)
- [ ] 7.6 After writing layout.json, update manifest.json total_package_size if the property exists (zero-padded to 20 chars, CRLF line endings)
- [ ] 7.7 Write layout.json to the root of the livery package folder

## 8. Main ViewModel

- [ ] 8.1 Create MainViewModel with ObservableProperties: SelectedStoreType, AircraftPackages, SelectedAircraft, Liveries, IsLoading
- [ ] 8.2 Implement store type selection change handler that triggers aircraft discovery
- [ ] 8.3 Implement aircraft selection change handler that triggers livery list refresh
- [ ] 8.4 Implement InstallLiveryCommand: open file dialog, run installation pipeline, regenerate layout, refresh list
- [ ] 8.5 Implement UninstallLiveryCommand: confirm, run uninstall, regenerate layout, refresh list
- [ ] 8.6 Add error handling and user feedback (success/error messages)

## 9. Main Window UI

- [ ] 9.1 Design MainWindow.xaml layout: top toolbar (Store selector, Aircraft selector), main area (livery list), action buttons (Install, Uninstall)
- [ ] 9.2 Bind Store type selector (ComboBox/RadioButtons) to MainViewModel.SelectedStoreType
- [ ] 9.3 Bind Aircraft selector (ComboBox) to MainViewModel.AircraftPackages and SelectedAircraft
- [ ] 9.4 Bind livery list (ListView/DataGrid) to MainViewModel.Liveries showing folder name and atcId
- [ ] 9.5 Bind Install and Uninstall buttons to respective commands
- [ ] 9.6 Add progress indicator bound to IsLoading
- [ ] 9.7 Add empty state message when no liveries are installed
- [ ] 9.8 Add error/status message display area

## 10. Wiring & Integration

- [ ] 10.1 Register services in App.xaml.cs or use simple DI (constructor injection in MainViewModel)
- [ ] 10.2 Set MainWindow DataContext to MainViewModel
- [ ] 10.3 End-to-end test: select store, select aircraft, view liveries, install from ZIP, verify livery appears, uninstall, verify removal
