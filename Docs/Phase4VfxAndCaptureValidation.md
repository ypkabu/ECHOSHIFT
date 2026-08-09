# Phase 4.3 VFX and Human Review Capture Validation

## Scope

This follow-up starts after the Phase 4.3 static Human Visual Review accepted the
Character, Battery, split Door, and floor-wiring structure. Those structures are
not changed here. The scope is limited to pooled feedback VFX readability, a
normal-speed Human Review video, and capture-only shutdown fault isolation.

## Previous video baseline

The reviewed baseline was
`Captures/Phase4_3/Phase4_3_PresentationProbe.mp4`:

- H.264, 1920×1080, 30 fps, 900 frames, 30.000 s
- internal route completed in 1.80 probe seconds
- visible Gameplay began at approximately video time 3.25 s
- the final Completed state began at approximately video time 6.75 s
- usable Gameplay was therefore approximately 3.5 s
- the `luma >= 210` frame occupancy measured at 8 samples/s between video time
  3.00 and 7.00 s peaked at 5.27% at 3.75 s; this metric includes legitimate
  white scene surfaces and UI, so it is an upper-bound frame metric rather than
  a per-particle segmentation

The baseline Player wrote `PHASE4_3_PRESENTATION_PROBE_OK` and finished managed
and physics cleanup, but the host observed process code `0xC0000005` after the
recording run.

## White-out VFX source audit

All transient white blocks came from the pooled `P4 Pulse VFX` prefab. The
prefab used billboard particles with the opaque URP/Lit `PlateAccent` material,
no alpha texture, 18-particle bursts, 2.2 m/s speed, and a renderer maximum
particle screen size of 0.5. `Phase4FeedbackPool.Emit` then replaced the authored
0.14 m start size with event values from 0.65 m through 1.80 m and retained
alpha 1.0. The result was a cluster of opaque square cards rather than a pulse.

The baseline timings below correlate the old probe markers with the first
Gameplay frame at video time approximately 3.25 s. Events sharing a frame cannot
be pixel-isolated from one another; their time is therefore recorded as the
smallest observed video interval rather than asserted as an exact GPU timestamp.

| Observed video time | VFX / source event | Old renderer and material | Old HDR / emission / size / alpha | Bloom contribution and occupancy | Intended expression |
| --- | --- | --- | --- | --- | --- |
| 3.50–3.88 s | Echo creation plus Loop Transition | pooled billboard Particle System, opaque `PlateAccent` URP/Lit | Echo color and white, material emission 1.15, 1.20 m + 0.72 m, alpha 1.0, 18 + 18 cards | two simultaneous bursts; peak frame bright-luma occupancy reached 5.27% at 3.75 s | brief colored Echo materialization and small transition spark |
| 3.75–4.00 s | Section 1 completion | same pooled system and material | Goal color, emission inherited from opaque material, 1.80 m, alpha 1.0, 18 cards | overlapped Echo/transition cards and obscured Actor/Goal | compact success pulse at the Player |
| 4.12–4.38 s | Battery pickup / Interaction success | same pooled system and material | white, 0.65 m, alpha 1.0, 18 cards | visible opaque white block at Actor; sampled bright-luma occupancy 0.38–1.51% | short hand-level interaction confirmation |
| 4.62–4.88 s | Battery insertion / Interaction success | same pooled system and material | white, 0.65 m, alpha 1.0, 18 cards | overlapped Actor and Socket; sampled occupancy up to 3.34% | short Socket insertion confirmation |
| 4.88–5.25 s | Section 2 loop transition, Echo creation, Door open | same pooled system and material | 1.20 m + 0.72 m + 1.25 m, alpha 1.0, up to 54 opaque cards | multiple pooled systems overlapped; sampled occupancy up to 4.82% | Echo arrival, small loop spark, Door status pulse |
| 5.38–6.38 s | Section 3 Echo 1 / Echo 2 creation and loop transitions | same pooled system and material | repeated 1.20 m + 0.72 m bursts, alpha 1.0 | repeated cards crossed Actors and devices; sampled occupancy up to 5.02% | generation-colored Echo arrival without hiding either role |
| 5.88–6.50 s | recorded Battery pickup / insertion and Door open | same pooled system and material | 0.65 m interaction and 1.25 m Door bursts, alpha 1.0 | simultaneous cards obscured the interaction chain; sampled occupancy 2.35–3.51% | localized interaction and Door acknowledgement |
| 6.50–6.88 s | final Goal / game completion | same pooled system and material | Goal color, 1.80 m, alpha 1.0, 18 cards | large white/cyan cards remained visible into Completed; sampled occupancy up to 4.88% | bounded final success pulse |
| not present in baseline route | Echo removal | same pooled system and material | generation color, 0.90 m, alpha 1.0, 18 cards | not measured in the reviewed video | compact de-materialization pulse |
| failure path not present | Interaction failure / Door close | same pooled system and material | danger color, 0.75 m / 0.80 m, alpha 1.0 | not measured in the reviewed video | compact red failure or close acknowledgement |

