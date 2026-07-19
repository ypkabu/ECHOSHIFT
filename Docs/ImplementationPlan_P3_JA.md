# Phase 3 Japanese Localization and Codex Pretest Plan

## Scope and gate

This work localizes only Phase 3 player-facing presentation and performs one source-blind Codex black-box pretest. It does not replace human acceptance, move `phase3-automation-passed`, create `phase3-validated`, merge to `main`, or begin Phase 4.

## Baseline

- Branch: `feature/phase3-playable-greybox`
- Starting commit and `phase3-automation-passed`: `ff28b97bc5679eadaa239df9553a3021077cd511`
- Starting worktree: clean
- `phase3-validated`: absent

## Implementation

1. Expand `Phase3TextCatalog` into the single Japanese source for section names, objectives, tutorial guidance, prompts, HUD labels, transitions, completion, pause/restart/quit confirmation, carried Battery state, Echo generation, and all interaction failures.
2. Keep enum and telemetry values unchanged. Use indexed fields/switches and static keys rather than rebuilding dictionaries during gameplay; expose validation APIs for empty text, duplicate keys, and legacy English fragments.
3. Separate persistent objective, contextual tutorial, transient state, and failure labels in the HUD. Widen/wrap Japanese UI and add two-step restart/quit confirmation in the pause menu.
4. Apply a dynamic Japanese OS font from an ordered allow-list (`Noto Sans JP`, `BIZ UDPGothic`, `Yu Gothic UI`, `Meiryo`) without downloading or redistributing a font. Validate every required catalog glyph at startup and in tests.
5. Extend the P3 builder so repeated generation preserves Japanese catalog/font references and creates no English player-facing text.
6. Add the requested EditMode and PlayMode localization coverage without deleting or weakening P0-P3 tests.
7. Build `Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe`, run the natural-exit probe, and verify warning/error/font/glyph/reference counts and telemetry output.
8. Assemble `Playtest/Phase3-JA-BlackBox/` with the Standalone build, source-free launch instructions, blank result template, and empty `Screenshots/` and `Logs/` directories.
9. Give only that package and allowed screen/input rules to a fresh-context agent. The implementation agent must not provide routes, source, Scene data, test inputs, coordinates, telemetry internals, or debug overlay information.
10. Record the pretest separately in `Docs/Phase3CodexBlackBoxPlaytest.md`. Automatically fix only observed Critical/High defects; report Medium/Low findings with priority.

## Verification and Git

- Rebuild P3 Scene, then run complete EditMode and PlayMode suites.
- Confirm P0-P2 Scene hashes remain unchanged and P3 has no Missing Component/reference.
- Confirm the JA executable and Data folder, natural exit code 0, `PHASE3_PROBE_OK`, telemetry JSON, and zero matched font/glyph/compiler/reference errors.
- Commit localization, tests, build documentation, and pretest record as `feat: localize phase 3 greybox to Japanese`.
- If the black-box pretest requires a Critical/High code correction, commit that correction separately.
- Leave `phase3-automation-passed` at the baseline commit and leave `phase3-validated` absent.

## Completion record

- Japanese Scene generation, 57 EditMode tests, 49 PlayMode tests, corrected Windows Development Build, natural-exit probe, font/glyph validation, telemetry JSON, and package synchronization passed.
- The independent source-blind run was executable but ended `PRETEST FAIL` after 15 minutes in Section 1. It found High defaults/input issues in Debug Overlay, Pause UI, and movement guidance; all received code fixes and regressions.
- Medium/Low observations were recorded without broad automatic redesign. Formal human acceptance remains entirely unchecked.
