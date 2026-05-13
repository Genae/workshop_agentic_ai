# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**PaperClaw** — a C# .NET 10.0 console application that polls an IMAP mailbox for emails with PDF attachments, transcribes them via the Claude API, and stores each PDF alongside a `.md` transcript in a categorized local library. See `DESIGN.md` for the full architecture and security decisions.

## First-time setup

After cloning, activate the pre-commit hooks:

```bash
git config core.hooksPath .githooks
```

## Commands

All commands run from the repo root or `PaperClaw/` (the solution directory).

```bash
# Build / typecheck
dotnet build PaperClaw/PaperClaw.sln

# Run
dotnet run --project PaperClaw/PaperClaw

# Format (auto-fix)
dotnet format PaperClaw/PaperClaw.sln

# Format check only (what the pre-commit hook runs)
dotnet format PaperClaw/PaperClaw.sln --verify-no-changes

# Run all tests
dotnet test PaperClaw/PaperClaw.sln

# Run a single test class
dotnet test PaperClaw/PaperClaw.sln --filter "FullyQualifiedName~MyTestClass"

# Add a NuGet package to the main project
dotnet add PaperClaw/PaperClaw package <PackageName>
```

## Pre-commit hook

`.githooks/pre-commit` runs on every commit: build → format check → tests. All three must pass. Fix format issues with `dotnet format PaperClaw/PaperClaw.sln` before committing.

## Architecture

- **Entry point:** `PaperClaw/PaperClaw/Program.cs`
- **Tests:** `PaperClaw/PaperClaw.Tests/` (NUnit)
- **Library:** `Library/<category>/` — PDFs land here alongside their `.md` transcripts. PDFs are git-ignored (potentially sensitive); `.md` files are tracked.
- **Formatting rules:** `.editorconfig` at repo root; enforced by `dotnet format`.

Key planned components (not yet implemented): `EmailPoller` (MailKit/IMAP), `PdfProcessor` (PdfPig + Claude vision fallback), `Classifier` (Claude API), `LibraryWriter`.

## Environment variables

| Variable | Purpose |
|---|---|
| `ANTHROPIC_API_KEY` | Claude API |
| `PAPERCLAW_IMAP_HOST` | IMAP server |
| `PAPERCLAW_IMAP_PORT` | IMAP port (default 993) |
| `PAPERCLAW_IMAP_USER` | Mailbox username |
| `PAPERCLAW_IMAP_PASSWORD` | Mailbox password |
| `PAPERCLAW_LIBRARY_PATH` | Absolute path to the library root |
