namespace LiveryManager.Services;

using LiveryManager.Models;

public interface IPathService
{
    string GetCommunityFolderPath(StoreType storeType, string? customPath = null);
    string GetPmdgWorkFolderPath(StoreType storeType, string aircraftName, string? customPath = null);
    string GetAirplanesFolderPath(string communityPath, string aircraftName);
    bool ValidatePath(string path, out string errorMessage);
}
