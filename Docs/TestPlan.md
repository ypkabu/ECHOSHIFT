# Test Plan

## EditMode

The 23-case suite retains the 12 Phase 0 cases and covers:

- replay frame ordering, capacity, overflow rejection, immutable finalization, and short prefixes;
- LoopSettings validation and drift monitoring;
- empty and duplicate Stable ID detection;
- Registry exact-ID resolution and stale removal;
- ordered, immutable interaction-event finalization and recorded-frame bounds;
- deterministic Stable-ID candidate tie-break;
- Battery double-ownership rejection and Battery/Socket reset consistency;
- required keyboard and gamepad bindings in the Input Actions asset.

## PlayMode

The 26-case suite retains the 12 Phase 0 cases and covers:

- unchanged movement replay, pressure-plate lifecycle cleanup, actor collision matrix, short replay, Echo cap, and generated Phase 0 flow;
- Player pickup, carry-socket following, deterministic drop, Socket insertion, and powered Door opening;
- Player/Echo ownership conflict rejection;
- Actor disable/destruction cleanup without Battery destruction;
- target disable/unregister/re-enable lifecycle;
- explicit Battery/Socket/Door world reset consistency;
- exact recorded-ID Echo pickup and Socket insertion;
- missing recorded target failure without nearby-target fallback;
- interaction cursor stopping at a short recording's end;
- generated `P1_InteractionLab` references, Stable IDs, Reset Registry, overlay, two-loop interaction replay, Door opening, current-Player Goal completion, unexpected-log absence, interaction counts, and drift.

## Batch commands

From the repository root in PowerShell:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.P1SceneBuilder.BuildFromCommandLine -logFile "$PWD\Logs\phase1-scene-builder.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\phase1-edit.xml" -logFile "$PWD\Logs\phase1-edit.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\phase1-play.xml" -logFile "$PWD\Logs\phase1-play.log"
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase1BuildPipeline.BuildWindowsDevelopment -logFile "$PWD\Logs\phase1-build.log"
```

Review XML contents and logs. A process exit code alone is not sufficient evidence. Do not add `-quit` to test commands: Unity Test Framework exits after writing XML, while an early generic quit can occur before the runner starts.

`Phase1BuildPipeline` regenerates Phase 0 and Phase 1 Scenes, builds `P1_InteractionLab` as startup Scene, retains `P0_ReplayLab` as the second Scene, targets Windows x86_64, enables Development Build, and writes `Builds/Phase1/ECHOSHIFT_Phase1.exe`.

## Latest verified results

Executed with Unity `6000.4.6f1` on 2026-07-19:

- Phase 1 Scene Builder: completed; generated Scene contains exactly the authored Battery and PowerSocket Stable IDs.
- EditMode: 23 passed, 0 failed, 0 skipped.
- PlayMode: 26 passed, 0 failed, 0 skipped.
- Phase 0 integration maximum drift: `0 m`.
- Phase 1 integration maximum drift: `0 m`; Echo interaction success `2`, failure `0`.
- Windows x86_64 Development Build: return code `0`, BuildReport succeeded, zero warnings, expected EXE/Data output generated.
- Headless Standalone smoke: ran for 20 seconds, reached engine/Input System/PhysX/Scene initialization, then the audit stopped the process; matched exception, error, missing-script, and missing-reference counts were all zero.
- Raw logs and test XML stay ignored because they contain machine-specific paths/timestamps and are reproducible from the documented commands. Summary evidence is committed in `Docs/Phase1Validation.md`.
