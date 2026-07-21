# ECHO//SHIFT

ECHO//SHIFT is a top-down 3D time-loop puzzle prototype made with Unity 6 and URP. Phase 4.1 replaces the rejected primitive-led visual pass with a curated CC0 Quaternius modular sci-fi/robot presentation and Kenney audio while retaining the validated three-section Phase 3 game. Gameplay, replay, Stable IDs, fixed-tick simulation, colliders, section solutions, and P0-P2 scenes remain unchanged. **Phase 4.1: Automation Passed; Human Visual Review Pending.**

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
- `ECHO SHIFT/Capture Phase 4 Frames` writes the eight 1920x1080 Phase 4.1 review images to ignored `Captures/Phase4_1/After/`.
- `ECHO SHIFT/Build Phase 4 Windows Development` writes `Builds/Phase4_1/ECHOSHIFT_Phase4_1.exe`.
- `EchoShift.Editor.Phase4BuildPipeline.BuildWindowsNonDevelopment` writes `Builds/Phase4_1_NonDevelopment/ECHOSHIFT_Phase4_1_NonDevelopment.exe`.
- `ECHO SHIFT/Build Phase 2 Scene` regenerates `Assets/_Project/Scenes/P2_CoordinationLab.unity`.
- `ECHO SHIFT/Build Phase 1 Scene` generates `Assets/_Project/Scenes/P1_InteractionLab.unity`.
- `ECHO SHIFT/Build Phase 0 Scene` regenerates `Assets/_Project/Scenes/P0_ReplayLab.unity`.
- `ECHO SHIFT/Validate Stable Interaction IDs` rejects empty or duplicate replay-target IDs in the active Scene.
- `ECHO SHIFT/Build Phase 3 Windows Development` regenerates P3 and builds Windows x86_64 with P3 as the startup Scene.
- `EchoShift.Editor.Phase3BuildPipeline.BuildJapaneseWindowsDevelopment` regenerates P3 and writes the Japanese Windows x86_64 Development Build to `Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe` in batch mode.
- `EchoShift.Editor.Phase3BuildPipeline.BuildFinalWindowsDevelopment` regenerates P3 and writes the final Windows x86_64 Development Build to `Builds/Phase3-Final/ECHOSHIFT_Phase3_Final.exe`.

## Automated verification

Batch-mode commands are documented in `Docs/TestPlan.md`. The latest Phase 4.1 verification with Unity `6000.4.6f1` produced:

- Scene Builder: exit code `0`; P0-P2 SHA-256 values unchanged; P3/P2/P1/P0 Build Settings order retained.
- Curated third-party originals: 39 files / 9,984,796 bytes; official CC0 sources, hashes, licenses, unused files, and import settings recorded in `Docs/ThirdPartyAssets.md`.
- EditMode: `94/94` passed; PlayMode: `93/93` passed. Added coverage includes source/license/hash, imports, materials/textures/shaders, root motion absence, wrapper idempotence, gameplay-root integrity, labels/beams, floating-perimeter regeneration, Pause focus/HUD, and external visuals.
- P3 automated solution: Replay Drift `0 m`; interactions `4` successful and `0` failed.
- Windows x86_64 Development Build: BuildReport success, warnings `0`, errors `0`, 291 files / 204,467,042 bytes at `Builds/Phase4_1`.
- Windows x86_64 Non-Development Build: BuildReport success, warnings `0`, errors `0`, 182 files / 140,785,117 bytes at `Builds/Phase4_1_NonDevelopment`.
- Standalone batch probe: `ja-JP`, packaged `Noto Sans JP`, required glyphs, HUD, telemetry, real D3D11 GPU, and natural exit code `0`.
- Maximum-Echo performance probe: 1920x1080, 600 frames at a 120fps cap, average `8.338 ms`, p95 `8.337 ms`, Main Thread average `8.330 ms`, steady GC `0 B/frame`, transition maximum `15.721 ms`, and maximum used memory `168,041,477 B`.
- Matching Before/After sets of eight ignored 1920x1080 captures were generated with Debug Overlay off.
- Development/Non-Development, D3D11/D3D12, audio on/off, auto-quit, Pause-menu Quit, and ordinary visible Window Close probes all exited `0`; the historical `0xC0000005` was not reproduced, but its root cause remains unproven.

Detailed Japanese evidence is in `Docs/Phase3JapaneseLocalizationValidation.md`. The original failure remains historical evidence in `Docs/Phase3CodexBlackBoxPlaytest.md`; a new source-blind Fresh7 run completed all three sections in 10:53.2 and passed Pause/Quit with no Critical or High issue, as recorded in `Docs/Phase3CodexBlackBoxRetest.md`. This remains a Codex pretest, not a substitute for human acceptance.

## Manual verification

Human acceptance passed full completion, movement, E/R/Escape, Camera, all three puzzle-comprehension stages, Echo/device/Door readability, HUD routing, Restart, Pause/Resume, Quit, Japanese display, and Interaction failure feedback. Critical and High findings were 0. Unrecorded environment and timing values remain explicitly unrecorded rather than inferred in `Docs/Phase3ManualAcceptance.md`.

Phase 4.1 automation is not formal acceptance. Complete `Docs/Phase4VisualAcceptance.md` against `Builds/Phase4_1/ECHOSHIFT_Phase4_1.exe` and `Captures/Phase4_1/After/` before creating `phase4-validated`.

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
- There is no Steam integration, enemy AI, combat, save data, production BGM, or final production art pass.
- The generated build is a Development validation build, not a signed or installable Steam package.
- Draw Calls were unavailable from the runtime ProfilerRecorder; GPU Frame Time, SetPass, Triangles, and Vertices returned `0` despite availability. These values are not inferred and require an interactive Frame Debugger/Profiler review.
- Historical Phase 3/Phase 4 shutdown records contain `UnityPlayer.dll` `0xC0000005`. Phase 4.1's required nine-scenario matrix exited `0`, but the historical root cause is not established and visible Quit should still be checked by a human.
- The Quaternius Robot importer reports one source `Foot.L` self-intersection diagnostic; no visible or Build defect was found automatically, so both feet remain a manual review item.
- Final visual quality remains gated by `Docs/Phase4VisualAcceptance.md`; advanced art, animation, texture, and BGM work remains in `Docs/Phase4Backlog.md`.
