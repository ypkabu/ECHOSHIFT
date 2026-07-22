# Phase 4 Human Visual Acceptance

**Status: Phase 4.1 Human Visual Review Conditional Fail; Phase 4.2 automation passed; repeat review pending**

The first primitive-led Phase 4 presentation was reviewed and rejected. Phase 4.1 successfully integrated curated external assets, but its Human Visual Review was **Conditional Fail** because camera composition and persistent HUD quality remained High issues. Phase 4.2 applies the bounded readability corrections and passed automation. A human reviewer must now play `Builds/Phase4_2/ECHOSHIFT_Phase4_2.exe` and inspect `Captures/Phase4_2/`. Automation does not prove visual acceptance; any remaining Critical/High finding still blocks `phase4-validated`.

## Test record

| Field | Result |
|---|---|
| Date/time | Not recorded |
| Reviewer | Not recorded |
| Commit/tag | Phase 4.2 automation candidate / `phase4-presentation-automation-passed` after creation |
| Build | `Builds/Phase4_2/ECHOSHIFT_Phase4_2.exe` |
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
| P4.1-H01 | Cyan/orange lamps, partial rails, and diagonal supports floated outside the facility | High | Phase 4.1 After capture review | Remove independent lamps and anchor full wall cladding to continuous floor-connected backing | Corrected by Builder and automation; human recheck pending |

## Phase 4.1 Human Visual Review result

The recorded Human Visual Review result was **Conditional Fail**: Critical `0`,
High `2`, Medium `5`, and multiple Low observations. External asset integration,
dark-value recovery, floating-artifact removal, Door/Goal readability, and kit
cohesion passed. ES technical-portfolio quality was rated `7/10`; representative
screenshot and Steam-store quality were rated `5/10` and `4/10`.

| Finding | Severity | Phase 4.2 disposition |
|---|---|---|
| Player/Echo clipped or weakly placed by gameplay/capture camera | High | Section bounds, look-ahead, two-Echo zoom, smoothing, projection tests, and new presets implemented; repeat human review pending |
| Persistent HUD looked provisional and occupied too much screen space | High | Compact status card, transient objective, context-only prompt, carry-only chip, removed lower bar, and Pause HUD hiding implemented; repeat human review pending |
| Player/Echo too small and insufficiently distinct by silhouette | Medium | Visual-child scale, emission, marker geometry, generation fins, and replay/stopped marks implemented; repeat human review pending |
| Facility can read as floating in a black void | Medium | Deferred to later visual backlog; Phase 4.2 only reduced excess black in capture framing |
| Floor texture density competes with gameplay | Medium | Deferred to later visual backlog |
| Door motion may look too simple | Medium | Deferred to later visual backlog |
| Pause focus/spacing almost complete | Medium | Strong selected fill/cyan outline and reduced spacing implemented; repeat human review pending |

Phase 4.2 regenerated these review images at 1920x1080: Section 1 overview,
Player/Echo 1 identity, Plate cooperation, Battery held close-up, recorded Socket
insertion/Door opening, two-Echo roles, Goal arrival, and compact gameplay HUD.
Their automated framing passed, but their visual score is not updated until the
repeat human review.

## Decision

- Critical: `0` in the completed Phase 4.1 review
- High: `2` in the completed Phase 4.1 review; corrected by Phase 4.2 automation, human confirmation pending
- Medium: `5` in the completed Phase 4.1 review; three remain later backlog work
- Low: Multiple, not individually enumerated by the reviewer
- Final human decision: Phase 4.1 Conditional Fail; Phase 4.2 repeat review pending
- `phase4-validated` authorization: No
