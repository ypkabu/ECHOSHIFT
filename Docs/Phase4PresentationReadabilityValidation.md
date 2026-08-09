# Phase 4.2 Presentation Readability Automation Validation

**Status: Automation Passed; repeat Human Visual Review pending**

Validation date: 2026-07-22 (Asia/Tokyo)  
Branch: `feature/phase4-presentation-readability`  
Baseline: `38beb04363f029948da85837aac935898d9eba5c`  
Unity Editor: `6000.4.6f1`  
URP: `17.4.0`

Phase 4.2 addresses only the Human Visual Review's camera, HUD, Actor identity,
Pause focus, and capture-composition findings. It introduces no puzzle rule,
gameplay coordinate, Stable ID, collider, Replay format, environment kit, or
enemy system. Automation passing does not authorize `phase4-validated`.

## Delivered presentation changes

- Section-bounded, smoothed look-ahead camera with limited two-Echo zoom and
  reference-resolution projection gates.
- Compact persistent Loop/time/Echo HUD, transient section/tutorial card,
  contextual Interact prompt, carry-only Battery chip, no lower black bar, and
  a more legible Pause selection state.
- Visual-child-only Player/Echo scaling, warm Player emission, cold Echo
  outlines, circular versus segmented-hex floor markers, three fin-count
  silhouettes, and replaying/stopped model marks. Gameplay roots stay scale 1.
- Eight deterministic 1920x1080 capture presets generated from real recorded
  routes and interactions. The pipeline does not instantiate fake Echoes,
  teleport Doors, or reparent Batteries to synthesize a state.

## Scene, tests, and regression

The full capture command rebuilt P3 through `P3SceneBuilder`, regenerated all
eight images, and exited `0`. Compiler warning/error, native crash, and
unhandled exception matches were `0` in the successful final run.

| Gate | Result |
|---|---:|
| EditMode | 99/99 Pass; 0 failed; 0 skipped; 25.5873039 s |
| PlayMode | 105/105 Pass; 0 failed; 0 skipped; 23.096738 s |
| P3 real-Scene automated completion | Pass; 1,064 advances |
| P3 interaction success / failure | 4 / 0 |
| Maximum Replay Drift | 0 m |
| P0/P1/P2 Scene hashes | Unchanged |
| Compiler warning / error | 0 / 0 |
| Missing Script/Reference / unhandled exception | 0 / 0 |

Preserved Scene SHA-256 values:

| Scene | SHA-256 |
|---|---|
| P0 | `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5` |
| P1 | `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB` |
| P2 | `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C` |

## Capture gate

`Captures/Phase4_2/` contains exactly eight PNG files. Every file is
1920x1080, Debug Overlay and cursor are off, the required Actor renderer bounds
are inside the configured viewport margin, and the output validation marker is
`PHASE4_2_CAPTURES_OK count=8;width=1920;height=1080`.

1. `01_section1_plate_door_overview.png`
2. `02_player_echo1_identity.png`
3. `03_echo_plate_player_door.png`
4. `04_battery_held_close.png`
5. `05_recorded_insert_door_open.png`
6. `06_echo1_echo2_roles.png`
7. `07_player_goal_arrival.png`
8. `08_compact_gameplay_hud.png`

The final visual inspection found no clipped required Actor. The identity shot
was regenerated after moving its capture tick from 20 to 40 so Player and Echo
read as separate Actors; the two-role shot was reframed closer to reduce black
background. These are automated checks, not the pending human verdict.

## Performance

Final Development Player, 1920x1080 D3D11, NVIDIA GeForce RTX 5070 Laptop GPU,
120 fps cap, maximum three Echoes, 600 steady samples:

| Metric | Result |
|---|---:|
| Average / p95 / maximum frame | 8.341 / 8.370 / 8.821 ms |
| Main Thread average / maximum | 8.337 / 8.832 ms |
| Camera update average / maximum | 0.0077 / 0.0629 ms |
| UI update average / maximum | 0.0036 / 0.0532 ms |
| Transition frame / Main Thread maximum | 11.918 / 11.915 ms |
| Steady GC maximum / total / non-zero frames | 0 B / 0 B / 0 |
| Maximum used / texture memory | 134,356,445 / 40,891,573 B |
| Final / required Echo count | 3 / 3 |

Draw Calls was unavailable. GPU Frame Time, SetPass, Triangles, and Vertices
reported non-authoritative zero values and are not interpreted as measured zero.
Interactive Profiler/Frame Debugger review remains manual.

## Windows Development Build

Output: `Builds/Phase4_2/ECHOSHIFT_Phase4_2.exe`

| Check | Result |
|---|---|
| Build process / BuildReport | exit 0 / Success |
| BuildReport warnings / errors | 0 / 0 |
| Files / bytes | 291 / 204,479,394 B |
| exe and Data folder | Present |
| Standalone P3 auto-completion | exit 0; 4 success; 0 failure; Drift 0 m |
| Telemetry JSON | Generated and exists |
| Packaged Japanese font/glyph probe | `Noto Sans JP (Packaged)` / true |
| Normal-window startup/auto-quit | exit 0 |
| Normal-window Pause-menu Quit | menu true; paused true; exit 0 |
| Missing/Null/unhandled matches | 0 |

Restart Section and Pause/Resume behavior are covered by the retained PlayMode
suite. Subjective Japanese rendering, camera feel in motion, capture quality,
and manual input remain part of the repeat Human Visual Review.

## Decision

Phase 4.2 automation gates pass. The two prior High findings have corrective
implementation and automated regression coverage, but they remain open for
human visual confirmation. Background structure, floor-art density, richer Door
motion, and broader production polish remain Medium backlog work. Do not create
`phase4-validated`; proceed only to a repeat Human Visual Review of this Build
and the eight Phase 4.2 captures.
