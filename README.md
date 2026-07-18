# ECHO//SHIFT

ECHO//SHIFT is a top-down 3D time-loop puzzle prototype made with Unity 6 and URP. Phase 2 adds deterministic multi-Echo coordination: the oldest Echo holds a PressurePlate for Gate A, the next Echo replays Stable-ID-addressed Battery insertion for Gate B, and the current Player crosses both Gates to the Goal.

## Phase 2 play loop

1. Loop 1: move onto the blue PressurePlate and end the loop while waiting there.
2. Loop 2: while Echo 1 holds Gate A open, cross it, pick up the orange Battery, insert it into the purple PowerSocket, then end the loop.
3. Loop 3: Echo 1 opens Gate A and Echo 2 powers Gate B; move the current Player through both Gates to the green Goal.

Phase 0 and Phase 1 remain available as independent 10-second replay laboratories. Phase 2 uses its own 15-second, 900-tick settings asset.

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

- `ECHO SHIFT/Build Phase 2 Scene` generates `Assets/_Project/Scenes/P2_CoordinationLab.unity` and preserves P2/P1/P0 Build Settings order.
- `ECHO SHIFT/Build Phase 1 Scene` generates `Assets/_Project/Scenes/P1_InteractionLab.unity`.
- `ECHO SHIFT/Build Phase 0 Scene` regenerates `Assets/_Project/Scenes/P0_ReplayLab.unity`.
- `ECHO SHIFT/Validate Stable Interaction IDs` rejects empty or duplicate replay-target IDs in the active Scene.
- `ECHO SHIFT/Build Phase 2 Windows Development` regenerates all required Scenes and builds Phase 2 as the startup Scene.

## Automated verification

Batch-mode commands are documented in `Docs/TestPlan.md`. The latest verification with Unity `6000.4.6f1` produced:

- Phase 2 Scene Builder: successful; stable P2 Battery and PowerSocket IDs, one P2 settings asset, and P2/P1/P0 Build Settings order after repeated generation
- EditMode: `35/35` passed, including all existing Phase 0 and Phase 1 cases
- PlayMode: `32/32` passed, including P0, P1, and the P2 three-loop generated-Scene integrations
- Phase 0 maximum replay drift: `0 m`
- Phase 1 maximum replay drift: `0 m`; Echo interactions: `2` successful, `0` failed
- Phase 2 maximum replay drift: `0 m`; Echo interactions: `2` successful, `0` failed; Goal at Loop 3 tick 291
- Windows x86_64 Development Build: exit code `0`, zero BuildReport warnings, expected EXE/Data output generated
- Headless Standalone smoke: 30 seconds through engine, Input System, PhysX, and P2 Scene initialization; structured startup values matched 60 Hz, 15 seconds, 900 ticks, three Echoes, and 0.05 m tolerance, with no matched exception, error, or missing-reference message

Detailed Phase 2 evidence and remaining manual checks are in `Docs/Phase2Validation.md`.

## Manual verification

1. Open `Assets/_Project/Scenes/P2_CoordinationLab.unity` and enter Play Mode.
2. Perform the three-loop route above with keyboard, then with a physical gamepad.
3. Confirm Player, three generation colors, Plate, Battery, Socket, both Gate states, and Goal are visually clear and the overlay does not hide the route.
4. Repeat P0 and P1 manually to subjectively confirm unchanged feel.

## Project layout

- `Docs/`: milestone, architecture, ADR, test, and validation documentation
- `Unity/Assets/_Project/Scripts/Runtime`: fixed-tick simulation, replay, reset, and recorded interaction code
- `Unity/Assets/_Project/Scripts/Editor`: deterministic Scene builders, Stable ID validation, and build pipelines
- `Unity/Assets/_Project/Scripts/Tests`: EditMode and PlayMode suites
- `Unity/Assets/_Project/Scenes`: generated Phase 0, Phase 1, and Phase 2 verification Scenes

## Current limitations

- Replay drift is measured but not corrected.
- Interaction commands and Stable IDs are Scene-local and in-memory; there is no save/schema migration.
- Battery carry/drop is deterministic and kinematic, without throw, stacking, or free Rigidbody replay.
- The prototype resets explicitly registered state, not arbitrary physics state.
- There is no Steam integration, enemy AI, combat, save data, production UI, audio, VFX, or final art.
- The generated build is a validation build, not a production Steam package; graphical presentation, physical keyboard/gamepad feel, signing, and installer behavior remain manually unverified.
