namespace LiveryManager.Models;

public sealed class LiveryPackageInspectionResult
{
    public string ZipFilePath { get; init; } = string.Empty;

    public string TargetAircraft { get; init; } = string.Empty;

    public string? DetectedAircraft { get; init; }

    public string? AtcId { get; init; }

    public LiveryPackageValidationStatus Status { get; init; }

    public string? ValidationMessage { get; init; }

    public bool HasDetectedAircraft => DetectedAircraft is not null;

    public bool ShouldInstall => Status == LiveryPackageValidationStatus.Match;
}
