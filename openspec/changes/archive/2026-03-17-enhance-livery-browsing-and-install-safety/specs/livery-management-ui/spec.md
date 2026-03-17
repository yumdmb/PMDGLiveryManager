## ADDED Requirements

### Requirement: Search installed liveries
The system SHALL provide a search field that filters the installed livery list for the currently selected aircraft package by folder name and ATC ID as the user types.

#### Scenario: Search by folder name
- **WHEN** the user enters part of a livery folder name into the search field
- **THEN** the list shows only installed liveries whose folder names contain the entered text

#### Scenario: Search by ATC ID
- **WHEN** the user enters an ATC ID value into the search field
- **THEN** the list shows only installed liveries whose ATC ID contains the entered text

#### Scenario: Search has no matches
- **WHEN** the entered search text matches no installed liveries for the selected aircraft
- **THEN** the list shows no livery rows
- **AND** the empty-state messaging remains available to indicate that no matching liveries are currently visible

## MODIFIED Requirements

### Requirement: Installed livery list
The system SHALL display a list of installed liveries for the currently selected aircraft. Each entry SHALL show the livery folder name, the ATC ID (parsed from `livery.json` if available), and a thumbnail preview when one can be located in the livery folder.

#### Scenario: Liveries are installed
- **WHEN** the user selects an aircraft package that has livery subfolders in `SimObjects\Airplanes\`
- **THEN** the list displays each livery with its folder name and ATC ID
- **AND** each row includes the located thumbnail preview or a blank/placeholder preview state when no image exists

#### Scenario: No liveries installed
- **WHEN** the selected aircraft has no livery subfolders
- **THEN** the list area displays an empty state message (e.g., "No liveries installed")

### Requirement: Install livery action
The system SHALL provide a button to install one or more livery ZIP files. Clicking it SHALL open a file picker dialog that supports selecting multiple ZIP files, then execute the validated installation pipeline for each selected ZIP. The system SHALL also accept one or more ZIP files dropped onto the main window and run the same validated batch pipeline.

#### Scenario: User installs one or more ZIPs from the file picker
- **WHEN** the user clicks the Install button and selects one or more ZIP files
- **THEN** the system validates each selected ZIP against the current aircraft
- **AND** the system processes the selected ZIPs as a sequential batch

#### Scenario: User drags and drops one or more ZIPs
- **WHEN** the user drops one or more ZIP files onto the main window while a store type and aircraft are selected
- **THEN** the system validates each dropped ZIP against the current aircraft
- **AND** the system processes the dropped ZIPs as a sequential batch

#### Scenario: Batch completes with mixed outcomes
- **WHEN** at least one ZIP installs successfully and at least one ZIP is skipped or fails
- **THEN** the system refreshes the livery list after the batch completes
- **AND** the system displays a final summary containing installed, skipped, and failed counts

#### Scenario: Drop contains no eligible ZIP files
- **WHEN** the user drops files that do not contain any ZIP files eligible for installation
- **THEN** the system SHALL reject the drop
- **AND** the system SHALL display a validation message explaining that one or more ZIP files are required
