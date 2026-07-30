# Test Plan

**Phase 4.2: Automation Passed; repeat Human Visual Review pending**

## EditMode

The 61-case suite retains all Phase 0-3 cases and adds Japanese-localization and High-fix regression coverage for:

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

The 61-case suite retains all P0-P3 coverage and adds Japanese/localization and black-box regression tests:

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
- fixed Camera rotation during Player follow, low visible foreground wall plus full collision boundary, and serialized Player/device/Echo identity feedback; Phase 4.1 separately verifies that persistent world-label renderers stay hidden;
- generated P3 starting with Debug Overlay OFF, device-appropriate movement prompt before an interaction target, and EventSystem pointer/keyboard submit paths for Pause actions.
- the exact Section 1 Plate/tutorial boundary and Echo replay from the accepted endpoint;
- post-completion Escape/Select Pause, Resume back to Completed, disabled completed-section restart, and the non-quitting two-step Quit test seam.

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
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase3BuildPipeline.BuildFinalWindowsDevelopment -logFile "$PWD\Logs\phase3-final-build.log"
& "$PWD\Builds\Phase3-Final\ECHOSHIFT_Phase3_Final.exe" -batchmode -nographics -phase3AutoQuit -logFile "$PWD\Logs\phase3-final-standalone.log"
```

Review XML contents and logs. A process exit code alone is not sufficient evidence. Do not add `-quit` to test commands: Unity Test Framework exits after writing XML, while an early generic quit can occur before the runner starts.

`Phase3BuildPipeline` regenerates P3, builds P3/P2/P1/P0 in that order, targets Windows x86_64, and enables Development Build. The standard, Japanese, and final methods write `Builds/Phase3/ECHOSHIFT_Phase3.exe`, `Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe`, and `Builds/Phase3-Final/ECHOSHIFT_Phase3_Final.exe`. `-phase3AutoQuit` is probe-only and does not affect ordinary play.

## Latest verified results

Executed with Unity `6000.4.6f1` on 2026-07-21 after the last code change:

- Phase 3 Scene Builder: completed repeatedly; P0-P2 Scene hashes unchanged, unique Stable IDs, P3/P2/P1/P0 Build Settings order, no serialized Missing Script marker.
- EditMode: 61 passed, 0 failed, 0 skipped; duration 1.8848759 seconds.
- PlayMode: 61 passed, 0 failed, 0 skipped; duration 16.4244913 seconds.
- Phase 0 integration maximum drift: `0 m`.
- Phase 1 integration maximum drift: `0 m`; Echo interaction success `2`, failure `0`.
- Phase 2 integration maximum drift: `0 m`; Echo interaction success `2`, failure `0`; Goal reached on Loop 3 tick 291.
- Phase 3 integration maximum drift: `0 m`; interaction success `4`, failure `0`; all sections completed in 1,081 tick advances.
- Final Windows x86_64 Development Build: process exit code 0, BuildReport succeeded with zero warnings, 289 files and 166,334,282 bytes at `Builds/Phase3-Final`; EXE SHA-256 `098A43C3B20762E4BDF938771C36F0FB116126AEC8932B2A77EB403F0CB77938`.
- Headless final Standalone probe: P3 reached Playing with `language=ja-JP`, `font=Noto Sans JP`, `glyphs=True`, HUD, and telemetry; it generated one schema-1 JSON, requested the normal quit path, and exited naturally with code 0. Matched Error, Exception, assertion, Missing Script/Reference, and NullReference messages were zero.
- Source-blind Fresh7 graphical retest: `PRETEST PASS`; 10:53.2 total, sections 2:08 / 3:33 / 4:24, loops 2 / 3 / 3, restarts 0, post-completion Pause and visible two-step Quit successful, Critical/High 0.
- Human acceptance: full completion and every recorded gameplay/control/comprehension/visibility/Japanese/Restart/Pause/Quit check passed; Critical 0, High 0, Medium 1, Low 0. Unrecorded environment and timing metrics remain unrecorded. The Medium visual-simplicity observation is deferred to `Phase4Backlog.md`.
- Raw logs and test XML stay ignored because they contain machine-specific paths/timestamps and are reproducible from the documented commands. Summary evidence is committed in `Docs/Phase3JapaneseLocalizationValidation.md`.

## Phase 4 additions

The retained suites are not removed, disabled, or relaxed. Phase 4 adds 23 EditMode cases covering visual settings/material references, distinct and cycling Echo slots, safe Volume values, pooled feedback, complete Audio cues, packaged legacy/TMP font coverage, no runtime OS-font construction, license presence, shared-material/PropertyBlock discipline, unique builder outputs, capture resolution, and immutable SHA-256 values for P0-P2 Scenes.

The 26 Phase 4 PlayMode cases load the real generated P3 Scene and cover its three sections, modular facility roots, compound Player/Echo visuals, device/door adapters, bounded feedback/audio pools, packaged Japanese font resolution, three HUD reference resolutions, Pause layout, HDR/post-processing, local-light limits, no presentation colliders, Plate/Door/Battery/Socket state propagation, maximum three-Echo identity cycling and eviction feedback, Goal feedback de-duplication, short-loop Drift, and unexpected-log absence. Existing tests continue to cover P0-P3 real-Scene load, P3 automatic completion, normal-route interaction failure `0`, Drift tolerance, Missing components/references, and lifecycle behavior.

## Phase 4 batch commands

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine -logFile "$PWD\Logs\Phase4-SceneBuilder.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\Phase4-Automation-EditMode.xml" -logFile "$PWD\Logs\Phase4-Automation-EditMode.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\Phase4-Automation-PlayMode.xml" -logFile "$PWD\Logs\Phase4-Automation-PlayMode.log"
& $unity -batchmode -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4CapturePipeline.CaptureAllFromCommandLine -logFile "$PWD\Logs\Phase4-Captures.log"
& $unity -batchmode -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4BuildPipeline.BuildWindowsDevelopment -logFile "$PWD\Logs\Phase4-Build.log"
& "$PWD\Builds\Phase4\ECHOSHIFT_Phase4.exe" -batchmode -force-d3d11 -phase3AutoQuit -screen-width 1920 -screen-height 1080 -logFile "$PWD\Logs\Phase4-Startup.log"
& "$PWD\Builds\Phase4\ECHOSHIFT_Phase4.exe" -batchmode -force-d3d11 -phase4PerfProbe -screen-width 1920 -screen-height 1080 -logFile "$PWD\Logs\Phase4-Performance.log"
```

