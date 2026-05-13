using System.Text.Json;
using System.Text.RegularExpressions;

using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;

using Microsoft.Extensions.Logging;

namespace PaperClaw;

internal sealed class Classifier
{
    private const string SystemPrompt =
        "You are a document classifier. Given document metadata and a content excerpt, respond " +
        "with a JSON object containing exactly three fields:\n" +
        "- \"categorySlug\": short lowercase slug (letters, digits, hyphens only), e.g. \"invoices\", " +
        "\"contracts\", \"bank-statements\", \"receipts\", \"tax-documents\", \"correspondence\", \"other\"\n" +
        "- \"title\": concise human-readable title for this specific document (max 80 characters)\n" +
        "- \"summary\": 2–4 sentence paragraph summarizing the document's content and purpose\n\n" +
        "Return only the JSON object — no markdown fences, no explanation.";

    private static readonly Regex NonAlphanumeric = new("[^a-z0-9]+", RegexOptions.Compiled);

    private readonly AnthropicClient _claude;
    private readonly ILogger<Classifier> _logger;

    internal Classifier(AnthropicClient claude, ILogger<Classifier> logger)
    {
        _claude = claude;
        _logger = logger;
    }

    internal async Task<ClassificationResult> ClassifyAsync(EmailMessage email, PdfContent content)
    {
        var excerpt = content.ExtractedText.Length > 4000
            ? content.ExtractedText[..4000]
            : content.ExtractedText;

        var userMessage = $"""
            Sender: {email.Sender}
            Date: {email.Date:yyyy-MM-dd}
            Subject: {email.Subject}

            Content excerpt:
            {excerpt}
            """;

        var messages = new List<Message>
        {
            new Message(RoleType.User, userMessage),
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = 1024,
            Model = AnthropicModels.Claude46Sonnet,
            Stream = false,
            Temperature = 0m,
            System = [new SystemMessage(SystemPrompt)],
        };

        var response = await _claude.Messages.GetClaudeMessageAsync(parameters);
        var rawText = response.Message.ToString();

        var result = JsonSerializer.Deserialize<ClassificationResult>(rawText)
            ?? throw new InvalidOperationException("Claude returned null classification response");

        var categorySlug = NormalizeSlug(result.CategorySlug);
        if (string.IsNullOrEmpty(categorySlug))
        {
            categorySlug = "other";
        }

        _logger.LogInformation("Classified as '{Category}': {Title}", categorySlug, result.Title);

        return result with { CategorySlug = categorySlug };
    }

    private static string NormalizeSlug(string raw)
    {
        var lower = raw?.ToLowerInvariant() ?? string.Empty;
        return NonAlphanumeric.Replace(lower, "-").Trim('-');
    }
}
