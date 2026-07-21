# ECHO//SHIFT

ECHO//SHIFT is a top-down 3D time-loop puzzle prototype made with Unity 6 and URP. Phase 4 gives the validated three-section Phase 3 game a coherent near-future research-facility presentation: modular rooms, compound robot/Echo silhouettes, dedicated devices, restrained URP lighting/post-processing, bounded VFX/audio, a packaged Japanese font, and a redesigned HUD. Gameplay, replay, Stable IDs, fixed-tick simulation, section solutions, and P0-P2 scenes remain unchanged. **Phase 4: Automation Passed; Human Visual Review Pending.**

## Phase 3 game flow

1. Section 1 teaches recording an Echo on a PressurePlate so the current Player can cross its Door.
2. Section 2 teaches recording Battery pickup and PowerSocket insertion so an Echo powers the Door.
3. Section 3 combines the Phase 2 route: Echo 1 holds a Plate, Echo 2 inserts the Battery, and the current Player crosses both Gates.

Section completion does not reload the Scene. The outgoing section releases carried objects, clears Echoes, and shuts down its loop; the incoming section restores authored state and starts at its own spawn. P0, P1, and P2 remain separate regression scenes.

## Controls

- `W`, `A`, `S`, `D` or Gamepad Left Stick: move on the XZ plane
- `E` or Gamepad South Button: interact
- `R` or Gamepad Start Button: end the current loop early
- `Escape` or Gamepad Select Button: pause/resume
- Pause menu: resume, restart section, restart game, or quit

## Requirements

- Unity Editor `6000.4.6f1`
- Universal Render Pipeline `17.4.0`
- Input System `1.19.0`
- Unity Test Framework `1.6.0`
- Windows x64 editor/build support

## Open and generate

Open the `Unity/` folder from Unity Hub or launch the installed Editor with `-projectPath <repository>/Unity`.

- `ECHO SHIFT/Build Phase 3 Scene` generates `Assets/_Project/Scenes/P3_PlayableGreybox.unity` and preserves P3/P2/P1/P0 Build Settings order.
- The Phase 3 Scene Builder automatically invokes the idempotent Phase 4 asset and presentation passes for P3 only.
- `ECHO SHIFT/Capture Phase 4 Frames` writes the eight 1920x1080 review images to ignored `Captures/Phase4/`.
- `ECHO SHIFT/Build Phase 4 Windows Development` writes `Builds/Phase4/ECHOSHIFT_Phase4.exe`.
- `ECHO SHIFT/Build Phase 2 Scene` regenerates `Assets/_Project/Scenes/P2_CoordinationLab.unity`.
- `ECHO SHIFT/Build Phase 1 Scene` generates `Assets/_Project/Scenes/P1_InteractionLab.unity`.
- `ECHO SHIFT/Build Phase 0 Scene` regenerates `Assets/_Project/Scenes/P0_ReplayLab.unity`.
- `ECHO SHIFT/Validate Stable Interaction IDs` rejects empty or duplicate replay-target IDs in the active Scene.
- `ECHO SHIFT/Build Phase 3 Windows Development` regenerates P3 and builds Windows x86_64 with P3 as the startup Scene.
- `EchoShift.Editor.Phase3BuildPipeline.BuildJapaneseWindowsDevelopment` regenerates P3 and writes the Japanese Windows x86_64 Development Build to `Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe` in batch mode.
- `EchoShift.Editor.Phase3BuildPipeline.BuildFinalWindowsDevelopment` regenerates P3 and writes the final Windows x86_64 Development Build to `Builds/Phase3-Final/ECHOSHIFT_Phase3_Final.exe`.

## Automated verification

Batch-mode commands are documented in `Docs/TestPlan.md`. The latest Phase 4 verification with Unity `6000.4.6f1` produced:

