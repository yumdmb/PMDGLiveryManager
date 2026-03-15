namespace LiveryManager.Services;

public interface ILayoutService
{
    /// <summary>
    /// Regenerate layout.json for the given livery package folder.
    /// Uses safe-move strategy for long paths.
    /// </summary>
    Task RegenerateLayoutAsync(string liveryPackagePath);
}
