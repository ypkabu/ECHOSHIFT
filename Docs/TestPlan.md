# Test Plan

## EditMode

The 60-case suite retains all Phase 0-3 cases and adds Japanese-localization and High-fix regression coverage for:

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
- immutable Loop history values, bounded rollover, Evicted state without runtime references, and one-tick Door application;
- legal and illegal game-state transitions, duplicate-transition rejection, and paused simulation clocks;
- section/game restart state, authored reset baselines, and replay-reference cleanup;
- per-section tutorial progress and keyboard/gamepad prompt selection;
- telemetry aggregation, deterministic schema, immutable snapshot arrays, and exclusion of replay payloads;
- complete centralized text catalog values;
- repeatable P3 Scene generation, unchanged P0-P2 Scene hashes, unique Stable IDs, and P3/P2/P1/P0 build order;
- required Japanese catalog values, no legacy English player text, complete and serialized-replaceable failure mappings, Japanese keyboard/gamepad prompts, and unique stable keys;
- repeated P3 Scene generation retaining Japanese catalog/font/EventSystem references, valid glyphs, responsive wrapping, and Pause button fit.

## PlayMode

The 54-case suite retains all P0-P3 coverage and adds Japanese/localization and black-box regression tests:

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
- a tick-driven three-loop real-Scene solution proving Echo 1 Plate/Gate A, Echo 2 Battery/Socket/Gate B, two simultaneous Echoes, Goal completion, drift tolerance, and zero unexpected interaction failures;
- the actual generated P3 Scene completing all three sections in sequence without Scene reload;
- section cleanup, active-root switching, two simultaneous Echoes, four exact successful interactions, zero failures, and drift tolerance;
- pause freezing timer, Player, Echo/Door simulation and resuming on the same tick;
- restart section and restart game scope, Loop/tick/history reset, and no stale Echoes;
- keyboard/gamepad prompts, reason-specific interaction feedback, and hidden debug overlay not affecting simulation;
- quit intent, telemetry JSON persistence, Completed-state replay suppression, and P0-P3 Scene load regression;
- recursive Missing Component traversal of the generated P3 Scene;
- Japanese objectives for all three sections, Japanese loop transition/completion/failure/Pause text, and no English placeholder tutorial with Debug Overlay OFF;
- staged Section 1/2 Japanese guidance, Restart guidance reset, Battery/Socket/Drop-specific prompts, live no-target interaction failure routing, and no Pickup/Drop false-positive completion hint;
- fixed Camera rotation during Player follow, low visible foreground wall plus full collision boundary, Player/device world labels, and Echo generation identity labels/rings;
- generated P3 starting with Debug Overlay OFF, device-appropriate movement prompt before an interaction target, and EventSystem pointer/keyboard submit paths for Pause actions.

## Batch commands

From the repository root in PowerShell:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine -logFile "$PWD\Logs\phase3-scene-builder.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\phase3-edit.xml" -logFile "$PWD\Logs\phase3-edit.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\phase3-play.xml" -logFile "$PWD\Logs\phase3-play.log"
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase3BuildPipeline.BuildWindowsDevelopment -logFile "$PWD\Logs\phase3-build.log"
& "$PWD\Builds\Phase3\ECHOSHIFT_Phase3.exe" -batchmode -nographics -phase3AutoQuit -logFile "$PWD\Logs\phase3-standalone.log"
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase3BuildPipeline.BuildJapaneseWindowsDevelopment -logFile "$PWD\Logs\phase3-ja-build.log"
& "$PWD\Builds\Phase3-JA\ECHOSHIFT_Phase3_JA.exe" -batchmode -nographics -phase3AutoQuit -logFile "$PWD\Logs\phase3-ja-standalone.log"
```

Review XML contents and logs. A process exit code alone is not sufficient evidence. Do not add `-quit` to test commands: Unity Test Framework exits after writing XML, while an early generic quit can occur before the runner starts.

`Phase3BuildPipeline` regenerates P3, builds P3/P2/P1/P0 in that order, targets Windows x86_64, and enables Development Build. The standard and Japanese methods write `Builds/Phase3/ECHOSHIFT_Phase3.exe` and `Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe` respectively. `-phase3AutoQuit` is probe-only and does not affect ordinary play.

## Latest verified results

Executed with Unity `6000.4.6f1` on 2026-07-20:

- Phase 3 Scene Builder: completed repeatedly; P0-P2 Scene hashes unchanged, unique Stable IDs, P3/P2/P1/P0 Build Settings order, no serialized Missing Script marker.
- EditMode: 60 passed, 0 failed, 0 skipped; duration 1.6551463 seconds.
- PlayMode: 54 passed, 0 failed, 0 skipped; duration 14.6311932 seconds.
- Phase 0 integration maximum drift: `0 m`.
- Phase 1 integration maximum drift: `0 m`; Echo interaction success `2`, failure `0`.
- Phase 2 integration maximum drift: `0 m`; Echo interaction success `2`, failure `0`; Goal reached on Loop 3 tick 291.
- Phase 3 integration maximum drift: `0 m`; interaction success `4`, failure `0`; all sections completed in 1,081 tick advances.
- Japanese Windows x86_64 Development Build: BuildReport succeeded, zero warnings, 166,327,358 bytes; the 290-file output tree totals 166,522,490 bytes at `Builds/Phase3-JA`.
- Headless Japanese Standalone probe: P3 reached Playing with `language=ja-JP`, `font=Noto Sans JP`, `glyphs=True`, HUD, and telemetry; it saved schema 1 JSON, requested the normal quit path, and exited naturally with code 0. Matched exception/font/glyph/missing-reference messages were zero.
- Raw logs and test XML stay ignored because they contain machine-specific paths/timestamps and are reproducible from the documented commands. Summary evidence is committed in `Docs/Phase3JapaneseLocalizationValidation.md`.
