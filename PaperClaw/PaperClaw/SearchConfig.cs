using Microsoft.Extensions.Logging;

namespace PaperClaw;

internal sealed record SearchConfig(string LibraryPath, LogLevel LogLevel)
{
    internal static SearchConfig LoadFromEnvironment()
    {
        var (libraryPath, logLevel) = ConfigHelper.LoadCommon();
        return new SearchConfig(libraryPath, logLevel);
    }
}
