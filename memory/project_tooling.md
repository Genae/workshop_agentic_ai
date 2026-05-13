---
name: project-tooling
description: PaperClaw scaffold: lefthook for pre-commit hooks, gitleaks + dotnet format + build on commit, tests on push
metadata:
  type: project
---

Bootstrapped with bootstrap-project skill on 2026-05-13.

**Why:** Replaced manual `.githooks/pre-commit` bash script with lefthook so hooks are declarative, parallelized, and easy to extend.

**How to apply:** When touching the pre-commit setup, edit `lefthook.yml`. Don't edit `.git/hooks/pre-commit` directly — lefthook regenerates it. Run `lefthook install` after cloning.

Key decisions:
- `git config core.hooksPath` was reset to default (`.git/hooks`); lefthook installs there.
- `.githooks/` directory still exists but is no longer the active hooks path.
- `TreatWarningsAsErrors=true` + `AnalysisLevel=latest-recommended` on main project; CA2007 suppressed (ConfigureAwait not needed in app code).
- Anthropic.SDK 5.10.0 added as the HTTP boundary tool for Claude API calls.
- `PAPERCLAW_LOG_LEVEL` env var controls Microsoft.Extensions.Logging verbosity.
- gitleaks installed via winget (Gitleaks.Gitleaks), lefthook via npm (@evilmartians/lefthook).
