# ADR 0012: Local Playtest Telemetry

## Status

Accepted for Phase 3 automation; subject to manual acceptance.

## Decision

Write schema-versioned JSON to `Application.persistentDataPath/EchoShiftPlaytests` on quit and completion. Store build/Unity versions, duration, per-section time and loop counts, end reasons, restart counts, interaction result aggregates, maximum drift, final section, and outcome. Snapshots clone arrays. Replay frames, commands, positions, Stable IDs, object references, personal data, upload, and cross-session mutation are excluded.

## Reasons

Small local aggregates are enough to evaluate greybox friction while minimizing privacy, payload, coupling, and replay lifetime risk. Schema version 1 makes later migration explicit.

## Alternatives considered

- Full replay capture: rejected for scope, size, and privacy.
- Remote analytics SDK: rejected because networking and consent are outside Phase 3.
- PlayerPrefs: rejected because structured session artifacts are easier to inspect and archive.

## Current constraints

Writes are best effort and log one clear error on failure. There is no retention policy, upload, dashboard, or schema migration yet.

## Replacement conditions

Replace when an approved analytics/privacy design requires consent, remote transport, retention, or version migration.
