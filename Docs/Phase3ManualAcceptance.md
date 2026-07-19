# Phase 3 Manual Acceptance

Phase 3 automation is complete, but formal validation is not. Every item below is intentionally unchecked and has no inferred result. A human tester must run the normal graphical build without `-phase3AutoQuit`, record Pass or Fail, add a comment, and assign `Critical`, `High`, `Medium`, or `Low` when improvement is needed.

Build under test: `Builds/Phase3/ECHOSHIFT_Phase3.exe`

| Done | Required check | Pass / Fail | Comment | Improvement priority |
|---|---|---|---|---|
| [ ] | WASD movement feels natural | Unchecked |  |  |
| [ ] | Physical keyboard E, R, and Escape work | Unchecked |  |  |
| [ ] | Camera does not cause discomfort | Unchecked |  |  |
| [ ] | Section 1 solution is understandable without explanation | Unchecked |  |  |
| [ ] | Battery operation is understandable in Section 2 | Unchecked |  |  |
| [ ] | Both Echo roles can be followed in Section 3 | Unchecked |  |  |
| [ ] | Echo 1 and Echo 2 are distinguishable | Unchecked |  |  |
| [ ] | Plate-to-Door relationship is clear | Unchecked |  |  |
| [ ] | Socket-to-Door relationship is clear | Unchecked |  |  |
| [ ] | Closed and open Door states are distinguishable | Unchecked |  |  |
| [ ] | Loop Transition meaning is understandable | Unchecked |  |  |
| [ ] | HUD does not cover the main route | Unchecked |  |  |
| [ ] | Restart Section behaves as expected | Unchecked |  |  |
| [ ] | Pause and Resume work | Unchecked |  |  |
| [ ] | Quit exits normally | Unchecked |  |  |
| [ ] | Full game can be completed in about 5-10 minutes | Unchecked |  |  |
| [ ] | Interaction failure reason is understandable | Unchecked |  |  |

Formal completion requires all required checks to be recorded, every Critical/High issue fixed with appropriate regression tests, and the corrected build reverified. Only then may a separate explicitly authorized final commit and `phase3-validated` tag be created.
