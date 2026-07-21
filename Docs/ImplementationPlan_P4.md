# Phase 4 Visual Polish Vertical Slice Implementation Plan

## Baseline and objective

Phase 3 was fast-forwarded into `main` at `86cdf46635b0de8df3c0adf599a670d903941fe8`, matching `phase3-validated`. The first Phase 4 automation pass was preserved at `e043b160a945939d5f862a4648d3df728a40a49e` and tagged `phase4-automation-passed`, but human Visual Review rejected its primitive-led presentation. Corrective work proceeds from that immutable baseline on `feature/phase4-external-asset-integration`. The objective remains presentation quality suitable for a Steam page, internship application, and portfolio footage without changing puzzle rules, replay formats, fixed-tick simulation, Stable IDs, colliders, or section solutions.

Current milestone result: the bounded external-asset integration, corrective visual changes, tests, captures, performance probe, Development/Non-Development Builds, and shutdown matrix are complete. **Phase 4.1: Automation Passed; Human Visual Review Pending.** The plan remains open only at the human visual-acceptance gate; it does not authorize `phase4-validated`.

## Why the current build looks inexpensive

- Rooms are large unbroken cubes with uniform materials, little scale hierarchy, and no authored facility silhouette.
- Player, Echoes, Doors, Plate, Battery, Socket, and Goal are mostly one primitive each, so function is communicated mainly by color and labels.
- One Directional Light and flat material response give foreground, actor, and background nearly equal visual weight.
- Device wiring is a single thin LineRenderer with no inactive/active structure.
- HUD text is functionally complete but lacks panel hierarchy, iconography, progress framing, and consistent spacing.
- Feedback is limited to color and scale changes; audio and most event-specific VFX are absent.
- Japanese text depends on a machine-installed OS font, which is unsuitable for a distributable Steam build.

## Art direction

Theme: **an austere near-future research facility for temporal duplication experiments**. The facility is clean and controlled, with a slightly unsettling dark void beyond readable structural panels. The camera reads traversable floor shapes first, doors and puzzle routes second, and actors/devices third. The free CC0 Standard edition of Quaternius Modular Sci-Fi MegaKit is the primary shape library; imported originals remain isolated from project-authored materials and prefabs. Kenney and Poly Haven are used only when the primary library has a documented gap, so one section never becomes a mixture of competing kit styles.

## Visual language

- Environment: warm white structure, charcoal recesses, black void, low-emission neutral surfaces.
- Player: high-luminance warm white/amber body core, forward-facing visor, circular identity base.
- Echoes: cyan / violet / blue-green generation channels plus distinct chest bars, head fins, labels, and trails; generation recognition never relies on color alone.
- PressurePlate circuit: cyan, inset chevron pad, cyan floor conduit, single-bar Door badge.
- Battery circuit: orange, directional battery fins/core, orange floor conduit, double-bar Door badge and keyed Socket opening.
- Goal: white/green vertical portal arch and converging floor chevrons, not a red/green device state.
- Failure/hazard: red plus cross/flash motion; success: white/cyan burst plus expanding ring.
- Interactable: outline-like duplicated silhouette, emission, and foot/device ring. Non-interactive structure has restrained emission.
- Closed/open state combines position, shape, badge, emissive line, and animation; red/green is supplementary only.

## Implementation targets

1. Split Phase 4 authoring out of the P3 builder into visual-asset, environment/device/actor, lighting-volume, UI, audio, validation, capture, and build helpers. The P3 builder remains the gameplay source and invokes the Phase 4 pass only for P3.
2. Add shared `Phase4VisualSettings` with required colors, materials, font, VFX, audio cues, HUD icons, capture resolution, and safe post-processing values.
3. Rebuild visible P3 rooms from modular panel, trim, column, corner, pedestal, machine-bank, section-boundary, and Goal modules while preserving simple gameplay colliders. Camera-crossing overhead rails were removed during the Phase 4.1 corrective pass.
4. Replace visible actor primitives with compound low-poly experiment suits; keep the existing root collider, movement, carry socket, and replay ownership unchanged.
5. Add device-specific visual adapters driven by existing Plate, Door, Battery, Socket, Goal, Loop, and Interaction state.
6. Add a small preallocated feedback pool and bounded AudioSources. Use PropertyBlocks and shared materials; do not instantiate runtime materials.
7. Redesign the legacy Unity UI hierarchy with restrained panels, progress strip, icon marks, safe anchors, and responsive scaling while retaining current Japanese strings and control paths.
8. Package Noto Sans JP from the official Google Fonts `ofl/notosansjp` source. The official repository places the family under OFL and supplies `OFL.txt`; the license permits bundling with software when the copyright notice and license accompany it. Store the source font and license locally, build a project font asset, and remove the runtime OS-font dependency.
9. Add bounded URP Volume overrides: ACES tonemapping, mild contrast/saturation, thresholded Bloom, subtle Vignette, and the existing restrained SSAO renderer feature. Do not use Motion Blur, Chromatic Aberration, or gameplay Depth of Field.
10. Add a reproducible 1920x1080 capture pipeline for eight required gameplay compositions; preserve the original set and write matching Phase 4.1 comparisons under ignored `Captures/Phase4_1/Before` and `Captures/Phase4_1/After`.

