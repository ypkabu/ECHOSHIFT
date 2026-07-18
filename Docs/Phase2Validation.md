# Phase 2 Validation

## Validation environment

- Date: 2026-07-19 (Asia/Tokyo)
- Unity Editor: `6000.4.6f1` (`0b051c2e5d54`)
- URP: `17.4.0`
- Input System: `1.19.0`
- Test Framework: `1.6.0`
- Platform: Windows x86_64 Development Build
- Branch: `feature/phase2-multi-echo-coordination`

## Phase 1 integration baseline

The pre-work tree was clean. `main` and `phase0-validated` initially pointed to `767116cfd9f2c6805dab271c368ee0a23ff25263`; `feature/phase1-recorded-interaction` and `phase1-validated` pointed to `b1b09e551b8519313bb2badd659c27f9111e2445`. Phase 1 documentation matched its 23/23 EditMode, 26/26 PlayMode, build, and smoke artifacts.

`git merge --ff-only feature/phase1-recorded-interaction` fast-forwarded `main` to `b1b09e551b8519313bb2badd659c27f9111e2445`. `phase0-validated` remained unchanged. Phase 2 work continued on its own feature branch; it was not merged back to `main`.

## Scene Builder and authored settings

`EchoShift.Editor.P2SceneBuilder.BuildFromCommandLine` completed with process exit code 0 and generated `P2_CoordinationLab.unity`. The final build pipeline reran the builder successfully.

- P2 settings: 60 Hz, 15 seconds, 900 ticks, max Echoes 3, drift tolerance 0.05 m
- P0/P1 shared settings remain 60 Hz, 10 seconds, 600 ticks
- Safety ceiling: 36,000 ticks (ten minutes at 60 Hz), preventing accidental very large preallocated frame and interaction buffers
- Build Settings: P2, P1, P0, each once and in that order
- P2 Battery and PowerSocket use fixed authored Stable IDs; generated-Scene validation found no empty or duplicate ID
- Repeated generation reuses the P2 settings, Echo prefab, and temporary Plate material

## Automated tests

Final EditMode command exited 0. XML result: 35 passed, 0 failed, 0 skipped, duration 0.084991 seconds.

The 12 Phase 2 EditMode additions cover P2 settings and capacity, invalid settings, Actor generation order, Echo-over-Player conflict priority, Stable-ID tie-break, `TargetBusy` no-fallback behavior, Loop history values/capacity/eviction reference safety, and Door tick delay.

Final PlayMode command exited 0. XML result: 32 passed, 0 failed, 0 skipped, duration 8.111419 seconds. This retains all P0/P1 tests and adds six grouped P2 tests covering all 19 requested behaviors. Generated P0, P1, and P2 Scenes loaded with valid required components. Runtime missing-component traversal found none, and no unhandled test exception occurred.

Integration markers:

- P0 maximum Replay Drift: 0 m
- P1 maximum Replay Drift: 0 m; interaction success 2, failure 0
- P2 maximum Replay Drift: 0 m; Echo interaction success 2, failure 0
- P2 Goal: reached during Loop 3 at tick 291

## Three-loop solution

The P2 real-Scene test advances the production tick transaction directly rather than waiting on rendered-frame time:

1. Loop 1 records Player movement to PressurePlate A and ends while holding it.
2. Loop 2 verifies Echo generation 1 holds the Plate, Gate A opens, the current Player passes Gate A, picks up the exact Battery, inserts it into PowerSocket B, and ends the loop.
3. Loop 3 verifies both Echoes advance concurrently; Echo 1 opens Gate A, Echo 2 repeats two successful interactions and opens Gate B, and current Player crosses both Gates to Goal.

Both Echoes rewind to tick zero at the Loop 2 to Loop 3 transition. Door source changes are committed at tick N and become movement/collision-visible at tick N+1. A separate four-Replay test verifies oldest-generation eviction, Plate release, Battery-holder cleanup, three-Echo cap, monotonic generations, and value-only Evicted history.

## Windows Development Build

`EchoShift.Editor.Phase2BuildPipeline.BuildWindowsDevelopment` completed with Unity process exit code 0. BuildReport result was Succeeded, total warnings 0, and reported size 166,127,414 bytes.

Generated output includes:

- `Builds/Phase2/ECHOSHIFT_Phase2.exe`
- `Builds/Phase2/ECHOSHIFT_Phase2_Data/`
- Unity player/runtime dependencies beside the executable

No C# compiler warning or error was present in the final test/build logs. PlayMode checks found no Missing Component or required reference. The build log contained no unhandled exception.

## Standalone smoke

The final executable was launched with `-batchmode -nographics` and a dedicated Player log. It ran for 30 seconds and was then forcibly stopped by the audit because the prototype has no automatic quit route.

Observed startup evidence:

- Input System initialized
- Unity engine `6000.4.6f1` initialized
- PhysX 4.1.2 selected
- startup Scene `P2_CoordinationLab`
- tick rate 60, duration 15, max ticks 900, max Echoes 3, drift tolerance 0.05
- Player reference present; initial Echo count 0 as expected before completing a loop
- matched `Exception`, `Error`, `Missing Script`, `Missing Reference`, `NullReference`, and `Assertion failed`: all 0

The forced stop is expected and is not a clean game exit-code check. A user-facing quit flow is outside Phase 2.

## Git artifact decision

Raw logs, test-result XML, Builds, Library, and other generated output remain ignored. XML/logs contain machine paths, timestamps, and run-specific durations; the commands are reproducible and the measured summary is committed here. Source, generated Unity Scenes/assets, Project Settings, and documentation are tracked.

## Remaining constraints and manual checks

- No visible Unity Editor or rendered standalone subjective review was performed. Camera framing, colors, Echo opacity, Door state readability, Battery carry/insertion readability, and overlay occlusion require human review.
- Physical keyboard and gamepad control feel were not automated.
- `-nographics` does not validate GPU rendering, display mode, monitor scaling, or graphical performance.
- Signing, installer/Steam packaging, achievements, save data, audio, VFX, final art, enemy AI, combat, and production UI remain outside scope.
- Replay drift is measured, not corrected; Unity kinematic physics is not guaranteed bitwise deterministic across all hardware/platforms.

## Gate decision

The automated Phase 2 gate is satisfied: P0/P1 regressions, P2 tests, three-loop solution, drift and interaction metrics, eviction cleanup, build, and headless startup all passed. Phase 3 may begin after the explicitly manual visual/input review is accepted; those subjective checks are not represented as automated successes.
