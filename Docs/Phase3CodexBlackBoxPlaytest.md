# Phase 3 Codex Black-Box Playtest

This is a source-blind Codex pretest, not formal human acceptance. Its result does not authorize `phase3-validated` or Phase 4.

## Test Environment

- Build: `Playtest/Phase3-JA-BlackBox/Runtime/ECHOSHIFT_Phase3_JA.exe`
- Unity Version: `6000.4.6f1`; this was supplied by the build owner and was not discoverable from the black-box screen
- Date and time: 2026-07-19 18:14-18:29 JST
- Input device: keyboard and mouse
- Resolution: 1280x720 client area, windowed, Windows display scaling 150%
- GUI operation method: visible-window recognition plus ordinary Windows mouse input and physical-key-equivalent scan-code input
- Source-blind constraint: maintained. The independent fresh-context agent did not open the Unity Project, C# source, Scene YAML, Prefabs, design documents, automated solutions, telemetry, existing logs, Stable IDs, coordinates, or Debug Overlay, and did not press F3
- Timing: the 15-minute limit began after the game was visible and foreground. Initial window focus was an operation-environment event and is separated from game observations

## Result

- Verdict: **PRETEST FAIL**
- Completion: not completed; Section 1 remained active at the time limit
- Total time: 15:00
- Section 1: 15:00, final visible value Loop 29, not completed
- Section 2: not reached
- Section 3: not reached
- Restart Section: 0 successful; the menu action did not respond in the tested build
- Visible unsuccessful Door attempts: 3
- Pause: Escape opened and closed the menu, but its buttons did not respond
- Quit: failed in the tested build. After the independent record was complete, the implementation agent verified the exact package process and force-terminated only that process so the corrected build could replace it

The original source-blind result remains `PRETEST FAIL` after corrective work because only the observed High issue locations are rechecked; a limited recheck is not a second full completion attempt and cannot convert an unfinished playthrough into `PRETEST PASS`.

## Attempt validity and corrective sequence

An earlier launch was stopped and excluded before the counted test because the technical Debug Overlay was visible by default. Screenshot: [`BB-001_debug-overlay-default-on.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/BB-001_debug-overlay-default-on.png). Investigation found that overlay visibility was held only in a non-serialized field, so the Scene Builder's OFF state did not survive serialization. The state is now serialized, P3 explicitly writes OFF, and a PlayMode regression asserts the generated P3 Scene starts with it hidden.

The counted source-blind attempt then ran with Debug Overlay OFF. It found two further High issues. Investigation after the independent result found that the generated Scene had no EventSystem/Input System UI module, and that the HUD deliberately returned an empty prompt when no interaction target was nearby. The corrected build adds a generated EventSystem with `InputSystemUIInputModule`, selects Resume whenever Pause opens, and shows the device-appropriate movement prompt until an interaction target is available.

High-issue-only GUI recheck: **PASS for BBJA-001 and BBJA-002**. A fresh-context agent confirmed the visible `WASD：移動` prompt, then its GUI-input permission review stalled beyond the five-minute cap. The implementation agent completed only the targeted packaged-build UI check: Escape plus Enter resumed, two pointer clicks confirmed and executed Restart Section, and two pointer clicks confirmed Quit; the Standalone then exited without force after 7.42 seconds. This limited recheck does not change the original completion verdict. Evidence: [`HighIssueRetest.md`](../Playtest/Phase3-JA-BlackBox/HighIssueRetest.md).

## Observations

- Understood: the objective associated the blue Plate with the Door. At Loop 23, the Door changed from red to green while an actor occupied the Plate, which communicated the causal relationship without source information.
- Confusing: movement controls were absent in the tested build; the timer continued while input was being discovered; Echoes reached 3/3 early; actor colors did not explain which recording was replaying; camera rotation changed orientation; Pause buttons did not respond.
- Missed display: movement binding, an explicit Player marker/legend, Echo generation-to-color mapping, and the currently replaying recording number.
- Japanese: no clipped in-game Japanese was observed. `R / START` and `ESC / SELECT` mix keyboard and gamepad terminology; the time display used a culture-dependent decimal comma and no `秒` unit.
- Camera / HUD / Echo identification: foreground walls and the lower screen edge hid actors at times. Bottom-center prompts could overlap actors. Color alone was insufficient to track three Echoes reliably.
- Difficulty: too high for first-time onboarding in this run; the tester did not establish a repeatable Plate-to-Echo plan within 15 minutes.
- Tempo: explanation and input discovery competed with an already-running short loop.
- Package text: `起動説明.md` is strict-valid UTF-8 without BOM and reads correctly when UTF-8 is selected. The black-box agent's viewer treated it as a legacy code page and displayed mojibake; this remains an environment-compatibility finding rather than proven text corruption.

## Issues

### BB-001 — Debug Overlay visible on first launch

- Severity: **High**
- Section: global presentation
- Reproduction: launch the first Japanese package build normally
- Expected: technical Debug Overlay is OFF for black-box play
- Actual: English technical data covered the normal presentation
- Screenshot: [`BB-001_debug-overlay-default-on.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/BB-001_debug-overlay-default-on.png)
- Proposed fix: serialize overlay visibility and have P3 Scene Builder author OFF explicitly
- Confidence: High
- Status: fixed; full automated suites, rebuild, and startup regression passed