## Existing functions reused

- Fixed-tick `LoopDirector`, immutable replay and interaction recordings, Actor arbitration, reset registries, and Stable IDs.
- `DoorController`, `PressurePlate`, `CarryableBattery`, `PowerSocket`, and `GoalVolume` as authoritative state.
- `SectionTransitionCoordinator`, `GameplayHud`, Pause flow, tutorial catalog, Camera settings, telemetry, and all P0-P3 integration tests.
- Existing MaterialPropertyBlock, TrailRenderer, LineRenderer, EventSystem, URP PC renderer, and local Development Build probe.

## Planned new assets

- `Phase4VisualSettings.asset` and generated shared material set.
- A curated subset of the Quaternius Modular Sci-Fi MegaKit Standard FBX library under `Assets/_Project/ThirdParty/Quaternius/`; project-authored prefabs/materials remain under `Assets/_Project/Art/`.
- A candidate Quaternius Animated Robot visual imported separately and accepted only if 1920x1080 gameplay-camera comparison confirms suitable scale, silhouette, carry pose, and Echo readability without root motion.
- A curated subset of Kenney Sci-Fi Sounds under `Assets/_Project/ThirdParty/Kenney/`, copied into project-authored AudioClips only where it improves an existing bounded cue.
- `Docs/ThirdPartyAssets.md`, per-package license/source snapshots in `ThirdPartyNotices/`, archive SHA-256 values, imported-file manifests, and import-setting evidence.
- `NotoSansJP[wght].ttf`, project font asset, and `ThirdPartyNotices/NotoSansJP-OFL.txt`.
- Shared actor/device/environment visual prefabs or builder-authored module roots.
- Bounded VFX prefabs for spawn/despawn, loop, interaction, Battery, Door, Section, and completion feedback.
- Generated WAV tone cues plus `Phase4AudioCueSet.asset`.
- `Phase4VolumeProfile.asset`, lightweight HUD icon sprites/materials, capture settings, and performance probe configuration.
- Runtime presentation adapters and Editor builders/capture/build pipeline.

## GPU, CPU, and GC risks

- Primitive modular detail can increase draw and SetPass counts. Mitigation: one shared material palette, SRP Batcher-compatible URP/Lit materials, no per-object material instances, modest module count, no high-density clutter.
- Transparent Echo/VFX overdraw can become expensive. Mitigation: opaque or near-opaque Echo bodies, short narrow trails, bounded particles, limited screen coverage.
- Additional lights and shadows can multiply GPU cost. Mitigation: one shadowed Directional Light and a small fixed number of unshadowed local lights, maximum four additional lights per object.
- Per-frame hierarchy search, Raycast, string generation, or allocation would violate the budget. Mitigation: serialized references, event/state caches, fixed arrays/pools, PropertyBlocks, and no runtime material creation.
- Dynamic font atlas growth can allocate and produce missing glyphs. Mitigation: packaged static/project font coverage for the catalog glyph set and explicit fallback policy.
- Post-processing can reduce fill-rate headroom. Mitigation: mild Bloom/Vignette/Color Adjustments, existing half/low-cost SSAO settings, no blur/DOF/aberration.

## P0-P3 regression risks

- Rebuilding P3 can accidentally touch older scenes, change Stable IDs, duplicate assets, or alter Build Settings.
- Child meshes can intercept trigger queries or change Actor collision if visible colliders are retained.
- Door frames can block the authored clear width, and foreground structure can occlude Players.
- Visual feedback may mutate gameplay transforms, shared materials, or reset state.
- Font/UI replacement can break Japanese wrapping, Pause navigation, or non-16:9 anchors.
- Audio/VFX event subscriptions can duplicate on restart or section activation.

Mitigation is explicit separation: visible children have no gameplay colliders, gameplay roots and IDs remain authoritative, P0-P2 SHA-256 tests remain unchanged, state adapters write only child visuals/PropertyBlocks, and lifecycle tests cover duplicate subscription/playback.

## Performance and rendering measurement

