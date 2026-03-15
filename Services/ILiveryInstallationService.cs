namespace LiveryManager.Services;

using LiveryManager.Models;

public interface ILiveryInstallationService
{
    /// <summary>
    /// Install a livery from a ZIP file. Returns the installed Livery info.
    /// </summary>
    Task<Livery> InstallLiveryAsync(string zipFilePath, string airplanesPath, string workFolderPath);

    /// <summary>
    /// Uninstall a livery by removing its folder and INI file.
    /// </summary>
    Task UninstallLiveryAsync(Livery livery, string workFolderPath);
}
