# Phase 3 Automation Validation

**Phase 3: Validated**

## Validation environment

- Final regression date: 2026-07-21 (Asia/Tokyo)
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

Final EditMode XML: **61 passed, 0 failed, 0 skipped**, duration **1.8848759 seconds**. The retained suite covers state transitions, paused clocks, restart/reset scope, tutorial/input prompts, telemetry immutability/schema/payload boundaries, Japanese text/layout, interaction-boundary fixes, text completeness, and repeatable Scene generation.

Final PlayMode XML: **61 passed, 0 failed, 0 skipped**, duration **16.4244913 seconds**. The suite loads actual P0-P3 Scenes, completes the P3 route, verifies playing- and completed-state pause/resume/restart/quit and telemetry behavior, rejects replay creation after Completed, and traverses the generated Scene for Missing Components. Final test logs contained zero compiler warnings, C# compiler errors, unhandled exceptions, and unexpected test logs.

## Automated three-section solution

The actual `P3_PlayableGreybox` Scene completed in 1,081 deterministic tick advances with rendered/fixed updates supplied for physics triggers and Door presentation:

1. Echo 1 holds Section 1 Plate while Player reaches its Goal.
2. Echo replays Battery pickup and exact Stable-ID Socket insertion in Section 2.
3. Two Echoes coordinate Plate/Gate A and Battery/Socket/Gate B in Section 3 while current Player reaches the Goal.

Measured P3 integration values: maximum Replay Drift `0 m`, interaction success `4`, interaction failure `0`, maximum simultaneous Echoes at least two. Outgoing section Echo counts were zero and every section root was inactive after completion.

## Telemetry

Schema version 1 JSON was generated under `Application.persistentDataPath/EchoShiftPlaytests`. Tests verified aggregate counts, restart/end-reason tracking, array-copy snapshot immutability, deterministic schema, history consistency, and absence of replay-frame payloads. The final Standalone probe wrote a new 745-byte Quit session with Unity/build version, 0.0885 seconds duration, final section 1, zero interactions, maximum drift 0, and outcome `Quit`.

Raw JSON is not tracked because it contains timestamps and machine-local paths and is reproducible. Telemetry has no network transport.

## Windows Development Build

`EchoShift.Editor.Phase3BuildPipeline.BuildFinalWindowsDevelopment` succeeded with process exit code 0. BuildReport result was Succeeded, warnings 0, and reported size 166,334,282 bytes. The 289-file output contains `Builds/Phase3-Final/ECHOSHIFT_Phase3_Final.exe`, `ECHOSHIFT_Phase3_Final_Data`, and required runtime dependencies. EXE SHA-256 is `098A43C3B20762E4BDF938771C36F0FB116126AEC8932B2A77EB403F0CB77938`. P3 is the startup Scene followed by P2, P1, and P0.

## Standalone natural-exit probe

The final executable launched with `-batchmode -nographics -phase3AutoQuit`. It initialized Unity, Input System, PhysX, P3 state `Playing`, section 1, Japanese HUD, `Noto Sans JP`, required glyphs, and telemetry. It logged `PHASE3_PROBE_OK`, saved a new JSON session through the normal application quit request, and exited naturally with code 0. Matches for Error, Exception, Missing Script, Missing Reference, NullReference, and assertion failure were all 0. Ordinary launches do not auto-quit because the flag is explicit.

The separate source-blind Fresh7 graphical run used the corrected Japanese Build at 1280x720, completed Sections 1-3 in 2:08 / 3:33 / 4:24 using 2 / 3 / 3 loops, opened Pause after completion, and quit through the two-click visible menu path. It found no Critical or High issue. See `Phase3CodexBlackBoxRetest.md`.

## Artifact policy

`Builds/`, `Logs/`, `TestResults/`, and Unity `Library/` remain ignored. Logs/XML/JSON contain timestamps and machine paths; committed documentation records measured results and reproducible commands. Source, generated Unity assets/Scenes, settings, and docs are tracked.

## Remaining constraints and gate decision

All final technical gates pass. The Codex Fresh7 run records one Medium delayed-replay clarity issue and two Low presentation issues; these are separated as Phase 4-or-later backlog and did not block completion.

Human acceptance passed full completion and every recorded gameplay, control, comprehension, visibility, Japanese-display, Restart, Pause/Resume, Quit, and Interaction-feedback check. Unrecorded Build, device, resolution, timings, Loop counts, and Restart count remain explicitly unrecorded. Human severity is Critical 0, High 0, Medium 1, Low 0. The Medium observation is visual simplicity appropriate to the greybox milestone and is deferred to `Phase4Backlog.md`. Phase 3 is formally validated and may advance to separately planned Phase 4 work.
