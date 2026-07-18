# Phase 0 Implementation Plan

## Status

Final audit and standalone validation completed on 2026-07-19 with Unity `6000.4.6f1`. Phase 1 remains out of scope.

## Objective

Build one replay laboratory scene that records 600 fixed ticks of WASD movement over ten seconds. At the next loop, a physical Echo uses the same movement implementation to replay those commands, hold a pressure plate, and open a door for the current player.

## Work order

1. Bootstrap a Unity `6000.4.6f1` project under `Unity/` and apply the bundled Universal 3D template package set.
2. Add one Runtime assembly, one Editor assembly, and separate EditMode and PlayMode test assemblies.
3. Implement validated loop settings, a 60 Hz accumulator clock, immutable command/frame/recording data, and a fixed-capacity recorder.
4. Implement a shared kinematic character motor with keyboard and replay input sources.
5. Implement explicit reset registration and ordered loop transitions.
6. Implement Echo playback and drift measurement without position correction.
7. Implement a multi-actor pressure plate, door, goal, debug overlay, and path gizmos.
8. Implement an idempotent `P0SceneBuilder` that creates assets, scene references, layers, and the build-scene entry.
9. Generate the scene in batch mode, compile, and run EditMode and PlayMode tests.
10. Inspect logs and Git diff, then update documentation with verified results and remaining limitations.
11. Audit actor/environment/trigger collision layers and make the collision matrix reproducible through the scene builder.
12. Harden pressure-plate occupant cleanup for disabled and destroyed actors.
13. Lock the short-recording replay contract with EditMode and PlayMode coverage.
14. Validate the generated scene end to end, then produce and smoke-test a Windows x86_64 Development Build.

## Planned files

- `Unity/Assets/_Project/Scripts/Runtime/Core/LoopSettings.cs` and `Settings/LoopSettings.asset`
- Runtime scripts under `Scripts/Runtime/{Core,Input,Player,Replay,Reset,Interaction,Debugging}`
- `Scripts/Editor/P0SceneBuilder.cs`
- tests under `Scripts/Tests/{EditMode,PlayMode}`
- four assembly definition files
- generated materials, prefabs if needed, and `Scenes/P0_ReplayLab.unity`

## Design constraints

- One scene-owned `LoopDirector` coordinates transition order but does not read input, move actors, animate doors, or draw UI.
- `SimulationClock` owns real-time accumulation and emits explicit simulation ticks. Gameplay does not depend on Unity's `FixedUpdate` frequency.
- `CharacterMotor` updates transforms deterministically and uses a capsule cast to avoid walls without Rigidbody-to-Rigidbody forces.
- Player and Echo have non-trigger colliders on separate layers. The generated Layer Collision Matrix disables actor-to-actor contacts while preserving collisions with Environment and trigger overlap with InteractionTrigger.
- Replay storage is allocated to `LoopSettings.MaxTicks`; finalization copies the populated prefix into a private array.
- Reset membership is serialized by the scene builder and captured once by `ResetRegistry`.

## Verification gates

- Unity imports without C# compiler errors.
- Scene builder exits successfully and produces the expected scene and assets.
- EditMode and PlayMode test result XML files report no failures.
- Batch logs contain no unhandled exception or recurring gameplay warning.
- A manual two-loop run demonstrates the plate/door/goal flow.

## Recorded verification

- `P0SceneBuilder`: completed with exit code `0` and generated the scene, materials, settings asset, Echo prefab, layers, and Build Settings entry.
- EditMode: 12 passed, 0 failed, 0 skipped.
- PlayMode: 12 passed, 0 failed, 0 skipped.
- The generated-scene integration test recorded a path to the plate, reset the loop, replayed it through an Echo, observed the door opening, and moved the current player to the goal with `0 m` maximum measured drift in that run.
- Player/Echo collision separation, Environment blocking, InteractionTrigger detection, stale pressure-plate occupant cleanup, the three-Echo eviction path, and short replay termination are covered in PlayMode.
- The Windows x86_64 Development Build completed with zero reported build warnings and was smoke-tested headlessly for ten seconds without a matched unhandled exception or missing-reference error.
- The final scene-builder and test logs contain no matched C# warning, C# error, unhandled exception, missing script/reference, or test failure.
