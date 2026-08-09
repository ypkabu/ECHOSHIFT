# Phase 4 Windows Player Exit Crash Investigation

**Status: Engine-side native shutdown defect strongly supported; release validation blocked**

## Conclusion

> 画面付きWindows PlayerのUnity Native Shutdown Pathで発生する再現性の高いエンジン側不具合。ゲームコードが発火条件となる可能性は完全には否定しないが、Access Violation本体はUnityPlayer.dllのNative Cleanup内で発生する。

This conclusion applies to the confirmed product and earlier-minimal dumps. It does not assign the same stack to staged Stage B, because no Stage B dump was obtained.

## Confirmed facts

- Graphical Windows Players reproduce `0xC0000005` in Unity 6000.4.6f1, 6000.4.8f1, and 6000.4.12f1.
- Development and Non-Development builds reproduce.
- D3D11 GPU device 0, GPU device 1, and WARP reproduce.
- Pause Quit, automatic `Application.Quit`, and Window Close reproduce in product matrices.
- `-batchmode -nographics` ended normally in `5/5` runs.
- The product's three 6000.4.6f1 dumps and earlier minimal 6000.4.8f1/6000.4.12f1 dumps normalize to the same Unity frame sequence.
- The exception is a read from `0x138` with `RAX = 0` in `UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`.
- The caller is `RuntimeStatic<PlatformAccessibilityManager,0>::StaticDestroy`, under `RegisterRuntimeInitializeAndCleanup::ExecuteCleanup` and `RuntimeCleanup`.
- Gameplay completed before the product fault: replay drift `0 m`, interactions success `4`, failure `0`, telemetry saved, and no managed compiler/Missing/Null/Unhandled match in final logs.
- Visual and gameplay implementation remained frozen during this investigation.

## Staged minimal project

The source-only project is in `BugReports/UnityWindowsExitCrash/MinimalRepro`. Stages A-I were built with 6000.4.12f1, warning `0`, error `0`, and run five times each.

| Stage | Exit route | AV / runs |
| --- | --- | ---: |
| A empty Scene + Camera | Window Close | 0/5 |
| B + URP | Window Close | 1/5 |
| C + Input System | Window Close | 0/5 |
| D + uGUI/TMP | Window Close | 0/5 |
| E + Volume | Window Close | 0/5 |
| F + Audio | Window Close | 0/5 |
| G + product-like Player settings | Window Close | 0/5 |
| H + project-style quit | Application.Quit | 0/5 |
| I + minimum visual/component | Application.Quit | 0/5 |

The Stage B interpretation is fixed:

> Stage BはA～Iで唯一Access Violationを観測した最小構成候補。ただし1/5の低頻度であり、URPを直接原因とは断定できない。後続Stageで単調に再現率が上がらないため、タイミングまたはBuild layout依存の可能性がある。

Follow-up on the same Stage B binary produced `0/20` under ProcDump and `0/30` with user-level WER LocalDumps. No Stage B dump exists. Its fault function, stack hash, offset, shutdown subsystem, and equality to the product defect are **unconfirmed**. A larger repetition matrix was not started because the specified prerequisite—same-stack confirmation—was not satisfied.

## Confirmed symbolicated stack

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

Only nine frames are present; a thirty-frame stack does not exist. The first non-Unity module is the generated Player EXE CRT entry. There is no user/native plugin/driver frame between `UnityMain` and the fault.

Normalized Unity-frame SHA-256: `2D128D9A9C6B4956572BA97A01E6C2E5B03F8DBE6EEB93632AB6DC3B03E34C98`.

| Unity | UnityPlayer offset | Stack status |
| --- | --- | --- |
| 6000.4.6f1 | `0x1d2f39` | product, 3/3 same |
| 6000.4.8f1 | `0x1d3e49` | earlier minimal, same normalized stack |
| 6000.4.12f1 | `0x1d4a89` | earlier minimal, same normalized stack |
| Stage B 6000.4.12f1 | Not captured | not comparable |

## Static shutdown audit

No `Application.quitting` handler or `OnApplicationQuit` implementation was found. The product quit coordinator guards duplicate requests, pauses simulation, writes telemetry synchronously, logs the saved path, and then calls `Application.Quit(0)`. Event subscriptions inspected in the Phase 4 presentation flow have matching unsubscription. Normal gameplay paths do not create custom threads, Tasks, NativeArray/Jobs, P/Invoke, native plugins, external Recorder/FFmpeg processes, or background disposal work. Audio/particle pools are Unity-owned scene objects and did not appear on the native fault stack.

This audit reduces the likelihood of an obvious managed double-dispose or after-quit callback. It cannot prove that game configuration never changes Unity's internal destruction order.

## Version, graphics, and backend result

- Unity 6000.4.6f1 / 6000.4.8f1 / 6000.4.12f1: affected; no first fixed patch found.
- NVIDIA D3D11: affected (`4/5` in the dedicated device matrix).
- Intel D3D11: affected (`5/5`).
- WARP D3D11: affected (`5/5`).
- Headless `-batchmode -nographics`: not affected (`0/5`).
- Mono: affected.
- IL2CPP: untested; module not installed.
- Other PC: untested; no second PC available.

## Root-cause boundary

The Access Violation body is confirmed inside UnityPlayer native cleanup. A game-code/configuration trigger cannot be excluded because reproduction rates change with build layout, timing, and monitoring. No evidence supports a direct URP root-cause claim. No project code fix or safe graphical workaround was identified.

`-batchmode -nographics` avoids the graphical path but is not a usable workaround for a released game. Process kill, `Environment.Exit`, wrapper exit-code rewriting, sleeps, and hidden crash suppression were rejected.

## Phase status

- Visual Human Review: Passed.
- Character / Carry / Door / Wiring: Passed.
- Critical visual issues: 0.
- High visual issues: 0.
- Release Validation: **Blocked**.
- Blocker: Unity Windows native graphical shutdown crash.
- `phase4-validated`: must not be created.

No tag was created. If a non-release evidence tag is desired later, a candidate name is `phase4-visual-accepted-shutdown-blocked`; this is a proposal only.
