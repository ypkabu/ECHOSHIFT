# ECHO//SHIFT

ECHO//SHIFT is a top-down 3D time-loop puzzle prototype made with Unity 6 and URP. Phase 3 packages the validated replay mechanics into one playable greybox: three puzzle sections, in-place transitions, camera and HUD guidance, pause/restart/quit flows, visual feedback, and local playtest telemetry. Player-facing Phase 3 text defaults to Japanese (`ja-JP`). **Phase 3: Validated.**

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
- `ECHO SHIFT/Build Phase 2 Scene` regenerates `Assets/_Project/Scenes/P2_CoordinationLab.unity`.
- `ECHO SHIFT/Build Phase 1 Scene` generates `Assets/_Project/Scenes/P1_InteractionLab.unity`.
- `ECHO SHIFT/Build Phase 0 Scene` regenerates `Assets/_Project/Scenes/P0_ReplayLab.unity`.
- `ECHO SHIFT/Validate Stable Interaction IDs` rejects empty or duplicate replay-target IDs in the active Scene.
- `ECHO SHIFT/Build Phase 3 Windows Development` regenerates P3 and builds Windows x86_64 with P3 as the startup Scene.
- `EchoShift.Editor.Phase3BuildPipeline.BuildJapaneseWindowsDevelopment` regenerates P3 and writes the Japanese Windows x86_64 Development Build to `Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe` in batch mode.
- `EchoShift.Editor.Phase3BuildPipeline.BuildFinalWindowsDevelopment` regenerates P3 and writes the final Windows x86_64 Development Build to `Builds/Phase3-Final/ECHOSHIFT_Phase3_Final.exe`.

## Automated verification

Batch-mode commands are documented in `Docs/TestPlan.md`. The latest verification with Unity `6000.4.6f1` produced:

- Phase 3 Scene Builder: successful and repeatable; old Scene files remain unchanged; P3/P2/P1/P0 Build Settings order
- EditMode: `61/61` passed, including Japanese catalog, glyph/layout safe areas, repeatable Scene generation, and telemetry de-duplication checks
- PlayMode: `61/61` passed, including P0-P3 real-Scene integration, actionable Socket prompts, recorded-only P3 Door power, start-input consumption, the interactive start gate, completed-state Pause/Quit, and Japanese/black-box regressions
- P3 automated solution: all three sections completed in 1,081 advances; Replay Drift `0 m`; interactions `4` successful, `0` failed
- Final Windows x86_64 Development Build: exit code `0`, zero BuildReport warnings, 289 files / 166,334,282 bytes at `Builds/Phase3-Final`
- Headless Standalone probe: `ja-JP`, `Noto Sans JP`, required glyphs, HUD, and telemetry initialized; JSON was saved and the player exited naturally with code `0`

Detailed Japanese evidence is in `Docs/Phase3JapaneseLocalizationValidation.md`. The original failure remains historical evidence in `Docs/Phase3CodexBlackBoxPlaytest.md`; a new source-blind Fresh7 run completed all three sections in 10:53.2 and passed Pause/Quit with no Critical or High issue, as recorded in `Docs/Phase3CodexBlackBoxRetest.md`. This remains a Codex pretest, not a substitute for human acceptance.

## Manual verification

Human acceptance passed full completion, movement, E/R/Escape, Camera, all three puzzle-comprehension stages, Echo/device/Door readability, HUD routing, Restart, Pause/Resume, Quit, Japanese display, and Interaction failure feedback. Critical and High findings were 0. Unrecorded environment and timing values remain explicitly unrecorded rather than inferred in `Docs/Phase3ManualAcceptance.md`.

## Project layout

- `Docs/`: milestone, architecture, ADR, test, and validation documentation
- `Unity/Assets/_Project/Scripts/Runtime`: simulation, replay, interaction, section lifecycle, presentation, and telemetry code
- `Unity/Assets/_Project/Scripts/Editor`: deterministic Scene builders, Stable ID validation, and build pipelines
- `Unity/Assets/_Project/Scripts/Tests`: EditMode and PlayMode suites
- `Unity/Assets/_Project/Scenes`: generated Phase 0 through Phase 3 Scenes

## Current limitations

- Replay drift is measured but not corrected.
- Interaction commands and Stable IDs are Scene-local and in-memory; there is no save/schema migration.
- Battery carry/drop is deterministic and kinematic, without throw, stacking, or free Rigidbody replay.
- The prototype resets explicitly registered state, not arbitrary physics state.
- Telemetry is local JSON only and intentionally excludes replay frames and positions.
- There is no Steam integration, enemy AI, combat, save data, audio, VFX, or final art.
- The generated build is a Development validation build, not a signed or installable Steam package.
- Codex pretest backlog: delayed replay progress can resemble an interaction failure (Medium), close world labels can overlap (Low), and a faint duplicate Pause title can appear (Low).
- Human acceptance recorded one non-blocking Medium: the playable greybox looks too simple for a finished product. Visual Polish / Art Direction is deferred to `Docs/Phase4Backlog.md`.
