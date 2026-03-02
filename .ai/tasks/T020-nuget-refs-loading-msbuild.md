# T020: Add NuGet Package Refs to Loading.MSBuild
- Milestone: M3
- Status: Done
- Agent: Claude (Executor)
- Started: 2026-03-02
- Completed: 2026-03-02

## Objective
Add `Microsoft.Build` and `Microsoft.Build.Locator` NuGet references to `src/Typewriter.Loading.MSBuild/Typewriter.Loading.MSBuild.csproj` so the project can load MSBuild project graphs without shipping conflicting MSBuild assemblies.

## Approach
Add both packages with a major-wildcard version constraint and apply `ExcludeAssets="runtime"` to `Microsoft.Build` so the MSBuild assemblies are not copied to the output directory — MSBuildLocator loads them at runtime from the SDK-installed location.

Key file: `src/Typewriter.Loading.MSBuild/Typewriter.Loading.MSBuild.csproj`

## Journey
### 2026-03-02
- Looked at `Microsoft.Build.Locator` docs: `RegisterDefaults()` must be called before any `Microsoft.Build.*` type is loaded. If the Microsoft.Build assemblies are in the output directory, the CLR may load them before `RegisterDefaults()` is called, causing version conflicts.
- The correct pattern is `ExcludeAssets="runtime"` on the `Microsoft.Build` reference so only the compile-time reference is present; at runtime MSBuildLocator resolves the correct version from the SDK.
- Chose major-wildcard versions (`17.*` for Microsoft.Build, `1.*` for Microsoft.Build.Locator) rather than pinning exact patch versions. This prevents lock-step bumps for every MSBuild SDK patch while MSBuildLocator still controls which DLL is actually loaded at runtime.
- Added inline XML comment explaining the `ExcludeAssets="runtime"` rationale for future maintainers.
- `dotnet restore` resolved successfully; `dotnet build -c Release` 0 errors, 0 warnings; 129/129 tests pass.

## Outcome
`Typewriter.Loading.MSBuild.csproj` contains:
```xml
<PackageReference Include="Microsoft.Build" Version="17.*" ExcludeAssets="runtime" />
<PackageReference Include="Microsoft.Build.Locator" Version="1.*" />
```
Build and test pass. See D-0008 for the formal decision record.

## Follow-ups
- T025: implement `MsBuildLocatorService` with the one-shot guard that calls `MSBuildLocator.RegisterDefaults()` exactly once per process (see D-0009).
