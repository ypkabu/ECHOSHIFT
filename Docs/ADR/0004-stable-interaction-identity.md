# ADR 0004: Stable Interaction Identity

## Status

Accepted for Phase 1.

## Decision

Replay-addressable interaction targets receive a serialized GUID-format `StableId`. Phase 1 assigns IDs only to the Battery and PowerSocket. IDs are authored and repaired by Editor tooling, validated for empty and duplicate values before a generated scene is saved, and never generated during runtime initialization.

A scene-owned `InteractionRegistry` stores the mapping from ordinal ID string to active `IInteractable`. Each `StableId` registers when enabled and unregisters when disabled. Recorded playback resolves exactly one ID through the Registry and does not search the Scene or substitute another target.

## Reasons

- Transform hierarchy paths and object names are fragile under renaming and reparenting.
- Direct Unity object references cannot identify the corresponding target after a loop reset or across an immutable serialized command.
- Editor-owned GUID values remain stable across scene saves without introducing a global identity system for unrelated objects.
- A scene-owned dictionary provides constant-time lookup and explicit lifetime cleanup.

## Alternatives considered

- Hierarchy path: readable but changes under ordinary scene editing and duplicate names are ambiguous.
- Runtime-generated GUID: cannot reproduce a previously recorded target after restart and violates authoring stability.
- Scene-wide lookup on every event: simple but allocates or scans repeatedly and can silently choose a different object.
- Stable IDs on every GameObject: unnecessary scope and authoring noise for the Phase 1 experiment.

## Current limitations

IDs are unique only within one validation scene and are not a save-game or cross-scene content identity. Runtime duplicate registration is rejected, but authoring repair is an Editor responsibility.

## Replacement conditions

Replace the scene-local scheme when production levels require cross-scene persistence, streamed content, save migration, prefab-instance identity, or externally authored content catalogs.
