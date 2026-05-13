namespace PaperClaw;

internal sealed class ProcessedUidStore
{
    private readonly string _filePath;

    internal ProcessedUidStore(string libraryPath)
    {
        _filePath = Path.Combine(libraryPath, ".processed-uids");
    }

    internal async Task<IReadOnlySet<uint>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new HashSet<uint>();
        }

        var lines = await File.ReadAllLinesAsync(_filePath);
        var uids = new HashSet<uint>();
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line) && uint.TryParse(line.Trim(), out var uid))
            {
                uids.Add(uid);
            }
        }

        return uids;
    }

    internal Task AddAsync(uint uid)
    {
        return File.AppendAllTextAsync(_filePath, $"{uid}\n");
    }
}
