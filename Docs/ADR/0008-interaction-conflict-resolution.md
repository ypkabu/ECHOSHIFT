# ADR 0008: Interaction Conflict Resolution

## Status

Accepted for Phase 2.

## Decision

Each Actor emits at most one immutable request per tick. Requests sort by tick, Actor priority and Replay generation, ordinal target Stable ID, then Interaction kind. Actor priority is oldest Echo, successively newer Echoes, then current Player.

The first request for an exclusive Stable ID reserves that target for the tick, even if execution fails. A later same-target request returns `TargetBusy`. It does not select another nearby target and is not carried into the next tick. Instance IDs, physics result order, and unordered collection enumeration never determine the result.

## Reasons

Prior recordings must remain stable when the current Player attempts the same action. Reserving the target for the entire tick produces one observable result and prevents a failed earlier request from making later execution dependent on target-specific failure behavior.

## Alternatives considered

- Current Player first: rejected because it can invalidate an already recorded solution.
- Last writer wins: rejected because it cannot preserve exclusive ownership.
- Retry or nearest-target fallback: rejected because it changes recorded intent.

## Current constraints

The resolver has a fixed four-request buffer: three Echoes and one Player. A target is exclusive at Stable-ID granularity for one tick. Phase 2 has no multi-target atomic transaction.

## Replacement conditions

Replace or extend the policy only if a later design introduces cooperative non-exclusive actions, more than three Echoes, or explicitly atomic multi-object commands.
