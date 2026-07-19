# Phase 3 Japanese Localization Validation

This validation is separate from `Phase3Validation.md` and from formal human acceptance. It verifies the Japanese presentation build and records the Codex pretest without creating `phase3-validated`.

## Environment and baseline

- Date: 2026-07-20 (Asia/Tokyo)
- Unity Editor: `6000.4.6f1`
- URP: `17.4.0`
- Input System: `1.19.0`
- Unity Test Framework: `1.6.0`
- Platform: Windows x86_64 Development Build
- Branch: `feature/phase3-playable-greybox`
- High-correction baseline commit: `866a4646fd3501e39e1b05c918d424762de00b06`
- Starting worktree: clean
- `phase3-validated`: absent

## Japanese presentation

`Phase3TextCatalog` is the single serialized source for `ja-JP` player text: Section names/objectives, tutorials, keyboard/gamepad prompts, loop/Echo/state labels, Battery state, transition/completion messages, Pause/restart/quit labels and confirmations, world markers, and every `InteractionFailureReason`. Internal enums and telemetry schema were not changed. Validation covers empty values, duplicate stable keys, and legacy English player text without rebuilding runtime dictionaries.

Section 3 initially shows only `2体のエコーと協力して出口へ進む`; solution-like tutorial steps are catalogued for future staged hints but are not displayed automatically in Phase 3.

## Font and layout

`JapaneseFontApplier` resolves an installed OS font from an allow-list and validates all required catalog glyphs. No font was downloaded or redistributed. The final machine selected `Noto Sans JP`; startup reported `glyphs=True`. HUD and Pause canvases use 1920x1080 reference scaling, wrapping, expanded labels, and best-fit Pause buttons. The four guidance rows now occupy separate top-right safe-area rectangles, while generated world labels retain unit world scale under non-uniform gameplay primitives. Automated Scene inspection found no old visible English player text. The retained visual run was 1280x720 (16:9); an actual non-16:9 render was not measured, so aspect-ratio visual acceptance remains a manual item even though responsive-layout properties are covered automatically.

## Scene Builder

`EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine` completed with exit code 0 after the final High fixes. Repeated generation retained the Japanese catalog/font references, generated one Input System EventSystem for Pause, authored all three P3 Debug Overlays OFF, kept P0-P2 Scene bytes unchanged in the regression suite, and preserved P3/P2/P1/P0 Build Settings order.

## Automated tests

Final EditMode XML: **61 passed, 0 failed, 0 skipped**, duration `1.9120047` seconds. Japanese-localization and High-correction cases cover responsive layout, non-overlapping HUD safe-area rows, unit-scale world labels, serialized failure-text replacement, generated EventSystem checks, authored world-label framing, and Telemetry failure de-duplication.

Final PlayMode XML: **59 passed, 0 failed, 0 skipped**, duration `15.2049905` seconds. Japanese/localization and black-box regression cases cover default Debug Overlay OFF, an interactive start gate that freezes the first Loop, real Input System E/R and gamepad action-latch consumption, pointer/keyboard Pause Menu operation, staged guidance, the real-Scene Socket collider/range boundary, invalid-candidate Battery preservation, exact Pickup/Insert recording without a false Drop, Echo-only P3 Door power, lateral Door opening, fixed Camera rotation, and Player/Echo identity. Existing P0-P3 tests were retained without disabled cases or loosened tolerances.

The actual P3 integration still completed all three sections in **1,081** advances with maximum Replay Drift **0 m**, interaction success **4**, and interaction failure **0**.

Raw XML and logs remain ignored because they contain timestamps and machine paths and are reproducible from `TestPlan.md`; the measured result is committed here.

## Windows Development Build

`EchoShift.Editor.Phase3BuildPipeline.BuildJapaneseWindowsDevelopment` completed with process exit code 0 and marker:

`PHASE3_JA_BUILD_OK path=.../Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe;size=166333594;warnings=0`

The output contains `ECHOSHIFT_Phase3_JA.exe`, `ECHOSHIFT_Phase3_JA_Data`, and 290 required files totalling 166,528,726 bytes. Compiler warning count was 0; log matches for compiler errors, Missing Font/Glyph, Missing Script/Reference, NullReference, and unhandled exceptions were 0.

## Standalone probe and telemetry

The corrected executable launched with explicit `-phase3AutoQuit`, bypassed the interactive start gate as designed, initialized P3 in Playing/Section 1, and exited naturally with code 0. Marker:

`PHASE3_PROBE_OK state=Playing;section=1;hud=True;language=ja-JP;font=Noto Sans JP;glyphs=True;...`

It then logged `PHASE3_QUIT_REQUESTED` with a newly generated schema-1 telemetry JSON path. The synchronized `Playtest/Phase3-JA-BlackBox/Runtime` copy repeated the same probe marker and natural exit from its handoff location. Matched runtime exception/font/glyph/reference/assertion problems were 0. Ordinary launches do not auto-quit.

## Playtest package

`Playtest/Phase3-JA-BlackBox/` contains the 290-file corrected runtime, a source-free launch note, a blank result template, retained cropped screenshots, and ignored runtime logs. Source and package relative file sets, sizes, and SHA-256 hashes match exactly. The source/package `globalgamemanagers` SHA-256 is `DB1B7CD28DD820158144CC74067D717A65DF5259FA81DB6ABB0483661DF48155`; the EXE SHA-256 is `098A43C3B20762E4BDF938771C36F0FB116126AEC8932B2A77EB403F0CB77938`. The two excluded invalid-attempt full-desktop PNGs were deleted and are not part of the handoff package.

## Codex black-box pretest

The original independent source-blind test remains documented in `Phase3CodexBlackBoxPlaytest.md`. A later clean-context Fresh3 run completed Section 1 but exposed a real Socket boundary defect: the HUD advertised Insert while the request fell back to Drop, preventing Section 2 progress. Its evidence was finalized before source inspection. This second correction makes candidate display/execution agree, makes Recorded Interaction mechanically required in P3, clears the opened Door from the framed opening, and prevents launch-time focus interruptions from consuming the first Loop. A fully new source-blind agent must retest the rebuilt package; that result belongs in `Phase3CodexBlackBoxRetest.md` and is not inferred from automation or the implementation-side GUI smoke.

## Gate decision

Japanese localization automation and the corrected build pass their technical gates. The source-blind completion gate remains pending a fully new run, so no automated result is treated as acceptance. All 17 items in `Phase3ManualAcceptance.md` remain unchecked. `phase3-validated` must remain absent, and Phase 4 must not begin until formal human acceptance is recorded and remaining priorities are adjudicated.
