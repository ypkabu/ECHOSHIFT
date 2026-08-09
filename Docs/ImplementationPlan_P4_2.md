# Phase 4.2 Presentation Readability Pass Implementation Plan

## Baseline and gate

- Source branch: `feature/phase4-external-asset-integration`
- Verified source commit: `38beb04363f029948da85837aac935898d9eba5c`
- Work branch: `feature/phase4-presentation-readability`
- Human Visual Review result: **Conditional Fail** (`Critical 0`, `High 2`, `Medium 5`)
- High findings in scope: gameplay-camera composition and placeholder-like persistent HUD.
- Supporting scope: Player/Echo at-a-glance identity and eight purpose-built captures.
- `phase4-validated` remains prohibited. Automation may create only
  `phase4-presentation-automation-passed` after every gate below succeeds.

## Scope boundaries

Phase 4.2 changes presentation only. It does not add or change puzzle rules,
fixed-tick simulation, replay data, Stable IDs, interaction arbitration,
colliders, environment kits, enemies, or gameplay coordinates.

Deferred Medium findings stay in the Phase 4+ backlog: additional background
structure, quieter floor art, richer Door animation, and broader art-production
work. Pause receives only the requested focus/readability adjustment.

## 1. Gameplay camera

1. Extend the camera settings asset with bounded presentation values: forward
   look-ahead, focus and zoom smoothing, base/two-Echo field of view, and three
   section-local focus bounds.
2. Let the scene-owned camera receive the active section, follow the Player's
   facing direction, and apply a limited group adjustment when two or more
   Echoes exist.
3. Clamp the focus point inside the active section and smooth both focus and
   field of view. Camera code must only read gameplay transforms.
4. Add projection tests at 1280x720, 1920x1080, 2560x1440, 16:10, and 21:9,
   including Player body/marker margins, two-Echo visibility, and bounds.

## 2. HUD and Pause

1. Replace the large left/right/bottom backing arrangement with a compact
   top-left Loop/time/Echo card, a transient section/tutorial card, a compact
   contextual prompt, and a Battery chip shown only while carrying.
2. Keep the initial movement guidance transient; hide section/objective after
   the configured reveal interval and hide the prompt when no target/action is
   available after onboarding.
3. Remove the bottom black bar, orphan carry icon, empty Battery row, and
   persistent R/Pause helper rows from normal gameplay.
4. Preserve Pause behavior while reducing unused card space and adding an
   explicit selection fill plus cyan border/arrow. Gameplay HUD remains hidden
   during Pause; Quit retains its destructive color and confirmation.
5. Validate safe anchors at all requested aspect ratios and keep Debug Overlay
   independent.

## 3. Player and Echo identity

1. Scale only visual children (Player 12%, Echo 8%); gameplay roots and
   colliders remain unchanged.
2. Strengthen Player warm-white head/chest emission and use a smaller circular
   floor marker.
3. Use a segmented hex marker and cold emissive silhouette elements for Echoes.
4. Retain three non-text generation fins with distinct counts/silhouettes and
   add clear playing/stopped model marks. Do not restore world-space generation
   text.

## 4. Capture presets

1. Add a project capture-preset asset containing eight independently framed
   camera compositions at 1920x1080.
2. Generate `Captures/Phase4_2/` with Debug Overlay/cursor hidden.
3. Stage each shot through the existing deterministic route input and recorded
   Loop/Interaction transitions. Presentation refresh calls are allowed; direct
   Door teleport, Battery reparenting, and fake Echo instantiation are removed.
4. Validate file count, dimensions, minimum content size, actor viewport
   margins, composition diversity, and UI-safe framing.

## 5. Regression and performance gates

- Rebuild P3 through `P3SceneBuilder` and confirm P0-P2 hashes, P3 Stable IDs,
  colliders, and missing-component checks are unchanged.
- Run all EditMode and PlayMode tests without deleting, disabling, or weakening
  retained coverage.
- Require P3 completion, maximum drift <= 0.05 m, interaction success 4,
  interaction failure 0, and no compiler warning/error or unhandled exception.
- Re-run the three-Echo performance probe and record Main Thread, frame time,
  GC allocation, camera/UI update cost, and steady `0 B/frame` status. Unsupported
  counters remain explicitly unavailable.

## 6. Build and completion

- Build Windows x86_64 to
  `Builds/Phase4_2/ECHOSHIFT_Phase4_2.exe`.
- Verify BuildReport success/warnings/errors, required Data folder, packaged
  Japanese glyph probe, telemetry output, automated P3 completion, Pause/Quit,
  and normal-window exit behavior. Host-specific shutdown results are reported
  exactly rather than inferred.
- Review diffs and ignored-output status, then commit in meaningful boundaries.
- Finish with `feat: complete phase 4 presentation readability pass` and create
  lightweight tag `phase4-presentation-automation-passed` only if every
  automation gate passes. Do not merge, push, rewrite history, or create
  `phase4-validated`.

## Execution status

Completed on 2026-07-22. The final gates are EditMode `99/99`, PlayMode
`105/105`, P3 Standalone completion with interaction `4/0` and Drift `0 m`,
eight validated 1920x1080 captures, BuildReport warning/error `0/0`, normal
startup and Pause-menu Quit exit `0`, and steady GC `0 B/frame` with three
Echoes. Phase 4.2 is **Automation Passed; repeat Human Visual Review pending**.
`phase4-validated` remains prohibited.
