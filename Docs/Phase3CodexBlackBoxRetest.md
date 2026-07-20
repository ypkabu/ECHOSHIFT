# Phase 3 Japanese Codex Black-Box Retest

**Phase 3: Validated** after a separate human acceptance record. This document remains the independent Codex pretest evidence only.

## Result

`PRETEST PASS`

The final Fresh7 run completed all three sections within the 15-minute limit, completed Section 1 within three minutes, found no Critical or High issue, and exited through the in-game Pause Menu. This is source-blind Codex pretest evidence; it does not replace human acceptance.

## Build and environment

- Test date: 2026-07-21 (Asia/Tokyo)
- Build: source-free copy of `Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe`
- Source commit represented by the Build: `5ce00ddacc18fdc89daa9ea12af9339566588efb`
- Window: 1280x720, windowed Windows Standalone Development Player
- Input: ordinary vision-guided WASD, E, R, Escape, and mouse input
- Debug Overlay: OFF throughout; F3 was never pressed
- Continuous recording: H.264, 1280x720, 653.2 seconds
- Evidence: ignored runtime folder `Playtest/Phase3-JA-BlackBox/Logs/Retest-20260721-Fresh7-Pass`
- Evidence inventory: 38 files, 18,346,533 bytes; 34 screenshots and one continuous video
- Video SHA-256: `444B3731DDD3023A9720959ECE3D06E9871973A088D7816391253E928BAB569B`

The Fresh7 execution context was newly created and was not reused from the prior black-box attempts. The test executor received only the Standalone Build, the development-build notice, the instruction to use visible on-screen information, the 15-minute limit, and the request for post-run observations. It did not open the Unity Project, source, Scenes, Prefabs, design documents, prior results, telemetry, or Player logs; it did not receive coordinates, Stable IDs, solutions, automation routes, or correction details. It did not use Debug Overlay, F3, fixed solution input, state modification, or coordinate extraction.

## Timing and attempts

| Event | Time (JST) | Elapsed from first visible |
|---|---:|---:|
| Game first visible | 04:22:20 | 0:00 |
| Player first moved | 04:22:43 | 0:23 |
| First Plate | 04:22:56 | 0:36 |
| First loop ended | 04:23:07 | 0:47 |
| Echo mechanism understood | 04:23:21 | 1:01 |
| Section 1 complete | 04:24:28 | 2:08 |
| First Battery | 04:24:56 | 2:36 |
| Battery held | 04:25:10 | 2:50 |
| Socket relationship understood | 04:25:22 | 3:02 |
| Section 2 complete | 04:28:01 | 5:41 |
| Two-Echo roles understood | 04:30:49 | 8:29 |
| Section 3 complete | 04:32:25 | 10:05 |
| Pause Menu opened | 04:32:39 | 10:19 |
| Quit completed / video end | approximately 04:33:14 | 10:53 |

| Section | Section time | Loops | Restarts | Result |
|---|---:|---:|---:|---|
| 1: 過去の自分 | 2:08 | 2 | 0 | Complete |
| 2: 記録された操作 | 3:33 | 3 | 0 | Complete |
| 3: 過去との協力 | 4:24 | 3 | 0 | Complete |

Total recorded duration was 10:53.2. Restart Section and Restart from Beginning were visible and understood, but neither was activated because the run remained recoverable. Pause opened after completion. Quit showed its second-click confirmation and the second click closed the Player normally.

## Player observation record

The first noticed elements were the blue floor switch, red Door, yellow current Player, green exit, Japanese objective, and the right-side WASD/R/Escape guide. The Door changed from red to green when the Player stood on the Plate. The on-screen instruction then established where to end the loop. In Loop 2, the cyan trail and E1 label made the replay visible, and the tester described E1 as repeating earlier movement while the current Player waited at the Door. The eventual Door opening confirmed that model. The stopped/replaying Echo remained distinguishable from the current Player.

In Section 2, the orange Battery, purple Socket, proximity prompts, carried-state HUD, and insertion prompt established the pickup-to-Socket relationship. During Loop 2 the tester mistook delayed replay progress for a failed recorded interaction and spent an additional loop. The section still completed without external help.

In Section 3, the tester independently constructed the combined route: E1 as Plate holder, E2 as Battery/Socket actor, and the current Player as the final traverser. Cyan E1, magenta E2, yellow Self, labels, and trails were sufficient to maintain those roles. The camera exposed the multi-room structure; perspective changes made exact approach distance occasionally harder to judge but did not hide a required target. HUD did not block progression.

Japanese objectives and prompts were understandable and actionable. Door red/green feedback was immediate. No truncation, mojibake, unreadable required text, or progression-blocking Japanese was observed.

## Issues

### Medium: delayed Echo replay can resemble a failed interaction

- Observed fact: in Section 2 Loop 2, the Door remained closed and the Battery looked unmoved while E1 appeared near the Door. The tester concluded that the recorded Battery interaction had failed and created another Echo. The delayed route later completed successfully.
- Inferred cause: the presentation communicates the recorded path but not clearly enough which phase of a longer replay is currently executing.
- Proposed improvement: add a small replay-progress cue or stronger `E1 is replaying` state without exposing the puzzle solution.

### Low: close world labels can overlap

- Observed fact: Self, Battery, and Socket labels became visually noisy when the objects clustered. Interaction prompts remained readable.
- Inferred cause: independent world-space labels do not resolve screen-space overlap.
- Proposed improvement: add small label offsets, priority, or collision avoidance for close objects.

### Low: faint duplicate Pause title

- Observed fact: a faint second `一時停止` appeared behind the Pause button stack. Buttons and Quit confirmation remained usable.
- Inferred cause: the HUD paused-state label and Pause panel title are both visible through the overlay.
- Proposed improvement: hide or dim the underlying HUD state label while the Pause panel is active.

These Medium/Low items are non-blocking Phase 4-or-later backlog candidates. No Critical or High issue remained in the Fresh7 run.

## Retest audit trail

Fresh4 exposed a High Section 1 tutorial/Plate boundary mismatch. Fresh6 completed all sections but exposed a High inability to open Pause after completion and therefore could not Quit. Each observation was recorded before source inspection; the implementation-side agent then corrected the issue and added regression coverage. Fresh5 was not used for a pass decision because Windows focus/security interference made its input run unreliable. Fresh7 used a new source-blind context after the completion-Pause correction and is the sole final pass result.

## Final decision

`PRETEST PASS`

- Full completion: yes, 10:53.2
- Section 1 under three minutes: yes, 2:08
- Critical / High: 0 / 0
- Pause: success
- Quit: success through the visible two-step Pause Menu path
- Source-blind constraint: maintained
- Japanese progression blocker: none observed
