---
name: paperclaw-search
description: Search the local PaperClaw document library using the agentic Claude search loop. Use when the user asks to find, look up, or check documents in their library — invoices, tax notices, contracts, correspondence, etc.
---

# PaperClaw Search

This skill searches the local PaperClaw document library by running the agentic search CLI.

## When to Use This Skill

Use when the user asks to:
- Find or look up a document
- Check what documents exist in the library
- Search for invoices, tax documents, contracts, or correspondence
- Ask what's important, urgent, or due in their documents
- Review any content stored in the local PaperClaw library

## How to Run the Search

Run from the repo root (`C:\Users\Admin\git\workshop_agentic_ai`):

```bash
dotnet run --project PaperClaw/PaperClaw -- search "<query>"
```

Craft the query to match what the user is looking for. Be specific — the agent uses Claude to semantically search transcripts, so natural-language queries work well (e.g. "urgent deadlines and due dates", "invoices from last month", "tax refunds").

## Presenting Results

- Summarize what was found in plain language.
- Highlight deadlines, amounts due, or required actions prominently.
- If the library is empty or no relevant documents are found, say so plainly and suggest the user run `dotnet run --project PaperClaw/PaperClaw -- ingest` to pull in new emails.

