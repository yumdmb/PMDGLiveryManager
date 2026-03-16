## 1. Preflight Inspection & Aircraft Validation

- [ ] 1.1 Add livery package inspection models/interfaces and wire the new inspection service in `App.xaml.cs`
- [ ] 1.2 Implement ZIP inspection logic that reads archive metadata and detects the target PMDG aircraft package plus `atcId` when available
- [ ] 1.3 Integrate inspection into the existing single-ZIP install flow so mismatched or unknown packages are blocked before extraction
- [ ] 1.4 Add clear status messaging for match, mismatch, and unknown-package validation outcomes

## 2. Searchable Livery List

- [ ] 2.1 Extend `MainViewModel` state to track a search term and maintain a filtered installed-livery list for the selected aircraft
- [ ] 2.2 Add a search input to `MainWindow.xaml` and bind it to filter by folder name and ATC ID
- [ ] 2.3 Preserve empty-state and refresh behavior when search text changes or the installed-livery data is reloaded

## 3. Multi-ZIP Install & Batch Summary

- [ ] 3.1 Update the file picker flow to support selecting multiple ZIP files and route both picker and drag-drop through a shared batch install helper
- [ ] 3.2 Update drag-and-drop validation in `MainWindow.xaml.cs` to accept multiple ZIP files and reject drops with no eligible ZIPs
- [ ] 3.3 Implement sequential batch orchestration in `MainViewModel` using the inspection and installation services for each ZIP
- [ ] 3.4 Regenerate layout once after successful batch installs and surface a final installed/skipped/failed summary through `StatusMessage`

## 4. Thumbnail Discovery & List Presentation

- [ ] 4.1 Extend livery discovery and the `Livery` model with optional thumbnail metadata from common preview image locations
- [ ] 4.2 Add a small thumbnail preview column/template to the installed livery list without replacing the current list-based layout
- [ ] 4.3 Ensure missing or unreadable thumbnails fall back to a blank/placeholder state without failing livery discovery

## 5. Verification

- [ ] 5.1 Build the solution and verify the new services, ViewModel changes, and UI bindings compile cleanly
- [ ] 5.2 Manually smoke-test single ZIP install, aircraft mismatch rejection, unknown-package rejection, multi-ZIP batch install, search filtering, and thumbnail display
