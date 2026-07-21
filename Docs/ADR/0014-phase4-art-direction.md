# ADR 0014: Phase 4 Art Direction and Generated Modular Facility

## Status

Accepted for Phase 4 automation; human visual acceptance pending.

## Decision

Adopt an austere near-future temporal research facility using a small generated module kit and shared URP/Lit palette. P3 gameplay roots, colliders, Stable IDs, and section solutions remain authoritative; Phase 4 adds collider-free visual children only. White/charcoal architecture, cool PressurePlate circuits, warm Battery circuits, and a pale Goal portal form the global hierarchy.

## Reasons

- A single coherent kit addresses the accepted Phase 3 visual-simplicity Medium without importing a large third-party environment.
- Editor generation keeps Scene rebuilding repeatable and reviewable.
- Broad silhouettes and limited material channels are readable from the fixed gameplay camera.
- Presentation children can be replaced later without migrating gameplay state.

## Alternatives considered

- Large Asset Store environment: rejected for licensing, dependency, cohesion, and scope risk.
- Hand-authored external DCC assets: deferred because Phase 4 is a vertical slice, not final art production.
- Keep the greybox and change only materials: rejected because primitive scale hierarchy and silhouettes were major causes of the inexpensive appearance.

## Current limitations

Generated primitives do not provide final topology, UVs, authored textures, decals, or production character animation. Human review must decide whether the slice is portfolio-usable.

## Replacement conditions

Production art may replace visual children when it preserves collider roots, Door clearances, Camera readability, three Echo identities, device state language, Stable IDs, deterministic tests, and P0-P2 hashes.
