# PaperClaw — Design Document

## What it does

PaperClaw polls an IMAP mailbox for incoming emails with PDF attachments. Each PDF is sent to the Claude API for content extraction and categorization. The result is stored in a local library as the original PDF alongside a `.md` transcript, organized by sender category.

## Stack

| Concern | Choice |
|---|---|
| Language | C# / .NET 10.0 |
| Email | MailKit (IMAP client) |
| PDF text extraction | PdfPig (native text); Claude vision fallback for scanned/image PDFs |
| AI | Anthropic Claude API (transcription, classification, summarization) |
| Storage | Local filesystem |

## Architecture

```
IMAP Mailbox
     │  (poll on schedule, fetch unseen messages with PDF attachments)
     ▼
EmailPoller
     │
     ▼
PdfProcessor
     │  PdfPig extracts text; falls back to Claude vision for image-only PDFs
     ▼
Classifier  ──► Claude API
     │  returns: category slug, display title, one-paragraph summary
     ▼
LibraryWriter
     │  saves PDF + .md transcript under Library/<category>/
     ▼
Library/
  invoices/
    acme-corp-2025-05.pdf
    acme-corp-2025-05.md
  contracts/
    lease-2025-01.pdf
    lease-2025-01.md
```

The `.md` transcript contains the Claude-generated summary, extracted metadata (sender, date, subject), and the full extracted text.

## Configuration

All secrets are supplied via environment variables — never committed.

| Variable | Purpose |
|---|---|
| `ANTHROPIC_API_KEY` | Claude API access |
| `PAPERCLAW_IMAP_HOST` | IMAP server hostname |
| `PAPERCLAW_IMAP_PORT` | IMAP port (default 993) |
| `PAPERCLAW_IMAP_USER` | Mailbox username |
| `PAPERCLAW_IMAP_PASSWORD` | Mailbox password |
| `PAPERCLAW_LIBRARY_PATH` | Absolute path to the local library root |

## Security & Privacy

- Credentials are read exclusively from environment variables; no config files containing secrets are committed.
- PDFs may contain sensitive financial or personal data. They are stored locally only. The only external service that receives document content is the Claude API for transcription.
- The library directory should be excluded from cloud sync (Dropbox, OneDrive, etc.) if it contains sensitive documents.
- Processed email UIDs are tracked locally to avoid reprocessing; no email content is retained beyond what is written to the library.
- Logging records file names and processing status only — never document content.
