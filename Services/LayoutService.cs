namespace LiveryManager.Services;

using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

public class LayoutService : ILayoutService
{
    private static readonly JsonSerializerOptions LayoutJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public async Task RegenerateLayoutAsync(string liveryPackagePath)
    {
        await Task.Run(() =>
        {
            // Try safe-move approach first to avoid long path issues during enumeration.
            string? tempPath = null;
            try
            {
                string pathRoot = Path.GetPathRoot(liveryPackagePath)!;
                tempPath = Path.Combine(pathRoot, "_LM_TEMP");

                Directory.Move(liveryPackagePath, tempPath);
            }
            catch
            {
                // Move failed (files locked, cross-drive, etc.)
                tempPath = null;
            }

            if (tempPath is not null)
            {
                try
                {
                    GenerateLayout(tempPath);
                }
                finally
                {
                    Directory.Move(tempPath, liveryPackagePath);
                }
            }
            else
            {
                // Fallback: prepend \\?\ extended-length prefix to bypass MAX_PATH in-place.
                string extPath = liveryPackagePath.StartsWith(@"\\?\", StringComparison.Ordinal)
                    ? liveryPackagePath
                    : @"\\?\" + liveryPackagePath;
                GenerateLayout(extPath);
            }
        });
    }

    /// <summary>
    /// Strips the <c>\\?\</c> extended-length path prefix before passing to <see cref="Uri"/>,
    /// which does not understand the Win32 extended prefix.
    /// </summary>
    private static string NormalPath(string path) =>
        path.StartsWith(@"\\?\", StringComparison.Ordinal) ? path[4..] : path;

    private static void GenerateLayout(string packagePath)
    {
        string normalPackagePath = NormalPath(packagePath);
        var baseUri = new Uri(normalPackagePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
        var entries = new List<LayoutEntry>();

        foreach (string filePath in Directory.EnumerateFiles(packagePath, "*", SearchOption.AllDirectories))
        {
            var fileUri = new Uri(NormalPath(filePath));
            string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString());

            if (IsExcludedFromLayout(relativePath))
                continue;

            var info = new FileInfo(filePath);
            entries.Add(new LayoutEntry
            {
                path = relativePath,
                size = info.Length,
                date = info.LastWriteTimeUtc.ToFileTimeUtc()
            });
        }

        var root = new LayoutRoot { content = entries };
        string jsonString = JsonSerializer.Serialize(root, LayoutJsonOptions);
        jsonString = jsonString.Replace("\r\n", "\n");

        string layoutPath = Path.Combine(packagePath, "layout.json");
        File.WriteAllText(layoutPath, jsonString, Utf8NoBom);

        // Update manifest.json total_package_size if applicable.
        UpdateManifestTotalSize(packagePath);
    }

    private static bool IsExcludedFromLayout(string relativePath)
    {
        if (relativePath.Equals("layout.json", StringComparison.OrdinalIgnoreCase))
            return true;
        if (relativePath.Equals("manifest.json", StringComparison.OrdinalIgnoreCase))
            return true;
        if (relativePath.Equals("MSFSLayoutGenerator.exe", StringComparison.OrdinalIgnoreCase))
            return true;
        if (relativePath.StartsWith("_CVT_", StringComparison.OrdinalIgnoreCase))
            return true;
        if (relativePath.Contains("/_CVT_", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static void UpdateManifestTotalSize(string packagePath)
    {
        string manifestPath = Path.Combine(packagePath, "manifest.json");
        if (!File.Exists(manifestPath))
            return;

        string manifestText = File.ReadAllText(manifestPath);

        using var doc = JsonDocument.Parse(manifestText);
        if (!doc.RootElement.TryGetProperty("total_package_size", out JsonElement sizeElement))
            return;

        string? oldValue = sizeElement.GetString();
        if (oldValue is null)
            return;

        // Calculate total size: all files except _CVT_* and MSFSLayoutGenerator.exe,
        // including manifest.json itself and the freshly written layout.json.
        string normalPackagePath = NormalPath(packagePath);
        var baseUri = new Uri(normalPackagePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
        long totalSize = 0;

        foreach (string filePath in Directory.EnumerateFiles(packagePath, "*", SearchOption.AllDirectories))
        {
            var fileUri = new Uri(NormalPath(filePath));
            string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString());

            if (relativePath.StartsWith("_CVT_", StringComparison.OrdinalIgnoreCase)
                || relativePath.Contains("/_CVT_", StringComparison.OrdinalIgnoreCase))
                continue;

            if (relativePath.Equals("MSFSLayoutGenerator.exe", StringComparison.OrdinalIgnoreCase))
                continue;

            totalSize += new FileInfo(filePath).Length;
        }

        string newValue = totalSize.ToString().PadLeft(20, '0');
        manifestText = manifestText.Replace(oldValue, newValue);

        // Manifest uses CRLF line endings.
        manifestText = manifestText.Replace("\r\n", "\n").Replace("\n", "\r\n");
        File.WriteAllText(manifestPath, manifestText, Utf8NoBom);
    }

    private sealed class LayoutEntry
    {
        public string path { get; set; } = "";
        public long size { get; set; }
        public long date { get; set; }
    }

    private sealed class LayoutRoot
    {
        public List<LayoutEntry> content { get; set; } = new();
    }
}
