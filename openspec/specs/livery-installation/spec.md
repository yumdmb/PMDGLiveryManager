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

### Requirement: Extract livery ZIP into aircraft liveries folder
The system SHALL accept a livery ZIP file and extract its contents into a new subfolder under the selected aircraft's `SimObjects\Airplanes` directory within the liveries package. The subfolder name SHALL be derived from the ZIP file name (without the .zip extension).

#### Scenario: Successful ZIP extraction
- **WHEN** the user selects a livery ZIP file (e.g., "KOREAN AIR (HL7203).zip") and triggers install for aircraft `pmdg-aircraft-77f`
- **THEN** the system extracts the ZIP contents into `<Community>\pmdg-aircraft-77f-liveries\SimObjects\Airplanes\KOREAN AIR (HL7203)\`

#### Scenario: Subfolder already exists
- **WHEN** a folder with the derived name already exists in the Airplanes directory
- **THEN** the system SHALL warn the user and ask whether to overwrite or cancel

### Requirement: Parse livery.json for atcId
The system SHALL read the `livery.json` file from the extracted livery folder and extract the `atcId` value.

#### Scenario: Successful atcId extraction
- **WHEN** the livery folder contains a `livery.json` with an `atcId` field (e.g., `"atcId": "HL7203"`)
- **THEN** the system reads and returns the value `HL7203`

#### Scenario: Missing livery.json
- **WHEN** the extracted folder does not contain a `livery.json` file
- **THEN** the system SHALL report an error indicating the livery package is invalid

#### Scenario: Missing atcId field
- **WHEN** the `livery.json` exists but does not contain an `atcId` field
- **THEN** the system SHALL report an error indicating the atcId is missing from livery.json

### Requirement: Rename options.ini to atcId-based filename
The system SHALL rename the `options.ini` file in the extracted livery folder to `{atcId}.ini` using the atcId parsed from livery.json.

#### Scenario: Successful rename
- **WHEN** the livery folder contains `options.ini` and atcId is `HL7203`
- **THEN** the system renames the file to `HL7203.ini`

#### Scenario: Missing options.ini
- **WHEN** the livery folder does not contain an `options.ini` file
- **THEN** the system SHALL skip the INI rename step and log a warning (some liveries may not include options.ini)

### Requirement: Copy renamed INI to PMDG work folder
The system SHALL copy the renamed `{atcId}.ini` file to the PMDG aircraft work folder at the path determined by the selected store type and aircraft.

#### Scenario: Copy to Microsoft Store path
- **WHEN** store type is Microsoft Store and aircraft is `pmdg-aircraft-77f`
- **THEN** the system copies `{atcId}.ini` to `%LOCALAPPDATA%\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalState\Packages\pmdg-aircraft-77f\work\Aircraft\`

#### Scenario: Copy to Steam path
- **WHEN** store type is Steam and aircraft is `pmdg-aircraft-77f`
- **THEN** the system copies `{atcId}.ini` to `%APPDATA%\Microsoft Flight Simulator\Packages\pmdg-aircraft-77f\work\Aircraft\`

#### Scenario: Work folder does not exist
- **WHEN** the PMDG work folder does not exist (aircraft has never been flown)
- **THEN** the system SHALL report a warning that the user must fly the aircraft at least once before the INI can be copied, and SHALL skip this step

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

### Requirement: Uninstall livery
The system SHALL support removing an installed livery by deleting its folder from the Airplanes directory and removing its INI file from the PMDG work folder.

#### Scenario: Successful uninstall
- **WHEN** the user selects an installed livery and triggers uninstall
- **THEN** the system deletes the livery's subfolder from `SimObjects\Airplanes\` and deletes the corresponding `{atcId}.ini` from the PMDG work folder

#### Scenario: Confirm before uninstall
- **WHEN** the user triggers uninstall
- **THEN** the system SHALL prompt for confirmation before deleting files
