# AGENTS.md

Guidance for AI coding agents working in this repository.

## Project Overview

WPF desktop application (.NET 10, Windows only) that automates PMDG livery
installation for Microsoft Flight Simulator 2020. MVVM architecture using
`CommunityToolkit.Mvvm 8.*`. No test projects, no CI pipeline.

---

## Build & Run Commands

```bash
dotnet restore
dotnet build LiveryManager.sln          # debug (default feedback loop)
dotnet build LiveryManager.sln -c Release

dotnet run --project LiveryManager.csproj   # Windows only – WPF requires display

# Self-contained single-file publish
dotnet publish LiveryManager.csproj -c Release -r win-x64 \
  --self-contained true -p:PublishSingleFile=true
# Output: bin\Release\net10.0-windows\win-x64\publish\
```

**Tests:** No test projects exist. `dotnet test` finds nothing.  
**Linting:** No `.editorconfig`, StyleCop, or linting toolchain. Nullable
compiler enforcement is the primary guardrail.

---

## Project Settings (LiveryManager.csproj)

| Setting | Value |
|---|---|
| Target framework | `net10.0-windows` |
| Nullable reference types | `enable` (strictly enforced) |
| Implicit usings | `enable` (System, Linq, IO, Threading.Tasks, etc. in scope globally) |
| WPF | `<UseWPF>true</UseWPF>` |
| Output type | `WinExe` |
| Key dependency | `CommunityToolkit.Mvvm 8.*` |

---

## Code Style

### Namespaces & Usings

File-scoped namespace declarations (no block braces). Place `using` directives
**after** the `namespace` line — this is the dominant pattern in services and
models and new code should follow it:

```csharp
namespace LiveryManager.Services;

using System.IO;
using LiveryManager.Models;
```

### Naming

| Symbol | Convention | Example |
|---|---|---|
| Classes, interfaces, enums, properties, methods | `PascalCase` | `PathService`, `IPathService`, `AtcId` |
| Private fields | `_camelCase` | `_pathService`, `_suppressStoreTypeChanged` |
| Local variables & parameters | `camelCase` | `communityPath`, `storeType` |
| Async methods | `PascalCase` + `Async` suffix | `InstallLiveryAsync`, `DiscoverAircraftAsync` |
| Interfaces | `I` prefix | `ILayoutService`, `IConfigService` |
| Private static helpers | `PascalCase` | `NormalPath`, `IsExcludedFromLayout` |

Private core helpers that do not manage `IsLoading` themselves may omit the
`Async` suffix only if they are internal implementation details (e.g.,
`RefreshLiveriesCore`); document this explicitly with an XML `<summary>`.

### Null Handling

Nullable reference types are **enabled** — all code must be null-safe.

```csharp
if (atcId is not null) { ... }                          // pattern matching, not != null
return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
var dir = Path.GetDirectoryName(ConfigFilePath)!;       // ! only when provably non-null
public string FolderName { get; set; } = string.Empty; // init to Empty, not null
public string? AtcId { get; set; }                      // nullable for optional values
```

### Collections

```csharp
List<Livery> GetInstalledLiveries(string path);             // service return types
ObservableCollection<AircraftPackage> _packages = [];       // ViewModel bindings
public static IReadOnlyList<StoreType> StoreTypes { get; } = [...]; // static data
return [];   // C# 12 empty collection expression
```

### Async Patterns

```csharp
// Always async Task — never async void
_ = DiscoverAircraftAsync(lastAircraft);   // fire-and-forget discard idiom

var packages = await Task.Run(() =>        // all blocking I/O via Task.Run
    _aircraftDiscoveryService.DiscoverAircraftPackages(communityPath));

// Standard ViewModel command with IsLoading guard
IsLoading = true;
StatusMessage = null;
try   { /* work */ }
catch (Exception ex) { StatusMessage = $"Operation failed: {ex.Message}"; }
finally { IsLoading = false; }
```

### Error Handling

- **Service layer** throws typed exceptions (`ArgumentException`,
  `InvalidOperationException`, `FileNotFoundException`). Never silently swallow
  unless the failure is truly non-fatal and a comment explains why.
- **ViewModel layer** catches `Exception` generically and surfaces the message
  via `StatusMessage`. No error dialogs — all feedback through `StatusMessage`.
- Silent catch for non-critical parse fallbacks requires an explanatory comment:
  ```csharp
  catch { /* Parsing failed — leave atcId null and isValid false */ }
  ```
- Use `Debug.WriteLine(...)` for non-fatal diagnostic output in services.

### CommunityToolkit.Mvvm Patterns

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    [NotifyCanExecuteChangedFor(nameof(InstallLiveryCommand))]
    private bool _isLoading;

    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task InstallLiveryAsync() { ... }
    private bool CanInstall() => SelectedAircraft != null && !IsLoading;

    partial void OnSelectedStoreTypeChanged(StoreType? value) { ... }
}
```

Use guard flags (`_suppressX = true; Prop = val; _suppressX = false;`) to
bypass change handlers during programmatic initialization. **Do not** access
`[ObservableProperty]` backing fields directly in other methods (MVVMTK0034).

### Expressions & Brevity

```csharp
public override string ToString() => Name;
public bool IsEmptyState => SelectedAircraft != null && !IsLoading && Liveries.Count == 0;

return storeType switch
{
    StoreType.Steam          => Path.Combine(...),
    StoreType.MicrosoftStore => Path.Combine(...),
    StoreType.Custom         => customPath ?? throw new ArgumentException(...),
    _                        => throw new ArgumentOutOfRangeException(nameof(storeType))
};
```

### Strings & Comments

```csharp
string.Empty           // not ""
string.IsNullOrEmpty() // not IsNullOrWhiteSpace
$"interpolated {x}"   // for formatting
StringComparison.Ordinal  // explicit on all StartsWith/Contains/Equals calls
```

- XML `/// <summary>` on all **interface members** and significant **ViewModel
  methods**. Skip trivial auto-properties.
- Inline `//` for multi-step logic (number the steps).
- `// ── Section ──` ASCII banners to separate regions in large partial classes.

---

## Architecture Notes

- **Manual DI:** services instantiated in `App.OnStartup`, injected into
  `MainViewModel` via constructor. No IoC container.
- **Thin ViewModels:** all file-system logic in services behind interfaces.
  ViewModels orchestrate services and manage UI state only.
- **Services code to interfaces:** fields typed as `IPathService`, not
  `PathService`, to allow future testing/mocking.
- **Single window:** `MainWindow` + `MainViewModel` is the entire UI surface.
- **Long-path awareness:** `app.manifest` enables `longPathAware`. Services use
  the safe-move strategy (`C:\_LM_TEMP\`) and `\\?\` prefix fallbacks for MSFS
  Community folder paths that exceed 260 characters.
