# Windows Player crashes with access violation in UnityPlayer.dll during graphical shutdown

## Summary

A Windows Standalone Player intermittently crashes during graphical shutdown with exception `0xC0000005`. Symbolicated full dumps from the product and an earlier simple minimal project fault on a null-derived read in `UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`, called while Unity destroys `PlatformAccessibilityManager` in `RuntimeCleanup`.

The issue reproduces on Unity 6000.4.6f1, 6000.4.8f1, and 6000.4.12f1. It is not limited to Development Builds, a hardware GPU, one GPU vendor, or one quit route. `-batchmode -nographics` does not reproduce.

## Reproduction rate

Product and earlier-minimal results:

- 6000.4.6f1 product Development automatic quit: 3/3 crashes.
- 6000.4.8f1 earlier simple minimal Player: 9/10 crashes.
- 6000.4.12f1 earlier simple minimal Player: 9/10 crashes.
- 6000.4.12f1 product Development: automatic quit 7/10, Pause Quit 8/10, Window Close 2/10.
- 6000.4.12f1 product Non-Development automatic quit: 9/10. Two runs reached the external timeout before normal completion, but still faulted during cleanup.
- 6000.4.12f1 D3D11 device matrix: NVIDIA 4/5, Intel 5/5, WARP 5/5.
- 6000.4.12f1 `-batchmode -nographics`: 0/5.

Submitted staged-project result:

- Stage A, empty Scene + Camera: 0/5 Window Close.
- Stage B, adding URP 17.4.0 and pipeline assets: 1/5 Window Close.
- Stages C-I: 0/5 each.
- Same Stage B binary under ProcDump: 0/20, no dump.
- Same Stage B binary with user-level WER LocalDumps: 0/30, no dump.

Stage B reproduces intermittently and is the smallest staged configuration in which an access violation was observed. It is not a stable minimal reproduction. Because later cumulative stages did not increase monotonically, URP is not identified as the direct cause. Timing, monitoring, serialization, or build layout may affect the rate.

## Tested Unity versions

| Version | Changeset | Result |
| --- | --- | --- |
| 6000.4.6f1 | `0b051c2e5d54` | Reproduces; product full dumps |
| 6000.4.8f1 | `f8b72d3d7343` | Reproduces in earlier minimal Player |
| 6000.4.12f1 | `3ca267ce8005` | Reproduces in product and earlier minimal Player; staged project included |

No first regressed or fixed Unity version has been established.

## Exception and symbolicated stack

- Exception code: `0xC0000005`.
- Access type: read.
- Invalid address: `0x0000000000000138`.
- Faulting register: `RAX = 0`.
- Faulting function: `UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`.
- Shutdown subsystem: native runtime static cleanup for `PlatformAccessibilityManager`.

```text
UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9
UnityPlayer!RuntimeStatic<PlatformAccessibilityManager,0>::StaticDestroy+0x3b
UnityPlayer!RegisterRuntimeInitializeAndCleanup::ExecuteCleanup+0xed
UnityPlayer!RuntimeCleanup+0x27
UnityPlayer!UnityMainImpl+0x1300
UnityPlayer!UnityMain+0xb
<player-exe>!__scrt_common_main_seh+0x106
kernel32!BaseThreadInitThunk+0x17
ntdll!RtlUserThreadStart+0x2c
```

The three analyzed Unity versions have different binary offsets (`0x1d2f39`, `0x1d3e49`, `0x1d4a89`) but the same normalized Unity function sequence. The first non-Unity module is the generated Player EXE CRT entry below `UnityMain`. No user DLL, native plugin, GPU driver, or managed callback is on the fault stack.

Normalized stack SHA-256: `2D128D9A9C6B4956572BA97A01E6C2E5B03F8DBE6EEB93632AB6DC3B03E34C98`.

Important limitation: no dump was captured from the staged Stage B failure. Stage B's one AV has not been proven to have this stack and must not be treated as the same defect solely from its exit code.

## Minimal reproduction steps

1. Open `BugReports/UnityWindowsExitCrash/MinimalRepro` in Unity 6000.4.12f1.
2. The checked-in `Packages/manifest.json` and `packages-lock.json` select Stage B (URP 17.4.0).
3. Run `Automation/BuildStage.ps1 -Stage B -UnityEditor <Unity.exe path>` once.
4. Record the build hash; do not rebuild between runs.
5. Launch `Builds/B/MinimalExitCrash.exe -force-d3d11 -screen-fullscreen 0`.
6. Wait for the window to finish loading, then close the window normally.
7. Repeat. The submitted Stage B build reproduced once in its initial five-run set; later monitored/unmonitored follow-ups did not reproduce.

The included A-I manifests allow the same cumulative comparison. Stage B should be described as “reproduces intermittently,” not as a deterministic reproducer.

## Expected result

The graphical Windows Player completes shutdown and returns exit code `0`.

## Actual result

Affected runs terminate with `0xC0000005` during `UnityPlayer.dll` native cleanup. In confirmed product runs, gameplay and synchronous telemetry complete first, with no managed unhandled exception, Missing Script/Reference, or NullReference in the Player log.

## Development, GPU, and route independence

- Development and Non-Development builds reproduce.
- NVIDIA, Intel, and D3D11 WARP reproduce.
- Pause Menu Quit, automated `Application.Quit(0)`, and normal Window Close reproduce in product builds.
- Graphical Players reproduce; `-batchmode -nographics` does not.
- The issue occurs without Recorder initialization and without recording.

## Regression status

Unknown. The same symbolicated function sequence occurs in the tested 6000.4.6f1, 6000.4.8f1, and 6000.4.12f1 evidence. The investigation did not test a pre-6000.4 editor.

## Workaround status

No safe graphical workaround is known. `-batchmode -nographics` avoids the affected path but cannot be used for a normal graphical game release. Upgrading within the tested 6000.4 patches does not resolve it. Force-killing the process, replacing `Application.Quit`, sleeping during shutdown, or rewriting the exit code were not used and are not acceptable workarounds.

## Attachments

- Staged source-only minimal project with A-I manifests and Stage B lock.
- Reproduction steps and complete staged result matrix.
- Unity version/build/GPU matrices.
- Full dump SHA-256 list; dump binaries supplied separately if requested.
- Symbolicated stack and normalized stack hash.
- Curated Player log ordering and system information.
- Package manifest/lock and Project Settings comparison.
