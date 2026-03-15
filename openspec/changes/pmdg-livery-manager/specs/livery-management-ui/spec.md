## ADDED Requirements

### Requirement: Store type selector
The system SHALL display a selector (dropdown) at the top of the main window allowing the user to choose between "Steam", "Microsoft Store", or "Custom" as their MSFS installation type. When "Custom" is selected, a folder browser dialog SHALL open allowing the user to select their Community folder path manually.

#### Scenario: User selects store type
- **WHEN** the application starts with no saved config
- **THEN** the store selector is visible and defaults to no selection (user must choose)

#### Scenario: User selects Custom
- **WHEN** the user selects "Custom" from the store dropdown
- **THEN** a folder browser dialog opens for the user to select their Community folder
- **AND** if the user cancels the dialog, the store selection reverts to the previous value

#### Scenario: Store selection persists across sessions
- **WHEN** the user selects a store type (and custom path if applicable)
- **THEN** the selection is saved to the config file and restored on next app launch

### Requirement: Aircraft package selector
The system SHALL display a selector listing available PMDG aircraft packages discovered in the Community folder. Each entry SHALL show the package identifier (e.g., `pmdg-aircraft-77f`, `pmdg-aircraft-738`).

#### Scenario: Aircraft packages found
- **WHEN** the user has selected a store type and PMDG livery packages exist in the Community folder
- **THEN** the aircraft selector is populated with discovered packages (derived from folder names matching `pmdg-aircraft-*-liveries`)

#### Scenario: No aircraft packages found
- **WHEN** no `pmdg-aircraft-*-liveries` folders exist in the Community folder
- **THEN** the system SHALL display a message indicating no PMDG livery packages were found

### Requirement: Installed livery list
The system SHALL display a list of installed liveries for the currently selected aircraft. Each entry SHALL show the livery folder name and the atcId (parsed from livery.json if available).

#### Scenario: Liveries are installed
- **WHEN** the user selects an aircraft package that has livery subfolders in `SimObjects\Airplanes\`
- **THEN** the list displays each livery with its folder name and atcId

#### Scenario: No liveries installed
- **WHEN** the selected aircraft has no livery subfolders
- **THEN** the list area displays an empty state message (e.g., "No liveries installed")

### Requirement: Install livery action
The system SHALL provide a button to install a new livery. Clicking it SHALL open a file picker dialog for selecting a livery ZIP file, then execute the full installation pipeline.

#### Scenario: User installs a livery
- **WHEN** the user clicks the Install button and selects a ZIP file
- **THEN** the system runs the installation pipeline (extract, parse, rename, copy, regenerate layout) and refreshes the livery list

#### Scenario: Installation succeeds
- **WHEN** the installation pipeline completes without errors
- **THEN** the system displays a success message and the new livery appears in the installed list

#### Scenario: Installation fails
- **WHEN** any step of the installation pipeline fails
- **THEN** the system displays an error message describing which step failed and why

### Requirement: Uninstall livery action
The system SHALL allow the user to select an installed livery from the list and uninstall it via a button or context action.

#### Scenario: User uninstalls a livery
- **WHEN** the user selects a livery from the list and clicks Uninstall
- **THEN** the system removes the livery folder and INI file, regenerates layout.json, and refreshes the list

### Requirement: Progress feedback during operations
The system SHALL show progress indication during install and uninstall operations to prevent the UI from appearing frozen.

#### Scenario: Long-running operation
- **WHEN** an install or uninstall operation is in progress
- **THEN** the UI displays a progress indicator (e.g., spinner or progress bar) and disables action buttons until complete
