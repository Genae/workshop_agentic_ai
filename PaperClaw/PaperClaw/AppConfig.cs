using Microsoft.Extensions.Logging;

namespace PaperClaw;

internal sealed record AppConfig(
    string ImapHost,
    int ImapPort,
    string ImapUser,
    string ImapPassword,
    string LibraryPath,
    LogLevel LogLevel)
{
    internal static AppConfig LoadFromEnvironment()
    {
        var host = Environment.GetEnvironmentVariable("PAPERCLAW_IMAP_HOST")
            ?? throw new InvalidOperationException("PAPERCLAW_IMAP_HOST is not set");
        var portStr = Environment.GetEnvironmentVariable("PAPERCLAW_IMAP_PORT");
        var port = portStr is not null && int.TryParse(portStr, out var p) ? p : 993;
        var user = Environment.GetEnvironmentVariable("PAPERCLAW_IMAP_USER")
            ?? throw new InvalidOperationException("PAPERCLAW_IMAP_USER is not set");
        var password = Environment.GetEnvironmentVariable("PAPERCLAW_IMAP_PASSWORD")
            ?? throw new InvalidOperationException("PAPERCLAW_IMAP_PASSWORD is not set");
        var libraryPath = Environment.GetEnvironmentVariable("PAPERCLAW_LIBRARY_PATH")
            ?? throw new InvalidOperationException("PAPERCLAW_LIBRARY_PATH is not set");
        var logLevelStr = Environment.GetEnvironmentVariable("PAPERCLAW_LOG_LEVEL") ?? "Information";
        var logLevel = Enum.TryParse<LogLevel>(logLevelStr, true, out var ll) ? ll : LogLevel.Information;

        return new AppConfig(host, port, user, password, libraryPath, logLevel);
    }
}
