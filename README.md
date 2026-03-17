# PMDG Livery Manager

PMDG Livery Manager is a Windows desktop app that helps you install and remove third-party liveries for PMDG aircraft in Microsoft Flight Simulator 2020.

It handles the repetitive package steps for you, including updating `layout.json` and `manifest.json` after each install or uninstall.

## Features

- Detects the MSFS Community folder for Steam, Microsoft Store, or custom installs
- Finds installed PMDG livery packages automatically
- Installs livery ZIP files in one click
- Uninstalls installed liveries cleanly
- Regenerates `layout.json` and updates `manifest.json` automatically
- Saves your selected store type, custom path, and last aircraft

## Download

Download the latest Windows x64 release from the GitHub Releases page for this repository.

- Download the latest `win-x64` ZIP asset
- Extract the ZIP to a normal folder first
- Run `LiveryManager.exe`

If Windows SmartScreen appears, use `More info` and then `Run anyway` if you trust the release source.

## Requirements

- Windows 10 or Windows 11
- Microsoft Flight Simulator 2020
- At least one supported PMDG aircraft package installed

You do not need to install the .NET runtime if you use the self-contained x64 release build.

## How To Use

1. Start the app.
2. Choose your simulator install type: `Steam`, `Microsoft Store`, or `Custom`.
3. If you use `Custom`, browse to your Community folder.
4. Select the PMDG aircraft package you want to manage.
5. Click `Install Livery...` and choose the livery ZIP file.
6. Wait for the status message confirming the install completed.

To remove a livery:

1. Select the aircraft package.
2. Select the installed livery from the list.
3. Click `Uninstall`.
4. Confirm the removal.

## Notes

- The app automatically regenerates the MSFS package layout after each install or uninstall.
- Your settings are stored in `%APPDATA%\LiveryManager\config.json`.
- Extract release ZIP files before running the app. Do not run the executable directly from inside a ZIP.

## Build From Source

If you want to run or publish the app yourself:

```bash
dotnet restore
dotnet build LiveryManager.sln
dotnet run --project LiveryManager.csproj
```

To create a self-contained Windows x64 publish:

```bash
dotnet publish LiveryManager.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Published output:

```text
bin\Release\net10.0-windows\win-x64\publish\
```

## Documentation

- User guide: this file
- Technical overview: [ARCHITECTURE.md](ARCHITECTURE.md)

## Credits

- Credit to [HughesMDflyer4/MSFSLayoutGenerator](https://github.com/HughesMDflyer4/MSFSLayoutGenerator) for the MSFS layout generator code that informed the `layout.json` generation support in this project.
