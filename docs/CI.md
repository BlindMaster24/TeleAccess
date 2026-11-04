# Continuous integration

Everything runs on GitHub Actions. Documentation is not part of any pipeline: no job
builds or lints markdown, because a broken link does not break the client.

| Workflow | Trigger | What it proves |
| --- | --- | --- |
| `ci` | push to `main`, pull request, manual | Release build with warnings as errors, formatting unchanged, all tests green with coverage, portable single-file artifact for `win-x64` and `win-arm64` |
| `codeql` | push to `main`, pull request, weekly, manual | Security analysis of C# sources |
| `dependencies` | pull request, weekly, manual | Report of outdated packages; opens an issue when newer versions exist |

## What the build gate means

A pull request is green only when all of this holds:

1. `dotnet build -c Release` succeeds — analyzers (NetAnalyzers, Meziantou, Roslynator,
   Sonar) run as errors, so a warning breaks the build.
2. `dotnet format --verify-no-changes` reports no diff.
3. `dotnet test` passes and produces a Cobertura coverage file.
4. `dotnet publish` produces a single self-contained executable for both architectures.

## Artifacts

- `coverage` — Cobertura report from the test run.
- `teleaccess-win-x64`, `teleaccess-win-arm64` — portable executables.

## What is deliberately not here

- **Dependency diff review.** `actions/dependency-review-action` refuses to run on this
  repository because the dependency graph is not available to the action. An always-red
  check teaches people to ignore checks, so the job is gone rather than disabled.
- **Documentation linting.** A broken link does not break the client; markdown is not
  built or checked.

## Updating dependencies

Dependabot proposes weekly updates for NuGet packages and GitHub Actions. Analyzer
updates are grouped into one pull request to keep review cheap. The `dependencies`
workflow opens an issue when a newer version exists, so "always the latest version" is
tracked by the repository and not by memory.

## Adding a job

Add it to `.github/workflows/`, keep it on `windows-latest` when it touches the solution
(the target framework is Windows-only), and record it in the table above. A workflow that
is not in this table does not exist.
