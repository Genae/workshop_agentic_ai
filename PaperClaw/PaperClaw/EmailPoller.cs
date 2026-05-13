using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;

using Microsoft.Extensions.Logging;

using MimeKit;

namespace PaperClaw;

internal sealed class EmailPoller : IDisposable
{
    private readonly AppConfig _config;
    private readonly ILogger<EmailPoller> _logger;
    private readonly ImapClient _client = new();

    internal EmailPoller(AppConfig config, ILogger<EmailPoller> logger)
    {
        _config = config;
        _logger = logger;
    }

    internal async Task<IReadOnlyList<EmailMessage>> PollAsync()
    {
        await _client.ConnectAsync(_config.ImapHost, _config.ImapPort, SecureSocketOptions.SslOnConnect);
        await _client.AuthenticateAsync(_config.ImapUser, _config.ImapPassword);

        var inbox = _client.Inbox
            ?? throw new InvalidOperationException("IMAP inbox is not available after authentication");
        await inbox.OpenAsync(FolderAccess.ReadWrite);

        var uids = await inbox.SearchAsync(SearchQuery.NotSeen);
        if (uids.Count == 0)
        {
            _logger.LogInformation("No unseen messages found");
            return [];
        }

        var summaries = await inbox.FetchAsync(
            uids,
            MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

        var emails = new List<EmailMessage>();

        foreach (var summary in summaries)
        {
            var pdfParts = FindPdfParts(summary.Body).ToList();
            if (pdfParts.Count == 0)
            {
                continue;
            }

            var attachments = new List<PdfAttachment>();
            foreach (var part in pdfParts)
            {
                try
                {
                    var entity = await inbox.GetBodyPartAsync(summary.UniqueId, part);
                    if (entity is MimePart mimePart && mimePart.Content is not null)
                    {
                        using var ms = new MemoryStream();
                        await mimePart.Content.DecodeToAsync(ms);
                        var fileName = part.FileName ?? part.ContentLocation?.ToString() ?? "attachment.pdf";
                        attachments.Add(new PdfAttachment(fileName, ms.ToArray()));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to fetch attachment for UID {Uid}: {Message}", summary.UniqueId.Id, ex.Message);
                }
            }

            if (attachments.Count == 0)
            {
                continue;
            }

            await inbox.StoreAsync(
                new UniqueId[] { summary.UniqueId },
                new StoreFlagsRequest(StoreAction.Add, MessageFlags.Seen) { Silent = true });

            var sender = summary.Envelope?.From?.Mailboxes.FirstOrDefault()?.ToString() ?? "unknown";
            var subject = summary.Envelope?.Subject ?? string.Empty;
            var date = summary.Envelope?.Date ?? DateTimeOffset.UtcNow;

            emails.Add(new EmailMessage(summary.UniqueId.Id, sender, date, subject, attachments));
        }

        _logger.LogInformation("Fetched {Count} emails with PDF attachments", emails.Count);
        return emails;
    }

    private static IEnumerable<BodyPartBasic> FindPdfParts(BodyPart? body)
    {
        if (body is BodyPartMultipart multipart)
        {
            foreach (var part in multipart.BodyParts)
            {
                foreach (var pdf in FindPdfParts(part))
                {
                    yield return pdf;
                }
            }
        }
        else if (body is BodyPartBasic basic
            && basic.ContentType.MediaType.Equals("application", StringComparison.OrdinalIgnoreCase)
            && basic.ContentType.MediaSubtype.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            yield return basic;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
