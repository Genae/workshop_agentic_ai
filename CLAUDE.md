# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**PaperClaw** — a C# .NET console application to help organize PDF documents received by mail. Currently in early development (bootstrapped scaffold only).

## Commands

All commands run from `PaperClaw/PaperClaw/` (the project directory) or `PaperClaw/` (the solution directory).

```bash
# Build
dotnet build

# Run
dotnet run

# Run tests (once tests exist)
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~MyTestClass"

# Add a NuGet package
dotnet add package <PackageName>
```

## Architecture

- **Target:** .NET 10.0 console application (`Exe`)
- **C# features:** implicit usings and nullable reference types enabled
- **Entry point:** `PaperClaw/PaperClaw/Program.cs`
- **Solution file:** `PaperClaw/PaperClaw.sln` (single-project solution)

The application goal is to receive PDF documents (likely via email/IMAP), extract relevant metadata, and organize or categorize them. No domain logic has been implemented yet — the scaffold is ready for development.
