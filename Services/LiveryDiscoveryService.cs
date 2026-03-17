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
            var thumbnailPath = FindThumbnail(directory);

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
                IsValid = isValid,
                ThumbnailPath = thumbnailPath
            });
        }

        liveries.Sort((a, b) => string.Compare(a.FolderName, b.FolderName, StringComparison.Ordinal));

        return liveries;
    }

    private static string? FindThumbnail(string liveryDirectory)
    {
        if (string.IsNullOrEmpty(liveryDirectory))
            return null;

        string[] subFolders = { string.Empty, "thumbnail", "texture" };
        string[] extensions = { "jpg", "png", "jpeg" };

        var candidates = new List<string>();

        foreach (var sub in subFolders)
        {
            foreach (var extension in extensions)
            {
                var candidate = string.IsNullOrEmpty(sub)
                    ? Path.Combine(liveryDirectory, $"thumbnail.{extension}")
                    : Path.Combine(Path.Combine(liveryDirectory, sub), $"thumbnail.{extension}");

                candidates.Add(candidate);
            }
        }

        try
        {
            foreach (var textureDir in Directory.EnumerateDirectories(liveryDirectory, "texture.*"))
            {
                foreach (var extension in extensions)
                {
                    candidates.Add(Path.Combine(textureDir, $"thumbnail.{extension}"));
                }
            }
        }
        catch
        {
            // Directory enumeration failed — ignore and continue
        }

        foreach (var candidate in candidates)
        {
            if (TryGetReadableThumbnailPath(candidate, out var readableThumbnailPath))
            {
                return readableThumbnailPath;
            }
        }

        return null;
    }

    private static bool TryGetReadableThumbnailPath(string candidatePath, out string readableThumbnailPath)
    {
        readableThumbnailPath = string.Empty;

        if (!File.Exists(candidatePath))
        {
            return false;
        }

        try
        {
            using var stream = File.Open(candidatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            readableThumbnailPath = candidatePath;
            return stream.Length >= 0;
        }
        catch
        {
            // Thumbnail is unreadable — fall back to placeholder state
            return false;
        }
    }
}
