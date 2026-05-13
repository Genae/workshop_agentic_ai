using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;

using Microsoft.Extensions.Logging;

using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace PaperClaw;

internal sealed class PdfProcessor
{
    private const int MaxVisionBytes = 20 * 1024 * 1024; // 20 MB — well within Claude API's 32 MB PDF limit

    private const string VisionSystemPrompt =
        "You are a document transcription assistant. Extract all text content from the " +
        "provided PDF document as accurately as possible. Preserve the logical reading order. " +
        "Return only the extracted text — no commentary, no formatting instructions, no markdown fences. " +
        "If the document contains tables, render them as plain text with pipe separators.";

    private readonly AnthropicClient _claude;
    private readonly ILogger<PdfProcessor> _logger;

    internal PdfProcessor(AnthropicClient claude, ILogger<PdfProcessor> logger)
    {
        _claude = claude;
        _logger = logger;
    }

    internal async Task<PdfContent> ProcessAsync(PdfAttachment attachment)
    {
        _logger.LogInformation("Processing {FileName} ({Bytes} bytes)", attachment.FileName, attachment.Bytes.Length);

        try
        {
            var (text, isScanned) = ExtractWithPdfPig(attachment.Bytes);
            if (!isScanned)
            {
                return new PdfContent(text, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("PdfPig failed for {FileName}: {Message}", attachment.FileName, ex.Message);
        }

        if (attachment.Bytes.Length > MaxVisionBytes)
            throw new InvalidOperationException(
                $"PDF '{attachment.FileName}' is {attachment.Bytes.Length / 1024 / 1024} MB " +
                $"— exceeds the {MaxVisionBytes / 1024 / 1024} MB vision limit.");

        _logger.LogInformation("Vision fallback for {FileName}", attachment.FileName);
        var visionText = await ExtractViaVisionAsync(attachment.Bytes);
        return new PdfContent(visionText, true);
    }

    private static (string Text, bool IsScanned) ExtractWithPdfPig(byte[] bytes)
    {
        using var doc = PdfDocument.Open(bytes);
        var pages = doc.GetPages().ToList();

        var totalLetters = pages.Sum(p => p.Letters.Count);
        if (totalLetters == 0)
        {
            return (string.Empty, true);
        }

        var text = string.Join("\n\n", pages.Select(p => ContentOrderTextExtractor.GetText(p)));
        return (text, false);
    }

    private async Task<string> ExtractViaVisionAsync(byte[] bytes)
    {
        var messages = new List<Message>
        {
            new()
            {
                Role = RoleType.User,
                Content =
                [
                    new DocumentContent
                    {
                        Source = new DocumentSource
                        {
                            Type = SourceType.base64,
                            Data = Convert.ToBase64String(bytes),
                            MediaType = "application/pdf",
                        },
                    },
                    new TextContent { Text = "Please transcribe all text from this document." },
                ],
            },
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = 4096,
            Model = AnthropicModels.Claude46Sonnet,
            Stream = false,
            Temperature = 0m,
            System = [new SystemMessage(VisionSystemPrompt)],
        };

        var response = await _claude.Messages.GetClaudeMessageAsync(parameters);
        return response.Message.ToString();
    }
}
