using System.Text;

using Microsoft.Extensions.Logging;

namespace PaperClaw;

internal sealed class LibraryWriter
{
    private readonly string _libraryPath;
    private readonly ILogger<LibraryWriter> _logger;

    internal LibraryWriter(string libraryPath, ILogger<LibraryWriter> logger)
    {
        _libraryPath = libraryPath;
        _logger = logger;
    }

    internal async Task WriteAsync(
        EmailMessage email,
        PdfAttachment attachment,
        ClassificationResult classification,
        PdfContent content)
    {
        var categoryDir = Path.Combine(_libraryPath, classification.CategorySlug);
        Directory.CreateDirectory(categoryDir);

        var baseSlug = SlugGenerator.Generate(email.Subject, email.Date, email.Sender);
        var slug = ResolveSlug(categoryDir, baseSlug);
        if (slug != baseSlug)
            _logger.LogDebug("Slug collision for '{Base}', resolved to '{Resolved}'", baseSlug, slug);

        var pdfPath = Path.Combine(categoryDir, $"{slug}.pdf");
        var mdPath = Path.Combine(categoryDir, $"{slug}.md");

        await File.WriteAllBytesAsync(pdfPath, attachment.Bytes);
        _logger.LogInformation("Saved {Category}/{Slug}.pdf", classification.CategorySlug, slug);

        var markdown = TranscriptFormatter.Format(email, classification, content);
        await File.WriteAllTextAsync(mdPath, markdown, Encoding.UTF8);
        _logger.LogInformation("Saved {Category}/{Slug}.md", classification.CategorySlug, slug);
    }

    private static string ResolveSlug(string categoryDir, string baseSlug)
    {
        if (!File.Exists(Path.Combine(categoryDir, $"{baseSlug}.pdf")))
        {
            return baseSlug;
        }

        for (var i = 2; i <= 99; i++)
        {
            var candidate = $"{baseSlug}-{i}";
            if (!File.Exists(Path.Combine(categoryDir, $"{candidate}.pdf")))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            $"Cannot find a unique filename for slug '{baseSlug}' in '{categoryDir}' after 99 attempts.");
    }
}
