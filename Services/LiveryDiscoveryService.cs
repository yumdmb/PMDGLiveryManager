namespace LiveryManager.Services;

using System.IO;
using System.Text.Json;
using LiveryManager.Models;

public class LiveryDiscoveryService : ILiveryDiscoveryService
{
    public List<Livery> GetInstalledLiveries(string airplanesPath)
    {
        if (string.IsNullOrEmpty(airplanesPath) || !Directory.Exists(airplanesPath))
            return [];

        var liveries = new List<Livery>();

        foreach (var directory in Directory.GetDirectories(airplanesPath))
        {
            var folderName = Path.GetFileName(directory);
            string? atcId = null;
            var isValid = false;

            var liveryJsonPath = Path.Combine(directory, "livery.json");

            if (File.Exists(liveryJsonPath))
            {
                try
                {
                    var json = File.ReadAllText(liveryJsonPath);
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("atcId", out var atcIdElement))
                    {
                        atcId = atcIdElement.GetString();
                        isValid = atcId is not null;
                    }
                }
                catch
                {
                    // Parsing failed — leave atcId null and isValid false
                }
            }

            liveries.Add(new Livery
            {
                FolderName = folderName,
                AtcId = atcId,
                FolderPath = directory,
                IsValid = isValid
            });
        }

        liveries.Sort((a, b) => string.Compare(a.FolderName, b.FolderName, StringComparison.Ordinal));

        return liveries;
    }
}
