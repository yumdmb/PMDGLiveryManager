using System.Text.Json.Serialization;

namespace LiveryManager.Models;

public class AppConfig
{
    [JsonConverter(typeof(JsonStringEnumConverter<StoreType>))]
    public StoreType? StoreType { get; set; }

    public string? CustomCommunityPath { get; set; }

    public string? LastAircraft { get; set; }
}
