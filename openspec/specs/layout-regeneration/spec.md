## ADDED Requirements

### Requirement: Regenerate layout.json for livery package
The system SHALL regenerate the `layout.json` file at the root of the livery package folder after any install or uninstall operation. The generated layout.json SHALL be identical in format to the output of [MSFSLayoutGenerator](https://github.com/HughesMDflyer4/MSFSLayoutGenerator).

#### Scenario: Regenerate after install
- **WHEN** a livery installation completes successfully
- **THEN** the system regenerates `<Community>\pmdg-aircraft-<type>-liveries\layout.json` listing all files in the package

#### Scenario: Regenerate after uninstall
- **WHEN** a livery uninstall completes successfully
- **THEN** the system regenerates `layout.json` reflecting the removed files

### Requirement: Layout.json content entry format
Each entry in the `content` array SHALL have exactly three fields: `path` (string), `size` (long), and `date` (long). The `path` SHALL be the file path relative to the package root using forward slashes (URI-style via `Uri.MakeRelativeUri`). The `size` SHALL be the file size in bytes. The `date` SHALL be the file's last modified time in UTC as a Windows FILETIME (100-nanosecond intervals since January 1, 1601 UTC), obtained via `FileInfo.LastWriteTimeUtc.ToFileTimeUtc()`.

#### Scenario: Correct format output
- **WHEN** the livery package contains `SimObjects/Airplanes/MyLivery/aircraft.cfg` (1024 bytes, last modified 2025-01-15T10:00:00Z)
- **THEN** the layout.json content array includes an entry with `"path": "SimObjects/Airplanes/MyLivery/aircraft.cfg"`, `"size": 1024`, and `"date": <Windows FILETIME value>`

### Requirement: Excluded files
The layout.json content array SHALL exclude the following files: `layout.json` (self), `manifest.json`, `MSFSLayoutGenerator.exe`, and any file whose relative path starts with `_CVT_` (case-insensitive). All other files SHALL be included.

#### Scenario: Self-exclusion
- **WHEN** layout.json is regenerated
- **THEN** the file `layout.json` does not appear in the `content` array

#### Scenario: Manifest exclusion
- **WHEN** a `manifest.json` exists in the package root
- **THEN** it does not appear in the `content` array

#### Scenario: Generator exe exclusion
- **WHEN** `MSFSLayoutGenerator.exe` exists in the package root
- **THEN** it does not appear in the `content` array

#### Scenario: CVT folder exclusion
- **WHEN** files exist under a `_CVT_` prefixed path
- **THEN** those files do not appear in the `content` array

### Requirement: JSON serialization format
The layout.json SHALL be written with indented formatting (`WriteIndented = true`), `UnsafeRelaxedJsonEscaping` (to preserve non-ASCII characters like airline names without escaping), and LF line endings (replace `\r\n` with `\n`).

#### Scenario: Non-ASCII characters preserved
- **WHEN** a file path contains non-ASCII characters (e.g., Korean airline name)
- **THEN** the characters are written as-is in the JSON, not escaped to `\uXXXX`

#### Scenario: LF line endings
- **WHEN** layout.json is written
- **THEN** the file uses LF (`\n`) line endings, not CRLF (`\r\n`)

### Requirement: Update manifest.json total_package_size
If a `manifest.json` exists in the package root and contains a `total_package_size` property, the system SHALL update its value to reflect the current total file size of the package in bytes. The value SHALL be a string zero-padded to 20 characters. The total size SHALL include all files except `_CVT_*` and `MSFSLayoutGenerator.exe`, plus the size of `manifest.json` itself and the freshly written `layout.json`. The manifest.json SHALL be written with CRLF line endings.

#### Scenario: Manifest total_package_size updated
- **WHEN** layout.json is regenerated and manifest.json contains `total_package_size`
- **THEN** the `total_package_size` value is updated to the zero-padded total (e.g., `"00000000000012345678"`)

#### Scenario: No manifest.json
- **WHEN** no manifest.json exists in the package root
- **THEN** the system skips the manifest update step without error
