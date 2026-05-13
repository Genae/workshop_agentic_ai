using System.Security.Cryptography;

namespace PaperClaw;

internal sealed class ImportedHashStore
{
    private readonly string _filePath;

    internal ImportedHashStore(string libraryPath)
    {
        _filePath = Path.Combine(libraryPath, ".imported-hashes");
    }

    internal async Task<HashSet<string>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lines = await File.ReadAllLinesAsync(_filePath);
        var hashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                hashes.Add(line.Trim());
        }

        return hashes;
    }

    internal Task AddAsync(string hash) =>
        File.AppendAllTextAsync(_filePath, $"{hash}\n");

    internal static string ComputeHash(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