### BBJA-001 — Pause actions did not respond

- Severity: **High**
- Section: Pause Menu
- Reproduction: foreground the game, press Escape, then use normal pointer click or keyboard navigation/submit on Restart Section or Quit
- Expected: two-step confirmation appears, Restart resets Section 1, and Quit exits normally
- Actual: Escape toggled Pause, but button input produced no confirmation or action; Quit failed
- Screenshot: [`20_Menu_EConfirm.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/20_Menu_EConfirm.png)
- Proposed fix: create one EventSystem with `InputSystemUIInputModule`, set a default selected button, and cover pointer/submit paths in PlayMode
- Confidence: High after source inspection found the generated Scene had no EventSystem
- Status: fixed; automated regression and packaged-build keyboard/pointer recheck passed

### BBJA-002 — Movement control absent from the initial HUD

- Severity: **High**
- Section: Section 1 onboarding
- Reproduction: launch from a clean start and read only the visible HUD
- Expected: the minimum movement binding is visible in Japanese
- Actual: only Loop End and Pause bindings were visible; movement had to be guessed
- Screenshot: [`01_Section1_Start.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/01_Section1_Start.png)
- Proposed fix: show `WASD：移動` or `左スティック：移動` whenever no interaction target is near, replacing it with the interaction prompt when appropriate
- Confidence: High
- Status: fixed for the stated visibility expectation; automated regression and fresh-context visible-HUD recheck passed. Delaying the first timer remains a separate tempo proposal and was not added automatically

### BBJA-003 — Echo roles were hard to distinguish

- Severity: **Medium**
- Section: Section 1 / Echo identification
- Reproduction: allow multiple loops and observe three differently colored actors
- Expected: Player and each recording can be tracked and related to an Echo generation
- Actual: colors had no visible legend or generation label, and overlaps/camera movement made the source recording unclear
- Screenshot: [`18_DoorRunWithEcho.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/18_DoorRunWithEcho.png)
- Proposed fix: add compact E1-E3 identifiers tied to HUD colors and retain a distinct Player marker
- Confidence: Medium
- Status: not changed automatically; priority 1 among Medium findings

### BBJA-004 — Camera, wall, and bottom HUD obscured actors

- Severity: **Medium**
- Section: Section 1 presentation
- Reproduction: move between the spawn, Plate, and Door while returning toward the near wall
- Expected: actors and route remain trackable
- Actual: actors were partly hidden by the near wall or lower frame and competed with bottom-center prompts
- Screenshots: [`15_OnSwitchAttempt.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/15_OnSwitchAttempt.png), [`19_Section1_EndAttempt.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/19_Section1_EndAttempt.png)
- Proposed fix: human-evaluate a calmer Section 1 camera, near-wall transparency/height, and HUD safe area
- Confidence: Medium-High
- Status: not changed automatically; priority 2 among Medium findings

### BBJA-005 — UTF-8 launch note was misdetected by the agent's viewer

- Severity: **Low**
- Section: playtest package
- Reproduction: open the BOM-less UTF-8 `起動説明.md` in a legacy-code-page reader
- Expected: readable Japanese instructions
- Actual: the independent agent's reader displayed mojibake; strict UTF-8 validation later passed and explicit UTF-8 decoding was readable
- Screenshot: none; desktop content was intentionally not retained
- Proposed fix: decide whether the package policy should add a UTF-8 BOM after checking the human test environment
- Confidence: Medium; the file is valid and the failure is viewer-dependent
- Status: not changed automatically; priority 2 among Low findings

### BBJA-006 — Time formatting was not natural Japanese UI

- Severity: **Low**
- Section: runtime HUD
- Reproduction: observe the remaining-time label under a culture using comma decimals
- Expected: a stable form such as `残り時間 8.9秒`
- Actual: values such as `残り時間 08,9` appeared
- Screenshot: [`01_Section1_Start.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/01_Section1_Start.png)
- Proposed fix: use an explicit display culture/format and add the seconds unit
- Confidence: Medium
- Status: not changed automatically; priority 1 among Low findings

## Evidence

- Independent raw record: [`BlackBoxAgentResult.md`](../Playtest/Phase3-JA-BlackBox/BlackBoxAgentResult.md)
- Section 1 start: [`01_Section1_Start.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/01_Section1_Start.png)
- Plate/Door relationship understood: [`17_OnSwitchPossible.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/17_OnSwitchPossible.png)
- Pause failure: [`20_Menu_EConfirm.png`](../Playtest/Phase3-JA-BlackBox/Screenshots/20_Menu_EConfirm.png)
- High-fix recheck: [`HighIssueRetest.md`](../Playtest/Phase3-JA-BlackBox/HighIssueRetest.md)

No Section 2, Section 3, Battery-held, or game-completion screenshot exists because the tester did not leave Section 1. All retained black-box screenshots are cropped to the 1280x720 game client; full-desktop captures were not retained.
