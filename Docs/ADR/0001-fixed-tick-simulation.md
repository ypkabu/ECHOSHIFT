# ADR 0001: Explicit Fixed-Tick Simulation

## Status

Accepted for Phase 0.

## Decision

Use a plain `SimulationClock` accumulator at 60 Hz, advanced from a scene-owned coordinator with unscaled frame delta time. The coordinator executes zero or more explicit ticks per rendered frame.

## Reasons

Recording and replay require a stable tick index and duration independent of render rate. A plain clock is directly unit-testable and makes loop boundaries explicit.

## Alternatives considered

- Unity `FixedUpdate`: simple, but the ordering of multiple components is implicit and harder to test as a single transaction.
- Frame-based recording: produces different recordings at different render rates.
- DOTS fixed-step groups: excessive infrastructure for this prototype.

## Current limitations

Long stalls can exceed the catch-up budget and leave simulation time temporarily behind real time.

## Replacement conditions

Replace or integrate the clock when production physics, networking, pause semantics, or platform timing measurements prove that the Phase 0 accumulator is insufficient.
