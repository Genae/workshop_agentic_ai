using System.Text.RegularExpressions;

namespace PaperClaw;

internal sealed class LibrarySearch
{
    private const int MaxSearchResults = 20;
    private const int ExcerptLength = 120;
    private static readonly Regex SafeChars = new(@"[^a-z0-9\-]", RegexOptions.Compiled);

    private readonly string _libraryPath;

    internal LibrarySearch(string libraryPath)
    {
        _libraryPath = libraryPath;
    }

    internal string ListCategories()
    {
        if (!Directory.Exists(_libraryPath))
        {
            return "(library is empty)";
        }

        var categories = Directory.GetDirectories(_libraryPath)
            .Select(Path.GetFileName)
            .Where(n => n is not null)
            .Order()
            .ToList();

        return categories.Count == 0
            ? "(library is empty)"
            : string.Join("\n", categories);
    }

    internal string ListDocuments(string category)
    {
        var dir = Path.Combine(_libraryPath, Sanitise(category));
        if (!Directory.Exists(dir))
        {
            return "(no documents)";
        }

        var files = Directory.GetFiles(dir, "*.md")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Order()
            .ToList();

        return files.Count == 0
            ? "(no documents)"
            : string.Join("\n", files);
    }

    internal string ReadDocument(string category, string filename)
    {
        var path = Path.Combine(_libraryPath, Sanitise(category), $"{Sanitise(filename)}.md");
        return File.Exists(path)
            ? File.ReadAllText(path)
            : "Document not found.";
    }

    internal string SearchLibrary(string query)
    {
        if (!Directory.Exists(_libraryPath) || string.IsNullOrWhiteSpace(query))
        {
            return "No matches found.";
        }

        var results = new List<string>();

        foreach (var categoryDir in Directory.GetDirectories(_libraryPath).OrderBy(d => d))
        {
            var category = Path.GetFileName(categoryDir);
            foreach (var file in Directory.GetFiles(categoryDir, "*.md").OrderBy(f => f))
            {
                var stem = Path.GetFileNameWithoutExtension(file);
                foreach (var line in File.ReadAllLines(file))
                {
                    if (line.Contains(query, StringComparison.OrdinalIgnoreCase))
                    {
                        var excerpt = line.Length > ExcerptLength
                            ? line[..ExcerptLength]
                            : line;
                        results.Add($"{category}/{stem}: {excerpt}");

                        if (results.Count >= MaxSearchResults)
                        {
                            return string.Join("\n", results);
                        }

                        break;
                    }
                }
            }
        }

        return results.Count == 0
            ? "No matches found."
            : string.Join("\n", results);
    }

    private static string Sanitise(string input)
    {
        return SafeChars.Replace(input.ToLowerInvariant(), string.Empty);
    }
}