- Windows Development Build at 1920x1080 is the reference.
- Runtime `ProfilerRecorder` samples Main Thread time, GC allocated per frame, draw calls, SetPass calls, total/reserved memory, and transition maxima into a committed summary; no unsupported value will be inferred.
- A graphical timed probe is preferred for rendering counters. Headless/Null-GPU values are labelled non-representative.
- Frame Debugger composition is inspectable from the generated Scene and shared materials; interactive Frame Debugger observations that cannot be automated will remain a manual Visual Review item.

## Screenshot plan

The Editor capture pipeline renders the actual gameplay Camera at 1920x1080 with Debug Overlay off and cursor hidden. It stages only presentation state and never changes Stable IDs or saves staged gameplay state. Required outputs:

1. Section 1 facility overview;
2. Player with Echo 1 identity treatment;
3. visible Battery carry composition;
4. inserted Battery, powered Socket, and opened Door;
5. Echo 1 / Echo 2 role composition;
6. Section 3 facility overview;
7. Goal arrival/completion presentation;
8. redesigned gameplay HUD or Pause Menu.

The current matching captures are written to ignored `Captures/Phase4_1/Before` and `Captures/Phase4_1/After` and validated for names, count, and 1920x1080 dimensions.

## External-asset corrective pass (Phase 4.1)

1. Preserve the rejected automation build and its eight captures as the `Before` baseline; regenerate only if an exact named state is missing. Produce matching `After` captures under `Captures/Phase4_1/` at 1920x1080 with cursor and Debug Overlay hidden.
2. Download only official, login-free archives into ignored `ExternalDownloads/`. Record original SHA-256 before extraction and never commit an archive.
3. Import only the FBX, texture, and audio files selected by a machine-readable manifest. Keep vendor originals under `ThirdParty`, with no gameplay colliders and no vendor material used directly by a Scene.
4. Build project-owned URP materials and modular prefabs around the selected meshes. Floors become brighter than walls, door silhouettes stay readable, ambient fill prevents black crush, and AO/Vignette are reduced.
5. Remove camera-crossing opaque beams and permanent world labels. Replace identity text with silhouette, floor ring, compact non-text generation marks, device geometry, emission, and one restrained EXIT sign.
6. Rebuild Door and Goal presentation with frame/moving-panel separation, distinct plate/socket circuit marks, a recognizable portal volume, rear glow, vertical particles, and floor guidance while retaining the gameplay roots and colliders byte-for-byte in authored data.
7. Rebuild Pause presentation so gameplay HUD/world tutorial presentation is hidden or strongly dimmed, focus state has background/border/arrow, destructive Quit is separated, and all supported resolutions stay inside safe bounds.
8. Add licensing, import-health, no-world-label, no-occluding-beam, root-motion, animation-transform, Stable-ID/collider, responsive-HUD, and visual-material validation. Retain every P0-P3 and existing Phase 4 test.
9. Re-run Scene generation, all EditMode/PlayMode tests, P3 automated completion, performance probes, comparison captures, and Windows builds. Record unsupported GPU/draw/texture-memory counters as unavailable rather than inferred.
10. Isolate ordinary-window `0xC0000005` by Development/non-Development, D3D11/D3D12, audio enabled/disabled, Pause-Quit/Window-Close, and Windows Event Viewer fault-module evidence. Do not conflate clean batch auto-quit with ordinary-window behavior.

Commit boundaries are: licensed vendor import, environment/presentation rebuild, readability fixes and tests, then the final automation evidence update. `phase4-assets-automation-passed` may be created only after every gate passes; `phase4-validated` remains prohibited until a new human Visual Review accepts the corrected build.

## Out of Phase 4

Enemy AI, stealth, combat, new puzzle mechanics, story dialogue, voice acting, Steamworks, achievements, save slots, production settings/rebinding UI, final character models, high-quality humanoid animation/IK, large Asset Store packages, procedural generation, level editor, replay redesign, online features, production BGM, and final content-scale art production remain excluded.

## Completion gate

Corrective automation may create `phase4-assets-automation-passed` only after every retained and new test, both Builds, packaged-font probe, captures, measured performance report, and shutdown matrix pass with no Critical/High automation defect. Phase 4 is not formally complete and `phase4-validated` must not be created until the separate human Visual Review is recorded and any Critical/High visual issue is corrected and rebuilt.

## Execution status

The primitive-led automation pass was implemented and automatically validated on 2026-07-21, but human Visual Review found four High presentation defects: excessive darkness, camera-crossing black beams, persistent world labels, and insufficient Pause hierarchy. Phase 4.1 completed the external-asset corrective pass with 93/93 EditMode, 93/93 PlayMode, matching captures, measured performance, two Windows Builds, and a nine-scenario clean shutdown matrix. Its evidence is recorded independently without rewriting the original `phase4-automation-passed` baseline. Human Visual Review remains pending.
