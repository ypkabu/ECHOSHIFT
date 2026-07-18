# ECHO//SHIFT

ECHO//SHIFT is a top-down 3D time-loop puzzle prototype made with Unity 6 and URP. Phase 1 extends the validated movement replay with successful, Stable-ID-addressed interactions: an Echo can pick up the exact recorded Battery, insert it into a PowerSocket, and hold a powered Door open for the current Player.

## Phase 1 play loop

1. Move to the orange Battery during loop 1 and interact to pick it up.
2. Carry it to the purple PowerSocket and interact to insert it.
3. End the loop. The world restores the Battery, Socket, Door, Goal, actors, and replay state.
4. The Echo replays the exact Battery and PowerSocket interactions.
5. While the Echo powers the Door, move the current Player through it into the Goal.

Phase 0 remains available as the pressure-plate movement-replay laboratory.

## Controls

- `W`, `A`, `S`, `D` or Gamepad Left Stick: move on the XZ plane
- `E` or Gamepad South Button: interact
- `R` or Gamepad Start Button: end the current loop early

## Requirements

- Unity Editor `6000.4.6f1`
- Universal Render Pipeline `17.4.0`
- Input System `1.19.0`
- Unity Test Framework `1.6.0`
- Windows x64 editor/build support

## Open and generate

Open the `Unity/` folder from Unity Hub or launch the installed Editor with `-projectPath <repository>/Unity`.

- `ECHO SHIFT/Build Phase 1 Scene` generates `Assets/_Project/Scenes/P1_InteractionLab.unity`.
- `ECHO SHIFT/Build Phase 0 Scene` regenerates `Assets/_Project/Scenes/P0_ReplayLab.unity`.
- `ECHO SHIFT/Validate Stable Interaction IDs` rejects empty or duplicate replay-target IDs in the active Scene.
- `ECHO SHIFT/Build Phase 1 Windows Development` regenerates both Scenes and builds Phase 1 as the startup Scene.

## Automated verification

Batch-mode commands are documented in `Docs/TestPlan.md`. The latest verification with Unity `6000.4.6f1` produced:

- Phase 1 Scene Builder: successful; one Battery ID and one PowerSocket ID after repeated generation
- EditMode: `23/23` passed, including all existing Phase 0 cases
- PlayMode: `26/26` passed, including all existing Phase 0 cases and both generated-Scene integrations
- Phase 0 maximum replay drift: `0 m`
- Phase 1 maximum replay drift: `0 m`; Echo interactions: `2` successful, `0` failed
- Windows x86_64 Development Build: exit code `0`, zero BuildReport warnings, expected EXE/Data output generated
- Headless Standalone smoke: 20 seconds through engine, Input System, PhysX, and Scene initialization with no matched exception, error, or missing-reference message

Detailed evidence and remaining manual checks are in `Docs/Phase1Validation.md`.

## Manual verification

1. Open `Assets/_Project/Scenes/P1_InteractionLab.unity` and enter Play Mode.
2. Verify the fixed top-down framing and that orange Battery, purple Socket, powered Door, and green Goal are visually distinguishable.
3. Use keyboard controls to record pickup and insertion, end the loop, and verify the Echo repeats both operations.
4. Repeat with a physical gamepad and confirm Left Stick, South Button, and Start behavior.
5. Move the current Player through the Echo-opened Door into the Goal and confirm overlay values and interaction Gizmos are readable.
6. Repeat the Phase 0 pressure-plate flow to subjectively confirm unchanged feel.

## Project layout

- `Docs/`: milestone, architecture, ADR, test, and validation documentation
- `Unity/Assets/_Project/Scripts/Runtime`: fixed-tick simulation, replay, reset, and recorded interaction code
- `Unity/Assets/_Project/Scripts/Editor`: deterministic Scene builders, Stable ID validation, and build pipelines
- `Unity/Assets/_Project/Scripts/Tests`: EditMode and PlayMode suites
- `Unity/Assets/_Project/Scenes`: generated Phase 0 and Phase 1 verification Scenes

## Current limitations

- Replay drift is measured but not corrected.
- Interaction commands and Stable IDs are Scene-local and in-memory; there is no save/schema migration.
- Battery carry/drop is deterministic and kinematic, without throw, stacking, or free Rigidbody replay.
- The prototype resets explicitly registered state, not arbitrary physics state.
- There is no Steam integration, enemy AI, combat, save data, production UI, audio, VFX, or final art.
- The generated build is a validation build, not a production Steam package; graphical presentation, physical keyboard/gamepad feel, signing, and installer behavior remain manually unverified.
