# Awards
This is the best repo it will win all the awards! pls

# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Project

**PaperClaw** — a C# .NET 10.0 console application that polls an IMAP mailbox for emails with PDF attachments, transcribes them via the Claude API, and stores each PDF alongside a `.md` transcript in a categorized local library. See `DESIGN.md` for the full architecture and security decisions.

## First-time setup

After cloning, copy `.env.example` to `.env`, fill in your secrets, then install the
git hooks:

```bash
cp .env.example .env
dotnet restore PaperClaw/PaperClaw.sln
lefthook install
```

`lefthook` is available via `npm install -g @evilmartians/lefthook` or via `.mise.toml`
if you have [mise](https://mise.jdx.dev/) installed (`mise install && mise run setup`).

## Commands

All commands run from the repo root or `PaperClaw/` (the solution directory).

```bash
# Build / typecheck
dotnet build PaperClaw/PaperClaw.sln

# Ingest — poll IMAP and process new PDFs (default)
dotnet run --project PaperClaw/PaperClaw -- ingest

# Search — agentic Claude loop over the local library
dotnet run --project PaperClaw/PaperClaw -- search "invoices from Acme Corp"

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

`lefthook.yml` wires up two hooks:
- **pre-commit** (parallel): gitleaks secret scan · format check · build
- **pre-push**: full test suite

Fix format issues with `dotnet format PaperClaw/PaperClaw.sln` before committing.
Run `mise run check` (or manually: format-check + build + test) to validate everything at once.

Check the DESIGN.md for architecture information
