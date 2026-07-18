# Phase 0: Replay Laboratory

## Scope

Phase 0 validates only fixed-tick command recording, Echo playback, explicit loop reset, trigger interaction, and replay drift measurement.

The simulation runs at 60 ticks per second for 10 seconds, producing at most 600 frames per recording. Up to three completed recordings are replayed as Echo actors; when a fourth recording is completed, the oldest Echo and recording are removed.

## Success scenario

The player walks to the pressure plate in loop 1. At the next loop, the Echo repeats that path and holds the plate. The current player crosses the open doorway and reaches the goal.

## Explicit exclusions

Phase 0 has no enemy AI, explicit interaction button, object carrying, Stable IDs, Steam integration, save data, full rewind, gamepad input, production UI, final art, audio, or VFX.

## Runtime observability

The debug overlay reports loop number, tick, Echo count, maximum replay drift, plate state, and goal state. Scene gizmos show recorded expected paths and actual Echo paths.

## Verification status

The generated `P0_ReplayLab` scene compiles, loads in PlayMode, and builds as a Windows x86_64 Development Player. Automated integration coverage confirms the intended two-loop solution, three-Echo cap, actor-to-actor Layer Collision Matrix separation, Environment blocking, InteractionTrigger detection, explicit reset, short-recording replay stop, pressure-plate stale-reference cleanup, and replay drift tolerance. Keyboard feel and visual framing still require the documented manual Player check.
