# T318: Implement dual release artifacts (`dotnet tool` + `exe`)
- Milestone: Post
- Status: Done
- Agent: Codex (Executor)
- Started: 2026-03-10
- Completed: 2026-03-10

## Objective
Implement release pipeline support for publishing both the NuGet `dotnet tool` package and framework-dependent single-file executable bundles, while preserving existing NuGet publish flow.

## Approach
Update `.github/workflows/release.yml` to add a RID matrix executable publish job and attach resulting archives to GitHub releases. Keep current tool packaging/publish steps unchanged. Update `README.md` release docs to describe both artifact channels and install/run commands.

## Journey
### 2026-03-10 - Initial implementation pass
- Read current release/CI workflows and CLI project packaging settings.
- Confirmed current release pipeline only creates/uploads `*.nupkg` artifacts and pushes NuGet package.
- Confirmed CLI project already supports both executable output and `PackAsTool` packaging.
- Planned workflow changes:
  - add `publish-exe` job for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`
  - run `dotnet publish` with `--self-contained false -p:PublishSingleFile=true -p:PublishTrimmed=false`
  - smoke-run executable on runnable x64 host architectures
  - zip per-RID outputs and upload as `exe-<rid>` artifacts
  - update `github-release` to download/attach both `nupkg` and executable archives

### 2026-03-10 - Workflow and docs updates
- Updated `.github/workflows/release.yml`:
  - added `publish-exe` job with RID matrix:
    - `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`
  - added per-RID publish command:
    - `dotnet publish ... --self-contained false -p:PublishSingleFile=true -p:PublishTrimmed=false`
  - added publish-output existence check for each RID
  - added executable smoke tests for runnable x64 host-arch entries
  - added per-RID versioned zip archive creation and `exe-<rid>` artifact upload
  - updated `github-release` to depend on both `publish` and `publish-exe`
  - updated `github-release` to attach both `./artifacts/nupkg/*.nupkg` and `./artifacts/exe/*.zip`
- Updated `README.md` release section:
  - documented dual-channel release artifacts (NuGet tool + GitHub executable archives)
  - added install/use examples for both channels

### 2026-03-10 - Verification attempts
- Attempted required commands:
  - `dotnet restore`
  - `dotnet build -c Release`
  - `dotnet test -c Release`
  - `dotnet pack -c Release`
- Observed sandbox failure mode:
  - restore/build/pack returned `Build FAILED` with `0 Error(s)` and no actionable diagnostics
  - diagnostic capture for restore showed failure at `_FilterRestoreGraphProjectInputItems` while restoring `Typewriter.Cli.slnx`
  - one `dotnet test` run hung and was user-aborted; orphaned `dotnet` processes were terminated to clean the session
- Conclusion:
  - code/workflow/doc changes completed
  - full verification remains blocked in this sandbox environment and must be re-run in stable local/CI environment

## Outcome
Implemented:
- `.github/workflows/release.yml`
- `README.md`
- `.ai/progress.md`
- `.ai/tasks/T318-dual-release-artifacts.md`

Verification:
- Required verification commands were attempted but did not complete successfully in this sandbox due non-actionable MSBuild failure mode (documented above and in `.ai/progress.md` roadblocks).

## Follow-ups
- Run required verification commands after workflow/doc changes:
  - `dotnet restore`
  - `dotnet build -c Release`
  - `dotnet test -c Release`
  - `dotnet pack -c Release`
- Re-run a tag-based release dry-run in CI to validate `publish-exe` matrix artifacts and GitHub release attachments.
