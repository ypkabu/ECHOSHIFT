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

`LoopDirector.Update` feeds unscaled delta time to `SimulationClock`. Each tick is an explicit transaction: apply the prior Door commit; acquire oldest-to-newest Echo frames and current Player input; simulate all movement; synchronize physics and sensors; collect at most one interaction request per Actor; resolve requests; commit devices; record poses/drift; then evaluate Goal and loop end.

The production adapter is `InputSystemInputSource`, backed by the `Gameplay` action map in `EchoShiftControls.inputactions`. Tests inject scripted `IInputSource` components and can advance the same transaction one tick at a time. The clock caps catch-up work per rendered frame to avoid an unbounded spiral after a pause.

## Actor order and request arbitration

`LoopActor` stores explicit Replay generation. `ActorSimulationOrder` places older Echo generations first, newer Echoes next, and current Player last. `InteractionConflictResolver` sorts immutable requests by tick, Actor order, ordinal Stable ID, and interaction kind. The first request reserves its target for the tick; subsequent requests to that ID fail once with `TargetBusy`, without fallback or retry.

The fixed request capacity is four because the milestone supports three Echoes and one Player. See ADR 0008.

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

`PowerSocket` owns insertion and powered state and implements the small `IDoorOpenSource` contract. `DoorController` accepts one or more serialized sources. In a coordinated Scene, a source change at tick N is committed after interactions and applied to the Door Transform and collision at tick N+1. Standalone component use retains legacy rendered-frame movement.

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

## Scene settings and Loop history

P0/P1 share a 600-tick asset; P2 owns a 900-tick asset. Recorder arrays are allocated from the active Scene asset. Authored loops above 36,000 ticks are rejected to prevent accidental large dual-buffer allocations.

`LoopHistory` retains 16 immutable aggregate summaries and no Replay or Unity object references. Active Echo runtime results update at transition and evicted generations remain as value-only summaries until the bounded history ages them out.

## Collision and Unity boundary

`CharacterMotor` performs deterministic X-then-Z capsule casts against the Environment layer and updates the Transform directly. Player and Echo use separate non-trigger actor layers and kinematic rigidbodies. The generated Layer Collision Matrix disables Player/Player, Player/Echo, and Echo/Echo contacts while retaining actor/Environment, actor/InteractionTrigger, and actor/InteractionTarget relationships.

Plain classes hold clock, immutable command, recorder, candidate comparison, and drift logic. MonoBehaviours adapt Input System actions, transforms, colliders/triggers, lifecycle, Gizmos, and OnGUI. Editor-only Stable ID generation/repair, layer setup, Scene creation, and build orchestration live in `EchoShift.Editor`.

## Phase 3 gameplay shell

`SectionTransitionCoordinator` owns the Scene-level flow, while each `PuzzleSectionController` owns one section's Director, Player, Goal, spawn, and root. The explicit state graph is `Booting -> Playing -> LoopTransition/SectionTransition/Paused -> Playing`, with `SectionTransition -> Completed`. Duplicate or illegal transitions are rejected. Section changes never reload the Scene: carried references and Echoes are released, the outgoing Director shuts down, authored state resets, and only the next root activates.

`LoopDirector` remains the fixed-tick simulation owner. Phase 3 only added pause/restart/shutdown lifecycle methods and aggregate loop/interaction events. It does not own camera, HUD, tutorial, section order, telemetry persistence, or application quit policy.

## Presentation and input prompts

`SectionCameraController` provides a configurable damped top-down follow camera without adding Cinemachine. `GameplayHud`, `PauseMenuController`, and `TutorialGuide` consume centralized `Phase3TextCatalog` strings. `InputSystemInputSource` records the most recently used keyboard/mouse or gamepad class so prompts can switch without affecting deterministic commands. MaterialPropertyBlocks, line renderers, and Echo trails communicate target focus, wiring, Door state, replay generation, and stopped playback without instantiating materials per frame.

## Local telemetry boundary

`PlaytestTelemetry` records value-only session aggregates: build/Unity version, section duration and loops, manual/timer loop endings, restarts, interaction result counts, maximum drift, final section, and outcome. It emits schema-versioned JSON under `Application.persistentDataPath/EchoShiftPlaytests`. It intentionally stores no `ReplayRecording`, per-tick command, position, stable object reference, or cross-session mutable singleton.
