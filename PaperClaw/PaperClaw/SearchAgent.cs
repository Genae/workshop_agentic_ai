using System.Text.Json.Nodes;

using Anthropic.SDK;
using Anthropic.SDK.Common;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;

using Microsoft.Extensions.Logging;

namespace PaperClaw;

internal sealed class SearchAgent
{
    private const int MaxTurns = 10;

    private const string SystemPrompt =
        "You are a document library assistant. The library contains PDF transcripts organised by " +
        "category. Use the available tools to find and read relevant documents, then answer the " +
        "user's question. Always cite which document(s) you found the information in.";

    private static JsonObject BuildSchema(params (string name, string type, string description)[] props)
    {
        var properties = new JsonObject();
        foreach (var (name, type, description) in props)
            properties[name] = new JsonObject
            {
                ["type"] = JsonValue.Create(type),
                ["description"] = JsonValue.Create(description),
            };

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = new JsonArray(props.Select(p => (JsonNode?)JsonValue.Create(p.name)).ToArray()),
        };
    }

    private static readonly List<Anthropic.SDK.Common.Tool> Tools =
    [
        new Function(
            "list_categories",
            "List all document categories in the library",
            new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() }),
        new Function(
            "list_documents",
            "List all documents in a specific category",
            BuildSchema(("category", "string", "Category slug"))),
        new Function(
            "read_document",
            "Read the full transcript of a specific document",
            BuildSchema(
                ("category", "string", "Category slug"),
                ("filename", "string", "Document slug without extension"))),
        new Function(
            "search_library",
            "Full-text search across all document transcripts",
            BuildSchema(("query", "string", "Text to search for"))),
    ];

    private readonly AnthropicClient _claude;
    private readonly LibrarySearch _search;
    private readonly ILogger<SearchAgent> _logger;

    internal SearchAgent(AnthropicClient claude, LibrarySearch search, ILogger<SearchAgent> logger)
    {
        _claude = claude;
        _search = search;
        _logger = logger;
    }

    internal async Task<string> SearchAsync(string query)
    {
        var messages = new List<Message>
        {
            new Message(RoleType.User, query, null!),
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = 2048,
            Model = AnthropicModels.Claude46Sonnet,
            Stream = false,
            Temperature = 0m,
            System = [new SystemMessage(SystemPrompt)],
            Tools = Tools,
        };

        for (var turn = 0; turn < MaxTurns; turn++)
        {
            var response = await _claude.Messages.GetClaudeMessageAsync(parameters);
            messages.Add(response.Message);

            var toolCalls = response.ToolCalls;
            if (toolCalls is null || toolCalls.Count == 0)
            {
                return response.Message.ToString();
            }

            foreach (var toolCall in toolCalls)
            {
                _logger.LogInformation("Tool called: {Name}", toolCall.Name);
                var result = Dispatch(toolCall.Name, toolCall.Arguments);
                messages.Add(new Message(toolCall, result, false, null!));
            }
        }

        return "(search exceeded turn limit)";
    }

    private string Dispatch(string name, JsonNode? args)
    {
        return name switch
        {
            "list_categories" => _search.ListCategories(),
            "list_documents" => _search.ListDocuments(GetString(args, "category")),
            "read_document" => _search.ReadDocument(
                GetString(args, "category"),
                GetString(args, "filename")),
            "search_library" => _search.SearchLibrary(GetString(args, "query")),
            _ => "Unknown tool.",
        };
    }

    private static string GetString(JsonNode? args, string key)
    {
        return args?[key]?.GetValue<string>() ?? string.Empty;
    }
}
