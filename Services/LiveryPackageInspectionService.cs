namespace LiveryManager.Services;

using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LiveryManager.Models;

public class LiveryPackageInspectionService : ILiveryPackageInspectionService
{
    private const long MaxTextEntryLength = 1_000_000;

    private static readonly Regex PmdgAircraftRegex = new(@"pmdg-aircraft-[\w-]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Dictionary<string, string> AliasTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        ["b736"] = "pmdg-aircraft-736",
        ["737-600"] = "pmdg-aircraft-736",
        ["737600"] = "pmdg-aircraft-736",
        ["b737"] = "pmdg-aircraft-737",
        ["737-700"] = "pmdg-aircraft-737",
        ["737-800"] = "pmdg-aircraft-738",
        ["737800"] = "pmdg-aircraft-738",
        ["b738"] = "pmdg-aircraft-738",
        ["737-900"] = "pmdg-aircraft-739",
        ["b739"] = "pmdg-aircraft-739",
        ["777-300"] = "pmdg-aircraft-77w",
        ["b77w"] = "pmdg-aircraft-77w",
        ["777-200"] = "pmdg-aircraft-772",
        ["b772"] = "pmdg-aircraft-772",
        ["77er"] = "pmdg-aircraft-772",
        ["777-200lr"] = "pmdg-aircraft-77l",
        ["b77l"] = "pmdg-aircraft-77l",
        ["777-200f"] = "pmdg-aircraft-77f",
        ["b77f"] = "pmdg-aircraft-77f"
    };

    private static readonly string[] TextExtensions = { ".json", ".cfg", ".ini", ".txt" };

    public async Task<LiveryPackageInspectionResult> InspectPackageAsync(string zipFilePath, string targetAircraft)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
            throw new ArgumentException("ZIP file path must be provided.", nameof(zipFilePath));

        if (string.IsNullOrWhiteSpace(targetAircraft))
            throw new ArgumentException("Target aircraft identifier must be provided.", nameof(targetAircraft));

        return await Task.Run(() => InspectPackageCore(zipFilePath, targetAircraft));
    }

    private static LiveryPackageInspectionResult InspectPackageCore(string zipFilePath, string targetAircraft)
    {
        if (!File.Exists(zipFilePath))
            throw new FileNotFoundException("Livery ZIP file not found.", zipFilePath);

        string? atcId = null;
        string? detectedAircraft = null;
        var normalizedTargetAircraft = NormalizeAircraftToken(targetAircraft);

        using var archive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name) || entry.Length == 0)
                continue;

            if (entry.Length > MaxTextEntryLength)
                continue;

            var fileName = Path.GetFileName(entry.Name);
            var content = ReadEntryText(entry);
            if (content.Length == 0)
                continue;

            if (fileName.Equals("livery.json", StringComparison.OrdinalIgnoreCase))
            {
                if (atcId is null)
                {
                    atcId = ExtractAtcId(content);
                }

                if (detectedAircraft is null)
                {
                    detectedAircraft = ExtractProductPackage(content);
                }
            }

            if (atcId is null)
            {
                atcId = ExtractAtcId(content);
            }

            if (detectedAircraft is null)
            {
                detectedAircraft = FindAircraftToken(content);
            }

            if (atcId is not null && detectedAircraft is not null)
            {
                break;
            }
        }

        var (status, message) = DetermineStatus(normalizedTargetAircraft, detectedAircraft);

        return new LiveryPackageInspectionResult
        {
            ZipFilePath = zipFilePath,
            TargetAircraft = normalizedTargetAircraft,
            AtcId = string.IsNullOrWhiteSpace(atcId) ? null : atcId,
            DetectedAircraft = detectedAircraft,
            Status = status,
            ValidationMessage = message
        };
    }

    private static string ReadEntryText(ZipArchiveEntry entry)
    {
        var extension = Path.GetExtension(entry.Name);
        if (extension.Length == 0 || !IsTextExtension(extension))
            return string.Empty;

        using var stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static bool IsTextExtension(string extension)
    {
        foreach (var candidate in TextExtensions)
        {
            if (extension.Equals(candidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string? ExtractAtcId(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("atcId", out var element))
            {
                var jsonAtcId = element.GetString();
                if (!string.IsNullOrWhiteSpace(jsonAtcId))
                {
                    return jsonAtcId;
                }
            }
        }
        catch { /* Parsing failed — leave atcId null */ }

        using var reader = new StringReader(content);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var cleanLine = line.Trim().Trim('"', '\'', ',');
            if (!cleanLine.StartsWith("atc_id", StringComparison.OrdinalIgnoreCase)
                && !cleanLine.StartsWith("registration", StringComparison.OrdinalIgnoreCase)
                && !cleanLine.StartsWith("tailnumber", StringComparison.OrdinalIgnoreCase)
                && !cleanLine.StartsWith("tail_number", StringComparison.OrdinalIgnoreCase)
                && !cleanLine.StartsWith("atcid", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = cleanLine.Split(['=', ':'], 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            var candidateValue = parts[1].Trim().Trim('"', '\'', ',');
            if (!string.IsNullOrWhiteSpace(candidateValue))
            {
                return candidateValue;
            }
        }

        return null;
    }

    private static string? ExtractProductPackage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (!doc.RootElement.TryGetProperty("productPackage", out var element))
            {
                return null;
            }

            var productPackage = element.GetString();
            if (string.IsNullOrWhiteSpace(productPackage))
            {
                return null;
            }

            var normalizedProductPackage = NormalizeAircraftToken(productPackage);
            if (normalizedProductPackage.StartsWith("pmdg-aircraft-", StringComparison.Ordinal))
            {
                return normalizedProductPackage;
            }

            var match = PmdgAircraftRegex.Match(normalizedProductPackage);
            return match.Success
                ? NormalizeAircraftToken(match.Value)
                : null;
        }
        catch
        {
            // Parsing failed — fall back to token scanning.
            return null;
        }
    }

    private static string? FindAircraftToken(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var match = PmdgAircraftRegex.Match(content);
        if (match.Success)
        {
            return NormalizeAircraftToken(match.Value);
        }

        foreach (var (alias, canonical) in AliasTokens)
        {
            if (content.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return canonical;
            }
        }

        return null;
    }

    private static (LiveryPackageValidationStatus Status, string? Message) DetermineStatus(string targetAircraft, string? detectedAircraft)
    {
        if (detectedAircraft is null)
        {
            return (LiveryPackageValidationStatus.Unknown, "The package aircraft could not be determined.");
        }

        if (string.Equals(detectedAircraft, targetAircraft, StringComparison.OrdinalIgnoreCase))
        {
            return (LiveryPackageValidationStatus.Match, null);
        }

        return (LiveryPackageValidationStatus.Mismatch,
            $"Package targets '{detectedAircraft}' but '{targetAircraft}' is selected.");
    }

    private static string NormalizeAircraftToken(string aircraftToken)
    {
        var normalized = aircraftToken.Trim().ToLowerInvariant();

        return normalized.EndsWith("-liveries", StringComparison.Ordinal)
            ? normalized[..^"-liveries".Length]
            : normalized;
    }
}
