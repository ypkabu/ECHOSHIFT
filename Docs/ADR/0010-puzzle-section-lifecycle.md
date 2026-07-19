# ADR 0010: Puzzle Section Lifecycle

## Status

Accepted for Phase 3 automation; subject to manual acceptance.

## Decision

One generated Scene contains three independently owned section roots. `PuzzleSectionController` owns each section's spawn, Player, LoopDirector, Goal, and reset boundary. `SectionTransitionCoordinator` deactivates the outgoing root after shutting down its Director, releasing carried objects, clearing Echoes, and restoring registered state; it then activates and initializes the next root. Normal loop and section transitions never reload the Scene.

## Reasons

This preserves deterministic Phase 0-2 reset behavior, makes stale Echo/reference cleanup explicit, and allows a continuous playable flow without cross-scene persistence.

## Alternatives considered

- One Scene per section: rejected because reload and persistence would obscure lifecycle ownership.
- One global Director for every section: rejected because reset/history boundaries would be ambiguous.
- Destroy and recreate the full section: rejected because authored reset state is already deterministic and tested.

## Current constraints

Only three authored sections are supported. All section-owned mutable gameplay objects must be registered explicitly.

## Replacement conditions

Replace with additive scenes or content streaming only when production-sized levels require it and persistence semantics have their own ADR.
