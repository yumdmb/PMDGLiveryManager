namespace LiveryManager.Services;

using LiveryManager.Models;

public interface ILiveryPackageInspectionService
{
    /// <summary>
    /// Inspect a livery ZIP to detect target aircraft and metadata before installation.
    /// </summary>
    Task<LiveryPackageInspectionResult> InspectPackageAsync(string zipFilePath, string targetAircraft);
}
