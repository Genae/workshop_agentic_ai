using PaperClaw;

namespace PaperClaw.Tests;

[TestFixture]
public class TranscriptFormatterTests
{
    private static EmailMessage MakeEmail() =>
        new(42u, "sender@example.com", new DateTimeOffset(2025, 5, 15, 0, 0, 0, TimeSpan.Zero),
            "Test Subject", []);

    private static ClassificationResult MakeClassification() =>
        new("invoices", "Test Invoice", "A summary of the test invoice.");

    [Test]
    public void Format_ContainsYamlFrontMatterFields()
    {
        var result = TranscriptFormatter.Format(
            MakeEmail(),
            MakeClassification(),
            new PdfContent("some text", false));

        Assert.That(result, Does.Contain("sender: sender@example.com"));
        Assert.That(result, Does.Contain("date: 2025-05-15"));
        Assert.That(result, Does.Contain("subject: Test Subject"));
        Assert.That(result, Does.Contain("category: invoices"));
        Assert.That(result, Does.Contain("title: Test Invoice"));
        Assert.That(result, Does.Contain("processed: "));
    }

    [Test]
    public void Format_NotScanned_NoVisionNote()
    {
        var result = TranscriptFormatter.Format(
            MakeEmail(),
            MakeClassification(),
            new PdfContent("extracted text", false));

        Assert.That(result, Does.Not.Contain("vision"));
    }

    [Test]
    public void Format_Scanned_IncludesVisionNote()
    {
        var result = TranscriptFormatter.Format(
            MakeEmail(),
            MakeClassification(),
            new PdfContent("extracted text", true));

        Assert.That(result, Does.Contain("Claude vision"));
    }

    [Test]
    public void Format_EndsWithNewline()
    {
        var result = TranscriptFormatter.Format(
            MakeEmail(),
            MakeClassification(),
            new PdfContent("text", false));

        Assert.That(result, Does.EndWith("\n"));
    }

    [Test]
    public void Format_ProcessedTimestampIsIso8601Utc()
    {
        var result = TranscriptFormatter.Format(
            MakeEmail(),
            MakeClassification(),
            new PdfContent("text", false));

        var match = System.Text.RegularExpressions.Regex.Match(
            result,
            @"processed: (\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z)");
        Assert.That(match.Success, Is.True, "processed field should be ISO 8601 UTC");
    }
}
