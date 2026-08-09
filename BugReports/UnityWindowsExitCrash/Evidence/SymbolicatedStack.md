# Symbolicated stack evidence

## Confirmed dump facts

- Exception: `0xC0000005` access violation.
- Operation: read (`Parameter[0] = 0`).
- Invalid address: `0x0000000000000138` (`Parameter[1]`).
- Register state: `RAX = 0`; faulting instruction reads `[RAX+0x138]`.
- Fault function: `UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`.
- Thread: Unity main thread.
- Shutdown subsystem: runtime static cleanup of `PlatformAccessibilityManager`, inside `RegisterRuntimeInitializeAndCleanup::ExecuteCleanup` / `RuntimeCleanup`.
- No user DLL, native plugin, GPU driver DLL, or managed callback appears on the fault stack.

## Available stack

The native stack terminates after eight frames, so there are not thirty frames to report:

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

The first non-Unity module is the generated Player EXE CRT entry (`__scrt_common_main_seh`), below `UnityMain`; no third-party module intervenes.

Normalized six-Unity-frame SHA-256, excluding version-specific offsets:

`2D128D9A9C6B4956572BA97A01E6C2E5B03F8DBE6EEB93632AB6DC3B03E34C98`

## Version offset comparison

| Unity | Source | UnityPlayer offset | Normalized stack |
| --- | --- | --- | --- |
| 6000.4.6f1 | Product, three dumps | `+0x1d2f39` | Same hash |
| 6000.4.8f1 | Earlier minimal Player | `+0x1d3e49` | Same hash |
| 6000.4.12f1 | Earlier minimal Player | `+0x1d4a89` | Same hash |
| 6000.4.12f1 | Staged Stage B | **Unconfirmed** | **No dump** |

The three tested Unity versions resolve to the same function sequence even though their binary offsets differ. This supports a recurring Unity native cleanup defect for the confirmed dumps. It does not prove that Stage B's single exit-code AV was the same defect.
