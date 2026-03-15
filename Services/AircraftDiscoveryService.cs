namespace LiveryManager.Services;

using System.IO;
using LiveryManager.Models;

public class AircraftDiscoveryService : IAircraftDiscoveryService
{
    public List<AircraftPackage> DiscoverAircraftPackages(string communityPath)
    {
        if (string.IsNullOrEmpty(communityPath) || !Directory.Exists(communityPath))
            return [];

        var packages = new List<AircraftPackage>();

        foreach (var directory in Directory.GetDirectories(communityPath))
        {
            var dirName = Path.GetFileName(directory);

            if (dirName.StartsWith("pmdg-aircraft-") && dirName.EndsWith("-liveries"))
            {
                var baseName = dirName[..^"-liveries".Length];

                packages.Add(new AircraftPackage
                {
                    Name = baseName,
                    LiveriesPath = directory,
                    BasePath = Path.Combine(communityPath, baseName)
                });
            }
        }

        packages.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        return packages;
    }
}
