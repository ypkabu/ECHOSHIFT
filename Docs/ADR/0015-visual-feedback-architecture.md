# ADR 0015: Bounded Visual and Audio Feedback Architecture

## Status

Accepted.

## Decision

Use scene-owned presentation adapters and one `Phase4FeedbackDirector`. Adapters observe authoritative gameplay components and modify only child transforms and `MaterialPropertyBlock` state. The director subscribes once to Loop, Interaction, Echo removal, and Goal notifications; it observes a fixed serialized Door array. Effects use a preallocated 12-system Particle pool and eight reusable AudioSources with duplicate-cue throttling.

## Reasons

- Keeps gameplay simulation independent of art, sound, and camera timing.
- Avoids runtime material instances, unbounded GameObject/AudioSource creation, and repeated hierarchy search.
- Central ownership makes section/restart subscription cleanup and one-shot completion behavior testable.
- `LoopDirector.EchoRemoved` exposes an event without changing Replay generation, eviction, or reset behavior.

## Alternatives considered

- One manager polling the entire Scene by tag/name: rejected by architecture and allocation rules.
- Device components owning gameplay and presentation together: rejected because art replacement would risk deterministic state.
- Instantiate/destroy every effect and sound: rejected for GC and overlap risk.
- Third-party VFX/audio framework: rejected as unnecessary scope.

## Current limitations

Audio is generated tonal feedback, not production sound design. Door observation is a small per-frame fixed-array read. VFX share one generic pulse shape with event-specific color/scale/duration.

## Replacement conditions

Replace cues or VFX when new assets remain licensed, pooled/bounded, gameplay-independent, duplicate-safe, and covered by lifecycle and performance tests.
