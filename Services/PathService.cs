namespace LiveryManager.Services;

using System.IO;
using LiveryManager.Models;

public class PathService : IPathService
{
    public string GetCommunityFolderPath(StoreType storeType, string? customPath = null)
    {
        return storeType switch
        {
            StoreType.MicrosoftStore => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages",
                "Microsoft.FlightSimulator_8wekyb3d8bbwe",
                "LocalCache",
                "Packages",
                "Community"),

            StoreType.Steam => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft Flight Simulator",
                "Packages",
                "Community"),

            StoreType.Custom => string.IsNullOrEmpty(customPath)
                ? throw new ArgumentException("Custom path must be provided when StoreType is Custom.", nameof(customPath))
                : customPath,

            _ => throw new ArgumentOutOfRangeException(nameof(storeType), storeType, "Unknown store type.")
        };
    }

    public string GetPmdgWorkFolderPath(StoreType storeType, string aircraftName, string? customPath = null)
    {
        return storeType switch
        {
            StoreType.MicrosoftStore => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages",
                "Microsoft.FlightSimulator_8wekyb3d8bbwe",
                "LocalState",
                "Packages",
                aircraftName,
                "work",
                "Aircraft"),

            StoreType.Steam => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft Flight Simulator",
                "Packages",
                aircraftName,
                "work",
                "Aircraft"),

            StoreType.Custom => DeriveWorkFolderFromCustomPath(customPath, aircraftName),

            _ => throw new ArgumentOutOfRangeException(nameof(storeType), storeType, "Unknown store type.")
        };
    }

    public string GetAirplanesFolderPath(string communityPath, string aircraftName)
    {
        return Path.Combine(communityPath, aircraftName + "-liveries", "SimObjects", "Airplanes");
    }

    public bool ValidatePath(string path, out string errorMessage)
    {
        if (string.IsNullOrEmpty(path))
        {
            errorMessage = "Path cannot be empty.";
            return false;
        }

        if (!Directory.Exists(path))
        {
            errorMessage = $"Directory does not exist: {path}";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static string DeriveWorkFolderFromCustomPath(string? customPath, string aircraftName)
    {
        if (string.IsNullOrEmpty(customPath))
            return string.Empty;

        // Go one level up from whatever folder the user selected as the community folder,
        // then construct <parent>\<aircraftName>\work\Aircraft — matching the MSFS Packages layout.
        var parent = Path.GetDirectoryName(customPath);
        if (parent is null)
            return string.Empty;

        return Path.Combine(parent, aircraftName, "work", "Aircraft");
    }
}
