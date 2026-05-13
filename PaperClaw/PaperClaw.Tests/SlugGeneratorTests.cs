using PaperClaw;

namespace PaperClaw.Tests;

[TestFixture]
public class SlugGeneratorTests
{
    private static readonly DateTimeOffset BaseDate = new(2025, 5, 1, 0, 0, 0, TimeSpan.Zero);

    [Test]
    public void Generate_NormalAddress_ReturnsDomainDateSubject()
    {
        var slug = SlugGenerator.Generate("Invoice May 2025", BaseDate, "Jane Smith <jane@acme-corp.com>");
        Assert.That(slug, Is.EqualTo("acme-corp-2025-05-invoice-may-2025"));
    }

    [Test]
    public void Generate_SpecialCharsInSubject_OnlyAlphanumericAndHyphens()
    {
        var slug = SlugGenerator.Generate("Re: [FWD] Invoice #1234 (urgent!)", BaseDate, "sender@example.com");
        Assert.That(slug, Does.Not.Contain(" "));
        Assert.That(slug, Does.Match(@"^[a-z0-9\-]+$"));
        Assert.That(slug, Does.Not.Contain("--"));
    }

    [Test]
    public void Generate_EmptySubject_FallsBackToDocumentDate()
    {
        var slug = SlugGenerator.Generate(string.Empty, BaseDate, "sender@example.com");
        Assert.That(slug, Is.EqualTo("example-2025-05"));
    }

    [Test]
    public void Generate_WwwDomain_StripsPrefixAndFirstLabel()
    {
        var slug = SlugGenerator.Generate("Test", BaseDate, "user@www.example.com");
        Assert.That(slug, Does.StartWith("example-"));
    }

    [Test]
    public void Generate_SubjectTruncatedAt40Chars()
    {
        var longSubject = new string('a', 60);
        var slug = SlugGenerator.Generate(longSubject, BaseDate, "u@x.com");
        // slug is "z-2025-05-subjectpart", so split gives [z, 2025, 05, ...]
        var parts = slug.Split('-');
        var subjectPart = string.Join("-", parts.Skip(3));
        Assert.That(subjectPart.Length, Is.LessThanOrEqualTo(40));
    }

    [Test]
    public void Generate_NoAtSign_FallsBackGracefully()
    {
        var slug = SlugGenerator.Generate("Invoice", BaseDate, "noatsignhere");
        Assert.That(slug, Is.Not.Empty);
        Assert.That(slug, Does.Match(@"^[a-z0-9\-]+$"));
    }
}
