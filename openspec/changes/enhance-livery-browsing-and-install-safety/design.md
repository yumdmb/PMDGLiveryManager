## Context

The current application is a single-window WPF MVVM tool for Microsoft Flight Simulator 2020 PMDG liveries. It already supports store selection, aircraft package discovery, installed livery listing, one-at-a-time ZIP install, uninstall, drag-and-drop of a single ZIP, and in-app `layout.json` / `manifest.json` regeneration. The codebase is intentionally service-oriented: `MainViewModel` orchestrates UI state while path resolution, aircraft discovery, livery discovery, installation, layout generation, and config persistence live behind focused interfaces.

The requested enhancements cut across that flow in two places:

- the install path must become safer by validating ZIP contents before extraction and by handling multiple ZIPs in one action
- the browsing surface must become more usable for larger livery collections through search and thumbnail previews

The main constraint is architectural: these features should be added without drifting toward the monolithic "engine + code-behind" structure seen in the external comparison repo. They must fit the existing MVVM and service boundaries, keep nullable-safe code, and preserve the current status-message-based feedback style.

## Goals / Non-Goals

**Goals:**

- Reject or skip livery ZIPs that clearly target a different PMDG aircraft than the user has selected
- Stop unvalidated installs before any livery folder is extracted into the selected aircraft package
- Support multi-select ZIP installation from the file picker and multi-ZIP drag-and-drop on the main window
- Process ZIP batches sequentially and refresh the selected aircraft package once after batch completion
- Add a search field that filters installed liveries by folder name and ATC ID
- Add thumbnail previews to the existing list-based browsing UI
- Keep the existing MVVM/service architecture and reuse the current install pipeline where possible

**Non-Goals:**

- Auto-routing a mismatched ZIP to a different aircraft package automatically
- Supporting folder installs, raw `.ini` installs, or non-ZIP batch inputs
- Replacing the current list UI with a card gallery or multi-window workflow
- Supporting MSFS 2024, iFly aircraft, or non-PMDG packages
- Introducing network dependencies, background downloads, or application self-update behavior

## Decisions

### 1. Add a dedicated ZIP inspection service ahead of installation

**Decision**: Introduce a new service, tentatively `ILiveryPackageInspectionService`, that reads ZIP entries with `System.IO.Compression.ZipArchive` and extracts lightweight metadata without writing to the Community folder.

**Rationale**: The current `ILiveryInstallationService` assumes the selected aircraft is correct and starts extracting immediately. Preflight validation is a distinct concern: it needs archive inspection, text parsing, and target-aircraft detection before the existing install pipeline runs. A dedicated service keeps `ILiveryInstallationService` focused on actual file writes and keeps validation reusable for both single-file and batch flows.

**Expected outputs**: folder-name candidate, `atcId` when detectable, detected aircraft package name, detection confidence/evidence, and validation failure reason when applicable.

**Alternatives considered**:
- Extending `ILiveryInstallationService` to both inspect and install would blur read-only validation and write operations
- Doing inspection directly in `MainViewModel` would violate the current service-oriented design

### 2. Use deterministic aircraft detection with a conservative failure policy

**Decision**: Detect package aircraft by scanning known text-bearing files inside the ZIP (`livery.json`, `aircraft.cfg`, `livery.cfg`, and similar config files) for PMDG aircraft identifiers and aliases such as `pmdg-aircraft-738`, `b738`, and `737-800`.

**Rationale**: The real-world packages users install are inconsistent, so detection must tolerate multiple token styles. However, the safety goal is more important than maximizing installs. For this change, an unknown result is treated the same as a mismatch: installation stops before extraction and the user receives a clear status message.

**Alternatives considered**:
- Allowing the user to override unknown packages would increase flexibility, but it weakens the core protection this phase is meant to add
- Detecting by ZIP filename alone is too fragile and should only be used for display, not validation

### 3. Keep batch orchestration in the ViewModel and installation service single-package

**Decision**: Keep the existing `ILiveryInstallationService.InstallLiveryAsync(...)` as the unit that performs one install, and add batch orchestration in `MainViewModel` through a shared helper that accepts multiple ZIP paths, runs inspection and install sequentially, collects outcomes, and triggers one refresh/layout pass after the batch.

**Rationale**: Batch install is primarily application orchestration: validating user input, iterating over files, updating UI state, aggregating outcomes, and deciding when to refresh the UI. Those responsibilities already live in the ViewModel layer. Keeping the write-heavy service focused on one package avoids a second layer of orchestration services while preserving testable seams around inspection and installation.

**Expected additions**: a small result model such as `LiveryInstallOutcome` / `LiveryBatchSummary`, plus a shared helper used by both file-picker and drag-drop flows.

**Alternatives considered**:
- A dedicated batch-install service would also work, but adds an extra abstraction layer before the team has multiple non-UI callers for batch behavior

