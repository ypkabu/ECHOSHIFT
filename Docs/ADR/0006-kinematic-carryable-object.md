# ADR 0006: Kinematic Carryable Object

## Status

Accepted for Phase 1.

## Decision

The Phase 1 Battery is a kinematic gameplay object. When free, it rests at an explicitly managed pose. When carried, it copies the shared actor Carry Socket pose during `LateUpdate` while remaining outside the actor hierarchy and with its collider disabled. When inserted, it is parented to the PowerSocket insertion Transform. Replay never records or restores arbitrary Rigidbody velocities.

The Battery owns single-holder and inserted-socket state. Interactor and PowerSocket maintain the opposite references, and reset explicitly severs both sides before restoring the authored Battery pose.

## Reasons

- Free dynamic physics would introduce nondeterministic replay and require broad physics rewind outside the milestone.
- A shared Carry Socket makes Player and Echo carry behavior identical and directly testable.
- Keeping the Battery outside the actor hierarchy prevents Unity hierarchy destruction from deleting it before ownership cleanup can run.
- Disabling collision while held prevents the Battery from pushing actors or the Environment.
- Explicit bidirectional cleanup handles Actor disable, destroy, and loop reset without relying on trigger-exit order.

## Alternatives considered

- Dynamic Rigidbody pickup: visually richer but nondeterministic and expensive to rewind correctly.
- Parent to the actor Carry Socket: concise, but destroying the actor hierarchy can destroy the Battery before cleanup.
- Transform animation independent of the actor: deterministic but bypasses ownership gameplay.

## Current limitations

Carry pose copying occurs once per rendered frame after simulation ticks, so it is suitable for the current kinematic socket but not a physics constraint. Drop placement is a simple deterministic offset and does not search for a physically ideal surface. The Battery has no throw, stacking, momentum, or removal-from-socket behavior.

## Replacement conditions

Introduce constrained physics or pose checkpoints only when production puzzles require throwing, dynamic obstacles, stacking, or physically authored carry motion and the corresponding rewind contract has been designed.
