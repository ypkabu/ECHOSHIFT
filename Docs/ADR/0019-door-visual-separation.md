# ADR 0019: Separate door gameplay authority from retracting visuals

## Status

Accepted for Phase 4.3.

## Context

`DoorController` moves the authoritative door root and Collider according to a coordinated one-tick rule. A single imported panel under that root reads as a large board when open and cannot provide a controlled presentation transition without changing gameplay timing.

## Decision

Keep `DoorController`, its open sources, root motion, Collider, and one-tick state unchanged. Place a project-owned visual assembly at the closed doorway position as a sibling presentation object. Two visual half-panels wait through a short seam-light preparation, then retract into the left and right frame over a bounded unscaled-time transition. Closing reverses the presentation.

`DoorVisualFeedback` reads the existing committed door state and drives only the visual assembly, panel renderers, and frame emission. Plate and Socket doors retain distinct circuit symbols. Presentation meshes have no Colliders and do not participate in raycasts or Stable ID resolution.

## Reasons

- Gameplay timing and collision remain independently testable and unchanged.
- Split panels no longer follow the authoritative Collider into a visibly oversized open position.
- The visual transition can be polished without adding a new door gameplay state.
- Scene Builder regeneration is deterministic.

## Alternatives

- Change `DoorController` to animate panels: rejected because it mixes presentation and simulation authority.
- Animate the Collider gradually: rejected because it changes the validated one-tick puzzle rule.
- Hide the old panel instantly: rejected because it removes the artifact but does not communicate a mechanical opening.

## Limitations and replacement conditions

The panels use a short transform animation without production VFX, sound redesign, or mechanical rigging. Replace the assembly when final Door art supplies authored retracting parts, provided the visual remains downstream of the existing one-tick gameplay state.
