# Phase 2 Implementation Plan

## Status and baseline

Implementation starts on 2026-07-19 from `phase1-validated` (`b1b09e551b8519313bb2badd659c27f9111e2445`) after a successful `--ff-only` integration into `main`, on branch `feature/phase2-multi-echo-coordination`.

## Objective

Validate a three-loop puzzle with two simultaneously replaying Echoes. Echo 1 holds a PressurePlate for Gate A, Echo 2 traverses Gate A and replays Battery pickup and PowerSocket insertion for Gate B, and the current Player traverses both Gates to reach Goal.

Phase 0 and Phase 1 remain independent 10-second Scenes. Phase 2 owns a separate 15-second, 900-tick `LoopSettings` asset.

## Current tick processing order

The Phase 1 `LoopDirector` currently performs one complete Player transaction before any Echo:

1. `PlayerSimulation` samples input.
2. Player movement is simulated.
3. Player sensor selection and interaction execute immediately.
4. Player pose and successful interaction are recorded.
5. Each Echo, in current list order, moves, measures drift, and executes its recorded interaction immediately.
6. Loop termination is evaluated.

PressurePlate, Goal, and Door also receive ordinary Unity callbacks or `Update`, so their relationship to a replay tick is not an explicit transaction boundary.

## Nondeterminism risks with multiple Actors

- The current Player always executes before Echoes, contradicting the required oldest-Replay priority.
- An exclusive target can change ownership before later Actors expose their same-tick requests, making results depend on call order rather than a declared rule.
- Echo list order is chronological today but has no explicit Replay-generation identity and could be confused after eviction.
- Trigger callbacks and Door `Update` can occur between coordinator ticks based on Unity frame/physics timing.
- Door motion can affect a later Actor in the same coordinator tick if source state changes during an earlier Actor's interaction.
- `GameObject` creation order, Instance ID, physics query order, and `HashSet` enumeration are not valid final tie-breaks.
- Current carry pose copying occurs in `LateUpdate`; ownership is deterministic, but its visual Transform update is rendered-frame-bound and must remain outside interaction arbitration.

## Planned deterministic tick transaction

`LoopDirector` remains the Scene-owned coordinator, but only sequences component-owned operations. One tick will use these stages:

1. Begin the tick by applying Door state committed by the previous tick.
2. Acquire commands/recorded frames for oldest Echo through newest Echo, then current Player.
3. Simulate all Actor movement in that deterministic order.
4. Synchronize transforms, refresh PressurePlate occupancy, and refresh live Interaction sensors.
5. Collect at most one immutable Interaction request per Actor.
6. Sort and resolve requests deterministically.
7. Commit PressurePlate/PowerSocket-derived Door requests for the next tick.
8. Record Player pose/events and complete Echo pose/drift accounting.
9. Refresh Goal state and evaluate Goal, manual, and timer loop-end conditions.

Door source changes at tick N are committed during tick N and become collision-visible at the start of tick N+1. P0/P1 generated Scenes will use the same coordinated Door mode. Direct component tests retain a legacy visual-update mode where a `LoopDirector` is absent.

Unity trigger callbacks remain supported for ordinary Scene behavior, but coordinated PressurePlate and Goal state will also use explicit, non-allocating overlap refreshes after Actor movement. ADR 0007 will document the achievable stage boundary and Unity Physics limitations.

## Actor order and Interaction conflict rule

Each `LoopActor` will carry an explicit Replay generation:

- completed Loop 1 creates Echo generation 1;
- completed Loop 2 creates Echo generation 2;
- generations remain monotonic after eviction;
- the current Player carries the generation that its in-progress recording will receive.

Priority is oldest Echo, successively newer Echoes, then current Player. An immutable request contains tick, Actor kind, Replay generation, target Stable ID, interaction kind, expected Actor position, and the issuing Interactor.

Requests are compared by tick, Actor priority/generation, ordinal Stable ID, then interaction kind. The first request reserves an exclusive target for that tick. Later requests to that Stable ID fail once with `TargetBusy`; they do not resolve another target or retry next tick. Runtime Instance IDs and unordered collection enumeration are not used. ADR 0008 will record the rule and alternatives.

## Scene-specific LoopSettings and safety limit

