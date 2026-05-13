# PaperClaw — Design Document

## What it does

PaperClaw polls an IMAP mailbox for incoming emails with PDF attachments. Each PDF is sent to the Claude API for content extraction and categorization. The result is stored in a local library as the original PDF alongside a `.md` transcript, organized by document category.

## Stack

| Concern | Choice |
|---|---|
| Language | C# / .NET 10.0 |
| Email | MailKit (IMAP client) |
| PDF text extraction | PdfPig (native text); Claude vision fallback for scanned/image PDFs |
| AI | Anthropic Claude API (transcription, classification, summarization) |
| Storage | Local filesystem |

## Architecture

## Architecture

- **Entry point:** `PaperClaw/PaperClaw/Program.cs`
- **Tests:** `PaperClaw/PaperClaw.Tests/` (NUnit)
- **Library:** `Library/<category>/` — PDFs land here alongside their `.md` transcripts. PDFs are git-ignored (potentially sensitive); `.md` files are tracked.
- **Formatting rules:** `.editorconfig` at repo root; enforced by `dotnet format`.

Key planned components (not yet implemented): `EmailPoller` (MailKit/IMAP), `PdfProcessor` (PdfPig + Claude vision fallback), `Classifier` (Claude API), `LibraryWriter`.

```
IMAP Mailbox
     │  (poll on request, fetch unseen messages with PDF attachments)
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

## CLI

| Command | Description |
|---|---|
| `dotnet run --project PaperClaw/PaperClaw -- ingest` | Poll IMAP and process new PDFs (default when no args) |
| `dotnet run --project PaperClaw/PaperClaw -- search <query>` | Search the library using Claude |

The `search` command runs an agentic loop: Claude calls filesystem tools
(`list_categories`, `list_documents`, `read_document`, `search_library`) until it can
answer the query, then prints the result to stdout.

`PAPERCLAW_LIBRARY_PATH` and `ANTHROPIC_API_KEY` are the only env vars required for
`search` mode. IMAP vars are not needed.

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
- Processed email UIDs are tracked locally to avoid reprocessing; no email content is retained beyond what is written to the library.
- Logging records file names and processing status only — never document content.
