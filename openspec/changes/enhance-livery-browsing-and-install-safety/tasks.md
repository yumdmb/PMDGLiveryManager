## 1. Preflight Inspection & Aircraft Validation

- [x] 1.1 Add livery package inspection models/interfaces and wire the new inspection service in `App.xaml.cs`
- [x] 1.2 Implement ZIP inspection logic that reads archive metadata and detects the target PMDG aircraft package plus `atcId` when available
- [x] 1.3 Integrate inspection into the existing single-ZIP install flow so mismatched or unknown packages are blocked before extraction
- [x] 1.4 Add clear status messaging for match, mismatch, and unknown-package validation outcomes

## 2. Searchable Livery List

- [x] 2.1 Extend `MainViewModel` state to track a search term and maintain a filtered installed-livery list for the selected aircraft
- [x] 2.2 Add a search input to `MainWindow.xaml` and bind it to filter by folder name and ATC ID
- [x] 2.3 Preserve empty-state and refresh behavior when search text changes or the installed-livery data is reloaded

## 3. Multi-ZIP Install & Batch Summary

- [x] 3.1 Update the file picker flow to support selecting multiple ZIP files and route both picker and drag-drop through a shared batch install helper
- [x] 3.2 Update drag-and-drop validation in `MainWindow.xaml.cs` to accept multiple ZIP files and reject drops with no eligible ZIPs
- [x] 3.3 Implement sequential batch orchestration in `MainViewModel` using the inspection and installation services for each ZIP
- [x] 3.4 Regenerate layout once after successful batch installs and surface a final installed/skipped/failed summary through `StatusMessage`

## 4. Thumbnail Discovery & List Presentation

- [x] 4.1 Extend livery discovery and the `Livery` model with optional thumbnail metadata from common preview image locations
- [x] 4.2 Add a small thumbnail preview column/template to the installed livery list without replacing the current list-based layout
- [x] 4.3 Ensure missing or unreadable thumbnails fall back to a blank/placeholder state without failing livery discovery

## 5. Verification

- [x] 5.1 Build the solution and verify the new services, ViewModel changes, and UI bindings compile cleanly
- [ ] 5.2 Manually smoke-test single ZIP install, aircraft mismatch rejection, unknown-package rejection, multi-ZIP batch install, search filtering, and thumbnail display
