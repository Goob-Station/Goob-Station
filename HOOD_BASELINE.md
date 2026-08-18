# Hood fork baseline

Captured on 2026-08-11 before Hood implementation work.

## Repository

- Branch: `master`
- Commit: `464b90c42c Automated changelog update (#6702)`
- Remote: `origin https://github.com/Goob-Station/Goob-Station.git`
- The pinned `RobustToolbox` submodule was initialized at
  `68f8d00931d6b14f3e592d50c47dd44ef09eed1f`.
- Pre-existing worktree changes, intentionally preserved:
  - deleted `REUSE.toml`
  - deleted `bors.toml`

No upstream merge or rebase was performed.

## CI commands

The commands below come from `.github/workflows/build-test-debug.yml`:

```powershell
dotnet restore
dotnet build --configuration DebugOpt --no-restore /m
dotnet test --no-build --configuration DebugOpt Content.Tests/Content.Tests.csproj -- NUnit.ConsoleOut=0
$env:DOTNET_gcServer=1
dotnet test --no-build --configuration DebugOpt Content.IntegrationTests/Content.IntegrationTests.csproj -- NUnit.ConsoleOut=0 NUnit.MapWarningTo=Failed
```

## Baseline results

- Restore: passed, with existing NuGet trim and vulnerability warnings.
- DebugOpt build: passed with 0 errors and 1,616 warnings.
- `Content.Tests`: passed; 372 passed, 1 skipped, 0 failed.
- `Content.IntegrationTests`: baseline infrastructure timeout. The test host
  terminated after 18 minutes 46 seconds with `Tests took way too`; 113 tests
  had passed and 0 had failed before termination. The runner printed its
  active pool death report rather than an assertion failure.

The integration timeout predates all Hood source/prototype changes and must be
kept distinct from failures introduced by this fork.

## Fork isolation

New fork-owned code and resources should live under `_Hood` directories where
the existing project dependency direction permits it. Small integration edits
to upstream-owned files are acceptable where the shared profile, database,
inventory, or UI architecture requires them; those edits should delegate to
isolated Hood policy/components rather than duplicate upstream systems.
