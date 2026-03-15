namespace LiveryManager.Models;

public class AircraftPackage
{
    public string Name { get; set; } = string.Empty;

    public string LiveriesPath { get; set; } = string.Empty;

    public string BasePath { get; set; } = string.Empty;

    public override string ToString() => Name;
}
