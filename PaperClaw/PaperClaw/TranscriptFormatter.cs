namespace PaperClaw;

internal static class TranscriptFormatter
{
    internal static string Format(
        EmailMessage email,
        ClassificationResult classification,
        PdfContent content)
    {
        var scannedNote = content.IsScanned
            ? "\n> Note: Text extracted via Claude vision (scanned document).\n"
            : string.Empty;

        return $"""
            ---
            sender: {email.Sender}
            date: {email.Date:yyyy-MM-dd}
            subject: {email.Subject}
            category: {classification.CategorySlug}
            title: {classification.Title}
            processed: {DateTimeOffset.UtcNow:yyyy-MM-ddTHH:mm:ssZ}
            ---

            # {classification.Title}

            ## Summary

            {classification.Summary}
            {scannedNote}
            ## Document Text

            {content.ExtractedText}

            """;
    }
}