`Phase4CapturePipeline` exits the Editor itself after all eight files pass existence/size validation. The Standalone flags are opt-in probes and do not affect ordinary play. The performance run must use a non-Null graphics device; unsupported counters are reported as unavailable, not estimated.

## Historical first-pass Phase 4 automated results

Executed with Unity `6000.4.6f1`, URP `17.4.0`, on 2026-07-21 after the final code change:

- Scene Builder: process exit `0`; P0 `1411EB0E...24C5`, P1 `4AADD3D3...EBB`, and P2 `73EC41AA...22C` SHA-256 values unchanged.
- EditMode: `84` passed, `0` failed, `0` skipped; duration `8.059572 s`; Phase 4 cases `23`.
- PlayMode: `87` passed, `0` failed, `0` skipped; duration `19.2221506 s`; Phase 4 cases `26`.
- P0/P1/P2/P3 integration maximum Drift: `0 m`; P3 interaction success `4`, failure `0`; all P3 sections completed.
- Build: process exit `0`; BuildReport Success; warnings `0`; errors `0`; reported size `178,650,134 B`; output `291` files / `178,845,266 B`.
- Standalone batch startup: D3D11 on NVIDIA GeForce RTX 5070 Laptop GPU; `ja-JP`; packaged Noto Sans JP; glyph validation true; telemetry JSON saved; normal quit requested; process exit `0`; matched Missing/Null/unhandled messages `0`.
- Performance: maximum three Echoes, 1920x1080, 600 frames at a 120fps cap; average `8.339 ms`, p95 `8.359 ms`, maximum `8.581 ms`; Main Thread average `8.335 ms`, maximum `8.596 ms`; Loop Transition maximum frame `14.093 ms`, Main Thread `14.074 ms`; steady GC `0 B/frame`; maximum used memory `105,811,560 B`.
- Runtime Draw Calls counter was unavailable. SetPass counter was valid but returned `0`; neither value is inferred. Interactive Frame Debugger confirmation remains manual.
- Eight captures passed automated count/dimension/size validation at 1920x1080 and remain ignored.
- A normal visible automated Player exit returned `0xC0000005` after clean Unity cleanup on this machine; the unchanged Phase 3 control Build reproduced it. This remains historical evidence from `phase4-automation-passed`; current Phase 4.1 results follow.

