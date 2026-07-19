# Phase 3 Automation Validation

## Validation environment

- Date: 2026-07-19 (Asia/Tokyo)
- Unity Editor: `6000.4.6f1` (`0b051c2e5d54`)
- URP: `17.4.0`
- Input System: `1.19.0`
- Test Framework: `1.6.0`
- Platform: Windows x86_64 Development Build
- Branch: `feature/phase3-playable-greybox`

## Phase 2 integration baseline

The Phase 2 tree was clean at `f88ff829fb15ef28bba72ae4a46b25c37aafc0ec`, matching `phase2-validated`. `main` was fast-forwarded from Phase 1 to that exact Phase 2 commit. Tags `phase0-validated`, `phase1-validated`, and `phase2-validated` remained unchanged. Phase 3 work then moved to its own feature branch; no remote exists and no push or merge back to `main` was performed.

## Scene Builder and static audit

`EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine` completed with exit code 0 after repeated generation. It generated one P3 Scene, three inactive/active section roots, P3 assets, and P3/P2/P1/P0 Build Settings order. EditMode hashing proved P0-P2 Scene bytes were unchanged across P3 builds. Generated Stable IDs were unique and stable across builds. Final Scene/prefab text contained zero `m_Script: {fileID: 0}` or zero-GUID script patterns; PlayMode recursively found no Missing Component.

## Automated tests

Final EditMode XML: 50 passed, 0 failed, 0 skipped, duration 1.0030866 seconds. Fifteen P3 additions cover state transitions, paused clocks, restart/reset scope, tutorial/input prompts, telemetry immutability/schema/payload boundaries, text completeness, and repeatable Scene generation.

Final PlayMode XML: 39 passed, 0 failed, 0 skipped, duration 12.720299 seconds. Seven grouped P3 additions cover at least 25 requested runtime assertions. They load actual P0-P3 Scenes, complete the P3 route, verify pause/resume/restart/quit and telemetry behavior, reject replay creation after Completed, and traverse the generated Scene for missing components. Final test logs contained no C# compiler error, unhandled exception, or unexpected test log.

## Automated three-section solution

The actual `P3_PlayableGreybox` Scene completed in 1,081 deterministic tick advances with rendered/fixed updates supplied for physics triggers and Door presentation:

1. Echo 1 holds Section 1 Plate while Player reaches its Goal.
2. Echo replays Battery pickup and exact Stable-ID Socket insertion in Section 2.
3. Two Echoes coordinate Plate/Gate A and Battery/Socket/Gate B in Section 3 while current Player reaches the Goal.

Measured P3 integration values: maximum Replay Drift `0 m`, interaction success `4`, interaction failure `0`, maximum simultaneous Echoes at least two. Outgoing section Echo counts were zero and every section root was inactive after completion.

## Telemetry

Schema version 1 JSON was generated under `Application.persistentDataPath/EchoShiftPlaytests`. Tests verified aggregate counts, restart/end-reason tracking, array-copy snapshot immutability, deterministic schema, history consistency, and absence of replay-frame payloads. The Standalone probe wrote a Quit session with Unity/build version, approximately 0.12 seconds duration, final section 1, zero interactions, and outcome `Quit`.

Raw JSON is not tracked because it contains timestamps and machine-local paths and is reproducible. Telemetry has no network transport.

## Windows Development Build

`EchoShift.Editor.Phase3BuildPipeline.BuildWindowsDevelopment` succeeded. BuildReport result was Succeeded, warnings 0, reported size 166,269,622 bytes. Output contains `Builds/Phase3/ECHOSHIFT_Phase3.exe`, `ECHOSHIFT_Phase3_Data`, and required runtime dependencies. P3 is startup Scene followed by P2, P1, and P0.

## Standalone natural-exit probe

The executable launched with `-batchmode -nographics -phase3AutoQuit`. It initialized Unity, Input System, PhysX, P3 state `Playing`, section 1, HUD, and telemetry. It logged `PHASE3_PROBE_OK`, saved a JSON session through the same user-facing quit path, and exited naturally with code 0. No matching Exception, Error, Missing Script, Missing Reference, NullReference, or assertion failure was present. Ordinary launches do not auto-quit because the flag is explicit.

## Artifact policy

`Builds/`, `Logs/`, `TestResults/`, and Unity `Library/` remain ignored. Logs/XML/JSON contain timestamps and machine paths; committed documentation records measured results and reproducible commands. Source, generated Unity assets/Scenes, settings, and docs are tracked.

## Remaining constraints and gate decision

Automation passed. Camera comfort, visible graphical quality, HUD occlusion, color/shape readability, physical keyboard/gamepad feel, tutorial comprehension, and a human-paced 5-10 minute completion were not observed and are not marked successful. All 17 items in `Phase3ManualAcceptance.md` remain unchecked.

The automation gate permits `phase3-automation-passed`. Phase 3 is not formally validated, `phase3-validated` must not exist yet, and Phase 4 must not begin until human acceptance is recorded and Critical/High findings are resolved and retested.