`Goal Vertical Particles` are a separate local, continuous Particle System using
0.07 m particles. They were inspected but were not the rectangular white-out
source.

## Corrective design

The pooled architecture and Gameplay events remain unchanged. The generated VFX
asset now uses:

- project-owned 64×64 radial-alpha texture
- transparent URP Particles/Unlit additive material
- non-HDR white base color; event color components remain at or below 1.0
- 8 particles per burst, 0.8 m/s speed, 0.12 m spawn radius
- event size clamped to 0.06–0.32 m
- event alpha clamped to 0.04–0.42
- four simultaneous systems maximum, with oldest-effect reuse
- alpha fade-in/fade-out and renderer maximum screen size 0.08
- Bloom retained at intensity 0.22, threshold 1.35, scatter 0.42

The final measured post-fix occupancy, video contract, shutdown matrix, and
tests are recorded below.

## Post-fix evidence

### VFX event audit

The final Human Review route started its capture timeline approximately 4.190 s
after Unity's realtime clock. The table converts the runtime VFX markers to the
video timeline. Every cue uses the same pooled `P4 Pulse VFX` Particle System,
`P4_SoftPulse` URP Particles/Unlit additive material, radial-alpha texture, and
billboard `ParticleSystemRenderer`. The material base color is non-HDR white
`(1,1,1)`, material emission is `0`, and the event color components are all
`<= 1`. The listed alpha is the per-event start-color alpha; the authored
color-over-lifetime curve fades from zero to that value and back to zero.

| Video time | VFX / firing event | Color / size / alpha / active count | Bloom and observed occupancy | Intended expression |
| --- | --- | --- | --- | --- |
| 11.172 s | `DoorOpened` / Section 1 Plate opens Door | Goal `(0.72,1,0.82)`, 0.22 m, 0.32, 1 | Bloom 0.22 / threshold 1.35; no white block | local Door-open acknowledgement |
| 11.753 s | `EchoCreated` / loop finalized | Echo 1 `(0.06,0.78,1)`, 0.22 m, 0.34, 1 | bounded cyan pulse | Echo materialization |
| 11.753 s | `LoopTransition` / same loop finalization | white, 0.10 m, 0.18, 2 | overlaps the Echo cue but remains translucent | small transition spark |
| 11.756 s | `DoorClosed` / loop reset | Danger `(1,0.12,0.16)`, 0.16 m, 0.24, 3 | concurrent limit remains below 4 | local close acknowledgement |
| 12.867 s | `DoorOpened` / replayed Plate activation | Goal, 0.22 m, 0.32, 1 | Actor and Door remain readable | replay-driven Door acknowledgement |
| 19.448 s | `SectionCompleted` / Section 1 Goal | Goal, 0.28 m, 0.38, 1 | largest cue remains bounded | compact Section success pulse |
| 19.947 s | `EchoRemoved` / Section cleanup | Echo 1, 0.16 m, 0.24, 2 | does not obscure the transition | Echo de-materialization |
| 20.990 s | `BatteryPickup` / live interaction success | white, 0.12 m, 0.20, 1 | hand-localized | pickup acknowledgement |
| 29.611 s | `BatteryInsert` / live insertion | white, 0.12 m, 0.20, 1 | Socket remains visible | insertion acknowledgement |
| 31.016 s | `EchoCreated` + `LoopTransition` / Section 2 replay creation | Echo 1 0.22/0.34 plus white 0.10/0.18, 2 | no opaque overlap | replay creation |
| 32.547 s | `BatteryPickup` / recorded interaction | white, 0.12 m, 0.20, 1 | Echo and Battery remain readable | replayed pickup |
| 36.609 s | `BatteryInsert` / recorded interaction | white, 0.12 m, 0.20, 1 | Socket remains readable | replayed insertion |
| 36.610 s | `DoorOpened` / Socket powers Door | Goal, 0.22 m, 0.32, 2 | Door panels and Actor remain readable | powered Door acknowledgement |

The Human Review route intentionally stops before Game completion, so
`GameCompleted` is not fired in this video. The existing PlayMode stress gate
fires twelve maximum-size cues in one frame and confirms that only four pooled
systems can remain active. The largest projected cue union is required to stay
below 12% of the viewport.

Post-fix video analysis sampled all 39.97 s at 8 samples/s. Pixels with
`luma >= 210` peaked at **2.00%** of the entire frame at 11.625 s. This is an
upper-bound that includes the Goal face, Plate, Japanese UI, and other authored
white surfaces. Visual inspection of that peak frame and a 12-frame contact
sheet found **0 giant white rectangles / white particle masses**. The old
review video peaked at 5.27% and visibly contained opaque square cards.

### Human Review video

`Captures/Phase4_3/Phase4_3_HumanReview.mp4`:

