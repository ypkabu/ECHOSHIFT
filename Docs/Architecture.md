# Architecture

## Assembly dependency direction

```text
EchoShift.Runtime
    ^
    +-- EchoShift.Editor
    +-- EchoShift.Tests.EditMode
    +-- EchoShift.Tests.PlayMode
```

Runtime code has no dependency on Editor or test assemblies. Only the input adapter and its asset reference `Unity.InputSystem`; simulation, movement, replay, and interaction code continue to depend on `IInputSource`.

## Fixed-tick simulation and input

`LoopDirector.Update` feeds unscaled delta time to `SimulationClock`. For each 60 Hz tick, `PlayerSimulation` samples its replaceable `IInputSource`, advances `CharacterMotor`, executes an interaction only when requested, and returns the recorded post-tick pose. `LoopDirector` then records the frame, records only a successful interaction event, and advances each `EchoPlayback` through the same motor.

The production adapter is `InputSystemInputSource`, backed by the `Gameplay` action map in `EchoShiftControls.inputactions`. Tests inject scripted `IInputSource` components without changing gameplay code. The clock caps catch-up work per rendered frame to avoid an unbounded spiral after a pause.

## Replay ownership

`ReplayRecorder` owns two preallocated mutable columns:

- a dense movement-frame buffer containing command and expected pose;
- a sparse successful-interaction buffer containing tick, operation kind, target Stable ID, and validation position.

Finalization copies only populated prefixes into `ReplayRecording` and `InteractionRecording`. Consumers receive indexed read access and cannot obtain or alter the source buffers. An early-ended replay cannot accept or execute an interaction outside its recorded frame range.

## Recorded interaction flow

`InteractionSensor` performs a preallocated `Physics.OverlapSphereNonAlloc` query. Candidate selection is independent of physics result order and compares availability, squared distance, forward alignment, then ordinal Stable ID. `Interactor` executes the live candidate and returns an immutable command only on success.

Replay targets are limited to objects that can be referenced by recorded commands. Their serialized `StableId` registers with a Scene-owned `InteractionRegistry`; disable/destruction unregisters the target. Echo playback resolves the recorded ID once at the recorded tick and executes the recorded operation. A missing, inactive, occupied, out-of-range, or otherwise invalid target increments one failure count with a reason. Playback never scans the Scene, selects a nearby substitute, moves toward the target, or retries every tick.

## Battery, Socket, and Door

`CarryableBattery` owns single-holder and inserted-Socket state. While held it remains outside the Actor hierarchy, copies the shared Carry Socket pose during `LateUpdate`, and disables collision. This prevents actor hierarchy destruction from deleting the Battery. Insertion attaches it to the PowerSocket insertion Transform. Free Rigidbody state is not replayed.

`PowerSocket` owns insertion and powered state and implements the small `IDoorOpenSource` contract. `DoorController` accepts one or more serialized sources; therefore Phase 0 PressurePlate and Phase 1 PowerSocket use the same Door without a global manager.

## Loop transition

The transition is synchronous and guarded against re-entry:

```text
pause simulation
finalize movement and interaction recording
explicitly release objects held by Player and Echoes
restore Battery, PowerSocket, Door, plate/goal state in registry order
rewind existing Echoes
evict oldest Echo if required and create the new Echo
restore Player pose
start fresh recorders at tick zero
resume simulation
```

No normal loop transition reloads the Scene or depends on trigger-exit/disable callback ordering for world consistency.

## Collision and Unity boundary

`CharacterMotor` performs deterministic X-then-Z capsule casts against the Environment layer and updates the Transform directly. Player and Echo use separate non-trigger actor layers and kinematic rigidbodies. The generated Layer Collision Matrix disables Player/Player, Player/Echo, and Echo/Echo contacts while retaining actor/Environment, actor/InteractionTrigger, and actor/InteractionTarget relationships.

Plain classes hold clock, immutable command, recorder, candidate comparison, and drift logic. MonoBehaviours adapt Input System actions, transforms, colliders/triggers, lifecycle, Gizmos, and OnGUI. Editor-only Stable ID generation/repair, layer setup, Scene creation, and build orchestration live in `EchoShift.Editor`.
