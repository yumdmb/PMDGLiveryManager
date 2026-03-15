namespace LiveryManager.Services;

using LiveryManager.Models;

public interface IConfigService
{
    AppConfig LoadConfig();
    void SaveConfig(AppConfig config);
}
