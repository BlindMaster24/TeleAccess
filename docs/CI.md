# Continuous integration

Everything runs on GitHub Actions. Documentation is not part of any pipeline: no job
builds or lints markdown, because a broken link does not break the client.

| Workflow | Trigger | What it proves |
| --- | --- | --- |
| `ci` | push to `main`, pull request, manual | Release build with warnings as errors, formatting unchanged, all tests green with coverage, portable single-file artifact for `win-x64` and `win-arm64` |
| `codeql` | push to `main`, pull request, weekly, manual | Security analysis of C# sources |
| `dependencies` | pull request, weekly, manual | Dependency diff review on pull requests (fails on moderate or worse additions); report of outdated packages, with an issue opened when newer versions exist |

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

## Why lock files are committed

GitHub's dependency graph reads NuGet dependencies from `packages.lock.json`, not from
`PackageReference` entries. Without those files the dependency review job cannot see the
graph and refuses to run. `RestorePackagesWithLockFile` keeps them current, and CI runs in
`RestoreLockedMode`, so a version bump that forgets the lock file fails the build instead
of drifting silently.

Publish is the one place that restores outside locked mode. Publishing a single file adds
`Microsoft.NET.ILLink.Tasks` to the graph, and that implicit reference cannot exist in a
lock file produced by a normal restore. The build and test gate stays locked, so versions
are pinned where it matters; publish only adds the linker the SDK injects itself.

## What is deliberately not here

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
