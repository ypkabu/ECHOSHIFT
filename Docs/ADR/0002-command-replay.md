# ADR 0002: Command Replay With Pose Validation

## Status

Accepted for Phase 0.

## Decision

Record an immutable input command and post-tick expected pose for every tick. Echoes replay commands through the player's motor and measure pose drift after each tick. Phase 0 does not correct drift.

## Reasons

Command replay keeps Echoes as gameplay actors instead of transform animations. Expected poses expose nondeterminism without hiding it through teleportation.

## Alternatives considered

- Direct transform playback: deterministic visually but does not validate gameplay simulation.
- Rigidbody snapshots: broad physics rewind is outside Phase 0.
- Input-only recording: cannot quantify replay divergence.

## Current limitations

Collisions with a world state different from the recording can produce legitimate drift. Recordings are in-memory only.

## Replacement conditions

Add sparse correction checkpoints only if measured production scenarios cannot stay within tolerance after the source of divergence has been addressed.
