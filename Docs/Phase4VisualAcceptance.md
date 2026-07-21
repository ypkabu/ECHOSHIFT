# Phase 4 Human Visual Acceptance

**Status: First review failed; Phase 4.1 corrective automation passed; new human review pending**

The first primitive-led Phase 4 presentation was reviewed and rejected. Phase 4.1 uses curated external assets and addresses the recorded High issues, but automation does not prove visual acceptance. A human reviewer must play `Builds/Phase4_1/ECHOSHIFT_Phase4_1.exe`, inspect `Captures/Phase4_1/Before/` and `Captures/Phase4_1/After/`, and record only observed results. Any Critical/High finding blocks `phase4-validated` and requires correction, full automation, rebuilt Players, and another human review.

## Test record

| Field | Result |
|---|---|
| Date/time | Not recorded |
| Reviewer | Not recorded |
| Commit/tag | `phase4-assets-automation-passed` after creation |
| Build | `Builds/Phase4_1/ECHOSHIFT_Phase4_1.exe` |
| Device / OS / GPU | Not recorded |
| Resolution / display mode | Not recorded |
| Input device | Not recorded |
| Audio output device | Not recorded |
| Total play time | Not measured |

## Required checklist

Use `Pass`, `Fail`, or `Not checked`. Severity is `Critical`, `High`, `Medium`, `Low`, or `N/A`.

| # | Check | Result | Comment / evidence | Severity |
|---:|---|---|---|---|
| 1 | Greybox feel is substantially reduced | Not checked |  | N/A |
| 2 | Player and Echo are distinguishable immediately | Not checked |  | N/A |
| 3 | Echo 1, Echo 2, and Echo 3 remain distinguishable without color alone | Not checked |  | N/A |
| 4 | PressurePlate and its Door relationship is understandable | Not checked |  | N/A |
| 5 | PowerSocket and its Door relationship is understandable | Not checked |  | N/A |
| 6 | Battery floor/held/inserted state is understandable | Not checked |  | N/A |
| 7 | Door closed/open/moving state and passage width are understandable | Not checked |  | N/A |
| 8 | Goal reads as an exit/completion location | Not checked |  | N/A |
| 9 | Environment shape, materials, and screen composition feel coherent | Not checked |  | N/A |
| 10 | Lighting does not hide Player, Echo, devices, routes, or Door openings | Not checked |  | N/A |
| 11 | Bloom/exposure/black level are restrained and readable | Not checked |  | N/A |
| 12 | HUD no longer feels like placeholder UI | Not checked |  | N/A |
| 13 | Japanese text and glyphs are readable and not clipped | Not checked |  | N/A |
| 14 | Camera framing and foreground structures do not obstruct play | Not checked |  | N/A |
| 15 | VFX do not hide gameplay and correctly distinguish success/failure | Not checked |  | N/A |
| 16 | Movement, device, Loop, completion, and UI Audio are useful and not loud/repetitive | Not checked |  | N/A |
| 17 | 1920x1080 presentation looks good in motion | Not checked |  | N/A |
| 18 | Each screenshot explains a useful Phase 0-3 feature without Debug Overlay | Not checked |  | N/A |
| 19 | Screen quality is usable in a 30-second portfolio video | Not checked |  | N/A |
| 20 | Pause/Resume/Restart/visible Quit paths still work; Quit exits normally | Not checked | Recheck host-specific automated window shutdown anomaly. | N/A |
| 21 | 1280x720 layout remains usable | Not checked |  | N/A |
| 22 | 2560x1440 and one non-16:9 layout remain usable | Not checked |  | N/A |

## Capture review

| File | Intended evidence | Result | Comment / Severity |
|---|---|---|---|
| `01_section1_facility.png` | Modular Section 1, route, Plate, Door, Goal | Not checked |  |
| `02_player_echo1.png` | Player/Echo 1 silhouette and identity | Not checked |  |
| `03_battery_carry.png` | Visible carry state | Not checked |  |
| `04_socket_door_open.png` | Battery insertion, Socket, Door open | Not checked |  |
| `05_echo1_echo2_roles.png` | Two-Echo cooperation/readability | Not checked |  |
| `06_goal_arrival.png` | Goal recognition/completion framing | Not checked |  |
| `07_gameplay_hud.png` | Japanese gameplay HUD and route visibility | Not checked |  |
| `08_pause_menu.png` | Pause hierarchy, dimmer, focus, and Quit separator | Not checked |  |

## Findings

| ID | Observation | Severity | Reproduction | Proposed correction | Status |
|---|---|---|---|---|---|
| P4-H01 | Initial pass crushed dark values and hid readable form | High | First human Visual Review | Rebalanced lighting/exposure/materials; compare Before/After in motion | Corrected by automation; human recheck pending |
| P4-H02 | Black overhead beams read as obstruction/artifact | High | First human Visual Review | Removed camera-crossing overhead rails/beams | Corrected by automation; human recheck pending |
| P4-H03 | Persistent world labels made the scene look provisional | High | First human Visual Review | Removed persistent object labels; retain contextual Japanese UI only | Corrected by automation; human recheck pending |
| P4-H04 | Pause Menu hierarchy/focus was not release-readable | High | First human Visual Review | Full-screen dimmer, centered card, focus arrow, separated Quit, hidden gameplay HUD | Corrected by automation; human recheck pending |
| P4-M01 | Door and Goal needed stronger authored form | Medium | First human Visual Review | External frames/panels, circuit badges, portal/glow/`出口ゲート` | Corrected by automation; human recheck pending |

## Decision

- Critical: New review not assessed
- High: Four prior findings corrected by automation; acceptance not yet confirmed
- Medium: Not assessed
- Low: Not assessed
- Final human decision: Pending Phase 4.1 review
- `phase4-validated` authorization: No
