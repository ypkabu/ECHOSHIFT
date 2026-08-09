# ADR 0018: Curated External Visual Asset Boundary

## Status

Accepted for Phase 4.1 automation; human visual acceptance pending. Supersedes ADR 0014 for the current acceptance candidate without altering its historical baseline.

## Decision

Use a small manifest-controlled CC0 subset of Quaternius Modular Sci-Fi MegaKit, Quaternius Animated Robot, and Kenney Sci-Fi Sounds. Store untouched selected originals under `Assets/_Project/ThirdParty` with source/license/hash records. Generate project-owned URP Materials and wrapper Prefabs under `Assets/_Project/Art`; only these wrappers may be composed into P3 presentation children.

Imported meshes, skeletons, vendor materials, names, and hierarchy never own gameplay. Stable IDs, colliders, layers, Motor, carry/reset state, Replay behavior, and section solutions remain on the existing project-authored roots. Robot imports use Rig None with animation/root motion disabled. Environment and actor visual children have no colliders.

## Reasons

- Human Visual Review found the generated primitive presentation insufficient; higher-quality authored topology and textures were needed without redesigning gameplay.
- One primary external kit keeps architecture coherent while a strict manifest avoids importing hundreds of unused variants.
- CC0 official sources support commercial use and Build distribution while local notices/hashes make provenance auditable.
- Project-owned deterministic wrappers isolate Unity import differences and allow the vendor presentation to be replaced without migrating gameplay state.
- Import, license, material, collider, Stable-ID, idempotence, and real-Scene tests make the boundary reproducible.

## Alternatives considered

- Continue refining generated primitives: rejected because the first visual review found the presentation materially below the target.
- Import entire free packages: rejected for repository size, shader/material noise, unused content, and audit cost.
- Use mixed Asset Store packages: rejected because licensing, account dependency, and style cohesion would be harder to verify.
- Make the imported Robot Animator authoritative: rejected because animation/root motion could move deterministic Actor roots and alter Replay drift.
- Modify vendor originals in place: rejected because it obscures provenance and makes upgrades/rebuilds nondeterministic.

## Current limitations

- The Robot is used as a static visual; there is no production locomotion, interaction animation, or IK.
- The vendor importer reports one self-intersecting `Foot.L` polygon; no automatic visible or Build defect was found, but both feet need human inspection in motion.
- Curated external art, project materials, lighting, VFX, audio, and UI still require human judgment for cohesion and portfolio quality.
- Runtime ProfilerRecorder did not provide authoritative Draw Calls, GPU Frame Time, SetPass, Triangle, or Vertex values on this host.

## Replacement conditions

Replace or expand the selected art only when official source, distributable license, archive/file hash, curated manifest, notices, Import settings, project-owned wrappers, no-gameplay-collider rule, Stable-ID/collider preservation, P0-P2 hash preservation, P3 automatic solution, Replay Drift, GC/performance, Build, and human visual-acceptance gates remain satisfied.
