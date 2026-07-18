# ADR 0005: Separate Interaction Event Replay

## Status

Accepted for Phase 1.

## Decision

Keep movement frames and successful interaction commands as separate immutable columns in one `ReplayRecording`.

Each `InteractionCommand` stores its simulation tick, operation kind, target Stable ID, and the actor position used for validation Gizmos. `ReplayRecorder` accepts an interaction only after the matching movement frame exists, enforces increasing tick order, and copies the populated event prefix during finalization.

Echo playback advances a separate interaction cursor after simulating movement for that tick. It resolves the recorded Stable ID and attempts the recorded operation once. Failure is counted with a reason and never causes spatial correction, target reselection, or repeated per-tick logging.

## Reasons

- Most movement ticks contain no interaction, so a sparse column avoids empty per-frame payloads.
- The Phase 0 `ReplayFrame` contract remains intact.
- Event counts, next-event ticks, success, and failure are directly observable.
- A separate cursor naturally stops at the finalized short-recording boundary.

## Alternatives considered

- Embed an optional interaction in every ReplayFrame: simple indexing but increases the core frame payload and couples two concerns.
- Record raw Interact input: would replay failed attempts and could choose a different target.
- Record target transforms: bypasses gameplay rules and does not validate interaction replay.

## Current limitations

Phase 1 records at most one successful interaction per simulation tick. Commands are in-memory only, and there is no schema migration or persistence format.

## Replacement conditions

Replace or version the format when simultaneous interactions, save persistence, network transfer, deterministic rollback, or interaction payload evolution becomes required.
