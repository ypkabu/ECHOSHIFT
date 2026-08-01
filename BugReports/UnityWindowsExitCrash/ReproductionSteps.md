# Reproduction steps
Affected runs intermittently terminate with `0xC0000005`. Confirmed dumps from the product and an earlier simple minimal Player fault on a null-derived read at address `0x138` in `UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`, called from the runtime cleanup of `PlatformAccessibilityManager`.
## Environment

- Windows 11 Home, build 26200
- Unity 6000.4.12f1 (`3ca267ce8005`)
- Windows x86_64, Mono, D3D11, Development Build
- Default Stage B packages: URP 17.4.0 and its resolved dependencies

## Build Stage B once

1. Open `MinimalRepro` in Unity 6000.4.12f1, or invoke:

   ```powershell
   .\Automation\BuildStage.ps1 -Stage B -UnityEditor "C:\Path\To\6000.4.12f1\Editor\Unity.exe"
   ```

2. Confirm `Builds/B/MinimalExitCrash.exe` exists. Do not rebuild between repetitions.
3. Record a whole-build hash before running. The tested Stage B aggregate hash was `229597DCD872E3F09E46FFB35AF0814745F06B4311E1F385861B10DFFC3D8B69`.

## Exercise graphical shutdown

1. Start `Builds/B/MinimalExitCrash.exe -force-d3d11 -screen-fullscreen 0`.
2. Wait for the Player window and scene to finish loading.
3. Close the window normally.
4. Record the process exit code and repeat at least five times with the same build.

Observed in the initial set: one of five runs ended with signed exit code `-1073741819` (`0xC0000005`). This is intermittent. Twenty ProcDump-observed runs and thirty WER LocalDumps-enabled, non-debugger runs subsequently ended with code `0`; no Stage B dump was captured.

## Other stages

Stage definitions are cumulative and live under `MinimalRepro/StageDefinitions`. The automation copies the selected manifest, resolves a fresh lock, creates the scene, and produces a clean Development Build.

- A: empty Scene + Camera
- B: URP
- C: Input System
- D: uGUI/TMP
- E: Volume/Post Processing component
- F: Audio
- G: product-like Windows Player settings
- H: guarded synchronous save + `Application.Quit(0)`
- I: one minimum URP-lit visual/collider

Stages A-G were closed through the normal window close route. H-I used the project-style automatic quit route. See `Evidence/StageMatrix.md`; these results are not a monotonic package bisection.

## Expected

The graphical Windows Player exits normally with code `0` after completing native cleanup.

## Actual

Affected builds intermittently terminate with `0xC0000005`. Confirmed dumps from the product and an earlier simple minimal Player fault on a null-derived read at address `0x138` in `UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`, called from the runtime cleanup of `PlatformAccessibilityManager`.