## Phase 4.1 external-asset additions

The full retained suites remain enabled and unrelaxed. Phase 4.1 adds nine EditMode and six PlayMode cases for official asset/license/hash records, curated-file count, importer settings, 2K texture limits, supported URP shaders, non-missing materials/textures, no imported collider/Animator/root motion, deterministic wrapper generation, preserved Stable IDs/colliders, external actor/environment/device visuals, no persistent world labels or camera-crossing overhead beams, Pause focus/HUD hiding, and Robot child-transform isolation.

### Phase 4.1 batch commands

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine -logFile "$PWD\Logs\Phase4_1_SceneBuilder.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\Phase4_1_EditMode_Final.xml" -logFile "$PWD\Logs\Phase4_1_EditMode_Final.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\Phase4_1_PlayMode_Final.xml" -logFile "$PWD\Logs\Phase4_1_PlayMode_Final.log"
& $unity -batchmode -force-d3d11 -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4CapturePipeline.CaptureAllFromCommandLine -logFile "$PWD\Logs\Phase4_1_Captures.log"
& $unity -batchmode -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4BuildPipeline.BuildWindowsDevelopment -logFile "$PWD\Logs\Phase4_1_Build_Development_Final.log"
& $unity -batchmode -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4BuildPipeline.BuildWindowsNonDevelopment -logFile "$PWD\Logs\Phase4_1_Build_NonDevelopment_Final.log"
& "$PWD\Builds\Phase4_1\ECHOSHIFT_Phase4_1.exe" -batchmode -force-d3d11 -phase3AutoQuit -screen-width 1920 -screen-height 1080 -logFile "$PWD\Logs\Phase4_1_Startup.log"
& "$PWD\Builds\Phase4_1\ECHOSHIFT_Phase4_1.exe" -batchmode -force-d3d11 -phase4PerfProbe -screen-width 1920 -screen-height 1080 -logFile "$PWD\Logs\Phase4_1_Performance.log"
```

### Latest Phase 4.1 results

Executed with Unity `6000.4.6f1`, URP `17.4.0`, on 2026-07-22 after the last code change:

- Scene Builder: repeatable; external wrapper hash stable; P0-P2 hashes unchanged; no duplicate Stable ID or serialized Missing Script/Reference.
- EditMode: `93/93` passed in `28.2191387 s`; PlayMode: `93/93` passed in `24.4194633 s`.
- P0-P3 maximum Drift: `0 m`; P1/P2/P3 interaction success/failure `2/0`, `2/0`, `4/0`; all P3 sections completed.
- Development Build: Success, warning `0`, error `0`, 291 files / 204,467,042 B at `Builds/Phase4_1`.
- Non-Development Build: Success, warning `0`, error `0`, 182 files / 140,785,117 B at `Builds/Phase4_1_NonDevelopment`.
- Startup: P3 Playing, `ja-JP`, packaged Noto/glyphs/HUD true, telemetry JSON saved, natural exit `0`, Missing/Null/unhandled matches `0`.
- Performance: 1920x1080, D3D11, maximum 3 Echoes, 600 frames; average `8.337547 ms`, p95 `8.336699 ms`, main average `8.329742 ms`, transition max `15.721394 ms`, steady GC `0 B/frame`, max used memory `168,041,477 B`, texture memory `40,882,741 B`.
- Draw Calls was unavailable; GPU Frame Time, SetPass, Triangles, and Vertices returned non-authoritative `0` values. They require interactive Profiler/Frame Debugger confirmation.
- Matching Before/After capture sets each contain eight 1920x1080 PNGs with Debug Overlay off.
- Nine Development/Non-Development D3D11/D3D12/audio/Quit/visible-window-close scenarios exited `0`; no new Application Error event was recorded. The historical `UnityPlayer.dll` `0xC0000005` root cause remains unproven.
- Raw logs, XML, captures, Builds, Library, and downloads remain ignored; reproducible summary evidence is committed in `Phase4ExternalAssetIntegrationValidation.md`.

## Phase 4.1 floating-perimeter regression

After the `phase4-assets-automation-passed` capture review, EditMode adds `BuilderRegenerationDoesNotRestoreFloatingPerimeterDecorations`. It rebuilds P3 and rejects the removed wall-lamp names, partial wall wrappers `03`-`06`, diagonal column wrapper `03`, missing continuous backings, wrong perimeter instance counts, and retained wall/column bounds that do not reach the floor envelope.

Latest post-correction results on 2026-07-22:

- Targeted Builder regeneration: `1/1` passed.
- EditMode: `94/94` passed in `27.4009265 s`; PlayMode: `93/93` passed in `21.5473395 s`.
- P0/P1/P2/P3 maximum Drift: `0 m`; P3 success/failure `4/0`; all three sections completed in 1,091 advances.
- Stable-ID preservation, actor collision, no presentation collider, Missing Component, and P0-P2 hash gates remain passed.
- Matching defect Before/fixed After sets: 8 PNG each, 1920x1080, under ignored `Captures/Phase4_1/FloatingVisualFix`.

## Phase 4.2 presentation-readability gate

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -force-d3d11 -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4CapturePipeline.CaptureAllFromCommandLine -logFile "$PWD\Logs\Phase4_2_CaptureFull.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\Phase4_2_EditMode_Final.xml" -logFile "$PWD\Logs\Phase4_2_EditMode_Final.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\Phase4_2_PlayMode_Final.xml" -logFile "$PWD\Logs\Phase4_2_PlayMode_Final.log"
& $unity -batchmode -force-d3d11 -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4BuildPipeline.BuildPhase4TwoWindowsDevelopment -logFile "$PWD\Logs\Phase4_2_Build_Final.log"
& "$PWD\Builds\Phase4_2\ECHOSHIFT_Phase4_2.exe" -batchmode -force-d3d11 -phase4AutoCompleteProbe -screen-width 1920 -screen-height 1080 -logFile "$PWD\Logs\Phase4_2_StandaloneCompletion.log"
& "$PWD\Builds\Phase4_2\ECHOSHIFT_Phase4_2.exe" -batchmode -force-d3d11 -phase4PerfProbe -screen-width 1920 -screen-height 1080 -logFile "$PWD\Logs\Phase4_2_Performance_Final.log"
& "$PWD\Builds\Phase4_2\ECHOSHIFT_Phase4_2.exe" -force-d3d11 -phase3AutoQuit -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -logFile "$PWD\Logs\Phase4_2_NormalWindowStartup.log"
& "$PWD\Builds\Phase4_2\ECHOSHIFT_Phase4_2.exe" -force-d3d11 -phase4PauseQuitProbe -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -logFile "$PWD\Logs\Phase4_2_NormalWindowPauseQuit.log"
```

