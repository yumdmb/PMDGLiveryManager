## ADDED Requirements

### Requirement: Resolve Community folder path by store type
The system SHALL resolve the MSFS Community folder path based on the selected store type.

#### Scenario: Microsoft Store path
- **WHEN** store type is "Microsoft Store"
- **THEN** the Community folder path is `%LOCALAPPDATA%\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\Packages\Community\`

#### Scenario: Steam path
- **WHEN** store type is "Steam"
- **THEN** the Community folder path is `%APPDATA%\Microsoft Flight Simulator\Packages\Community\`

#### Scenario: Custom path
- **WHEN** store type is "Custom"
- **THEN** the system SHALL use the user-specified Community folder path (stored in config or selected via folder browser dialog)

#### Scenario: Resolved path does not exist
- **WHEN** the resolved Community folder does not exist on disk
- **THEN** the system SHALL report an error indicating MSFS may not be installed for the selected store type

### Requirement: Resolve PMDG work folder path by store type and aircraft
The system SHALL resolve the PMDG aircraft work folder path based on the selected store type and aircraft package name.

#### Scenario: Microsoft Store work path
- **WHEN** store type is "Microsoft Store" and aircraft is `pmdg-aircraft-77f`
- **THEN** the work folder path is `%LOCALAPPDATA%\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalState\Packages\pmdg-aircraft-77f\work\Aircraft\`

#### Scenario: Steam work path
- **WHEN** store type is "Steam" and aircraft is `pmdg-aircraft-77f`
- **THEN** the work folder path is `%APPDATA%\Microsoft Flight Simulator\Packages\pmdg-aircraft-77f\work\Aircraft\`

#### Scenario: Custom work path
- **WHEN** store type is "Custom" and the custom Community path is `D:\MSFS\Community`
- **THEN** the work folder path SHALL be resolved relative to the Community folder's parent: `D:\MSFS\Packages\pmdg-aircraft-77f\work\Aircraft\`
- **NOTE** If the work folder cannot be inferred (non-standard structure), the system SHALL log a warning and skip INI copy

### Requirement: Discover PMDG aircraft packages
The system SHALL scan the Community folder for directories matching `pmdg-aircraft-*-liveries` and derive the base aircraft package name by removing the `-liveries` suffix.

#### Scenario: Multiple aircraft packages present
- **WHEN** the Community folder contains `pmdg-aircraft-77f-liveries` and `pmdg-aircraft-738-liveries`
- **THEN** the system returns two aircraft entries: `pmdg-aircraft-77f` and `pmdg-aircraft-738`

#### Scenario: No matching packages
- **WHEN** no directories matching `pmdg-aircraft-*-liveries` exist in the Community folder
- **THEN** the system returns an empty list

### Requirement: Resolve liveries Airplanes folder
The system SHALL derive the path to the `SimObjects\Airplanes` directory within a livery package for a given aircraft.

#### Scenario: Standard path
- **WHEN** aircraft is `pmdg-aircraft-77f`
- **THEN** the Airplanes path is `<Community>\pmdg-aircraft-77f-liveries\SimObjects\Airplanes\`