### 4. Regenerate layout once after batch completion

**Decision**: When one or more ZIPs install successfully, regenerate `layout.json` and update `manifest.json` once after the batch completes for the selected livery package, rather than after each successful file.

**Rationale**: Layout regeneration is one of the more expensive and path-sensitive operations in the app. Running it once per batch reduces unnecessary work, shortens the end-to-end install path, and minimizes repeated safe-move operations. This is a good fit because the current app always installs into the currently selected aircraft package rather than routing files to multiple aircraft.

**Alternatives considered**:
- Regenerating after each ZIP preserves the current per-file workflow, but adds avoidable overhead and longer lock windows during large batches

### 5. Add search through ViewModel filtering rather than a new collection framework

**Decision**: Extend `MainViewModel` with a search term property and keep an internal master list of discovered liveries. The bound list is refreshed from the master list whenever the search term or installed-livery data changes.

**Rationale**: The current app already refreshes `ObservableCollection<Livery>` instances directly after discovery and install operations. Reusing that pattern keeps the implementation predictable and avoids introducing `CollectionViewSource` or more complex WPF filtering infrastructure unless future UI requirements need it.

**Filter behavior**: case-insensitive match on `FolderName` and `AtcId`, limited to the currently selected aircraft.

**Alternatives considered**:
- `ICollectionView` filtering is a valid WPF option, but it adds more UI-specific state and is unnecessary for the current list size and feature set

### 6. Extend livery discovery to include thumbnail metadata

**Decision**: Extend the `Livery` model and discovery flow to include an optional thumbnail path discovered from common livery locations, then present it in a small preview column in the existing list view.

**Rationale**: Thumbnail lookup is metadata about installed liveries, not a separate install concern. The existing `ILiveryDiscoveryService` already parses each livery folder for display metadata (`atcId`), so it is the natural place to also detect a preview image. This keeps thumbnail logic tied to list refresh rather than scattering it across the view and view model.

**Expected search locations**: `thumbnail.jpg`, `thumbnail.png`, `thumbnail.jpeg` in the livery root, `thumbnail\`, `texture\`, or `texture.*\` subfolders, mirroring the practical heuristics from the comparison repo without copying its architecture.

**Alternatives considered**:
- A separate thumbnail service would be reasonable if preview generation becomes expensive or asynchronous, but is unnecessary for path-based discovery alone

### 7. Keep user feedback in the existing status-message model

**Decision**: Continue surfacing install validation errors and batch summaries through `StatusMessage` and the existing loading state, rather than adding new dialogs beyond the existing uninstall confirmation.

**Rationale**: This is already an intentional pattern in the app and keeps feedback consistent. Batch completion messages should summarize installed, skipped, and failed counts in a compact form. Individual mismatch or validation failures are still represented in the aggregated results used to compose that final message.

**Alternatives considered**:
- A modal summary dialog would be more visible, but it breaks the current non-modal workflow and adds a new interaction pattern for a list-based utility app

## Risks / Trade-offs

- **[Risk] Some real-world ZIPs may not contain enough metadata to determine aircraft** -> **Mitigation**: use multiple file types and alias tokens for detection, then fail safely with a clear validation message instead of guessing
- **[Risk] Batch status summaries may become too terse for very large batches** -> **Mitigation**: report aggregate counts in `StatusMessage` now and keep per-file outcomes in structured models so the UI can be expanded later without redesigning the install flow
- **[Risk] Thumbnail lookup may slow livery refresh on large packages** -> **Mitigation**: limit search to a small set of common filenames and folders, and treat missing thumbnails as optional rather than errors
- **[Risk] Search filtering and refresh logic may drift out of sync** -> **Mitigation**: centralize filtering in a single ViewModel helper applied after discovery, install, uninstall, and search-term changes
- **[Trade-off] Blocking unknown packages may reject some valid but unusually structured ZIPs** -> **Mitigation**: document the conservative policy in status feedback and revisit an explicit override only after the inspection heuristics are proven in use

## Migration Plan

1. Add the new inspection model/service and wire it into `App.OnStartup`
2. Update `MainViewModel` to inspect ZIPs before installation and to support multi-file input paths
3. Extend livery discovery and the `Livery` model with thumbnail metadata
4. Update `MainWindow` to add the search field, thumbnail column, and multi-file drag-drop behavior
5. Verify single-file install still works, then verify mixed-result batch installs and layout regeneration timing

Rollback is straightforward because the change is self-contained within the desktop app: remove the new inspection/thumbnails wiring, revert the ViewModel orchestration changes, and restore the previous single-ZIP UI bindings.

## Open Questions

- None at proposal time. The change deliberately chooses conservative package blocking for unknown aircraft and keeps the existing list-based UI, so implementation can proceed without additional product decisions.
