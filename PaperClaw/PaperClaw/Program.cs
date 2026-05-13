using Anthropic.SDK;

using Microsoft.Extensions.Logging;

using PaperClaw;

LoadDotEnv();

var command = args.Length > 0 ? args[0].ToLowerInvariant() : "ingest";

switch (command)
{
    case "ingest":
        await RunIngestAsync();
        break;

    case "import":
        var importPath = args.Length > 1 ? args[1] : string.Empty;
        if (string.IsNullOrEmpty(importPath))
        {
            Console.Error.WriteLine("Usage: paperclaw import <path>");
            Environment.Exit(1);
        }

        await RunImportAsync(importPath);
        break;

    case "search":
        var query = string.Join(" ", args.Skip(1)).Trim();
        if (string.IsNullOrEmpty(query))
        {
            Console.Error.WriteLine("Usage: paperclaw search <query>");
            Environment.Exit(1);
        }

        await RunSearchAsync(query);
        break;

    default:
        Console.Error.WriteLine("Usage: paperclaw [ingest|import <path>|search <query>]");
        Console.Error.WriteLine("  ingest          Poll IMAP and process new PDFs (default)");
        Console.Error.WriteLine("  import <path>   Import a local PDF file or folder into the library");
        Console.Error.WriteLine("  search <query>  Search the library using Claude");
        Environment.Exit(1);
        break;
}

// ── Ingest mode ──────────────────────────────────────────────────────────────

async Task RunIngestAsync()
{
    var config = AppConfig.LoadFromEnvironment();
    using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(config.LogLevel));
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
        catch (Exception ex) when (ex is IOException or InvalidOperationException or HttpRequestException)
        {
            logger.LogWarning("Skipping UID {Uid}: {Message}", email.Uid, ex.Message);
        }
    }

    logger.LogInformation("Done. {Processed} processed, {Skipped} skipped.", processed, skipped);
}

// ── Import mode ───────────────────────────────────────────────────────────────

async Task RunImportAsync(string path)
{
    var config = SearchConfig.LoadFromEnvironment();
    using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(config.LogLevel));
    var logger = loggerFactory.CreateLogger("PaperClaw");
    var claude = new AnthropicClient();

    var processor = new PdfProcessor(claude, loggerFactory.CreateLogger<PdfProcessor>());
    var classifier = new Classifier(claude, loggerFactory.CreateLogger<Classifier>());
    var writer = new LibraryWriter(config.LibraryPath, loggerFactory.CreateLogger<LibraryWriter>());

    IEnumerable<string> files;
    if (Directory.Exists(path))
        files = Directory.GetFiles(path, "*.pdf", SearchOption.TopDirectoryOnly);
    else if (File.Exists(path))
        files = [path];
    else
    {
        Console.Error.WriteLine($"Path not found: {path}");
        Environment.Exit(1);
        return;
    }

    var pdfFiles = files.OrderBy(f => f).ToList();
    if (pdfFiles.Count == 0)
    {
        logger.LogInformation("No PDF files found at {Path}", path);
        return;
    }

    var hashStore = new ImportedHashStore(config.LibraryPath);
    var seenHashes = await hashStore.LoadAsync();

    var processed = 0;
    var skipped = 0;
    foreach (var file in pdfFiles)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(file);
            var hash = ImportedHashStore.ComputeHash(bytes);
            if (!seenHashes.Add(hash))
            {
                logger.LogInformation("Skipping {File} (already imported)", Path.GetFileName(file));
                skipped++;
                continue;
            }

            var attachment = new PdfAttachment(Path.GetFileName(file), bytes);
            var email = new EmailMessage(
                Uid: 0,
                Sender: "local-import",
                Date: new DateTimeOffset(File.GetLastWriteTimeUtc(file), TimeSpan.Zero),
                Subject: Path.GetFileNameWithoutExtension(file),
                Attachments: [attachment]);

            var content = await processor.ProcessAsync(attachment);
            var classification = await classifier.ClassifyAsync(email, content);
            await writer.WriteAsync(email, attachment, classification, content);
            await hashStore.AddAsync(hash);
            processed++;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or HttpRequestException)
        {
            logger.LogWarning("Skipping {File}: {Message}", Path.GetFileName(file), ex.Message);
        }
    }

    logger.LogInformation("Done. {Processed} imported, {Skipped} skipped.", processed, skipped);
}

// ── Search mode ───────────────────────────────────────────────────────────────

async Task RunSearchAsync(string searchQuery)
{
    var config = SearchConfig.LoadFromEnvironment();
    using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(config.LogLevel));
    var claude = new AnthropicClient();

    var agent = new SearchAgent(
        claude,
        new LibrarySearch(config.LibraryPath),
        loggerFactory.CreateLogger<SearchAgent>());

    var answer = await agent.SearchAsync(searchQuery);
    Console.WriteLine(answer);
}

// ── .env loader ───────────────────────────────────────────────────────────────

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
