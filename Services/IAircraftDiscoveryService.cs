namespace LiveryManager.Services;

using LiveryManager.Models;

public interface IAircraftDiscoveryService
{
    List<AircraftPackage> DiscoverAircraftPackages(string communityPath);
}
