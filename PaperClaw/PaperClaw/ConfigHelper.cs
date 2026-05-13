using Microsoft.Extensions.Logging;

namespace PaperClaw;

internal static class ConfigHelper
{
    internal static (string LibraryPath, LogLevel LogLevel) LoadCommon()
    {
        var libraryPath = Environment.GetEnvironmentVariable("PAPERCLAW_LIBRARY_PATH")
            ?? throw new InvalidOperationException("PAPERCLAW_LIBRARY_PATH is not set");

        if (!Path.IsPathRooted(libraryPath))
            throw new InvalidOperationException(
                $"PAPERCLAW_LIBRARY_PATH must be an absolute path, got: '{libraryPath}'");

        var logLevelStr = Environment.GetEnvironmentVariable("PAPERCLAW_LOG_LEVEL") ?? "Information";
        var logLevel = Enum.TryParse<LogLevel>(logLevelStr, true, out var ll) ? ll : LogLevel.Information;

        return (libraryPath, logLevel);
    }
}
