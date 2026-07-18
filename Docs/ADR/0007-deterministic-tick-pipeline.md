# ADR 0007: Deterministic Tick Pipeline

## Status

Accepted for Phase 2.

## Decision

`LoopDirector` explicitly sequences each tick: apply previously committed Door state; acquire Echo frames and Player command; simulate oldest Echo through newest Echo then Player; call `Physics.SyncTransforms`; refresh PressurePlate and live Interaction sensors; collect requests; resolve requests; commit Door sources; record Replay poses and drift; refresh Goal and evaluate loop termination.

Door source changes committed at tick N become collision-visible at the beginning of tick N+1. PressurePlate and Goal retain Unity trigger callbacks for ordinary use, while coordinated Scenes also use bounded `OverlapBoxNonAlloc` refreshes at the declared sensor stage.

## Reasons

MonoBehaviour `Update`, trigger callback, registration, and GameObject creation order do not define a stable multi-Actor transaction. A Scene-owned coordinator makes the boundary testable while leaving movement, interaction, devices, and replay logic in focused components.

## Alternatives considered

- Script Execution Order: rejected because it spreads an implicit order across component metadata.
- One large simulation manager containing gameplay rules: rejected because it centralizes unrelated behavior.
- A separate Unity Physics simulation Scene: disproportionate for this kinematic prototype.

## Current constraints

Transform-based capsule casts and explicit physics synchronization are deterministic for this Scene and platform, but this is not a cross-platform deterministic physics engine. Carry visuals still update in `LateUpdate`; ownership and insertion do not depend on that visual update.

## Replacement conditions

Replace this pipeline if free-Rigidbody replay, network lockstep, cross-platform bitwise determinism, or additive simulation Scenes become required.
