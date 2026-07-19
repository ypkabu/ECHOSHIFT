# Phase 3 Japanese Localization Validation

This validation is separate from `Phase3Validation.md` and from formal human acceptance. It verifies the Japanese presentation build and records the Codex pretest without creating `phase3-validated`.

## Environment and baseline

- Date: 2026-07-19 (Asia/Tokyo)
- Unity Editor: `6000.4.6f1`
- URP: `17.4.0`
- Input System: `1.19.0`
- Unity Test Framework: `1.6.0`
- Platform: Windows x86_64 Development Build
- Branch: `feature/phase3-playable-greybox`
- Starting commit and unchanged `phase3-automation-passed`: `ff28b97bc5679eadaa239df9553a3021077cd511`
- Starting worktree: clean
- `phase3-validated`: absent

## Japanese presentation

`Phase3TextCatalog` is the single serialized source for `ja-JP` player text: Section names/objectives, tutorials, keyboard/gamepad prompts, loop/Echo/state labels, Battery state, transition/completion messages, Pause/restart/quit labels and confirmations, world markers, and every `InteractionFailureReason`. Internal enums and telemetry schema were not changed. Validation covers empty values, duplicate stable keys, and legacy English player text without rebuilding runtime dictionaries.

Section 3 initially shows only `2体のエコーと協力して出口へ進む`; solution-like tutorial steps are catalogued for future staged hints but are not displayed automatically in Phase 3.

## Font and layout

`JapaneseFontApplier` resolves an installed OS font from an allow-list and validates all required catalog glyphs. No font was downloaded or redistributed. The final machine selected `Noto Sans JP`; startup reported `glyphs=True`. HUD and Pause canvases use 1920x1080 reference scaling, wrapping, expanded labels, and best-fit Pause buttons. Automated Scene inspection found no old visible English player text. The retained visual run was 1280x720 (16:9); an actual non-16:9 render was not measured, so aspect-ratio visual acceptance remains a manual item even though responsive-layout properties are covered automatically.

## Scene Builder

`EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine` completed with exit code 0 after the final High fixes. Repeated generation retained the Japanese catalog/font references, generated one Input System EventSystem for Pause, authored all three P3 Debug Overlays OFF, kept P0-P2 Scene bytes unchanged in the regression suite, and preserved P3/P2/P1/P0 Build Settings order.

## Automated tests

Final EditMode XML: **57 passed, 0 failed, 0 skipped**, duration `1.3506917` seconds. Seven Japanese-localization cases cover all requested EditMode categories plus responsive layout, serialized failure-text replacement, and generated EventSystem checks.

Final PlayMode XML: **49 passed, 0 failed, 0 skipped**, duration `13.1806739` seconds. Ten Japanese/localization and black-box regression cases cover the eight requested PlayMode categories, default Debug Overlay OFF, movement-prompt visibility, and pointer/keyboard Pause Menu operation. Existing P0-P3 tests were retained without disabled cases or loosened tolerances.

The actual P3 integration still completed all three sections in **1,081** advances with maximum Replay Drift **0 m**, interaction success **4**, and interaction failure **0**.

Raw XML and logs remain ignored because they contain timestamps and machine paths and are reproducible from `TestPlan.md`; the measured result is committed here.

## Windows Development Build

`EchoShift.Editor.Phase3BuildPipeline.BuildJapaneseWindowsDevelopment` completed with process exit code 0 and marker:

`PHASE3_JA_BUILD_OK path=.../Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe;size=166307570;warnings=0`

The output contains `ECHOSHIFT_Phase3_JA.exe`, `ECHOSHIFT_Phase3_JA_Data`, and 290 required files. Compiler warning count was 0; log matches for compiler errors, Missing Font/Glyph, Missing Script/Reference, NullReference, and unhandled exceptions were 0.

## Standalone probe and telemetry

The corrected executable launched with explicit `-phase3AutoQuit`, initialized P3 in Playing/Section 1, and exited naturally with code 0. Marker:

`PHASE3_PROBE_OK state=Playing;section=1;hud=True;language=ja-JP;font=Noto Sans JP;glyphs=True;...`

It then logged `PHASE3_QUIT_REQUESTED` with a newly generated schema-1 telemetry JSON path. The synchronized `Playtest/Phase3-JA-BlackBox/Runtime` copy repeated the same probe marker and natural exit from its handoff location. Matched runtime exception/font/glyph/reference/assertion problems were 0. Ordinary launches do not auto-quit.

## Playtest package

`Playtest/Phase3-JA-BlackBox/` contains the 290-file corrected runtime, a source-free launch note, a blank result template, retained cropped screenshots, and ignored runtime logs. Source and package relative file sets match; all file sizes match, and the source/package `globalgamemanagers` SHA-256 is `A323A1370C26DF1904900E8377E6A6FB203B8FA3D86871F7ED8D11558E3CE353`. The two excluded invalid-attempt full-desktop PNGs were deleted and are not part of the handoff package.

## Codex black-box pretest

The independent source-blind test was executable, but its counted run ended **PRETEST FAIL**: 15 minutes, Section 1 Loop 29, Section 1 incomplete, Section 2/3 not reached, Restart 0, and Quit failed. It did not use source, telemetry, Debug Overlay, F3, coordinates, or an automated solution. Details and screenshots are in `Phase3CodexBlackBoxPlaytest.md`.

Three High defects were found across the excluded preflight and counted test: Debug Overlay persisted ON, Pause had no EventSystem, and the initial movement prompt was hidden. All three received code fixes and regression coverage. The packaged-build High recheck then visibly confirmed the movement prompt, keyboard Resume, pointer Restart with Loop/Echo reset, and pointer Quit with process exit after 7.42 seconds and no force. The fresh-context recheck agent confirmed the prompt but was blocked from further GUI input by an approval timeout; the implementation agent performed only the remaining targeted UI actions. Medium/Low camera, Echo identification, package-viewer encoding, input-label, and time-format observations were intentionally not all changed automatically.

## Gate decision

Japanese localization automation and the corrected build pass their technical gates. The Codex source-blind completion pretest remains **PRETEST FAIL**, so it supplies risk evidence rather than acceptance. All 17 items in `Phase3ManualAcceptance.md` remain unchecked. `phase3-validated` must remain absent, and Phase 4 must not begin until formal human acceptance is recorded and remaining priorities are adjudicated.
