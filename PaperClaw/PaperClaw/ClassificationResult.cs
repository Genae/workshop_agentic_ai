using System.Text.Json.Serialization;

namespace PaperClaw;

internal sealed record ClassificationResult(
    [property: JsonPropertyName("categorySlug")] string CategorySlug,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("summary")] string Summary);
