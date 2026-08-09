# Staged configuration

The manifests are cumulative. `Packages/manifest.json` and `packages-lock.json`
are checked in at Stage B, the smallest observed candidate. To select another
stage, use `Automation/BuildStage.ps1`; it copies the manifest and lets Unity
resolve a fresh lock before invoking the build method.

| Stage | Delta from the previous stage | Exit route used for the five-run matrix |
| --- | --- | --- |
| A | Empty Scene, one Camera, D3D11 | External `WM_CLOSE` |
| B | URP 17.4.0 and a URP asset | External `WM_CLOSE` |
| C | Input System 1.19.0 and `PlayerInput` | External `WM_CLOSE` |
| D | uGUI 2.0.0, Canvas, CanvasScaler, TMP label | External `WM_CLOSE` |
| E | Global Volume component | External `WM_CLOSE` |
| F | Audio module and AudioSource | External `WM_CLOSE` |
| G | ECHO//SHIFT window and Player settings | External `WM_CLOSE` |
| H | Guarded synchronous save, log, `Application.Quit(0)` | Automatic project-style quit |
| I | One URP-lit cube and Collider | Automatic project-style quit |

Stages A-G intentionally use the same external close route. Stages H-I exercise
the added project-style quit path. Stage B was the only initial A-I stage to
produce an access violation (`1/5`), but it did not reproduce in later Stage B
follow-up runs and no Stage B dump was captured. Treat it as an intermittent
candidate, not a stable reproduction and not evidence that URP is the direct
cause.