- H.264 High-compatible `yuv420p`, 1920x1080, 30 fps
- 1,199 frames, 39.966667 s, 13,842,633 bytes
- SHA-256
  `A184E822ECC8EBFB5E0EAA05C0B92169690535F69D662CA4CEDA1E88F0D46219`
- D3D11, `Time.timeScale = 1`, Debug Overlay off, cursor off
- measured Gameplay 39.626 s; Completed 0.000 s
- sampled first/last frame luma proves the file begins and ends on rendered
  Gameplay rather than a black frame

| Required scene | Start | Measured duration |
| --- | ---: | ---: |
| Idle | 0.000 s | 2.002 s |
| Straight Walk | 2.003 s | 3.003 s |
| Diagonal Walk | 5.007 s | 3.020 s |
| Direction Change | 8.028 s | 2.019 s |
| Echo Spawn and Move | 11.753 s | 3.013 s |
| Echo Stopped | 14.767 s | 2.000 s |
| Battery Pickup | 20.990 s | 2.003 s |
| Carry Idle | 22.994 s | 2.002 s |
| Carry Walk | 24.995 s | 4.011 s |
| Battery Insert | 29.007 s | 2.004 s |
| Door Open | 36.608 s | 3.018 s |
| Player Passes Door | 37.610 s | 2.016 s |

The route uses ordinary loop recording/replay and recorded Battery interaction.
It does not accelerate simulation or rewrite Gameplay state. Automation proves
that the states are present and viewable for the required durations. A human
must still judge foot sliding, turn quality, Carry intersections, insertion
motion, mechanical Door motion, and Japanese glyph appearance.

### Shutdown isolation

The external capture process is FFmpeg desktop duplication. It is not injected
into Unity, and the Unity Recorder package is not initialized by the Player.
The "Recorder initialized" conditions below therefore mean that FFmpeg opened
the D3D11 desktop-duplication source; "recording" means that it also encoded an
MP4.

| Condition | Player exit | FFmpeg exit / flush | Fault module | Player.log tail |
| --- | ---: | ---: | --- | --- |
| no recording / Pause Quit | `0xC0000005` | N/A | `UnityPlayer.dll` | normal physics and Input System cleanup, then `PlayerConnection::Cleanup` |
| no recording / automatic Quit | `0xC0000005` | N/A | `UnityPlayer.dll` | same |
| recording / Pause Quit | `0xC0000005` | `0`, flush after Player | `UnityPlayer.dll` | same |
| recording / automatic Quit | `0xC0000005` | `0`, flush after Player | `UnityPlayer.dll` | same |
| recorder initialized / no encoded file | `0xC0000005` | `0`, null output closed after Player | `UnityPlayer.dll` | same |
| recorder not initialized | `0xC0000005` | N/A | `UnityPlayer.dll` | same |

The Human Review handshake itself reached
`PHASE4_3_HUMAN_REVIEW_OK ... flushAcknowledged=true`, and FFmpeg completed its
file successfully. Windows Event Viewer then recorded exception
`0xc0000005` at `UnityPlayer.dll + 0x1d2f39`. Repeating with XInput instead of
the default Windows.Gaming.Input backend did not change the fault, so that
diagnostic setting was reverted.

This is **not capture-pipeline-only**: it reproduces in ordinary visible
Standalone runs without FFmpeg. It is therefore an open **High** release blocker
under the requested rule. Headless D3D11 probes and Unity batchmode exit `0`,
but that does not replace the failed normal-window shutdown gate.

### Automated regression and performance

| Gate | Result |
| --- | --- |
| Scene Builder / compile | Pass; warning/error 0/0 |
| EditMode | 120/120 Pass, failed/skipped 0/0 |
| PlayMode | 130/130 Pass, failed/skipped 0/0 |
| P0-P2 Scene bytes | unchanged from HEAD |
| P3 Sections 1-3 | completed in 1,070 advances |
| Replay Drift | 0 m |
| Interaction | 4 success / 0 failure |
| Build | `PHASE4_3_BUILD_OK`; 291 files / 204,623,446 bytes |
| Build/log scan | Compiler/Missing/Null/unhandled matches 0 |

The maximum-three-Echo headless D3D11 steady probe sampled 600 frames:

- frame average/p95/maximum: 8.342/8.405/8.639 ms
- Main Thread average/maximum: 8.337/8.637 ms
- Camera average/maximum: 0.0066/0.0205 ms
- UI average/maximum: 0.0036/0.0222 ms
- Robot pose average/maximum: 0.0034/0.0211 ms
- Door visual average/maximum: 0.0122/0.0686 ms
- transition frame/Main Thread maximum: 12.493/12.457 ms
- steady GC: 0 B/frame, total 0 B, 0 non-zero frames
- maximum used/texture memory: 134,811,677/40,925,157 bytes

Normal rendered Development probes expose 368 B/frame of profiler-side
measurement traffic on this Unity version; the independent headless steady
window is retained as the same authoritative GC gate used by the accepted
Phase 4.3 automation. Unsupported zero GPU/draw values are not claimed.
