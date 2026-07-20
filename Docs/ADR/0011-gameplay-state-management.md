# ADR 0011: Gameplay State Management

## Status

Accepted for Phase 3 automation; subject to manual acceptance.

## Decision

Use a plain `GameplayStateController` with explicit `Booting`, `Playing`, `LoopTransition`, `SectionTransition`, `Paused`, and `Completed` states. A closed transition table rejects duplicates and illegal edges. The Scene coordinator applies state changes to Director pause/resume and UI; simulation code does not infer state from panels, time scale, or active GameObjects.

`Completed` remains terminal for simulation and section progression, but it may transition to `Paused` for the player-facing completion menu. The coordinator remembers whether Pause originated from `Playing` or `Completed`; Resume returns to that origin, and returning to `Completed` keeps the final Director shut down. Restart Section is disabled in the completion menu because the final section root has already completed its shutdown lifecycle. Restart From Beginning and Quit remain available.

## Reasons

An explicit graph makes double transitions, post-completion replay creation, pause timing, and restart behavior directly testable without introducing a framework or global singleton.

## Alternatives considered

- Boolean flags: rejected because combinations allow contradictory states.
- Animator-driven flow: rejected because presentation would own simulation lifecycle.
- General state-machine package: rejected as unnecessary Phase 3 scope.

## Current constraints

Pause is Scene-local and does not persist. The completion menu is not a new frontend state and does not reactivate gameplay. There is no boot/loading screen or save-state restoration.

## Replacement conditions

Replace only when asynchronous loading or persistent frontend states need hierarchical or concurrent state regions.
