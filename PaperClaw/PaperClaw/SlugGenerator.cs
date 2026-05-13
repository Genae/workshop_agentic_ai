using System.Globalization;
using System.Text.RegularExpressions;

namespace PaperClaw;

internal static class SlugGenerator
{
    private static readonly Regex NonAlphanumeric = new("[^a-z0-9]+", RegexOptions.Compiled);

    internal static string Generate(string subject, DateTimeOffset date, string sender)
    {
        var domain = ExtractDomain(sender);
        var subjectSlug = NormalizeSubject(subject);
        var dateSlug = date.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        var parts = new[] { domain, dateSlug, subjectSlug }
            .Where(s => !string.IsNullOrEmpty(s));
        var slug = string.Join("-", parts);

        return string.IsNullOrEmpty(slug)
            ? $"document-{date:yyyy-MM-dd}"
            : slug;
    }

    private static string ExtractDomain(string sender)
    {
        var atIndex = sender.IndexOf('@');
        if (atIndex < 0)
        {
            return string.Empty;
        }

        var domain = sender[(atIndex + 1)..].TrimEnd('>', ' ');
        domain = domain.ToLowerInvariant();
        if (domain.StartsWith("www.", StringComparison.Ordinal))
        {
            domain = domain[4..];
        }

        var dotIndex = domain.IndexOf('.');
        return dotIndex > 0 ? domain[..dotIndex] : domain;
    }

    private static string NormalizeSubject(string subject)
    {
        var lower = subject.ToLowerInvariant();
        var slugged = NonAlphanumeric.Replace(lower, "-").Trim('-');
        return slugged.Length > 40 ? slugged[..40].TrimEnd('-') : slugged;
    }
}
