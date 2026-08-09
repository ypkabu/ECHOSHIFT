# Phase 4 Human Visual Acceptance

**Status: Visual Human Review Passed; Release Validation Blocked by Unity native shutdown crash**

The first primitive-led Phase 4 presentation was reviewed and rejected. Phase
4.1 integrated curated external assets. Phase 4.2 corrected Camera, HUD, Pause,
Actor identity, and capture framing; its Human Visual Review passed those goals
but found one remaining High character-presentation issue plus three Medium
device/presentation issues. Phase 4.3 implements the bounded Robot, Battery,
floor-circuit, and Door corrections and passes automation. The final Phase 4
Visual Human Review has accepted the bounded Phase 4.3 presentation. This
visual acceptance does not satisfy release validation because graphical
Windows Players still fail intermittently inside Unity native shutdown cleanup.

## Final Visual Human Review record

| Item | Final result |
|---|---|
| Visual Human Review | Passed |
| Character presentation | Passed |
| Battery size / Carry presentation | Passed |
| Split Door presentation | Passed |
| Floor wiring presentation | Passed |
| Player / Echo identity and two-Echo roles | Passed |
| Critical visual issues | 0 |
| High visual issues | 0 |
| Release Validation | **Blocked** |
| Release blocker | Unity Windows native graphical shutdown crash |

The detailed checklists below preserve the pre-final review history and fields
that were not numerically recorded. They do not override this final visual
decision.

## Test record

| Field | Result |
|---|---|
| Date/time | Not recorded |
| Reviewer | Not recorded |
| Commit/tag | Phase 4.3 automation completion / `phase4-character-automation-passed` |
| Build | `Builds/Phase4_3/ECHOSHIFT_Phase4_3.exe` |
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
| P4.2-H01 | Robot arms read as a T-pose; Battery did not appear hand-held | High | Phase 4.2 Human Visual Review | Six visual-child poses, chest carry socket, smaller Battery, and state captures | Corrected by Phase 4.3 automation; human recheck pending |
| P4.2-M01 | Circuit lines read as airborne lasers | Medium | Phase 4.2 Human Visual Review | Floor-height axis-aligned thin meshes | Corrected by Phase 4.3 automation; human recheck pending |
| P4.2-M02 | Open Door read as a large board | Medium | Phase 4.2 Human Visual Review | Split panels retract inside fixed frame | Corrected by Phase 4.3 automation; human recheck pending |
| P4.2-M03 | Battery was oversized for the Robot | Medium | Phase 4.2 Human Visual Review | 0.78m by 0.44m visual and chest-front carry | Corrected by Phase 4.3 automation; human recheck pending |

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

## Phase 4.2 Human Visual Review result

The bounded Camera, HUD, Pause Menu, Player/Echo identity, two-Echo roles, and
capture composition goals passed. The overall Phase 4 decision remained
unvalidated: Critical `0`, High `1`, Medium `3`, Low `2`.

- High: the Robot read as a T-pose and the Battery carry presentation appeared
  unfinished.
- Medium: circuit lines appeared airborne, open Door panels appeared as a
  large board, and the Battery was oversized.
- Low: the Goal face was overexposed and the floor remained visually dense.

Phase 4.3 automation verifies the replacement structures and states, but does
not update these visual severities. Human review must decide whether the High
and Medium observations are actually resolved in motion.

## Phase 4.3 capture review

Use `Pass`, `Fail`, or `Not checked`; all entries intentionally remain
`Not checked` until a human reviews them.

| File | Intended evidence | Result | Comment / Severity |
|---|---|---|---|
| `01_player_idle.png` | Natural non-T Idle | Not checked |  |
| `02_player_walk_turn.png` | Walk/turn pose and foot sliding impression | Not checked |  |
| `03_player_echo_pose.png` | Player/Echo pose differentiation | Not checked |  |
| `04_battery_carry_idle.png` | Battery scale and static hand placement | Not checked |  |
| `05_battery_carry_walk.png` | Carry gait and intersections | Not checked |  |
| `06_battery_insertion.png` | Socket alignment and insertion readability | Not checked |  |
| `07_split_door_open.png` | Retracting panels and clear passage | Not checked |  |
| `08_two_echo_roles.png` | Two-Echo role and pose readability | Not checked |  |

## Decision

- Final Phase 4 Visual Human Review: Passed
- Character / Carry / Door / Wiring: Passed
- Critical / High visual findings: `0 / 0`
- Visible Standalone normal exit: release blocker (`UnityPlayer.dll`,
  `0xC0000005`)
- Release Validation: Blocked by Unity Windows native shutdown crash
- Final Phase 4 validation: Not complete
- `phase4-validated` authorization: No

## Phase 4.3 static Human Visual Review result

The reviewer accepted the following static evidence:

- T-pose eliminated;
- Idle and Walk pose difference;
- Player/Echo identity;
- smaller Battery and chest-front Carry Pose;
- Battery insertion presentation;
- floor circuit presentation;
- split Door presentation;
- two-Echo role readability.

No structural change to Character, Battery, Door, or floor wiring is authorized
after this acceptance.

## Phase 4.3 motion-review replacement

`Captures/Phase4_3/Phase4_3_HumanReview.mp4` replaces the previous fast probe.
It provides 39.626 s of measured Gameplay at normal time scale, twelve required
scenes, and no Completed screen. Automated image analysis found no giant white
particle mass; maximum whole-frame bright-luma occupancy is 2.00%.

The final reviewer accepted the visual target after the static and motion
evidence cycle. No further Character, Battery, Door, wiring, Camera, HUD,
Animation, or Gameplay change is authorized by this document.

An independent release blocker remains: graphical Windows Standalone exits can
end with `0xC0000005` in `UnityPlayer.dll` native cleanup. This is not counted as
a visual Critical/High issue, but Phase 4 cannot be marked Validated while the
release shutdown gate is blocked.
