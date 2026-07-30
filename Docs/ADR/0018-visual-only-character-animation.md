# ADR 0018: Visual-only procedural character animation

## Status

Accepted for Phase 4.3.

## Context

The curated Quaternius Robot improves the actor silhouette but appears in a T-pose. Its imported FBX has no named Idle, Walk, Carry, Interact, or Stopped clips; animation import is disabled and there is no Avatar. Gameplay roots are already authoritative for fixed-tick movement and replay drift.

## Decision

Drive the existing project-owned robot wrapper bones with a `Phase4RobotPoseController` placed below the gameplay root. The controller reads root displacement, existing carry state, existing interaction-success counters, and Echo playback completion. It blends six presentation states without writing to gameplay state.

An Animator and project-owned controller remain on the visual child only, with Root Motion disabled and empty semantic states matching the six pose states. The procedural component performs the actual bone offsets after Animator update. No Animation Events or gameplay Transform curves are permitted.

## Reasons

- It leaves the third-party FBX and importer metadata unchanged.
- It avoids inventing clip boundaries from an unnamed source take.
- It keeps replay and collision authority on the fixed-tick gameplay root.
- It provides deterministic, builder-authored state names and testable references.

## Alternatives

- Enable and split the FBX take: rejected because source clip intent and boundaries are not defined.
- Root Motion: rejected because it would compete with CharacterMotor and replay poses.
- Animation Rigging package: rejected because Phase 4.3 does not justify another framework or package.
- Leave the robot static: rejected by Human Visual Review.

## Limitations and replacement conditions

The procedural motion is intentionally simple and does not provide production-quality foot placement or IK. Replace it only when final character art includes authored clips with known licenses, named state coverage, and a validated visual-only import pipeline. Gameplay roots must remain authoritative after replacement.
