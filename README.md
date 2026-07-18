# ECHO//SHIFT

ECHO//SHIFT is a top-down 3D time-loop puzzle prototype made with Unity 6 and URP. The current Phase 0 milestone validates a single mechanic: record ten seconds of movement, reset the room, then replay that movement as a physical Echo.

## Phase 0 play loop

1. Move the player onto the blue pressure plate during loop 1.
2. When ten seconds elapse, the room resets and an Echo replays loop 1.
3. While the Echo holds the plate and the door is open, move the current player through the door into the goal.

## Controls

- `W`, `A`, `S`, `D`: move on the XZ plane
- `R`: end the current loop early

## Requirements

- Unity Editor `6000.4.6f1`
- Universal Render Pipeline `17.4.0`
- Unity Test Framework `1.6.0`
- Visual Studio Editor integration `2.0.22`
- Windows x64 editor/build support

## Open the project

Open the `Unity/` folder from Unity Hub or launch the installed Editor with `-projectPath <repository>/Unity`.

If the verification scene is missing, run `ECHO SHIFT/Build Phase 0 Scene` in the Unity Editor. The generated scene is `Assets/_Project/Scenes/P0_ReplayLab.unity`.

## Run tests

In the Editor, open **Window > General > Test Runner**, then run both EditMode and PlayMode suites.

Batch-mode examples are documented in `Docs/TestPlan.md`.

Latest automated verification:

- Scene Builder: exit code `0`, no matched compiler error or warning
- EditMode: `12/12` passed
- PlayMode: `12/12` passed, including collision, short replay, stale plate reference, and complete generated-scene flow coverage
- Windows x86_64 Development Build: generated at `Builds/Phase0/ECHOSHIFT_Phase0.exe` with exit code `0` and zero BuildReport warnings
- Headless Player smoke: startup completed with no matched unhandled exception or missing-reference error

## Manual verification

1. Open `Assets/_Project/Scenes/P0_ReplayLab.unity`.
2. Enter Play Mode and confirm the overlay starts at loop 1, tick 0/600.
3. Move onto the pressure plate before the first loop ends and remain there.
4. Confirm the player and door reset at the loop transition and one Echo appears.
5. Confirm the Echo follows the recorded route, presses the plate, and opens the door.
6. Move the player through the doorway into the green goal.
7. Confirm `GOAL REACHED` is shown and replay drift remains at or below `0.05 m` during ordinary unobstructed movement.
8. Repeat loops and confirm no more than three Echoes remain.

## Project layout

- `Docs/`: milestone, architecture, ADR, and test documentation
- `Unity/Assets/_Project/Scripts/Runtime`: Phase 0 runtime code
- `Unity/Assets/_Project/Scripts/Editor`: deterministic scene builder
- `Unity/Assets/_Project/Scripts/Tests`: EditMode and PlayMode tests
- `Unity/Assets/_Project/Scenes`: generated verification scene

## Known Phase 0 limitations

- Keyboard input only; Input System and gamepad support are deferred.
- Replay drift is measured but not corrected.
- The prototype resets only registered Phase 0 components, not arbitrary physics state.
- There is no save data, Steam integration, enemy AI, production UI, audio, or final art.
- The generated build is a validation build, not a production Steam package; signing, installer behavior, graphical presentation, and real keyboard feel remain manually unverified.
