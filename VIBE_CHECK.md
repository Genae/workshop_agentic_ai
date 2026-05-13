# 🪩 Vibe Check Report — PaperClaw

## TL;DR
- **Score:** 72 / 100 — *Works for now. Drink some water before the agent gets ambitious.*
- **Biggest win:** Strictness stack (items 2, 3, 8) — `TreatWarningsAsErrors` + elevated IDE0051/0052 means dead code and unused privates fail the build, not just review.
- **Biggest miss:** No agentic review panel (item 12) and no blast-radius friction (item 13) — every diff lands with a single human reviewer and no extra speed bump on IMAP/secret/library code.
- **Do this now:** Add a `REVIEW.md` and a `/review` slash command (or `mise` task) that fans out a parallel best-practices + C# + security reviewer pass before PR open.
- **Earned bonuses:** 3 earned 🎁🎁🎁 — *Vibe Pioneer*

## 🌴 Stack detected
- **Language:** C# / .NET 10.0
- **Package manager:** `dotnet` (NuGet) · toolchain pinned via `.mise.toml`
- **Toolchain notes:** MailKit · PdfPig · Anthropic.SDK 5.x · Microsoft.Extensions.Logging.Console · NUnit · lefthook · gitleaks · dotnet format

## Vibe Check Report Card

```
┌─────┬───────────────────────────────────────┬──────┬──────────────────────────────────────────────────────────────────────────────┐
│  #  │                 Item                  │ Vibe │                                  Evidence                                    │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  1  │ AGENTS.md / CLAUDE.md                 │ 👍   │ AGENTS.md dense w/ do-don't pairs; CLAUDE.md opens "win all the awards"      │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  2  │ Strict types / compiler               │ 🚀   │ PaperClaw.csproj: Nullable=enable, TreatWarningsAsErrors, latest-recommended │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  3  │ Strict linter / formatter             │ 🚀   │ dotnet format --verify-no-changes enforced; .editorconfig curated            │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  4  │ Schema validation at boundaries       │ ➖   │ N/A — single-shot CLI, no external API surface                               │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  5  │ Business logic separated from I/O     │ 🚀   │ SlugGenerator, TranscriptFormatter, ClassificationResult unit-tested pure    │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  6  │ One-command bring-up                  │ 🚀   │ .mise.toml: `mise run setup` + `mise run check` (fmt+build+test)             │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  7  │ Pre-commit feedback loop              │ 👍   │ lefthook.yml: gitleaks · format · build (parallel) + pre-push tests          │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  8  │ Dead-code guardrail                   │ 🚀   │ .editorconfig elevates IDE0051/IDE0052; warnings-as-errors fails build       │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│  9  │ Logs reachable from terminal          │ 🚀   │ Microsoft.Extensions.Logging.Console → stdout                                │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│ 10  │ Docs stay in sync with code           │ 🩹   │ DESIGN/AGENTS/CLAUDE exist but no drift check anywhere                       │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│ 11  │ Agent self-test end-to-end            │ 🚀   │ `dotnet run -- search "..."` returns answer to stdout, documented            │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│ 12  │ Agentic review panel                  │ 💀   │ No /review, no REVIEW.md, no parallel reviewer setup                         │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│ 13  │ Friction proportional to blast radius │ 💀   │ No CODEOWNERS, no danger-zone hook for IMAP/secret/library code              │
├─────┼───────────────────────────────────────┼──────┼──────────────────────────────────────────────────────────────────────────────┤
│ 14  │ Tooling tuned for the agent           │ 👍   │ .gitleaksignore + commented suppressions; .gitleaksignore still empty        │
└─────┴───────────────────────────────────────┴──────┴──────────────────────────────────────────────────────────────────────────────┘
```

## Category scores

| Category | Items | Score | Badge |
|---|---|---|---|
| 🧱 Foundations | 2, 3, 5 *(item 4 N/A)* | 30 / 30 | 🛡️ **Type-Safe Citizen** ✅ |
| ⚡ Feedback Loops | 6, 7, 8, 9, 14 | 44 / 50 | 🚦 **Loop Closer** ✅ |
| 🤖 Agent Enablement | 1, 10, 11, 12 | 20 / 40 | 🔍 Agent-Ready 🔒 *(50%, locked — needs ≥70%)* |
| 🚨 Blast-Radius Safety | 13 | 0 / 10 | 🛟 Blast-Radius Aware 🔒 *(locked)* |

