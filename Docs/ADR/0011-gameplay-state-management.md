# ADR 0011: Gameplay State Management

## Status

Accepted for Phase 3 automation; subject to manual acceptance.

## Decision

Use a plain `GameplayStateController` with explicit `Booting`, `Playing`, `LoopTransition`, `SectionTransition`, `Paused`, and `Completed` states. A closed transition table rejects duplicates and illegal edges. The Scene coordinator applies state changes to Director pause/resume and UI; simulation code does not infer state from panels, time scale, or active GameObjects.

## Reasons

An explicit graph makes double transitions, post-completion replay creation, pause timing, and restart behavior directly testable without introducing a framework or global singleton.

## Alternatives considered

- Boolean flags: rejected because combinations allow contradictory states.
- Animator-driven flow: rejected because presentation would own simulation lifecycle.
- General state-machine package: rejected as unnecessary Phase 3 scope.

## Current constraints

Pause is Scene-local and does not persist. There is no boot/loading screen or save-state restoration.

## Replacement conditions

Replace only when asynchronous loading or persistent frontend states need hierarchical or concurrent state regions.
