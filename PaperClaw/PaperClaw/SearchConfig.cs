using Microsoft.Extensions.Logging;

namespace PaperClaw;

internal sealed record SearchConfig(string LibraryPath, LogLevel LogLevel)
{
    internal static SearchConfig LoadFromEnvironment()
    {
        var libraryPath = Environment.GetEnvironmentVariable("PAPERCLAW_LIBRARY_PATH")
            ?? throw new InvalidOperationException("PAPERCLAW_LIBRARY_PATH is not set");
        var logLevelStr = Environment.GetEnvironmentVariable("PAPERCLAW_LOG_LEVEL") ?? "Information";
        var logLevel = Enum.TryParse<LogLevel>(logLevelStr, true, out var ll) ? ll : LogLevel.Information;

        return new SearchConfig(libraryPath, logLevel);
    }
}