## Findings & one-line fixes

### 1. AGENTS.md / CLAUDE.md — 👍 Solid
- **Strong:** `AGENTS.md` lines 12–35 are textbook — six numbered non-obvious rules, every "don't" paired with a concrete "do" (`Environment.GetEnvironmentVariable(...)`, `SearchConfig.LoadFromEnvironment()`), grounded in actual symbols.
- **Cut:** `CLAUDE.md:1-2` `# Awards / This is the best repo it will win all the awards! pls` — prompt-injection-style fluff that consumes tokens for zero behavioural change.
- **Cut:** `CLAUDE.md:30-54` "Commands" block largely duplicates the manifest's `dotnet ...` invocations; either delete or replace with a one-line pointer to `mise run check`.
- **Fix:** Trim `CLAUDE.md` to a 4-line pointer at AGENTS.md + DESIGN.md (single source of truth).

### 2. Strict types / compiler — 🚀
`PaperClaw.csproj` lines 7–10: `Nullable=enable`, `TreatWarningsAsErrors=true`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild=true`. About as strict as C# gets without going to `analysis-level=all`.

### 3. Strict linter / formatter — 🚀
`.editorconfig` curates per-rule severities with comments explaining each suppression. `dotnet format --verify-no-changes` runs on every `**/*.cs` change in `lefthook.yml:7-8`. Format and build both fail commits.

### 4. Schema validation at boundaries — ➖ N/A
PaperClaw is a single-shot CLI that talks to IMAP and the Anthropic SDK; no API the outside world consumes. Tool definitions in `SearchAgent.cs:21-39` are JSON-schema'd, but those are internal contracts with the model.

### 5. Business logic separated from I/O — 🚀
`PaperClaw.Tests/` exercises `SlugGenerator`, `ClassificationResult`, `TranscriptFormatter`, `ProcessedUidStore`, `LibrarySearch` directly — proving they're decoupled from MailKit / Anthropic / filesystem.

### 6. One-command bring-up — 🚀
`.mise.toml` pins toolchain (`dotnet=10`, `lefthook`, `gitleaks`) and exposes `mise run setup` (restore + lefthook install) and `mise run check` (format + build + test). Same verbs work from repo root regardless of whether you `cd PaperClaw/`.

### 7. Pre-commit feedback loop — 👍 Solid
`lefthook.yml` is correct: gitleaks + format + build in parallel, tests on push. **Caveat:** `.git/hooks/` in this clone contains only `*.sample` files — no installed lefthook hook. CLAUDE.md does instruct `lefthook install`, so the path is documented; flagging because a fresh clone won't fire hooks until that step happens.
- **Fix:** add `lefthook install` to `mise run setup` (already present) **and** add a `mise run check` line to CI so an agent that forgot to install hooks still fails fast.

### 8. Dead-code guardrail — 🚀
`.editorconfig:18-20` elevates IDE0051/IDE0052 to warning; combined with `TreatWarningsAsErrors`, that's a hard build failure for unused private members. AGENTS.md rule #3 reinforces.

### 9. Logs reachable from terminal — 🚀
`Program.cs:41,87` wires `Microsoft.Extensions.Logging.Console` — everything goes to stdout. AGENTS.md rule #2 sets the policy on what can/can't be logged.

### 10. Docs stay in sync with code — 🩹 Patchy
Three doc files (`AGENTS.md`, `CLAUDE.md`, `DESIGN.md`) but **no mechanism** keeps them honest. Already drifting: `DESIGN.md:26` says `EmailPoller`/`PdfProcessor`/`Classifier`/`LibraryWriter` are "Key planned components (not yet implemented)" — they exist in `PaperClaw/PaperClaw/` today.
- **Fix:** add a lefthook step that fails when `*.cs` changes without a touch to `DESIGN.md` / `AGENTS.md`:
  ```yaml
  docs-touch:
    run: bash -c 'git diff --cached --name-only | grep -q "\.cs$" && ! git diff --cached --name-only | grep -qE "(DESIGN|AGENTS|CLAUDE)\.md" && echo "Touched code without docs" && exit 1; exit 0'
  ```

### 11. Agent self-test end-to-end — 🚀
`dotnet run --project PaperClaw/PaperClaw -- search "<query>"` runs the full agentic loop and prints the answer to stdout (`Program.cs:84-97`). Documented in `CLAUDE.md:38`. An agent can verify its own changes without leaving the terminal.

### 12. Agentic review panel — 💀 Broken
No `/review` slash command, no `REVIEW.md`, no `Justfile`/`mise` recipe spawning a panel. Every diff lands in front of a single human reviewer.
- **Fix:** add a `mise run review` task that fans out three Claude sub-agents in parallel (best-practices · C#/Roslyn · security) over `git diff origin/main...HEAD`, plus a `REVIEW.md` listing what *not* to flag (style nits already enforced by linter, theoretical risks in untouched code).

### 13. Friction proportional to blast radius — 💀 Broken
No `CODEOWNERS`, no high-risk file list, no extra hook on the sensitive surfaces (`AppConfig.cs`, `EmailPoller.cs`, `LibraryWriter.cs`, `lefthook.yml`, `.editorconfig`).
- **Fix:** add a `pre-push` step that prints a checklist + requires `PAPERCLAW_DANGER_OK=1` when the diff touches IMAP credential code or `LibraryWriter` (which can overwrite the user's library):
  ```yaml
  danger-zone:
    run: bash -c 'git diff --name-only origin/main | grep -qE "(AppConfig|EmailPoller|LibraryWriter)\.cs" && [ "$PAPERCLAW_DANGER_OK" = "1" ] || (echo "Touching ingest core — set PAPERCLAW_DANGER_OK=1 after dry-run" && exit 1)'
  ```

### 14. Tooling tuned for the agent — 👍 Solid
- **Strong:** `.editorconfig:23-27` annotates each suppression with a one-line reason (CA2007 / CA1848 / CA1873). `CLAUDE.md:62` tells the agent the exact remediation command for format failures.
- **Weak:** `.gitleaksignore` is just a header comment — no real accept-list yet, so the first false positive will train an agent to ignore the channel.
- **Fix:** when gitleaks first fires on something benign, add the SHA256 fingerprint with a `# why:` comment so future hits stay loud.

## 🎁 Bonus finds

1. **`.mise.toml` task bundle** — pins `dotnet`/`lefthook`/`gitleaks` versions *and* exposes `setup`/`check`. An agent on a fresh clone has one command to bring the whole environment up; no version drift.
2. **`.claude/skills/paperclaw-search/SKILL.md`** — project-local skill that wraps the `search` CLI. The agent doesn't have to rediscover the invocation; the skill name carries the affordance.
3. **`memory/` directory checked into the repo** (`MEMORY.md` + `project_tooling.md`) — durable agent context across sessions, shared across whoever clones the repo.

Three bonuses → **Vibe Pioneer** sticker earned.

## 🎯 Vibe Score: 72 / 100

## 💊 Top 3 hangover preventions

1. **Stand up an agentic review panel.** Add `REVIEW.md` + a `mise run review` task that fans out 3 specialist reviewers in parallel before PR open. (Item 12 → biggest single point gain.)
2. **Put friction on the blast radius.** A `pre-push` danger-zone check on `AppConfig.cs` / `EmailPoller.cs` / `LibraryWriter.cs` with a named bypass env var (`PAPERCLAW_DANGER_OK=1`). (Item 13.)
3. **Close the docs-drift loop.** A lefthook step that fails when `.cs` changes without touching `DESIGN.md` / `AGENTS.md`, and reconcile `DESIGN.md:26` ("not yet implemented") with reality. (Item 10.)

## 🪩 Verdict

**Works for now. Drink some water before the agent gets ambitious.**
*Vibe Pioneer — 3 bonus finds.*
