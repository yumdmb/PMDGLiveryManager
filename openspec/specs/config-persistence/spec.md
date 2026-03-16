## ADDED Requirements

### Requirement: Persist user settings to config file
The system SHALL save user settings to a JSON file at `%APPDATA%\LiveryManager\config.json`. Settings SHALL include: store type, custom community folder path (if applicable), and last selected aircraft package name.

#### Scenario: Save on store type change
- **WHEN** the user selects or changes the store type
- **THEN** the config file is updated with the new store type value

#### Scenario: Save on custom path selection
- **WHEN** the user selects a custom Community folder path via the folder browser
- **THEN** the config file is updated with the custom path

#### Scenario: Save on aircraft selection change
- **WHEN** the user selects a different aircraft package
- **THEN** the config file is updated with the last selected aircraft name

### Requirement: Restore settings on startup
The system SHALL load settings from the config file on application startup and restore the UI state accordingly.

#### Scenario: Config file exists with valid settings
- **WHEN** the application starts and `%APPDATA%\LiveryManager\config.json` exists with valid JSON
- **THEN** the store type selector is set to the saved value, the custom path is restored (if Custom), and the last selected aircraft is pre-selected

#### Scenario: Config file does not exist
- **WHEN** the application starts and no config file exists
- **THEN** the application starts with default state (no store type selected, no aircraft selected)

#### Scenario: Config file is corrupt or unreadable
- **WHEN** the config file exists but contains invalid JSON
- **THEN** the system SHALL log a warning, ignore the file, and start with default state

### Requirement: Config file format
The config file SHALL be a JSON object with the following structure:

```json
{
  "storeType": "Steam" | "MicrosoftStore" | "Custom",
  "customCommunityPath": "D:\\MSFS\\Community",
  "lastAircraft": "pmdg-aircraft-77f"
}
```

#### Scenario: Properties are optional
- **WHEN** a property is missing from the config JSON
- **THEN** the system SHALL use the default value for that property (null/empty)

### Requirement: Config directory creation
The system SHALL create the `%APPDATA%\LiveryManager\` directory if it does not exist before writing the config file.

#### Scenario: First run
- **WHEN** the config directory does not exist
- **THEN** the system creates the directory and writes the config file