- Scene Builder: exit code `0`; P0-P2 SHA-256 values unchanged; P3/P2/P1/P0 Build Settings order retained.
- EditMode: `84/84` passed, including `23` Phase 4 cases for shared assets, font/license, PropertyBlocks, safe Volume values, builder uniqueness, and old-Scene hashes.
- PlayMode: `87/87` passed, including `26` Phase 4 cases plus all P0-P3 real-Scene and automatic-solution regressions.
- P3 automated solution: Replay Drift `0 m`; interactions `4` successful and `0` failed.
- Windows x86_64 Development Build: BuildReport success, warnings `0`, errors `0`, 291 files / 178,845,266 bytes at `Builds/Phase4`.
- Standalone batch probe: `ja-JP`, packaged `Noto Sans JP`, required glyphs, HUD, telemetry, real D3D11 GPU, and natural exit code `0`.
- Maximum-Echo performance probe: 1920x1080, 600 frames at a 120fps cap, average `8.339 ms`, p95 `8.359 ms`, Main Thread average `8.335 ms`, steady GC `0 B/frame`, transition maximum `14.093 ms`, and maximum used memory `105,811,560 B`.
- Eight ignored 1920x1080 captures were generated with Debug Overlay off.

Detailed Japanese evidence is in `Docs/Phase3JapaneseLocalizationValidation.md`. The original failure remains historical evidence in `Docs/Phase3CodexBlackBoxPlaytest.md`; a new source-blind Fresh7 run completed all three sections in 10:53.2 and passed Pause/Quit with no Critical or High issue, as recorded in `Docs/Phase3CodexBlackBoxRetest.md`. This remains a Codex pretest, not a substitute for human acceptance.

## Manual verification

Human acceptance passed full completion, movement, E/R/Escape, Camera, all three puzzle-comprehension stages, Echo/device/Door readability, HUD routing, Restart, Pause/Resume, Quit, Japanese display, and Interaction failure feedback. Critical and High findings were 0. Unrecorded environment and timing values remain explicitly unrecorded rather than inferred in `Docs/Phase3ManualAcceptance.md`.

Phase 4 automation is not formal acceptance. Complete `Docs/Phase4VisualAcceptance.md` against the current Build and captures before creating `phase4-validated`.

## Project layout

- `Docs/`: milestone, architecture, ADR, test, and validation documentation
- `Unity/Assets/_Project/Scripts/Runtime`: simulation, replay, interaction, section lifecycle, presentation, and telemetry code
- `Unity/Assets/_Project/Scripts/Editor`: deterministic Scene/visual asset builders, Stable ID validation, capture, and build pipelines
- `Unity/Assets/_Project/Scripts/Tests`: EditMode and PlayMode suites
- `Unity/Assets/_Project/Scenes`: generated Phase 0 through Phase 3 Scenes

## Current limitations

- Replay drift is measured but not corrected.
- Interaction commands and Stable IDs are Scene-local and in-memory; there is no save/schema migration.
- Battery carry/drop is deterministic and kinematic, without throw, stacking, or free Rigidbody replay.
- The prototype resets explicitly registered state, not arbitrary physics state.
- Telemetry is local JSON only and intentionally excludes replay frames and positions.
- There is no Steam integration, enemy AI, combat, save data, production BGM, final character art, or final environment texture pass.
- The generated build is a Development validation build, not a signed or installable Steam package.
- Draw Calls were unavailable from the runtime ProfilerRecorder on this configuration; SetPass was valid but reported `0`. These values are not inferred and require an interactive Frame Debugger/Profiler review.
- Normal windowed automated shutdown returned `0xC0000005` after clean Unity cleanup on this machine for both Phase 3 and Phase 4 builds; the supported batch probe exited `0`. Human Visual Review must exercise ordinary visible Quit again.
- Final visual quality remains gated by `Docs/Phase4VisualAcceptance.md`; advanced art, animation, texture, and BGM work remains in `Docs/Phase4Backlog.md`.
