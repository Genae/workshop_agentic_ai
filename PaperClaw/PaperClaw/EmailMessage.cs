namespace PaperClaw;

internal sealed record EmailMessage(
    uint Uid,
    string Sender,
    DateTimeOffset Date,
    string Subject,
    IReadOnlyList<PdfAttachment> Attachments);
