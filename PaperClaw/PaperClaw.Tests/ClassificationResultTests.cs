using System.Text.Json;

using PaperClaw;

namespace PaperClaw.Tests;

[TestFixture]
public class ClassificationResultTests
{
    [Test]
    public void Deserialize_ValidJson_PopulatesAllFields()
    {
        const string json = """{"categorySlug":"invoices","title":"Acme Invoice","summary":"A test summary."}""";
        var result = JsonSerializer.Deserialize<ClassificationResult>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CategorySlug, Is.EqualTo("invoices"));
        Assert.That(result.Title, Is.EqualTo("Acme Invoice"));
        Assert.That(result.Summary, Is.EqualTo("A test summary."));
    }

    [Test]
    public void Deserialize_ExtraFields_IgnoresUnknown()
    {
        const string json = """{"categorySlug":"other","title":"T","summary":"S","unknownField":"value"}""";
        Assert.DoesNotThrow(() => JsonSerializer.Deserialize<ClassificationResult>(json));
    }

    [Test]
    public void Deserialize_NullInput_ThrowsOrReturnsNull()
    {
        var result = JsonSerializer.Deserialize<ClassificationResult>("null");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void WithExpression_UpdatesCategorySlug()
    {
        var original = new ClassificationResult("invoices", "Title", "Summary");
        var updated = original with { CategorySlug = "contracts" };

        Assert.That(updated.CategorySlug, Is.EqualTo("contracts"));
        Assert.That(updated.Title, Is.EqualTo("Title"));
        Assert.That(updated.Summary, Is.EqualTo("Summary"));
    }
}
