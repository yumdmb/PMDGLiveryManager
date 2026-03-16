## ADDED Requirements

### Requirement: Inspect livery ZIP before extraction
The system SHALL inspect each selected livery ZIP before extraction and determine whether the package targets the currently selected PMDG aircraft package.

#### Scenario: Matching aircraft package detected
- **WHEN** the user has selected `pmdg-aircraft-738` and the ZIP contents identify `pmdg-aircraft-738`
- **THEN** the system marks the ZIP as valid for the current aircraft
- **AND** the install pipeline is allowed to continue

#### Scenario: Different aircraft package detected
- **WHEN** the user has selected `pmdg-aircraft-738` and the ZIP contents identify `pmdg-aircraft-77w`
- **THEN** the system SHALL stop the installation before extraction
- **AND** the system SHALL report that the ZIP targets a different aircraft than the current selection

#### Scenario: Aircraft package cannot be determined
- **WHEN** the system cannot determine a target PMDG aircraft from the ZIP contents
- **THEN** the system SHALL stop the installation before extraction
- **AND** the system SHALL report that the package could not be validated for the selected aircraft

### Requirement: Process multiple livery ZIPs sequentially
The system SHALL support processing multiple selected ZIP files in a single user action by validating and installing each ZIP sequentially against the currently selected aircraft package.

#### Scenario: Mixed batch results
- **WHEN** a batch contains a mix of valid ZIPs, mismatched ZIPs, and ZIPs that fail during installation
- **THEN** the system installs the valid ZIPs in selection order
- **AND** the system skips ZIPs that fail validation or installation
- **AND** the system reports a batch summary containing installed, skipped, and failed counts

#### Scenario: Layout regeneration after successful batch items
- **WHEN** one or more ZIPs in the batch are installed successfully
- **THEN** the system regenerates `layout.json` and updates `manifest.json` after batch processing completes
- **AND** the regeneration runs once per affected livery package rather than once per file
