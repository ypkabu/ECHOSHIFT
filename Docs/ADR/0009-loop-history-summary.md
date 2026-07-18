# ADR 0009: Bounded Loop History Summary

## Status

Accepted for Phase 2.

## Decision

`LoopHistory` stores at most 16 immutable value summaries. Each contains Loop number, recorded ticks, interaction-event count, Replay generation, maximum drift, interaction successes/failures, Active or Evicted state, and Timer, Manual, Goal, or Test end reason.

When full, adding a summary drops the oldest summary. Eviction changes only the summary state. No `ReplayRecording`, Echo, Interactor, Unity object, or other runtime reference is retained.

## Reasons

Sixteen entries exceed a typical debugging session and the three active Replay limit while bounding memory and overlay work. Storing aggregates avoids duplicating up to 900 Replay frames per loop.

## Alternatives considered

- Unbounded history: rejected because long Editor sessions would grow indefinitely.
- Duplicate complete Replay data: rejected because active playback already owns it.
- Disk persistence: outside the Phase 2 in-memory validation scope.

## Current constraints

Once a summary ages out, it cannot be recovered. Runtime metrics update when a loop transitions; the current in-progress loop is shown through live Actor data instead.

## Replacement conditions

Replace with a persistent telemetry format if long-session analysis, save data, or automated external profiling requires it.
