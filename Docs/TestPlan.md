# Test Plan

## EditMode

The 35-case suite retains all Phase 0 and Phase 1 cases and additionally covers:

- replay frame ordering, capacity, overflow rejection, immutable finalization, and short prefixes;
- LoopSettings validation and drift monitoring;
- empty and duplicate Stable ID detection;
- Registry exact-ID resolution and stale removal;
- ordered, immutable interaction-event finalization and recorded-frame bounds;
- deterministic Stable-ID candidate tie-break;
- Battery double-ownership rejection and Battery/Socket reset consistency;
- required keyboard and gamepad bindings in the Input Actions asset;
- P2 15-second/900-tick settings, recorder preallocation, invalid settings, and the 36,000-tick limit;
- Actor generation ordering, oldest-Echo conflict priority, Player-last priority, Stable-ID tie-break, `TargetBusy`, and no fallback;
- immutable Loop history values, bounded rollover, Evicted state without runtime references, and one-tick Door application.

## PlayMode

The 32-case suite retains all P0/P1 coverage and adds:

- unchanged movement replay, pressure-plate lifecycle cleanup, actor collision matrix, short replay, Echo cap, and generated Phase 0 flow;
- Player pickup, carry-socket following, deterministic drop, Socket insertion, and powered Door opening;
- Player/Echo ownership conflict rejection;
- Actor disable/destruction cleanup without Battery destruction;
- target disable/unregister/re-enable lifecycle;
- explicit Battery/Socket/Door world reset consistency;
- exact recorded-ID Echo pickup and Socket insertion;
- missing recorded target failure without nearby-target fallback;
- interaction cursor stopping at a short recording's end;
- generated `P1_InteractionLab` references, Stable IDs, Reset Registry, overlay, two-loop interaction replay, Door opening, current-Player Goal completion, unexpected-log absence, interaction counts, and drift;
- generated `P2_CoordinationLab` references, collision matrix, deterministic Battery conflict, four-Replay eviction cleanup, and P0/P1 load regression;
- a tick-driven three-loop real-Scene solution proving Echo 1 Plate/Gate A, Echo 2 Battery/Socket/Gate B, two simultaneous Echoes, Goal completion, drift tolerance, and zero unexpected interaction failures.

## Batch commands

From the repository root in PowerShell:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.P2SceneBuilder.BuildFromCommandLine -logFile "$PWD\Logs\phase2-scene-builder.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\phase2-edit.xml" -logFile "$PWD\Logs\phase2-edit.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\phase2-play.xml" -logFile "$PWD\Logs\phase2-play.log"
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase2BuildPipeline.BuildWindowsDevelopment -logFile "$PWD\Logs\phase2-build.log"
```

Review XML contents and logs. A process exit code alone is not sufficient evidence. Do not add `-quit` to test commands: Unity Test Framework exits after writing XML, while an early generic quit can occur before the runner starts.

`Phase2BuildPipeline` regenerates required Scenes, builds P2/P1/P0 in that order, targets Windows x86_64, enables Development Build, and writes `Builds/Phase2/ECHOSHIFT_Phase2.exe`.

## Latest verified results

Executed with Unity `6000.4.6f1` on 2026-07-19:

- Phase 2 Scene Builder: completed; generated Scene retains stable Battery/PowerSocket IDs and P2/P1/P0 Build Settings order.
- EditMode: 35 passed, 0 failed, 0 skipped.
- PlayMode: 32 passed, 0 failed, 0 skipped.
- Phase 0 integration maximum drift: `0 m`.
- Phase 1 integration maximum drift: `0 m`; Echo interaction success `2`, failure `0`.
- Phase 2 integration maximum drift: `0 m`; Echo interaction success `2`, failure `0`; Goal reached on Loop 3 tick 291.
- Windows x86_64 Development Build: return code `0`, BuildReport succeeded, zero warnings, expected EXE/Data output generated at `Builds/Phase2`.
- Headless Standalone smoke: ran for 30 seconds, reached Input System, PhysX, and `P2_CoordinationLab`; startup logged 60 Hz, 15 seconds, 900 ticks, max Echoes 3, drift tolerance 0.05, Player present, and initial Echo count 0. The audit then stopped the process because the game has no automatic quit path. Matched exception, error, missing-script, and missing-reference counts were zero.
- Raw logs and test XML stay ignored because they contain machine-specific paths/timestamps and are reproducible from the documented commands. Summary evidence is committed in `Docs/Phase2Validation.md`.
