namespace LiveryManager.Services;

using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LiveryManager.Models;

public class ConfigService : IConfigService
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LiveryManager",
        "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AppConfig LoadConfig()
    {
        if (!File.Exists(ConfigFilePath))
            return new AppConfig();

        try
        {
            var json = File.ReadAllText(ConfigFilePath);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load config: {ex.Message}");
            return new AppConfig();
        }
    }

    public void SaveConfig(AppConfig config)
    {
        var directory = Path.GetDirectoryName(ConfigFilePath)!;
        Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigFilePath, json);
    }
}
