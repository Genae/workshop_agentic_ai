# PaperClaw — Agent Guide

PaperClaw polls an IMAP mailbox for emails with PDF attachments, transcribes each PDF
via the Claude API, and stores the original PDF alongside a `.md` transcript in a
categorized local library. It is not a web service, not a library, not a daemon — it
is a single-shot CLI invocation that polls once and exits.

**Stack:** C# / .NET 10.0 console app · MailKit (IMAP) · PdfPig (text extraction) ·
Anthropic.SDK 5.x (Claude API, vision fallback for scanned PDFs) ·
Microsoft.Extensions.Logging → stdout

## Non-obvious rules

1. **Secrets come from environment variables only.** Never read credentials from a
   config file or hard-code them. See `.env.example` for the full variable list.
   Do: `Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")`.

2. **Log file names and status, never document content.** PDFs may contain sensitive
   financial or personal data. Log lines like `"Saved invoices/acme-2025.md"` are
   fine; logging the extracted text or email body is not.

3. **Unused private members are build errors** (IDE0051/IDE0052 via `.editorconfig`).
   Do: delete dead code instead of commenting it out.

4. **All mutations must be idempotent.** The poller tracks processed UIDs locally.
   Re-running with the same UID must be safe — no duplicate writes, no double-charges.

5. **`TreatWarningsAsErrors` is on.** Every Roslyn warning is a build failure.
   Do not suppress with `#pragma warning disable` unless you add a comment explaining
   exactly why and link a tracking issue.
