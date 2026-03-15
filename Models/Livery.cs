namespace LiveryManager.Models;

public class Livery
{
    public string FolderName { get; set; } = string.Empty;

    public string? AtcId { get; set; }

    public string FolderPath { get; set; } = string.Empty;

    public bool IsValid { get; set; }
}
