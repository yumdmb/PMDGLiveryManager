namespace LiveryManager.Services;

using LiveryManager.Models;

public interface ILiveryDiscoveryService
{
    List<Livery> GetInstalledLiveries(string airplanesPath);
}
