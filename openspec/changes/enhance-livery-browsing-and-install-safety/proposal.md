## Why

The current app is reliable for one-at-a-time PMDG 2020 installs, but it still has a few sharp edges for real-world livery management. Users cannot quickly search large livery lists, cannot preview liveries visually, cannot install multiple ZIPs in one operation, and can accidentally install a ZIP for the wrong aircraft because the app trusts the selected aircraft instead of validating the package first.

## What Changes

- Add preflight ZIP inspection before installation so the app can detect the target PMDG aircraft, extract package metadata, and block obvious aircraft mismatches before any files are written
- Add install mismatch and unknown-package handling in the UI with clear status feedback for selected-aircraft vs package-aircraft conflicts
- Add livery list search on the current main window so users can filter installed liveries by folder name and ATC ID without changing the existing single-window workflow
- Extend install flows to support multi-select ZIP picking and multi-file ZIP drag-and-drop, with sequential batch processing and a final per-file summary
- Add thumbnail discovery and a small preview column in the installed livery list so users can identify liveries visually without moving to a full card-based layout
- Preserve the current MVVM and service-based architecture by introducing focused services and models for inspection, batch processing, filtering, and thumbnail lookup rather than moving logic into code-behind

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `livery-installation`: Add package inspection, aircraft mismatch blocking, and batch ZIP install behavior before the existing extraction and layout regeneration pipeline runs
- `livery-management-ui`: Add search, multi-ZIP selection and drag-drop behavior, batch result feedback, and thumbnail display to the existing main window workflow

## Impact

- **Code**: `MainWindow.xaml`, `MainWindow.xaml.cs`, `ViewModels/MainViewModel.cs`, livery models, installation/discovery services, and app startup wiring
- **New services/models**: likely an inspection service for ZIP metadata and aircraft detection, plus thumbnail discovery and batch result models
- **Behavior**: install now performs preflight validation before extraction; drag-and-drop and file picker flows expand from single ZIP to multi-ZIP batch processing
- **UI**: the existing list-based main window gains search and thumbnail affordances without replacing the current layout with a new browsing surface
