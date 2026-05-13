using Anthropic.SDK;

using Microsoft.Extensions.Logging;

using PaperClaw;

LoadDotEnv();
var config = AppConfig.LoadFromEnvironment();

// Loads .env from the repo root (or any ancestor directory) into the process environment.
// Variables already set in the environment take precedence, so this is a no-op in production.
static void LoadDotEnv()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null)
    {
        var candidate = Path.Combine(dir.FullName, ".env");
        if (File.Exists(candidate))
        {
            foreach (var line in File.ReadAllLines(candidate))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                {
                    continue;
                }

                var eq = line.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }

                var key = line[..eq].Trim();
                var value = line[(eq + 1)..].Trim();
                if (!string.IsNullOrEmpty(key) && Environment.GetEnvironmentVariable(key) is null)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }

            return;
        }

        dir = dir.Parent;
    }
}

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(config.LogLevel));

var logger = loggerFactory.CreateLogger("PaperClaw");

var claude = new AnthropicClient();
var uidStore = new ProcessedUidStore(config.LibraryPath);
var processedUids = await uidStore.LoadAsync();

using var poller = new EmailPoller(config, loggerFactory.CreateLogger<EmailPoller>());
var emails = await poller.PollAsync();
var newEmails = emails.Where(e => !processedUids.Contains(e.Uid)).ToList();

var processor = new PdfProcessor(claude, loggerFactory.CreateLogger<PdfProcessor>());
var classifier = new Classifier(claude, loggerFactory.CreateLogger<Classifier>());
var writer = new LibraryWriter(config.LibraryPath, loggerFactory.CreateLogger<LibraryWriter>());

var processed = 0;
var skipped = emails.Count - newEmails.Count;

foreach (var email in newEmails)
{
    try
    {
        foreach (var attachment in email.Attachments)
        {
            var content = await processor.ProcessAsync(attachment);
            var classification = await classifier.ClassifyAsync(email, content);
            await writer.WriteAsync(email, attachment, classification, content);
        }

        await uidStore.AddAsync(email.Uid);
        processed++;
    }
    catch (Exception ex)
    {
        logger.LogWarning("Skipping UID {Uid}: {Message}", email.Uid, ex.Message);
    }
}

logger.LogInformation("Done. {Processed} processed, {Skipped} skipped.", processed, skipped);
