# F-0013: COM, Registry, and Windows-only Assumptions Cluster

- ID: F-0013
- Title: Windows-specific behavior in runtime and tests
- Date: 2026-02-19

## Context
The target CLI must be cross-platform, so all Windows-only assumptions need explicit handling.

## Evidence
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:11` imports `Microsoft.Win32`.
- `origin/src/Typewriter/VisualStudio/ExtensionPackage.cs:229` writes icon association under `HKCU\Software\Classes`.
- `origin/src/Typewriter/Generation/Template.cs:8` imports `Microsoft.Win32`.
- `origin/src/Typewriter/Generation/Template.cs:557` checks `HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled`.
- `origin/src/Typewriter/Generation/Template.cs:173` uses Windows long-path prefix (`\\?\`).
- `origin/src/Typewriter/VisualStudio/ContextMenu/RenderTemplate.cs:143` uses COM marshal (`Marshal.GetObjectForIUnknown` / `Marshal.Release`).
- `origin/src/Tests/TestInfrastructure/MessageFilter.cs:41` P/Invoke `Ole32.dll` (`CoRegisterMessageFilter`) and COM interface definitions.
- `origin/src/Tests/TestInfrastructure/Dte.cs:10`/`:13` P/Invoke `ole32.dll` for running object table access (commented block).

## Conclusion
Several runtime behaviors and test infrastructure components are Windows/COM/registry-specific and cannot be carried directly into a cross-platform CLI.

## Impact
- Classification:
  - Hard (to remove/replace): registry icon registration, COM-based selection/services, Windows long-path registry gating.
  - Soft: Windows-only IDE niceties.
  - Accidental: some test harness COM infrastructure.
- CLI replacement:
  - Use standard cross-platform filesystem APIs and path normalization.
  - Do not depend on registry for long-path behavior; rely on runtime/platform capabilities.
  - Remove COM/DTE selection logic entirely from CLI scope.

## Next steps
- Document these as non-CLI features or parity gaps with mitigation.
- Add risk/open question entries where behavior may differ on Windows vs Linux/macOS.
