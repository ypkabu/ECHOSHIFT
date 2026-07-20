# Phase 3 Implementation Plan

## Baseline

Phase 2 was fast-forwarded into `main` at `f88ff829fb15ef28bba72ae4a46b25c37aafc0ec`. Phase 3 work proceeds on `feature/phase3-playable-greybox`. Phase 0 through Phase 2 tags remain unchanged.

## Player experience

The greybox teaches one concept per section without a long instruction screen. Section 1 teaches movement, ending a loop, and an Echo holding a PressurePlate. Section 2 introduces recorded Battery pickup and PowerSocket insertion. Section 3 combines both ideas into the validated two-Echo coordination puzzle. Short objectives, prompts, floor wiring, framing, and object state feedback replace explanatory menus. The expected first-session duration is five to ten minutes; this duration remains a manual acceptance item.

The Japanese black-box High correction keeps manual `R` loop completion and changes only the Phase 3 authoring to a 45-second ceiling. Staged in-game guidance now advances after reaching the Plate, recording the first loop, and completing Battery interactions. The Camera follows position with fixed rotation, the foreground wall has a low visible mesh plus a full-height invisible collision boundary, and world labels/rings identify the current Player, Echo generations, and puzzle devices. These changes address observed first-session comprehension and do not modify Phase 0 through Phase 2 settings or Replay formats.

The clean-context Fresh3 retest exposed a second interaction High: a Socket whose collider was sensed just outside its center-distance range advertised Insert while the live request silently fell back to Drop. A non-actionable candidate will now report its own failure and preserve the carried Battery; Drop remains available only when no candidate exists. The HUD follows the same actionable-candidate rule. In P3, `IsPowered` means the Battery is visibly connected, while `RequestsDoorOpen` additionally requires a replay Actor to have performed that insertion; the live insertion therefore provides success feedback and the staged tutorial can require recording before passage. The same correction moves an open P3 Door laterally out of the camera-framed opening, authors world markers at unit world scale, and holds the first Loop paused behind an explicit input-to-start prompt in interactive Standalone sessions. Starting with E/A or R/START consumes those one-shot action latches before simulation resumes. Batchmode tests and the `-phase3AutoQuit` package probe bypass that start gate.

The subsequent source-blind Fresh4 retest exposed a Section 1 onboarding boundary High. Its oversized tutorial volume changed the guidance to "on the switch" before the Player collider overlapped the actual PressurePlate, so the Player could reasonably end a Loop while the Door was still closed and reproduce the same non-pressing endpoint with an Echo. The tutorial step will use the PressurePlate's own trigger collider, making the instruction and device activation share one physical boundary. Regression coverage will exercise the former false-positive edge, the true Plate boundary, Door state, and an Echo replay from the accepted endpoint. The Windows Security firewall prompt observed during this Development Player run is tracked separately as a test-environment interruption rather than attributed to game logic; the next isolated package will be preflighted at its final path before it is handed to a new blind tester.

## Three-section introduction order

1. **Echo Basics**: Player, Plate, one Door, Goal. No Battery or Socket.
2. **Recorded Interaction**: Player, Battery, Socket, one powered Door, Goal. No Plate.
3. **Multi-Echo Coordination**: Plate/Gate A plus Battery/Socket/Gate B and final Goal, retaining the Phase 2 three-loop solution.

The sections occupy separated greybox bays in one Scene. Only the current section root is active, so inactive content cannot interact with physics or Stable ID registries. A short transition switches roots instead of reloading the Scene.

## LoopDirector responsibility audit

The Phase 2 `LoopDirector` is 449 lines and currently owns:

- fixed-tick orchestration and deterministic Actor order;
- Interaction request collection/resolution and device commit order;
- one Loop recorder and immutable finalized recordings;
- Player/Echo reset, rewind, spawn, cap, and eviction;
- Loop history aggregation and loop-end reasons;
- a narrow direct-tick test entry point.

It is large but cohesive around one Loop lifecycle. It does not own UI, Camera, tutorial text, section layout, telemetry I/O, materials, audio, VFX, or Scene rebuilding. Phase 3 will not move those concerns into it. The only additions are external pause gating, section restart/shutdown lifecycle APIs, and notifications for Loop/Interaction results. `SectionTransitionCoordinator` owns section and game state. ADR 0010 and 0011 record the boundary.

## Responsibility split