Latest results on 2026-07-22:

- Full Scene Builder/capture gate: exit `0`; eight 1920x1080 images; Debug
  Overlay/cursor off; required Actor bounds inside the frame.
- EditMode `99/99` in `25.5873039 s`; PlayMode `105/105` in
  `23.096738 s`; warning/error `0/0`.
- P3 integration and Development Player both completed all three sections;
  Drift `0 m`, interaction `4/0`, Telemetry JSON present.
- BuildReport Success, warning/error `0/0`; 291 files / 204,479,394 B at
  `Builds/Phase4_2`.
- 600-frame maximum-three-Echo probe: frame average/p95/max
  `8.341/8.370/8.821 ms`, Main Thread average/max `8.337/8.832 ms`,
  Camera average/max `0.0077/0.0629 ms`, UI average/max
  `0.0036/0.0532 ms`, steady GC `0 B/frame`.
- Packaged Japanese font/glyph marker true; normal-window startup/auto-Quit and
  Pause-menu Quit exited `0`; Missing/Null/unhandled matches `0`.
- Raw XML, logs, captures, Builds, and Library remain ignored. Human visual
  judgment is still required before `phase4-validated`.

## Phase 4.3 character and device presentation gate

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\Phase43EditModeAccepted.xml" -logFile "$PWD\Logs\Phase43EditModeAccepted.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\Phase43PlayModeAccepted.xml" -logFile "$PWD\Logs\Phase43PlayModeAccepted.log"
& $unity -batchmode -force-d3d11 -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4CapturePipeline.CapturePhase43ExistingFromCommandLine -logFile "$PWD\Logs\Phase43CaptureAccepted.log"
& $unity -batchmode -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4BuildPipeline.BuildPhase4ThreeWindowsDevelopment -logFile "$PWD\Logs\Phase43BuildReleaseCandidate.log"
& "$PWD\Builds\Phase4_3\ECHOSHIFT_Phase4_3.exe" -force-d3d11 -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -phase4AutoCompleteProbe -logFile "$PWD\Logs\Phase43StandaloneReleaseCandidate.log"
& "$PWD\Builds\Phase4_3\ECHOSHIFT_Phase4_3.exe" -force-d3d11 -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -phase4PauseQuitProbe -logFile "$PWD\Logs\Phase43PauseQuitReleaseCandidate.log"
& "$PWD\Builds\Phase4_3\ECHOSHIFT_Phase4_3.exe" -batchmode -force-d3d11 -screen-width 1920 -screen-height 1080 -phase4PerfProbe -logFile "$PWD\Logs\Phase43PerformanceAcceptedRepeat.log"
& "$PWD\Builds\Phase4_3\ECHOSHIFT_Phase4_3.exe" -force-d3d11 -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -phase43PresentationProbe -logFile "$PWD\Logs\Phase43PresentationProbeFinal.log"
```

Accepted results on 2026-07-23:

- EditMode `116/116` in `49.3721874 s`; PlayMode `127/127` in
  `37.6364498 s`; failed/skipped `0/0`; Unity exit `0`.
- P0-P2 Scene object hashes match HEAD; P0-P3 integrations and the full P3
  presentation route pass.
- Final Standalone completed Sections 1-3 with Drift `0 m`, Interaction
  success/failure `4/0`, Telemetry JSON, Pause, Quit, and exit `0`.
- Maximum-three-Echo 600-frame repeat: frame average/p95/max
  `8.342/8.388/8.883 ms`, Main Thread average/max `8.331/8.894 ms`,
  Camera average/max `0.0126/0.0432 ms`, UI average/max
  `0.0070/0.0310 ms`, Robot pose average/max `0.0054/0.0389 ms`, Door
  visual average/max `0.0181/0.0423 ms`, steady GC `0 B/frame`.
- Transition maximum was `16.887 ms` in the first accepted run and
  `18.038 ms` in the repeat. It is recorded as a bounded spike, not hidden or
  represented as a strict 16.6ms maximum.
- D3D11 Capture: eight unique 1920x1080 PNGs. Normal-rendered Presentation
  Probe: `30.025 s`, nine required state screenshots, all Sections complete,
  exit `0`.
- Windows x86_64 Development Build: BuildReport Success,
  `PHASE4_3_BUILD_OK`, warning/error `0/0`, 291 files, no matching
  Compiler/Missing/Null/unhandled final-log entry.
- Captures, Builds, Logs, TestResults, and Unity Library remain ignored with
  zero tracked files. Human review is still required for foot sliding, pose
  naturalness, Battery hand placement, Door motion, circuit readability, and
  representative-image quality.