- Keep the existing `LoopSettings.asset` at 60 Hz x 10 seconds = 600 ticks for P0/P1.
- Add `LoopSettings_P2.asset` at 60 Hz x 15 seconds = 900 ticks for P2.
- Continue preallocating both frame and sparse interaction buffers from `MaxTicks`.
- Add a maximum safe recording length of 36,000 ticks. At the current 60 Hz this is ten minutes, well beyond a puzzle loop while preventing accidentally authored multi-million-element frame and interaction arrays.
- Reject non-positive tick rate/duration/Echo count/catch-up count/speed/tolerance, more than three Echoes, arithmetic overflow, and calculated ticks above the safety limit.

## Loop History Debugger

Add a fixed-capacity plain-C# history containing immutable summaries only: Loop number, ticks, interaction events, Replay generation, maximum drift, interaction successes/failures, Active/Evicted state, and Timer/Manual/Goal/Test end reason.

Capacity will be 16 summaries. This shows substantially more than the three active Echoes and typical debugging session while bounding overlay work and memory. It never retains `ReplayRecording`, Echo, Interactor, or other Unity runtime references. ADR 0009 will document this choice.

## Planned files

### Add

- Runtime coordination types for Actor order, Interaction requests/resolution, and Loop history.
- `LoopHistoryDebugger` and a Phase 2 startup probe.
- `P2SceneBuilder`, `Phase2BuildPipeline`, and shared reproducible Build Settings helper.
- `Assets/_Project/Scenes/P2_CoordinationLab.unity`.
- `Assets/_Project/Settings/LoopSettings_P2.asset`.
- A Phase 2 Echo prefab and bounded temporary materials where needed.
- Phase 2 EditMode and PlayMode test files.
- ADR 0007, 0008, and 0009.
- `Docs/Phase2Validation.md` after measured validation.

### Modify

- `LoopSettings`, `LoopDirector`, `LoopActor`, `PlayerSimulation`, and `EchoPlayback`.
- `Interactor` and interaction contracts for request creation and `TargetBusy` results.
- `PressurePlate`, `GoalVolume`, and `DoorController` for coordinated tick refresh/commit.
- Existing P0/P1 Scene Builders to configure coordinated devices and preserve P2 Build Settings when it exists.
- Debug overlay for Actor kind/generation/ticks/interactions/carry/position/drift.
- README, AGENTS, Architecture, and TestPlan.

## Added tests

### EditMode

- P2 15-second/900-tick calculation and recorder preallocation.
- invalid settings including the 36,000-tick safety boundary.
- explicit Actor generation order and current-Player-last ordering.
- same-target oldest-Echo victory, Player rejection, Stable-ID tie-break, `TargetBusy`, and no fallback.
- Loop History aggregate values, 16-entry capacity behavior, eviction state, and absence of runtime references.
- Door source commit at tick N and collision-visible application at tick N+1.

### PlayMode

- Echo 1 plate hold and Gate A traversal.
- Echo 2 Battery pickup, Socket insertion, and Gate B opening.
- simultaneous two-Echo playback and non-blocking actor collision layers.
- deterministic Battery conflict and intact ownership after rejection.
- two-Echo rewind, fourth-recording eviction cleanup for plate and Battery.
- unchanged P0 and P1 generated-Scene integrations.
- complete generated P2 three-loop solution, drift, interaction failure count, missing references/components, and unexpected logs.

## Phase 0 and Phase 1 regression risks

- Staging interaction after all movement can shift existing event execution within a tick if wrapper APIs are not preserved.
- Coordinated Door state adds the formal one-tick delay; P0/P1 routes and tests must remain spatially valid without relaxing assertions.
- Explicit overlap refresh must coexist with trigger callbacks without double-counting occupants.
- Replay generation and history must not retain evicted Echo references or change the three-Echo cap.
- Scene-specific settings must not let P2 generation overwrite the shared P0/P1 10-second asset.
- Build Settings regeneration must retain all three Scenes in P2/P1/P0 order without changing Phase-specific build pipelines' explicit Scene lists.

## Explicitly out of Phase 2

Enemy AI, stealth, combat, inventory, save data, Steamworks, achievements, online features, Timeline editing, replay scrubbing or correction, free-Rigidbody replay, additive Scenes, a global Stable ID registry, a generic puzzle graph, Visual Scripting, Addressables, final UI/art/VFX/audio, and an in-game level editor are not implemented.

The coordinator will not become a general Simulation Manager: movement, interaction execution, device state, replay storage, history, and presentation remain in their existing focused components or small plain-C# services.
