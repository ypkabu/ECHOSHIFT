# Phase 4.3 Character and Device Presentation Validation

**Status: Automation Passed; Human Visual Review pending**

## Scope

Phase 4.3 changes only Robot presentation, Battery size/carry presentation,
floor circuit presentation, and Door opening presentation. Phase 4.2 Camera,
HUD, Pause Menu, Actor identity rules, puzzle layout, gameplay coordinates,
fixed-tick simulation, Replay, Interaction, Collider, and Stable ID behavior
were not redesigned.

Validation was performed with Unity `6000.4.6f1`, URP `17.4.0`, Windows
x86_64 Development Player, Direct3D 11, 1920x1080, and an NVIDIA GeForce RTX
5070 Laptop GPU. The accepted automation evidence was generated on
2026-07-23.

## Implemented presentation

- The Quaternius Robot FBX exposes no usable named Idle, Walk, Carry,
  Interact, or Stopped clips. Its third-party source and importer metadata
  remain unchanged.
- `Phase4RobotPoseController` drives six project-owned, visual-child-only
  states: Idle, Walk, Carry Idle, Carry Walk, Interact, and Echo Stopped.
  Empty semantic Animator states keep authored state names, while the
  procedural controller performs the bounded bone pose. Root Motion is off;
  Animation Events and gameplay-root curves are absent.
- Battery visual dimensions are 0.78m long and 0.44m in diameter. The
  authoritative Battery root, Collider, Stable ID, reset position, and
  Interaction range are unchanged. Player and Echo use the same chest-front
  Carry Socket.
- Circuit routes are collider-free axis-aligned meshes at 0.012m above the
  floor with 0.06m width. They replace the old LineRenderer source in the
  Builder.
- Door presentation uses two collider-free panels that retract into the
  frame. `DoorController`, its authoritative root/Collider, open sources, and
  one-tick committed state remain unchanged.

## Automated tests

| Gate | Result |
|---|---|
| Scene Builder / deterministic rerun | Pass |
| EditMode | 116/116 Pass, 0 failed, 0 skipped, Unity exit 0 |
| PlayMode | 127/127 Pass, 0 failed, 0 skipped, Unity exit 0 |
| P0-P3 real-Scene integration | Pass |
| P3 all Sections with presentation enabled | Pass |
| Required Robot states and visual-only ownership | Pass |
| Battery size, socket, carry bounds, and floor clearance | Pass |
| Split Door references, travel, passage clearance, and Collider timing | Pass |
| Circuit height, width, axis alignment, collider absence, and rerun count | Pass |
| Missing required references / runtime exceptions | 0 matches |

The new Phase 4.3 suites contribute 17 EditMode cases and 22 PlayMode cases.
Existing tests were retained without disabling or tolerance changes.

## Gameplay regression evidence

| Measurement | Result |
|---|---|
| P0 Scene object hash | Unchanged from HEAD |
| P1 Scene object hash | Unchanged from HEAD |
| P2 Scene object hash | Unchanged from HEAD |
| P3 Standalone route | Sections 1-3 completed |
| Replay Drift maximum | 0m; requirement <= 0.05m |
| Normal-route Interaction | 4 success / 0 failure |
| Telemetry | JSON generated |
| Pause menu | Visible and paused state confirmed |
| Quit | Exit code 0 |
| Compiler warning / error | 0 / 0 |
| Missing Script / Missing Reference | 0 matching final-log entries |
| NullReference / unhandled exception | 0 matching final-log entries |

`P0_ReplayLab.unity`, `P1_InteractionLab.unity`, and
`P2_CoordinationLab.unity` are byte-identical to the branch HEAD. Presentation
components add no Collider or Stable ID.

## Performance

The final maximum-three-Echo probe sampled 600 steady frames at 1920x1080
under D3D11.

| Measurement | Result |
|---|---:|
| Frame average / p95 / maximum | 8.342 / 8.388 / 8.883ms |
| Main Thread average / maximum | 8.331 / 8.894ms |
| Camera average / maximum | 0.0126 / 0.0432ms |
| UI average / maximum | 0.0070 / 0.0310ms |
| Robot pose average / maximum | 0.0054 / 0.0389ms |
| Door visual average / maximum | 0.0181 / 0.0423ms |
| Transition frame / Main Thread maximum | 18.038 / 18.024ms |
| Steady GC maximum / total / non-zero frames | 0 B / 0 B / 0 |
| Maximum used / texture memory | 134,947,681 / 40,891,597 B |

The first accepted run recorded a 16.887ms transition maximum; the repeat
recorded 18.038ms. This is a bounded transition spike rather than sustained
frame cost, but it is recorded instead of being represented as a strict
16.6ms maximum. Batch D3D11 returned non-authoritative zero values for GPU
frame time and draw statistics, so those values are not claimed.

## Capture and 30-second presentation probe

`Captures/Phase4_3/` contains eight unique 1920x1080 D3D11 PNG files:

1. `01_player_idle.png`
2. `02_player_walk_turn.png`
3. `03_player_echo_pose.png`
4. `04_battery_carry_idle.png`
5. `05_battery_carry_walk.png`
6. `06_battery_insertion.png`
7. `07_split_door_open.png`
8. `08_two_echo_roles.png`

Automated validation confirms count, dimensions, unique content hashes,
Actor projection bounds, Debug Overlay off, and cursor off. The normal-rendered
Presentation Probe ran for 30.025 seconds and recorded nine state screenshots:
Player Idle, Player movement, Echo spawn, Echo movement, Battery pickup,
Battery carry movement, Socket insertion, Door open, and Echo stopped. It
completed the normal Section 1-3 Replay route and exited 0.

These images prove that the intended states render and are not cropped. They
do not prove that foot motion, hand placement, mechanical motion, or overall
composition looks natural to a human reviewer.

## Standalone Build

| Field | Result |
|---|---|
| Path | `Builds/Phase4_3/ECHOSHIFT_Phase4_3.exe` |
| Target | Windows x86_64 Development Build |
| BuildReport | Success |
| Build pipeline marker | `PHASE4_3_BUILD_OK` |
| Warning / error | 0 / 0 |
| Files / audited directory bytes | 291 / 204,561,206 B |
| P3 route | Sections 1-3 complete, exit 0 |
| Drift / Interaction | 0m / 4 success, 0 failure |
| Telemetry | Generated |
| Pause / Quit | Pass / exit 0 |
| Font/Glyph automated log scan | No matching error |

Japanese glyph appearance and presentation quality remain Human Visual Review
items even though the packaged font path and automated log scan are clean.

## Repository hygiene

`Captures/`, `Builds/`, `Logs/`, `TestResults/`, and `Unity/Library/` are
ignored and have zero tracked files. Test XML and runtime evidence remain local
because they are reproducible, machine-specific generated output; the durable
results are summarized in this document.

## Human Visual Review required

Automation does not mark the following as Pass:

- whether straight, diagonal, turning, Echo, Carry, reset, and transition
  motion has noticeable foot sliding;
- whether Idle, Walk, Carry, Interact, and Echo Stopped poses look natural;
- whether the Battery appears held rather than intersecting the Robot;
- whether Battery insertion reads naturally in motion;
- whether Door preparation, panel retraction, and closing look mechanical and
  unobtrusive;
- whether floor circuits read as floor wiring instead of lasers;
- whether the eight captures and 30-second presentation are representative
  Steam/portfolio material;
- whether Japanese glyphs and all presentation remain visually correct during
  ordinary human play.

`phase4-character-automation-passed` records only this automation gate.
`phase4-validated` remains forbidden until the review reports Critical 0 and
High 0 and any required correction is rebuilt and revalidated.
