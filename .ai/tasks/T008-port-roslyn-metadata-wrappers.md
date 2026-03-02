# T008: Port Roslyn Metadata Wrappers and Extensions.cs
- Milestone: M1
- Status: Done
- Agent: Executor (claude-sonnet-4-6)
- Started: 2026-03-02
- Completed: 2026-03-02

## Objective

Copy `origin/src/Roslyn/Extensions.cs` and all `Roslyn*Metadata.cs` wrapper files
(excluding `RoslynMetadataProvider.cs` and `RoslynFileMetadata.cs`) into
`src/Typewriter.Metadata.Roslyn/`, adapting namespaces and usings for the ported project.

## Approach

1. Read all 19 source files from `origin/src/Roslyn/`.
2. Identify that `RoslynFileMetadata` (deferred) is referenced by 3 files (Class, Interface, Record metadata).
3. Create a minimal, VS-free stub for `RoslynFileMetadata` with just `Settings` + `FullName` properties.
4. Identify that `Settings.PartialRenderingMode` (used by the 3 files) required `PartialRenderingMode` on the base `Settings` class — which was blocked by the circular-dependency constraint in T007.
5. Resolve: move `PartialRenderingMode.cs` from `Typewriter.CodeModel` project to `Typewriter.Metadata` project (keeping `namespace Typewriter.Configuration`). Add property to base `Settings`.
6. Copy all 19 files, replacing `using Typewriter.Configuration;` (for Settings) with `using Typewriter.Metadata;`. Keep `using Typewriter.Configuration;` in the 3 files that reference `PartialRenderingMode` enum values.
7. Add `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.Workspaces.Common` NuGet refs.
8. Remove `Placeholder.cs`.

## Journey

### 2026-03-02

- Read all origin Roslyn files. All 19 target files already use `namespace Typewriter.Metadata.Roslyn`.
- Origin `Settings` is in `Typewriter.Configuration`; our `Settings` is in `Typewriter.Metadata` — all
  `using Typewriter.Configuration;` directives for `Settings` must become `using Typewriter.Metadata;`.
- T007 noted that `PartialRenderingMode` couldn't be on base `Settings` due to circular dependency.
  Resolution: The enum is a standalone type with no dependencies; it was in `Typewriter.CodeModel`
  but CAN live in `Typewriter.Metadata` (which `Typewriter.CodeModel` already depends on). Moved the
  file; kept `namespace Typewriter.Configuration` for API compatibility.
- Removed `PartialRenderingMode` property from `SettingsImpl` (now inherited from base).
- Removed `using Typewriter.Configuration;` from `SettingsImpl.cs` (now unused → would be CS8019 error
  with `TreatWarningsAsErrors=true`).
- dotnet toolchain not present; build deferred to CI.

## Outcome

Files created or modified:

| File | Action |
|------|--------|
| `src/Typewriter.Metadata/PartialRenderingMode.cs` | Moved from `Typewriter.CodeModel/Configuration/` (namespace unchanged) |
| `src/Typewriter.Metadata/Settings.cs` | Added `PartialRenderingMode` property; updated doc comment |
| `src/Typewriter.CodeModel/Configuration/SettingsImpl.cs` | Removed duplicate `PartialRenderingMode` property; removed unused `using Typewriter.Configuration;` |
| `src/Typewriter.Metadata.Roslyn/Typewriter.Metadata.Roslyn.csproj` | Added `Microsoft.CodeAnalysis.CSharp 4.*` and `Microsoft.CodeAnalysis.Workspaces.Common 4.*` |
| `src/Typewriter.Metadata.Roslyn/RoslynFileMetadata.cs` | Created: minimal stub (Settings + FullName, no VS coupling) |
| `src/Typewriter.Metadata.Roslyn/Extensions.cs` | Created: ported from origin |
| `src/Typewriter.Metadata.Roslyn/Roslyn*Metadata.cs` (18 files) | Created: ported from origin |
| `src/Typewriter.Metadata.Roslyn/Placeholder.cs` | Deleted |

No VS/EnvDTE/VisualStudio references in any ported file. `origin/` unchanged.

## Follow-ups

- M5: Replace `RoslynFileMetadata` stub with full implementation (no more `ThreadHelper.JoinableTaskFactory`).
