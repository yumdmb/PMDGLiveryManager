namespace LiveryManager.Models;

public sealed class LiveryInstallOutcome
{
    public string ZipFilePath { get; set; } = string.Empty;

    public string ZipFileName { get; set; } = string.Empty;

    public LiveryInstallOutcomeStatus Status { get; set; }

    public string Message { get; set; } = string.Empty;
}
