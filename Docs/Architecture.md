# Architecture

## Assembly dependency direction

```text
EchoShift.Runtime
    ^
    +-- EchoShift.Editor
    +-- EchoShift.Tests.EditMode
    +-- EchoShift.Tests.PlayMode
```

Runtime code has no dependency on Editor or test assemblies.

## Simulation flow

`LoopDirector.Update` feeds unscaled delta time to `SimulationClock`. For each produced tick, it asks `PlayerSimulation` to sample its `IInputSource`, moves the player through `CharacterMotor`, records the post-tick pose, then advances every `EchoPlayback` through the same motor. The production adapter is `KeyboardInputSource`; integration tests can supply a scripted implementation without changing movement or loop code.

The clock caps catch-up work per rendered frame. This prevents an unbounded spiral after an Editor pause while preserving fixed tick duration for ordinary play.

## Loop transition

The transition is synchronous and guarded against re-entry:

```text
pause clock
finalize player recording
restore registered world state
rewind existing Echoes
create the new Echo
restore player pose
start a new recorder
resume clock
```

No normal loop transition reloads the scene.

## Replay ownership

`ReplayRecorder` owns a preallocated mutable buffer while recording. `FinalizeRecording` copies only valid frames into `ReplayRecording`; consumers receive indexed read access and cannot obtain the mutable source buffer.

## Unity boundary

Plain classes hold clock and replay buffer logic. MonoBehaviours adapt keyboard input, transforms, triggers, scene lifecycle, Gizmos, and OnGUI. Required references are serialized and assembled by `P0SceneBuilder`.

`CharacterMotor` performs deterministic X-then-Z capsule casts against the Environment layer and updates the Transform directly. Player and Echo use separate non-trigger actor layers and kinematic rigidbodies. The generated Layer Collision Matrix disables Player/Echo and Echo/Echo contacts while retaining Player/Environment, Echo/Environment, and actor/InteractionTrigger contacts. Pressure plates and goals own trigger colliders on the InteractionTrigger layer.
