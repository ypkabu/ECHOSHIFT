# Phase 4 Human Visual Acceptance

**Status: Not executed — Phase 4: Automation Passed; Human Visual Review Pending**

Do not infer Pass from automated tests or captures. A human reviewer must play the current `Builds/Phase4/ECHOSHIFT_Phase4.exe`, inspect the eight `Captures/Phase4/` images, and record only observed results. Any Critical/High finding blocks `phase4-validated` and requires correction, full automation, a rebuilt Player, and a new human review.

## Test record

| Field | Result |
|---|---|
| Date/time | Not recorded |
| Reviewer | Not recorded |
| Commit/tag | `phase4-automation-passed` after creation |
| Build | `Builds/Phase4/ECHOSHIFT_Phase4.exe` |
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
| `06_section3_facility.png` | Section 3 framing and path language | Not checked |  |
| `07_goal_arrival.png` | Goal recognition/completion framing | Not checked |  |
| `08_hud_pause.png` | Japanese HUD and Pause hierarchy | Not checked |  |

## Findings

| ID | Observation | Severity | Reproduction | Proposed correction | Status |
|---|---|---|---|---|---|
| — | No human findings recorded yet | N/A |  |  | Pending review |

## Decision

- Critical: Not assessed
- High: Not assessed
- Medium: Not assessed
- Low: Not assessed
- Final human decision: Pending
- `phase4-validated` authorization: No
