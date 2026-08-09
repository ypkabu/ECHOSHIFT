# Windows Event Viewer evidence

The retained product Application Error record identifies:

- Exception code: `0xc0000005`.
- Faulting module: `UnityPlayer.dll`.
- Unity 6000.4.6f1 fault offset: `0x1d2f39`.
- Affected process: `ECHOSHIFT_Phase4_3_CrashDebug.exe`.

WinDbg resolves that offset to `ExternalGPUProfiler::GetGameViewWindowHandle+0x9`; see `SymbolicatedStack.md`.

For staged Stage B, run 4 returned signed code `-1073741819`, equivalent to `0xC0000005`, but an Event Viewer record and dump were not retained as Stage B-specific evidence. Its module, offset, and stack are therefore marked **unconfirmed** rather than copied from the product event.
