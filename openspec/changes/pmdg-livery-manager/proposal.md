## Why

Installing PMDG liveries for Microsoft Flight Simulator 2020 is a tedious, error-prone manual process involving extracting ZIP files, reading JSON for ATC IDs, renaming INI files, copying them to the correct PMDG work folder, and regenerating layout.json. This WPF application automates the entire workflow into a one-click experience, eliminating user mistakes and saving time for simmers managing multiple liveries across multiple PMDG aircraft types.

## What Changes

- Add a WPF UI with a top-level **Store selector** (Steam vs Microsoft Store) that determines all file paths
- Add an **Aircraft selector** to choose which PMDG aircraft package to manage (e.g., `pmdg-aircraft-738`, `pmdg-aircraft-77f`, `pmdg-aircraft-77w`, etc.)
- Display a **list of installed liveries** for the selected aircraft, scanned from the liveries addon folder
- Provide an **Install** action that automates: extract livery ZIP, parse `livery.json` for `atcId`, rename `options.ini` to `{atcId}.ini`, copy INI to the PMDG work folder
- Integrate **layout.json regeneration** after install/uninstall operations
- Support **uninstalling/removing** liveries (reverse the installation steps)
- Auto-detect available PMDG aircraft packages from the Community folder

## Capabilities

### New Capabilities
- `livery-installation`: Automate the full livery installation pipeline -- extract ZIP, parse livery.json, rename options.ini, copy to PMDG work folder -- for any PMDG aircraft type
- `livery-management-ui`: WPF interface with Store selector, Aircraft selector, installed livery list, and install/uninstall actions
- `msfs-path-detection`: Resolve MSFS 2020 file paths based on user-selected store type (Microsoft Store vs Steam), and discover available PMDG aircraft packages
- `layout-regeneration`: Regenerate the layout.json file after livery changes to keep the addon package consistent

### Modified Capabilities

(none -- this is a greenfield project)

## Impact

- **Code**: Entire application is new -- MainWindow.xaml, view models, services, and models will be created from scratch on the existing WPF scaffold
- **Dependencies**: May add NuGet packages for MVVM framework (CommunityToolkit.Mvvm). ZIP handling (System.IO.Compression) and JSON parsing (System.Text.Json) are built-in.
- **File system**: Reads/writes to MSFS Community folder and PMDG work directories; requires appropriate file system permissions
- **External tools**: Optionally invokes MSFSLayoutGenerator.exe for layout.json regeneration, or implements equivalent logic in-app
