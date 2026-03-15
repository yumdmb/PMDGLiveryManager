namespace LiveryManager.Services;

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using LiveryManager.Models;

public class LiveryInstallationService : ILiveryInstallationService
{
    public async Task<Livery> InstallLiveryAsync(string zipFilePath, string airplanesPath, string workFolderPath)
    {
        return await Task.Run(() =>
        {
            // 1. Derive folder name from ZIP file name
            var folderName = Path.GetFileNameWithoutExtension(zipFilePath);

            // 2. Build destination path
            var destinationPath = Path.Combine(airplanesPath, folderName);

            // 3. If destination already exists, throw
            if (Directory.Exists(destinationPath))
            {
                throw new InvalidOperationException(
                    $"The folder '{folderName}' already exists in the airplanes directory.");
            }

            // 4. Extract ZIP to destination
            ZipFile.ExtractToDirectory(zipFilePath, destinationPath);

            // 5. Parse livery.json
            var liveryJsonPath = Path.Combine(destinationPath, "livery.json");

            if (!File.Exists(liveryJsonPath))
            {
                throw new FileNotFoundException("livery.json not found in the livery package");
            }

            var jsonContent = File.ReadAllText(liveryJsonPath);
            using var jsonDoc = JsonDocument.Parse(jsonContent);

            string? atcId = null;
            if (jsonDoc.RootElement.TryGetProperty("atcId", out var atcIdElement))
            {
                atcId = atcIdElement.GetString();
            }

            if (string.IsNullOrEmpty(atcId))
            {
                throw new InvalidOperationException("atcId field is missing from livery.json");
            }

            // 6. Rename options.ini to {atcId}.ini
            var optionsIniPath = Path.Combine(destinationPath, "options.ini");
            var renamedIniPath = Path.Combine(destinationPath, $"{atcId}.ini");

            if (!File.Exists(optionsIniPath))
            {
                Debug.WriteLine($"Warning: options.ini not found in '{destinationPath}'. Some liveries don't include it.");
            }
            else
            {
                File.Move(optionsIniPath, renamedIniPath);
            }

            // 7. Copy renamed INI to work folder
            if (File.Exists(renamedIniPath))
            {
                if (!Directory.Exists(workFolderPath))
                {
                    Debug.WriteLine($"Warning: Work folder '{workFolderPath}' does not exist. The user may not have flown this aircraft yet.");
                }
                else
                {
                    var workIniPath = Path.Combine(workFolderPath, $"{atcId}.ini");
                    File.Copy(renamedIniPath, workIniPath, overwrite: true);
                }
            }

            // 8. Return Livery object
            return new Livery
            {
                FolderName = folderName,
                AtcId = atcId,
                FolderPath = destinationPath,
                IsValid = true
            };
        });
    }

    public async Task UninstallLiveryAsync(Livery livery, string workFolderPath)
    {
        await Task.Run(() =>
        {
            // 1. Delete livery folder recursively
            if (Directory.Exists(livery.FolderPath))
            {
                Directory.Delete(livery.FolderPath, true);
            }

            // 2. Delete INI from work folder
            if (livery.AtcId is not null && Directory.Exists(workFolderPath))
            {
                var iniPath = Path.Combine(workFolderPath, $"{livery.AtcId}.ini");
                if (File.Exists(iniPath))
                {
                    File.Delete(iniPath);
                }
            }
        });
    }
}