- `SectionTransitionCoordinator`: explicit Booting/Playing/LoopTransition/SectionTransition/Paused/Completed state, section sequencing, restart, pause, and quit requests.
- `PuzzleSectionController`: one section's Player, LoopDirector, Goal, spawn, and activation/reset boundary.
- `SectionCameraController` + `Phase3CameraSettings`: smooth target following and section handoff.
- `GameplayHud` / `PauseMenuController`: production-style information and buttons, separate from `Phase0DebugOverlay`.
- `TutorialGuide` + `Phase3TextCatalog`: centralized display strings and tutorial progress.
- focused visual adapters: target highlight, Door state, Echo trail/pulse, and authored floor wiring.
- `PlaytestTelemetry`: local bounded aggregates and JSON persistence; no tick positions or replay payloads.

## Puzzle section transition

On completion, the coordinator atomically enters SectionTransition, pauses the active Loop, releases held objects, clears its Echoes/replay/history, deactivates the old root, activates the next root at its authored spawn, captures that section's own reset baseline through its independent LoopDirector/ResetRegistry, retargets Camera/HUD, displays a short message, and resumes. Section roots use separate Scene-owned registries and reset lists, preventing a prior section state from becoming the next reset baseline. Loop reset and section switching remain separate methods.

## Game state and transition timing

`GameplayStateController` is a small plain-C# transition validator. Pause and transition states gate Loop ticks and recording. Presentation durations use unscaled time through an injectable/skip-capable coordinator method; tests advance transitions explicitly without real-time waits. Runtime Loop transition feedback defaults to 0.75 seconds and section transition feedback to 1.0 second.

## Camera, HUD, and visual feedback

No Cinemachine package is installed. A small position-only smooth-follow camera with fixed rotation is sufficient and avoids a new dependency. Offset, smoothing, look offset, and section framing are stored in a ScriptableObject. The HUD uses a scaled Canvas, last-device prompts, section/loop/time/Echo/carry state, transition/completion text, and a separate pause panel. Interaction prompts name Battery pickup, Battery drop, and Socket insertion instead of using one generic device label; an invalid sensed target shows the localized failure instead of advertising an action that will not run. The bottom guidance occupies non-overlapping safe-area rows at 1280x720. F3 toggles the existing debug overlay.

Greybox visuals combine color with geometry, emission, trails, target highlight, state color, and floor wiring. Runtime changes use MaterialPropertyBlock. Echoes retain visible alpha, generation colors, a trail, interaction pulse, stopped state, and failure indication.

## Tests

EditMode additions cover the state transition graph, pause/duplicate-transition gates, section reset models, tutorial progress, device prompt changes, immutable telemetry aggregates/schema/no replay references, HUD/history/telemetry loop consistency, non-empty text catalog, and P3 Builder idempotence including P0-P2 Scene hashes and Stable IDs.

PlayMode additions use injected tick advancement and zero-duration transitions to cover all three section solutions, Player spawns, old Echo/holding/device cleanup, pause/resume, section/game restart, Completed gating, prompts, failure HUD, debug overlay independence, P0-P2 regressions, P3 full completion, drift/failure metrics, telemetry output, quit request, and missing references/components. The second Japanese correction adds real-Scene coverage for the Socket collider/range boundary, carried-Battery preservation, exact Pickup/Insert recording, powered-Door opening, lateral Door clearance, and the interactive start gate pausing/resuming Loop ticks. The Fresh4 correction adds generated-Scene assertions that the Section 1 tutorial shares the PressurePlate collider plus PlayMode coverage proving that the old near-edge position neither advances guidance nor advertises a valid recording, while a real Plate press advances guidance, opens the Door, and remains valid when replayed by an Echo.

## Playtest measurements

One local session stores build/version, Unity version, UTC start and duration, section start/completion durations, loops used, manual/timer ends, section/game restarts, interaction successes and failures by reason, maximum drift, final section, and Completed/Quit/ForcedTermination outcome. It stores no raw keys, ReplayFrame, or per-tick positions. JSON writes beneath `Application.persistentDataPath`; a failure logs once and never stops gameplay.

## Regression risks

- External pause must not alter Phase 0-2 default behavior.
- Loop completion events must not change request ordering or recording bounds.
- clearing a section must release Battery ownership before deactivation/destruction.
- multiple inactive section registries must not register Stable IDs.
- P3 Builder must not invoke older builders or rewrite P0-P2 Scenes/Prefabs.
- Input additions must preserve E/R and Gamepad South/Start behavior.
- HUD/Camera code must remain outside per-tick simulation and warning-free in batchmode.

## Out of Phase 3

Enemy AI, stealth, combat, story dialogue, voice, final models/animation/shaders/audio, Steamworks, achievements, cloud/save slots/checkpoints/level select, settings and rebinding UI, the Unity Localization package and multilingual runtime switching, online features, procedural generation, Addressables, large Asset Store environments, a generic level editor, new Replay formats, and Timeline editing are excluded. The later Phase 3 Japanese pretest adds only a centralized serialized `ja-JP` presentation catalog and installed-OS-font selection.
