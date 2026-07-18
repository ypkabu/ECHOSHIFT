# Phase 1 Validation

## Environment

- Validation date: 2026-07-19 (Asia/Tokyo)
- Unity Editor: `6000.4.6f1` (`0b051c2e5d54`)
- Universal Render Pipeline: `17.4.0`
- Input System: `1.19.0`
- Unity Test Framework: `1.6.0`
- Target: Windows x86_64 Development Build
- Branch: `feature/phase1-recorded-interaction`
- Phase 0 baseline: `767116cfd9f2c6805dab271c368ee0a23ff25263`, lightweight tag `phase0-validated`

## Scene Builder and authoring validation

- Command-line `P1SceneBuilder.BuildFromCommandLine`: completed with Unity return code `0`.
- Generated Scene: `Assets/_Project/Scenes/P1_InteractionLab.unity`.
- Build Settings order: `P1_InteractionLab`, then retained `P0_ReplayLab`.
- Repeated generation retained one `Battery.mat`, one `PowerSocket.mat`, one `P1_Echo.prefab`, and one Scene.
- Generated Scene contains exactly one authored Battery Stable ID and one authored PowerSocket Stable ID; they are non-empty and distinct.
- Editor validation tests reject empty and duplicate IDs.
- No matched C# compiler error/warning, Stable ID registration failure, or Scene Builder exception occurred in the final Builder log.

Scene YAML object identifiers can be reserialized by a Builder run, so byte-for-byte Scene hashes are not the idempotence contract. The verified contract is bounded generated assets, fixed Stable ID values, valid references, and reproducible Build Settings.

## Automated tests

### EditMode

- Result: 23 passed, 0 failed, 0 skipped.
- Unity process exit code: `0`.
- Duration reported by final post-build XML: `0.0799247 s`.
- Includes all 12 Phase 0 tests plus 11 Phase 1 cases.
- Final artifacts: ignored `TestResults/phase1-edit-postbuild.xml` and `Logs/phase1-edit-postbuild.log`.

### PlayMode and integrations

- Result: 26 passed, 0 failed, 0 skipped.
- Unity process exit code: `0`.
- Duration reported by final post-build XML: `7.2252397 s`.
- Includes all 12 Phase 0 tests plus 14 Phase 1 cases.
- Actual `P0_ReplayLab` integration: passed; maximum replay drift `0 m`.
- Actual `P1_InteractionLab` integration: passed.
  - Scene loaded and no Missing Component was found.
  - LoopDirector, Player, Battery, PowerSocket, Door, GoalVolume, InteractionRegistry, ResetRegistry, and Phase 1 overlay references validated.
  - Loop 1 recorded Battery pickup and PowerSocket insertion as two successful events.
  - Reset restored unheld Battery, unpowered Socket, and closed Door state.
  - Echo executed the two exact recorded Stable IDs.
  - Echo interaction successes: `2`.
  - Echo interaction failures: `0`.
  - Powered Door opened and the current Player reached Goal.
  - Maximum replay drift: `0 m`, within the unchanged `0.05 m` tolerance.
  - No unexpected log was received by the integration test.
- Final artifacts: ignored `TestResults/phase1-play-postbuild.xml` and `Logs/phase1-play-postbuild.log`.

## Standalone Build

- Pipeline: `EchoShift.Editor.Phase1BuildPipeline.BuildWindowsDevelopment`.
- Unity batchmode termination: return code `0` in the final log.
- BuildReport: `Succeeded`.
- Build warnings: `0`.
- Total reported build size: `166,082,466 bytes`.
- Output executable: `Builds/Phase1/ECHOSHIFT_Phase1.exe` (`667,648 bytes`).
- Required output found: `ECHOSHIFT_Phase1_Data/`, `UnityPlayer.dll`, `MonoBleedingEdge/`, and crash handler/runtime DLLs.
- Startup Scene: Phase 1; Phase 0 retained as the second Build Settings Scene.
- Final build log contains no matched C# compiler error/warning, Missing Script, Missing Reference, null-reference exception, or unhandled exception.

## Standalone startup log

- Started `ECHOSHIFT_Phase1.exe -batchmode -nographics` from the generated output.
- The second smoke run remained alive for the 20-second audit window and was then forcibly stopped by the audit because the game has no automated quit path.
- Log reached Unity `6000.4.6f1` initialization, Input System initialization, Mono domain reload, PhysX selection, Null Graphics initialization, and initial Scene load/unload timing.
- Full-log matched counts: Exception `0`, Error `0`, Missing Script `0`, Missing Reference `0`, NullReference `0`, Failed `0`.
- Final artifact: ignored `Logs/phase1-player-smoke-20s.log`.

## Problems found and corrected during validation

- Input migration initially lacked the Editor assembly's `Unity.InputSystem` reference; the assembly definition was corrected.
- Initial interaction compilation had an unassigned candidate failure reason; candidate evaluation was initialized explicitly.
- Disabling a replay target could leave lifecycle registration inconsistent; Battery/Socket now explicitly unregister and re-register through StableId, while Registry resolve prunes unavailable targets.
- Parenting a held Battery under an Actor allowed actor hierarchy destruction to delete the Battery before cleanup. Carry now copies the shared socket pose while the Battery remains outside the Actor hierarchy.
- The first EditMode batch invocation exited after assembly reimport without producing XML. Tests were rerun only after XML existed; that invocation was not counted as a pass.
- The Scene Builder's initial settings validation needed a definitely assigned error local; this was fixed before Scene generation validation.

## Artifact retention decision

`Library/`, `Builds/`, `Logs/`, and `TestResults/` remain ignored and are not committed. Build products and Library are large machine outputs. XML/logs contain machine-specific absolute paths and timestamps and are reproducible using `Docs/TestPlan.md`; this document records the stable result summary instead.

## Remaining constraints and manual checks

- Automated headless testing does not verify rendered framing, transparency/material appearance, overlay legibility, Gizmo readability, or visual polish.
- Physical keyboard and gamepad device feel, including South Button and Start bindings on real hardware, requires manual Play Mode or Standalone testing.
- Standalone was forcibly stopped after the smoke window, so graceful user-driven application exit was not tested.
- Audio, VFX, accessibility, installer/signing, Steam packaging, platform-specific controller prompts, and performance on target consumer hardware remain outside Phase 1.
- Interaction data is in-memory and Scene-local; cross-scene/save migration is not implemented.

## Gate decision

All automated Phase 1 completion gates are satisfied: Phase 0 regression tests pass, Phase 1 tests and both generated-Scene integrations pass, exact-ID interaction playback succeeds without fallback, reset state is consistent, drift is within tolerance, and the Windows Development Build and headless startup audit succeed. Phase 2 may begin after the listed visual and physical-input checks are accepted as manual product checks rather than automated blockers.
