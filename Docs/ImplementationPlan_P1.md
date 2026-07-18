# Phase 1 Implementation Plan

## Status

Implementation planned on 2026-07-19 from the validated Phase 0 baseline `767116cfd9f2c6805dab271c368ee0a23ff25263` on branch `feature/phase1-recorded-interaction`.

## Objective

Validate that a successful Player interaction can be recorded with a stable target identity and replayed by an Echo at the same simulation tick. The Phase 1 laboratory uses a kinematic Battery and PowerSocket: loop 1 records pickup and insertion, loop 2 replays those exact targets so the Echo powers a Door while the current Player reaches the Goal.

## Package and input migration

- Add Unity Input System `1.19.0`. Unity Editor `6000.4.6f1` declares this as its bundled and minimum discoverable version, and the matching package archive is installed with the Editor.
- Replace the legacy keyboard adapter with an `InputSystemInputSource` while preserving `IInputSource` as the only gameplay input boundary.
- Define Move, Interact, and EndLoop actions with WASD/E/R and Gamepad Left Stick/South Button/Start bindings.
- Configure new Input System handling reproducibly from Editor setup. Runtime movement, replay, and interaction code will not reference Input System APIs.

## Replay format decision

Keep movement frames and successful interaction events in separate immutable columns owned by one finalized `ReplayRecording`.

- `ReplayFrame[]` remains the 60 Hz movement/pose stream.
- `InteractionCommand[]` stores only successful operations in tick order.
- Finalization copies both populated prefixes and exposes indexed read-only access.
- An Echo advances one interaction cursor alongside its existing movement cursor and never selects a substitute target during playback.

This preserves the Phase 0 frame contract, avoids an empty interaction payload on most frames, and makes interaction event counts and failures directly observable. ADR 0005 will record the decision and replacement conditions.

## Runtime responsibility split

- `StableId`: serialized identity only; never generates an ID at runtime.
- `InteractionRegistry`: scene-owned ID-to-target cache, with explicit registration and removal.
- `IInteractable` and `InteractionContext`: target contract and immutable execution context.
- `InteractionSensor`: preallocated NonAlloc physics query and deterministic candidate ranking.
- `Interactor`: actor-owned selection, carry socket, live execution, and exact recorded-command execution.
- `InteractionRecorder` / `InteractionRecording`: ordered mutable capture and immutable finalized events.
- `EchoPlayback`: movement replay plus one-shot exact-ID interaction playback statistics.
- `CarryableBattery`: single-owner pickup/drop/insertion state and kinematic reset.
- `PowerSocket`: exact insertion target, powered state, and reset.
- `IDoorOpenSource`: small multi-source Door contract used by both Phase 0 PressurePlate and Phase 1 PowerSocket.
- `LoopDirector`: coordinates explicit actor release before ordered world restoration; it does not select targets or implement carry rules.

## Stable identity workflow

- Limit Stable IDs to Battery and PowerSocket replay targets.
- Use serialized GUID-format values assigned by the Phase 1 Editor builder and retained across repeated generation.
- Add an Editor validator that reports empty and duplicate IDs before saving the scene.
- Register once into the scene-owned Registry during initialization and remove on disable/destruction.
- Resolve recorded targets through the Registry only; do not scan the Scene or fall back to another candidate.

ADR 0004 will document identity ownership, validation, limitations, and replacement conditions.

## World reset order

1. Pause the simulation clock.
2. Finalize movement and interaction recording.
3. Explicitly release objects held by Player and existing Echoes.
4. Restore Battery state.
5. Restore PowerSocket state.
6. Restore Door, PressurePlate, and Goal state in serialized registry order.
7. Rewind existing Echoes.
8. Evict the oldest Echo if required and create the new Echo.
9. Restore Player pose.
10. Allocate new recorders, reset tick zero, and resume simulation.

Battery motion remains kinematic and follows a shared Carry Socket without becoming an actor child, rather than using free Rigidbody replay. ADR 0006 records that choice.

## Planned files

### Add

- `Unity/Assets/_Project/Settings/EchoShiftControls.inputactions`
- Runtime interaction files under `Scripts/Runtime/Interaction/Recorded/`
- `Scripts/Runtime/Input/InputSystemInputSource.cs`
- `Scripts/Editor/StableIdSceneValidator.cs`
- `Scripts/Editor/P1SceneBuilder.cs`
- `Scripts/Editor/Phase1BuildPipeline.cs`
- Phase 1 EditMode and PlayMode test files
- `Assets/_Project/Scenes/P1_InteractionLab.unity`
- Phase 1 Battery/Echo prefabs and temporary materials
- `Docs/ADR/0004-stable-interaction-identity.md`
- `Docs/ADR/0005-interaction-replay-format.md`
- `Docs/ADR/0006-kinematic-carryable-object.md`
- `Docs/Phase1Validation.md`

### Modify

- `Unity/Packages/manifest.json` and resolved lock file
- Runtime and Editor assembly definitions
- `InputCommand`, replay recorder/recording, PlayerSimulation, EchoPlayback, LoopDirector
- DoorController, PressurePlate, and debug overlay
- `P0SceneBuilder` and both build pipelines so Phase 0 remains reproducible
- Project Settings for Input System handling, build scenes, and interaction layer
- Existing test assembly definitions and Phase 0 integration tests only where setup APIs change
- `README.md`, `Docs/Architecture.md`, and `Docs/TestPlan.md`

### Remove

- Legacy `KeyboardInputSource.cs` after both generated scenes use the new adapter.

## Test plan

Retain all existing 12 EditMode and 12 PlayMode cases without deletion, disabling, or tolerance changes.

Add EditMode coverage for:

- empty and duplicate Stable IDs;
- Registry resolve and stale removal;
- ordered interaction event finalization and immutability;
- short-record bounds;
- deterministic candidate comparison;
- Battery ownership rejection;
- Battery/Socket reset consistency.

Add PlayMode coverage for:

- pickup, carry-socket following, drop, insertion, and powered Door;
- ownership conflict and actor disable/destroy cleanup;
- ordered loop reset consistency;
- exact-ID Echo pickup and insertion;
- no playback fallback on failure;
- early-loop interaction termination;
- unchanged Phase 0 generated-scene flow;
- full generated `P1_InteractionLab` flow with missing-reference and unexpected-log checks.

Final gates:

- Scene builders exit zero and both scenes remain in Build Settings.
- EditMode and PlayMode XML report all tests passed.
- Phase 0 replay drift remains at or below `0.05 m`.
- Phase 1 integration reports drift and interaction success/failure counts.
- Windows x86_64 Development Build succeeds at `Builds/Phase1/ECHOSHIFT_Phase1.exe`.
- Headless Player startup log has no unhandled exception or missing reference.

## Expected risks

- Input System package activation can trigger a project input-backend restart or serialized PlayerSettings change.
- Component enable order can register a Stable ID before all scene systems finish `Awake`; configuration and registration must therefore be idempotent.
- Battery, Socket, and Interactor hold references to one another, so reset must explicitly sever both sides before restoring transforms.
- Trigger/collider enable changes can omit `OnTriggerExit`; ownership cleanup cannot rely on physics callbacks.
- A different world state can make an exact recorded interaction fail. Playback must record the reason once and never auto-correct.
- Automated headless validation cannot confirm graphical framing, physical keyboard/gamepad feel, or controller-device availability.
