using Anthropic.SDK;

using Microsoft.Extensions.Logging;

using PaperClaw;

var config = AppConfig.LoadFromEnvironment();

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
