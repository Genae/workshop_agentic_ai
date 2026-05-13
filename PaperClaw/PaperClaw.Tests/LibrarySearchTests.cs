using PaperClaw;

namespace PaperClaw.Tests;

[TestFixture]
public class LibrarySearchTests
{
    private string _tempDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    // ── ListCategories ────────────────────────────────────────────────────────

    [Test]
    public void ListCategories_EmptyLibrary_ReturnsEmptyMessage()
    {
        var search = new LibrarySearch(_tempDir);
        Assert.That(search.ListCategories(), Is.EqualTo("(library is empty)"));
    }

    [Test]
    public void ListCategories_WithCategories_ReturnsCategoryNames()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, "invoices"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "contracts"));

        var result = new LibrarySearch(_tempDir).ListCategories();

        Assert.That(result, Does.Contain("invoices"));
        Assert.That(result, Does.Contain("contracts"));
    }

    // ── ListDocuments ─────────────────────────────────────────────────────────

    [Test]
    public void ListDocuments_UnknownCategory_ReturnsNoDocuments()
    {
        var result = new LibrarySearch(_tempDir).ListDocuments("nonexistent");
        Assert.That(result, Is.EqualTo("(no documents)"));
    }

    [Test]
    public void ListDocuments_WithFiles_ReturnsStemNames()
    {
        var dir = Path.Combine(_tempDir, "invoices");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "acme-2025-01.md"), "content");
        File.WriteAllText(Path.Combine(dir, "bob-2025-02.md"), "content");

        var result = new LibrarySearch(_tempDir).ListDocuments("invoices");

        Assert.That(result, Does.Contain("acme-2025-01"));
        Assert.That(result, Does.Contain("bob-2025-02"));
        Assert.That(result, Does.Not.Contain(".md"));
    }

    // ── ReadDocument ──────────────────────────────────────────────────────────

    [Test]
    public void ReadDocument_Found_ReturnsContent()
    {
        var dir = Path.Combine(_tempDir, "invoices");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "acme-2025-01.md"), "# Invoice\nContent here.");

        var result = new LibrarySearch(_tempDir).ReadDocument("invoices", "acme-2025-01");

        Assert.That(result, Does.Contain("# Invoice"));
        Assert.That(result, Does.Contain("Content here."));
    }

    [Test]
    public void ReadDocument_NotFound_ReturnsNotFoundMessage()
    {
        var result = new LibrarySearch(_tempDir).ReadDocument("invoices", "missing-doc");
        Assert.That(result, Is.EqualTo("Document not found."));
    }

    [Test]
    public void ReadDocument_PathTraversalInCategory_Sanitised()
    {
        // "../" in category should not escape the library root
        var result = new LibrarySearch(_tempDir).ReadDocument("../secrets", "anything");
        Assert.That(result, Is.EqualTo("Document not found."));
    }

    // ── SearchLibrary ─────────────────────────────────────────────────────────

    [Test]
    public void SearchLibrary_NoMatches_ReturnsNoMatchesMessage()
    {
        var dir = Path.Combine(_tempDir, "invoices");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "doc.md"), "unrelated content");

        var result = new LibrarySearch(_tempDir).SearchLibrary("xyzzy-nonexistent");
        Assert.That(result, Is.EqualTo("No matches found."));
    }

    [Test]
    public void SearchLibrary_Match_ReturnsFormattedExcerpt()
    {
        var dir = Path.Combine(_tempDir, "invoices");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "acme-2025-01.md"), "Invoice from Acme Corp for services.");

        var result = new LibrarySearch(_tempDir).SearchLibrary("Acme Corp");

        Assert.That(result, Does.Contain("invoices/acme-2025-01:"));
        Assert.That(result, Does.Contain("Acme Corp"));
    }

    [Test]
    public void SearchLibrary_CaseInsensitive_Matches()
    {
        var dir = Path.Combine(_tempDir, "invoices");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "doc.md"), "Invoice from ACME CORP");

        var result = new LibrarySearch(_tempDir).SearchLibrary("acme corp");
        Assert.That(result, Does.Not.EqualTo("No matches found."));
    }

    [Test]
    public void SearchLibrary_CapsAt20Matches_Returns20Results()
    {
        var dir = Path.Combine(_tempDir, "docs");
        Directory.CreateDirectory(dir);
        for (var i = 0; i < 25; i++)
        {
            File.WriteAllText(Path.Combine(dir, $"doc-{i:D2}.md"), "matching keyword here");
        }

        var result = new LibrarySearch(_tempDir).SearchLibrary("matching keyword");
        var lineCount = result.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
        Assert.That(lineCount, Is.EqualTo(20));
    }
}
