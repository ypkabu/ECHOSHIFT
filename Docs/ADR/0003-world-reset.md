# ADR 0003: Explicit Registered World Reset

## Status

Accepted for Phase 0.

## Decision

Resettable components implement `IResettable`. A scene-owned `ResetRegistry` receives a serialized component list, captures initial state once, and restores that list in a defined order during loop transitions.

## Reasons

Explicit registration avoids scene reloads, repeated searches, hidden lifecycle ordering, and accidental inclusion of unrelated objects.

## Alternatives considered

- Reloading the scene: resets state but destroys replay continuity and obscures transition ordering.
- Searching for interfaces every loop: avoidable cost and easy to misuse at runtime.
- General serialized snapshots: unnecessary abstraction for the small Phase 0 state set.

## Current limitations

Unregistered components are not reset. Phase 0 captures only component-specific state, not arbitrary Rigidbody state or instantiated object graphs.

## Replacement conditions

Introduce typed snapshot storage and validation when levels contain dynamic spawn/despawn, cross-object restore dependencies, or many resettable object types.
